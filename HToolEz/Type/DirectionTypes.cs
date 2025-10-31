using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Direction types
/// </summary>
public enum DirectionTypes : byte {
    /// <summary>
    ///     Clockwise
    /// </summary>
    [Description("CW")]
    CW = 0x00,

    /// <summary>
    ///     Counter-Clockwise
    /// </summary>
    [Description("CCW")]
    CCW = 0x01,

    /// <summary>
    ///     Both directions
    /// </summary>
    [Description("Both")]
    Both = 0x02
}