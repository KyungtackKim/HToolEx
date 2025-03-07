using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Graph direction option types
/// </summary>
[PublicAPI]
public enum OptionTypes {
    [Description("Fasten only")] FastenOnly = 1,
    [Description("Loosen only")] LoosenOnly,
    [Description("Both")] Both
}