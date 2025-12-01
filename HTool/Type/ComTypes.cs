using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     MODBUS 통신 프로토콜 열거형. RTU (시리얼) 또는 TCP (이더넷) 중 선택합니다.
///     MODBUS communication protocol enum. Choose between RTU (serial) or TCP (ethernet).
/// </summary>
public enum ComTypes {
    [Description("MODBUS-RTU")]
    Rtu,
    [Description("MODBUS-TCP")]
    Tcp
}

/// <summary>
///     MODBUS 통신 오류 열거형. 프로토콜 오류 코드와 라이브러리 내부 오류(CRC, Frame, Timeout)를 포함합니다.
///     MODBUS communication error enum. Includes protocol error codes and library internal errors (CRC, Frame, Timeout).
/// </summary>
public enum ComErrorTypes {
    /// <summary>
    ///     잘못된 함수 코드
    ///     Invalid function code
    /// </summary>
    [Description("잘못된 함수 / Invalid function")]
    InvalidFunction = 0x01,

    /// <summary>
    ///     잘못된 레지스터 주소
    ///     Invalid register address
    /// </summary>
    [Description("잘못된 주소 / Invalid address")]
    InvalidAddress = 0x02,

    /// <summary>
    ///     잘못된 데이터 값
    ///     Invalid data value
    /// </summary>
    [Description("잘못된 값 / Invalid value")]
    InvalidValue = 0x03,

    /// <summary>
    ///     CRC 검증 실패
    ///     CRC validation failed
    /// </summary>
    [Description("잘못된 CRC / Invalid CRC")]
    InvalidCrc = 0x07,

    /// <summary>
    ///     잘못된 프레임 형식
    ///     Invalid frame format
    /// </summary>
    [Description("잘못된 프레임 / Invalid frame")]
    InvalidFrame = 0x0C,

    /// <summary>
    ///     값이 유효 범위를 벗어남
    ///     Value out of valid range
    /// </summary>
    [Description("잘못된 값 범위 / Invalid value range")]
    InvalidValueRange = 0x0E,

    /// <summary>
    ///     통신 타임아웃
    ///     Communication timeout
    /// </summary>
    [Description("타임아웃 / Timeout")]
    Timeout = 0x0F
}