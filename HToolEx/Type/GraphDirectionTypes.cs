using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Graph direction types
/// </summary>
public enum GraphDirectionTypes {
    [Display(Description = @"GraphDirAll", ResourceType = typeof(HToolExRes))]
    All,
    [Display(Description = @"GraphDirFasten", ResourceType = typeof(HToolExRes))]
    FastenOnly,
    [Display(Description = @"GraphDirLoosen", ResourceType = typeof(HToolExRes))]
    LoosenOnly,
    [Display(Description = @"GraphDirBoth", ResourceType = typeof(HToolExRes))]
    Both
}