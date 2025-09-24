using System.ComponentModel;

namespace HTool.Type;

/// <summary>
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