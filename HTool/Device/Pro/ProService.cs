using System.Timers;
using HTool.Core.Type.Pro;
using HTool.Core.Util;
using HTool.Device.Codec;
using HTool.Device.Protocol;
using HTool.Device.Transport;
using HTool.Format.Pro;
using HTool.Type;
using JobEvent = HTool.Format.Pro.JobEvent;
using Timer = System.Timers.Timer;
using ToolEvent = HTool.Format.Process.Event;

namespace HTool.Device.Pro;

/// <summary>
///     PRO X 게이트웨이 통신 관리 서비스.
///     PRO X gateway communication management service.
/// </summary>
/// <remarks>
///     <para>
///         MID 기반 메시지 프로토콜을 처리하며, MODBUS 패스스루(MID 110/111),
///         이벤트 구독/해제, Keep-Alive, 멀티 툴 관리를 담당한다.
///     </para>
///     <para>
///         Handles MID-based message protocol including MODBUS passthrough (MID 110/111),
///         event subscription/unsubscription, Keep-Alive, and multi-tool management.
///     </para>
/// </remarks>
public sealed class ProService : IDisposable {
    // 타이머 주기 (밀리초)
    // timer period (milliseconds)
    private const int TimerPeriod = 100;

    // 수신 링 버퍼 크기 (16KB)
    // receive ring buffer size (16KB)
    private const int RingBufferSize = 16 * 1024;

    // 로거 참조
    // logger reference
    private readonly HToolLogger _logger;

    // PRO X 메시지 큐 (키 기반 중복 방지)
    // PRO X message queue (key-based deduplication)
    private readonly KeyedQueue<ProRequest, ProRequest.RequestKey> _queue;

    // 수신 데이터 파싱용 링 버퍼
    // ring buffer for received data parsing
    private readonly RingBuffer _ringBuffer;
    // 인스턴스별 설정 참조
    // per-instance settings reference
    private readonly HToolSettings _settings;

    // 폴링 타이머
    // polling timer
    private readonly Timer _timer;

    // 전송 계층 참조
    // transport layer reference
    private readonly ITransport _transport;

    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;

    // Keep-Alive 미응답 카운터
    // keep-alive missed counter
    private int _keepAliveMissCount;

    // 마지막 활동 시각 — ticks 단위 (스레드 안전)
    // last activity time — in ticks (thread-safe)
    private long _lastActivityTicks;

    // 마지막 수신 시각 — ticks 단위 (스레드 안전)
    // last receive time — in ticks (thread-safe)
    private long _lastReceiveTimeTicks;

    // 타이머 콜백 재진입 방지 플래그
    // timer callback reentrancy guard flag
    private int _processing;

    /// <summary>
    ///     ProService를 생성한다.
    ///     Creates a ProService.
    /// </summary>
    /// <param name="transport">전송 계층 / transport layer</param>
    /// <param name="logger">로거 / logger</param>
    /// <param name="settings">인스턴스별 설정 / per-instance settings</param>
    internal ProService(ITransport transport, HToolLogger logger, HToolSettings settings) {
        // 전송 계층 설정
        // set transport layer
        _transport = transport;
        // 로거 설정
        // set logger
        _logger = logger;
        // 설정 참조 저장
        // store settings reference
        _settings = settings;

        // 툴 서비스 생성
        // create tool service
        Tools = new ToolService();

        // PRO X 메시지 큐 생성 (키 기반 중복 방지, 초기 용량 32)
        // create PRO X message queue (key-based deduplication, initial capacity 32)
        _queue = KeyedQueue<ProRequest, ProRequest.RequestKey>.Create(static r => r.Key, capacity: 32);

        // 수신 링 버퍼 생성
        // create receive ring buffer
        _ringBuffer = new RingBuffer(RingBufferSize);

        // 폴링 타이머 생성 (100ms 주기)
        // create polling timer (100ms period)
        _timer = new Timer(TimerPeriod) {
            // 반복 실행 활성화
            // enable auto-reset
            AutoReset = true
        };
        // 타이머 이벤트 핸들러 등록
        // register timer event handler
        _timer.Elapsed += OnTimerElapsed;

        // 수신 이벤트 구독
        // subscribe to receive event
        _transport.DataReceived += OnDataReceived;
    }

    /// <summary>
    ///     멀티 툴 상태 관리 서비스.
    ///     Multi-tool state management service.
    /// </summary>
    public ToolService Tools { get; }

    /// <summary>
    ///     FTP 파일 전송 서비스. 연결 후 생성되며, 연결 해제 시 null.
    ///     FTP file transfer service. Created after connection, null when disconnected.
    /// </summary>
    public FtpService? Ftp { get; private set; }

    /// <summary>
    ///     툴 이벤트 구독 상태.
    ///     Whether tool events are subscribed.
    /// </summary>
    public bool IsToolEventSubscribed { get; private set; }

