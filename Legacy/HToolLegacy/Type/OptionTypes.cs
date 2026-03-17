using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     그래프 방향 옵션 타입
///     Graph direction option types
/// </summary>
public enum OptionTypes {
    [Description("Fasten only")]
    FastenOnly = 1,
    [Description("Loosen only")]
    LoosenOnly,
    [Description("Both")]
    Both
}