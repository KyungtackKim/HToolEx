using HTool.Device;

namespace HTool.Type;

/// <summary>
///     통신 오류 코드를 정의한다.
///     Defines communication error codes.
/// </summary>
/// <remarks>
///     사용처: <see cref="ComError"/>, <see cref="MessagePipeline"/>.
///     used by: <see cref="ComError"/>, <see cref="MessagePipeline"/>.
/// </remarks>
public enum ComErrorCode {
	/// <summary>
	///     CRC 검증 실패 (RTU 전용).
	///     CRC validation failed (RTU only).
	/// </summary>
	InvalidCrc,

	/// <summary>
	///     프레임 구조 오류 (알 수 없는 함수 코드 등).
	///     Frame structure error (unknown function code, etc.).
	/// </summary>
	InvalidFrame,

	/// <summary>
	///     값 범위 초과.
	///     Value out of valid range.
	/// </summary>
	InvalidValueRange,

	/// <summary>
	///     응답 타임아웃.
	///     Response timeout.
	/// </summary>
	Timeout
}