using System.Timers;
using HTool.Device;
using HTool.Device.Codec;
using HTool.Device.Pro;
using HTool.Device.Protocol;
using HTool.Device.Transport;
using HTool.Format.Device;
using HTool.Type;
using Timer = System.Timers.Timer;

namespace HTool;

/// <summary>
///     HANTAS 장비와의 MODBUS 통신을 위한 통합 라이브러리 진입점.
///     직접 RTU/TCP 연결과 PRO X 게이트웨이 연결을 동일한 API로 제공한다.
///     동일 인스턴스에서 ComType 전환이 가능하다.
///     Unified library entry point for MODBUS communication with HANTAS devices.
///     Provides direct RTU/TCP and PRO X gateway connections through the same API.
///     Supports ComType switching on the same instance.
/// </summary>
public sealed class HTool : IDisposable {
    // MODBUS 읽기 최대 레지스터 수
    // maximum register count per MODBUS read request
    private const int MaxReadRegisters = 125;

    // MODBUS 쓰기 최대 레지스터 수
    // maximum register count per MODBUS write request
    private const int MaxWriteRegisters = 123;

    // 코덱 (직접 연결 전용, PRO X 모드에서는 null)
    // codec (direct connection only, null in PRO X mode)
    private IModbusCodec? _codec;

    // ConnectionState backing field — Volatile.Read/Write로 스레드 안전 보장
    // ConnectionState backing field — thread-safe via Volatile.Read/Write
    private int _connectionState = (int)Connection.Closed;
    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;
    // Keep-Alive 타이머 (직접 연결 전용, PRO X 모드에서는 null)
    // keep-alive timer (direct connection only, null in PRO X mode)
    private Timer? _keepAliveTimer;
    // Keep-Alive 마지막 활동 시각 — ticks 단위 (스레드 안전)
    // keep-alive last activity time — in ticks (thread-safe)
    private long _lastActivityTicks;
    // 메시지 파이프라인 (직접 연결 전용, PRO X 모드에서는 null)
    // message pipeline (direct connection only, null in PRO X mode)
    private MessagePipeline? _pipeline;

    // MODBUS 슬레이브 ID
    // MODBUS slave ID
    private byte _slaveId = 0x01;

    // 연결 대상 (IP 주소 또는 COM 포트명)
    // connection target (IP address or COM port name)
    private string _target = string.Empty;
    // TCP 트랜잭션 ID (자동 증가)
    // TCP transaction ID (auto-increment)
    private ushort _transactionId;

    // 전송 계층 (Connect 시 생성, 타입 전환 시 재생성)
    // transport layer (created on Connect, recreated on type switch)
    private ITransport? _transport;

    /// <summary>
    ///     HTool 인스턴스를 생성한다. 컴포넌트는 Connect() 호출 시 지연 생성된다.
    ///     Creates an HTool instance. Components are lazily created on Connect().
    /// </summary>
    public HTool() {
        // 로거 인스턴스 생성
        // create logger instance
        Logger = new HToolLogger();
    }

    /// <summary>
    ///     HTool 인스턴스를 초기 통신 유형과 함께 생성한다.
    ///     컴포넌트는 Connect() 호출 시 지연 생성된다.
    ///     Creates an HTool instance with an initial communication type.
    ///     Components are lazily created on Connect().
    /// </summary>
    /// <param name="type">통신 유형 (Rtu, Tcp, Pro) / communication type (Rtu, Tcp, Pro)</param>
    public HTool(ComType type) : this() {
        // 초기 통신 유형 설정
        // set initial communication type
        Type = type;
    }

    /// <summary>
    ///     통신 유형. Connect(ComType, ...) 호출 시 변경된다.
    ///     Communication type. Changed on Connect(ComType, ...) calls.
    /// </summary>
    public ComType Type { get; private set; }

    /// <summary>
    ///     현재 연결 상태.
    ///     Current connection state.
    /// </summary>
    public Connection ConnectionState {
        get => (Connection)Volatile.Read(ref _connectionState);
        private set {
            // 새 상태 저장
            // store new state
            Volatile.Write(ref _connectionState, (int)value);
            // 4-state 이벤트 발행
            // raise 4-state event
            ConnectionStateChanged?.Invoke(value);
        }
    }

    /// <summary>
    ///     장치 기본 정보. 연결 완료 후 설정된다. PRO X 모드에서는 사용하지 않는다.
    ///     Device basic information. Set after connection is established. Not used in PRO X mode.
    /// </summary>
    public SimpleInfo Info { get; private set; }

    /// <summary>
    ///     인스턴스별 설정. Connect() 호출 전에 설정한다.
    ///     Per-instance settings. Configure before calling Connect().
    /// </summary>
    public HToolSettings Settings { get; } = new();

    /// <summary>
    ///     통합 로거 인스턴스.
    ///     Integrated logger instance.
    /// </summary>
    public HToolLogger Logger { get; }

    /// <summary>
    ///     PRO X 서비스. 직접 연결 시 null. ComType 전환 시 재생성된다.
    ///     PRO X service. null for direct connection. Recreated on ComType switch.
    /// </summary>
    public ProService? Pro { get; private set; }

