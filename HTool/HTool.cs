using System.Timers;
using HTool.Data;
using HTool.Device;
using HTool.Format;
using HTool.Type;
using HTool.Util;
using Timer = System.Timers.Timer;

namespace HTool;

/// <summary>
///     HTool 라이브러리의 메인 클래스. HANTAS 토크 도구와의 MODBUS 통신을 관리하는 고수준 API를 제공합니다.
///     Main class of HTool library. Provides high-level API for managing MODBUS communication with HANTAS torque tools.
/// </summary>
/// <remarks>
///     <para>주요 기능 / Key Features:</para>
///     <list type="bullet">
///         <item>MODBUS RTU/TCP 프로토콜 지원 / Supports MODBUS RTU/TCP protocols</item>
///         <item>자동 메시지 큐 관리 및 중복 방지 / Automatic message queue management with duplicate prevention</item>
///         <item>대용량 레지스터 읽기/쓰기 자동 분할 (125/123 레지스터) / Auto-splits large read/write operations (125/123 registers)</item>
///         <item>타임아웃 및 자동 재시도 / Timeout and automatic retry</item>
///         <item>Keep-Alive 메커니즘 (3초 주기, 10초 타임아웃) / Keep-Alive mechanism (3s period, 10s timeout)</item>
///         <item>이벤트 기반 비동기 통신 / Event-driven asynchronous communication</item>
///     </list>
/// </remarks>
public sealed class HTool {
    /// <summary>
    ///     연결 상태 변경 델리게이트
    ///     Connection state changed delegate
    /// </summary>
    /// <param name="state">연결 상태 (true: 연결됨, false: 연결 해제) / connection state (true: connected, false: disconnected)</param>
    public delegate void PerformChangedConnect(bool state);

    /// <summary>
    ///     원시 데이터 수신 델리게이트
    ///     Raw data received delegate
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    public delegate void PerformRawData(byte[] data);

    /// <summary>
    ///     파싱된 데이터 수신 델리게이트
    ///     Parsed data received delegate
    /// </summary>
    /// <param name="codeTypes">MODBUS 함수 코드 / MODBUS function code</param>
    /// <param name="addr">레지스터 주소 / register address</param>
    /// <param name="data">수신 데이터 / received data</param>
    public delegate void PerformReceivedData(CodeTypes codeTypes, int addr, IReceivedData data);

    /// <summary>
    ///     기본 생성자
    ///     Default constructor
    /// </summary>
    public HTool() {
        // 타이머 옵션 설정
        // set timer option
        ProcessTimer.Interval  =  Constants.ProcessPeriod;
        ProcessTimer.AutoReset =  true;
        ProcessTimer.Elapsed   += OnElapsed;
    }

    /// <summary>
    ///     통신 타입을 지정하는 생성자
    ///     Constructor with communication type
    /// </summary>
    /// <param name="type">통신 타입 (RTU 또는 TCP) / communication type (RTU or TCP)</param>
    public HTool(ComTypes type) : this() {
        // 통신 타입 설정
        // set the communication type
        SetType(type);
    }

    /// <summary>
    ///     통신 도구 인스턴스
    ///     Communication tool instance
    /// </summary>
    private ITool? Tool { get; set; }

    /// <summary>
    ///     메시지 큐 처리 타이머. 20ms 주기로 큐에서 메시지를 추출하여 전송하고 타임아웃을 관리합니다.
    ///     Message queue processing timer. Extracts and sends messages from queue every 20ms, manages timeouts.
    /// </summary>
    /// <remarks>
    ///     ProcessTimer는 메시지 전송, 타임아웃 확인, 재시도, Keep-Alive 등의 핵심 기능을 담당합니다.
    ///     ProcessTimer handles core functions: message transmission, timeout checking, retry, and keep-alive.
    /// </remarks>
    private Timer ProcessTimer { get; } = new();

