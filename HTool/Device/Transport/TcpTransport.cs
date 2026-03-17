using System.Buffers;
using System.Net;
using System.Net.Sockets;
using HTool.Type;

namespace HTool.Device.Transport;

/// <summary>
///     MODBUS TCP 소켓 전송 계층.
///     MODBUS TCP socket transport layer.
/// </summary>
/// <remarks>
///     <para>
///         .NET Socket 기반 자체 구현 (SuperSimpleTcp 제거).
///         비동기 수신 루프로 데이터를 수집하여 <see cref="ITransport.DataReceived" />로 전달한다.
///     </para>
///     <para>
///         Native .NET Socket implementation (SuperSimpleTcp removed).
///         Collects data via async receive loop and forwards through <see cref="ITransport.DataReceived" />.
///     </para>
/// </remarks>
public sealed class TcpTransport : ITransport {
    // TCP 소켓 수신 버퍼 크기
    // receive buffer size for TCP socket
    private const int BufferSize = 16 * 1024;
    // 연결 설정 참조
    // connection settings reference
    private readonly HToolSettings.ConnectionSettings _connection;
    // 연결 상태 (크로스 스레드 가시성을 위해 volatile)
    // connection state (volatile for cross-thread visibility)
    private volatile bool _connected;
    // 연결 진입 가드 (0 = 유휴, 1 = 연결 중 또는 연결됨)
    // connection entry guard (0 = idle, 1 = connecting or connected)
    private int _connectGuard;
    // 수신 루프 취소 소스
    // cancellation source for receive loop
    private CancellationTokenSource? _cts;
    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;
    // TCP 소켓 인스턴스
    // TCP socket instance
    private Socket? _socket;

    /// <summary>
    ///     TcpTransport를 생성한다.
    ///     Creates a TcpTransport.
    /// </summary>
    /// <param name="connection">연결 설정 / connection settings</param>
    public TcpTransport(HToolSettings.ConnectionSettings connection) {
        // 연결 설정 저장
        // store connection settings
        _connection = connection;
    }

    /// <inheritdoc />
    public bool IsConnected => _connected;

    /// <inheritdoc />
    public event Action<ReadOnlyMemory<byte>>? DataReceived;

    /// <inheritdoc />
    public event Action<bool>? ConnectionChanged;

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(string target, int option, CancellationToken ct = default) {
        // 동시 연결 시도 차단 — 단 하나의 호출만 통과
        // block concurrent connection attempts — only one call proceeds
        if (Interlocked.CompareExchange(ref _connectGuard, 1, 0) != 0)
            // 이미 연결 중이거나 연결됨
            // already connecting or connected
            return false;

        // 가드: 해제된 인스턴스에서 연결 방지
        // guard: prevent connection on disposed instance
        if (_disposed) {
            // 가드 해제 후 거부
            // release guard and reject
            Interlocked.Exchange(ref _connectGuard, 0);
            // 해제된 전송 계층에서 연결 거부
            // reject connection on disposed transport
            return false;
        }

        // 가드: 유효한 IP 주소 확인
        // guard: ensure valid IP address
        if (!IPAddress.TryParse(target, out var ipAddress)) {
            // 가드 해제 — 재시도 허용
            // release guard — allow retry
            Interlocked.Exchange(ref _connectGuard, 0);
            // 유효하지 않은 IP 주소 형식 거부
            // reject invalid IP address format
            return false;
        }

        // 가드: 유효한 포트 번호 확인
        // guard: ensure valid port number
        if (option is < 1 or > 65535) {
            // 가드 해제 — 재시도 허용
            // release guard — allow retry
            Interlocked.Exchange(ref _connectGuard, 0);
            // 범위 밖 포트 번호 거부
            // reject out-of-range port number
            return false;
        }

        // 가드: 소켓 연결 실패 처리
        // guard: handle socket connection failure
        try {
            // TCP 소켓 생성
            // create TCP socket
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {
                // 저지연 MODBUS 통신을 위해 Nagle 알고리즘 비활성화
                // disable Nagle algorithm for low-latency MODBUS communication
                NoDelay = true,
                // 수신 버퍼 크기 설정
                // set receive buffer size
                ReceiveBufferSize = BufferSize,
                // 송신 버퍼 크기 설정
                // set send buffer size
                SendBufferSize = BufferSize
            };

            // TCP Keep-Alive 설정 (5초 간격)
            // configure TCP keep-alive (5 second interval)
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            // 파싱된 IP와 포트로 엔드포인트 생성
            // create endpoint from parsed IP and port
            var endpoint = new IPEndPoint(ipAddress, option);
            // 타임아웃 포함 연결 CTS 생성 (외부 취소 토큰 연계)
            // create linked CTS with timeout (combines external cancellation)
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            // 설정된 타임아웃 적용
            // apply configured timeout
            cts.CancelAfter(_connection.TcpConnectTimeout);
            // 비동기 소켓 연결
            // connect socket asynchronously
            await _socket.ConnectAsync(endpoint, cts.Token).ConfigureAwait(false);

            // 연결 상태 갱신
            // update connection state
            _connected = true;
            // 수신 루프용 취소 소스 생성
            // create cancellation source for receive loop
            _cts = new CancellationTokenSource();
            // 백그라운드 수신 루프 시작
            // start background receive loop
            _ = ReceiveLoopAsync(_cts.Token);
            // 연결 상태 변경 알림
            // notify connection state change
            ConnectionChanged?.Invoke(true);
            // 연결 성공 반환
            // return connection success
            return true;
        } catch {
            // 연결 실패 — 정리
            // connection failed — clean up
            CleanupSocket();
            // 연결 실패 시 가드 해제
            // release guard on connection failure
            Interlocked.Exchange(ref _connectGuard, 0);
            // 연결 실패 반환
            // return connection failure
            return false;
        }
    }

