using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 출력 신호 유형
///     output signal type for Pro X
/// </summary>
public enum OutputSignal {
	/// <summary>
	///     임펄스 출력
	///     impulse output
	/// </summary>
	[Description("Impulse")]
    Impulse,

	/// <summary>
	///     HIGH 상태 유지 출력
	///     status high hold output
	/// </summary>
	[Description("Status high")]
    StatusHigh
}