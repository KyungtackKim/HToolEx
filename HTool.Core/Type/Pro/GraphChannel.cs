using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 그래프 채널 유형
///     graph channel type for Pro X
/// </summary>
/// <remarks>
///     Pro X 게이트웨이의 그래프 프로토콜 확장을 위한 정의이며, 본 라이브러리 내부에서 현재 참조되지 않습니다.
///     definition reserved for Pro X gateway graph protocol extensions; currently unreferenced within this library.
/// </remarks>
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