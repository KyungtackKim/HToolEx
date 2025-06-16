using System.ComponentModel;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Graph channel types for ParaMon-Pro X
/// </summary>
public enum GraphTypes {
    [Description("None")]
    None,
    [Description("Torque")]
    Torque,
    [Description("Speed")]
    Speed,
    [Description("Angle")]
    Angle,
    [Description("Torque / Angle")]
    TorqueAngle
}