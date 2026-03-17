using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     장치 명령 열거형
///     device command enumeration
/// </summary>
public enum DeviceCommand : byte {
	/// <summary>
	///     캘리브레이션 데이터 요청
	///     request calibration data
	/// </summary>
	[Description("Request the calibration data")]
    ReqCalData = 0x00,

	/// <summary>
	///     캘리브레이션 설정 포인트 요청
	///     request calibration set point
	/// </summary>
	[Description("Request the calibration set point")]
    ReqCalSetPoint = 0x01,

	/// <summary>
	///     캘리브레이션 저장 요청
	///     request calibration save
	/// </summary>
	[Description("Request the calibration save")]
    ReqCalSave = 0x02,

	/// <summary>
	///     캘리브레이션 종료 요청
	///     request calibration terminate
	/// </summary>
	[Description("Request the calibration terminate")]
    ReqCalTerminate = 0x03,

	/// <summary>
	///     설정 데이터 요청
	///     request setting data
	/// </summary>
	[Description("Request the setting data")]
    ReqSetData = 0x04,

	/// <summary>
	///     현재 토크값 요청 (단위 없음)
	///     request current torque without unit
	/// </summary>
	[Description("Request the current torque without the unit")]
    ReqTorque = 0x05,

	/// <summary>
	///     캘리브레이션 데이터 응답
	///     response calibration data
	/// </summary>
	[Description("Response the calibration data")]
    ResCalData = 0x80,

	/// <summary>
	///     캘리브레이션 설정 포인트 응답
	///     response calibration set point
	/// </summary>
	[Description("Response the calibration set point")]
    ResCalSetPoint = 0x81,

	/// <summary>
	///     캘리브레이션 저장 응답
	///     response calibration save
	/// </summary>
	[Description("Response the calibration save")]
    ResCalSave = 0x82,

	/// <summary>
	///     설정 데이터 응답
	///     response setting data
	/// </summary>
	[Description("Response the setting data")]
    ResSetData = 0x84,

	/// <summary>
	///     현재 토크값 응답 (단위 없음)
	///     response current torque without unit
	/// </summary>
	[Description("Response the current torque without the unit")]
    ResTorque = 0x85,

	/// <summary>
	///     ADC 데이터 보고
	///     report ADC data
	/// </summary>
	[Description("Report the ADC data")]
    RepAdc = 0xA0
}