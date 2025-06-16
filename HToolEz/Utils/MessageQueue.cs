using System.Collections.Concurrent;

namespace HToolEz.Utils;

/// <summary>
///     Message request interface used by MessageQueue
/// </summary>
public interface IMessageRequest {
    /// <summary>
    ///     Time when the message was activated
    /// </summary>
    DateTime ActiveTime { get; set; }

    /// <summary>
    ///     Whether the message is currently active
    /// </summary>
    bool Activated { get; set; }

    /// <summary>
    ///     Number of retry attempts made
    /// </summary>
    int Retry { get; set; }

    /// <summary>
    ///     Raw packet data
    /// </summary>
    byte[] Packet { get; set; }

    /// <summary>
    ///     Checksum or unique identifier for duplicate detection
    /// </summary>
    int Sum { get; set; }
}

/// <summary>
///     Message queue utility class for transmission with retry and duplicate prevention.
/// </summary>
public sealed class MessageQueue<TRequest> : IDisposable where TRequest : IMessageRequest {
    private readonly ConcurrentQueue<TRequest> _queue = new();
    private readonly int _retries;
    private readonly ConcurrentDictionary<int, byte> _sumSet = new();
    private int _count;
    private bool _isDisposed;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="retries">retry count</param>
    public MessageQueue(int retries = 1) {
        // set retries count
        _retries = retries;
    }

    /// <summary>
    ///     Get total message count
    /// </summary>
    public int Count {
        get {
            // throws if disposed
            ThrowIfDisposed();
            // get the message count
            return Volatile.Read(ref _count);
        }
    }

    /// <summary>
    ///     Get empty state
    /// </summary>
    public bool IsEmpty {
        get {
            // throws if disposed
            ThrowIfDisposed();
            // check if the queue is empty
            return Volatile.Read(ref _count) == 0;
        }
    }

    /// <summary>
    ///     Dispose of the queue and release resources
    /// </summary>
    public void Dispose() {
        // check disposed
        if (_isDisposed)
            return;
        // set disposed
        _isDisposed = true;
        // reset message queue
        _queue.Clear();
        _sumSet.Clear();
    }

    /// <summary>
    ///     Enqueue message with duplicate prevention
    /// </summary>
    /// <param name="msg">request message</param>
    public void Enqueue(TRequest msg) {
        // throws if disposed
        ThrowIfDisposed();
        // check the message
        if (msg is null)
            // throw exception
            throw new ArgumentNullException(nameof(msg));
        // check the packet in the message
        if (msg.Packet == null)
            // throw exception
            throw new ArgumentException("Packet data cannot be null.", nameof(msg));

        // check the hash
        if (msg.Sum == 0)
            // set the hash value
            msg.Sum = ComputeChecksum(msg.Packet);

        // duplicate prevention
        if (!_sumSet.TryAdd(msg.Sum, 0))
            return;
        // enqueue the message
        _queue.Enqueue(msg);
        // increment count
        Interlocked.Increment(ref _count);
    }

    /// <summary>
    ///     Try to peek at the next message
    /// </summary>
    public bool TryPeek(out TRequest? msg) {
        // throws if disposed
        ThrowIfDisposed();
        // try to peek the message
        return _queue.TryPeek(out msg);
    }

    /// <summary>
    ///     Try to dequeue the next message and clean up its checksum
    /// </summary>
    public bool TryDequeue(out TRequest? msg) {
        // throws if disposed
        ThrowIfDisposed();
        // try to dequeue the message
        if (!_queue.TryDequeue(out msg))
            return false;
        // try to remove the message
        _sumSet.TryRemove(msg.Sum, out _);
        // decrement count
        Interlocked.Decrement(ref _count);
        // success
        return true;
    }

    /// <summary>
    ///     Clear all messages and checksum state
    /// </summary>
    public void Clear() {
        ThrowIfDisposed();
        _queue.Clear();
        _sumSet.Clear();
        Interlocked.Exchange(ref _count, 0);
    }

    /// <summary>
    ///     Check if the specified message has timed out
    /// </summary>
    public bool IsTimeout(TRequest msg, int timeout) {
        // throws if disposed
        ThrowIfDisposed();
        // check the message
        if (msg is null)
            // throw exception
            throw new ArgumentNullException(nameof(msg));
        // check if the message is active
        if (!msg.Activated)
            return false;
        // check timeout
        return (DateTime.UtcNow - msg.ActiveTime).TotalMilliseconds > timeout;
    }

    /// <summary>
    ///     Active the message
    /// </summary>
    /// <param name="msg"></param>
    public void Active(TRequest msg) {
        // throws if disposed
        ThrowIfDisposed();
        // check the message
        if (msg is null)
            // throw exception
            throw new ArgumentNullException(nameof(msg));
        // set active
        msg.Activated = true;
        // set active time
        msg.ActiveTime = DateTime.UtcNow;
    }

    /// <summary>
    ///     Deactivate the message and check if retry limit is reached
    /// </summary>
    public bool Deactivate(TRequest msg) {
        // throws if disposed
        ThrowIfDisposed();
        // check the message
        if (msg is null)
            // throw exception
            throw new ArgumentNullException(nameof(msg));
        // check if the message is active 
        if (!msg.Activated)
            return false;
        // reset active
        msg.Activated = false;
        // increase retry count and check if max retries count
        return ++msg.Retry >= _retries;
    }

    /// <summary>
    ///     Throw if disposed
    /// </summary>
    private void ThrowIfDisposed() {
        // check disposed
        if (!_isDisposed)
            return;
        // throw exception
        throw new ObjectDisposedException(nameof(MessageQueue<TRequest>));
    }

    /// <summary>
    ///     Computes a 32-bit FNV-1a hash for the given packet.
    /// </summary>
    private static int ComputeChecksum(byte[] packet) {
        unchecked {
            const uint fnvOffset = 2166136261u;
            const uint fnvPrime = 16777619u;
            // get the hash offset
            var hash = fnvOffset;
            // check the packet
            foreach (var b in packet) {
                // calculate
                hash ^= b;
                hash *= fnvPrime;
            }

            // get the hash
            return (int)hash;
        }
    }
}