namespace HTool.Type;

/// <summary>
///     연결 상태를 정의한다.
///     Defines the connection state.
/// </summary>
public enum Connection {
	/// <summary>
	///     연결 해제 요청됨 (Close() 호출 직후).
	///     Disconnection requested (immediately after Close() call).
	/// </summary>
	Close,

	/// <summary>
	///     연결 해제 완료.
	///     Disconnection completed.
	/// </summary>
	Closed,

	/// <summary>
	///     연결 시도 중.
	///     Connection attempt in progress.
	/// </summary>
	Connecting,

	/// <summary>
	///     연결 완료 (장치 정보 수신 후).
	///     Connected (after device info received).
	/// </summary>
	Connected
}