using HToolEx.Type;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Session message request information class
/// </summary>
public class FormatMessageRequest {
    /// <summary>
    ///     Message data
    /// </summary>
    public FormatMessage Message { get; init; } = default!;

    /// <summary>
    ///     Not acknowledge status
    /// </summary>
    public bool IsNotAck { get; set; }

    /// <summary>
    ///     Message activation status
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Message retry count
    /// </summary>
    public int Retry { get; set; } = 3;

    /// <summary>
    ///     Activation time
    /// </summary>
    public DateTime ActiveTime { get; set; }

    /// <summary>
    ///     MODBUS request code type
    /// </summary>
    public CodeTypes Code { get; set; }

    /// <summary>
    ///     MODBUS request address
    /// </summary>
    public int Address { get; set; }
}