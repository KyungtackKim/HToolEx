using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Timers;
using HTool.Type;
using HTool.Util;
using SuperSimpleTcp;
using Timer = System.Timers.Timer;

namespace HTool.Device;

/// <summary>
///     MODBUS-TCP 프로토콜 구현 클래스. Ethernet을 통한 TCP/IP 통신을 담당합니다.
///     MODBUS-TCP protocol implementation class. Handles TCP/IP communication via Ethernet.
/// </summary>
/// <remarks>
///     <para>주요 특징 / Key Features:</para>
///     <list type="bullet">
///         <item>MBAP 헤더 자동 처리 (Transaction ID, Protocol ID, Length, Unit ID) / Automatic MBAP header handling</item>
///         <item>CRC 불필요 (TCP 레벨 체크섬 사용) / No CRC required (uses TCP-level checksum)</item>
///         <item>기본 포트 5000 (MODBUS 표준 502도 지원) / Default port 5000 (also supports MODBUS standard 502)</item>
///         <item>듀얼 버퍼 시스템 (ConcurrentQueue + RingBuffer) / Dual buffer system (ConcurrentQueue + RingBuffer)</item>
///         <item>SuperSimpleTcp 라이브러리 사용 / Uses SuperSimpleTcp library</item>
///         <item>타임아웃 기반 불완전 프레임 폐기 (500ms) / Timeout-based incomplete frame disposal (500ms)</item>
///     </list>
/// </remarks>
public sealed class HcTcp : ITool {
    /// <summary>
    ///     오류 프레임 크기 (TID + PID + LEN + UID + FC + ERROR)
    ///     Error frame size (TID + PID + LEN + UID + FC + ERROR)
    /// </summary>
    private const int ErrorFrameSize = 9;

    /// <summary>
    ///     TCP 클라이언트 인스턴스
    ///     TCP client instance
    /// </summary>
    private SimpleTcpClient? Client { get; set; }

    /// <summary>
    ///     1차 수신 버퍼. TCP 클라이언트 비동기 이벤트에서 받은 데이터를 임시 저장하는 스레드 안전 큐입니다.
    ///     Primary receive buffer. Thread-safe queue for temporarily storing data received from TCP client async events.
    /// </summary>
    /// <remarks>
    ///     비동기 수신 이벤트에서 즉시 데이터를 저장하고, ProcessTimer가 주기적으로 AnalyzeBuf로 이동합니다.
    ///     Stores data immediately from async receive events, then ProcessTimer periodically moves it to AnalyzeBuf.
    /// </remarks>
    private ConcurrentQueue<(byte[] buf, int len)> ReceiveBuf { get; } = [];

    /// <summary>
    ///     2차 분석 버퍼. MBAP 헤더 기반 프레임 경계 감지 및 완전한 MODBUS 패킷 추출을 위한 순환 버퍼입니다.
    ///     Secondary analysis buffer. Ring buffer for MBAP header-based frame boundary detection and complete MODBUS packet
    ///     extraction.
    /// </summary>
    /// <remarks>
    ///     ReceiveBuf의 데이터를 모아서 MBAP 헤더의 Length 필드로 프레임 길이를 계산하고 완전한 패킷을 추출합니다.
    ///     Collects data from ReceiveBuf, calculates frame length from MBAP header's Length field, and extracts complete
    ///     packets.
    /// </remarks>
    private RingBuffer AnalyzeBuf { get; } = new(16 * 1024);

    /// <summary>
    ///     데이터 처리 타이머
    ///     Data processing timer
    /// </summary>
    private Timer ProcessTimer { get; set; } = null!;

    /// <summary>
    ///     패킷 분석 타임아웃 시간
    ///     Packet analysis timeout time
    /// </summary>
    private DateTime AnalyzeTimeout { get; set; }

    /// <summary>
    ///     타이머 정지 플래그
    ///     Timer stop flag
    /// </summary>
    private bool IsStopTimer { get; set; }

    /// <summary>
    ///     프로토콜 헤더 크기 (MBAP 헤더 + FC)
    ///     Protocol header size (MBAP header + FC)
    /// </summary>
    public int HeaderSize { get; } = 8;

    /// <summary>
    ///     함수 코드 위치 (헤더 내 오프셋)
    ///     Function code position (offset in header)
    /// </summary>
    public int FunctionPos { get; } = 7;

