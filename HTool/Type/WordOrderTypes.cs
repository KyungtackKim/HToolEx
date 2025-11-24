using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     Word order types for 32-bit MODBUS register values
/// </summary>
public enum WordOrderTypes {
    /// <summary>
    ///     High word first (ABCD) - Most common
    ///     Example: 0x12345678 → [0x12, 0x34, 0x56, 0x78]
    /// </summary>
    [Description("High-Low (ABCD)")]
    HighLow,

    /// <summary>
    ///     Low word first (CDAB) - Some devices
    ///     Example: 0x12345678 → [0x56, 0x78, 0x12, 0x34]
    /// </summary>
    [Description("Low-High (CDAB)")]
    LowHigh
}