    /// <summary>
    ///     PRO X 모드 여부.
    ///     Whether this instance is in PRO X mode.
    /// </summary>
    private bool IsProMode => Type is ComType.Pro;

    /// <inheritdoc />
    public void Dispose() {
        // 이중 해제 방지
        // prevent double disposal
        if (_disposed)
            // 이미 해제됨 — 건너뜀
            // already disposed — skip
            return;
        // 해제됨으로 표시
        // mark as disposed
        _disposed = true;

        // 내부 컴포넌트 해제 (Transport, Pipeline, ProService, Timer)
        // dispose internal components (Transport, Pipeline, ProService, Timer)
        DisposeComponents();
        // 로거 해제
        // dispose logger
        Logger.Dispose();
        // Finalizer 큐 진입 방지
        // suppress finalizer queue entry
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Dispose 누락 시 안전망.
    ///     Safety net for missed Dispose calls.
    /// </summary>
    ~HTool() {
        // Dispose 패턴 위임
        // delegate to Dispose pattern
        Dispose();
    }

    /// <summary>
    ///     연결 상태 변경 이벤트. true = 연결됨, false = 해제됨.
    ///     Connection state changed event. true = connected, false = disconnected.
    /// </summary>
    public event Action<bool>? ChangedConnect;

    /// <summary>
    ///     연결 상태 변경 이벤트 (4-state). Close / Closed / Connecting / Connected 상태를 전달한다.
    ///     Connection state changed event (4-state). Delivers Close / Closed / Connecting / Connected states.
    /// </summary>
    public event Action<Connection>? ConnectionStateChanged;

    /// <summary>
    ///     MODBUS 응답 수신 이벤트.
    ///     MODBUS response received event.
    /// </summary>
    public event Action<ModbusResponse>? ReceivedData;

    /// <summary>
    ///     통신 오류 이벤트.
    ///     Communication error event.
    /// </summary>
    public event Action<ComError>? ReceiveError;

    /// <summary>
    ///     통신 유형을 지정하여 대상에 비동기로 연결한다. 유형이 변경되면 내부 컴포넌트를 재생성한다.
    ///     Asynchronously connects to the target with a specified communication type.
    ///     Rebuilds internal components when the type changes.
    /// </summary>
    /// <param name="type">통신 유형 (Rtu, Tcp, Pro) / communication type (Rtu, Tcp, Pro)</param>
    /// <param name="target">
    ///     연결 대상 (RTU: COM 포트명, TCP/Pro: IP 주소).
    ///     Connection target (RTU: COM port name, TCP/Pro: IP address).
    /// </param>
    /// <param name="option">
    ///     연결 옵션 (RTU: 보드레이트, TCP/Pro: 포트 번호).
    ///     Connection option (RTU: baud rate, TCP/Pro: port number).
    /// </param>
    /// <param name="id">MODBUS 슬레이브 ID. 기본값 0x01 / MODBUS slave ID. Default 0x01</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>연결 시도 성공 여부 / whether the connection attempt was initiated</returns>
    public async Task<bool> ConnectAsync(
        ComType           type,
        string            target,
        int               option,
        byte              id = 0x01,
        CancellationToken ct = default) {
        // 이미 연결 중이거나 연결된 상태면 거부
        // reject if already connecting or connected
        if (ConnectionState is Connection.Connecting or Connection.Connected)
            // 중복 연결 시도 거부
            // reject duplicate connection attempt
            return false;

        // 타입 변경 또는 첫 연결 시 컴포넌트 재생성
        // rebuild components on type change or first connection
        if (type != Type || _transport is null)
            // 새 통신 유형으로 컴포넌트 생성
            // build components for new communication type
            BuildComponents(type);

        // 비동기 연결 위임
        // delegate to async connection
        return await ConnectInternalAsync(target, option, id, ct).ConfigureAwait(false);
    }

    /// <summary>
    ///     현재 설정된 통신 유형으로 대상에 비동기로 연결한다.
    ///     Asynchronously connects to the target using the currently configured communication type.
    /// </summary>
    /// <param name="target">
    ///     연결 대상 (RTU: COM 포트명, TCP/Pro: IP 주소).
    ///     Connection target (RTU: COM port name, TCP/Pro: IP address).
    /// </param>
    /// <param name="option">
    ///     연결 옵션 (RTU: 보드레이트, TCP/Pro: 포트 번호).
    ///     Connection option (RTU: baud rate, TCP/Pro: port number).
    /// </param>
    /// <param name="id">MODBUS 슬레이브 ID. 기본값 0x01 / MODBUS slave ID. Default 0x01</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>연결 시도 성공 여부 / whether the connection attempt was initiated</returns>
    public async Task<bool> ConnectAsync(
        string            target,
        int               option,
        byte              id = 0x01,
        CancellationToken ct = default) {
        // 이미 연결 중이거나 연결된 상태면 거부
        // reject if already connecting or connected
        if (ConnectionState is Connection.Connecting or Connection.Connected)
            // 중복 연결 시도 거부
            // reject duplicate connection attempt
            return false;

        // 첫 연결 시 컴포넌트 생성
        // build components on first connection
        if (_transport is null)
            // 현재 통신 유형으로 컴포넌트 생성
            // build components for current communication type
            BuildComponents(Type);

        // 비동기 연결 위임
        // delegate to async connection
        return await ConnectInternalAsync(target, option, id, ct).ConfigureAwait(false);
    }

    /// <summary>
    ///     연결을 해제한다. 컴포넌트는 정지만 하고 해제하지 않는다.
    ///     Closes the connection. Components are stopped but not disposed.
    /// </summary>
    public void Close() {
        // 이미 닫혀있는 상태면 무시
        // ignore if already closed
        if (ConnectionState is Connection.Closed or Connection.Close)
            // 이미 닫힌 상태 — 반환
            // already closed — return
            return;

        // 연결 해제 상태로 변경
        // set state to closing
        ConnectionState = Connection.Close;
        // 연결 해제 로그 기록
        // log disconnection
        Logger.Log(LogCategories.Connection, LogLevel.Info, "Closing connection");

        // PRO X 모드 정리
        // PRO X mode cleanup
        if (Pro is not null) {
            // ProService 정지
            // stop ProService
            Pro.Stop();
        } else {
            // 직접 연결 모드 정리
            // direct connection mode cleanup
            // Keep-Alive 타이머 정지
            // stop keep-alive timer
            _keepAliveTimer?.Stop();
            // 파이프라인 정지
            // stop pipeline
            _pipeline?.Stop();
        }

        // 전송 계층 닫기
        // close transport
        _transport?.Close();
    }

    /// <summary>
    ///     보유 레지스터를 읽는다 (FC 0x03). 125개 초과 시 자동 분할.
    ///     Reads holding registers (FC 0x03). Auto-splits if count exceeds 125.
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">레지스터 개수 / register count</param>
    /// <returns>요청 인큐 성공 여부 / whether all requests were enqueued</returns>
    public bool ReadHoldingReg(ushort addr, ushort count) {
        // 자동 분할 읽기 위임
        // delegate to auto-split read
        return EnqueueReadRequests(FunctionCode.ReadHoldingReg, addr, count);
    }

    /// <summary>
    ///     입력 레지스터를 읽는다 (FC 0x04). 125개 초과 시 자동 분할.
    ///     Reads input registers (FC 0x04). Auto-splits if count exceeds 125.
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">레지스터 개수 / register count</param>
    /// <returns>요청 인큐 성공 여부 / whether all requests were enqueued</returns>
    public bool ReadInputReg(ushort addr, ushort count) {
        // 자동 분할 읽기 위임
        // delegate to auto-split read
        return EnqueueReadRequests(FunctionCode.ReadInputReg, addr, count);
    }

    /// <summary>
    ///     단일 레지스터에 값을 쓴다 (FC 0x06).
    ///     Writes a value to a single register (FC 0x06).
    /// </summary>
    /// <param name="addr">레지스터 주소 / register address</param>
    /// <param name="value">쓸 값 / value to write</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool WriteSingleReg(ushort addr, ushort value) {
        // 연결 상태 확인
        // check connection state
        if (ConnectionState is not Connection.Connected)
            // 미연결 — 거부
            // not connected — reject
            return false;

        // PRO X 모드: MODBUS 패스스루
        // PRO X mode: MODBUS passthrough
        if (IsProMode)
            // ProService를 통해 MODBUS 패스스루
            // MODBUS passthrough via ProService
            return EnqueueProModbusRequest(FunctionCode.WriteSingleReg, addr, value);

        // 트랜잭션 ID 발급
        // issue transaction ID
        var tId = NextTransactionId();
        // 패킷 생성
        // build packet
        var packet = _codec!.BuildWriteSingleReg(addr, value, _slaveId, tId);
        // 요청 생성 및 인큐
        // create request and enqueue
        var request = new ModbusRequest(FunctionCode.WriteSingleReg, addr, packet);
        // 마지막 활동 시각 갱신
        // update last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // 요청 인큐
        // enqueue request
        return _pipeline!.Enqueue(request);
    }

