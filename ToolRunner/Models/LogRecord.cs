namespace ToolRunner.Models;

/// <summary>
///     one line in the activity log — a timestamped, categorized message shown in the log list.
///     the timestamp is local wall clock because the log is only ever read on screen.
///     활동 로그 한 줄 (로컬 시각)
/// </summary>
/// <param name="Time">local time the entry was produced</param>
/// <param name="Category">short source tag (sequence, modbus, connection, warn, error)</param>
/// <param name="Message">human-readable message body</param>
public readonly record struct LogRecord(DateTime Time, string Category, string Message) {
    /// <summary>
    ///     time column text — local wall clock with millisecond precision, so the operator can
    ///     verify the configured delays against the actual command spacing.
    ///     시각 표시 문자열 (ms 정밀도)
    /// </summary>
    public string TimeText => Time.ToString("HH:mm:ss.fff");
}
