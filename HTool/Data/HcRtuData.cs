using HTool.Type;

namespace HTool.Data;

/// <summary>
///     MODBUS-RTU 수신 데이터 파싱 클래스. 시리얼 통신에서 받은 원시 패킷을 구조화된 데이터로 변환합니다.
///     MODBUS-RTU received data parsing class. Converts raw packets from serial communication into structured data.
/// </summary>
/// <remarks>
///     RTU 프레임 구조: [Device ID(1)] [Function Code(1)] [Data(N)] [CRC-Low(1)] [CRC-High(1)]
///     RTU frame structure: [Device ID(1)] [Function Code(1)] [Data(N)] [CRC-Low(1)] [CRC-High(1)]
/// </remarks>
public sealed class HcRtuData : IReceivedData {
    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    public HcRtuData(IReadOnlyList<byte> values) {
        // 수신 데이터 생성
        // create the received data
        Create(values);
    }

    /// <summary>
    ///     MODBUS 슬레이브 주소 (1-247). 패킷의 첫 번째 바이트입니다.
    ///     MODBUS slave address (1-247). First byte of packet.
    /// </summary>
    public byte DeviceId { get; private set; }

    /// <summary>
    ///     CRC-16 검증값 (Big-Endian). 패킷의 마지막 2바이트로부터 계산됩니다.
    ///     CRC-16 checksum (Big-Endian). Calculated from last 2 bytes of packet.
    /// </summary>
    /// <remarks>
    ///     CRC-16/MODBUS 알고리즘 사용 (Polynomial: 0xA001).
    ///     Uses CRC-16/MODBUS algorithm (Polynomial: 0xA001).
    /// </remarks>
    public int CyclicCheck { get; private set; }

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
        // 헤더 설정
        // set header
        DeviceId = values[pos++];
        Code     = (CodeTypes)values[pos++];
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

        // CRC 값 설정 (Big-Endian)
        // set CRC value (Big-Endian)
        CyclicCheck = (values[^1] << 8) | values[^2];
    }
}