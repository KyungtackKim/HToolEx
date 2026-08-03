namespace ToolRunner.Models;

/// <summary>
///     the operator-tunable timings of one control cycle. one wait covers every gap before a run
///     command — the one after Start and the one after each direction switch; the short fixed pause
///     that lets the spindle stop before the switch itself is a hardware property and lives in the
///     runner. the run duration is configured per direction, since fastening and loosening rarely
///     want the same run time. every value is in milliseconds; zero means "do not wait at all".
///     제어 사이클 타이밍 — Run 전 대기(공용) + 방향별 Run 시간
/// </summary>
/// <param name="WaitBeforeRunMs">
///     wait inserted before every run command: after Start once the initial direction is written,
///     and after each direction switch
/// </param>
/// <param name="CwRunDurationMs">how long the motor runs in each clockwise (fastening) cycle</param>
/// <param name="CcwRunDurationMs">how long the motor runs in each counter-clockwise (loosening) cycle</param>
/// <param name="InitialDirection">direction written once before the first run command</param>
public readonly record struct SequenceSettings(
    int          WaitBeforeRunMs,
    int          CwRunDurationMs,
    int          CcwRunDurationMs,
    RunDirection InitialDirection) {
    /// <summary>
    ///     default timings used when the app starts — 3 s wait before every run (the combined
    ///     stop-to-run gap of the earlier before/after pair), 3 s fastening run, 1 s loosening run,
    ///     first cycle fastening (CW).
    ///     앱 기동 시 기본 타이밍
    /// </summary>
    public static SequenceSettings Default => new(3000, 3000, 1000, RunDirection.Cw);

    /// <summary>
    ///     returns the run duration configured for the given direction, so the cycle loop does not
    ///     have to branch on the direction itself.
    ///     지정 방향의 Run 유지 시간 반환
    /// </summary>
    /// <param name="direction">direction the current cycle runs in</param>
    /// <returns>run duration in milliseconds</returns>
    public int RunDurationFor(RunDirection direction) {
        // clockwise uses the CW duration, counter-clockwise the CCW one
        return direction is RunDirection.Cw ? CwRunDurationMs : CcwRunDurationMs;
    }
}
