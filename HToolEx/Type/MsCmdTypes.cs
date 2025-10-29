using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Multi sequence command types
/// </summary>
public enum MsCmdTypes {
    [Display(Description = @"MsCmdTypeNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"MsCmdTypeFastening", ResourceType = typeof(HToolExRes))]
    Fastening,
    [Display(Description = @"MsCmdTypeEnd", ResourceType = typeof(HToolExRes))]
    End,
    [Display(Description = @"MsCmdTypeDelay", ResourceType = typeof(HToolExRes))]
    Delay,
    [Display(Description = @"MsCmdTypePreset", ResourceType = typeof(HToolExRes))]
    SelectPreset,
    [Display(Description = @"MsCmdTypeLoosening", ResourceType = typeof(HToolExRes))]
    Loosening,
    [Display(Description = @"MsCmdTypeJump", ResourceType = typeof(HToolExRes))]
    Jump,
    [Display(Description = @"MsCmdTypeCount", ResourceType = typeof(HToolExRes))]
    CountValue,
    [Display(Description = @"MsCmdTypeIf", ResourceType = typeof(HToolExRes))]
    SubIf
}