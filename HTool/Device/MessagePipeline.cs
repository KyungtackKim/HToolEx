using System.Timers;
using HTool.Core.Util;
using HTool.Device.Codec;
using HTool.Device.Protocol;
using HTool.Device.Transport;
using HTool.Type;
using Timer = System.Timers.Timer;

namespace HTool.Device;

/// <summary>
///     MODBUS 메시지 파이프라인. 타이머 기반으로 큐의 요청을 순차 전송하고, 수신 프레임을 파싱하여 응답을 매칭한다.
///     MODBUS message pipeline. Transmits queued requests sequentially on a timer tick,
///     parses received frames, and matches responses to requests.
/// </summary>
/// <remarks>
///     <para>
///         MODBUS는 순차 프로토콜이므로 async/await 병렬화는 이점이 없다.
///         50ms 타이머 주기로 폴링하며, 응답 타임아웃·재시도를 관리한다.
///     </para>
///     <para>
///         MODBUS is a sequential protocol, so async/await parallelism provides no benefit.
///         Polls on a 50ms timer period, managing response timeout and retry.
///     </para>
/// </remarks>
internal sealed class MessagePipeline : IDisposable {
    // 타이머 주기 (밀리초)
    // timer period (milliseconds)
    private const int TimerPeriod = 50;

    // 수신 링 버퍼 크기 (16KB)
    // receive ring buffer size (16KB)
    private const int RingBufferSize = 16 * 1024;
    // 코덱 참조
    // codec reference
    private readonly IModbusCodec _codec;
    // 로거 참조
    // logger reference
    private readonly HToolLogger _logger;

    // 메시지 큐 (키 기반 중복 방지)
    // message queue (key-based deduplication)
    private readonly KeyedQueue<ModbusRequest, ModbusRequest.RequestKey> _queue;
    // 수신 데이터 파싱용 링 버퍼
    // ring buffer for received data parsing
    private readonly RingBuffer _ringBuffer;
    // 인스턴스별 설정 참조
    // per-instance settings reference
    private readonly HToolSettings _settings;
    // 타이머 인스턴스
    // timer instance
    private readonly Timer _timer;

    // 전송 계층 참조
    // transport layer reference
    private readonly ITransport _transport;
    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;
    // 마지막 수신 시각 — ticks 단위 (스레드 안전)
    // last receive time — in ticks (thread-safe)
    private long _lastReceiveTimeTicks;
    // 타이머 콜백 재진입 방지 플래그
    // timer callback reentrancy guard flag
    private int _processing;

    /// <summary>
    ///     메시지 파이프라인을 생성한다.
    ///     Creates a message pipeline.
    /// </summary>
    /// <param name="transport">전송 계층 / transport layer</param>
    /// <param name="codec">MODBUS 코덱 / MODBUS codec</param>
    /// <param name="logger">로거 / logger</param>
    /// <param name="settings">인스턴스별 설정 / per-instance settings</param>
    internal MessagePipeline(ITransport transport, IModbusCodec codec, HToolLogger logger, HToolSettings settings) {
        // 전송 계층 설정
        // set transport layer
        _transport = transport;
        // 코덱 설정
        // set codec
        _codec = codec;
        // 로거 설정
        // set logger
        _logger = logger;
        // 설정 참조 저장
        // store settings reference
        _settings = settings;

        // 메시지 큐 생성 (요청 키로 중복 방지, 초기 용량 64)
        // create message queue (deduplication by request key, initial capacity 64)
        _queue = KeyedQueue<ModbusRequest, ModbusRequest.RequestKey>.Create(static r => r.Key, capacity: 64);

        // 수신 링 버퍼 생성
        // create receive ring buffer
        _ringBuffer = new RingBuffer(RingBufferSize);

        // 폴링 타이머 생성 (50ms 주기)
        // create polling timer (50ms period)
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
        // 큐 해제
        // dispose queue
        _queue.Dispose();
    }

    /// <summary>
    ///     MODBUS 응답 수신 시 발생한다.
    ///     Raised when a MODBUS response is received.
    /// </summary>
    internal event Action<ModbusResponse>? ResponseReceived;

    /// <summary>
    ///     통신 오류 발생 시 발생한다.
    ///     Raised when a communication error occurs.
    /// </summary>
    internal event Action<ComError>? ErrorOccurred;

