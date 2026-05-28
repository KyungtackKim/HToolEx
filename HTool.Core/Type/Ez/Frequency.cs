using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     주파수 열거형
///     frequency enumeration
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Ez.DeviceSettings</c>.
///     used by: <c>HTool.Format.Ez.DeviceSettings</c>.
/// </remarks>
public enum Frequency : byte {
	/// <summary>
	///     100 Hz
	///     100 Hz
	/// </summary>
	[Description("100 Hz")]
    Hz100 = 0x00,

	/// <summary>
	///     500 Hz
	///     500 Hz
	/// </summary>
	[Description("500 Hz")]
    Hz500 = 0x01,

	/// <summary>
	///     1,500 Hz
	///     1,500 Hz
	/// </summary>
	[Description("1,500 Hz")]
    Hz1500 = 0x02,

	/// <summary>
	///     2,000 Hz
	///     2,000 Hz
	/// </summary>
	[Description("2,000 Hz")]
    Hz2000 = 0x03,

	/// <summary>
	///     3,000 Hz
	///     3,000 Hz
	/// </summary>
	[Description("3,000 Hz")]
    Hz3000 = 0x04
}