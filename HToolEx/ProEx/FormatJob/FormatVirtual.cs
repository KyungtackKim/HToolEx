using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Format virtual preset data class in FastenStep
/// </summary>
public class FormatVirtual {
    /// <summary>
    ///     Enable status
    /// </summary>
    [PublicAPI]
    public bool Enable { get; set; }

    /// <summary>
    ///     Fasten data
    /// </summary>
    [PublicAPI]
    public byte[] Fasten { get; } = new byte[100];

    /// <summary>
    ///     Advance data
    /// </summary>
    [PublicAPI]
    public byte[] Advance { get; } = new byte[100];
}