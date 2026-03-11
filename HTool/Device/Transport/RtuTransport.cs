using System.Buffers;
using System.IO.Ports;
using System.Text;
using HTool.Type;

namespace HTool.Device.Transport;

/// <summary>
///     MODBUS RTU 시리얼 포트 전송 계층.
///     MODBUS RTU serial port transport layer.
/// </summary>
/// <remarks>
///     <para>
///         System.IO.Ports 기반. 비동기 수신 이벤트로 데이터를 수집하여
///         <see cref="ITransport.DataReceived" />로 전달한다.
///     </para>
///     <para>
///         Based on System.IO.Ports. Collects data via async receive events
///         and forwards through <see cref="ITransport.DataReceived" />.
///     </para>
/// </remarks>
public sealed class RtuTransport : ITransport {
    // 시리얼 포트 수신 버퍼 크기
    // receive buffer size for serial port
    private const int BufferSize = 16 * 1024;
    // 연결 설정 참조
    // connection settings reference
    private readonly HToolSettings.ConnectionSettings _connection;
    // 연결 진입 가드 (0 = 유휴, 1 = 연결 중 또는 연결됨)
    // connection entry guard (0 = idle, 1 = connecting or connected)
    private int _connectGuard;
    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;

    // 시리얼 포트 인스턴스
    // serial port instance
    private SerialPort? _port;

    /// <summary>
    ///     RtuTransport를 생성한다.
    ///     Creates an RtuTransport.
    /// </summary>
    /// <param name="connection">연결 설정 / connection settings</param>
    public RtuTransport(HToolSettings.ConnectionSettings connection) {
        // 연결 설정 저장
        // store connection settings
        _connection = connection;
    }

    /// <inheritdoc />
    public bool IsConnected => _port?.IsOpen is true;

    /// <inheritdoc />
    public event Action<ReadOnlyMemory<byte>>? DataReceived;

    /// <inheritdoc />
    public event Action<bool>? ConnectionChanged;