    /// <summary>
    ///     장치 식별자 번호 (MODBUS 슬레이브 ID)
    ///     Device identifier number (MODBUS slave ID)
    /// </summary>
    public byte DeviceId { get; set; }

    /// <summary>
    ///     도구 프로토콜 세대
    ///     Tool protocol generation
    /// </summary>
    public GenerationTypes Revision { get; set; }

    /// <summary>
    ///     연결 상태 변경 이벤트
    ///     Connection state changed event
    /// </summary>
    public event ITool.PerformConnect? ChangedConnect;

    /// <summary>
    ///     파싱된 데이터 수신 이벤트
    ///     Parsed data received event
    /// </summary>
    public event ITool.PerformReceiveData? ReceivedData;

    /// <summary>
    ///     수신 오류 이벤트
    ///     Receive error event
    /// </summary>
    public event ITool.PerformReceiveError? ReceivedError;

    /// <summary>
    ///     원시 데이터 수신 이벤트
    ///     Raw data received event
    /// </summary>
    public event ITool.PerformRawData? ReceivedRaw;

    /// <summary>
    ///     원시 데이터 전송 이벤트
    ///     Raw data transmitted event
    /// </summary>
    public event ITool.PerformRawData? TransmitRaw;

    /// <summary>
    ///     TCP 연결
    ///     Connect via TCP
    /// </summary>
    /// <param name="target">IP 주소 / IP address</param>
    /// <param name="option">포트 번호 / port number</param>
    /// <param name="id">장치 ID / device ID</param>
    /// <returns>연결 성공 여부 / connection success result</returns>
    public bool Connect(string target, int option, byte id = 1) {
        // 대상 IP 검증
        // check target IP
        if (!IPAddress.TryParse(target, out var ip))
            return false;
        // 포트 번호 검증
        // check port number
        if (option is < 0 or > 65535)
            return false;
        // 장치 ID 검증
        // check device ID
        if (id > 0x0F)
            return false;

        try {
            // 수신 버퍼 정리
            // clean up receive buffer
            foreach (var value in ReceiveBuf)
                // 풀에 반환
                // return to pool
                ArrayPool<byte>.Shared.Return(value.buf);
            // 버퍼 초기화
            // clear buffers
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();
            // 클라이언트 생성
            // create client
            Client = new SimpleTcpClient(ip, option);
            // 이벤트 설정
            // set events
            Client.Events.Connected    += ClientOnConnectionChanged;
            Client.Events.Disconnected += ClientOnConnectionChanged;
            Client.Events.DataReceived += ClientOnDataReceived;
            // Keep-Alive 설정
            // set keep-alive
            Client.Keepalive.EnableTcpKeepAlives    = true;
            Client.Keepalive.TcpKeepAliveInterval   = 5;
            Client.Keepalive.TcpKeepAliveTime       = 5;
            Client.Keepalive.TcpKeepAliveRetryCount = 5;
            // 경고: 반드시 아래 설정 필요. 미설정 시 데이터 수신 오류 발생 가능
            // WARNING: Settings below are required. Data receive errors may occur if not set
            Client.Settings.UseAsyncDataReceivedEvents = false;
            Client.Settings.StreamBufferSize           = 16 * 1024;
            // 연결
            // connect
            Client.Connect();

            // 장치 ID 설정
            // set device ID
            DeviceId = id;
            // 처리 타이머 생성
            // create process timer
            ProcessTimer = new Timer();
            // 타이머 옵션 설정
            // set timer options
            ProcessTimer.AutoReset =  true;
            ProcessTimer.Interval  =  Constants.ProcessPeriod;
            ProcessTimer.Elapsed   += ProcessTimerOnElapsed;
            // 타이머 시작
            // start timer
            ProcessTimer.Start();

            // 성공
            // success
            return true;
        } catch (Exception ex) {
            // 오류 출력
            // print error
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     TCP 연결 종료
    ///     Close TCP connection
    /// </summary>
    public void Close() {
        try {
            // 타이머 확인 및 정지
            // check and stop timer
            if (ProcessTimer is { Enabled: true })
                IsStopTimer = true;

            // 클라이언트 연결 해제
            // disconnect client
            if (Client is { IsConnected: true })
                Client.Disconnect();

            // 리소스 해제
            // dispose resources
            Client?.Dispose();
            Client = null;
        } catch (Exception ex) {
            // 오류 출력
            // print error
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    ///     패킷 전송
    ///     Write packet data
    /// </summary>
    /// <param name="packet">패킷 데이터 / packet data</param>
    /// <param name="length">전송 길이 / transmit length</param>
    /// <returns>전송 성공 여부 / transmission success result</returns>
    public bool Write(byte[] packet, int length) {
        // 클라이언트 연결 확인
        // check client connection
        if (Client is not { IsConnected: true })
            return false;

        try {
            // 패킷 전송
            // send packet
            Client.Send(packet);
            // 전송 이벤트 발생
            // raise transmit event
            TransmitRaw?.Invoke(packet);
            // 성공
            // success
            return true;
        } catch (Exception ex) {
            // 오류 출력
            // print error
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     홀딩 레지스터 읽기 패킷 생성 (0x03)
    ///     Create read holding register packet (0x03)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">레지스터 수 / register count</param>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] GetReadHoldingRegPacket(ushort addr, ushort count) {
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(12);
        // MBAP 헤더 설정
        // set MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x06;
        packet[6] = 0x00;
        // PDU 값 설정
        // set PDU values
        packet[7]  = (byte)CodeTypes.ReadHoldingReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        return packet;
    }

    /// <summary>
    ///     입력 레지스터 읽기 패킷 생성 (0x04)
    ///     Create read input register packet (0x04)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">레지스터 수 / register count</param>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] GetReadInputRegPacket(ushort addr, ushort count) {
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(12);
        // MBAP 헤더 설정
        // set MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x06;
        packet[6] = 0x00;
        // PDU 값 설정
        // set PDU values
        packet[7]  = (byte)CodeTypes.ReadInputReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        return packet;
    }

    /// <summary>
    ///     단일 레지스터 쓰기 패킷 생성 (0x06)
    ///     Create write single register packet (0x06)
    /// </summary>
    /// <param name="addr">레지스터 주소 / register address</param>
    /// <param name="value">쓸 값 / value to write</param>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] SetSingleRegPacket(ushort addr, ushort value) {
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(12);
        // MBAP 헤더 설정
        // set MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x06;
        packet[6] = 0x00;
        // PDU 값 설정
        // set PDU values
        packet[7]  = (byte)CodeTypes.WriteSingleReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((value >> 8) & 0xFF);
        packet[11] = (byte)(value        & 0xFF);
        return packet;
    }

    /// <summary>
    ///     다중 레지스터 쓰기 패킷 생성 (0x10)
    ///     Create write multiple register packet (0x10)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="values">쓸 값들 / values to write</param>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] SetMultiRegPacket(ushort addr, ReadOnlySpan<ushort> values) {
        var index = 13;
        // 레지스터 수 계산
        // calculate register count
        var count = values.Length;
        // PDU 크기 계산
        // calculate PDU size
        var pdu = 7 + count * 2;
        // 패킷 크기 계산
        // calculate packet size
        var size = 6 + pdu;
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(size);
        // MBAP 헤더 설정
        // set MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = (byte)((pdu >> 8) & 0xFF);
        packet[5] = (byte)(pdu        & 0xFF);
        packet[6] = 0x00;
        // PDU 값 설정
        // set PDU values
        packet[7]  = (byte)CodeTypes.WriteMultiReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        packet[12] = (byte)(count * 2);

        // 데이터 값 설정
        // set data values
        foreach (var value in values) {
            packet[index++] = (byte)((value >> 8) & 0xFF);
            packet[index++] = (byte)(value        & 0xFF);
        }

        return packet;
    }

    /// <summary>
    ///     문자열 레지스터 쓰기 패킷 생성
    ///     Create write string register packet (ASCII)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="str">문자열 / string</param>
    /// <param name="length">문자열 길이 / string length</param>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] SetMultiRegStrPacket(ushort addr, string str, int length) {
        var index = 13;
        // 길이 검증 및 조정
        // validate and adjust length
        if (length < str.Length)
            length = str.Length;
        // 레지스터 수 계산
        // calculate register count
        var count = length / 2;
        // PDU 크기 계산
        // calculate PDU size
        var pdu = index + count * 2;
        // 패킷 크기 계산
        // calculate packet size
        var size = 6 + pdu;
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(size);
        // MBAP 헤더 설정
        // set MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = (byte)((pdu >> 8) & 0xFF);
        packet[5] = (byte)(pdu        & 0xFF);
        packet[6] = 0x00;
        // PDU 값 설정
        // set PDU values
        packet[7]  = (byte)CodeTypes.WriteMultiReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        packet[12] = (byte)length;
        // 문자열 값 설정
        // set string values
        foreach (var c in str)
            packet[index++] = (byte)c;

        // 패딩 0 설정
        // set padding zeros
        while (index < 13 + length)
            packet[index++] = 0;
        return packet;
    }

    /// <summary>
    ///     장치 정보 읽기 패킷 생성 (0x11)
    ///     Create read device info packet (0x11)
    /// </summary>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] GetInfoRegPacket() {
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(8);
        // MBAP 헤더 설정
        // set MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x02;
        packet[6] = 0x00;
        // PDU 값 설정
        // set PDU values
        packet[7] = (byte)CodeTypes.ReadInfoReg;
        return packet;
    }

