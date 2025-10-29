using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Job event status types
/// </summary>
public enum JobEventTypes {
    [Display(Description = @"JobEventTypeStepInto", ResourceType = typeof(HToolExRes))]
    StepInto,
    [Display(Description = @"JobEventTypeFastenOk", ResourceType = typeof(HToolExRes))]
    FastenOk,
    [Display(Description = @"JobEventTypeFastenNg", ResourceType = typeof(HToolExRes))]
    FastenNg,
    [Display(Description = @"JobEventTypeStepOk", ResourceType = typeof(HToolExRes))]
    StepOk,
    [Display(Description = @"JobEventTypeStepNg", ResourceType = typeof(HToolExRes))]
    StepNg,
    [Display(Description = @"JobEventTypeJobOk", ResourceType = typeof(HToolExRes))]
    JobOk,
    [Display(Description = @"JobEventTypeJobNg", ResourceType = typeof(HToolExRes))]
    JobNg,
    [Display(Description = @"JobEventTypeJobAbort", ResourceType = typeof(HToolExRes))]
    JobAborted,
    [Display(Description = @"JobEventTypeSkip", ResourceType = typeof(HToolExRes))]
    Skip,
    [Display(Description = @"JobEventTypeBack", ResourceType = typeof(HToolExRes))]
    Back,
    [Display(Description = @"JobEventTypeResetStep", ResourceType = typeof(HToolExRes))]
    ResetStep,
    [Display(Description = @"JobEventTypeResetJob", ResourceType = typeof(HToolExRes))]
    ResetJob
}