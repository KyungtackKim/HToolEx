using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     그래프 방향 타입
///     Graph direction types
/// </summary>
public enum GraphDirectionTypes {
    [Description("All")]
    All,
    [Description("Fasten Only")]
    FastenOnly,
    [Description("Loosen Only")]
    LoosenOnly,
    [Description("Both")]
    Both
}