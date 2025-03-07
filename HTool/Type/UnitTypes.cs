using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Torque unit types
/// </summary>
[PublicAPI]
public enum UnitTypes {
    [Description("kgf.cm")] KgfCm,
    [Description("kgf.m")] KgfM,
    [Description("N.m")] Nm,
    [Description("N.cm")] NCm,
    [Description("lbf.in")] LbfIn,
    [Description("ozf.in")] OzfIn,
    [Description("lbf.ft")] LbfFt
}