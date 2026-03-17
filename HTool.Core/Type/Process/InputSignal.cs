using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     모델 입력 신호 기능 열거형
///     model input signal function enumeration
/// </summary>
public enum InputSignal {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None,

    /// <summary>
    ///     액티브 하이
    ///     active high
    /// </summary>
    [Description("Active high")]
    ActiveHigh,

    /// <summary>
    ///     액티브 로우
    ///     active low
    /// </summary>
    [Description("Active low")]
    ActiveLow,

    /// <summary>
    ///     상태 하이
    ///     status high
    /// </summary>
    [Description("Status high")]
    StatusHigh,

    /// <summary>
    ///     상태 로우
    ///     status low
    /// </summary>
    [Description("Status low")]
    StatusLow
}