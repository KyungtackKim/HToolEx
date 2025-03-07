using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Graph direction types
/// </summary>
[PublicAPI]
public enum GraphDirectionTypes {
    [Description("All")] All,
    [Description("Fasten Only")] FastenOnly,
    [Description("Loosen Only")] LoosenOnly,
    [Description("Both")] Both
}