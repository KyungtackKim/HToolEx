using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     다중 시퀀스 명령 열거형
///     multi-sequence command enumeration
/// </summary>
public enum MsCommand {
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
    ///     종료
    ///     end
    /// </summary>
    [Description("End")]
    End,

    /// <summary>
    ///     지연
    ///     delay
    /// </summary>
    [Description("Delay")]
    Delay,

    /// <summary>
    ///     프리셋 선택
    ///     select preset
    /// </summary>
    [Description("Select preset")]
    SelectPreset,

    /// <summary>
    ///     풀림
    ///     loosening
    /// </summary>
    [Description("Loosening")]
    Loosening,

    /// <summary>
    ///     점프
    ///     jump
    /// </summary>
    [Description("Jump")]
    Jump,

    /// <summary>
    ///     카운트 값
    ///     count value = (A)
    /// </summary>
    [Description("Count value = (A)")]
    CountValue,

    /// <summary>
    ///     조건 분기
    ///     sub if (A)
    /// </summary>
    [Description("Sub if (A)")]
    SubIf
}