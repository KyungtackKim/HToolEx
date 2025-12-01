using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Timers;
using HTool.Type;
using HTool.Util;
using Timer = System.Timers.Timer;

namespace HTool.Device;

/// <summary>
///     MODBUS-RTU 프로토콜 구현 클래스. 시리얼 포트를 통한 RS-232/RS-485 통신을 담당합니다.
///     MODBUS-RTU protocol implementation class. Handles RS-232/RS-485 communication via serial port.
/// </summary>
/// <remarks>
///     <para>주요 특징 / Key Features:</para>
///     <list type="bullet">
///         <item>CRC-16 자동 검증 / Automatic CRC-16 validation</item>
///         <item>보드레이트 9600-230400 bps 지원 / Supports baud rates 9600-230400 bps</item>
///         <item>듀얼 버퍼 시스템 (ConcurrentQueue + RingBuffer) / Dual buffer system (ConcurrentQueue + RingBuffer)</item>
///         <item>
///             자동 프레임 경계 감지 (함수 코드 기반 길이 계산) / Automatic frame boundary detection (function code-based length
///             calculation)
///         </item>
///         <item>타임아웃 기반 불완전 프레임 폐기 (500ms) / Timeout-based incomplete frame disposal (500ms)</item>
///     </list>
/// </remarks>
public sealed class HcRtu : ITool {
    /// <summary>
    ///     오류 프레임 크기 (ID + FC + ERROR + CRC)
    ///     Error frame size (ID + FC + ERROR + CRC)
    /// </summary>
    private const int ErrorFrameSize = 5;

    /// <summary>
    ///     지원 보드레이트 목록
    ///     Supported baud rate list
    /// </summary>
    private static readonly int[] BaudRates = [9600, 19200, 38400, 57600, 115200, 230400];

    /// <summary>
    ///     시리얼 포트 인스턴스
    ///     Serial port instance
    /// </summary>
    private SerialPort Port { get; } = new();

    /// <summary>
    ///     1차 수신 버퍼. SerialPort 비동기 이벤트에서 받은 데이터를 임시 저장하는 스레드 안전 큐입니다.
    ///     Primary receive buffer. Thread-safe queue for temporarily storing data received from SerialPort async events.
    /// </summary>
    /// <remarks>
    ///     비동기 수신 이벤트에서 즉시 데이터를 저장하고, ProcessTimer가 주기적으로 AnalyzeBuf로 이동합니다.
    ///     Stores data immediately from async receive events, then ProcessTimer periodically moves it to AnalyzeBuf.
    /// </remarks>
    private ConcurrentQueue<(byte[] buf, int len)> ReceiveBuf { get; } = [];

    /// <summary>
    ///     2차 분석 버퍼. 프레임 경계 감지 및 완전한 MODBUS 패킷 추출을 위한 순환 버퍼입니다.
    ///     Secondary analysis buffer. Ring buffer for frame boundary detection and complete MODBUS packet extraction.
    /// </summary>
    /// <remarks>
    ///     ReceiveBuf의 데이터를 모아서 함수 코드 기반으로 프레임 길이를 계산하고 완전한 패킷을 추출합니다.
    ///     Collects data from ReceiveBuf, calculates frame length based on function code, and extracts complete packets.
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
    ///     프로토콜 헤더 크기 (ID + FC)
    ///     Protocol header size (ID + FC)
    /// </summary>
    public int HeaderSize { get; } = 2;

