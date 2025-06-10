using System.ComponentModel;

namespace HToolEz.Defines.Enums;

/// <summary>
///     Device calibration point mode types
/// </summary>
public enum CalPointModeTypes {
    [Description("3-point")]
    PointThird,
    [Description("5-point")]
    PointFifth
}