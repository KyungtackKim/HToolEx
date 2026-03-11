using HTool.Type;

namespace HTool.Data;

/// <summary>
///     MODBUS 프로토콜로부터 수신한 데이터를 표현하는 인터페이스. RTU/TCP 공통 속성을 정의합니다.
///     Interface representing data received from MODBUS protocol. Defines common properties for RTU/TCP.
/// </summary>
/// <remarks>
///     HcRtuData와 HcTcpData 클래스가 이 인터페이스를 구현하여 프로토콜별 특화된 파싱을 제공합니다.
///     HcRtuData and HcTcpData classes implement this interface to provide protocol-specific parsing.
/// </remarks>
public interface IReceivedData {
    /// <summary>
    ///     MODBUS 함수 코드
    ///     MODBUS function code
    /// </summary>
    CodeTypes Code { get; }

    /// <summary>
    ///     데이터 길이 (바이트)
    ///     Data length (bytes)
    /// </summary>
    int Length { get; }

    /// <summary>
    ///     데이터 바이트 배열
    ///     Data byte array
    /// </summary>
    byte[] Data { get; }
}