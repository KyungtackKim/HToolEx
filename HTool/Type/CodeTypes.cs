using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     MODBUS 함수 코드 열거형. 표준 MODBUS 코드(0x03-0x10)와 HANTAS 커스텀 코드(0x11, 0x64-0x66)를 포함합니다.
///     MODBUS function code enum. Includes standard MODBUS codes (0x03-0x10) and HANTAS custom codes (0x11, 0x64-0x66).
/// </summary>
public enum CodeTypes {
    [Description("홀딩 레지스터 읽기 / Read holding register")]
    ReadHoldingReg = 0x03,
    [Description("입력 레지스터 읽기 / Read input register")]
    ReadInputReg = 0x04,
    [Description("단일 레지스터 쓰기 / Write single register")]
    WriteSingleReg = 0x06,
    [Description("다중 레지스터 쓰기 / Write multiple register")]
    WriteMultiReg = 0x10,
    [Description("정보 레지스터 읽기 / Read information register")]
    ReadInfoReg = 0x11,
    [Description("그래프 레지스터 / Graph register")]
    Graph = 0x64,
    [Description("그래프 결과 레지스터 / Graph result register")]
    GraphRes = 0x65,
    [Description("고해상도 그래프 레지스터 / High resolution graph register")]
    HighResGraph = 0x66,
    [Description("에러 / Error")]
    Error = 0x80
}