    /// <summary>
    ///     다중 레지스터에 값을 쓴다 (FC 0x10).
    ///     Writes values to multiple registers (FC 0x10).
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="values">쓸 값 배열 / values to write</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool WriteMultiReg(ushort addr, ushort[] values) {
        // ReadOnlySpan 오버로드 위임
        // delegate to ReadOnlySpan overload
        return WriteMultiReg(addr, (ReadOnlySpan<ushort>)values);
    }

    /// <summary>
    ///     다중 레지스터에 값을 쓴다 (FC 0x10, Span 오버로드).
    ///     Writes values to multiple registers (FC 0x10, Span overload).
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="values">쓸 값 스팬 / values span to write</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool WriteMultiReg(ushort addr, ReadOnlySpan<ushort> values) {
        // 연결 상태 확인
        // check connection state
        if (ConnectionState is not Connection.Connected)
            // 미연결 — 거부
            // not connected — reject
            return false;

        // 가드: 레지스터 개수 범위 확인
        // guard: validate register count range
        if (values.Length is 0 or > MaxWriteRegisters)
            // 범위 초과 — 거부
            // out of range — reject
            return false;

        // PRO X 모드: MODBUS 패스스루
        // PRO X mode: MODBUS passthrough
        if (IsProMode) {
            // MODBUS TCP 코덱으로 패킷 생성
            // build packet with MODBUS TCP codec
            var proCodec = new ModbusTcpCodec();
            // 패킷 생성
            // build packet
            var proPacket = proCodec.BuildWriteMultiReg(addr, values, _slaveId);
            // ProService를 통해 MODBUS 패스스루
            // MODBUS passthrough via ProService
            return Pro!.EnqueueModbusRequest(proPacket);
        }

        // 트랜잭션 ID 발급
        // issue transaction ID
        var tId = NextTransactionId();
        // 패킷 생성
        // build packet
        var packet = _codec!.BuildWriteMultiReg(addr, values, _slaveId, tId);
        // 요청 생성 및 인큐
        // create request and enqueue
        var request = new ModbusRequest(FunctionCode.WriteMultiReg, addr, packet);
        // 마지막 활동 시각 갱신
        // update last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // 요청 인큐
        // enqueue request
        return _pipeline!.Enqueue(request);
    }

