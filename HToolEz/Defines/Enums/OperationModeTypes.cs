using System.ComponentModel;

namespace HToolEz.Defines.Enums;

/// <summary>
///     Operation mode types
/// </summary>
public enum OperationModeTypes {
    [Description("Peak")]
    Peak,
    [Description("First-Peak")]
    FirstPeak,
    [Description("Track")]
    Track
}