    /// <summary>
    ///     파이프라인을 시작한다 (타이머 가동).
    ///     Starts the pipeline (activates the timer).
    /// </summary>
    internal void Start() {
        // 마지막 수신 시각 초기화
        // initialize last receive time
        Volatile.Write(ref _lastReceiveTimeTicks, DateTime.UtcNow.Ticks);
        // 타이머 시작
        // start timer
        _timer.Start();
        // 로그 기록
        // log pipeline start
        _logger.Log(LogCategories.Pipeline, LogLevel.Info, "Pipeline started");
    }

    /// <summary>
    ///     파이프라인을 정지한다 (타이머 정지, 큐 비우기).
    ///     Stops the pipeline (stops timer, clears queue).
    /// </summary>
    internal void Stop() {
        // 타이머 정지
        // stop timer
        _timer.Stop();
        // 큐 비우기
        // clear queue
        _queue.Clear();
        // 링 버퍼 비우기
        // clear ring buffer
        _ringBuffer.Clear();
        // 로그 기록
        // log pipeline stop
        _logger.Log(LogCategories.Pipeline, LogLevel.Info, "Pipeline stopped");
    }

    /// <summary>
    ///     요청을 큐에 추가한다.
    ///     Enqueues a request.
    /// </summary>
    /// <param name="request">MODBUS 요청 / MODBUS request</param>
    /// <returns>추가 성공 여부 (중복이면 false) / true if enqueued, false if duplicate</returns>
    internal bool Enqueue(ModbusRequest request) {
        // 큐에 중복 방지 모드로 추가
        // enqueue with uniqueness enforcement
        var result = _queue.TryEnqueue(request);
        // 추가 성공 시 로그 기록
        // log if enqueue succeeded
        if (result)
            // 요청 인큐 기록
            // log request enqueue
            _logger.Log(LogCategories.Pipeline, LogLevel.Debug, $"Enqueued: FC=0x{(byte)request.Code:X2} Addr={request.Address}");
        // 결과 반환
        // return result
        return result;
    }

    /// <summary>
    ///     수신 데이터 이벤트 핸들러. 링 버퍼에 데이터를 기록한다.
    ///     Receive data event handler. Writes data to the ring buffer.
    /// </summary>
    /// <param name="data">수신 데이터 / received data</param>
    private void OnDataReceived(ReadOnlyMemory<byte> data) {
        // 수신 패킷 로그 기록
        // log received packet
        _logger.LogPacket("RX", data.Span);
        // 링 버퍼에 수신 데이터 기록
        // write received data to ring buffer
        _ringBuffer.WriteBytes(data.Span);
        // 마지막 수신 시각 갱신
        // update last receive time
        Volatile.Write(ref _lastReceiveTimeTicks, DateTime.UtcNow.Ticks);
    }

