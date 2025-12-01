using HTool.Type;

namespace HTool.Device;

/// <summary>
///     MODBUS 프로토콜 구현을 위한 통신 도구 인터페이스. RTU 및 TCP 프로토콜 구현체가 이 인터페이스를 구현합니다.
///     Communication tool interface for MODBUS protocol implementation. RTU and TCP protocol implementations implement
///     this interface.
/// </summary>
/// <remarks>
///     이 인터페이스는 HcRtu (MODBUS-RTU/Serial)와 HcTcp (MODBUS-TCP/Ethernet) 클래스에 의해 구현됩니다.
///     This interface is implemented by HcRtu (MODBUS-RTU/Serial) and HcTcp (MODBUS-TCP/Ethernet) classes.
/// </remarks>
public interface ITool {
    /// <summary>
    ///     연결 상태 변경 델리게이트
    ///     Connection state changed delegate
    /// </summary>
    /// <param name="state">연결 상태 (true: 연결됨, false: 해제됨) / connection state (true: connected, false: disconnected)</param>
    delegate void PerformConnect(bool state);

    /// <summary>
    ///     원시 데이터 수신/전송 델리게이트
    ///     Raw data received/transmitted delegate
    /// </summary>
    /// <param name="packet">원시 패킷 데이터 / raw packet data</param>
    delegate void PerformRawData(byte[] packet);

    /// <summary>
    ///     파싱된 데이터 수신 델리게이트
    ///     Parsed data received delegate
    /// </summary>
    /// <param name="codeTypes">MODBUS 함수 코드 / MODBUS function code</param>
    /// <param name="packet">수신 패킷 데이터 / received packet data</param>
    delegate void PerformReceiveData(CodeTypes codeTypes, byte[] packet);

    /// <summary>
    ///     수신 오류 델리게이트
    ///     Receive error delegate
    /// </summary>
    /// <param name="reason">오류 원인 타입 / error reason type</param>
    /// <param name="param">추가 오류 파라미터 / additional error parameter</param>
    delegate void PerformReceiveError(ComErrorTypes reason, object? param = null);

    /// <summary>
    ///     프로토콜 헤더 크기 (바이트)
    ///     Protocol header size (bytes)
    /// </summary>
    int HeaderSize { get; }

    /// <summary>
    ///     함수 코드 위치 (헤더 내 오프셋)
    ///     Function code position (offset in header)
    /// </summary>
    int FunctionPos { get; }

    /// <summary>
    ///     장치 식별자 번호 (MODBUS 슬레이브 ID)
    ///     Device identifier number (MODBUS slave ID)
    /// </summary>
    byte DeviceId { get; set; }

    /// <summary>
    ///     도구 프로토콜 세대 (Gen.1, Gen.2 등)
    ///     Tool protocol revision (Gen.1, Gen.2, etc.)
    /// </summary>
    GenerationTypes Revision { get; set; }

    /// <summary>
    ///     연결 상태 변경 이벤트
    ///     Connection state changed event
    /// </summary>
    event PerformConnect ChangedConnect;

    /// <summary>
    ///     파싱된 데이터 수신 이벤트
    ///     Parsed data received event
    /// </summary>
    event PerformReceiveData ReceivedData;

    /// <summary>
    ///     수신 오류 이벤트
    ///     Receive error event
    /// </summary>
    event PerformReceiveError ReceivedError;

    /// <summary>
    ///     원시 데이터 수신 이벤트
    ///     Raw data received event
    /// </summary>
    event PerformRawData ReceivedRaw;

    /// <summary>
    ///     원시 데이터 전송 이벤트
    ///     Raw data transmitted event
    /// </summary>
    event PerformRawData TransmitRaw;

    /// <summary>
    ///     MODBUS 장치에 연결합니다. RTU는 시리얼 포트, TCP는 IP 주소를 사용합니다.
    ///     Connects to MODBUS device. RTU uses serial port, TCP uses IP address.
    /// </summary>
    /// <param name="target">
    ///     연결 대상. RTU: COM 포트 이름 (예: "COM3"), TCP: IP 주소 (예: "192.168.1.100")
    ///     Connection target. RTU: COM port name (e.g., "COM3"), TCP: IP address (e.g., "192.168.1.100")
    /// </param>
    /// <param name="option">
    ///     연결 옵션. RTU: 보드레이트 (9600-230400), TCP: 포트 번호 (기본 5000)
    ///     Connection option. RTU: baud rate (9600-230400), TCP: port number (default 5000)
    /// </param>
    /// <param name="id">MODBUS 장치 ID (슬레이브 주소, 1-247) / MODBUS device ID (slave address, 1-247)</param>
    /// <returns>
    ///     연결 시작 성공 여부. 실제 연결 완료는 ChangedConnect 이벤트로 확인 / Connection initiation success. Actual connection confirmed via
    ///     ChangedConnect event
    /// </returns>
    bool Connect(string target, int option, byte id = 0x01);

    /// <summary>
    ///     장치 연결 해제
    ///     Close device connection
    /// </summary>
    void Close();

    /// <summary>
    ///     패킷 전송
    ///     Write packet
    /// </summary>
    /// <param name="packet">패킷 데이터 / packet data</param>
    /// <param name="length">전송 길이 / transmit length</param>
    /// <returns>전송 성공 여부 / transmission success result</returns>
    bool Write(byte[] packet, int length);

