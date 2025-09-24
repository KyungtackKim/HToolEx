using System.ComponentModel;

namespace HTool.Type;

/// <summary>
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