    /// <summary>
    ///     타이머 이벤트 핸들러. 프레임 파싱과 요청 전송을 순차 처리한다.
    ///     Timer event handler. Processes frame parsing and request transmission sequentially.
    /// </summary>
    private void OnTimerElapsed(object? sender, ElapsedEventArgs e) {
        // 재진입 방지 (이전 콜백이 진행 중이면 건너뜀)
        // reentrancy guard (skip if previous callback is still running)
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
        } finally {
            // 처리 플래그 해제
            // release processing flag
            Interlocked.Exchange(ref _processing, 0);
        }
    }

    /// <summary>
    ///     링 버퍼에서 완전한 프레임을 추출하고 처리한다.
    ///     Extracts and processes complete frames from the ring buffer.
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
            if (elapsed > _settings.Pipeline.FrameTimeout) {
                // 불완전 프레임 오류 통보
                // notify incomplete frame error
                ErrorOccurred?.Invoke(new ComError(ComErrorCode.InvalidFrame, "Frame receive timeout"));
                // 로그 기록
                // log frame timeout
                _logger.Log(LogCategories.Pipeline, LogLevel.Warning, $"Frame timeout: {_ringBuffer.Available} bytes discarded");
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
            // 프레임 길이 계산
            // calculate frame length
            var frameLength = _codec.CalcFrameLength(_ringBuffer);
            // 알 수 없는 프레임 확인 (0이면 미지 FC — 1바이트 재동기화)
            // check for unknown frame (0 means unknown FC — 1-byte resync)
            if (frameLength is 0) {
                // 1바이트 제거 후 재시도
                // discard 1 byte and retry
                _ringBuffer.RemoveBytes(1);
                // 다음 반복으로 계속
                // continue to next iteration
                continue;
            }

            // 데이터 부족 확인 (-1이면 불완전)
            // check if insufficient data (-1 means incomplete)
            if (frameLength < 0)
                // 데이터 부족 — 다음 수신 대기
                // insufficient data — wait for next receive
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
            // 프레임 무결성 검증 (RTU: CRC-16)
            // validate frame integrity (RTU: CRC-16)
            if (!_codec.ValidateFrame(frame)) {
                // CRC 오류 로그 기록
                // log CRC error
                _logger.Log(LogCategories.Error, LogLevel.Warning, $"CRC validation failed: {frameLength} bytes discarded");
                // 다음 프레임 처리로 계속
                // continue to next frame
                continue;
            }

            // 함수 코드 추출
            // extract function code
            var code = _codec.ExtractCode(frame);
            // 페이로드 추출
            // extract payload
            var payload = _codec.ExtractPayload(frame);
            // 프레임 처리
            // process frame
            ProcessReceivedFrame(code, payload);
        }
    }

    /// <summary>
    ///     수신된 프레임을 요청과 매칭하여 응답 이벤트를 발생시킨다.
    ///     Matches a received frame to a request and raises the response event.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         HANTAS 전용 FC (0x64, 0x65, 0x66)는 비요청 응답이므로 addr=0으로 처리.
    ///     </para>
    ///     <para>
    ///         HANTAS custom FC (0x64, 0x65, 0x66) are unsolicited responses, handled with addr=0.
    ///     </para>
    /// </remarks>
    /// <param name="code">함수 코드 / function code</param>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    private void ProcessReceivedFrame(FunctionCode code, byte[] payload) {
        // 기본 주소 0 (비요청 응답)
        // default address 0 (unsolicited response)
        var addr = 0;
        // 큐 선두 항목 확인
        // check front item in queue
        if (_queue.TryPeek(out var request))
            // 활성화된 요청인지 확인
            // check if request is activated
            if (request is { Activated: true })
                // 함수 코드 일치 또는 오류 응답인지 확인
                // check if function code matches or is error response
                if (code == request.Code || code is FunctionCode.Error) {
                    // 요청 주소를 응답에 매칭
                    // match request address to response
                    addr = request.Address;
                    // 큐에서 요청 제거
                    // remove request from queue
                    _queue.TryDequeue(out _);
                }

        // 응답 로그 기록
        // log response
        _logger.Log(LogCategories.Pipeline, LogLevel.Debug, $"Response: FC=0x{(byte)code:X2} Addr={addr} Len={payload.Length}");
        // 응답 이벤트 발생
        // raise response event
        ResponseReceived?.Invoke(new ModbusResponse(code, addr, payload));
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
            _logger.LogPacket("TX", request.Packet.Span);
            // 전송 계층을 통해 전송
            // transmit via transport layer
            if (_transport.Write(request.Packet.Span))
                // 반환
                // return
                return;
            // 전송 실패 — 큐에서 제거
            // transmission failed — remove from queue
            _queue.TryDequeue(out _);
            // 오류 통보
            // notify error
            ErrorOccurred?.Invoke(new ComError(ComErrorCode.Timeout, "Transport write failed"));
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
            _logger.Log(LogCategories.Pipeline, LogLevel.Warning, $"Timeout: FC=0x{(byte)request.Code:X2} Addr={request.Address} Retry={retry - 1}");
            // 타임아웃 오류 통보
            // notify timeout error
            ErrorOccurred?.Invoke(new ComError(ComErrorCode.Timeout, $"FC=0x{(byte)request.Code:X2} Addr={request.Address}"));
            // 반환
            // return
            return;
        }

        // 재시도 로그 기록
        // log retry
        _logger.Log(LogCategories.Pipeline, LogLevel.Debug, $"Retry {retry}: FC=0x{(byte)request.Code:X2} Addr={request.Address}");
    }
}