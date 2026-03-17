using HTool.Device;
using HTool.Device.Codec;
using HTool.Device.Protocol;
using HTool.Type;
using Tester.TestHelpers;

namespace Tester.Device;

/// <summary>
///     MessagePipeline 통합 테스트.
///     MessagePipeline integration tests.
/// </summary>
public sealed class MessagePipelineTests : IDisposable {
    // TCP 코덱 (CRC 검증 없음 — 기본 테스트용)
    // TCP codec (no CRC validation — for default tests)
    private readonly ModbusTcpCodec _codec;
    // 로거 인스턴스
    // logger instance
    private readonly HToolLogger _logger;
    // 파이프라인 인스턴스
    // pipeline instance
    private readonly MessagePipeline _pipeline;
    // 설정 인스턴스
    // settings instance
    private readonly HToolSettings _settings;
    // 가짜 전송 계층
    // fake transport layer
    private readonly FakeTransport _transport;

    /// <summary>
    ///     테스트 픽스처를 초기화한다.
    ///     initializes the test fixture.
    /// </summary>
    public MessagePipelineTests() {
        // 가짜 전송 계층 생성
        // create fake transport
        _transport = new FakeTransport();
        // TCP 코덱 생성
        // create TCP codec
        _codec = new ModbusTcpCodec();
        // 로거 생성
        // create logger
        _logger = new HToolLogger();
        // 설정 생성 및 빠른 타임아웃 구성
        // create settings with fast timeouts
        _settings = new HToolSettings {
            // 파이프 라인 설정
            // pipeline settings
            Pipeline = {
                // 메시지 타임아웃을 200ms로 설정 (빠른 테스트용)
                // set message timeout to 200ms (for fast tests)
                MessageTimeout = 200, // 재시도 횟수를 1로 설정
                // set retry count to 1
                MessageRetry = 1, // 프레임 타임아웃을 200ms로 설정
                // set frame timeout to 200ms
                FrameTimeout = 200
            }
        };
        // 파이프라인 생성
        // create pipeline
        _pipeline = new MessagePipeline(_transport, _codec, _logger, _settings);
    }

    /// <inheritdoc />
    public void Dispose() {
        // 파이프라인 해제
        // dispose pipeline
        _pipeline.Dispose();
        // 전송 계층 해제
        // dispose transport
        _transport.Dispose();
        // 로거 해제
        // dispose logger
        _logger.Dispose();
    }

    /// <summary>
    ///     TCP ReadHolding 요청을 생성한다.
    ///     creates a TCP ReadHolding request.
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">레지스터 수 / register count</param>
    /// <param name="dId">장치 ID / device ID</param>
    /// <returns>MODBUS 요청 / MODBUS request</returns>
    private ModbusRequest MakeRequest(ushort addr = 100, ushort count = 1, byte dId = 1) {
        // TCP 코덱으로 패킷 생성
        // build packet via TCP codec
        var packet = _codec.BuildReadHoldingReg(addr, count, dId);
        // 요청 객체 생성 및 반환
        // create and return request object
        return new ModbusRequest(FunctionCode.ReadHoldingReg, addr, packet);
    }

    /// <summary>
    ///     TCP ReadHolding 응답 프레임을 생성한다.
    ///     creates a TCP ReadHolding response frame.
    /// </summary>
    /// <param name="payload">페이로드 데이터 / payload data</param>
    /// <param name="dId">장치 ID / device ID</param>
    /// <returns>완성된 TCP 프레임 / complete TCP frame</returns>
    private static byte[] MakeTcpResponse(byte[] payload, byte dId = 1) {
        // PDU 길이 계산: UID(1) + FC(1) + ByteCount(1) + Data(N)
        // calculate PDU length: UID(1) + FC(1) + ByteCount(1) + Data(N)
        var pduLen = 3 + payload.Length;
        // MBAP(7) + FC(1) + ByteCount(1) + Data(N) 프레임 생성
        // create MBAP(7) + FC(1) + ByteCount(1) + Data(N) frame
        var frame = new byte[7 + 1 + 1 + payload.Length];
        // 트랜잭션 ID (0)
        // transaction ID (0)
        frame[0] = 0;
        frame[1] = 0;
        // 프로토콜 ID (0)
        // protocol ID (0)
        frame[2] = 0;
        frame[3] = 0;
        // PDU 길이 기록 (Big-Endian)
        // write PDU length (Big-Endian)
        frame[4] = (byte)(pduLen >> 8);
        frame[5] = (byte)pduLen;
        // 유닛 ID 기록
        // write unit ID
        frame[6] = dId;
        // 함수 코드 기록
        // write function code
        frame[7] = (byte)FunctionCode.ReadHoldingReg;
        // 바이트 카운트 기록
        // write byte count
        frame[8] = (byte)payload.Length;
        // 페이로드 복사
        // copy payload
        Array.Copy(payload, 0, frame, 9, payload.Length);
        // 완성된 프레임 반환
        // return complete frame
        return frame;
    }