    /// <summary>
    ///     홀딩 레지스터 읽기 MODBUS 패킷 생성 (함수 코드 0x03). 읽기/쓰기 가능한 레지스터 영역에서 데이터를 읽습니다.
    ///     Creates MODBUS packet for reading holding registers (function code 0x03). Reads data from read/write register area.
    /// </summary>
    /// <param name="addr">시작 레지스터 주소 (0-65535) / Starting register address (0-65535)</param>
    /// <param name="count">읽을 레지스터 수 (1-125) / Number of registers to read (1-125)</param>
    /// <returns>CRC (RTU) 또는 MBAP 헤더 (TCP)가 포함된 완전한 MODBUS 패킷 / Complete MODBUS packet with CRC (RTU) or MBAP header (TCP)</returns>
    byte[] GetReadHoldingRegPacket(ushort addr, ushort count);

    /// <summary>
    ///     입력 레지스터 읽기 MODBUS 패킷 생성 (함수 코드 0x04). 읽기 전용 레지스터 영역에서 데이터를 읽습니다.
    ///     Creates MODBUS packet for reading input registers (function code 0x04). Reads data from read-only register area.
    /// </summary>
    /// <param name="addr">시작 레지스터 주소 (0-65535) / Starting register address (0-65535)</param>
    /// <param name="count">읽을 레지스터 수 (1-125) / Number of registers to read (1-125)</param>
    /// <returns>CRC (RTU) 또는 MBAP 헤더 (TCP)가 포함된 완전한 MODBUS 패킷 / Complete MODBUS packet with CRC (RTU) or MBAP header (TCP)</returns>
    byte[] GetReadInputRegPacket(ushort addr, ushort count);

    /// <summary>
    ///     단일 레지스터 쓰기 MODBUS 패킷 생성 (함수 코드 0x06). 하나의 레지스터에 16비트 값을 씁니다.
    ///     Creates MODBUS packet for writing single register (function code 0x06). Writes 16-bit value to one register.
    /// </summary>
    /// <param name="addr">쓸 레지스터 주소 (0-65535) / Register address to write (0-65535)</param>
    /// <param name="value">쓸 16비트 값 / 16-bit value to write</param>
    /// <returns>CRC (RTU) 또는 MBAP 헤더 (TCP)가 포함된 완전한 MODBUS 패킷 / Complete MODBUS packet with CRC (RTU) or MBAP header (TCP)</returns>
    byte[] SetSingleRegPacket(ushort addr, ushort value);

    /// <summary>
    ///     다중 레지스터 쓰기 MODBUS 패킷 생성 (함수 코드 0x10). 연속된 여러 레지스터에 값을 씁니다.
    ///     Creates MODBUS packet for writing multiple registers (function code 0x10). Writes values to consecutive registers.
    /// </summary>
    /// <param name="addr">시작 레지스터 주소 (0-65535) / Starting register address (0-65535)</param>
    /// <param name="values">쓸 16비트 값 배열 (최대 123개) / Array of 16-bit values to write (max 123)</param>
    /// <returns>CRC (RTU) 또는 MBAP 헤더 (TCP)가 포함된 완전한 MODBUS 패킷 / Complete MODBUS packet with CRC (RTU) or MBAP header (TCP)</returns>
    byte[] SetMultiRegPacket(ushort addr, ReadOnlySpan<ushort> values);

    /// <summary>
    ///     문자열을 레지스터에 쓰는 MODBUS 패킷 생성. ASCII 문자열을 레지스터 배열로 변환합니다 (2바이트/레지스터).
    ///     Creates MODBUS packet for writing string to registers. Converts ASCII string to register array (2 bytes/register).
    /// </summary>
    /// <param name="addr">시작 레지스터 주소 (0-65535) / Starting register address (0-65535)</param>
    /// <param name="str">쓸 ASCII 문자열 / ASCII string to write</param>
    /// <param name="length">총 바이트 길이 (패딩 포함). 0이면 문자열 길이 사용 / Total byte length (including padding). 0 uses string length</param>
    /// <returns>CRC (RTU) 또는 MBAP 헤더 (TCP)가 포함된 완전한 MODBUS 패킷 / Complete MODBUS packet with CRC (RTU) or MBAP header (TCP)</returns>
    byte[] SetMultiRegStrPacket(ushort addr, string str, int length);

    /// <summary>
    ///     장치 정보 읽기 MODBUS 패킷 생성 (함수 코드 0x11, HANTAS 커스텀). 시리얼 번호, 펌웨어 버전 등 장치 정보를 읽습니다.
    ///     Creates MODBUS packet for reading device info (function code 0x11, HANTAS custom). Reads device info like serial
    ///     number, firmware version.
    /// </summary>
    /// <returns>
    ///     CRC (RTU) 또는 MBAP 헤더 (TCP)가 포함된 완전한 MODBUS 패킷. 응답은 Gen.1에서 13바이트, Gen.2에서 200바이트
    ///     Complete MODBUS packet with CRC (RTU) or MBAP header (TCP). Response is 13 bytes for Gen.1, 200 bytes for Gen.2
    /// </returns>
    byte[] GetInfoRegPacket();
}