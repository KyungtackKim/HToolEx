using JetBrains.Annotations;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Job step types
/// </summary>
[PublicAPI]
public enum JobStepTypes {
    Fastening,
    Input,
    Output,
    Delay,
    Message,
    Id
}