using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     장치 모드 열거형
///     device mode enumeration
/// </summary>
/// <remarks>
///     EZTorQ 토크미터 동작 모드 정의. 본 라이브러리는 정의만 제공하며, 실제 상태 관리는 HToolEz 패키지에서 수행합니다.
///     EZTorQ torque meter operation mode definition. This library provides only the definition; actual state management
///     is handled by the HToolEz package.
/// </remarks>
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