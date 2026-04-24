using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     모델 명령 열거형
///     model command enumeration
/// </summary>
/// <remarks>
///     직결 통신 모델에 전송되는 명령 정의이며, 본 라이브러리 내부에서 현재 참조되지 않습니다.
///     definition of commands sent to directly-connected models; currently unreferenced within this library.
/// </remarks>
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