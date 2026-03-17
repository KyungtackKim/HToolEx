using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     모델 명령 열거형
///     model command enumeration
/// </summary>
public enum ModelCommand {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None,

    /// <summary>
    ///     체결
    ///     fastening
    /// </summary>
    [Description("Fastening")]
    Fastening,

    /// <summary>
    ///     지연
    ///     delay
    /// </summary>
    [Description("Delay")]
    Delay,

    /// <summary>
    ///     입력
    ///     input
    /// </summary>
    [Description("Input")]
    Input,

    /// <summary>
    ///     출력
    ///     output
    /// </summary>
    [Description("Output")]
    Output,

    /// <summary>
    ///     바코드
    ///     barcode
    /// </summary>
    [Description("Barcode")]
    Barcode
}