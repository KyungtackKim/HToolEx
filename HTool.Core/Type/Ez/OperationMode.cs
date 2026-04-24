using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     운전 모드 열거형
///     operation mode enumeration
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Format.Ez.DeviceSettings"/>.
///     used by: <see cref="HTool.Format.Ez.DeviceSettings"/>.
/// </remarks>
public enum OperationMode : byte {
	/// <summary>
	///     피크 모드
	///     peak mode
	/// </summary>
	[Description("Peak")]
    Peak = 0x00,

	/// <summary>
	///     퍼스트 피크 모드
	///     first-peak mode
	/// </summary>
	[Description("First-Peak")]
    FirstPeak = 0x01,

	/// <summary>
	///     추적 모드
	///     track mode
	/// </summary>
	[Description("Track")]
    Track = 0x02
}