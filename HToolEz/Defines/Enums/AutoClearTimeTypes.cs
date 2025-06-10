using System.ComponentModel;

namespace HToolEz.Defines.Enums;

/// <summary>
///     Auto clear time types
/// </summary>
public enum AutoClearTimeTypes {
    [Description("Disable")]
    Disable,
    [Description("0.5 sec")]
    HalfSecond,
    [Description("1 sec")]
    OneSecond,
    [Description("2 sec")]
    TwoSeconds,
    [Description("3 sec")]
    ThreeSeconds,
    [Description("4 sec")]
    FourSeconds,
    [Description("5 sec")]
    FiveSeconds
}