    /// <summary>
    ///     메시지 전송 큐. 중복 방지 기능이 있는 KeyedQueue를 사용하여 동일 주소/함수 코드 메시지가 큐에 중복 추가되지 않도록 합니다.
    ///     Message transmission queue. Uses KeyedQueue with duplicate prevention to avoid adding duplicate messages with same
    ///     address/function code.
    /// </summary>
    /// <remarks>
    ///     최대 64개 메시지 저장 가능. 메시지 키는 (FunctionCode, Address)로 구성되어 중복을 식별합니다.
    ///     Capacity of 64 messages. Message key consists of (FunctionCode, Address) to identify duplicates.
    /// </remarks>
    private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue { get; } =
        KeyedQueue<FormatMessage, FormatMessage.MessageKey>.Create(static m => m.Key, capacity: 64);

    /// <summary>
    ///     연결 시작 시간
    ///     Connection start time
    /// </summary>
    private DateTime ConnectionTime { get; set; }

    /// <summary>
    ///     마지막 Keep-Alive 요청 시간
    ///     Last keep-alive request time
    /// </summary>
    private DateTime KeepAliveRequestTime { get; set; } = DateTime.Now;

    /// <summary>
    ///     마지막 Keep-Alive 응답 수신 시간
    ///     Last keep-alive response received time
    /// </summary>
    private DateTime KeepAliveTime { get; set; } = DateTime.Now;

    /// <summary>
    ///     통신 타입 (RTU 또는 TCP)
    ///     Communication type (RTU or TCP)
    /// </summary>
    public ComTypes Type { get; private set; }

    /// <summary>
    ///     연결 상태
    ///     Connection state
    /// </summary>
    public ConnectionTypes ConnectionState { get; set; }

    /// <summary>
    ///     도구 프로토콜 세대 (Gen.1, Gen.2 등)
    ///     Tool protocol generation (Gen.1, Gen.2, etc.)
    /// </summary>
    public GenerationTypes Gen { get; private set; } = GenerationTypes.GenRev2;

    /// <summary>
    ///     장치 기본 정보
    ///     Device basic information
    /// </summary>
    public FormatSimpleInfo Info { get; private set; } = new();

    /// <summary>
    ///     Keep-Alive 기능 활성화 여부. 활성화 시 3초마다 장치 정보 요청을 보내 연결을 유지하고, 10초 응답 없으면 자동 종료합니다.
    ///     Keep-Alive feature enabled flag. When enabled, sends device info request every 3s to maintain connection,
    ///     auto-disconnects after 10s no response.
    /// </summary>
    /// <remarks>
    ///     장기 연결 모니터링 애플리케이션에 권장. 짧은 작업에는 불필요한 트래픽을 유발할 수 있습니다.
    ///     Recommended for long-running monitoring applications. May cause unnecessary traffic for short operations.
    /// </remarks>
    public bool EnableKeepAlive { get; set; }

    /// <summary>
    ///     한 번에 읽을 수 있는 최대 레지스터 수. MODBUS 프로토콜 제약사항으로 인한 제한값입니다.
    ///     Maximum register count for single read operation. Limited by MODBUS protocol constraints.
    /// </summary>
    /// <remarks>
    ///     이 값을 초과하는 읽기 요청은 자동으로 여러 메시지로 분할됩니다.
    ///     Read requests exceeding this value are automatically split into multiple messages.
    /// </remarks>
    public static int ReadRegMaxSize => 125;

    /// <summary>
    ///     한 번에 쓸 수 있는 최대 레지스터 수. MODBUS 프로토콜 제약사항으로 인한 제한값입니다.
    ///     Maximum register count for single write operation. Limited by MODBUS protocol constraints.
    /// </summary>
    /// <remarks>
    ///     이 값을 초과하는 쓰기 요청은 자동으로 여러 메시지로 분할됩니다.
    ///     Write requests exceeding this value are automatically split into multiple messages.
    /// </remarks>
    public static int WriteRegMaxSize => 123;

    /// <summary>
    ///     연결 상태 변경 이벤트
    ///     Connection state changed event
    /// </summary>
    public event PerformChangedConnect? ChangedConnect;

