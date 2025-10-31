using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Torque unit types
/// </summary>
public enum UnitTypes {
    [Description("kgf.cm")]
    KgfCm,
    [Description("kgf.m")]
    KgfM,
    [Description("N.m")]
    Nm,
    [Description("N.cm")]
    NCm,
    [Description("lbf.in")]
    LbfIn,
    [Description("ozf.in")]
    OzfIn,
    [Description("lbf.ft")]
    LbfFt
}