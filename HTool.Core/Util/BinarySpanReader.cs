using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace HTool.Core.Util;

/// <summary>
///     ReadOnlySpan&lt;byte&gt;에서 Big-Endian 바이너리 데이터를 읽는 유틸리티 클래스. MODBUS 데이터 파싱에 사용됩니다.
///     utility class for reading Big-Endian binary data from ReadOnlySpan&lt;byte&gt;. Used for MODBUS data parsing.
/// </summary>
/// <remarks>
///     MODBUS는 Big-Endian (네트워크 바이트 순서)를 사용합니다. 모든 메서드는 성능을 위해 AggressiveInlining이 적용됩니다.
///     MODBUS uses Big-Endian (network byte order). All methods are AggressiveInlining for performance.
/// </remarks>
public static class BinarySpanReader {
	/// <summary>
	///     바이트를 읽고 위치를 전진
	///     reads a byte and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(ReadOnlySpan<byte> span, ref int pos) {
        // read byte and advance position / 바이트 읽기 및 위치 이동
        return span[pos++];
    }

	/// <summary>
	///     빅엔디안 Int16 읽기
	///     reads an Int16 in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16(ReadOnlySpan<byte> span) {
        // read Big-Endian Int16 and return value / 빅엔디안 Int16 값 읽기 및 반환
        return BinaryPrimitives.ReadInt16BigEndian(span);
    }

	/// <summary>
	///     빅엔디안 Int16을 읽고 위치를 전진
	///     reads an Int16 in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian Int16 value / 값 읽기
        var value = BinaryPrimitives.ReadInt16BigEndian(span[pos..]);
        // advance position by 2 bytes / 위치 업데이트
        pos += 2;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 UInt16 읽기
	///     reads a UInt16 in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> span) {
        // read Big-Endian UInt16 and return value / 빅엔디안 UInt16 값 읽기 및 반환
        return BinaryPrimitives.ReadUInt16BigEndian(span);
    }

	/// <summary>
	///     빅엔디안 UInt16을 읽고 위치를 전진
	///     reads a UInt16 in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian UInt16 value / 값 읽기
        var value = BinaryPrimitives.ReadUInt16BigEndian(span[pos..]);
        // advance position by 2 bytes / 위치 업데이트
        pos += 2;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 Int32 읽기
	///     reads an Int32 in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> span) {
        // read Big-Endian Int32 and return value / 빅엔디안 Int32 값 읽기 및 반환
        return BinaryPrimitives.ReadInt32BigEndian(span);
    }

	/// <summary>
	///     빅엔디안 Int32를 읽고 위치를 전진
	///     reads an Int32 in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian Int32 value / 값 읽기
        var value = BinaryPrimitives.ReadInt32BigEndian(span[pos..]);
        // advance position by 4 bytes / 위치 업데이트
        pos += 4;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 UInt32 읽기
	///     reads a UInt32 in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32(ReadOnlySpan<byte> span) {
        // read Big-Endian UInt32 and return value / 빅엔디안 UInt32 값 읽기 및 반환
        return BinaryPrimitives.ReadUInt32BigEndian(span);
    }

	/// <summary>
	///     빅엔디안 UInt32를 읽고 위치를 전진
	///     reads a UInt32 in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian UInt32 value / 값 읽기
        var value = BinaryPrimitives.ReadUInt32BigEndian(span[pos..]);
        // advance position by 4 bytes / 위치 업데이트
        pos += 4;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 Int64 읽기
	///     reads an Int64 in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(ReadOnlySpan<byte> span) {
        // read Big-Endian Int64 and return value / 빅엔디안 Int64 값 읽기 및 반환
        return BinaryPrimitives.ReadInt64BigEndian(span);
    }

	/// <summary>
	///     빅엔디안 Int64를 읽고 위치를 전진
	///     reads an Int64 in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian Int64 value / 값 읽기
        var value = BinaryPrimitives.ReadInt64BigEndian(span[pos..]);
        // advance position by 8 bytes / 위치 업데이트
        pos += 8;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 UInt64 읽기
	///     reads a UInt64 in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ReadOnlySpan<byte> span) {
        // read Big-Endian UInt64 and return value / 빅엔디안 UInt64 값 읽기 및 반환
        return BinaryPrimitives.ReadUInt64BigEndian(span);
    }

	/// <summary>
	///     빅엔디안 UInt64를 읽고 위치를 전진
	///     reads a UInt64 in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian UInt64 value / 값 읽기
        var value = BinaryPrimitives.ReadUInt64BigEndian(span[pos..]);
        // advance position by 8 bytes / 위치 업데이트
        pos += 8;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 Single (float) 읽기
	///     reads a Single (float) in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingle(ReadOnlySpan<byte> span) {
        // read Big-Endian float and return value / 빅엔디안 float 값 읽기 및 반환
        return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span));
    }

	/// <summary>
	///     빅엔디안 Single (float)을 읽고 위치를 전진
	///     reads a Single (float) in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingle(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian float value / 값 읽기
        var value = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span[pos..]));
        // advance position by 4 bytes / 위치 업데이트
        pos += 4;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     빅엔디안 Double 읽기
	///     reads a Double in Big-Endian
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDouble(ReadOnlySpan<byte> span) {
        // read Big-Endian double and return value / 빅엔디안 double 값 읽기 및 반환
        return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span));
    }

	/// <summary>
	///     빅엔디안 Double을 읽고 위치를 전진
	///     reads a Double in Big-Endian and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <returns>읽은 값 / read value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDouble(ReadOnlySpan<byte> span, ref int pos) {
        // read Big-Endian double value / 값 읽기
        var value = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span[pos..]));
        // advance position by 8 bytes / 위치 업데이트
        pos += 8;
        // return the parsed value / 값 반환
        return value;
    }

	/// <summary>
	///     ASCII 문자열을 읽고 위치를 전진
	///     reads an ASCII string and advances position
	/// </summary>
	/// <param name="span">데이터 스팬 / data span</param>
	/// <param name="pos">현재 위치 / current position</param>
	/// <param name="length">읽을 바이트 길이 / byte length to read</param>
	/// <returns>트림된 문자열 / trimmed string</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAsciiString(ReadOnlySpan<byte> span, ref int pos, int length) {
        // decode ASCII string and trim null/whitespace / 문자열 값 읽기
        var value = Encoding.ASCII.GetString(span.Slice(pos, length)).Trim('\0').Trim();
        // advance position by string length / 위치 업데이트
        pos += length;
        // return the trimmed string / 값 반환
        return value;
    }
}