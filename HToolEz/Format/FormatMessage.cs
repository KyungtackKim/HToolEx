using HToolEz.Type;

namespace HToolEz.Format;

/// <summary>
///     Format message for queue
/// </summary>
public class FormatMessage {
    private int _retryCount;

    /// <summary>
    ///     Constructor (accepts byte array for backward compatibility)
    /// </summary>
    /// <param name="cmd">command</param>
    /// <param name="packet">packet data</param>
    /// <param name="retry">retry count</param>
    /// <param name="notCheck">not check response</param>
    public FormatMessage(DeviceCommandTypes cmd, byte[] packet, int retry = 1, bool notCheck = false)
        : this(cmd, packet.AsMemory(), retry, notCheck) { }

    /// <summary>
    ///     Constructor (preferred - avoids unnecessary copy)
    /// </summary>
    /// <param name="cmd">command</param>
    /// <param name="packet">packet data (will be stored as-is without copying)</param>
    /// <param name="retry">retry count</param>
    /// <param name="notCheck">not check response</param>
    public FormatMessage(DeviceCommandTypes cmd, ReadOnlyMemory<byte> packet, int retry = 1, bool notCheck = false) {
        Command     = cmd;
        Packet      = packet;
        Retry       = retry;
        NotCheck    = notCheck;
        _retryCount = retry;
    }

    /// <summary>
    ///     Command
    /// </summary>
    public DeviceCommandTypes Command { get; }

    /// <summary>
    ///     Packet (immutable copy)
    /// </summary>
    public ReadOnlyMemory<byte> Packet { get; }

    /// <summary>
    ///     Retry count
    /// </summary>
    public int Retry { get; }

    /// <summary>
    ///     Not check response (send only)
    /// </summary>
    public bool NotCheck { get; }

    /// <summary>
    ///     Activated state
    /// </summary>
    public bool Activated { get; private set; }

    /// <summary>
    ///     Activate time
    /// </summary>
    public DateTime ActiveTime { get; private set; }

    /// <summary>
    ///     Message key for uniqueness
    /// </summary>
    public MessageKey Key => new(Command);

    /// <summary>
    ///     Activate message
    /// </summary>
    public void Activate() {
        Activated  = true;
        ActiveTime = DateTime.Now;
    }

    /// <summary>
    ///     Deactivate message (for retry)
    /// </summary>
    /// <returns>remaining retry count</returns>
    public int Deactivate() {
        Activated = false;
        return --_retryCount;
    }

    /// <summary>
    ///     Message key for uniqueness check
    /// </summary>
    public record MessageKey(DeviceCommandTypes Command);
}