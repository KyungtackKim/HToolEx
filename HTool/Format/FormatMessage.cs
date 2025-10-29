using HTool.Type;

namespace HTool.Format;

/// <summary>
///     Hantas tool message format class
/// </summary>
public sealed class FormatMessage {
    /// <summary>
    ///     Default address
    /// </summary>
    public const int EmptyAddr = 0;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="code">code</param>
    /// <param name="addr">address</param>
    /// <param name="packet">packet</param>
    /// <param name="retry">retry</param>
    /// <param name="notCheck">not check</param>
    public FormatMessage(CodeTypes code, int addr, byte[] packet, int retry = 1, bool notCheck = false) {
        // set the information
        Code     = code;
        Address  = addr;
        Retry    = retry;
        NotCheck = notCheck;
        // set the packet
        Packet = packet;
        // get the hash
        var hash = FastHash(Packet.Span);
        // set the key
        Key = new MessageKey(code, addr, hash);
    }

    /// <summary>
    ///     Code
    /// </summary>
    public CodeTypes Code { get; }

    /// <summary>
    ///     Address
    /// </summary>
    public int Address { get; }

    /// <summary>
    ///     Remaining retry count. Decremented by <see cref="Activate" />.
    /// </summary>
    public int Retry { get; private set; }

    /// <summary>
    ///     Not check option
    /// </summary>
    public bool NotCheck { get; }

    /// <summary>
    ///     Activated state
    /// </summary>
    public bool Activated { get; private set; }

    /// <summary>
    ///     Activated time
    /// </summary>
    public DateTime ActiveTime { get; private set; }

    /// <summary>
    ///     Packet
    /// </summary>
    public ReadOnlyMemory<byte> Packet { get; }

    /// <summary>
    ///     Key of packet
    /// </summary>
    public MessageKey Key { get; }

    /// <summary>
    ///     Activate the message
    /// </summary>
    public void Activate() {
        // set the information
        Activated  = true;
        ActiveTime = DateTime.Now;
        Retry--;
    }

    /// <summary>
    ///     Deactivate
    /// </summary>
    /// <returns>retry</returns>
    public int Deactivate() {
        // set the information
        Activated = false;
        // return the retry count
        return Retry;
    }

    private static int FastHash(ReadOnlySpan<byte> packet) {
        var hash = 2166136261u;
        // check the packet
        foreach (var b in packet) {
            // calculate the hash
            hash ^= b;
            hash *= 16777619;
        }

        return unchecked((int)hash);
    }

    /// <summary>
    ///     Compact key for format message
    /// </summary>
    /// <param name="Code">code</param>
    /// <param name="Address">address</param>
    /// <param name="Hash">hash</param>
    public readonly record struct MessageKey(CodeTypes Code, int Address, int Hash);
}