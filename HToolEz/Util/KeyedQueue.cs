using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HToolEz.Util;

/// <summary>
///     Defines how <see cref="KeyedQueue{T, TKey}" /> handles duplicates when enqueuing.
/// </summary>
public enum EnqueueMode {
    /// <summary>
    ///     Enforce uniqueness by key: if an item with the same key is already present,
    ///     the enqueue attempt is skipped and returns false.
    /// </summary>
    EnforceUnique = 0,

    /// <summary>
    ///     Allow duplicates by key: the item is always enqueued, and an internal
    ///     per-key counter is incremented.
    /// </summary>
    AllowDuplicate = 1
}

/// <summary>
///     Exception thrown when the key selector function fails during queue operations.
/// </summary>
public sealed class KeySelectorException(string message, Exception innerException) : InvalidOperationException(message, innerException);

/*/// <summary>
/// A high-performance, thread-safe queue that:
/// <list type="bullet">
///   <item><description>Maintains FIFO ordering for items.</description></item>
///   <item><description>Tracks keys for each item (via a key selector) to prevent or allow duplicates.</description></item>
///   <item><description>Supports non-blocking and blocking Dequeue/Peek with timeout and cancellation.</description></item>
///   <item><description>Provides key-based queries (contains/pending counts) and optional mid-queue removal.</description></item>
//// </list>
/// </summary>
/// <remarks>
/// <para><strong>Performance Characteristics:</strong></para>
/// <list type="bullet">
///   <item><description>Enqueue/Dequeue: O(1) average.</description></item>
////   <item><description>Key-based queries: O(1).</description></item>
////   <item><description>Mid-queue removal: O(n) — use sparingly.</description></item>
//// </list>
/// <para><strong>Thread Safety:</strong> Single monitor lock (<c>lock (_lock)</c>). All public operations are thread-safe.</para>
/// <para>
///     <strong>Memory:</strong> Keys are stored with items to avoid recomputation. Call <see cref="TrimExcess" /> if
///     size shrinks a lot.
/// </para>
/// </remarks>*/
public class KeyedQueue<T, TKey> : IDisposable where TKey : notnull {
    // Per-key counters. If a key is present with value N, there are N pending items for that key in the queue.
    private readonly Dictionary<TKey, int> _keyCounts;

    // User-supplied function that maps an item T to its key TKey.
    private readonly Func<T, TKey> _keySelector;
    // Single lock object guarding both the queue and the key counters.
    private readonly object _lock = new();

    // FIFO store of items, alongside their computed keys (to avoid recomputing on dequeue).
    private readonly Queue<(T Item, TKey Key)> _queue;

    // Disposed flag (checked without lock for fast-fail; guarded by lock for state mutation).
    private volatile bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KeyedQueue{T, TKey}" /> class.
    /// </summary>
    /// <param name="keySelector">
    ///     A function that returns the key for an item. Must be non-null and should be fast and side-effect free.
    ///     If this function throws, the error is wrapped in <see cref="KeySelectorException" />.
    /// </param>
    /// <param name="keyComparer">
    ///     Optional equality comparer for <typeparamref name="TKey" />. If null, the default comparer is used.
    /// </param>
    /// <param name="capacity">
    ///     Optional initial capacity hint for the underlying queue.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keySelector" /> is null.</exception>
    protected KeyedQueue(
        Func<T, TKey>            keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        int                      capacity    = 0) {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _queue       = capacity > 0 ? new Queue<(T Item, TKey Key)>(capacity) : new Queue<(T Item, TKey Key)>();
        _keyCounts   = new Dictionary<TKey, int>(keyComparer);
    }

