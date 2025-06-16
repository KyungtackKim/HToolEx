using System.ComponentModel;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Job step types
/// </summary>
[PublicAPI]
public enum JobStepTypes {
    [Description("Fastening")]
    Fastening,
    [Description("Input")]
    Input,
    [Description("Output")]
    Output,
    [Description("Delay")]
    Delay,
    [Description("Message")]
    Message,
    [Description("ID")]
    Id
}