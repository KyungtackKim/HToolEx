using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Job step types
/// </summary>
[PublicAPI]
public enum JobStepTypes {
    [Display(Description = @"JobStepTypeFastening", ResourceType = typeof(HToolExRes))]
    Fastening,
    [Display(Description = @"JobStepTypeInput", ResourceType = typeof(HToolExRes))]
    Input,
    [Display(Description = @"JobStepTypeOutput", ResourceType = typeof(HToolExRes))]
    Output,
    [Display(Description = @"JobStepTypeDelay", ResourceType = typeof(HToolExRes))]
    Delay,
    [Display(Description = @"JobStepTypeMessage", ResourceType = typeof(HToolExRes))]
    Message,
    [Display(Description = @"JobStepTypeId", ResourceType = typeof(HToolExRes))]
    Id
}