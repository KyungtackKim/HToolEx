using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     모델 출력 신호 기능 열거형
///     model output signal function enumeration
/// </summary>
public enum OutputSignal {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None,

    /// <summary>
    ///     켜짐
    ///     on
    /// </summary>
    [Description("On")]
    On,

    /// <summary>
    ///     꺼짐
    ///     off
    /// </summary>
    [Description("Off")]
    Off,

    /// <summary>
    ///     0.5초간 켜짐
    ///     on for 0.5 sec
    /// </summary>
    [Description("On for 0.5 sec")]
    OnShort,

    /// <summary>
    ///     1.0초간 켜짐
    ///     on for 1.0 sec
    /// </summary>
    [Description("On for 1.0 sec")]
    OnLong
}