    /// <summary>
    ///     파싱된 데이터 수신 이벤트
    ///     Parsed data received event
    /// </summary>
    public event PerformReceivedData? ReceivedData;

    /// <summary>
    ///     수신 오류 이벤트
    ///     Receive error event
    /// </summary>
    public event ITool.PerformReceiveError? ReceiveError;

    /// <summary>
    ///     원시 데이터 수신 이벤트
    ///     Raw data received event
    /// </summary>
    public event PerformRawData? ReceivedRawData;

    /// <summary>
    ///     원시 데이터 전송 이벤트
    ///     Raw data transmitted event
    /// </summary>
    public event PerformRawData? TransmitRawData;

    /// <summary>
    ///     통신 타입 설정
    ///     Set the communication type
    /// </summary>
    /// <param name="type">통신 타입 (RTU 또는 TCP) / communication type (RTU or TCP)</param>
    public void SetType(ComTypes type) {
        // 기존 통신 도구 확인
        // check existing communication tool
        if (Tool != null) {
            // 연결 중이면 변경 불가
            // cannot change while connected
            if (ConnectionState == ConnectionTypes.Connected)
                return;
            // 이벤트 해제
            // unsubscribe events
            Tool.ChangedConnect -= OnChangedConnect;
            Tool.ReceivedData   -= OnReceivedData;
            Tool.ReceivedRaw    -= OnReceivedRaw;
            Tool.TransmitRaw    -= OnTransmitRaw;
            // 도구 해제
            // dispose tool
            Tool = null;
        }

        try {
            // 통신 타입 설정
            // set communication type
            Type = type;
            // 통신 도구 생성
            // create communication tool
            Tool = Device.Tool.Create(type);
            // 도구 생성 확인
            // check tool creation
            if (Tool == null)
                throw new Exception("Unable to create a Tool communication object.");
            // 이벤트 등록
            // subscribe events
            Tool.ChangedConnect += OnChangedConnect;
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    ///     장치에 연결
    ///     Connect to the device
    /// </summary>
    /// <param name="target">대상 (COM 포트 또는 IP 주소) / target (COM port or IP address)</param>
    /// <param name="option">옵션 (보드레이트 또는 포트) / option (baud rate or port)</param>
    /// <param name="id">장치 ID / device ID</param>
    /// <returns>연결 성공 여부 / connection success result</returns>
    public bool Connect(string target, int option, byte id = 0x01) {
        try {
            // 통신 도구 확인
            // check communication tool
            if (Tool == null)
                return false;
            // 연결 시도
            // attempt connection
            if (!Tool.Connect(target, option, id))
                return false;
            // 메시지 큐 초기화
            // clear message queue
            MessageQue.Clear();
            // 연결 중 상태로 변경
            // change to connecting state
            ConnectionState = ConnectionTypes.Connecting;
            // 연결 시작 시간 기록
            // record connection start time
            ConnectionTime = DateTime.Now;
            // 이벤트 등록
            // subscribe events
            Tool.ReceivedData  += OnReceivedData;
            Tool.ReceivedError += OnReceivedError;
            Tool.ReceivedRaw   += OnReceivedRaw;
            Tool.TransmitRaw   += OnTransmitRaw;
            // 처리 타이머 시작
            // start process timer
            ProcessTimer.Start();
            return true;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     장치 연결 해제
    ///     Close the device connection
    /// </summary>
    public void Close() {
        // 타이머 정지
        // stop timer
        ProcessTimer.Stop();
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return;
        // 장치 정보 초기화
        // reset device information
        Info = new FormatSimpleInfo();
        // 이벤트 해제
        // unsubscribe events
        Tool.ReceivedData  -= OnReceivedData;
        Tool.ReceivedError -= OnReceivedError;
        Tool.ReceivedRaw   -= OnReceivedRaw;
        Tool.TransmitRaw   -= OnTransmitRaw;
        // 연결 종료
        // close connection
        Tool.Close();
    }

    /// <summary>
    ///     메시지 큐에 단일 메시지 추가
    ///     Insert single message to queue
    /// </summary>
    /// <param name="msg">메시지 / message</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>추가 성공 여부 / insert success result</returns>
    private bool Insert(FormatMessage msg, bool check = true) {
        // 중복 확인 모드 설정
        // set duplicate check mode
        var mode = check ? EnqueueMode.EnforceUnique : EnqueueMode.AllowDuplicate;
        // 큐에 추가
        // enqueue message
        return MessageQue.TryEnqueue(msg, mode);
    }

    /// <summary>
    ///     메시지 큐에 여러 메시지 추가
    ///     Insert multiple messages to queue
    /// </summary>
    /// <param name="messages">메시지 목록 / message list</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>추가 성공 여부 / insert success result</returns>
    private bool InsertRange(IReadOnlyList<FormatMessage> messages, bool check = true) {
        // 중복 확인 모드 설정
        // set duplicate check mode
        var mode = check ? EnqueueMode.EnforceUnique : EnqueueMode.AllowDuplicate;
        // 큐에 추가
        // enqueue messages
        return MessageQue.TryEnqueueRange(messages, mode).Accepted > 0;
    }

    /// <summary>
    ///     홀딩 레지스터 읽기 (MODBUS 함수 코드 0x03)
    ///     Read holding registers (MODBUS function code 0x03)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">읽을 레지스터 수 / register count to read</param>
    /// <param name="split">분할 단위 (0=자동) / split size (0=auto)</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool ReadHoldingReg(ushort addr, ushort count, int split = 0, bool check = true) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return false;
        // 연결 상태 확인
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;
        // 개수 확인
        // check count
        if (count == 0)
            return true;

        var address = addr;
        // 분할 단위 확인
        // check split count
        if (split <= 0)
            split = ReadRegMaxSize;
        // 블록 수 계산
        // calculate block count
        var block = (count + split - 1) / split;
        // 메시지 목록 생성
        // create message list
        var messages = new List<FormatMessage>(block);
        // 블록별 메시지 생성
        // create message for each block
        for (var i = 0; i < block; i++) {
            // 남은 수량 계산
            // calculate remaining count
            var remaining = count - i * split;
            // 요청 수량 결정
            // determine request count
            var request = (ushort)Math.Min(split, remaining);
            // 메시지 추가
            // add message
            messages.Add(new FormatMessage(CodeTypes.ReadHoldingReg, address, Tool.GetReadHoldingRegPacket(address, request)));
            // 주소 갱신
            // update address
            address += request;
        }

        // 메시지 삽입
        // insert messages
        return InsertRange(messages, check);
    }

    /// <summary>
    ///     입력 레지스터 읽기 (MODBUS 함수 코드 0x04)
    ///     Read input registers (MODBUS function code 0x04)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="count">읽을 레지스터 수 / register count to read</param>
    /// <param name="split">분할 단위 (0=자동) / split size (0=auto)</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool ReadInputReg(ushort addr, ushort count, int split = 0, bool check = true) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return false;
        // 연결 상태 확인
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;
        // 개수 확인
        // check count
        if (count == 0)
            return true;

        var address = addr;
        // 분할 단위 확인
        // check split count
        if (split <= 0)
            split = ReadRegMaxSize;
        // 블록 수 계산
        // calculate block count
        var block = (count + split - 1) / split;
        // 메시지 목록 생성
        // create message list
        var messages = new List<FormatMessage>(block);
        // 블록별 메시지 생성
        // create message for each block
        for (var i = 0; i < block; i++) {
            // 남은 수량 계산
            // calculate remaining count
            var remaining = count - i * split;
            // 요청 수량 결정
            // determine request count
            var request = (ushort)Math.Min(split, remaining);
            // 메시지 추가
            // add message
            messages.Add(new FormatMessage(CodeTypes.ReadInputReg, address, Tool.GetReadInputRegPacket(address, request)));
            // 주소 갱신
            // update address
            address += request;
        }

        // 메시지 삽입
        // insert messages
        return InsertRange(messages, check);
    }

    /// <summary>
    ///     단일 레지스터 쓰기 (MODBUS 함수 코드 0x06)
    ///     Write single register (MODBUS function code 0x06)
    /// </summary>
    /// <param name="addr">레지스터 주소 / register address</param>
    /// <param name="value">쓸 값 / value to write</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool WriteSingleReg(ushort addr, ushort value, bool check = true) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return false;
        // 연결 상태 확인
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;

        // 메시지 생성
        // create message
        var msg = new FormatMessage(CodeTypes.WriteSingleReg, addr, Tool.SetSingleRegPacket(addr, value));
        // 메시지 삽입
        // insert message
        return Insert(msg, check);
    }

    /// <summary>
    ///     다중 레지스터 쓰기 (MODBUS 함수 코드 0x10)
    ///     Write multiple registers (MODBUS function code 0x10)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="values">쓸 값 배열 / values array to write</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool WriteMultiReg(ushort addr, ushort[] values, bool check = true) {
        return WriteMultiReg(addr, values.AsSpan(), check);
    }

