using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Auto clear time types
/// </summary>
public enum AutoClearTimeTypes : byte {
    /// <summary>
    ///     Disable
    /// </summary>
    [Description("Disable")]
    Disable = 0x00,

    /// <summary>
    ///     0.5 seconds
    /// </summary>
    [Description("0.5 sec")]
    Sec05 = 0x01,

    /// <summary>
    ///     1 second
    /// </summary>
    [Description("1 sec")]
    Sec1 = 0x02,

    /// <summary>
    ///     2 seconds
    /// </summary>
    [Description("2 sec")]
    Sec2 = 0x03,

    /// <summary>
    ///     3 seconds
    /// </summary>
    [Description("3 sec")]
    Sec3 = 0x04,

    /// <summary>
    ///     4 seconds
    /// </summary>
    [Description("4 sec")]
    Sec4 = 0x05,

    /// <summary>
    ///     5 seconds
    /// </summary>
    [Description("5 sec")]
    Sec5 = 0x06
}