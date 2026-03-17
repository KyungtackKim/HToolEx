namespace HTool.Type;

/// <summary>
///     로그 항목을 담는 레코드 구조체.
///     Record struct containing a single log entry.
/// </summary>
/// <param name="Timestamp">로그 발생 시각 / log timestamp</param>
/// <param name="Category">로그 카테고리 / log category</param>
/// <param name="Level">로그 수준 / log severity level</param>
/// <param name="Message">로그 메시지 / log message</param>
public readonly record struct LogEntry(
    DateTime      Timestamp,
    LogCategories Category,
    LogLevel      Level,
    string        Message
);