    /// <inheritdoc />
    public void Close() {
        // 가드 해제 — 이후 ConnectAsync() 재호출 허용
        // release guard — allow ConnectAsync() to be called again
        Interlocked.Exchange(ref _connectGuard, 0);
        // 수신 루프 취소
        // cancel receive loop
        _cts?.Cancel();
        // 연결 상태 갱신
        // update connection state
        _connected = false;
        // 소켓 정리
        // clean up socket
        CleanupSocket();
        // 연결 상태 변경 알림
        // notify connection state change
        ConnectionChanged?.Invoke(false);
    }

    /// <inheritdoc />
    public bool Write(ReadOnlySpan<byte> data) {
        // 소켓 쓰기 가능 여부 확인
        // check if socket is available for writing
        if (!_connected || _socket is null)
            // 소켓 미사용 가능 — 쓰기 불가
            // socket not available — cannot write
            return false;

        // 가드: 소켓 쓰기 실패 처리
        // guard: handle socket write failure
        try {
            // 소켓을 통해 데이터 전송
            // send data through socket
            _socket.Send(data);
            // 쓰기 성공 반환
            // return write success
            return true;
        } catch {
            // 전송 실패 (소켓 연결 해제 등)
            // send failed (socket disconnected, etc.)
            // 쓰기 실패 반환
            // return write failure
            return false;
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        // 이중 해제 방지
        // prevent double disposal
        if (_disposed)
            // 이미 해제됨 — 정리 건너뜀
            // already disposed — skip cleanup
            return;
        // 해제됨으로 표시
        // mark as disposed
        _disposed = true;
        // 가드 해제 (진행중인 가드 상태 정리)
        // release guard (clean up in-progress guard state)
        Interlocked.Exchange(ref _connectGuard, 0);
        // 수신 루프 취소 및 정리
        // cancel receive loop and clean up
        _cts?.Cancel();
        // 연결 상태 갱신
        // update connection state
        _connected = false;
        // 소켓 리소스 정리
        // clean up socket resources
        CleanupSocket();
    }

    /// <summary>
    ///     비동기 수신 루프. 백그라운드에서 소켓 데이터를 지속적으로 수신한다.
    ///     Async receive loop. Continuously receives socket data in background.
    /// </summary>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    private async Task ReceiveLoopAsync(CancellationToken ct) {
        // 무할당 수신을 위해 풀에서 버퍼 임차
        // rent buffer from pool for zero-allocation receive
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        // 가드: 루프 종료 시 리소스 정리 보장
        // guard: ensure resources are cleaned up when loop exits
        try {
            // 취소 요청까지 계속 수신
            // continue receiving until cancellation requested
            while (!ct.IsCancellationRequested && _socket is { Connected: true })
                // 가드: 비동기 루프 중 수신 실패 처리
                // guard: handle receive failure during async loop
                try {
                    // 소켓에서 비동기로 데이터 수신
                    // receive data from socket asynchronously
                    var bytesRead = await _socket.ReceiveAsync(
                        new Memory<byte>(buffer, 0, BufferSize), SocketFlags.None, ct
                    ).ConfigureAwait(false);

                    // 정상 연결 해제 확인 (0바이트)
                    // check for graceful disconnect (zero bytes)
                    if (bytesRead is 0)
                        // 원격 피어 연결 종료 — 루프 탈출
                        // remote peer closed connection — exit loop
                        break;

                    // 정확한 데이터 슬라이스로 구독자에게 알림 (동기 소비 계약)
                    // notify subscribers with exact data slice (synchronous consumption contract)
                    DataReceived?.Invoke(new ReadOnlyMemory<byte>(buffer, 0, bytesRead));
                } catch (OperationCanceledException) {
                    // 취소 요청됨 — 루프 탈출
                    // cancellation requested — exit loop
                    break;
                } catch (SocketException) {
                    // 소켓 오류 — 연결 끊김
                    // socket error — connection lost
                    break;
                }
        } finally {
            // 버퍼를 풀에 반환
            // return buffer to pool
            ArrayPool<byte>.Shared.Return(buffer);
        }

        // 예기치 않은 연결 해제 확인 (사용자 주도가 아닌 경우)
        // check if disconnection was unexpected (not user-initiated)
        if (_connected) {
            // 연결 상태 갱신
            // update connection state
            _connected = false;
            // 가드 해제 — 재연결 허용
            // release guard — allow reconnection
            Interlocked.Exchange(ref _connectGuard, 0);
            // 예기치 않은 연결 해제 알림
            // notify unexpected disconnection
            ConnectionChanged?.Invoke(false);
        }
    }

    /// <summary>
    ///     소켓 리소스를 정리한다.
    ///     Cleans up socket resources.
    /// </summary>
    private void CleanupSocket() {
        // 가드: 정리 오류를 우아하게 처리
        // guard: handle cleanup errors gracefully
        try {
            // 연결된 경우 소켓 종료
            // shutdown socket if connected
            if (_socket is { Connected: true })
                // 송수신 모두 정상 종료
                // gracefully shut down both send and receive
                _socket.Shutdown(SocketShutdown.Both);
        } catch {
            // 정리 중 종료 오류 무시
            // ignore shutdown errors during cleanup
        }

        // 소켓 해제
        // dispose socket
        _socket?.Dispose();
        // 참조 제거
        // clear reference
        _socket = null;

        // 취소 소스 해제
        // dispose cancellation source
        _cts?.Dispose();
        // 참조 제거
        // clear reference
        _cts = null;
    }
}