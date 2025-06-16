using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Multi sequence command types
/// </summary>
public enum MsCmdTypes {
    [Description("None")]
    None,
    [Description("Fastening")]
    Fastening,
    [Description("End")]
    End,
    [Description("Delay")]
    Delay,
    [Description("Select preset")]
    SelectPreset,
    [Description("Loosening")]
    Loosening,
    [Description("Jump")]
    Jump,
    [Description("Count value = A")]
    CountValue,
    [Description("Sub if (A)")]
    SubIf
}