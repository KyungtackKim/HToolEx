using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Operation mode types
/// </summary>
public enum OperationModeTypes : byte {
    /// <summary>
    ///     Peak mode
    /// </summary>
    [Description("Peak")]
    Peak = 0x00,

    /// <summary>
    ///     First-Peak mode
    /// </summary>
    [Description("First-Peak")]
    FirstPeak = 0x01,

    /// <summary>
    ///     Track mode
    /// </summary>
    [Description("Track")]
    Track = 0x02
}