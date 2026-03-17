using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace HTool.Util;

/// <summary>
///     ReadOnlySpan&lt;byte&gt;에서 Big-Endian 바이너리 데이터를 읽는 유틸리티 클래스. MODBUS 데이터 파싱에 사용됩니다.
///     Utility class for reading Big-Endian binary data from ReadOnlySpan&lt;byte&gt;. Used for MODBUS data parsing.
/// </summary>
/// <remarks>
///     MODBUS는 Big-Endian (네트워크 바이트 순서)를 사용합니다. 모든 메서드는 성능을 위해 AggressiveInlining이 적용됩니다.
///     MODBUS uses Big-Endian (network byte order). All methods are AggressiveInlining for performance.
/// </remarks>
public static class BinarySpanReader {
    /// <summary>
    ///     바이트를 읽고 위치를 전진
    ///     Reads a byte and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(ReadOnlySpan<byte> span, ref int pos) {
        return span[pos++];
    }

    /// <summary>
    ///     빅엔디안 Int16 읽기
    ///     Reads an Int16 in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16(ReadOnlySpan<byte> span) {
        return BinaryPrimitives.ReadInt16BigEndian(span);
    }

    /// <summary>
    ///     빅엔디안 Int16을 읽고 위치를 전진
    ///     Reads an Int16 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BinaryPrimitives.ReadInt16BigEndian(span[pos..]);
        // 위치 업데이트
        // update the position
        pos += 2;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 UInt16 읽기
    ///     Reads a UInt16 in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> span) {
        return BinaryPrimitives.ReadUInt16BigEndian(span);
    }

    /// <summary>
    ///     빅엔디안 UInt16을 읽고 위치를 전진
    ///     Reads a UInt16 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BinaryPrimitives.ReadUInt16BigEndian(span[pos..]);
        // 위치 업데이트
        // update the position
        pos += 2;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 Int32 읽기
    ///     Reads an Int32 in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> span) {
        return BinaryPrimitives.ReadInt32BigEndian(span);
    }

    /// <summary>
    ///     빅엔디안 Int32를 읽고 위치를 전진
    ///     Reads an Int32 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BinaryPrimitives.ReadInt32BigEndian(span[pos..]);
        // 위치 업데이트
        // update the position
        pos += 4;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 UInt32 읽기
    ///     Reads a UInt32 in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32(ReadOnlySpan<byte> span) {
        return BinaryPrimitives.ReadUInt32BigEndian(span);
    }

    /// <summary>
    ///     빅엔디안 UInt32를 읽고 위치를 전진
    ///     Reads a UInt32 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BinaryPrimitives.ReadUInt32BigEndian(span[pos..]);
        // 위치 업데이트
        // update the position
        pos += 4;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 Int64 읽기
    ///     Reads an Int64 in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(ReadOnlySpan<byte> span) {
        return BinaryPrimitives.ReadInt64BigEndian(span);
    }

    /// <summary>
    ///     빅엔디안 Int64를 읽고 위치를 전진
    ///     Reads an Int64 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BinaryPrimitives.ReadInt64BigEndian(span[pos..]);
        // 위치 업데이트
        // update the position
        pos += 8;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 UInt64 읽기
    ///     Reads a UInt64 in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ReadOnlySpan<byte> span) {
        return BinaryPrimitives.ReadUInt64BigEndian(span);
    }

    /// <summary>
    ///     빅엔디안 UInt64를 읽고 위치를 전진
    ///     Reads a UInt64 in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BinaryPrimitives.ReadUInt64BigEndian(span[pos..]);
        // 위치 업데이트
        // update the position
        pos += 8;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 Single (float) 읽기
    ///     Reads a Single (float) in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingle(ReadOnlySpan<byte> span) {
        return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span));
    }

    /// <summary>
    ///     빅엔디안 Single (float)을 읽고 위치를 전진
    ///     Reads a Single (float) in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingle(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span[pos..]));
        // 위치 업데이트
        // update the position
        pos += 4;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     빅엔디안 Double 읽기
    ///     Reads a Double in Big-Endian
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDouble(ReadOnlySpan<byte> span) {
        return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span));
    }

    /// <summary>
    ///     빅엔디안 Double을 읽고 위치를 전진
    ///     Reads a Double in Big-Endian and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <returns>읽은 값 / read value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDouble(ReadOnlySpan<byte> span, ref int pos) {
        // 값 읽기
        // get the value
        var value = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span[pos..]));
        // 위치 업데이트
        // update the position
        pos += 8;
        // 값 반환
        // return the value
        return value;
    }

    /// <summary>
    ///     ASCII 문자열을 읽고 위치를 전진
    ///     Reads an ASCII string and advances position
    /// </summary>
    /// <param name="span">데이터 스팬 / data span</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <param name="length">읽을 바이트 길이 / byte length to read</param>
    /// <returns>트림된 문자열 / trimmed string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAsciiString(ReadOnlySpan<byte> span, ref int pos, int length) {
        // 문자열 값 읽기
        // get the string value
        var value = Encoding.ASCII.GetString(span.Slice(pos, length)).Trim('\0').Trim();
        // 위치 업데이트
        // update the position
        pos += length;
        // 값 반환
        // return the value
        return value;
    }
}