using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 그래프 채널 유형
///     graph channel type for Pro X
/// </summary>
public enum GraphChannel {
	/// <summary>
	///     없음
	///     none
	/// </summary>
	[Description("None")]
    None,

	/// <summary>
	///     토크
	///     torque
	/// </summary>
	[Description("Torque")]
    Torque,

	/// <summary>
	///     속도
	///     speed
	/// </summary>
	[Description("Speed")]
    Speed,

	/// <summary>
	///     각도
	///     angle
	/// </summary>
	[Description("Angle")]
    Angle,

	/// <summary>
	///     토크 / 각도
	///     torque / angle
	/// </summary>
	[Description("Torque / Angle")]
    TorqueAngle
}