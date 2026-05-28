using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 입력 신호 유형
///     input signal type for Pro X
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Pro.Job.InputBody</c>.
///     used by: <c>HTool.Format.Pro.Job.InputBody</c>.
/// </remarks>
public enum InputSignal {
	/// <summary>
	///     HIGH 활성 트리거
	///     active high trigger
	/// </summary>
	[Description("Active high")]
    ActiveHigh,

	/// <summary>
	///     LOW 활성 트리거
	///     active low trigger
	/// </summary>
	[Description("Active low")]
    ActiveLow,

	/// <summary>
	///     HIGH 상태 유지
	///     status high hold
	/// </summary>
	[Description("Status high")]
    StatusHigh,

	/// <summary>
	///     LOW 상태 유지
	///     status low hold
	/// </summary>
	[Description("Status low")]
    StatusLow
}