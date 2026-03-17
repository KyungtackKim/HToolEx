using System.Collections.Concurrent;

namespace HToolEx.Util;

/// <summary>
///     Concurrent queue witch contains key check class
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConcurrentQueueWithCheck<T> where T : notnull {
    private const    int                                        Timeout     = 100;
    private readonly ConcurrentDictionary<T, LinkedListNode<T>> _hash       = new();
    private readonly LinkedList<T>                              _list       = [];
    private readonly object                                     _lockObject = new();

    /// <summary>
    ///     Enqueue item
    /// </summary>
    /// <param name="item">item</param>
    /// <returns>result</returns>
    public bool Enqueue(T item) {
        // try enter
        if (!Monitor.TryEnter(_lockObject, Timeout))
            return false;
        // try finally
        try {
            // check contains key
            if (!_hash.ContainsKey(item)) {
                // add item
                var node = _list.AddLast(item);
                // add hash
                if (_hash.TryAdd(item, node))
                    return true;
                // remote item
                _list.RemoveLast();
            }
        } finally {
            // exit
            Monitor.Exit(_lockObject);
        }

        return false;
    }

    /// <summary>
    ///     Try Dequeue item
    /// </summary>
    /// <param name="item">item</param>
    /// <returns>result</returns>
    public bool TryDequeue(out T item) {
        // reset item
        item = default!;
        // try enter
        if (!Monitor.TryEnter(_lockObject, Timeout))
            return false;
        // try finally
        try {
            // check first item
            if (_list.First != null) {
                // get first item
                item = _list.First.Value;
                // remove first
                _list.RemoveFirst();
                // remove hash
                if (_hash.TryRemove(item, out _))
                    return true;
                // add first
                _list.AddFirst(item);
            }
        } finally {
            // exit
            Monitor.Exit(_lockObject);
        }

        return false;
    }

    /// <summary>
    ///     Peek item
    /// </summary>
    /// <param name="item">item</param>
    /// <returns>result</returns>
    public bool Peek(out T item) {
        // reset item
        item = default!;
        // try enter
        if (!Monitor.TryEnter(_lockObject, Timeout))
            return false;
        // try finally
        try {
            // check first item
            if (_list.First != null) {
                // get first item
                item = _list.First.Value;
                // result
                return true;
            }
        } finally {
            // exit
            Monitor.Exit(_lockObject);
        }

        return false;
    }

    /// <summary>
    ///     Clear all items
    /// </summary>
    /// <param name="retry">retry</param>
    public void Clear(int retry = 3) {
        // Define the condition for ending the spin loop.
        var func = () => !Monitor.TryEnter(_lockObject, Timeout);
        // check retry count
        for (var i = 0; i < retry; i++) {
            // try enter
            if (!Monitor.TryEnter(_lockObject, Timeout)) {
                // check retry count
                if (i == retry)
                    // throw exception
                    throw new InvalidOperationException("Failed to clear the queue.");
                // spin wait
                SpinWait.SpinUntil(func, Timeout);
                // continue
                continue;
            }

            // try finally
            try {
                // clear items
                _list.Clear();
                _hash.Clear();
            } finally {
                // exit
                Monitor.Exit(_lockObject);
            }

            // exit
            break;
        }
    }

    /// <summary>
    ///     Get items count
    /// </summary>
    /// <param name="retry">retry</param>
    public int Count(int retry = 3) {
        var count = 0;
        // Define the condition for ending the spin loop.
        var func = () => !Monitor.TryEnter(_lockObject, Timeout);
        // check retry count
        for (var i = 0; i < retry; i++) {
            // try enter
            if (!Monitor.TryEnter(_lockObject, Timeout)) {
                // check retry count
                if (i == retry)
                    // throw exception
                    throw new InvalidOperationException("Failed to get the queue count.");
                // spin wait
                SpinWait.SpinUntil(func, Timeout);
                // continue
                continue;
            }

            // try finally
            try {
                // set count
                count = _list.Count;
            } finally {
                // exit
                Monitor.Exit(_lockObject);
            }

            // exit
            break;
        }

        return count;
    }

    /// <summary>
    ///     Get total items
    /// </summary>
    /// <param name="items">items</param>
    /// <returns>result</returns>
    public bool GetItems(out List<T> items) {
        // create items
        items = [];
        // try enter
        if (!Monitor.TryEnter(_lockObject, Timeout))
            return false;
        // try finally
        try {
            // check items
            items.AddRange(_list);
        } finally {
            // exit
            Monitor.Exit(_lockObject);
        }

        return true;
    }

    /// <summary>
    ///     Check contains item
    /// </summary>
    /// <param name="item">item</param>
    /// <param name="comparer">comparer</param>
    /// <param name="retry">retry count</param>
    /// <returns>result</returns>
    public bool Contains(T item, IEqualityComparer<T> comparer, int retry = 3) {
        var result = false;
        // Define the condition for ending the spin loop.
        var func = () => !Monitor.TryEnter(_lockObject, Timeout);
        // check retry count
        for (var i = 0; i < retry; i++) {
            // try enter
            if (!Monitor.TryEnter(_lockObject, Timeout)) {
                // check retry count
                if (i == retry)
                    // throw exception
                    throw new InvalidOperationException("Failed to check the queue.");
                // spin wait
                SpinWait.SpinUntil(func, Timeout);
                // continue
                continue;
            }

            // try finally
            try {
                // check compare
                result = _list.Contains(item, comparer);
            } finally {
                // exit
                Monitor.Exit(_lockObject);
            }

            // exit
            break;
        }

        return result;
    }
}