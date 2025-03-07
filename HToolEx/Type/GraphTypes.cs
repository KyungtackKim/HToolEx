using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Graph channel types
/// </summary>
public enum GraphTypes {
    [Description("None")] None,
    [Description("Torque")] Torque,
    [Description("Current")] Current,
    [Description("Speed")] Speed,
    [Description("Angle")] Angle,
    [Description("Speed command")] SpeedCmd,
    [Description("Current command")] CurrentCmd,
    [Description("Snug angle")] SnugAngle,
    [Description("Torque / Angle")] TorqueAngle
}