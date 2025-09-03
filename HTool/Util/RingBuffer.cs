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
#if NOT_USE
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
#else
        // get the data length
        var length = data.Length;
        var remain = Capacity - _writePos;
        // check the length
        if (length <= remain) {
            // copy all data
            Array.Copy(data, 0, _buffer, _writePos, length);
            // update the position
            _writePos = (_writePos + length) & _mask;
        } else {
            // get the remain data length
            var offset = length - remain;
            // copy the first block data
            Array.Copy(data, 0, _buffer, _writePos, remain);
            Array.Copy(data, remain, _buffer, 0, offset);
            // update the position
            _writePos = offset & _mask;
        }

        // get the new available size
        var newAvailable = Available + length;
        // check the available
        if (newAvailable > Capacity) {
            // get the overwritten size
            var size = newAvailable - Capacity;
            // overwritten the data
            _readPos = (_readPos + size) & _mask;
            // set the available
            Available = Capacity;
        } else {
            // set the available
            Available = newAvailable;
        }
#endif
    }

    /// <summary>
    ///     Write for many data by ReadOnlySpan
    /// </summary>
    /// <param name="data">data</param>
    public void WriteBytes(ReadOnlySpan<byte> data) {
        // check the length
        if (data.Length == 0)
            return;
        // get the data length
        var length = data.Length;
        var remain = Capacity - _writePos;

        // check the length
        if (length <= remain) {
            // copy all data directly
            data.CopyTo(_buffer.AsSpan(_writePos, length));
            // update the position
            _writePos = (_writePos + length) & _mask;
        } else {
            // get the remain data length
            var offset = length - remain;
            // copy the first block data
            data[..remain].CopyTo(_buffer.AsSpan(_writePos, remain));
            data[remain..].CopyTo(_buffer.AsSpan(0, offset));
            // update the position
            _writePos = offset & _mask;
        }

        // get the new available size
        var newAvailable = Available + length;
        // check the available
        if (newAvailable > Capacity) {
            // get the overwritten size
            var size = newAvailable - Capacity;
            // overwritten the data
            _readPos = (_readPos + size) & _mask;
            // set the available
            Available = Capacity;
        } else {
            // set the available
            Available = newAvailable;
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
    ///     Read all peek data
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> PeekBytes() {
        // check data
        if (Available == 0)
            return ReadOnlySpan<byte>.Empty;
        // check if all data is contiguous (no wrap around)
        if (_readPos + Available <= _buffer.Length)
            // get the all data
            return new ReadOnlySpan<byte>(_buffer, _readPos, Available);
        // create the array
        var data = new byte[Available];
        // get the first part length
        var first = _buffer.Length - _readPos;
        // copy the data
        Array.Copy(_buffer, _readPos, data, 0, first);
        Array.Copy(_buffer, 0, data, first, Available - first);
        // get the all data
        return data;
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
#if NOT_USE
        // check data
        if (length > Available)
            // exception
            throw new IndexOutOfRangeException("Not enough data");
        // check length
        if (length == 0)
            return [];
        // create the buffer
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
#else
        // get the actual length
        var actual = Math.Min(length, Available);
        // check the actual length
        if (actual == 0)
            return [];
        // create the buffer
        var result = new byte[actual];
        // check the length
        for (var i = 0; i < actual; i++) {
            // set the data
            result[i] = _buffer[_readPos];
            // update the position
            _readPos = (_readPos + 1) & _mask;
        }

        // reset available
        Available -= actual;
        // result
        return result;
#endif
    }

    /// <summary>
    ///     Remove the data
    /// </summary>
    /// <param name="length">length</param>
    public void RemoveBytes(int length) {
        // check length
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");

        // get the actual length
        var actual = Math.Min(length, Available);
        // check the actual length
        if (actual == 0)
            return;
        // move read position
        _readPos = (_readPos + actual) & _mask;
        // reset available
        Available -= actual;
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