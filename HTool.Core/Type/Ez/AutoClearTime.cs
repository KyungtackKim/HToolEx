using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     자동 클리어 시간 열거형
///     auto clear time enumeration
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Ez.DeviceSettings</c>.
///     used by: <c>HTool.Format.Ez.DeviceSettings</c>.
/// </remarks>
public enum AutoClearTime : byte {
	/// <summary>
	///     비활성화
	///     disable
	/// </summary>
	[Description("Disable")]
    Disable = 0x00,

	/// <summary>
	///     0.5초
	///     0.5 seconds
	/// </summary>
	[Description("0.5 sec")]
    Sec05 = 0x01,

	/// <summary>
	///     1초
	///     1 second
	/// </summary>
	[Description("1 sec")]
    Sec1 = 0x02,

	/// <summary>
	///     2초
	///     2 seconds
	/// </summary>
	[Description("2 sec")]
    Sec2 = 0x03,

	/// <summary>
	///     3초
	///     3 seconds
	/// </summary>
	[Description("3 sec")]
    Sec3 = 0x04,

	/// <summary>
	///     4초
	///     4 seconds
	/// </summary>
	[Description("4 sec")]
    Sec4 = 0x05,

	/// <summary>
	///     5초
	///     5 seconds
	/// </summary>
	[Description("5 sec")]
    Sec5 = 0x06
}