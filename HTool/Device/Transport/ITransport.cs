namespace HTool.Device.Transport;

/// <summary>
///     통신 전송 계층 인터페이스.
///     Transport layer interface for communication.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="DataReceived" /> 이벤트의 버퍼는 <see cref="System.Buffers.ArrayPool{T}" />에서 임차되며,
///         핸들러 반환 즉시 반납된다. 핸들러는 반드시 동기적으로 데이터를 소비해야 한다.
///     </para>
///     <para>
///         The buffer passed via <see cref="DataReceived" /> is rented from <see cref="System.Buffers.ArrayPool{T}" />
///         and returned immediately after the handler returns. The handler MUST consume data synchronously.
///     </para>
/// </remarks>
public interface ITransport : IDisposable {
	/// <summary>
	///     현재 연결 상태를 반환한다.
	///     Gets whether the transport is currently connected.
	/// </summary>
	bool IsConnected { get; }

	/// <summary>
	///     원시 데이터 수신 시 발생한다. 버퍼는 동기 소비 전용.
	///     Raised when raw data is received. Buffer is for synchronous consumption only.
	/// </summary>
	event Action<ReadOnlyMemory<byte>>? DataReceived;

	/// <summary>
	///     연결 상태 변경 시 발생한다. true = 연결됨, false = 해제됨.
	///     Raised when connection state changes. true = connected, false = disconnected.
	/// </summary>
	event Action<bool>? ConnectionChanged;

	/// <summary>
	///     대상에 비동기로 연결한다.
	///     Connects to the specified target asynchronously.
	/// </summary>
	/// <param name="target">
	///     연결 대상 (RTU: COM 포트명, TCP/Pro: IP 주소).
	///     Connection target (RTU: COM port name, TCP/Pro: IP address).
	/// </param>
	/// <param name="option">
	///     연결 옵션 (RTU: 보드레이트, TCP/Pro: 포트 번호).
	///     Connection option (RTU: baud rate, TCP/Pro: port number).
	/// </param>
	/// <param name="ct">취소 토큰 / cancellation token</param>
	/// <returns>
	///     연결 시도 성공 여부. 실제 연결 완료는 <see cref="ConnectionChanged" />로 확인한다.
	///     Whether the connection attempt was initiated. Actual connection is confirmed via <see cref="ConnectionChanged" />.
	/// </returns>
	Task<bool> ConnectAsync(string target, int option, CancellationToken ct = default);

	/// <summary>
	///     연결을 해제한다.
	///     Closes the connection.
	/// </summary>
	void Close();

	/// <summary>
	///     데이터를 전송한다.
	///     Writes data to the transport.
	/// </summary>
	/// <param name="data">
	///     전송할 데이터.
	///     Data to transmit.
	/// </param>
	/// <returns>
	///     전송 성공 여부.
	///     Whether the write was successful.
	/// </returns>
	bool Write(ReadOnlySpan<byte> data);
}