using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     목표값 활성화 열거형
///     target enable enumeration
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Format.Ez.DeviceSettings"/>.
///     used by: <see cref="HTool.Format.Ez.DeviceSettings"/>.
/// </remarks>
public enum TargetEnable : byte {
	/// <summary>
	///     목표값 비활성화
	///     target disable
	/// </summary>
	[Description("Disable")]
    Disable = 0x00,

	/// <summary>
	///     목표값 활성화
	///     target enable
	/// </summary>
	[Description("Enable")]
    Enable = 0x01
}