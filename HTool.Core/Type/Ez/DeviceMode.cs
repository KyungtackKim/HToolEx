using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     장치 모드 열거형
///     device mode enumeration
/// </summary>
public enum DeviceMode : byte {
	/// <summary>
	///     운전 모드
	///     operation mode
	/// </summary>
	[Description("Operation")]
    Operation = 0x00,

	/// <summary>
	///     캘리브레이션 모드
	///     calibration mode
	/// </summary>
	[Description("Calibration")]
    Calibration = 0x01
}