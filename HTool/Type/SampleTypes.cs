using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Sample time types
/// </summary>
[PublicAPI]
public enum SampleTypes {
    [Description("5 ms")]
    Ms5 = 1,
    [Description("10 ms")]
    Ms10,
    [Description("15 ms")]
    Ms15,
    [Description("30 ms")]
    Ms30
}