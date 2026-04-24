using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     토크 방향 열거형
///     torque direction enumeration
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Format.Ez.DeviceSettings"/>.
///     used by: <see cref="HTool.Format.Ez.DeviceSettings"/>.
/// </remarks>
public enum Direction : byte {
	/// <summary>
	///     시계 방향
	///     clockwise
	/// </summary>
	[Description("CW")]
    CW = 0x00,

	/// <summary>
	///     반시계 방향
	///     counter-clockwise
	/// </summary>
	[Description("CCW")]
    CCW = 0x01,

	/// <summary>
	///     양방향
	///     both directions
	/// </summary>
	[Description("Both")]
    Both = 0x02
}