    /// <summary>
    ///     연결 상태 변경 이벤트 핸들러
    ///     Connection state changed event handler
    /// </summary>
    /// <param name="sender">이벤트 발생 객체 / event sender</param>
    /// <param name="e">연결 이벤트 인자 / connection event args</param>
    private void ClientOnConnectionChanged(object? sender, ConnectionEventArgs e) {
        // 연결 해제 사유 확인
        // check disconnect reason
        if (e.Reason != DisconnectReason.None)
            Close();

        // 연결 상태 변경 이벤트 발생
        // raise connection changed event
        ChangedConnect?.Invoke(e.Reason == DisconnectReason.None);
    }

    /// <summary>
    ///     데이터 수신 이벤트 핸들러
    ///     Data received event handler
    /// </summary>
    /// <param name="sender">이벤트 발생 객체 / event sender</param>
    /// <param name="e">데이터 수신 이벤트 인자 / data received event args</param>
    private void ClientOnDataReceived(object? sender, DataReceivedEventArgs e) {
        // 데이터 가져오기
        // get data
        var data = e.Data;
        // 데이터 유효성 확인
        // check data validity
        if (data.Array is null)
            return;
        // 길이 가져오기
        // get length
        var read = data.Count;
        // 길이 확인
        // check length
        if (read < 1)
            return;

        // 풀에서 버퍼 대여
        // rent buffer from pool
        var chunk = ArrayPool<byte>.Shared.Rent(read);
        // 데이터 복사
        // copy data
        Buffer.BlockCopy(data.Array, data.Offset, chunk, 0, read);
        // 큐에 데이터 블록 추가
        // enqueue data block
        ReceiveBuf.Enqueue((chunk, read));
        // 원시 데이터 이벤트 확인
        // check raw data event
        if (ReceivedRaw is null)
            return;
        // 원시 데이터용 메모리 할당
        // allocate memory for raw data
        var raw = GC.AllocateUninitializedArray<byte>(read);
        // 데이터 복사
        // copy data
        Buffer.BlockCopy(chunk, 0, raw, 0, read);
        // 이벤트 발생
        // raise event
        ReceivedRaw(raw);
    }

