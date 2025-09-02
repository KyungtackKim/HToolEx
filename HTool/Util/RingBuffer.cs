using JetBrains.Annotations;

namespace HTool.Util;

/// <summary>
///     Ring buffer class
/// </summary>
[PublicAPI]
public class RingBuffer {
    private readonly byte[] _buffer;
    private readonly int    _mask;
    private          int    _readPos;
    private          int    _writePos;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="capacity">size</param>
    public RingBuffer(int capacity) {
        // check the capacity
        if (capacity < 1)
            // throw the exception
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));
        // get the actual size
        var size = GetNextPowerOfTwo(capacity);
        // create buffer
        _buffer = new byte[size];
        // set the mask
        _mask = size - 1;
    }

    /// <summary>
    ///     Capacity of buffer
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    ///     Available for buffer
    /// </summary>
    public int Available { get; private set; }

    /// <summary>
    ///     get the next power of two
    /// </summary>
    /// <param name="value">input value</param>
    /// <returns>pow value</returns>
    private static int GetNextPowerOfTwo(int value) {
        // check the value is 2 ^ n
        if ((value & (value - 1)) == 0)
            return value;
        var result = 1;
        // find the next higher power of 2
        while (result < value)
            // shift
            result <<= 1;
        // result the pow value
        return result;
    }

    /// <summary>
    ///     Write data
    /// </summary>
    /// <param name="data">data</param>
    public void Write(byte data) {
        // set data
        _buffer[_writePos] = data;
        // update position
        _writePos = (_writePos + 1) & _mask;
        // check capacity
        if (Available < Capacity)
            // update available
            Available++;
        else
            // overwritten data
            _readPos = (_readPos + 1) & _mask;
    }

    /// <summary>
    ///     Write for many data
    /// </summary>
    /// <param name="data">data</param>
    public void WriteBytes(byte[] data) {
        // check the data
        ArgumentNullException.ThrowIfNull(data);
        // check the length
        if (data.Length == 0)
            return;
        // check data length
        foreach (var b in data) {
            // set data
            _buffer[_writePos] = b;
            // update position
            _writePos = (_writePos + 1) & _mask;
            // check capacity
            if (Available < Capacity)
                // update available
                Available++;
            else
                // overwritten data
                _readPos = (_readPos + 1) & _mask;
        }
    }

    /// <summary>
    ///     Peek the data
    /// </summary>
    /// <param name="offset">offset</param>
    /// <returns>data</returns>
    public byte Peek(int offset) {
        // check offset
        if (offset < 0)
            // throw the exception
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");

        // check offset
        return offset < Available ? _buffer[(_readPos + offset) & _mask] : throw new IndexOutOfRangeException("Not enough data");
    }

    /// <summary>
    ///     Read the data
    /// </summary>
    /// <param name="length">length</param>
    /// <returns>data</returns>
    public byte[] ReadBytes(int length) {
        // check length
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");
        // check data
        if (length > Available)
            // exception
            throw new IndexOutOfRangeException("Not enough data");
        // check length
        if (length == 0)
            return [];

        var result = new byte[length];
        // check length
        for (var i = 0; i < length; i++) {
            // set data
            result[i] = _buffer[_readPos];
            // update position
            _readPos = (_readPos + 1) & _mask;
        }

        // reset available
        Available -= length;
        // result
        return result;
    }

    /// <summary>
    ///     Clear all data
    /// </summary>
    public void Clear() {
        // reset positions
        _readPos  = 0;
        _writePos = 0;
        Available = 0;
    }
}