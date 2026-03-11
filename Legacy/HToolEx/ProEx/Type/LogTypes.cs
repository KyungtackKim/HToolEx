using System.ComponentModel;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Type;

/// <summary>
///     Log types for ParaMon-Pro X
/// </summary>
[PublicAPI]
public enum LogTypes {
    #region REV.0

    [Description("Job name")]
    JobName,
    [Description("Step name")]
    StepName,
    [Description("Tool name")]
    ToolName,
    [Description("Barcode")]
    [Obsolete("Support Rev.0 only")]
    Barcode,
    [Description("Fasten time")]
    FastenTime,
    [Description("Preset no.")]
    PresetNo,
    [Description("Torque unit")]
    TorqueUnit,
    [Description("Remain screw")]
    RemainScrew,
    [Description("Direction")]
    Direction,
    [Description("Error")]
    Error,
    [Description("Status")]
    Status,
    [Description("Target torque")]
    TargetTorque,
    [Description("Converted torque")]
    ConvertedTorque,
    [Description("Seating torque")]
    SeatingTorque,
    [Description("Clamp torque")]
    ClampTorque,
    [Description("Prevailing torque")]
    PrevailingTorque,
    [Description("Snug torque")]
    SnugTorque,
    [Description("Speed")]
    Speed,
    [Description("Angle 1")]
    A1,
    [Description("Angle 2")]
    A2,
    [Description("Angle 3")]
    A3,
    [Description("Snug angle")]
    SnugAngle,

    #endregion

    #region REV.1

    [Description("NG cause")]
    NgCause,
    [Description("ID1 name")]
    Id1Name,
    [Description("ID1")]
    Id1,
    [Description("ID2 name")]
    Id2Name,
    [Description("ID2")]
    Id2,
    [Description("ID3 name")]
    Id3Name,
    [Description("ID3")]
    Id3,
    [Description("ID4 name")]
    Id4Name,
    [Description("ID4")]
    Id4,
    [Description("ID5 name")]
    Id5Name,
    [Description("ID5")]
    Id5,
    [Description("ID6 name")]
    Id6Name,
    [Description("ID6")]
    Id6,

    #endregion
}