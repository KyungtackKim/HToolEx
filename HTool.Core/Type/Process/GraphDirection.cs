using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     그래프 방향 열거형
///     graph direction enumeration
/// </summary>
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