    [Fact]
    public async Task Enqueue_TimerTick_PacketWrittenToTransport() {
        // 요청 생성
        // create request
        var request = MakeRequest();
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();

        // 요청 인큐
        // enqueue request
        _pipeline.Enqueue(request);
        // 타이머 틱 대기
        // wait for timer tick
        await Task.Delay(200);

        // 전송 패킷이 기록되었는지 확인
        // verify packet was written to transport
        Assert.NotEmpty(_transport.WrittenPackets);
    }

    [Fact]
    public void Enqueue_DuplicateRequest_Rejected() {
        // 동일한 주소로 두 요청 생성
        // create two requests with same address
        var request1 = MakeRequest();
        // 동일 키의 두 번째 요청 생성
        // create second request with same key
        var request2 = MakeRequest();

        // 첫 번째 요청 인큐
        // enqueue first request
        var result1 = _pipeline.Enqueue(request1);
        // 중복 요청 인큐 시도
        // attempt to enqueue duplicate
        var result2 = _pipeline.Enqueue(request2);

        // 첫 번째는 성공해야 함
        // first should succeed
        Assert.True(result1);
        // 두 번째는 거부되어야 함
        // second should be rejected
        Assert.False(result2);
    }

    [Fact]
    public async Task Response_Matching_FiresResponseReceived() {
        // 수신된 응답 기록용 변수
        // variable to record received response
        ModbusResponse? received = null;
        // 응답 이벤트 구독
        // subscribe to response event
        _pipeline.ResponseReceived += r => received = r;
        // 요청 생성
        // create request
        var request = MakeRequest();
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 요청 인큐
        // enqueue request
        _pipeline.Enqueue(request);

        // 요청이 전송될 때까지 대기
        // wait for request to be transmitted
        await Task.Delay(200);

        // 응답 프레임 생성 (2바이트 페이로드)
        // create response frame (2-byte payload)
        var response = MakeTcpResponse("\0d"u8.ToArray());
        // 응답 수신 시뮬레이션
        // simulate receiving response
        _transport.SimulateReceive(response);
        // 응답 처리 대기
        // wait for response processing
        await Task.Delay(200);

        // 응답이 수신되었는지 확인
        // verify response was received
        Assert.NotNull(received);
        // 함수 코드 일치 확인
        // verify function code matches
        Assert.Equal(FunctionCode.ReadHoldingReg, received.Value.Code);
        // 주소가 요청의 주소와 매칭되었는지 확인
        // verify address was matched from request
        Assert.Equal(100, received.Value.Address);
    }

    [Fact]
    public async Task Response_UnmatchedRequest_StillFiresEvent() {
        // 수신된 응답 기록용 변수
        // variable to record received response
        ModbusResponse? received = null;
        // 응답 이벤트 구독
        // subscribe to response event
        _pipeline.ResponseReceived += r => received = r;
        // 파이프라인 시작 (큐 비어 있음)
        // start pipeline (queue is empty)
        _pipeline.Start();

        // 요청 없이 응답 수신 시뮬레이션
        // simulate response without matching request
        var response = MakeTcpResponse("\0d"u8.ToArray());
        // 응답 수신 시뮬레이션
        // simulate receiving response
        _transport.SimulateReceive(response);
        // 응답 처리 대기
        // wait for response processing
        await Task.Delay(200);

        // 비매칭 응답도 이벤트 발생 확인
        // verify event fires even for unmatched response
        Assert.NotNull(received);
        // 비요청 응답의 주소는 0
        // unmatched response address is 0
        Assert.Equal(0, received.Value.Address);
    }

