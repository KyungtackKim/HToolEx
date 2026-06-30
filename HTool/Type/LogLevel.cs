using HTool.Device;

namespace HTool.Type;

/// <summary>
///     로그 수준을 정의한다.
///     Defines log severity levels.
/// </summary>
/// <remarks>
///     사용처: <see cref="HToolLogger" />, <see cref="MessagePipeline" />.
///     used by: <see cref="HToolLogger" />, <see cref="MessagePipeline" />.
/// </remarks>
public enum LogLevel {
	/// <summary>
	///     디버깅 정보.
	///     Debugging information.
	/// </summary>
	Debug,

	/// <summary>
	///     일반 정보.
	///     General information.
	/// </summary>
	Info,

	/// <summary>
	///     경고 (동작에는 영향 없음).
	///     Warning (does not affect operation).
	/// </summary>
	Warning,

	/// <summary>
	///     오류 (동작에 영향 있음).
	///     Error (affects operation).
	/// </summary>
	Error
}