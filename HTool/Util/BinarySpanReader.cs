using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace HTool.Util;

/// <summary>
///     Big-endian binary reading utilities for ReadOnlySpan&lt;byte&gt;
/// </summary>
public static class BinarySpanReader {
    /// <summary>
    ///     Reads a byte and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(ReadOnlySpan<byte> span, ref int pos) // return the value
    {
        return span[pos++];
    }

    /// <summary>
    ///     Reads an Int16 in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16(ReadOnlySpan<byte> span) // return the value
    {
        return BinaryPrimitives.ReadInt16BigEndian(span);
    }

    /// <summary>
    ///     Reads an Int16 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadInt16BigEndian(span[pos..]);
        // update the position
        pos += 2;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads a UInt16 in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> span) // return the value
    {
        return BinaryPrimitives.ReadUInt16BigEndian(span);
    }

    /// <summary>
    ///     Reads a UInt16 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadUInt16BigEndian(span[pos..]);
        // update the position
        pos += 2;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads an Int32 in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> span) // return the value
    {
        return BinaryPrimitives.ReadInt32BigEndian(span);
    }

    /// <summary>
    ///     Reads an Int32 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadInt32BigEndian(span[pos..]);
        // update the position
        pos += 4;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads a UInt32 in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32(ReadOnlySpan<byte> span) // return the value
    {
        return BinaryPrimitives.ReadUInt32BigEndian(span);
    }

    /// <summary>
    ///     Reads a UInt32 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadUInt32BigEndian(span[pos..]);
        // update the position
        pos += 4;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads an Int64 in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(ReadOnlySpan<byte> span) // return the value
    {
        return BinaryPrimitives.ReadInt64BigEndian(span);
    }

    /// <summary>
    ///     Reads an Int64 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadInt64BigEndian(span[pos..]);
        // update the position
        pos += 8;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads a UInt64 in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ReadOnlySpan<byte> span) // return the value
    {
        return BinaryPrimitives.ReadUInt64BigEndian(span);
    }

    /// <summary>
    ///     Reads a UInt64 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadUInt64BigEndian(span[pos..]);
        // update the position
        pos += 8;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads a Single (float) in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingle(ReadOnlySpan<byte> span) // return the value
    {
        return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span));
    }

    /// <summary>
    ///     Reads a Single (float) in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingle(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span[pos..]));
        // update the position
        pos += 4;
        // return the value
        return value;
    }

    /// <summary>
    ///     Reads a Double in Big-Endian
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDouble(ReadOnlySpan<byte> span) // return the value
    {
        return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span));
    }

    /// <summary>
    ///     Reads a Double in Big-Endian and advances position
    /// </summary>
    /// <param name="span">span</param>
    /// <param name="pos">position</param>
    /// <returns>value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDouble(ReadOnlySpan<byte> span, ref int pos) {
        // get the value
        var value = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span[pos..]));
        // update the position
        pos += 8;
        // return the value
        return value;
    }
}