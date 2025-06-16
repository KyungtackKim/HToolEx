using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Motor direction types
/// </summary>
[PublicAPI]
public enum DirectionTypes {
    [Description("Fastening")]
    Fastening,
    [Description("Loosening")]
    Loosening
}