    /// <summary>
    ///     함수 코드 위치 (헤더 내 오프셋)
    ///     Function code position (offset in header)
    /// </summary>
    public int FunctionPos { get; } = 1;

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
    ///     RTU 연결
    ///     Connect via RTU
    /// </summary>
    /// <param name="target">COM 포트 이름 / COM port name</param>
    /// <param name="option">보드레이트 / baud rate</param>
    /// <param name="id">장치 ID / device ID</param>
    /// <returns>연결 성공 여부 / connection success result</returns>
    public bool Connect(string target, int option, byte id = 1) {
        // 대상 포트 검증
        // check target port
        if (string.IsNullOrWhiteSpace(target))
            return false;
        // 보드레이트 검증
        // check baud rate
        if (!Constants.BaudRates.Contains(option))
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
            // 시리얼 포트 설정
            // set serial port
            Port.PortName        = target;
            Port.BaudRate        = option;
            Port.ReadBufferSize  = 16 * 1024;
            Port.WriteBufferSize = 16 * 1024;
            Port.Handshake       = Handshake.None;
            Port.Encoding        = Encoding.GetEncoding(@"iso-8859-1");
            // 포트 열기
            // open port
            Port.Open();
            // 장치 ID 설정
            // set device ID
            DeviceId = id;
            // 이벤트 설정
            // set event
            Port.DataReceived += PortOnDataReceived;

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

            // 연결 상태 변경 이벤트 발생
            // raise connection changed event
            ChangedConnect?.Invoke(true);

            // 성공
            // success
            return true;
        } catch (Exception e) {
            // 오류 출력
            // print error
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     RTU 연결 해제
    ///     Close RTU connection
    /// </summary>
    public void Close() {
        try {
            // 타이머 확인 및 정지
            // check and stop timer
            if (ProcessTimer.Enabled)
                IsStopTimer = true;
            // 포트 닫기
            // close port
            Port.Close();
            // 이벤트 해제
            // unsubscribe event
            Port.DataReceived -= PortOnDataReceived;
            // 연결 상태 변경 이벤트 발생
            // raise connection changed event
            ChangedConnect?.Invoke(false);
        } catch (Exception ex) {
            // 오류 출력
            // print error
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    ///     패킷 데이터 전송
    ///     Write packet data
    /// </summary>
    /// <param name="packet">패킷 데이터 / packet data</param>
    /// <param name="length">전송 길이 / transmit length</param>
    /// <returns>전송 성공 여부 / transmission success result</returns>
    public bool Write(byte[] packet, int length) {
        // 포트 연결 상태 확인
        // check port connection state
        if (!Port.IsOpen)
            return false;

        try {
            // 패킷 전송
            // send packet
            Port.Write(packet, 0, length);
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
        var packet = GC.AllocateUninitializedArray<byte>(8);
        // 헤더 및 데이터 설정
        // set header and data
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.ReadHoldingReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);

        // CRC 계산 및 설정
        // calculate and set CRC
        var p = packet.AsSpan();
        Utils.CalculateCrcTo(p[..6], p[6..]);
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
        var packet = GC.AllocateUninitializedArray<byte>(8);
        // 헤더 및 데이터 설정
        // set header and data
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.ReadInputReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);

        // CRC 계산 및 설정
        // calculate and set CRC
        var p = packet.AsSpan();
        Utils.CalculateCrcTo(p[..6], p[6..]);
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
        var packet = GC.AllocateUninitializedArray<byte>(8);
        // 헤더 및 데이터 설정
        // set header and data
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.WriteSingleReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((value >> 8) & 0xFF);
        packet[5] = (byte)(value        & 0xFF);

        // CRC 계산 및 설정
        // calculate and set CRC
        var p = packet.AsSpan();
        Utils.CalculateCrcTo(p[..6], p[6..]);
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
        var index = 7;
        // 레지스터 수 계산
        // calculate register count
        var count = values.Length;
        // 패킷 크기 계산
        // calculate packet size
        var size = index + count * 2 + 2;
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(size);
        // 헤더 설정
        // set header
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.WriteMultiReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);
        packet[6] = (byte)(count * 2);

        // 데이터 값 설정
        // set data values
        foreach (var value in values) {
            packet[index++] = (byte)((value >> 8) & 0xFF);
            packet[index++] = (byte)(value        & 0xFF);
        }

        // CRC 계산 및 설정
        // calculate and set CRC
        var p = packet.AsSpan();
        Utils.CalculateCrcTo(p[..^2], p[^2..]);
        return packet;
    }

    /// <summary>
    ///     문자열 레지스터 쓰기 패킷 생성
    ///     Create write string register packet
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="str">문자열 / string</param>
    /// <param name="length">문자열 길이 / string length</param>
    /// <returns>패킷 데이터 / packet data</returns>
    public byte[] SetMultiRegStrPacket(ushort addr, string str, int length) {
        var index = 7;
        // 길이 검증 및 조정
        // validate and adjust length
        if (length < str.Length)
            length = str.Length;
        // 레지스터 수 계산
        // calculate register count
        var count = length / 2;
        // 패킷 크기 계산
        // calculate packet size
        var size = index + count * 2 + 2;
        // 패킷 메모리 할당
        // allocate packet memory
        var packet = GC.AllocateUninitializedArray<byte>(size);
        // 헤더 설정
        // set header
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.WriteMultiReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);
        packet[6] = (byte)length;
        // 문자열 값 설정
        // set string values
        foreach (var c in str)
            packet[index++] = (byte)c;

        // 패딩 0 설정
        // set padding zeros
        while (index < 7 + length)
            packet[index++] = 0;

        // CRC 계산 및 설정
        // calculate and set CRC
        var p = packet.AsSpan();
        Utils.CalculateCrcTo(p[..^2], p[^2..]);
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
        var packet = GC.AllocateUninitializedArray<byte>(4);
        // 헤더 및 데이터 설정
        // set header and data
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.ReadInfoReg;

        // CRC 계산 및 설정
        // calculate and set CRC
        var p = packet.AsSpan();
        Utils.CalculateCrcTo(p[..2], p[2..]);
        return packet;
    }

