namespace HToolEz.Utils;

/// <summary>
///     Circular buffer class
/// </summary>
/// <typeparam name="T">type</typeparam>
public sealed class CircularBuffer<T> {
    private readonly T[] _buffer;
    private int _head;
    private int _tail;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="capacity"></param>
    public CircularBuffer(int capacity) {
        // initialize
        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;
    }

    /// <summary>
    ///     Capacity
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    ///     Count
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Empty state
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    ///     Value
    /// </summary>
    /// <param name="index">index</param>
    public T this[int index] {
        get {
            // check index
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException($"{index} is out of range");
            // get the real index
            var real = (_head + index) % Capacity;
            // get value
            return _buffer[real];
        }
        set {
            // check index
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException($"{index} is out of range");
            // get the real index
            var real = (_head + index) % Capacity;
            // set value
            _buffer[real] = value;
        }
    }

    /// <summary>
    ///     Enqueue to buffer
    /// </summary>
    /// <param name="value">value</param>
    public void Enqueue(T value) {
        // check size
        if (Count == Capacity)
            throw new InvalidOperationException("Circular buffer is full");
        // set value
        _buffer[_tail] = value;
        // set index
        _tail = (_tail + 1) % Capacity;
        // set count
        Count++;
    }

    /// <summary>
    ///     Enqueue range
    /// </summary>
    /// <param name="values">values</param>
    public void EnqueueRange(ReadOnlySpan<T> values) {
        // check length
        if (values.Length > Capacity - Count)
            throw new InvalidOperationException("Not enough free space in buffer");
        // get the first part
        var first = Math.Min(values.Length, Capacity - _tail);
        // copy
        values[..first].CopyTo(_buffer.AsSpan(_tail, first));
        // get the rest part
        var rest = values.Length - first;
        // check the rest part
        if (rest > 0)
            // wrap around
            values.Slice(first, rest).CopyTo(_buffer.AsSpan(0, rest));
        // set index
        _tail = (_tail + values.Length) % Capacity;
        // set count
        Count += values.Length;
    }

    /// <summary>
    ///     Dequeue from buffer
    /// </summary>
    /// <returns>value</returns>
    public T Dequeue() {
        // check size
        if (Count == 0)
            throw new InvalidOperationException("Circular buffer is empty");
        // get value
        var value = _buffer[_head];
        // set index
        _head = (_head + 1) % Capacity;
        // set count
        Count--;
        // return value
        return value;
    }

    /// <summary>
    ///     Dequeue range
    /// </summary>
    /// <param name="values">values</param>
    /// <returns>result</returns>
    public int DequeueRange(Span<T> values) {
        // get dequeue length
        var to = Math.Min(values.Length, Count);
        // get the first part
        var first = Math.Min(to, Capacity - _head);
        // copy
        _buffer.AsSpan(_head, first).CopyTo(values[..first]);
        // get the rest part
        var rest = to - first;
        // check the rest part
        if (rest > 0)
            // wrap around
            _buffer.AsSpan(0, rest).CopyTo(values.Slice(first, rest));
        // set index
        _head = (_head + to) % Capacity;
        // set count
        Count -= to;
        // return length
        return to;
    }

    /// <summary>
    ///     Clear
    /// </summary>
    public void Clear() {
        _head = 0;
        _tail = 0;
        Count = 0;
    }
}