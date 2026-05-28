using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     캘리브레이션 포인트 모드 열거형
///     calibration point mode enumeration
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Ez.CalibrationData</c>, <c>HTool.Format.Ez.CalibrationSettings</c>.
///     used by: <c>HTool.Format.Ez.CalibrationData</c>, <c>HTool.Format.Ez.CalibrationSettings</c>.
/// </remarks>
public enum CalPointMode : byte {
	/// <summary>
	///     3점 캘리브레이션
	///     3-point calibration
	/// </summary>
	[Description("3-Point")]
    ThreePoint = 0x00,

	/// <summary>
	///     5점 캘리브레이션
	///     5-point calibration
	/// </summary>
	[Description("5-Point")]
    FivePoint = 0x01
}