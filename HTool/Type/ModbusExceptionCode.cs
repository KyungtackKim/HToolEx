using HTool.Device.Protocol;

namespace HTool.Type;

/// <summary>
///     MODBUS 프로토콜 예외 코드를 정의한다.
///     Defines MODBUS protocol exception codes.
/// </summary>
/// <remarks>
///     사용처: <see cref="ModbusResponse"/>.
///     used by: <see cref="ModbusResponse"/>.
/// </remarks>
public enum ModbusExceptionCode : byte {
	/// <summary>
	///     잘못된 함수 코드.
	///     Illegal function code.
	/// </summary>
	IllegalFunction = 0x01,

	/// <summary>
	///     잘못된 데이터 주소.
	///     Illegal data address.
	/// </summary>
	IllegalDataAddress = 0x02,

	/// <summary>
	///     잘못된 데이터 값.
	///     Illegal data value.
	/// </summary>
	IllegalDataValue = 0x03,

	/// <summary>
	///     슬레이브 장치 장애.
	///     Slave device failure.
	/// </summary>
	SlaveDeviceFailure = 0x04,

	/// <summary>
	///     처리 중 (장시간 작업).
	///     Acknowledge (long-running operation).
	/// </summary>
	Acknowledge = 0x05,

	/// <summary>
	///     슬레이브 장치 사용 중.
	///     Slave device busy.
	/// </summary>
	SlaveDeviceBusy = 0x06,

	/// <summary>
	///     게이트웨이 경로 사용 불가.
	///     Gateway path unavailable.
	/// </summary>
	GatewayPathUnavailable = 0x0A,

	/// <summary>
	///     게이트웨이 대상 장치 응답 없음.
	///     Gateway target device failed to respond.
	/// </summary>
	GatewayTargetDeviceFailed = 0x0B
}