    /// <summary>
    ///     다중 레지스터 쓰기 (MODBUS 함수 코드 0x10)
    ///     Write multiple registers (MODBUS function code 0x10)
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="values">쓸 값 스팬 / values span to write</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool WriteMultiReg(ushort addr, ReadOnlySpan<ushort> values, bool check = true) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return false;
        // 연결 상태 확인
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;
        // 값 배열 확인
        // check values array
        if (values.Length == 0)
            return true;

        var offset   = 0;
        var total    = values.Length;
        var blocks   = (total + WriteRegMaxSize - 1) / WriteRegMaxSize;
        var messages = new List<FormatMessage>(blocks);
        // 오프셋별 메시지 생성
        // create message for each offset
        while (offset < total) {
            // 길이 계산
            // calculate length
            var len = Math.Min(total - offset, WriteRegMaxSize);
            // 주소 계산
            // calculate address
            var address = (ushort)(addr + offset);
            // 버퍼 생성
            // create buffer
            var buf = GC.AllocateUninitializedArray<ushort>(len);
            // 버퍼에 복사
            // copy to buffer
            values.Slice(offset, len).CopyTo(buf);
            // 패킷 생성
            // create packet
            var packet = Tool.SetMultiRegPacket(address, buf);
            // 메시지 추가
            // add message
            messages.Add(new FormatMessage(CodeTypes.WriteMultiReg, address, packet));

            // 오프셋 갱신
            // update offset
            offset += len;
        }

