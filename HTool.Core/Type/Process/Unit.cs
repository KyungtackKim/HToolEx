using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     토크 단위 열거형
///     torque unit enumeration
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Core.Util.TorqueUnit" />, <c>HTool.Format.Ez.CalibrationData</c>.
///     used by: <see cref="HTool.Core.Util.TorqueUnit" />, <c>HTool.Format.Ez.CalibrationData</c>.
/// </remarks>
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