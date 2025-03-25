using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Format encoder data class in FastenStep
/// </summary>
public class FormatEncoder {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatEncoder() {
        // set default values
        SavePos = [0, 0, 0, 0];
        ZoneTol = [0, 0, 0, 0];
        OkTol = [0, 0, 0, 0];
        EnabledPickUp = [false, false];
    }

    /// <summary>
    ///     Saved encoder values
    /// </summary>
    [PublicAPI]
    public int[] SavePos { get; private set; }

    /// <summary>
    ///     Zone tolerance
    /// </summary>
    [PublicAPI]
    public int[] ZoneTol { get; private set; }

    /// <summary>
    ///     OK tolerance
    /// </summary>
    [PublicAPI]
    public int[] OkTol { get; private set; }

    /// <summary>
    ///     Enabled pick-up 1
    /// </summary>
    [PublicAPI]
    public bool[] EnabledPickUp { get; private set; }
}