    /// <summary>
    ///     문자열을 레지스터에 쓴다 (FC 0x10, 문자열 인코딩).
    ///     Writes a string to registers (FC 0x10, string encoding).
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="str">쓸 문자열 / string to write</param>
    /// <param name="length">고정 바이트 길이 (0이면 문자열 길이 사용) / fixed byte length (0 uses string length)</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool WriteStrReg(ushort addr, string str, int length = 0) {
        // 연결 상태 확인
        // check connection state
        if (ConnectionState is not Connection.Connected)
            // 미연결 — 거부
            // not connected — reject
            return false;

        // PRO X 모드: MODBUS 패스스루
        // PRO X mode: MODBUS passthrough
        if (IsProMode) {
            // MODBUS TCP 코덱으로 패킷 생성
            // build packet with MODBUS TCP codec
            var proCodec = new ModbusTcpCodec();
            // 패킷 생성
            // build packet
            var proPacket = proCodec.BuildWriteStrReg(addr, str, length, _slaveId);
            // ProService를 통해 MODBUS 패스스루
            // MODBUS passthrough via ProService
            return Pro!.EnqueueModbusRequest(proPacket);
        }

        // 트랜잭션 ID 발급
        // issue transaction ID
        var tId = NextTransactionId();
        // 패킷 생성
        // build packet
        var packet = _codec!.BuildWriteStrReg(addr, str, length, _slaveId, tId);
        // 요청 생성 및 인큐
        // create request and enqueue
        var request = new ModbusRequest(FunctionCode.WriteMultiReg, addr, packet);
        // 마지막 활동 시각 갱신
        // update last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // 요청 인큐
        // enqueue request
        return _pipeline!.Enqueue(request);
    }

    /// <summary>
    ///     장치 정보를 읽는다 (FC 0x11, HANTAS 전용). PRO X 모드에서는 사용하지 않는다.
    ///     Reads device information (FC 0x11, HANTAS custom). Not used in PRO X mode.
    /// </summary>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool ReadInfoReg() {
        // PRO X 모드에서는 지원하지 않음
        // not supported in PRO X mode
        if (IsProMode)
            // PRO X 모드 — 거부
            // PRO X mode — reject
            return false;

        // 연결 해제 상태면 거부 (Connecting 상태에서도 허용)
        // reject if closed (allowed during Connecting state)
        if (ConnectionState is Connection.Closed or Connection.Close)
            // 미연결 — 거부
            // not connected — reject
            return false;

        // 트랜잭션 ID 발급
        // issue transaction ID
        var tId = NextTransactionId();
        // 패킷 생성
        // build packet
        var packet = _codec!.BuildReadInfoReg(_slaveId, tId);
        // 요청 생성 및 인큐
        // create request and enqueue
        var request = new ModbusRequest(FunctionCode.ReadInfoReg, 0, packet);
        // 마지막 활동 시각 갱신
        // update last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // 요청 인큐
        // enqueue request
        return _pipeline!.Enqueue(request);
    }

    /// <summary>
    ///     통신 유형에 따라 내부 컴포넌트를 생성한다.
    ///     기존 컴포넌트가 있으면 먼저 해제한다.
    ///     Builds internal components based on communication type.
    ///     Disposes existing components first if present.
    /// </summary>
    /// <param name="type">통신 유형 / communication type</param>
    private void BuildComponents(ComType type) {
        // 기존 컴포넌트 해제
        // dispose existing components
        DisposeComponents();

        // 통신 유형 설정
        // set communication type
        Type = type;

        // 통신 유형에 따라 전송 계층과 코덱 생성
        // create transport and codec based on communication type
        switch (type) {
            case ComType.Rtu:
                // RTU 전송 계층 생성
                // create RTU transport
                _transport = new RtuTransport(Settings.Connection);
                // RTU 코덱 생성
                // create RTU codec
                _codec = new ModbusRtuCodec();
                // RTU 전송 계층 생성 완료
                // RTU transport setup complete
                break;
            case ComType.Tcp:
                // TCP 전송 계층 생성
                // create TCP transport
                _transport = new TcpTransport(Settings.Connection);
                // TCP 코덱 생성
                // create TCP codec
                _codec = new ModbusTcpCodec();
                // TCP 전송 계층 생성 완료
                // TCP transport setup complete
                break;
            case ComType.Pro:
                // PRO X는 TCP 전송 계층 사용
                // PRO X uses TCP transport
                _transport = new TcpTransport(Settings.Connection);
                // PRO X 서비스 생성
                // create PRO X service
                Pro = new ProService(_transport, Logger, Settings);
                // ProService 연결 상태 변경 이벤트 구독
                // subscribe to ProService connection state change event
                Pro.ConnectionChanged += OnProConnectionChanged;
                // ProService MODBUS 응답 이벤트 구독
                // subscribe to ProService MODBUS response event
                Pro.ModbusResponseReceived += OnProModbusResponse;
                // PRO X 서비스 생성 완료
                // PRO X service setup complete
                break;
            default:
                // 지원하지 않는 통신 유형 예외
                // unsupported communication type
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported communication type");
        }

        // 직접 연결 모드에서만 파이프라인과 Keep-Alive 타이머 생성
        // create pipeline and Keep-Alive timer only in direct connection mode
        if (type is not ComType.Pro) {
            // 메시지 파이프라인 생성
            // create message pipeline
            _pipeline = new MessagePipeline(_transport, _codec!, Logger, Settings);
            // 파이프라인 응답 이벤트 구독
            // subscribe to pipeline response event
            _pipeline.ResponseReceived += OnResponseReceived;
            // 파이프라인 오류 이벤트 구독
            // subscribe to pipeline error event
            _pipeline.ErrorOccurred += OnErrorOccurred;

            // Keep-Alive 타이머 생성 (Connect 시점에 Settings.KeepAlive.Period 반영)
            // create keep-alive timer (Settings.KeepAlive.Period applied at Connect time)
            _keepAliveTimer = new Timer(1000) {
                // 반복 실행 활성화
                // enable auto-reset
                AutoReset = true
            };
            // Keep-Alive 타이머 이벤트 핸들러 등록
            // register keep-alive timer event handler
            _keepAliveTimer.Elapsed += OnKeepAliveElapsed;
        }

        // 전송 계층 연결 상태 변경 이벤트 구독
        // subscribe to transport connection state change event
        _transport.ConnectionChanged += OnConnectionChanged;
    }

    /// <summary>
    ///     내부 컴포넌트를 해제한다. Logger와 Settings는 유지한다.
    ///     Disposes internal components. Logger and Settings are preserved.
    /// </summary>
    private void DisposeComponents() {
        // PRO X 모드 정리
        // PRO X mode cleanup
        if (Pro is not null) {
            // ProService 이벤트 해제
            // unsubscribe ProService events
            Pro.ConnectionChanged -= OnProConnectionChanged;
            // ProService MODBUS 응답 이벤트 해제
            // unsubscribe ProService MODBUS response event
            Pro.ModbusResponseReceived -= OnProModbusResponse;
            // ProService 해제
            // dispose ProService
            Pro.Dispose();
            // ProService 참조 제거
            // clear ProService reference
            Pro = null;
        }

        // 직접 연결 모드 정리
        // direct connection mode cleanup
        if (_keepAliveTimer is not null) {
            // Keep-Alive 타이머 정지
            // stop keep-alive timer
            _keepAliveTimer.Stop();
            // Keep-Alive 타이머 이벤트 핸들러 해제
            // unsubscribe keep-alive timer event handler
            _keepAliveTimer.Elapsed -= OnKeepAliveElapsed;
            // Keep-Alive 타이머 해제
            // dispose keep-alive timer
            _keepAliveTimer.Dispose();
            // Keep-Alive 타이머 참조 제거
            // clear keep-alive timer reference
            _keepAliveTimer = null;
        }

        // 파이프라인 해제 (Transport 구독 해제를 위해 Transport보다 먼저 해제)
        // dispose pipeline (before Transport to unsubscribe from Transport.DataReceived)
        if (_pipeline is not null) {
            // 파이프라인 해제
            // dispose pipeline
            _pipeline.Dispose();
            // 파이프라인 참조 제거
            // clear pipeline reference
            _pipeline = null;
        }

        // 전송 계층 해제
        // dispose transport
        if (_transport is not null) {
            // 전송 계층 이벤트 해제
            // unsubscribe transport events
            _transport.ConnectionChanged -= OnConnectionChanged;
            // 전송 계층 해제
            // dispose transport
            _transport.Dispose();
            // 전송 계층 참조 제거
            // clear transport reference
            _transport = null;
        }

        // 코덱 참조 제거 (상태 없음, Dispose 불필요)
        // clear codec reference (stateless, no Dispose needed)
        _codec = null;

        // 장치 정보 초기화 (stale 데이터 방지)
        // reset device info (prevent stale data)
        Info = default;
    }

    /// <summary>
    ///     연결 공유 로직. 전송 계층 비동기 연결을 시도한다.
    ///     Shared connection logic. Attempts async transport connection.
    /// </summary>
    /// <param name="target">
    ///     연결 대상 (RTU: COM 포트명, TCP/Pro: IP 주소).
    ///     Connection target (RTU: COM port name, TCP/Pro: IP address).
    /// </param>
    /// <param name="option">
    ///     연결 옵션 (RTU: 보드레이트, TCP/Pro: 포트 번호).
    ///     Connection option (RTU: baud rate, TCP/Pro: port number).
    /// </param>
    /// <param name="id">MODBUS 슬레이브 ID / MODBUS slave ID</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>연결 시도 성공 여부 / whether the connection attempt was initiated</returns>
    private async Task<bool> ConnectInternalAsync(
        string            target,
        int               option,
        byte              id,
        CancellationToken ct) {
        // 연결 대상 저장 (FTP 서비스에서 사용)
        // store connection target (used by FTP service)
        _target = target;
        // 슬레이브 ID 설정
        // set slave ID
        _slaveId = id;
        // 트랜잭션 ID 초기화
        // reset transaction ID
        _transactionId = 0;
        // 연결 상태를 연결 중으로 변경
        // set connection state to connecting
        ConnectionState = Connection.Connecting;
        // 연결 로그 기록
        // log connection attempt
        Logger.Log(LogCategories.Connection, LogLevel.Info, $"Connecting: {target}:{option} SlaveId=0x{id:X2}");

        // 전송 계층 비동기 연결 시도
        // attempt async transport connection
        if (await _transport!.ConnectAsync(target, option, ct).ConfigureAwait(false))
            // 연결 성공 반환
            // return connection success
            return true;
        // 연결 실패 — 상태 복원
        // connection failed — restore state
        ConnectionState = Connection.Closed;
        // 연결 실패 로그 기록
        // log connection failure
        Logger.Log(LogCategories.Connection, LogLevel.Error, "Connection failed");
        // 연결 실패 반환
        // return connection failure
        return false;
    }

    /// <summary>
    ///     읽기 요청을 자동 분할하여 인큐한다 (125 레지스터 초과 시).
    ///     Auto-splits and enqueues read requests (when exceeding 125 registers).
    /// </summary>
    /// <param name="code">함수 코드 (ReadHoldingReg 또는 ReadInputReg) / function code</param>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">총 레지스터 개수 / total register count</param>
    /// <returns>모든 요청 인큐 성공 여부 / whether all requests were enqueued</returns>
    private bool EnqueueReadRequests(FunctionCode code, ushort addr, ushort count) {
        // 연결 상태 확인
        // check connection state
        if (ConnectionState is not Connection.Connected)
            // 미연결 — 거부
            // not connected — reject
            return false;

        // 가드: 레지스터 개수 유효성 확인
        // guard: validate register count
        if (count is 0)
            // 개수 0 — 거부
            // count is 0 — reject
            return false;

        // 남은 레지스터 수
        // remaining register count
        var remaining = (int)count;
        // 현재 주소 오프셋
        // current address offset
        var offset = 0;
        // 모든 요청 인큐 성공 여부
        // all requests enqueued successfully
        var success = true;

        // 남은 레지스터가 있는 동안 분할 전송
        // split and transmit while registers remain
        while (remaining > 0) {
            // 현재 청크 크기 (최대 125)
            // current chunk size (max 125)
            var chunk = (ushort)Math.Min(remaining, MaxReadRegisters);
            // 현재 청크 시작 주소
            // current chunk start address
            var currentAddr = (ushort)(addr + offset);

            // PRO X 모드: MODBUS 패스스루
            // PRO X mode: MODBUS passthrough
            if (IsProMode) {
                // MODBUS 패스스루로 분할 읽기 인큐
                // enqueue split read via MODBUS passthrough
                if (!EnqueueProModbusRequest(code, currentAddr, chunk))
                    // 인큐 실패 기록
                    // record enqueue failure
                    success = false;
            } else {
                // 트랜잭션 ID 발급
                // issue transaction ID
                var tId = NextTransactionId();

                // 함수 코드에 따라 패킷 생성
                // build packet based on function code
                var packet = code is FunctionCode.ReadHoldingReg
                    ? _codec!.BuildReadHoldingReg(currentAddr, chunk, _slaveId, tId)
                    : _codec!.BuildReadInputReg(currentAddr, chunk, _slaveId, tId);

                // 요청 생성 및 인큐
                // create request and enqueue
                var request = new ModbusRequest(code, currentAddr, packet);
                // 인큐 실패 시 전체 실패 표시
                // mark total failure if enqueue fails
                if (!_pipeline!.Enqueue(request))
                    // 인큐 실패 기록
                    // record enqueue failure
                    success = false;
            }

            // 남은 레지스터 수 감소
            // decrement remaining register count
            remaining -= chunk;
            // 주소 오프셋 증가
            // increment address offset
            offset += chunk;
        }

        // 마지막 활동 시각 갱신 (직접 모드)
        // update last activity time (direct mode)
        if (!IsProMode)
            // 마지막 활동 시각 갱신
            // update last activity time
            Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // 전체 성공 여부 반환
        // return overall success
        return success;
    }

    /// <summary>
    ///     PRO X MODBUS 패스스루 요청을 인큐한다.
    ///     Enqueues a PRO X MODBUS passthrough request.
    /// </summary>
    /// <param name="code">함수 코드 / function code</param>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="value">값 또는 카운트 / value or count</param>
    /// <returns>인큐 성공 여부 / whether the request was enqueued</returns>
    private bool EnqueueProModbusRequest(FunctionCode code, ushort addr, ushort value) {
        // MODBUS TCP 코덱으로 패킷 생성
        // build packet with MODBUS TCP codec
        var codec = new ModbusTcpCodec();

        // 함수 코드에 따라 MODBUS 패킷 생성
        // build MODBUS packet based on function code
        var modbusPacket = code switch {
            // 보유 레지스터 읽기
            // read holding registers
            FunctionCode.ReadHoldingReg => codec.BuildReadHoldingReg(addr, value, _slaveId),
            // 입력 레지스터 읽기
            // read input registers
            FunctionCode.ReadInputReg => codec.BuildReadInputReg(addr, value, _slaveId),
            // 단일 레지스터 쓰기
            // write single register
            FunctionCode.WriteSingleReg => codec.BuildWriteSingleReg(addr, value, _slaveId),
            // 그 외 — 빈 패킷
            // otherwise — empty packet
            _ => []
        };

        // 빈 패킷이면 거부
        // reject if empty packet
        if (modbusPacket.Length is 0)
            // 미지원 함수 코드 — 거부
            // unsupported function code — reject
            return false;

        // ProService를 통해 MODBUS 패스스루 인큐
        // enqueue MODBUS passthrough via ProService
        return Pro!.EnqueueModbusRequest(modbusPacket);
    }

    /// <summary>
    ///     전송 계층 연결 상태 변경 핸들러.
    ///     Transport connection state change handler.
    /// </summary>
    /// <param name="connected">연결 상태 / connection state</param>
    private void OnConnectionChanged(bool connected) {
        // 연결됨 처리
        // handle connected
        if (connected) {
            // PRO X 모드: ProService 시작
            // PRO X mode: start ProService
            if (IsProMode) {
                // ProService 시작 (멤버 툴 요청 자동 발행)
                // start ProService (auto-issues member tool request)
                Pro!.Start(_target);
                // 연결 로그 기록
                // log connection
                Logger.Log(LogCategories.Connection, LogLevel.Info, "Transport connected, requesting member tools");
                // 반환
                // return
                return;
            }

            // 직접 연결 모드: 파이프라인 시작 + 장치 정보 읽기
            // direct mode: start pipeline + read device info
            // 파이프라인 시작
            // start pipeline
            _pipeline!.Start();
            // 마지막 활동 시각 초기화
            // initialize last activity time
            Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
            // 장치 정보 읽기 (연결 확인용)
            // read device info (for connection confirmation)
            ReadInfoReg();
            // 연결 로그 기록
            // log connection
            Logger.Log(LogCategories.Connection, LogLevel.Info, "Transport connected, reading device info");
            // 반환
            // return
            return;
        }

        // 연결 해제 처리
        // handle disconnected
        if (IsProMode) {
            // ProService 정지
            // stop ProService
            Pro!.Stop();
        } else {
            // Keep-Alive 타이머 정지
            // stop keep-alive timer
            _keepAliveTimer?.Stop();
            // 파이프라인 정지
            // stop pipeline
            _pipeline?.Stop();
        }

        // 연결 상태를 Closed로 변경
        // set connection state to Closed
        ConnectionState = Connection.Closed;
        // 연결 해제 로그 기록
        // log disconnection
        Logger.Log(LogCategories.Connection, LogLevel.Info, "Disconnected");
        // 연결 상태 변경 이벤트 발생
        // raise connection state change event
        ChangedConnect?.Invoke(false);
    }

    /// <summary>
    ///     ProService 연결 상태 변경 핸들러 (PRO X 레벨).
    ///     ProService connection state change handler (PRO X level).
    /// </summary>
    /// <param name="connected">연결 상태 / connection state</param>
    private void OnProConnectionChanged(bool connected) {
        // 연결 상태에 따라 처리 분기
        // branch on connection state
        switch (connected) {
            // 연결 완료 처리 (멤버 툴 수신 후)
            // handle connection complete (after member tools received)
            case true when ConnectionState is Connection.Connecting:
                // 연결 완료 상태로 전환
                // transition to connected state
                ConnectionState = Connection.Connected;
                // 연결 완료 로그 기록
                // log connection complete
                Logger.Log(LogCategories.Connection, LogLevel.Info, $"Connected via PRO X: {Pro!.Tools.MemberTools.Count} member tools");
                // 연결 상태 변경 이벤트 발생
                // raise connection state change event
                ChangedConnect?.Invoke(true);
                // 연결 완료 케이스 종료
                // end of connection complete case
                break;
            // Keep-Alive 타임아웃으로 인한 연결 해제
            // disconnection due to Keep-Alive timeout
            case false:
                // 연결 해제 로그
                // log disconnection
                Logger.Log(LogCategories.Connection, LogLevel.Warning, "PRO X Keep-Alive timeout, closing connection");
                // 연결 해제
                // close connection
                Close();
                // 연결 해제 케이스 종료
                // end of disconnection case
                break;
        }
    }

    /// <summary>
    ///     ProService MODBUS 응답 수신 핸들러 (MID 111 언래핑).
    ///     ProService MODBUS response received handler (MID 111 unwrapped).
    /// </summary>
    /// <param name="response">MODBUS 응답 / MODBUS response</param>
    private void OnProModbusResponse(ModbusResponse response) {
        // 구독자에게 응답 전달
        // deliver response to subscribers
        ReceivedData?.Invoke(response);
    }

    /// <summary>
    ///     파이프라인 응답 수신 핸들러 (직접 연결 전용).
    ///     Pipeline response received handler (direct connection only).
    /// </summary>
    /// <param name="response">수신된 응답 / received response</param>
    private void OnResponseReceived(ModbusResponse response) {
        // 마지막 활동 시각 갱신
        // update last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);

        // 장치 정보 응답 처리 (FC 0x11)
        // handle device info response (FC 0x11)
        if (response.Code is FunctionCode.ReadInfoReg && response.Payload.Length >= SimpleInfo.Size) {
            // 장치 정보 파싱
            // parse device info
            if (SimpleInfo.TryParse(response.Payload.Span, out var info))
                // 파싱 성공 — 장치 정보 설정
                // parse success — set device info
                Info = info;

            // Connecting 상태에서 장치 정보 수신 시 Connected로 전환
            // transition to Connected when device info received during Connecting
            if (ConnectionState is Connection.Connecting) {
                // 연결 완료 상태로 전환
                // transition to connected state
                ConnectionState = Connection.Connected;
                // Keep-Alive가 활성화되어 있으면 타이머 시작 (사용자 설정 반영)
                // start keep-alive timer if enabled (apply user settings)
                if (Settings.KeepAlive.Enabled) {
                    // 타이머 주기를 사용자 설정값으로 적용
                    // apply timer interval from user settings
                    _keepAliveTimer!.Interval = Settings.KeepAlive.Period;
                    // Keep-Alive 타이머 시작
                    // start keep-alive timer
                    _keepAliveTimer.Start();
                }

                // 연결 완료 로그 기록
                // log connection complete
                Logger.Log(LogCategories.Connection, LogLevel.Info, $"Connected: Model={Info.Controller} FW={Info.Firmware} SN={Info.Serial}");
                // 연결 상태 변경 이벤트 발생
                // raise connection state change event
                ChangedConnect?.Invoke(true);
            }
        }

        // 구독자에게 응답 전달
        // deliver response to subscribers
        ReceivedData?.Invoke(response);
    }

    /// <summary>
    ///     파이프라인 오류 발생 핸들러 (직접 연결 전용).
    ///     Pipeline error occurred handler (direct connection only).
    /// </summary>
    /// <param name="error">통신 오류 / communication error</param>
    private void OnErrorOccurred(ComError error) {
        // 오류 로그 기록
        // log error
        Logger.Log(LogCategories.Error, LogLevel.Error, $"{error.Reason}: {error.Detail}");
        // 구독자에게 오류 전달
        // deliver error to subscribers
        ReceiveError?.Invoke(error);
    }

    /// <summary>
    ///     Keep-Alive 타이머 이벤트 핸들러 (직접 연결 전용).
    ///     Keep-Alive timer handler (direct connection only).
    /// </summary>
    private void OnKeepAliveElapsed(object? sender, ElapsedEventArgs e) {
        // 연결 상태 확인
        // check connection state
        if (ConnectionState is not Connection.Connected)
            // 미연결 — 반환
            // not connected — return
            return;

        // Keep-Alive 비활성화 확인
        // check if keep-alive is disabled
        if (!Settings.KeepAlive.Enabled) {
            // 타이머 정지
            // stop timer
            _keepAliveTimer?.Stop();
            // 비활성화 — 반환
            // disabled — return
            return;
        }

        // 마지막 활동 이후 경과 시간 계산
        // calculate elapsed time since last activity
        var elapsed = new TimeSpan(DateTime.UtcNow.Ticks - Volatile.Read(ref _lastActivityTicks)).TotalMilliseconds;

        // 경과 시간에 따라 Keep-Alive 동작 분기
        // branch keep-alive action based on elapsed time
        switch (elapsed) {
            // Keep-Alive 타임아웃 확인
            // check keep-alive timeout
            case var _ when elapsed > Settings.KeepAlive.Timeout:
                // 타임아웃 — 연결 해제
                // timeout — disconnect
                Logger.Log(LogCategories.KeepAlive, LogLevel.Warning, "Keep-Alive timeout, disconnecting");
                // 연결 해제
                // close connection
                Close();
                // 케이스 종료
                // end case
                break;
            // 유휴 상태이면 장치 정보 읽기
            // read device info if idle
            case var _ when elapsed >= Settings.KeepAlive.Period:
                // Keep-Alive 장치 정보 읽기
                // keep-alive device info read
                Logger.Log(LogCategories.KeepAlive, LogLevel.Debug, "Keep-Alive ping");
                // 장치 정보 읽기
                // read device info
                ReadInfoReg();
                // 케이스 종료
                // end case
                break;
        }
    }

    /// <summary>
    ///     다음 TCP 트랜잭션 ID를 발급한다.
    ///     Issues the next TCP transaction ID.
    /// </summary>
    /// <returns>트랜잭션 ID / transaction ID</returns>
    private ushort NextTransactionId() {
        // 트랜잭션 ID 증가 후 반환
        // increment and return transaction ID
        return _transactionId++;
    }
}