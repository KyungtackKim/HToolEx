using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Body types
/// </summary>
public enum BodyTypes : byte {
    /// <summary>
    ///     Integrated type (일체형)
    /// </summary>
    [Description("일체형")]
    Integrated = 0x00,

    /// <summary>
    ///     Separated type (분리형)
    /// </summary>
    [Description("분리형")]
    Separated = 0x01
}