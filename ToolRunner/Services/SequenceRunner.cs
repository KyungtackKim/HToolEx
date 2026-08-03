using ToolRunner.Models;

namespace ToolRunner.Services;

/// <summary>
///     drives the repeating external control cycle against the connected tool:
///     start delay → (run → hold → stop → wait → reverse direction → wait) → repeat until stopped.
///     the loop runs on the thread pool; the motor is always left stopped when it ends, whether it
///     ended through a user stop, a communication failure, or an unexpected exception.
///     반복 제어 시퀀스 실행기 (종료 시 정지 명령 보장)
/// </summary>
/// <param name="htool">tool wrapper the control writes are issued through</param>
public sealed class SequenceRunner(HToolService htool) : IDisposable {
    // fixed pause between the stop command and the direction switch that follows it — the controller
    // ignores a direction change while the spindle is still winding down, so the switch has to wait
    // for the motor to settle. deliberately not operator-tunable: it is a property of the hardware,
    // not of the test cycle
    // 정지 후 방향 전환 전 고정 대기 (스핀다운 대기)
    private const int StopSettleMs = 250;

    // cancellation source of the active loop — null while idle
    private CancellationTokenSource? _cts;

    // direction currently written to the controller
    private RunDirection _direction;

    // completed cycle count of the active loop
    private int _cycle;

    // the loop task — kept so StopAsync can await an orderly shutdown
    private Task? _loop;

    /// <summary>
    ///     whether a sequence loop is currently active. a loop that has run to completion or been
    ///     stopped reports false.
    ///     시퀀스 실행 중 여부
    /// </summary>
    public bool IsRunning => _loop is { IsCompleted: false };

    /// <inheritdoc />
    public void Dispose() {
        // cancel a loop still in flight so the process can exit
        _cts?.Cancel();
        // release the cancellation source
        _cts?.Dispose();
        // clear the reference
        _cts = null;
    }

    /// <summary>
    ///     raised when the loop enters a new step, carrying the step, the current cycle number and
    ///     the direction in effect. always dispatch to the UI thread in the handler.
    ///     단계 전이 이벤트 (단계, 사이클 번호, 방향)
    /// </summary>
    public event Action<SequenceStep, int, RunDirection>? StateChanged;

    /// <summary>
    ///     raised for every issued command and every failure, as (category, message).
    ///     always dispatch to the UI thread in the handler.
    ///     명령/실패 로그 이벤트 (분류, 메시지)
    /// </summary>
    public event Action<string, string>? Logged;

    /// <summary>
    ///     starts the cycle loop with the given timings. does nothing when a loop is already
    ///     running, so a double click on Start cannot spawn a second loop.
    ///     시퀀스 시작 (중복 호출 무시)
    /// </summary>
    /// <param name="settings">delays and starting direction for this run</param>
    public void Start(SequenceSettings settings) {
        // ignore the request while a loop is already active
        if (IsRunning)
            // already running — nothing to do
            return;

        // dispose the source left behind by a previous run
        _cts?.Dispose();
        // create a fresh cancellation source for this run
        _cts = new CancellationTokenSource();
        // run the loop on the thread pool so the UI thread stays responsive
        _loop = Task.Run(() => RunLoopAsync(settings, _cts.Token), CancellationToken.None);
    }

    /// <summary>
    ///     requests a stop and waits until the loop has issued its final stop command, so the
    ///     caller can rely on the motor being stopped once this task completes.
    ///     시퀀스 정지 및 최종 정지 명령 전송 대기
    /// </summary>
    /// <returns>task completing when the loop has fully unwound</returns>
    public async Task StopAsync() {
        // capture the loop and its source — Start replaces both
        var cts = _cts;
        // capture the loop task
        var loop = _loop;

        // nothing to unwind when no loop was ever started
        if (cts is null || loop is null)
            // already idle — nothing to do
            return;

        // guard: cancelling and awaiting the loop must not throw at the call site
        try {
            // cancel the delay currently in flight
            await cts.CancelAsync();
            // wait for the loop to run its stop-and-cleanup path
            await loop;
        } catch (Exception ex) {
            // the loop faulted on the way out — the motor stop is in its finally, so just record it
            Logged?.Invoke("error", $"sequence stop failed: {ex.Message}");
        } finally {
            // release the cancellation source
            cts.Dispose();
            // clear the source so the next Start begins fresh
            _cts = null;
            // drop the finished loop task
            _loop = null;
            // report the idle state to the UI
            StateChanged?.Invoke(SequenceStep.Idle, _cycle, _direction);
        }
    }

