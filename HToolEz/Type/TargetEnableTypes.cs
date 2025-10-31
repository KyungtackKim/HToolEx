using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Target enable types
/// </summary>
public enum TargetEnableTypes : byte {
    /// <summary>
    ///     Target disable
    /// </summary>
    [Description("Disable")]
    Disable = 0x00,

    /// <summary>
    ///     Target enable
    /// </summary>
    [Description("Enable")]
    Enable = 0x01
}