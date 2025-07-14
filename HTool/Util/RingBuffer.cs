using JetBrains.Annotations;

namespace HTool.Util;

/// <summary>
///     Ring buffer class
/// </summary>
[PublicAPI]
public class RingBuffer {
    private readonly byte[] _buffer;
    private          int    _readPos;
    private          int    _writePos;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="capacity">size</param>
    public RingBuffer(int capacity) {
        // create buffer
        _buffer = new byte[capacity];
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
    ///     Write data
    /// </summary>
    /// <param name="data">data</param>
    public void Write(byte data) {
        // set data
        _buffer[_writePos] = data;
        // update position
        _writePos = (_writePos + 1) % Capacity;
        // check capacity
        if (Available < Capacity)
            // update available
            Available++;
        else
            // over raped data
            _readPos = (_readPos + 1) % Capacity;
    }

    /// <summary>
    ///     Write for many data
    /// </summary>
    /// <param name="data">data</param>
    public void WriteBytes(byte[] data) {
        // check data length
        foreach (var b in data)
            // write data
            Write(b);
    }

    /// <summary>
    ///     Peek the data
    /// </summary>
    /// <param name="offset">offset</param>
    /// <returns>data</returns>
    public byte Peek(int offset) {
        // check offset
        if (offset >= Available)
            // exception
            throw new IndexOutOfRangeException("Not enough data");
        // get data
        return _buffer[(_readPos + offset) % Capacity];
    }

    /// <summary>
    ///     Read the data
    /// </summary>
    /// <param name="length">length</param>
    /// <returns>data</returns>
    public byte[] ReadBytes(int length) {
        // check data
        if (length > Available)
            // exception
            throw new IndexOutOfRangeException("Not enough data");

        var result = new byte[length];
        // check length
        for (var i = 0; i < length; i++) {
            // set data
            result[i] = _buffer[_readPos];
            // update position
            _readPos = (_readPos + 1) % Capacity;
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