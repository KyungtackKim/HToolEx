using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Model output function types
/// </summary>
public enum ModelOutputTypes {
    [Display(Description = @"ModelOutputTypeNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"ModelOutputTypeOn", ResourceType = typeof(HToolExRes))]
    On,
    [Display(Description = @"ModelOutputTypeOff", ResourceType = typeof(HToolExRes))]
    Off,
    [Display(Description = @"ModelOutputTypeHalfSec", ResourceType = typeof(HToolExRes))]
    OnShort,
    [Display(Description = @"ModelOutputTypeSec", ResourceType = typeof(HToolExRes))]
    OnLong
}