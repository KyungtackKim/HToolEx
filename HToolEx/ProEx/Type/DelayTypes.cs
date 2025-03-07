namespace HToolEx.ProEx.Type;

/// <summary>
///     Delay step mode types for ParaMon-Pro X Job
/// </summary>
public enum DelayTypes {
    Time,
    [Obsolete("Support Rev.0 only")] PopUp,
    [Obsolete("Support Rev.0 only")] Barcode
}