    [Fact]
    public async Task Timeout_NoResponse_RetriesSend() {
        // 재시도 횟수를 1로 설정 (총 2회 전송)
        // set retry to 1 (total 2 transmissions)
        _settings.Pipeline.MessageRetry = 1;
        // 타임아웃을 100ms로 단축
        // shorten timeout to 100ms
        _settings.Pipeline.MessageTimeout = 100;

        // 요청 생성
        // create request
        var request = MakeRequest();
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 요청 인큐
        // enqueue request
        _pipeline.Enqueue(request);

        // 타임아웃 + 재시도 대기 (100ms 타임아웃 × 2 + 여유)
        // wait for timeout + retry (100ms timeout × 2 + margin)
        await Task.Delay(500);

        // 재시도로 인해 최소 2회 전송 확인
        // verify at least 2 transmissions due to retry
        Assert.True(_transport.WrittenPackets.Count >= 2);
    }

    [Fact]
    public async Task MaxRetryExceeded_FiresErrorOccurred() {
        // 재시도 없음 (총 1회 시도만)
        // no retry (only 1 attempt)
        _settings.Pipeline.MessageRetry = 0;
        // 타임아웃을 100ms로 단축
        // shorten timeout to 100ms
        _settings.Pipeline.MessageTimeout = 100;

        // 오류 기록용 변수
        // variable to record error
        ComError? error = null;
        // 오류 이벤트 구독
        // subscribe to error event
        _pipeline.ErrorOccurred += e => error = e;
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 요청 인큐
        // enqueue request
        _pipeline.Enqueue(MakeRequest());

        // 타임아웃 초과 대기
        // wait for timeout to exceed
        await Task.Delay(400);

        // 오류 이벤트 발생 확인
        // verify error event was fired
        Assert.NotNull(error);
        // 타임아웃 오류 코드 확인
        // verify timeout error code
        Assert.Equal(ComErrorCode.Timeout, error.Value.Reason);
    }

    [Fact]
    public async Task RtuFrame_InvalidCrc_Discarded() {
        // RTU 코덱 파이프라인 생성
        // create pipeline with RTU codec
        var rtuCodec = new ModbusRtuCodec();
        // RTU 설정 생성
        // create RTU settings
        var rtuSettings = new HToolSettings {
            // 파이프라인 설정
            // pipeline settings
            Pipeline = {
                // 프레임 타임아웃을 500ms로 설정
                // set frame timeout to 500ms
                FrameTimeout = 500
            }
        };
        // RTU 파이프라인 생성
        // create RTU pipeline
        using var rtuPipeline = new MessagePipeline(_transport, rtuCodec, _logger, rtuSettings);

        // 수신 응답 기록용 변수
        // variable to record received response
        ModbusResponse? received = null;
        // 응답 이벤트 구독
        // subscribe to response event
        rtuPipeline.ResponseReceived += r => received = r;

        // RTU 요청 생성
        // create RTU request
        var packet = rtuCodec.BuildReadHoldingReg(100, 1, 1);
        // 요청 객체 생성
        // create request object
        var request = new ModbusRequest(FunctionCode.ReadHoldingReg, 100, packet);

        // 파이프라인 시작
        // start pipeline
        rtuPipeline.Start();
        // 요청 인큐
        // enqueue request
        rtuPipeline.Enqueue(request);
        // 요청 전송 대기
        // wait for request transmission
        await Task.Delay(200);

        // 잘못된 CRC가 포함된 RTU 응답 프레임 구성
        // build RTU response frame with invalid CRC
        // RTU 응답: ID(1) + FC(1) + ByteCount(1) + Data(2) + CRC(2) = 7
        // RTU response: ID(1) + FC(1) + ByteCount(1) + Data(2) + CRC(2) = 7
        byte[] badFrame = [0x01, 0x03, 0x02, 0x00, 0x64, 0xFF, 0xFF];
        // 잘못된 프레임 수신 시뮬레이션
        // simulate receiving bad frame
        _transport.SimulateReceive(badFrame);
        // 프레임 처리 대기
        // wait for frame processing
        await Task.Delay(200);

        // CRC 무효로 응답이 폐기되었는지 확인
        // verify response was discarded due to invalid CRC
        Assert.Null(received);
    }

