using HToolEz.Utils;

namespace HToolEz.Defines.Entities;

/// <summary>
///     Message request class
/// </summary>
public class MessageRequest : IMessageRequest {
    /// <inheritdoc />
    public DateTime ActiveTime { get; set; }

    /// <inheritdoc />
    public bool Activated { get; set; }

    /// <inheritdoc />
    public int Retry { get; set; }

    /// <inheritdoc />
    public byte[] Packet { get; set; } = null!;

    /// <inheritdoc />
    public int Sum { get; set; }
}