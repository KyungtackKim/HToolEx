using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     그래프 방향 열거형
///     graph direction enumeration
/// </summary>
/// <remarks>
///     그래프 수집 시 체결/풀림 방향을 구분하는 정의이며, 본 라이브러리 내부에서 현재 참조되지 않습니다.
///     definition distinguishing fastening/loosening directions during graph acquisition; currently unreferenced within this library.
/// </remarks>
public enum GraphDirection {
    /// <summary>
    ///     전체
    ///     all directions
    /// </summary>
    [Description("All")]
    All,

    /// <summary>
    ///     체결만
    ///     fasten only
    /// </summary>
    [Description("Fasten only")]
    FastenOnly,

    /// <summary>
    ///     풀림만
    ///     loosen only
    /// </summary>
    [Description("Loosen only")]
    LoosenOnly,

    /// <summary>
    ///     양방향
    ///     both directions
    /// </summary>
    [Description("Both")]
    Both
}