    [Fact]
    public async Task TcpFrame_AlwaysAccepted() {
        // 수신 응답 기록용 변수
        // variable to record received response
        ModbusResponse? received = null;
        // 응답 이벤트 구독
        // subscribe to response event
        _pipeline.ResponseReceived += r => received = r;
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 요청 인큐 (응답 매칭용)
        // enqueue request (for response matching)
        _pipeline.Enqueue(MakeRequest());
        // 요청 전송 대기
        // wait for request transmission
        await Task.Delay(200);

        // TCP 응답 프레임 생성
        // create TCP response frame
        var response = MakeTcpResponse("\0d"u8.ToArray());
        // 응답 수신 시뮬레이션
        // simulate receiving response
        _transport.SimulateReceive(response);
        // 응답 처리 대기
        // wait for response processing
        await Task.Delay(200);

        // TCP는 CRC 없이 항상 수락됨을 확인
        // verify TCP always accepts (no CRC)
        Assert.NotNull(received);
    }

    [Fact]
    public async Task Start_TimerBegins_PacketSent() {
        // 요청 인큐 (시작 전)
        // enqueue request (before start)
        _pipeline.Enqueue(MakeRequest());

        // 시작 전에는 패킷 없음 확인
        // verify no packets before start
        Assert.Empty(_transport.WrittenPackets);

        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 타이머 작동 대기
        // wait for timer activation
        await Task.Delay(200);

        // 시작 후 패킷 전송 확인
        // verify packet sent after start
        Assert.NotEmpty(_transport.WrittenPackets);
    }

    [Fact]
    public async Task Stop_TimerStops_QueueCleared() {
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 요청 인큐
        // enqueue request
        _pipeline.Enqueue(MakeRequest(200));
        // 요청 전송 대기
        // wait for request transmission
        await Task.Delay(200);

        // 전송 목록 초기화
        // clear written packets list
        _transport.WrittenPackets.Clear();

        // 파이프라인 정지
        // stop pipeline
        _pipeline.Stop();

        // 정지 후 새 요청 인큐
        // enqueue new request after stop
        _pipeline.Enqueue(MakeRequest(300));
        // 타이머가 정지했으므로 패킷 전송 없어야 함
        // timer is stopped so no packets should be sent
        await Task.Delay(200);

        // 정지 후 추가 전송 없음 확인
        // verify no additional transmissions after stop
        Assert.Empty(_transport.WrittenPackets);
    }

    [Fact]
    public void Dispose_CleansUpResources() {
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();
        // 파이프라인 해제
        // dispose pipeline
        _pipeline.Dispose();

        // 이중 해제 시 예외 없음 확인
        // verify no exception on double dispose
        var ex = Record.Exception(() => _pipeline.Dispose());
        // 예외 없음 확인
        // verify no exception
        Assert.Null(ex);
    }

    [Fact]
    public async Task MultipleRequests_ProcessedSequentially() {
        // 수신 응답 목록
        // list of received responses
        var responses = new List<ModbusResponse>();
        // 응답 이벤트 구독
        // subscribe to response event
        _pipeline.ResponseReceived += r => responses.Add(r);
        // 파이프라인 시작
        // start pipeline
        _pipeline.Start();

        // 서로 다른 주소로 두 요청 인큐
        // enqueue two requests with different addresses
        _pipeline.Enqueue(MakeRequest());
        // 두 번째 요청 인큐
        // enqueue second request
        _pipeline.Enqueue(MakeRequest(200));

        // 첫 번째 요청 전송 대기
        // wait for first request transmission
        await Task.Delay(200);

        // 첫 번째 응답 수신
        // receive first response
        _transport.SimulateReceive(MakeTcpResponse("\0d"u8.ToArray()));
        // 첫 번째 응답 처리 및 두 번째 요청 전송 대기
        // wait for first response processing and second request transmission
        await Task.Delay(200);

        // 두 번째 응답 수신
        // receive second response
        _transport.SimulateReceive(MakeTcpResponse([0x00, 0xC8]));
        // 두 번째 응답 처리 대기
        // wait for second response processing
        await Task.Delay(200);

        // 두 응답 모두 수신 확인
        // verify both responses received
        Assert.True(responses.Count >= 2);
        // 첫 번째 응답의 주소 확인
        // verify first response address
        Assert.Equal(100, responses[0].Address);
        // 두 번째 응답의 주소 확인
        // verify second response address
        Assert.Equal(200, responses[1].Address);
    }
}