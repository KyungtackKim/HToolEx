using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     Hantas tool generation types
/// </summary>
public enum GenerationTypes {
    [Description("Gen.1")]
    GenRev1,
    [Description("Gen.1 for AD")]
    GenRev1Ad = 1,
    [Description("Gen.1+")]
    GenRev1Plus = 3000,
    [Description("Gen.2")]
    GenRev2 = 4000
}