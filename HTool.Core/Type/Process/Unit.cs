using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     토크 단위 열거형
///     torque unit enumeration
/// </summary>
public enum Unit {
    /// <summary>
    ///     kgf.cm
    /// </summary>
    [Description("kgf.cm")]
    KgfCm,

    /// <summary>
    ///     kgf.m
    /// </summary>
    [Description("kgf.m")]
    KgfM,

    /// <summary>
    ///     N.m
    /// </summary>
    [Description("N.m")]
    Nm,

    /// <summary>
    ///     N.cm
    /// </summary>
    [Description("N.cm")]
    NCm,

    /// <summary>
    ///     lbf.in
    /// </summary>
    [Description("lbf.in")]
    LbfIn,

    /// <summary>
    ///     ozf.in
    /// </summary>
    [Description("ozf.in")]
    OzfIn,

    /// <summary>
    ///     lbf.ft
    /// </summary>
    [Description("lbf.ft")]
    LbfFt
}