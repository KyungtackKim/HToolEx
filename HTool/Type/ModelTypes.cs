using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     한타스 도구 모델 타입
///     Hantas tool model types
/// </summary>
public enum ModelTypes {
    // 한타스 모델 코드
    // Hantas model codes
    [Description("MD")]
    Md = 1,
    [Description("AD")]
    Ad = 2,
    [Description("MDT")]
    Mdt = 15,
    [Description("BM")]
    Bm = 10,
    [Description("BMT")]
    Bmt = 19,
    [Description("BPT")]
    Bpt = 20,
    [Description("MDT40")]
    Mdt40 = 27,
    [Description("BMT40")]
    Bmt40 = 29,

    // Mountz 모델 코드
    // Mountz model codes
    [Description("EC")]
    Ec = 1001,
    [Description("ECT")]
    Ect = 1015,
    [Description("EP")]
    Ep = 1010,
    [Description("EPT")]
    Ept = 1019,
    [Description("ECT40")]
    Ect40 = 1027,
    [Description("EPT40")]
    Ept40 = 1029
}