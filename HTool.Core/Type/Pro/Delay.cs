using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 지연 스텝의 모드
///     delay step mode for Pro X job
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Pro.Job.DelayBody</c>.
///     used by: <c>HTool.Format.Pro.Job.DelayBody</c>.
/// </remarks>
public enum Delay {
	/// <summary>
	///     시간 지연
	///     time-based delay
	/// </summary>
	[Description("Time")]
    Time,

	/// <summary>
	///     팝업 대기 (Rev.0 전용)
	///     pop-up wait (Rev.0 only)
	/// </summary>
	[Description("Pop-up")]
    [Obsolete("Support Rev.0 only")]
    PopUp,

	/// <summary>
	///     바코드 대기 (Rev.0 전용)
	///     barcode wait (Rev.0 only)
	/// </summary>
	[Description("Barcode")]
    [Obsolete("Support Rev.0 only")]
    Barcode
}