    /// <summary>Gets the current number of items in the queue. O(1). Thread-safe.</summary>
    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ThrowIfDisposed();
            lock (_lock) {
                return _queue.Count;
            }
        }
    }

    /// <summary>Gets whether the queue is currently empty. O(1). Thread-safe.</summary>
    public bool IsEmpty {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ThrowIfDisposed();
            lock (_lock) {
                return _queue.Count == 0;
            }
        }
    }

    /// <summary>Gets the number of unique keys currently tracked. O(1). Thread-safe.</summary>
    public int UniqueKeyCount {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ThrowIfDisposed();
            lock (_lock) {
                return _keyCounts.Count;
            }
        }
    }

    /// <summary>
    ///     Disposes the queue: clears state and wakes any waiting threads so they can exit.
    ///     After disposal, all public members throw <see cref="ObjectDisposedException" />.
    /// </summary>
    public void Dispose() {
        if (_disposed)
            return;

        lock (_lock) {
            if (_disposed)
                return;

            _disposed = true;
            _queue.Clear();
            _keyCounts.Clear();

            // Wake up any waiting threads; they'll detect disposed and throw.
            Monitor.PulseAll(_lock);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Create the keyed-queue
    /// </summary>
    /// <param name="keySelector">key selector</param>
    /// <param name="keyComparer">key compare</param>
    /// <param name="capacity">capacity</param>
    /// <returns></returns>
    public static KeyedQueue<T, TKey> Create(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null, int capacity = 0) {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));
        // create the instance
        return new KeyedQueue<T, TKey>(keySelector, keyComparer, capacity);
    }

    /// <summary>
    ///     Attempts to enqueue an item with the specified duplicate-handling mode.
    ///     O(1) average. Thread-safe. Wakes waiters via <see cref="Monitor.PulseAll(object)" />.
    /// </summary>
    /// <exception cref="KeySelectorException">If key selector throws.</exception>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnqueue(T item, EnqueueMode mode = EnqueueMode.EnforceUnique) {
        ThrowIfDisposed();

        TKey key;
        try {
            key = _keySelector(item);
        } catch (Exception ex) {
            throw new KeySelectorException("Key selector function failed during enqueue.", ex);
        }

        lock (_lock) {
            ThrowIfDisposed_NoInline(); // re-check under lock

            var exists = _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
            if (mode == EnqueueMode.EnforceUnique && exists)
                return false;

            _queue.Enqueue((item, key));

            // Avoid second dictionary lookup using ref access.
            ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyCounts, key, out var added);
            slot = added ? 1 : slot + 1;

            // Wake any consumers waiting in blocking Dequeue/Peek.
            Monitor.PulseAll(_lock);
            return true;
        }
    }

    /// <summary>
    ///     Attempts to enqueue a batch of items with a single lock acquisition.
    ///     Returns counts for accepted/skipped and failures (key selector errors).
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="items" /> is null.</exception>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public BatchEnqueueResult<T> TryEnqueueRange(IEnumerable<T> items, EnqueueMode mode = EnqueueMode.EnforceUnique) {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(items);

        var                                  accepted = 0;
        var                                  skipped  = 0;
        List<(T Item, Exception Exception)>? failures = null;

        lock (_lock) {
            ThrowIfDisposed_NoInline();

            foreach (var item in items) {
                TKey key;
                try {
                    key = _keySelector(item);
                } catch (Exception ex) {
                    failures ??= [];
                    failures.Add((item, new KeySelectorException("Key selector failed in batch enqueue.", ex)));
                    continue;
                }

                var exists = _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;

                if (mode == EnqueueMode.EnforceUnique && exists) {
                    skipped++;
                    continue;
                }

                _queue.Enqueue((item, key));
                ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyCounts, key, out var added);
                slot = added ? 1 : slot + 1;
                accepted++;
            }

            if (accepted > 0)
                Monitor.PulseAll(_lock);
        }

        var failuresOrEmpty =
            (IReadOnlyList<(T, Exception)>?)failures ?? [];

        return new BatchEnqueueResult<T>(accepted, skipped, failuresOrEmpty);
    }

    /// <summary>
    ///     Attempts to remove and return the next item in FIFO order without blocking. O(1). Thread-safe.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out T item) {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();

            if (_queue.Count == 0) {
                item = default!;
                return false;
            }

            var (val, key) = _queue.Dequeue();
            DecrementKeyCount(key);
            item = val;
            return true;
        }
    }

    /// <summary>
    ///     Attempts to remove and return the next item, optionally blocking until an item becomes available.
    ///     Uses <see cref="Monitor.Wait(object, int)" /> and wakes via <see cref="Monitor.PulseAll(object)" />.
    /// </summary>
    /// <param name="item">item</param>
    /// <param name="timeoutMs">0 = non-blocking, -1 = infinite, &gt;0 = milliseconds.</param>
    /// <param name="cancellationToken">Optional cancellation.</param>
    /// <returns><c>true</c> if an item was dequeued; <c>false</c> on timeout/cancellation.</returns>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public bool TryDequeue(out T item, int timeoutMs, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();

        var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

        lock (_lock) {
            while (_queue.Count == 0) {
                ThrowIfDisposed_NoInline();

                if (cancellationToken.IsCancellationRequested || timeoutMs == 0) {
                    item = default!;
                    return false;
                }

                var remain = timeoutMs < 0 ? Timeout.Infinite : Math.Max(0, timeoutMs - (int)sw!.ElapsedMilliseconds);
                if (Monitor.Wait(_lock, remain))
                    continue;
                item = default!;
                return false; // timeout
            }

            var (val, key) = _queue.Dequeue();
            DecrementKeyCount(key);
            item = val;
            return true;
        }
    }

    /// <summary>
    ///     Attempts to read (without removing) the next item in FIFO order without blocking. O(1). Thread-safe.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out T item) {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();

            if (_queue.Count == 0) {
                item = default!;
                return false;
            }

            item = _queue.Peek().Item;
            return true;
        }
    }

    /// <summary>
    ///     Attempts to read (without removing) the next item, optionally blocking until an item becomes available.
    /// </summary>
    /// <param name="item">item</param>
    /// <param name="timeoutMs">0 = non-blocking, -1 = infinite, &gt;0 = milliseconds.</param>
    /// <param name="cancellationToken">Optional cancellation.</param>
    /// <returns><c>true</c> if an item was available; <c>false</c> on timeout/cancellation.</returns>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public bool TryPeek(out T item, int timeoutMs, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();

        var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

        lock (_lock) {
            while (_queue.Count == 0) {
                ThrowIfDisposed_NoInline();

                if (cancellationToken.IsCancellationRequested || timeoutMs == 0) {
                    item = default!;
                    return false;
                }

                var remain = timeoutMs < 0 ? Timeout.Infinite : Math.Max(0, timeoutMs - (int)sw!.ElapsedMilliseconds);
                if (Monitor.Wait(_lock, remain))
                    continue;
                item = default!;
                return false; // timeout
            }

            item = _queue.Peek().Item;
            return true;
        }
    }

    /// <summary>Checks whether at least one item with the specified key exists. O(1). Thread-safe.</summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) {
        ThrowIfDisposed();
        lock (_lock) {
            return _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
        }
    }

    /// <summary>Gets how many items with the specified key are pending. O(1). Thread-safe.</summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PendingCountByKey(TKey key) {
        ThrowIfDisposed();
        lock (_lock) {
            return _keyCounts.GetValueOrDefault(key, 0);
        }
    }

    /// <summary>
    ///     Removes the first occurrence of an item with the specified key (O(n)).
    ///     Preserves FIFO order of the remaining items.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public bool TryRemoveByKey(TKey key) {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();

            if (!_keyCounts.TryGetValue(key, out var cnt) || cnt == 0)
                return false;

            var cmp     = _keyCounts.Comparer; // ensure comparer consistency
            var removed = false;
            var n       = _queue.Count;

            // Rotate through the queue once; drop the first matching element.
            for (var i = 0; i < n; i++) {
                var entry = _queue.Dequeue();

                if (!removed && cmp.Equals(entry.Key, key)) {
                    removed = true;
                    DecrementKeyCount(key);
                    continue; // drop
                }

                _queue.Enqueue(entry);
            }

            return removed;
        }
    }

    /// <summary>
    ///     Removes all items with the specified key (O(n)).
    ///     Preserves the relative order of the remaining items.
    /// </summary>
    /// <returns>The number of removed items.</returns>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public int RemoveAllByKey(TKey key) {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();

            if (!_keyCounts.TryGetValue(key, out var cnt) || cnt == 0)
                return 0;

            var cmp     = _keyCounts.Comparer; // ensure comparer consistency
            var removed = 0;
            var n       = _queue.Count;

            for (var i = 0; i < n; i++) {
                var entry = _queue.Dequeue();

                if (cmp.Equals(entry.Key, key)) {
                    removed++;
                    continue; // drop
                }

                _queue.Enqueue(entry);
            }

            _keyCounts.Remove(key);
            return removed;
        }
    }

    /// <summary>
    ///     Removes all items and clears all per-key counters. Wakes any waiters.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();
            _queue.Clear();
            _keyCounts.Clear();
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    ///     Requests underlying storage to release unused memory where possible. Thread-safe.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess() {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();
            _queue.TrimExcess();
            _keyCounts.TrimExcess();
        }
    }

    /// <summary>
    ///     Returns a snapshot (shallow copy) of the current items in FIFO order.
    /// </summary>
    /// <remarks>O(n). Thread-safe.</remarks>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public List<T> Snapshot() {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();

            var list = new List<T>(_queue.Count);
            foreach (var (item, _) in _queue)
                list.Add(item);
            return list;
        }
    }

    /// <summary>
    ///     Gets a snapshot of all currently tracked keys and their counts.
    /// </summary>
    /// <remarks>O(k) with k = unique keys. Thread-safe.</remarks>
    /// <exception cref="ObjectDisposedException">If the queue has been disposed.</exception>
    public IReadOnlyDictionary<TKey, int> GetKeySnapshot() {
        ThrowIfDisposed();

        lock (_lock) {
            ThrowIfDisposed_NoInline();
            return new Dictionary<TKey, int>(_keyCounts);
        }
    }

    // Decrement key count and remove key entry if it reaches zero.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecrementKeyCount(TKey key) {
        if (!_keyCounts.TryGetValue(key, out var cnt))
            return;
        if (cnt <= 1)
            _keyCounts.Remove(key);
        else
            _keyCounts[key] = cnt - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed() {
        if (!_disposed)
            return;
        throw new ObjectDisposedException(nameof(KeyedQueue<T, TKey>));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed_NoInline() {
        if (!_disposed)
            return;
        throw new ObjectDisposedException(nameof(KeyedQueue<T, TKey>));
    }
}