    /// <summary>
    ///     issues a one-off run command outside the sequence, for manual verification.
    ///     수동 Run 명령
    /// </summary>
    public void ManualRun() {
        // write the run value to the run/stop register
        Write(ControlRegisters.RunStop, ControlRegisters.RunValue, "manual run");
    }

    /// <summary>
    ///     issues a one-off stop command outside the sequence, for manual verification.
    ///     수동 Stop 명령
    /// </summary>
    public void ManualStop() {
        // write the stop value to the run/stop register
        Write(ControlRegisters.RunStop, ControlRegisters.StopValue, "manual stop");
    }

    /// <summary>
    ///     switches the rotation direction outside the sequence, for manual verification. the new
    ///     direction also becomes the one the status display reports.
    ///     수동 방향 전환
    /// </summary>
    /// <param name="direction">direction to write to the controller</param>
    public void ManualDirection(RunDirection direction) {
        // adopt the requested direction as the current one
        _direction = direction;
        // write it to the direction register
        Write(ControlRegisters.Direction, (ushort)direction, $"manual direction {direction}");
        // reflect the change in the status display
        StateChanged?.Invoke(IsRunning ? SequenceStep.Running : SequenceStep.Idle, _cycle, _direction);
    }

    /// <summary>
    ///     the cycle loop itself. cancellation is the normal exit path and is swallowed; the
    ///     finally block guarantees a stop command regardless of how the loop ended.
    /// </summary>
    /// <param name="settings">delays and starting direction for this run</param>
    /// <param name="ct">token cancelled by <see cref="StopAsync" /></param>
    private async Task RunLoopAsync(SequenceSettings settings, CancellationToken ct) {
        // reset the cycle counter for this run
        _cycle = 0;
        // adopt the configured starting direction
        _direction = settings.InitialDirection;

        // guard: cancellation and write failures must not surface as unobserved task exceptions
        try {
            // write the starting direction so the first cycle begins from a known state
            Write(ControlRegisters.Direction, (ushort)_direction, $"initial direction {_direction}");
            // report the wait that precedes the first run command
            Report(SequenceStep.WaitBeforeRun);
            // hold before the first run command
            await DelayAsync(settings.WaitBeforeRunMs, ct);

            // repeat the run/stop/reverse cycle until the operator stops the sequence
            while (!ct.IsCancellationRequested) {
                // count this cycle
                _cycle++;
                // pick the run duration configured for the direction this cycle turns in
                var runDuration = settings.RunDurationFor(_direction);
                // issue the run command
                Write(ControlRegisters.RunStop, ControlRegisters.RunValue,
                    $"cycle {_cycle} run ({_direction}, {runDuration} ms)");
                // report the running step
                Report(SequenceStep.Running);
                // leave the motor running for the duration configured for this direction
                await DelayAsync(runDuration, ct);

                // issue the stop command
                Write(ControlRegisters.RunStop, ControlRegisters.StopValue, $"cycle {_cycle} stop");
                // report the wait that precedes the next run command
                Report(SequenceStep.WaitBeforeRun);

                // let the spindle come to rest before touching the direction register — a switch
                // issued while the motor is still winding down is not picked up by the controller
                await DelayAsync(StopSettleMs, ct);

                // flip the direction for the next cycle
                _direction = _direction is RunDirection.Cw ? RunDirection.Ccw : RunDirection.Cw;
                // write the reversed direction now that the motor is at rest
                Write(ControlRegisters.Direction, (ushort)_direction, $"direction switch to {_direction}");
                // read the register back so the log shows whether the switch actually stuck
                VerifyDirection();
                // hold the configured wait before the next run command
                await DelayAsync(settings.WaitBeforeRunMs, ct);
            }
        } catch (OperationCanceledException) {
            // operator-requested stop — the normal exit path, no action needed
        } catch (Exception ex) {
            // unexpected failure inside the loop (disposed tool, write path fault, ...)
            Logged?.Invoke("error", $"sequence aborted: {ex.Message}");
        } finally {
            // always leave the motor stopped, however the loop ended
            StopMotor();
        }
    }

