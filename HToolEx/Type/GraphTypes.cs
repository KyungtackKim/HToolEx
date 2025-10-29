using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Graph channel types
/// </summary>
public enum GraphTypes {
    [Display(Description = @"GraphTypeNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"GraphTypeTorque", ResourceType = typeof(HToolExRes))]
    Torque,
    [Display(Description = @"GraphTypeCurrent", ResourceType = typeof(HToolExRes))]
    Current,
    [Display(Description = @"GraphTypeSpeed", ResourceType = typeof(HToolExRes))]
    Speed,
    [Display(Description = @"GraphTypeAngle", ResourceType = typeof(HToolExRes))]
    Angle,
    [Display(Description = @"GraphTypeSpeedCmd", ResourceType = typeof(HToolExRes))]
    SpeedCmd,
    [Display(Description = @"GraphTypeCurrentCmd", ResourceType = typeof(HToolExRes))]
    CurrentCmd,
    [Display(Description = @"GraphTypeSnugAngle", ResourceType = typeof(HToolExRes))]
    SnugAngle,
    [Display(Description = @"GraphTypeTorqueAngle", ResourceType = typeof(HToolExRes))]
    TorqueAngle
}