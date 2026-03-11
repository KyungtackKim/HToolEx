namespace HTool.Type;

/// <summary>
///     통신 유형을 정의한다.
///     Defines the communication type.
/// </summary>
public enum ComType {
	/// <summary>
	///     MODBUS RTU 시리얼 통신.
	///     MODBUS RTU serial communication.
	/// </summary>
	Rtu,

	/// <summary>
	///     MODBUS TCP 이더넷 통신.
	///     MODBUS TCP Ethernet communication.
	/// </summary>
	Tcp,

	/// <summary>
	///     PRO X 게이트웨이 경유 통신.
	///     Communication via PRO X gateway.
	/// </summary>
	Pro
}