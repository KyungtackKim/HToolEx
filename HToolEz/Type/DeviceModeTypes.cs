using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Device mode types
/// </summary>
public enum DeviceModeTypes : byte {
    /// <summary>
    ///     Operation mode
    /// </summary>
    [Description("Operation")]
    Operation = 0x00,

    /// <summary>
    ///     Calibration mode
    /// </summary>
    [Description("Calibration")]
    Calibration = 0x01
}