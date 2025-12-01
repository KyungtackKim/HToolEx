using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     HANTAS 토크 도구 세대 열거형. 펌웨어 버전을 기반으로 자동 판별되며, 데이터 형식과 레지스터 구성이 세대별로 다릅니다.
///     HANTAS torque tool generation enum. Auto-detected based on firmware version, data format and register layout vary
///     by generation.
/// </summary>
public enum GenerationTypes {
    [Description("1세대 / Gen.1")]
    GenRev1,
    [Description("1세대 AD용 / Gen.1 for AD")]
    GenRev1Ad = 1,
    [Description("1세대+ / Gen.1+")]
    GenRev1Plus = 3000,
    [Description("2세대 / Gen.2")]
    GenRev2 = 4000
}