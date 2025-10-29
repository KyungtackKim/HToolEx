using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Motor direction types
/// </summary>
public enum DirectionTypes {
    [Display(Description = @"DirTypeFastening", ResourceType = typeof(HToolExRes))]
    Fastening,
    [Display(Description = @"DirTypeLoosening", ResourceType = typeof(HToolExRes))]
    Loosening
}