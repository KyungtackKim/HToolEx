using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     다중 시퀀스 명령 열거형
///     multi-sequence command enumeration
/// </summary>
/// <remarks>
///     다중 시퀀스(MS) 기능을 가진 모델의 명령 정의이며, 본 라이브러리 내부에서 현재 참조되지 않습니다.
///     definition of commands for models with multi-sequence (MS) capability; currently unreferenced within this library.
/// </remarks>
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