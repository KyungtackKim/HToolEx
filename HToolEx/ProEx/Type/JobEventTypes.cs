using System.ComponentModel;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Job event status types
/// </summary>
public enum JobEventTypes {
    [Description("Step into")]
    StepInto,
    [Description("Fastening OK")]
    FastenOk,
    [Description("Fastening NG")]
    FastenNg,
    [Description("Step OK")]
    StepOk,
    [Description("Step NG")]
    StepNg,
    [Description("Job OK")]
    JobOk,
    [Description("Job NG")]
    JobNg,
    [Description("Job aborted")]
    JobAborted,
    [Description("Skip")]
    Skip,
    [Description("Back")]
    Back,
    [Description("Reset step")]
    ResetStep,
    [Description("Reset job")]
    ResetJob
}