namespace ToolSample.Models;

/// <summary>
///     로그 레코드 모델. DataGrid 표시용.
///     Log record model. For DataGrid display.
/// </summary>
public sealed class LogRecord {
	/// <summary>
	///     타임스탬프 (HH:mm:ss.fff).
	///     Timestamp (HH:mm:ss.fff).
	/// </summary>
	public string Timestamp { get; init; } = "";

	/// <summary>
	///     카테고리명.
	///     Category name.
	/// </summary>
	public string Category { get; init; } = "";

	/// <summary>
	///     로그 레벨.
	///     Log level.
	/// </summary>
	public string Level { get; init; } = "";

	/// <summary>
	///     로그 메시지.
	///     Log message.
	/// </summary>
	public string Message { get; init; } = "";
}