namespace HTool.Util;

/// <summary>
///     Ring buffer class
/// </summary>
public sealed class RingBuffer {
    /// <summary>
    ///     Buffer array
    /// </summary>
    private readonly byte[] _buffer;

    /// <summary>
    ///     Mask for fast modulo operation (capacity - 1)
    /// </summary>
    private readonly int _mask;

    /// <summary>
    ///     Read position
    /// </summary>
    private int _readPos;

    /// <summary>
    ///     Write position
    /// </summary>
    private int _writePos;

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
        if (data.Length == 0 || data.Length > Capacity)
            return;
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
    }

    /// <summary>
    ///     Write for many data by ReadOnlySpan
    /// </summary>
    /// <param name="data">data</param>
    public void WriteBytes(ReadOnlySpan<byte> data) {
        // check the length
        if (data.Length == 0 || data.Length > Capacity)
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
        // get the values
        var available = Available;
        var pos       = _readPos;
        // check offset
        return (uint)offset < (uint)available ? _buffer[(pos + offset) & _mask] :
            throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset");
    }

    /// <summary>
    ///     Read all peek data
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> PeekBytes() {
        // get the values
        var available = Available;
        var pos       = _readPos;
        // check data
        if (available == 0)
            return ReadOnlySpan<byte>.Empty;
        // get the length
        var len = _buffer.Length;
        // check the length
        if (pos + available <= len)
            return new ReadOnlySpan<byte>(_buffer, pos, available);

        // create the array
        var result = new byte[available];
        // get the first part length
        var first = len - pos;
        // copy the data
        Array.Copy(_buffer, pos, result, 0, first);
        Array.Copy(_buffer, 0, result, first, available - first);
        // get the all data
        return result;
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
        // get the values
        var available = Available;
        var pos       = _readPos;
        // get the actual length
        var actual = Math.Min(length, available);
        // check the actual length
        if (actual == 0)
            return [];
        // get the length
        var len = _buffer.Length;
        // create the buffer
        var result = new byte[actual];
        // check the length
        if (pos + actual <= len) {
            // copy the single block
            Buffer.BlockCopy(_buffer, pos, result, 0, actual);
        } else {
            // get the first block index
            var first = len - pos;
            // copy the two blocks
            Buffer.BlockCopy(_buffer, pos, result, 0, first);
            Buffer.BlockCopy(_buffer, 0, result, first, actual - first);
        }

        // update the position
        _readPos = (pos + actual) & _mask;
        // reset available
        Available = available - actual;
        // result
        return result;
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