    /// <summary>
    ///     writes a control register and logs the outcome. a rejected enqueue is reported as a
    ///     warning rather than thrown, so one dropped write cannot kill the loop.
    /// </summary>
    /// <param name="address">holding-register address</param>
    /// <param name="value">value to write</param>
    /// <param name="what">short description used in the log line</param>
    private void Write(ushort address, ushort value, string what) {
        // enqueue the FC 0x06 write on the HTool pipeline
        var queued = htool.Tool.WriteSingleReg(address, value);

        // a false return means the pipeline refused the request (not connected, queue full)
        if (!queued) {
            // surface the rejected write
            Logged?.Invoke("warn", $"{what} — write rejected (reg {address} = {value})");
            // nothing more to do for a rejected write
            return;
        }

        // log the accepted command
        Logged?.Invoke("modbus", $"{what} — reg {address} = {value}");
    }

    /// <summary>
    ///     reads the direction register back after a switch. the value lands in the log through the
    ///     normal response path, which is what tells the operator whether the controller kept the
    ///     written direction or reverted it.
    /// </summary>
    private void VerifyDirection() {
        // enqueue an FC 0x03 read of the single direction register
        var queued = htool.Tool.ReadHoldingReg(ControlRegisters.Direction, 1);

        // a false return means the pipeline refused the read — the switch stays unverified
        if (!queued)
            // surface the skipped verification
            Logged?.Invoke("warn", $"direction read-back skipped — read rejected (reg {ControlRegisters.Direction})");
    }

    /// <summary>
    ///     writes the final stop command on the way out of the loop. this deliberately ignores the
    ///     cancellation token — the stop has to be attempted even when the operator cancelled.
    /// </summary>
    private void StopMotor() {
        // guard: the tool may already be disconnected or disposed by the time the loop unwinds
        try {
            // force the run register low so the motor cannot keep turning
            var queued = htool.Tool.WriteSingleReg(ControlRegisters.RunStop, ControlRegisters.StopValue);
            // report the final stop as a normal write when accepted, as a warning when refused
            Logged?.Invoke(queued ? "sequence" : "warn",
                queued
                    ? $"sequence ended — stop written (reg {ControlRegisters.RunStop} = {ControlRegisters.StopValue})"
                    : "sequence ended — final stop write rejected (tool disconnected?)");
        } catch (Exception ex) {
            // the final stop could not be issued at all — record it, there is nothing left to try
            Logged?.Invoke("error", $"final stop write failed: {ex.Message}");
        }
    }

    /// <summary>
    ///     waits the given number of milliseconds, cancelling promptly on stop. a zero or negative
    ///     delay only observes cancellation so the loop can still be interrupted.
    /// </summary>
    /// <param name="milliseconds">delay length; zero or negative skips the wait</param>
    /// <param name="ct">token cancelled by <see cref="StopAsync" /></param>
    private static async Task DelayAsync(int milliseconds, CancellationToken ct) {
        // a non-positive delay means "continue immediately"
        if (milliseconds <= 0) {
            // still honour a stop requested during the previous step
            ct.ThrowIfCancellationRequested();
            // no wait requested — continue
            return;
        }

        // wait the requested span, throwing OperationCanceledException on stop
        await Task.Delay(milliseconds, ct);
    }

    /// <summary>
    ///     reports the current step, cycle number and direction to the UI.
    /// </summary>
    /// <param name="step">step the loop has just entered</param>
    private void Report(SequenceStep step) {
        // notify subscribers of the new step
        StateChanged?.Invoke(step, _cycle, _direction);
    }
}