    /// <summary>
    ///     작업 이벤트 구독 상태.
    ///     Whether job events are subscribed.
    /// </summary>
    public bool IsJobEventSubscribed { get; private set; }

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
        // 수신 이벤트 구독 해제
        // unsubscribe from receive event
        _transport.DataReceived -= OnDataReceived;
        // 타이머 정지
        // stop timer
        _timer.Stop();
        // 타이머 이벤트 핸들러 해제
        // unsubscribe timer event handler
        _timer.Elapsed -= OnTimerElapsed;
        // 타이머 해제
        // dispose timer
        _timer.Dispose();
        // FTP 서비스 해제
        // dispose FTP service
        Ftp?.Dispose();
        // FTP 서비스 참조 제거
        // clear FTP service reference
        Ftp = null;
        // 큐 해제
        // dispose queue
        _queue.Dispose();
    }

    #region 이벤트 / Events

    /// <summary>
    ///     처리되지 않은 PRO X 메시지 수신 이벤트 (소비자 파싱 대상 MID).
    ///     Unhandled PRO X message received event (consumer-parsed MIDs).
    /// </summary>
    public event Action<ProMessage>? MessageReceived;

    /// <summary>
    ///     툴 이벤트 데이터 수신 (MID 102/106). 자동 Ack 처리.
    ///     Tool event data received (MID 102/106). Auto-Ack handled.
    /// </summary>
    public event Action<ToolEvent>? EventDataReceived;

    /// <summary>
    ///     작업 이벤트 수신 (MID 88). 자동 Ack 처리.
    ///     Job event received (MID 88). Auto-Ack handled.
    /// </summary>
    public event Action<JobEvent>? JobEventReceived;

    /// <summary>
    ///     마지막 이벤트 ID 수신 (MID 108).
    ///     Last event ID received (MID 108).
    /// </summary>
    public event Action<int>? LastEventIdReceived;

    /// <summary>
    ///     멤버 툴 목록 변경 (MID 11).
    ///     Member tool list changed (MID 11).
    /// </summary>
    public event Action<IReadOnlyList<ProToolInfo>>? MemberToolsChanged;

    /// <summary>
    ///     스캔 툴 목록 변경 (MID 13).
    ///     Scan tool list changed (MID 13).
    /// </summary>
    public event Action<IReadOnlyList<ProToolInfo>>? ScanToolsChanged;

    /// <summary>
    ///     PRO X 명령 오류 수신 (MID 1).
    ///     PRO X command error received (MID 1).
    /// </summary>
    public event Action<MessageId, int>? ErrorReceived;

    /// <summary>
    ///     MODBUS 패스스루 응답 수신 (MID 111 언래핑).
    ///     MODBUS passthrough response received (MID 111 unwrapped).
    /// </summary>
    internal event Action<ModbusResponse>? ModbusResponseReceived;

    /// <summary>
    ///     연결 상태 변경 (PRO X 레벨).
    ///     Connection state changed (PRO X level).
    /// </summary>
    internal event Action<bool>? ConnectionChanged;

    #endregion

    #region 요청 API / Request API

    /// <summary>
    ///     PRO X 메시지를 요청한다.
    ///     Sends a PRO X message request.
    /// </summary>
    /// <param name="mid">메시지 ID / message ID</param>
    /// <param name="revision">프로토콜 리비전 / protocol revision</param>
    /// <param name="values">페이로드 데이터 (없으면 null) / payload data (null if none)</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool Request(MessageId mid, int revision = 0, byte[]? values = null) {
        // 프레임 생성
        // build frame
        var packet = ProCodec.BuildMessage(mid, revision, values ?? ReadOnlySpan<byte>.Empty);
        // 요청 생성
        // create request
        var request = new ProRequest(mid, revision, packet);
        // 인큐
        // enqueue
        return EnqueueRequest(request);
    }

    /// <summary>
    ///     툴 이벤트를 구독한다 (MID 100).
    ///     Subscribes to tool events (MID 100).
    /// </summary>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool SubscribeToolEvent() {
        // MID 100 전송
        // send MID 100
        return Request(MessageId.LastEventSubscribe);
    }

    /// <summary>
    ///     툴 이벤트 구독을 해제한다 (MID 104).
    ///     Unsubscribes from tool events (MID 104).
    /// </summary>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool UnsubscribeToolEvent() {
        // 구독 상태 해제
        // clear subscription state
        IsToolEventSubscribed = false;
        // MID 104 전송
        // send MID 104
        return Request(MessageId.LastEventUnsubscribe);
    }

    /// <summary>
    ///     작업 이벤트를 구독한다 (MID 87).
    ///     Subscribes to job events (MID 87).
    /// </summary>
    /// <param name="flushBuffer">버퍼 플러시 여부 (리비전 1) / whether to flush buffer (revision 1)</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool SubscribeJobEvent(bool flushBuffer = false) {
        // 리비전 결정 (기본 0, 버퍼 플러시 시 1)
        // determine revision (0 by default, 1 for buffer flush)
        var revision = flushBuffer is false ? 0 : 1;
        // MID 87 전송
        // send MID 87
        return Request(MessageId.JobEventSubscribe, revision);
    }

    /// <summary>
    ///     작업 이벤트 구독을 해제한다 (MID 90).
    ///     Unsubscribes from job events (MID 90).
    /// </summary>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool UnsubscribeJobEvent() {
        // 구독 상태 해제
        // clear subscription state
        IsJobEventSubscribed = false;
        // MID 90 전송
        // send MID 90
        return Request(MessageId.JobEventUnsubscribe);
    }

    /// <summary>
    ///     마지막 이벤트 ID를 요청한다 (MID 107).
    ///     Requests the last event ID (MID 107).
    /// </summary>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool RequestLastEventId() {
        // MID 107 전송
        // send MID 107
        return Request(MessageId.LastEventIdRequest);
    }

    /// <summary>
    ///     과거 이벤트를 요청한다 (MID 105).
    ///     Requests a historical event (MID 105).
    /// </summary>
    /// <param name="eventId">이벤트 ID / event ID</param>
    /// <returns>요청 인큐 성공 여부 / whether the request was enqueued</returns>
    public bool RequestOldEvent(int eventId) {
        // 이벤트 ID를 4바이트 Big-Endian으로 변환
        // convert event ID to 4-byte Big-Endian
        var payload = new byte[4];
        // 이벤트 ID 기록
        // write event ID
        Utils.WriteInt32(payload, eventId);
        // MID 105 전송
        // send MID 105
        return Request(MessageId.OldEventRequest, 0, payload);
    }

    /// <summary>
    ///     MODBUS 패스스루 요청을 인큐한다 (MID 110).
    ///     Enqueues a MODBUS passthrough request (MID 110).
    /// </summary>
    /// <param name="modbusPacket">MODBUS TCP 프레임 / MODBUS TCP frame</param>
    /// <returns>인큐 성공 여부 / whether the request was enqueued</returns>
    internal bool EnqueueModbusRequest(ReadOnlyMemory<byte> modbusPacket) {
        // 선택된 툴로 작업 시작 시도 (원자적 유효성 확인 + 잠금 획득)
        // attempt to begin operation for selected tool (atomic validity check + lock acquisition)
        var toolId = Tools.TryBeginOperation();
        // 유효한 툴이 없거나 이미 작업 중이면 거부
        // reject if no valid tool selected or already busy
        if (toolId < 0)
            // no valid tool or operation already active — reject
            // 유효한 툴 없음 또는 작업 중 — 거부
            return false;

        // guard: 작업 잠금이 항상 해제되도록 보장
        // guard: ensure operation lock is always released
        try {
            // 캡처된 툴 ID를 Unit ID로 사용하여 MID 110 프레임 생성
            // build MID 110 frame using captured tool ID as Unit ID
            var packet = ProCodec.BuildModbusPassthrough(modbusPacket.Span, (byte)toolId);
            // 요청 생성
            // create request
            var request = new ProRequest(MessageId.ModbusRequest, 0, packet);
            // 인큐
            // enqueue
            return EnqueueRequest(request);
        } finally {
            // 작업 완료 처리 (잠금 해제)
            // mark operation complete (release lock)
            Tools.EndOperation();
        }
    }

    #endregion

    #region 수명주기 / Lifecycle

    /// <summary>
    ///     ProService를 시작한다 (타이머 가동, FTP 서비스 생성, 멤버 툴 요청).
    ///     Starts the ProService (activates timer, creates FTP service, requests member tools).
    /// </summary>
    /// <param name="host">PRO X 게이트웨이 IP 주소 / PRO X gateway IP address</param>
    internal void Start(string host) {
        // 마지막 수신 시각 초기화
        // initialize last receive time
        Volatile.Write(ref _lastReceiveTimeTicks, DateTime.UtcNow.Ticks);
        // 마지막 활동 시각 초기화
        // initialize last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // Keep-Alive 미응답 카운터 초기화
        // reset keep-alive missed counter
        _keepAliveMissCount = 0;
        // FTP 서비스 생성 (Lazy 연결 — 첫 작업 시 자동 연결)
        // create FTP service (lazy connection — auto-connects on first operation)
        Ftp = new FtpService(_logger, host, FtpService.DefaultPort);
        // 타이머 시작
        // start timer
        _timer.Start();
        // 로그 기록
        // log service start
        _logger.Log(LogCategories.Pro, LogLevel.Info, "ProService started");

        // 멤버 툴 목록 요청 (MID 10)
        // request member tool list (MID 10)
        Request(MessageId.MemberToolRequest);
    }

    /// <summary>
    ///     ProService를 정지한다 (구독 해제, 타이머 정지, FTP 해제, 큐·버퍼 비우기).
    ///     Stops the ProService (unsubscribe, stop timer, FTP cleanup, clear queue and buffer).
    /// </summary>
    internal void Stop() {
        // 타이머 정지
        // stop timer
        _timer.Stop();

        // 구독 상태 초기화
        // clear subscription states
        IsToolEventSubscribed = false;
        // 작업 이벤트 구독 상태 초기화
        // clear job event subscription state
        IsJobEventSubscribed = false;
        // FTP 서비스 해제
        // dispose FTP service
        Ftp?.Dispose();
        // FTP 서비스 참조 제거
        // clear FTP service reference
        Ftp = null;
        // 큐 비우기
        // clear queue
        _queue.Clear();
        // 링 버퍼 비우기
        // clear ring buffer
        _ringBuffer.Clear();
        // 툴 서비스 초기화
        // clear tool service
        Tools.Clear();
        // 로그 기록
        // log service stop
        _logger.Log(LogCategories.Pro, LogLevel.Info, "ProService stopped");
    }

    #endregion

    #region 내부 / Internal

    /// <summary>
    ///     요청을 큐에 추가한다.
    ///     Enqueues a request.
    /// </summary>
    /// <param name="request">PRO X 요청 / PRO X request</param>
    /// <returns>추가 성공 여부 / whether the request was enqueued</returns>
    private bool EnqueueRequest(ProRequest request) {
        // 큐에 중복 방지 모드로 추가
        // enqueue with uniqueness enforcement
        var result = _queue.TryEnqueue(request);
        // 추가 성공 시 로그 기록
        // log if enqueue succeeded
        if (result)
            // 인큐 로그
            // log enqueue
            _logger.Log(LogCategories.Pro, LogLevel.Debug, $"Enqueued: MID={request.Mid}({(int)request.Mid}) Rev={request.Revision}");
        // 결과 반환
        // return result
        return result;
    }

    /// <summary>
    ///     수신 데이터 이벤트 핸들러.
    ///     Receive data event handler.
    /// </summary>
    /// <param name="data">수신 데이터 / received data</param>
    private void OnDataReceived(ReadOnlyMemory<byte> data) {
        // 수신 패킷 로그 기록
        // log received packet
        _logger.LogPacket("PRX-RX", data.Span);
        // 링 버퍼에 수신 데이터 기록
        // write received data to ring buffer
        _ringBuffer.WriteBytes(data.Span);
        // 마지막 수신 시각 갱신
        // update last receive time
        Volatile.Write(ref _lastReceiveTimeTicks, DateTime.UtcNow.Ticks);
    }

    /// <summary>
    ///     타이머 이벤트 핸들러. 프레임 파싱, 요청 전송, Keep-Alive를 순차 처리한다.
    ///     Timer event handler. Processes frame parsing, request transmission, and Keep-Alive sequentially.
    /// </summary>
    private void OnTimerElapsed(object? sender, ElapsedEventArgs e) {
        // 재진입 방지
        // reentrancy guard
        if (Interlocked.CompareExchange(ref _processing, 1, 0) is not 0)
            // 이전 처리 진행 중 — 건너뜀
            // previous processing in progress — skip
            return;

        // 가드: 타이머 콜백 오류 처리
        // guard: handle timer callback errors
        try {
            // 수신 프레임 처리
            // process received frames
            ProcessReceivedFrames();
            // 요청 전송/타임아웃 처리
            // process request transmission/timeout
            ProcessQueue();
            // Keep-Alive 처리
            // process Keep-Alive
            ProcessKeepAlive();
        } finally {
            // 처리 플래그 해제
            // release processing flag
            Interlocked.Exchange(ref _processing, 0);
        }
    }

    /// <summary>
    ///     링 버퍼에서 완전한 PRO X 프레임을 추출하고 처리한다.
    ///     Extracts and processes complete PRO X frames from the ring buffer.
    /// </summary>
    private void ProcessReceivedFrames() {
        // 프레임 타임아웃 확인 (불완전 프레임 정리)
        // check frame timeout (clean up incomplete frames)
        if (_ringBuffer.Available > 0) {
            // 마지막 수신 이후 경과 시간 계산
            // calculate elapsed time since last receive
            var elapsed = new TimeSpan(DateTime.UtcNow.Ticks - Volatile.Read(ref _lastReceiveTimeTicks)).TotalMilliseconds;
            // 프레임 타임아웃 초과 확인
            // check if frame timeout exceeded
            if (elapsed > _settings.Pro.FrameTimeout) {
                // 불완전 프레임 로그
                // log incomplete frame
                _logger.Log(LogCategories.Pro, LogLevel.Warning, $"Frame timeout: {_ringBuffer.Available} bytes discarded");
                // 링 버퍼 비우기
                // clear ring buffer
                _ringBuffer.Clear();
                // 반환
                // return
                return;
            }
        }

        // 완전한 프레임이 있는 동안 반복
        // loop while complete frames are available
        while (_ringBuffer.Available > 0) {
            // PRO X 프레임 길이 계산
            // calculate PRO X frame length
            var frameLength = ProCodec.CalcFrameLength(_ringBuffer);
            // 데이터 부족 또는 무효 확인
            // check if insufficient data or invalid
            if (frameLength < 0)
                // 프레임 미완성 — 다음 수신 대기
                // frame incomplete — wait for next receive
                break;

            // 링 버퍼에 충분한 데이터가 있는지 확인
            // check if ring buffer has enough data
            if (_ringBuffer.Available < frameLength)
                // 프레임 미완성 — 다음 수신 대기
                // frame incomplete — wait for next receive
                break;

            // 프레임 데이터 추출
            // extract frame data
            var frame = _ringBuffer.ReadBytes(frameLength);
            // MID 추출
            // extract MID
            var mid = ProCodec.ExtractMid(frame);
            // 리비전 추출
            // extract revision
            var revision = ProCodec.ExtractRevision(frame);
            // 페이로드 추출
            // extract payload
            var payload = ProCodec.ExtractPayload(frame);

            // PRO X 메시지 로그 기록
            // log PRO X message
            _logger.Log(LogCategories.Pro, LogLevel.Debug, $"Received: MID={mid}({(int)mid}) Rev={revision} Len={payload.Length}");

            // 수신 프레임 처리
            // process received frame
            ProcessReceivedMessage(mid, revision, payload);
        }
    }

    /// <summary>
    ///     수신된 PRO X 메시지를 MID에 따라 분기 처리한다.
    ///     Routes a received PRO X message based on MID.
    /// </summary>
    /// <param name="mid">메시지 ID / message ID</param>
    /// <param name="revision">리비전 / revision</param>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    private void ProcessReceivedMessage(MessageId mid, int revision, byte[] payload) {
        // 마지막 활동 시각 갱신
        // update last activity time
        Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
        // Keep-Alive 미응답 카운터 초기화
        // reset keep-alive missed counter
        _keepAliveMissCount = 0;

        // MID별 분기 처리
        // route by MID
        switch (mid) {
            // 명령 수락 (MID 0)
            // command accepted (MID 0)
            case MessageId.CommandAccepted:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 케이스 종료
                // end case
                break;

            // 명령 오류 (MID 1)
            // command error (MID 1)
            case MessageId.CommandError:
                // 오류 로그 기록
                // log error
                _logger.Log(LogCategories.Pro, LogLevel.Error, $"Command error: Rev={revision}");
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 오류 이벤트 발생
                // raise error event
                ErrorReceived?.Invoke(mid, revision);
                // 케이스 종료
                // end case
                break;

            // Keep-Alive (MID 2)
            // Keep-Alive (MID 2)
            case MessageId.KeepAlive:
                // Keep-Alive 응답 수신 로그
                // log Keep-Alive response
                _logger.Log(LogCategories.Pro, LogLevel.Debug, "Keep-Alive response");
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 케이스 종료
                // end case
                break;

            // 멤버 툴 응답 (MID 11)
            // member tool reply (MID 11)
            case MessageId.MemberToolReply:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 멤버 툴 목록 처리
                // handle member tool list
                HandleMemberToolReply(payload);
                // 케이스 종료
                // end case
                break;

            // 스캔 툴 응답 (MID 13)
            // scan tool reply (MID 13)
            case MessageId.ScanToolReply:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 스캔 툴 목록 처리
                // handle scan tool list
                HandleScanToolReply(payload);
                // 케이스 종료
                // end case
                break;

            // 작업 이벤트 알림 (MID 88)
            // job event notification (MID 88)
            case MessageId.JobEvent:
                // 작업 이벤트 처리
                // handle job event
                HandleJobEvent(payload);
                // 케이스 종료
                // end case
                break;

            // 툴 이벤트 알림 (MID 102)
            // tool event notification (MID 102)
            case MessageId.LastEvent:
                // 툴 이벤트 처리 (리비전 전달)
                // handle tool event (pass revision)
                HandleToolEvent(mid, revision, payload);
                // 케이스 종료
                // end case
                break;

            // 과거 이벤트 응답 (MID 106)
            // old event reply (MID 106)
            case MessageId.OldEventReply:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 과거 이벤트 처리 (리비전 전달)
                // handle old event reply (pass revision)
                HandleToolEvent(mid, revision, payload);
                // 케이스 종료
                // end case
                break;

            // 마지막 이벤트 ID 응답 (MID 108)
            // last event ID reply (MID 108)
            case MessageId.LastEventIdReply:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 이벤트 ID 추출 (4바이트 Big-Endian)
                // extract event ID (4 bytes Big-Endian)
                if (payload.Length >= 4) {
                    // 이벤트 ID 읽기
                    // read event ID
                    var eventId = Utils.ReadInt32(payload);
                    // 이벤트 ID 알림
                    // notify event ID
                    LastEventIdReceived?.Invoke(eventId);
                }

                // 케이스 종료
                // end case
                break;

            // MODBUS 패스스루 응답 (MID 111)
            // MODBUS passthrough reply (MID 111)
            case MessageId.ModbusReply:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // MODBUS 응답 처리
                // handle MODBUS response
                HandleModbusReply(payload);
                // 케이스 종료
                // end case
                break;

            // 기타 응답 — 소비자에게 전달
            // other reply — deliver to consumer
            default:
                // 현재 요청 완료 처리
                // complete current request
                DequeueCurrentRequest();
                // 소비자 메시지 이벤트 발생
                // raise consumer message event
                MessageReceived?.Invoke(new ProMessage(mid, revision, payload));
                // 케이스 종료
                // end case
                break;
        }
    }

    /// <summary>
    ///     큐의 선두 요청을 전송하거나, 활성 요청의 타임아웃/재시도를 처리한다.
    ///     Transmits the front request in the queue, or handles timeout/retry for active requests.
    /// </summary>
    private void ProcessQueue() {
        // 큐 선두 항목 확인
        // check front item in queue
        if (!_queue.TryPeek(out var request))
            // 큐가 비어 있음 — 반환
            // queue is empty — return
            return;

        // 활성화되지 않은 요청이면 전송
        // transmit if request is not activated
        if (!request.Activated) {
            // 요청 활성화
            // activate request
            request.Activate();
            // 전송 패킷 로그 기록
            // log transmitted packet
            _logger.LogPacket("PRX-TX", request.Packet.Span);
            // 전송 계층을 통해 전송
            // transmit via transport layer
            if (_transport.Write(request.Packet.Span)) {
                // 응답 불필요 메시지면 즉시 큐에서 제거
                // dequeue immediately if no reply expected
                if (!request.ExpectReply)
                    // 큐에서 제거
                    // dequeue
                    _queue.TryDequeue(out _);
                // 마지막 활동 시각 갱신
                // update last activity time
                Volatile.Write(ref _lastActivityTicks, DateTime.UtcNow.Ticks);
                // 반환
                // return
                return;
            }

            // 전송 실패 — 큐에서 제거
            // transmission failed — remove from queue
            _queue.TryDequeue(out _);
            // 반환
            // return
            return;
        }

        // 활성 요청의 타임아웃 확인
        // check active request timeout
        var elapsed = (DateTime.UtcNow - request.ActiveTime).TotalMilliseconds;
        // 타임아웃 미초과 시 반환
        // return if timeout not exceeded
        if (elapsed < _settings.Pipeline.MessageTimeout)
            // 아직 타임아웃 전 — 반환
            // not timed out yet — return
            return;

        // 요청 비활성화 및 재시도 횟수 증가
        // deactivate request and increment retry count
        var retry = request.Deactivate();
        // 최대 재시도 초과 확인
        // check if max retries exceeded
        if (retry > _settings.Pipeline.MessageRetry) {
            // 최대 재시도 초과 — 큐에서 제거
            // max retries exceeded — remove from queue
            _queue.TryDequeue(out _);
            // 타임아웃 로그 기록
            // log timeout
            _logger.Log(LogCategories.Pro, LogLevel.Warning, $"Timeout: MID={request.Mid}({(int)request.Mid}) Retry={retry - 1}");
            // 반환
            // return
            return;
        }

        // 재시도 로그 기록
        // log retry
        _logger.Log(LogCategories.Pro, LogLevel.Debug, $"Retry {retry}: MID={request.Mid}({(int)request.Mid})");
    }

    /// <summary>
    ///     Keep-Alive를 처리한다. 유휴 시 MID 2를 전송하고, 타임아웃 시 연결을 해제한다.
    ///     Processes Keep-Alive. Sends MID 2 when idle, disconnects on timeout.
    /// </summary>
    private void ProcessKeepAlive() {
        // 마지막 활동 이후 경과 시간 계산
        // calculate elapsed time since last activity
        var elapsed = new TimeSpan(DateTime.UtcNow.Ticks - Volatile.Read(ref _lastActivityTicks)).TotalMilliseconds;

        // Keep-Alive 타임아웃 확인 (미응답 횟수 또는 절대 타임아웃)
        // check keep-alive timeout (miss count or absolute timeout)
        if (_keepAliveMissCount >= _settings.Pro.KeepAliveMissLimit || elapsed >= _settings.Pro.KeepAliveTimeout) {
            // 최대 미응답 초과 — 연결 해제
            // max misses exceeded — disconnect
            _logger.Log(LogCategories.Pro, LogLevel.Warning, $"Keep-Alive timeout ({_keepAliveMissCount} misses), disconnecting");
            // 연결 해제 이벤트 발생
            // raise connection change event
            ConnectionChanged?.Invoke(false);
            // 반환
            // return
            return;
        }

        // 유휴 상태 확인 (Keep-Alive 주기 초과)
        // check idle state (keep-alive period exceeded)
        if (elapsed < _settings.Pro.KeepAlivePeriod)
            // keep-alive 주기 미초과 — 반환
            // keep-alive period not yet reached — return
            return;
        // 큐에 대기 중인 요청이 있으면 Keep-Alive 불필요
        // skip keep-alive if there are pending requests in queue
        if (_queue.Count > 0)
            // 대기 중 요청 있음 — 반환
            // pending requests exist — return
            return;
        // Keep-Alive 로그
        // log Keep-Alive
        _logger.Log(LogCategories.Pro, LogLevel.Debug, "Keep-Alive ping");
        // Keep-Alive 미응답 카운터 증가
        // increment keep-alive missed counter
        _keepAliveMissCount++;
        // MID 2 전송
        // send MID 2
        var packet = ProCodec.BuildMessage(MessageId.KeepAlive);
        // Keep-Alive 요청 생성 (응답 대기)
        // create Keep-Alive request (expect reply)
        var request = new ProRequest(MessageId.KeepAlive, 0, packet);
        // 인큐
        // enqueue
        _queue.TryEnqueue(request);
    }

    /// <summary>
    ///     큐의 현재 활성 요청을 완료 처리하여 제거한다.
    ///     Completes and removes the current active request from the queue.
    /// </summary>
    private void DequeueCurrentRequest() {
        // 큐 선두 항목 확인
        // check front item in queue
        if (!_queue.TryPeek(out var request))
            // 큐가 비어 있음 — 반환
            // queue is empty — return
            return;
        // 활성화된 요청이면 제거
        // dequeue if activated
        if (request.Activated)
            // 큐에서 제거
            // dequeue
            _queue.TryDequeue(out _);
    }

    #endregion

    #region 메시지 핸들러 / Message Handlers

    /// <summary>
    ///     멤버 툴 응답을 처리한다 (MID 11).
    ///     Handles member tool reply (MID 11).
    /// </summary>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    private void HandleMemberToolReply(byte[] payload) {
        // 페이로드에서 멤버 툴 목록 파싱
        // parse member tool list from payload
        var tools = ParseToolList(payload, ProToolInfo.MemberSize);
        // 툴 서비스 갱신
        // update tool service
        if (Tools.UpdateMemberTools(tools)) {
            // 멤버 툴 목록 변경 로그
            // log member tool list change
            _logger.Log(LogCategories.Pro, LogLevel.Info, $"Member tools updated: {tools.Count} tools");
            // 이벤트 발생
            // raise event
            MemberToolsChanged?.Invoke(tools);
        }

        // 연결 완료 알림 (최초 멤버 툴 수신 시)
        // notify connection complete (on first member tool reception)
        ConnectionChanged?.Invoke(true);
    }

    /// <summary>
    ///     스캔 툴 응답을 처리한다 (MID 13).
    ///     Handles scan tool reply (MID 13).
    /// </summary>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    private void HandleScanToolReply(byte[] payload) {
        // 페이로드에서 스캔 툴 목록 파싱
        // parse scan tool list from payload
        var tools = ParseToolList(payload, ProToolInfo.ScanSize);
        // 툴 서비스 갱신
        // update tool service
        if (!Tools.UpdateScanTools(tools))
            // 변경 없음 — 반환
            // no changes — return
            return;
        // 스캔 툴 목록 변경 로그
        // log scan tool list change
        _logger.Log(LogCategories.Pro, LogLevel.Info, $"Scan tools updated: {tools.Count} tools");
        // 이벤트 발생
        // raise event
        ScanToolsChanged?.Invoke(tools);
    }

    /// <summary>
    ///     작업 이벤트를 처리한다 (MID 88). 자동 Ack(MID 89) 전송.
    ///     Handles job event (MID 88). Auto-sends Ack (MID 89).
    /// </summary>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    private void HandleJobEvent(byte[] payload) {
        // 구독 상태 설정
        // set subscription state
        IsJobEventSubscribed = true;
        // 작업 이벤트 파싱
        // parse job event
        if (!JobEvent.TryParse(payload, out var jobEvent) || jobEvent is null)
            // 파싱 실패 또는 null — 반환
            // parse failed or null — return
            return;
        // 작업 이벤트 로그
        // log job event
        _logger.Log(LogCategories.Pro, LogLevel.Info, $"Job event: {jobEvent.EventType} Job={jobEvent.JobName} Step={jobEvent.StepName}");
        // 자동 Ack 전송 (MID 89, fire-and-forget)
        // auto-send Ack (MID 89, fire-and-forget)
        SendAck(MessageId.JobEventAcknowledge);
        // 이벤트 발생
        // raise event
        JobEventReceived?.Invoke(jobEvent);
    }

    /// <summary>
    ///     툴 이벤트를 처리한다 (MID 102/106). MID 102는 자동 Ack(MID 103) 전송.
    ///     Handles tool event (MID 102/106). MID 102 auto-sends Ack (MID 103).
    /// </summary>
    /// <param name="mid">수신된 MID (102 또는 106) / received MID (102 or 106)</param>
    /// <param name="revision">PRO X 메시지 리비전 / PRO X message revision</param>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    private void HandleToolEvent(MessageId mid, int revision, byte[] payload) {
        // MID 102이면 구독 상태 설정
        // set subscription state if MID 102
        if (mid is MessageId.LastEvent)
            // 구독 상태 활성화
            // activate subscription state
            IsToolEventSubscribed = true;

        // 이벤트 데이터 파싱
        // parse event data
        if (!ToolEvent.TryParse(payload, out var eventData, revision) || eventData is null)
            // 파싱 실패 또는 null — 반환
            // parse failed or null — return
            return;
        // 이벤트 로그
        // log event
        _logger.Log(LogCategories.Pro, LogLevel.Info, $"Event: MID={mid}({(int)mid}) Id={eventData.Id}");
        // MID 102이면 자동 Ack 전송 (MID 103)
        // auto-send Ack (MID 103) if MID 102
        if (mid is MessageId.LastEvent)
            // Ack 전송 (이벤트 ID 포함)
            // send Ack (with event ID)
            SendEventAck(eventData.Id);
        // 이벤트 발생
        // raise event
        EventDataReceived?.Invoke(eventData);
    }

    /// <summary>
    ///     MODBUS 패스스루 응답을 처리한다 (MID 111).
    ///     Handles MODBUS passthrough reply (MID 111).
    /// </summary>
    /// <param name="payload">PRO X 페이로드 / PRO X payload</param>
    private void HandleModbusReply(byte[] payload) {
        // MODBUS 페이로드 추출 (MBAP 헤더 제외)
        // extract MODBUS payload (excluding MBAP header)
        var modbusPayload = ProCodec.ExtractModbusPayload(payload);
        // 유효한 MODBUS 페이로드 확인
        // check for valid MODBUS payload
        if (modbusPayload.IsEmpty)
            // 빈 MODBUS 페이로드 — 무시
            // empty MODBUS payload — ignore
            return;

        // MODBUS TCP 프레임에서 함수 코드 추출
        // extract function code from MODBUS TCP frame
        var rawFc = modbusPayload.Span[0];
        // 오류 응답 여부 확인
        // check for error response
        var code = (rawFc & 0x80) is not 0
            ? FunctionCode.Error
            : (FunctionCode)rawFc;

        // FC 이후 데이터를 페이로드로 추출
        // extract data after FC as payload
        var responsePayload = modbusPayload.Length > 1
            ? modbusPayload[1..]
            : ReadOnlyMemory<byte>.Empty;

        // MODBUS 응답 생성
        // create MODBUS response
        var response = new ModbusResponse(code, 0, responsePayload);
        // MODBUS 응답 로그
        // log MODBUS response
        _logger.Log(LogCategories.Pro, LogLevel.Debug, $"MODBUS reply: FC=0x{rawFc:X2} Len={responsePayload.Length}");
        // 응답 이벤트 발생
        // raise response event
        ModbusResponseReceived?.Invoke(response);
    }

    /// <summary>
    ///     Ack 메시지를 전송한다 (fire-and-forget).
    ///     Sends an Ack message (fire-and-forget).
    /// </summary>
    /// <param name="ackMid">Ack MID / Ack MID</param>
    private void SendAck(MessageId ackMid) {
        // Ack 프레임 생성
        // build Ack frame
        var packet = ProCodec.BuildMessage(ackMid);
        // 직접 전송 (큐 우회)
        // transmit directly (bypass queue)
        _transport.Write(packet);
        // Ack 로그
        // log Ack
        _logger.Log(LogCategories.Pro, LogLevel.Debug, $"Ack sent: MID={ackMid}({(int)ackMid})");
    }

    /// <summary>
    ///     이벤트 Ack를 전송한다 (MID 103, 이벤트 ID 포함).
    ///     Sends an event Ack (MID 103, with event ID).
    /// </summary>
    /// <param name="eventId">이벤트 ID / event ID</param>
    private void SendEventAck(uint eventId) {
        // 이벤트 ID를 4바이트 Big-Endian으로 변환
        // convert event ID to 4-byte Big-Endian
        var payload = new byte[4];
        // 이벤트 ID 기록
        // write event ID
        Utils.WriteInt32(payload, (int)eventId);
        // MID 103 프레임 생성
        // build MID 103 frame
        var packet = ProCodec.BuildMessage(MessageId.LastEventAcknowledge, 0, payload);
        // 직접 전송 (큐 우회)
        // transmit directly (bypass queue)
        _transport.Write(packet);
        // Ack 로그
        // log Ack
        _logger.Log(LogCategories.Pro, LogLevel.Debug, $"Event Ack sent: MID=103 EventId={eventId}");
    }

    /// <summary>
    ///     페이로드에서 ProToolInfo 목록을 파싱한다.
    ///     Parses a list of ProToolInfo from the payload.
    /// </summary>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    /// <param name="itemSize">개별 항목 크기 (바이트) / individual item size (bytes)</param>
    /// <returns>파싱된 ProToolInfo 목록 / parsed ProToolInfo list</returns>
    private static List<ProToolInfo> ParseToolList(byte[] payload, int itemSize) {
        // 결과 목록 생성
        // create result list
        var tools = new List<ProToolInfo>();
        // 오프셋 위치
        // offset position
        var offset = 0;

        // 페이로드에서 항목 단위로 파싱
        // parse items from payload
        while (offset + itemSize <= payload.Length) {
            // 현재 항목 슬라이스 추출
            // extract current item slice
            var slice = payload.AsSpan(offset, itemSize);
            // 툴 정보 파싱 시도
            // attempt to parse tool info
            if (ProToolInfo.TryParse(slice, out var info))
                // 파싱 성공 — 목록에 추가
                // parse success — add to list
                tools.Add(info);
            // 다음 항목으로 이동
            // move to next item
            offset += itemSize;
        }

        // 파싱된 목록 반환
        // return parsed list
        return tools;
    }

    #endregion
}