    /// <inheritdoc />
    public Task<bool> ConnectAsync(string target, int option, CancellationToken ct = default) {
        // 동시 연결 시도 차단 — 단 하나의 호출만 통과
        // block concurrent connection attempts — only one call proceeds
        if (Interlocked.CompareExchange(ref _connectGuard, 1, 0) != 0)
            // 이미 연결 중이거나 연결됨
            // already connecting or connected
            return Task.FromResult(false);

        // 가드: 해제된 인스턴스에서 연결 방지
        // guard: prevent connection on disposed instance
        if (_disposed) {
            // 가드 해제 후 거부
            // release guard and reject
            Interlocked.Exchange(ref _connectGuard, 0);
            // 해제된 전송 계층에서 연결 거부
            // reject connection on disposed transport
            return Task.FromResult(false);
        }

        // 가드: 시리얼 포트 열기 실패 처리
        // guard: handle serial port open failure
        try {
            // 동기 연결 수행 (SerialPort.Open은 즉각 반환)
            // perform synchronous connect (SerialPort.Open returns immediately)
            var result = SyncConnect(target, option);
            // 실패 시 가드 해제
            // release guard on failure
            if (!result)
                // 연결 실패 — 재시도 허용
                // connection failed — allow retry
                Interlocked.Exchange(ref _connectGuard, 0);
            // 연결 결과 반환
            // return connection result
            return Task.FromResult(result);
        } catch {
            // 연결 실패 시 가드 해제
            // release guard on connection failure
            Interlocked.Exchange(ref _connectGuard, 0);
            // 연결 실패 반환
            // return connection failure
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public void Close() {
        // 가드 해제 — 이후 ConnectAsync() 재호출 허용
        // release guard — allow ConnectAsync() to be called again
        Interlocked.Exchange(ref _connectGuard, 0);
        // 시리얼 포트 정리 및 닫기
        // close and clean up serial port
        CleanupPort();
        // 연결 상태 변경 알림
        // notify connection state change
        ConnectionChanged?.Invoke(false);
    }

    /// <inheritdoc />
    public bool Write(ReadOnlySpan<byte> data) {
        // 포트 쓰기 가능 여부 확인
        // check if port is available for writing
        if (_port?.IsOpen is not true)
            // 포트 미열림 — 쓰기 불가
            // port not open — cannot write
            return false;

        // 가드: 시리얼 포트 쓰기 실패 처리
        // guard: handle serial port write failure
        try {
            // 시리얼 포트에 데이터 쓰기
            // write data to serial port
            _port.Write(data.ToArray(), 0, data.Length);
            // 쓰기 성공 반환
            // return write success
            return true;
        } catch {
            // 쓰기 실패 (포트 연결 해제 등)
            // write failed (port disconnected, etc.)
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
        // 시리얼 포트 정리
        // clean up serial port
        CleanupPort();
    }

    /// <summary>
    ///     시리얼 포트를 동기적으로 열고 연결한다.
    ///     Opens and connects the serial port synchronously.
    /// </summary>
    /// <param name="target">COM 포트명 / COM port name</param>
    /// <param name="option">보드레이트 / baud rate</param>
    /// <returns>연결 성공 여부 / whether connection succeeded</returns>
    private bool SyncConnect(string target, int option) {
        // 가드: 대상 포트명 유효성 확인
        // guard: ensure target port name is provided
        if (string.IsNullOrWhiteSpace(target))
            // 빈 또는 공백 포트명 거부
            // reject empty or whitespace port name
            return false;

        // 가드: 유효한 보드레이트 확인
        // guard: ensure valid baud rate
        if (option <= 0)
            // 유효하지 않은 보드레이트 거부
            // reject invalid baud rate
            return false;

        // 지정된 파라미터로 시리얼 포트 생성
        // create serial port with specified parameters
        _port = new SerialPort(target, option, _connection.RtuParity, 8, _connection.RtuStopBits) {
            // 바이너리 안전 인코딩 사용
            // use binary-safe encoding
            Encoding = Encoding.GetEncoding(28591),
            // 읽기/쓰기 버퍼 크기 설정
            // set read/write buffer sizes
            ReadBufferSize  = BufferSize,
            WriteBufferSize = BufferSize,
            // 핸드셰이킹 비활성화
            // disable handshaking
            Handshake = Handshake.None,
            // 읽기 타임아웃 설정
            // set read timeout
            ReadTimeout = 500
        };

        // 비동기 데이터 수신 핸들러 등록
        // register async data receive handler
        _port.DataReceived += OnDataReceived;
        // 시리얼 포트 열기
        // open serial port
        _port.Open();
        // 연결 상태 변경 알림
        // notify connection state change
        ConnectionChanged?.Invoke(true);
        // 연결 성공 반환
        // return connection success
        return true;
    }

    /// <summary>
    ///     시리얼 포트 비동기 수신 이벤트 핸들러.
    ///     Serial port async data receive event handler.
    /// </summary>
    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e) {
        // 포트가 아직 열려 있는지 확인
        // check if port is still open
        if (_port?.IsOpen is not true)
            // 포트 닫힘 — 수신 데이터 폐기
            // port closed — discard incoming data
            return;

        // 가드: 비동기 수신 중 읽기 실패 처리
        // guard: handle read failure during async receive
        try {
            // 읽을 수 있는 바이트 수 확인
            // get number of bytes available to read
            var bytesToRead = _port.BytesToRead;
            // 데이터가 없으면 건너뜀
            // skip if no data available
            if (bytesToRead <= 0)
                // 읽을 데이터 없음 — 핸들러 종료
                // nothing to read — exit handler
                return;

            // 무할당 읽기를 위해 풀에서 버퍼 임차
            // rent buffer from pool for zero-allocation read
            var buffer = ArrayPool<byte>.Shared.Rent(bytesToRead);
            // 가드: 사용 후 버퍼를 풀에 반환 보장
            // guard: ensure buffer is returned to pool after use
            try {
                // 시리얼 포트에서 임차 버퍼로 데이터 읽기
                // read data from serial port into rented buffer
                var bytesRead = _port.Read(buffer, 0, bytesToRead);
                // 실제로 데이터가 읽혔는지 확인
                // check if data was actually read
                if (bytesRead > 0)
                    // 정확한 데이터 슬라이스로 구독자에게 알림 (동기 소비 계약)
                    // notify subscribers with exact data slice (synchronous consumption contract)
                    DataReceived?.Invoke(new ReadOnlyMemory<byte>(buffer, 0, bytesRead));
            } finally {
                // 성공 여부와 관계없이 버퍼를 풀에 반환
                // return buffer to pool regardless of success or failure
                ArrayPool<byte>.Shared.Return(buffer);
            }
        } catch {
            // 읽기 실패 — 포트 연결이 끊어졌을 수 있음
            // read failed — port may have been disconnected
        }
    }

    /// <summary>
    ///     시리얼 포트를 정리한다.
    ///     Cleans up the serial port resources.
    /// </summary>
    private void CleanupPort() {
        // 포트 존재 여부 확인
        // check if port exists
        if (_port is null)
            // 정리할 포트 없음
            // no port to clean up
            return;

        // 이벤트 핸들러 해제
        // unregister event handler
        _port.DataReceived -= OnDataReceived;
        // 가드: 닫기 실패를 우아하게 처리
        // guard: handle close failure gracefully
        try {
            // 아직 열려 있으면 포트 닫기
            // close port if still open
            if (_port.IsOpen)
                // 시리얼 포트 닫기
                // close the serial port
                _port.Close();
        } catch {
            // 정리 중 닫기 오류 무시
            // ignore close errors during cleanup
        }

        // 포트 리소스 해제
        // dispose port resources
        _port.Dispose();
        // 참조 제거
        // clear reference
        _port = null;
    }
}