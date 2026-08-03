namespace ToolRunner.Models;

/// <summary>
///     the phase the repeating control cycle is currently in. reported to the UI so the operator can
///     tell whether the tool is turning or waiting. the direction switch itself takes no measurable
///     time — it happens immediately after the stop command — so it has no step of its own.
///     제어 사이클의 현재 단계
/// </summary>
public enum SequenceStep {
    /// <summary>
    ///     no sequence is running — the motor is stopped.
    ///     시퀀스 정지 상태
    /// </summary>
    Idle,

    /// <summary>
    ///     the direction is set and the loop is waiting out the configured delay before the next run
    ///     command. covers both the wait after Start and the wait after each direction switch.
    ///     Run 명령 전 대기 (시작 직후 및 방향 전환 후)
    /// </summary>
    WaitBeforeRun,

    /// <summary>
    ///     the run command has been issued and the motor is turning.
    ///     Run 명령 후 구동 중
    /// </summary>
    Running
}
