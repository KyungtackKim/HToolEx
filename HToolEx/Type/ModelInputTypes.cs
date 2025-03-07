using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Model input function types
/// </summary>
public enum ModelInputTypes {
    [Description("None")] None,
    [Description("Active High")] ActiveHigh,
    [Description("Active Low")] ActiveLow,
    [Description("Status High")] StatusHigh,
    [Description("Status Low")] StatusLow
}