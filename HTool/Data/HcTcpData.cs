using HTool.Type;

namespace HTool.Data;

/// <summary>
///     MODBUS-TCP 수신 데이터 파싱 클래스. TCP/IP 통신에서 받은 MBAP 헤더 포함 패킷을 구조화된 데이터로 변환합니다.
///     MODBUS-TCP received data parsing class. Converts MBAP header-included packets from TCP/IP communication into
///     structured data.
/// </summary>
/// <remarks>
///     TCP 프레임 구조 (MBAP): [Trans ID(2)] [Proto ID(2)] [Length(2)] [Unit ID(1)] [Function Code(1)] [Data(N)]
///     TCP frame structure (MBAP): [Trans ID(2)] [Proto ID(2)] [Length(2)] [Unit ID(1)] [Function Code(1)] [Data(N)]
/// </remarks>
public sealed class HcTcpData : IReceivedData {
    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    public HcTcpData(IReadOnlyList<byte> values) {
        // 수신 데이터 생성
        // create the received data
        Create(values);
    }

    /// <summary>
    ///     트랜잭션 ID (0-65535). 요청과 응답을 매칭하기 위한 식별자입니다. MBAP 헤더의 첫 2바이트입니다.
    ///     Transaction ID (0-65535). Identifier for matching requests with responses. First 2 bytes of MBAP header.
    /// </summary>
    public int TransactionId { get; private set; }

    /// <summary>
    ///     프로토콜 ID. MODBUS TCP의 경우 항상 0x0000입니다. MBAP 헤더의 3-4번째 바이트입니다.
    ///     Protocol ID. Always 0x0000 for MODBUS TCP. Bytes 3-4 of MBAP header.
    /// </summary>
    public int ProtocolId { get; private set; }

    /// <summary>
    ///     유닛 ID (1-247). MODBUS 슬레이브 주소로, RTU의 Device ID에 해당합니다. MBAP 헤더의 7번째 바이트입니다.
    ///     Unit ID (1-247). MODBUS slave address, equivalent to RTU's Device ID. 7th byte of MBAP header.
    /// </summary>
    public int UnitId { get; private set; }

    /// <summary>
    ///     프레임 길이 (Unit ID + Function Code + Data의 총 바이트 수). MBAP 헤더의 5-6번째 바이트입니다.
    ///     Frame length (total bytes of Unit ID + Function Code + Data). Bytes 5-6 of MBAP header.
    /// </summary>
    public int FrameLength { get; private set; }

    /// <summary>
    ///     MODBUS 함수 코드
    ///     MODBUS function code
    /// </summary>
    public CodeTypes Code { get; private set; }

    /// <summary>
    ///     데이터 길이 (바이트)
    ///     Data length (bytes)
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    ///     데이터 바이트 배열
    ///     Data byte array
    /// </summary>
    public byte[] Data { get; private set; } = null!;

    /// <summary>
    ///     원시 패킷에서 데이터 포맷 생성
    ///     Create data format from raw packet
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    public void Create(IReadOnlyList<byte> values) {
        var pos = 0;
        // MBAP 헤더 설정 (Big-Endian 2바이트씩)
        // set MBAP header (Big-Endian 2 bytes each)
        TransactionId = (values[pos++] << 8) | values[pos++];
        ProtocolId    = (values[pos++] << 8) | values[pos++];
        FrameLength   = (values[pos++] << 8) | values[pos++];
        UnitId        = values[pos++];
        Code          = (CodeTypes)values[pos++];
        // 오류 코드 확인
        // check error code
        if ((byte)((int)Code & (int)CodeTypes.Error) == (byte)CodeTypes.Error)
            // 오류 코드로 변경
            // change to error code
            Code = CodeTypes.Error;
        // 함수 코드별 처리
        // process by function code
        switch (Code) {
            case CodeTypes.ReadHoldingReg:
            case CodeTypes.ReadInputReg:
            case CodeTypes.ReadInfoReg:
                // 길이 가져오기 (다음 바이트)
                // get length (next byte)
                Length = values[pos++];
                // 데이터 배열 생성
                // create data array
                Data = new byte[Length];

                break;
            case CodeTypes.WriteSingleReg:
            case CodeTypes.WriteMultiReg:
                // 길이 설정 (주소 2바이트 + 값 2바이트)
                // set length (address 2 bytes + value 2 bytes)
                Length = 4;
                // 데이터 배열 생성
                // create data array
                Data = new byte[Length];

                break;
            case CodeTypes.Graph:
            case CodeTypes.GraphRes:
            case CodeTypes.HighResGraph:
                // 길이 가져오기 (Big-Endian 2바이트)
                // get length (Big-Endian 2 bytes)
                Length = (values[pos++] << 8) | values[pos++];
                // 데이터 배열 생성
                // create data array
                Data = new byte[Length];

                break;
            case CodeTypes.Error:
                // 길이 설정 (오류 코드 1바이트)
                // set length (error code 1 byte)
                Length = 1;
                // 데이터 배열 생성
                // create data array
                Data = new byte[Length];

                break;
            default:
                // 길이 0 설정
                // set length to 0
                Length = 0;
                // 빈 배열 생성
                // create empty array
                Data = [];

                break;
        }

        // 데이터 복사
        // copy data
        for (var i = 0; i < Length; i++)
            // 바이트 값 설정
            // set byte value
            Data[i] = values[pos + i];
    }
}