using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Graph step types
/// </summary>
public enum GraphStepTypes {
    [Display(Description = @"GraphStepNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"GraphStepFreeReverse", ResourceType = typeof(HToolExRes))]
    FreeReverseRotation,
    [Display(Description = @"GraphStepThreadTap", ResourceType = typeof(HToolExRes))]
    ThreadTap,
    [Display(Description = @"GraphStepEngaging", ResourceType = typeof(HToolExRes))]
    Engaging,
    [Display(Description = @"GraphStepFreeRotation", ResourceType = typeof(HToolExRes))]
    FreeRotation,
    [Display(Description = @"GraphStepFasten", ResourceType = typeof(HToolExRes))]
    Fastening,
    [Display(Description = @"GraphStepSnugTorque", ResourceType = typeof(HToolExRes))]
    SnugTorque,
    [Display(Description = @"GraphStepPrevailingStart", ResourceType = typeof(HToolExRes))]
    Prevailing,
    [Display(Description = @"GraphStepSeating", ResourceType = typeof(HToolExRes))]
    Seating,
    [Display(Description = @"GraphStepClamp", ResourceType = typeof(HToolExRes))]
    Clamp,
    [Display(Description = @"GraphStepComplete", ResourceType = typeof(HToolExRes))]
    TorqueComplete,
    [Display(Description = @"GraphStepRotateAfterTorqueUp", ResourceType = typeof(HToolExRes))]
    RotationAfterTorqueUp
}