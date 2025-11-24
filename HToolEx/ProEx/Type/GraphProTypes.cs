using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Graph channel types for ParaMon-Pro X
/// </summary>
public enum GraphTypes {
    [Display(Description = @"GraphTypeNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"GraphTypeTorque", ResourceType = typeof(HToolExRes))]
    Torque,
    [Display(Description = @"GraphTypeSpeed", ResourceType = typeof(HToolExRes))]
    Speed,
    [Display(Description = @"GraphTypeAngle", ResourceType = typeof(HToolExRes))]
    Angle,
    [Display(Description = @"GraphTypeTorqueAngle", ResourceType = typeof(HToolExRes))]
    TorqueAngle
}