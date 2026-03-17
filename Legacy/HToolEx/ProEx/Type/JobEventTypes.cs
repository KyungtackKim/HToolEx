namespace HToolEx.ProEx.Type;

/// <summary>
///     Job event status types
/// </summary>
public enum JobEventTypes {
    StepInto,
    FastenOk,
    FastenNg,
    StepOk,
    StepNg,
    JobOk,
    JobNg,
    JobAborted,
    Skip,
    Back,
    ResetStep,
    ResetJob
}