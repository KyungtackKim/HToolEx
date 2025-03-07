using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Model output function types
/// </summary>
public enum ModelOutputTypes {
    [Description("None")] None,
    [Description("On")] On,
    [Description("Off")] Off,
    [Description("On for 0.5 sec")] OnShort,
    [Description("On for 1.0 sec")] OnLong
}