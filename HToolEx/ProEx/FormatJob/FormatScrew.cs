using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Format screw data class in FastenStep
/// </summary>
public class FormatScrew {
    /// <summary>
    ///     Enable screw position
    /// </summary>
    [PublicAPI]
    public bool Enable { get; set; }

    /// <summary>
    ///     X position
    /// </summary>
    [PublicAPI]
    public uint X { get; set; }

    /// <summary>
    ///     Y position
    /// </summary>
    [PublicAPI]
    public uint Y { get; set; }

    /// <summary>
    ///     Radius
    /// </summary>
    [PublicAPI]
    public uint Radius { get; set; }

    /// <summary>
    ///     Thickness
    /// </summary>
    [PublicAPI]
    public uint Thickness { get; set; }

    /// <summary>
    ///     Default color RGB
    /// </summary>
    [PublicAPI]
    public Color Default { get; set; }

    /// <summary>
    ///     OK color RGB
    /// </summary>
    [PublicAPI]
    public Color Ok { get; set; }

    /// <summary>
    ///     NG color RGB
    /// </summary>
    [PublicAPI]
    public Color Ng { get; set; }

    /// <summary>
    ///     Spare data
    /// </summary>
    [PublicAPI]
    public byte[] Spare { get; } = new byte[3];
}