/// <summary>
///     Result of a batch enqueue operation.
/// </summary>
/// <typeparam name="T">Item type of the enqueued elements.</typeparam>
/// <param name="Accepted">Number of items successfully enqueued.</param>
/// <param name="Skipped">Number of items skipped due to uniqueness enforcement.</param>
/// <param name="Failures">Items that failed due to key selector exceptions (wrapped).</param>
public readonly record struct BatchEnqueueResult<T>(
    int                           Accepted,
    int                           Skipped,
    IReadOnlyList<(T, Exception)> Failures) {
    /// <summary>True if at least one failure occurred.</summary>
    public bool HasFailures => Failures.Count > 0;

    /// <summary>Total = Accepted + Skipped + Failures.Count.</summary>
    public int TotalProcessed => Accepted + Skipped + Failures.Count;
}

/// <summary>
///     Convenience wrapper for <see cref="KeyedQueue{T, TKey}" /> that uses the item itself as the key,
///     based on its equality semantics (Equals/GetHashCode or a supplied comparer).
/// </summary>
public sealed class KeyedQueue<T> : KeyedQueue<T, T> where T : notnull {
    /// <summary>Initializes a new instance using <typeparamref name="T" /> itself as the key.</summary>
    public KeyedQueue(IEqualityComparer<T>? comparer = null, int capacity = 0)
        : base(static x => x, comparer, capacity) { }
}