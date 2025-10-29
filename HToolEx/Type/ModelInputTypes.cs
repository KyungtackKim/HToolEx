using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Model input function types
/// </summary>
public enum ModelInputTypes {
    [Display(Description = @"ModelInputTypeNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"ModelInputTypeActiveHigh", ResourceType = typeof(HToolExRes))]
    ActiveHigh,
    [Display(Description = @"ModelInputTypeActiveLow", ResourceType = typeof(HToolExRes))]
    ActiveLow,
    [Display(Description = @"ModelInputTypeStatusHigh", ResourceType = typeof(HToolExRes))]
    StatusHigh,
    [Display(Description = @"ModelInputTypeStatusLow", ResourceType = typeof(HToolExRes))]
    StatusLow
}