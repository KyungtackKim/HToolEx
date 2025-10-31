using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Calibration types
/// </summary>
public enum CalibrationTypes : byte {
    /// <summary>
    ///     3-Point calibration
    /// </summary>
    [Description("3-Point")]
    ThreePoint = 0x00,

    /// <summary>
    ///     5-Point calibration
    /// </summary>
    [Description("5-Point")]
    FivePoint = 0x01
}