        // 메시지 삽입
        // insert messages
        return InsertRange(messages, check);
    }

    /// <summary>
    ///     문자열 레지스터 쓰기
    ///     Write string to registers
    /// </summary>
    /// <param name="addr">시작 주소 / start address</param>
    /// <param name="str">문자열 값 / string value</param>
    /// <param name="length">문자열 길이 (0=자동) / string length (0=auto)</param>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool WriteStrReg(ushort addr, string str, int length = 0, bool check = true) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return false;
        // 연결 상태 확인
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;

        // 길이 검증 및 조정
        // validate and adjust length
        if (length < str.Length)
            length = str.Length;
        // 메시지 생성
        // create message
        var msg = new FormatMessage(CodeTypes.WriteMultiReg, addr, Tool.SetMultiRegStrPacket(addr, str, length));
        // 메시지 삽입
        // insert message
        return Insert(msg, check);
    }

    /// <summary>
    ///     장치 정보 레지스터 읽기 (MODBUS 함수 코드 0x11)
    ///     Read device information register (MODBUS function code 0x11)
    /// </summary>
    /// <param name="check">중복 확인 여부 / check duplicate flag</param>
    /// <returns>요청 성공 여부 / request success result</returns>
    public bool ReadInfoReg(bool check = true) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return false;
        // 연결 상태 확인
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;

        // 메시지 생성
        // create message
        var msg = new FormatMessage(CodeTypes.ReadInfoReg, FormatMessage.EmptyAddr, Tool.GetInfoRegPacket());
        // 메시지 삽입
        // insert message
        return Insert(msg, check);
    }

    /// <summary>
    ///     메시지 처리 타이머 이벤트 핸들러
    ///     Message processing timer event handler
    /// </summary>
    /// <param name="sender">이벤트 발생 객체 / event sender</param>
    /// <param name="e">타이머 이벤트 인자 / timer event args</param>
    private void OnElapsed(object? sender, ElapsedEventArgs e) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return;
        // 상태별 처리
        // process by state
        switch (ConnectionState) {
            case ConnectionTypes.Connecting
                when (DateTime.Now - ConnectionTime).TotalSeconds < Constants.ConnectTimeout:
                // 정보 요청
                // request information
                Insert(new FormatMessage(CodeTypes.ReadInfoReg, FormatMessage.EmptyAddr, Tool.GetInfoRegPacket()));
                break;
            case ConnectionTypes.Connecting:
                // 연결 종료
                // close connection
                Close();
                break;
            case ConnectionTypes.Close:
            case ConnectionTypes.Connected:
                // Keep-Alive 활성화 확인
                // check keep-alive enabled
                if (EnableKeepAlive) {
                    // Keep-Alive 요청 시간 확인
                    // check keep-alive request time
                    if ((DateTime.Now - KeepAliveRequestTime).TotalMilliseconds >= Constants.KeepAlivePeriod)
                        // 큐 비어있는지 확인
                        // check queue empty
                        if (MessageQue.IsEmpty)
                            // Keep-Alive 메시지 삽입
                            // insert keep-alive message
                            if (ReadInfoReg())
                                // Keep-Alive 시간 갱신
                                // update keep-alive time
                                KeepAliveRequestTime = DateTime.Now;
                    // Keep-Alive 타임아웃 확인
                    // check keep-alive timeout
                    if ((DateTime.Now - KeepAliveTime).TotalSeconds >= Constants.KeepAliveTimeout)
                        // 연결 종료
                        // close connection
                        Close();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(string.Empty);
        }

        // 큐 비어있는지 확인
        // check queue empty
        if (MessageQue.IsEmpty)
            return;
        // 메시지 조회
        // peek message
        if (!MessageQue.TryPeek(out var msg))
            return;
        // 활성화 상태 확인
        // check activation state
        if (!msg.Activated) {
            // 패킷 데이터 가져오기
            // get packet data
            var packet = msg.Packet.ToArray();
            // 패킷 전송
            // send packet
            if (!Tool.Write(packet, packet.Length))
                return;
            // 메시지 활성화
            // activate message
            msg.Activate();
            // 응답 확인 안 함이면 종료
            // exit if no check required
            if (!msg.NotCheck)
                return;
        } else {
            // 타임아웃 확인
            // check timeout
            if ((DateTime.Now - msg.ActiveTime).TotalMilliseconds < Constants.MessageTimeout)
                return;
            // 메시지 비활성화 (재시도 가능)
            // deactivate message (allows retry)
            if (msg.Deactivate() > 0)
                return;
        }

        // 메시지 제거
        // remove message
        MessageQue.TryDequeue(out _);
    }

    /// <summary>
    ///     연결 상태 변경 이벤트 핸들러
    ///     Connection state changed event handler
    /// </summary>
    /// <param name="state">연결 상태 / connection state</param>
    private void OnChangedConnect(bool state) {
        // Keep-Alive 시간 갱신
        // update keep-alive time
        KeepAliveRequestTime = DateTime.Now;
        KeepAliveTime        = DateTime.Now;
        // 연결 상태 확인
        // check connection state
        if (state)
            return;
        // 상태 변경
        // change state
        ConnectionState = ConnectionTypes.Close;
        // 연결 상태 변경 이벤트 발생
        // raise connection state changed event
        ChangedConnect?.Invoke(false);
    }

    /// <summary>
    ///     데이터 수신 이벤트 핸들러
    ///     Data received event handler
    /// </summary>
    /// <param name="code">함수 코드 / function code</param>
    /// <param name="packet">패킷 데이터 / packet data</param>
    private void OnReceivedData(CodeTypes code, byte[] packet) {
        // 통신 도구 확인
        // check communication tool
        if (Tool == null)
            return;

        var addr = FormatMessage.EmptyAddr;
        // 메시지 조회
        // peek message
        if (MessageQue.TryPeek(out var msg))
            // 활성화된 메시지 확인
            // check activated message
            if (msg is { Activated: true })
                // 코드 일치 확인
                // check code match
                if (code == msg.Code || code == CodeTypes.Error) {
                    // 주소 설정
                    // set address
                    addr = msg.Address;
                    // 메시지 제거
                    // remove message
                    MessageQue.TryDequeue(out _);
                }

        // 수신 데이터 생성
        // create received data
        IReceivedData? data = Type switch {
            ComTypes.Rtu => new HcRtuData(packet),
            ComTypes.Tcp => new HcTcpData(packet),
            _            => null
        };

        // 장치 정보 읽기 응답 처리
        // process device info read response
        if (code == CodeTypes.ReadInfoReg && data != null) {
            // 장치 정보 설정
            // set device information
            Info = new FormatSimpleInfo(data.Data);
            // 연결 중 상태인 경우
            // if connecting state
            if (ConnectionState == ConnectionTypes.Connecting) {
                // 프로토콜 세대 설정
                // set protocol revision
                Tool.Revision = Info.Firmware switch {
                    > (int)GenerationTypes.GenRev2                                  => GenerationTypes.GenRev2,
                    > (int)GenerationTypes.GenRev1Plus                              => GenerationTypes.GenRev1Plus,
                    > (int)GenerationTypes.GenRev1 when Info.Model == ModelTypes.Ad => GenerationTypes.GenRev1Ad,
                    _                                                               => GenerationTypes.GenRev1
                };
                Gen = Tool.Revision;
                // 상태 변경
                // change state
                ConnectionState = ConnectionTypes.Connected;
                // 연결 상태 변경 이벤트 발생
                // raise connection state changed event
                ChangedConnect?.Invoke(true);
            }
        }

        // 수신 데이터 확인
        // check received data
        if (data == null)
            return;
        // 수신 이벤트 발생
        // raise receive event
        ReceivedData?.Invoke(code, addr, data);
        // Keep-Alive 활성화 확인
        // check keep-alive enabled
        if (!EnableKeepAlive)
            return;
        // Keep-Alive 시간 갱신
        // update keep-alive time
        KeepAliveTime = DateTime.Now;
    }

    /// <summary>
    ///     수신 오류 이벤트 핸들러
    ///     Receive error event handler
    /// </summary>
    /// <param name="reason">오류 사유 / error reason</param>
    /// <param name="param">추가 파라미터 / additional parameter</param>
    private void OnReceivedError(ComErrorTypes reason, object? param) {
        // 오류 이벤트 발생
        // raise error event
        ReceiveError?.Invoke(reason, param);
    }

    /// <summary>
    ///     원시 데이터 수신 이벤트 핸들러
    ///     Raw data received event handler
    /// </summary>
    /// <param name="packet">패킷 데이터 / packet data</param>
    private void OnReceivedRaw(byte[] packet) {
        // 수신 이벤트 발생
        // raise receive event
        ReceivedRawData?.Invoke(packet);
    }

    /// <summary>
    ///     원시 데이터 전송 이벤트 핸들러
    ///     Raw data transmit event handler
    /// </summary>
    /// <param name="packet">패킷 데이터 / packet data</param>
    private void OnTransmitRaw(byte[] packet) {
        // 전송 이벤트 발생
        // raise transmit event
        TransmitRawData?.Invoke(packet);
    }
}