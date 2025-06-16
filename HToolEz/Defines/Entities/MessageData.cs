using HToolEz.Defines.Enums;
using HToolEz.Utils;

namespace HToolEz.Defines.Entities;

/// <summary>
///     Message data class
/// </summary>
public sealed class MessageData : IMessageRequest {
    /// <summary>
    ///     Command
    /// </summary>
    public DeviceCommandTypes Command { get; set; }

    /// <inheritdoc />
    public DateTime ActiveTime { get; set; } = DateTime.MinValue;

    /// <inheritdoc />
    public bool Activated { get; set; } = false;

    /// <inheritdoc />
    public int Retry { get; set; } = 1;

    /// <inheritdoc />
    public byte[] Packet { get; set; } = null!;

    /// <inheritdoc />
    public int Sum { get; set; } = 0;
}