    /// <summary>
    ///     사용 가능한 시리얼 포트 목록 조회
    ///     Get available serial port names
    /// </summary>
    /// <returns>포트 목록 / port list</returns>
    public static IEnumerable<string> GetPortNames() {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    ///     지원 보드레이트 목록 조회
    ///     Get supported baud rate list
    /// </summary>
    /// <returns>보드레이트 목록 / baud rate list</returns>
    public static IEnumerable<int> GetBaudRates() {
        return BaudRates;
    }

    /// <summary>
    ///     데이터 수신 이벤트 핸들러
    ///     Data received event handler
    /// </summary>
    /// <param name="sender">이벤트 발생 객체 / event sender</param>
    /// <param name="e">시리얼 데이터 수신 이벤트 인자 / serial data received event args</param>
    private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e) {
        // 무한 루프로 데이터 수신
        // receive data in infinite loop
        while (true) {
            // 수신 대기 길이 확인
            // check bytes to read
            var length = Port.BytesToRead;
            // 길이 확인
            // check length
            if (length < 1)
                break;
            // 풀에서 버퍼 대여
            // rent buffer from pool
            var chunk = ArrayPool<byte>.Shared.Rent(length);
            // 데이터 읽기
            // read data
            var read = Port.Read(chunk, 0, length);
            // 읽은 길이 확인
            // check read length
            if (read < 1) {
                // 풀에 반환
                // return to pool
                ArrayPool<byte>.Shared.Return(chunk);
                break;
            }

            // 큐에 데이터 블록 추가
            // enqueue data block
            ReceiveBuf.Enqueue((chunk, read));
            // 원시 데이터 이벤트 확인
            // check raw data event
            if (ReceivedRaw is null)
                continue;
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

            // 헤더 길이 확인 [ID(1) FUNC(1)]
            // check header length [ID(1) FUNC(1)]
            if (AnalyzeBuf.Available < HeaderSize)
                return;
            // 장치 ID 확인
            // check device ID
            if (AnalyzeBuf.Peek(0) != DeviceId)
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
                // 읽기 응답: ID(1) + FC(1) + LEN(1)=DATA_LEN + DATA(N) + CRC(2)
                // read response: ID(1) + FC(1) + LEN(1)=DATA_LEN + DATA(N) + CRC(2)
                CodeTypes.ReadHoldingReg or CodeTypes.ReadInputReg or CodeTypes.ReadInfoReg =>
                    AnalyzeBuf.Peek(HeaderSize) + 5,
                // 쓰기 응답: ID(1) + FC(1) + ADDR(2) + VALUE(2)/COUNT(2) + CRC(2)
                // write response: ID(1) + FC(1) + ADDR(2) + VALUE(2)/COUNT(2) + CRC(2)
                CodeTypes.WriteSingleReg or CodeTypes.WriteMultiReg => 8,
                // 그래프: ID(1) + FC(1) + LEN(2)=DATA_LEN + DATA(N) + CRC(2)
                // graph: ID(1) + FC(1) + LEN(2)=DATA_LEN + DATA(N) + CRC(2)
                CodeTypes.Graph or CodeTypes.GraphRes =>
                    ((AnalyzeBuf.Peek(HeaderSize) << 8) | AnalyzeBuf.Peek(HeaderSize + 1)) + 6,
                // 오류: ID(1) + FC(1) + ERROR(1) + CRC(2)
                // error: ID(1) + FC(1) + ERROR(1) + CRC(2)
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
            // CRC 검증
            // validate CRC
            if (Utils.ValidateCrc(packet))
                // 수신 이벤트 발생
                // raise receive event
                ReceivedData?.Invoke(cmd, packet);
            else
                // 오류 이벤트 발생
                // raise error event
                ReceivedError?.Invoke(ComErrorTypes.InvalidCrc);

            // 분석 시간 갱신
            // update analyze time
            AnalyzeTimeout = DateTime.Now;
        } finally {
            // 모니터 종료
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}