    /// <summary>
    ///     데이터 처리 타이머 이벤트 핸들러
    ///     Data processing timer event handler
    /// </summary>
    /// <param name="sender">이벤트 발생 객체 / event sender</param>
    /// <param name="e">타이머 이벤트 인자 / timer event args</param>
    private void ProcessTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        // 정지 플래그 확인
        // check stop flag
        if (IsStopTimer) {
            // 이벤트 해제
            // unsubscribe event
            ProcessTimer.Elapsed -= ProcessTimerOnElapsed;
            // 타이머 정지
            // stop timer
            ProcessTimer.Stop();
            // 타이머 해제
            // dispose timer
            ProcessTimer.Dispose();
            // 수신 버퍼 정리
            // clean up receive buffer
            foreach (var value in ReceiveBuf)
                // 풀에 반환
                // return to pool
                ArrayPool<byte>.Shared.Return(value.buf);
            // 버퍼 초기화
            // clear buffers
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();
            return;
        }

        // 모니터 진입 시도
        // try enter monitor
        if (!Monitor.TryEnter(AnalyzeBuf, Constants.ProcessLockTime))
            return;
        try {
            var isUpdateForAnalyzeBuf = false;
            // 데이터 블록 처리
            // process data blocks
            while (ReceiveBuf.TryDequeue(out var block)) {
                // 블록 분해
                // decompose block
                var (buf, length) = block;
                // 분석 버퍼에 쓰기
                // write to analyze buffer
                AnalyzeBuf.WriteBytes(buf.AsSpan(0, length));
                // 풀에 반환
                // return to pool
                ArrayPool<byte>.Shared.Return(buf);
                // 갱신 플래그 설정
                // set update flag
                isUpdateForAnalyzeBuf = true;
            }

            // 버퍼 갱신 시 타임아웃 리셋
            // reset timeout on buffer update
            if (isUpdateForAnalyzeBuf)
                AnalyzeTimeout = DateTime.Now;

            // 타임아웃 확인 및 처리
            // check and handle timeout
            if (AnalyzeBuf.Available > 0)
                if ((DateTime.Now - AnalyzeTimeout).TotalMilliseconds > Constants.ProcessTimeout) {
                    // 버퍼 길이 저장
                    // save buffer length
                    var len = AnalyzeBuf.Available;
                    // 버퍼 초기화
                    // clear buffer
                    AnalyzeBuf.Clear();
                    // 오류 이벤트 발생
                    // raise error event
                    ReceivedError?.Invoke(ComErrorTypes.Timeout, len);
                }

            // 헤더 길이 확인 [TID(2) PID(2) LEN(2) UID(1) FC(1)]
            // check header length [TID(2) PID(2) LEN(2) UID(1) FC(1)]
            if (AnalyzeBuf.Available < HeaderSize)
                return;
            // 함수 코드 값 가져오기
            // get function code value
            var value = AnalyzeBuf.Peek(FunctionPos);
            // 오류 여부 확인
            // check error flag
            var isError = (value & 0x80) != 0x00;
            // 기본 함수 코드 추출
            // extract base function code
            var baseFc = (byte)(value & 0x7F);
            // 알려진 코드 확인
            // check known code
            if (!Tool.IsKnownCode(baseFc) && !isError)
                return;
            // 명령 타입 결정
            // determine command type
            var cmd = !isError ? (CodeTypes)baseFc : CodeTypes.Error;
            // 프레임 크기 계산
            // calculate frame size
            var frame = cmd switch {
                // 읽기 응답: TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(1)=DATA_LEN DATA(N)
                // read response: TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(1)=DATA_LEN DATA(N)
                CodeTypes.ReadHoldingReg or CodeTypes.ReadInputReg or CodeTypes.ReadInfoReg =>
                    AnalyzeBuf.Peek(HeaderSize) + 9,
                // 쓰기 응답: TID(2) PID(2) LEN(2) UID(1) FC(1) ADDR(2) VALUE(2)/COUNT(2)
                // write response: TID(2) PID(2) LEN(2) UID(1) FC(1) ADDR(2) VALUE(2)/COUNT(2)
                CodeTypes.WriteSingleReg or CodeTypes.WriteMultiReg => 12,
                // 그래프: TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN DATA(N)
                // graph: TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN DATA(N)
                CodeTypes.Graph or CodeTypes.GraphRes =>
                    ((AnalyzeBuf.Peek(HeaderSize) << 8) | AnalyzeBuf.Peek(HeaderSize + 1)) + 10,
                // 고해상도 그래프: TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN REV(2) DATA(N)
                // high-res graph: TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN REV(2) DATA(N)
                CodeTypes.HighResGraph =>
                    ((AnalyzeBuf.Peek(HeaderSize) << 8) | AnalyzeBuf.Peek(HeaderSize + 1)) + 10,
                // 오류: TID(2) PID(2) LEN(2) UID(1) FC(1) ERROR(1)
                // error: TID(2) PID(2) LEN(2) UID(1) FC(1) ERROR(1)
                CodeTypes.Error => ErrorFrameSize,
                _               => throw new ArgumentOutOfRangeException(string.Empty)
            };

            // 프레임 완성 확인
            // check frame completeness
            if (AnalyzeBuf.Available < frame)
                return;

            // 패킷 추출
            // extract packet
            var packet = AnalyzeBuf.ReadBytes(frame);
            // 수신 이벤트 발생
            // raise receive event
            ReceivedData?.Invoke(cmd, packet);
        } finally {
            // 모니터 종료
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}