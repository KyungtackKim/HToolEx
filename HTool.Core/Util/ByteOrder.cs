using System.Runtime.CompilerServices;
using HTool.Core.Type.Process;

namespace HTool.Core.Util;

/// <summary>
///     바이트 스팬과 값 간의 엔디안·워드 순서 변환 유틸리티.
///     byte order conversion utility between byte spans and values.
/// </summary>
/// <remarks>
///     Read/Write 메서드는 MODBUS 빅엔디안(isBigEndian=true)을 기본값으로 사용합니다. 32비트 값은 WordOrder로 워드 순서를 지정합니다.
///     Read/Write methods default to MODBUS big-endian (isBigEndian=true). 32-bit values use WordOrder for word ordering.
/// </remarks>
public static class ByteOrder {
    /// <summary>
    ///     바이트 스팬에서 float 값을 읽습니다. 빅/리틀 엔디안과 워드 순서를 지원합니다.
    ///     read float value from byte span. supports big/little-endian and word order.
    /// </summary>
    /// <param name="values">바이트 스팬 (최소 4바이트) / byte span (minimum 4 bytes)</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <param name="isBigEndian">빅엔디안 여부 (기본값: true, MODBUS) / big-endian flag (default: true, MODBUS)</param>
    /// <returns>float 값 (길이 부족 시 0) / float value (0 when span is too short)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadFloat(ReadOnlySpan<byte> values, WordOrder wordOrder = WordOrder.HighLow, bool isBigEndian = true) {
        // 최소 길이(4바이트) 확인
        // check minimum length (4 bytes)
        if (values.Length < 4)
            // float 변환에 바이트가 부족해 0 반환
            // return 0 since span is too short for float conversion
            return 0;

        // float 비트의 중간 int 표현
        // intermediate int representation of float bits
        int intValue;
        // 엔디안 분기
        // branch on endianness
        if (isBigEndian)
            // 빅엔디안: 워드 순서에 따라 조립
            // big-endian: assemble according to word order
            intValue = wordOrder == WordOrder.HighLow
                ? (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3]
                : (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
        else
            // 리틀엔디안: 워드 순서에 따라 조립
            // little-endian: assemble according to word order
            intValue = wordOrder == WordOrder.HighLow
                ? values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24)
                : values[2] | (values[3] << 8) | (values[0] << 16) | (values[1] << 24);

        // int 비트를 float으로 재해석하여 반환
        // reinterpret int bits as float and return
        return BitConverter.Int32BitsToSingle(intValue);
    }

    /// <summary>
    ///     바이트 스팬에서 ushort 값을 읽습니다. 빅/리틀 엔디안을 지원합니다.
    ///     read ushort value from byte span. supports big/little-endian.
    /// </summary>
    /// <param name="values">바이트 스팬 (최소 2바이트) / byte span (minimum 2 bytes)</param>
    /// <param name="isBigEndian">빅엔디안 여부 (기본값: true, MODBUS) / big-endian flag (default: true, MODBUS)</param>
    /// <returns>ushort 값 (길이 부족 시 0) / ushort value (0 when span is too short)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> values, bool isBigEndian = true) {
        // 최소 길이(2바이트) 확인
        // check minimum length (2 bytes)
        if (values.Length < 2)
            // ushort 변환에 바이트가 부족해 0 반환
            // return 0 since span is too short for ushort conversion
            return 0;

        // 엔디안에 따라 조립한 값을 반환
        // assemble and return value based on endianness
        return isBigEndian
            ? (ushort)((values[0] << 8) | values[1])
            : (ushort)(values[0]        | (values[1] << 8));
    }

    /// <summary>
    ///     바이트 스팬에서 int 값을 읽습니다. 빅/리틀 엔디안과 워드 순서를 지원합니다.
    ///     read int value from byte span. supports big/little-endian and word order.
    /// </summary>
    /// <param name="values">바이트 스팬 (정확히 4바이트) / byte span (exactly 4 bytes)</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <param name="isBigEndian">빅엔디안 여부 (기본값: true, MODBUS) / big-endian flag (default: true, MODBUS)</param>
    /// <returns>int 값 (길이 불일치 시 0) / int value (0 when span length mismatches)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> values, WordOrder wordOrder = WordOrder.HighLow, bool isBigEndian = true) {
        // 정확한 길이(4바이트) 확인
        // check exact length (4 bytes)
        if (values.Length != 4)
            // int32 변환에 정확히 4바이트가 필요
            // int32 conversion requires exactly 4 bytes
            return 0;

        // 엔디안 분기
        // branch on endianness
        if (isBigEndian) {
            // 빅엔디안 + HighLow(ABCD) 조립 결과 반환
            // big-endian + HighLow(ABCD) assembled result
            if (wordOrder == WordOrder.HighLow)
                return (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3];
            // 빅엔디안 + LowHigh(CDAB) 조립 결과 반환
            // big-endian + LowHigh(CDAB) assembled result
            return (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
        }

        // 리틀엔디안 + HighLow(DCBA) 조립 결과 반환
        // little-endian + HighLow(DCBA) assembled result
        if (wordOrder == WordOrder.HighLow)
            return values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24);
        // 리틀엔디안 + LowHigh(BADC) 조립 결과 반환
        // little-endian + LowHigh(BADC) assembled result
        return values[2] | (values[3] << 8) | (values[0] << 16) | (values[1] << 24);
    }

    /// <summary>
    ///     int32 값을 스팬의 시작 위치에 기록합니다.
    ///     write int32 value to the start of the span.
    /// </summary>
    /// <param name="span">대상 스팬 (최소 4바이트) / destination span (minimum 4 bytes)</param>
    /// <param name="value">기록할 int32 값 / int32 value to write</param>
    /// <param name="isBigEndian">빅엔디안 여부 (기본값: true, MODBUS) / big-endian flag (default: true, MODBUS)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32(Span<byte> span, int value, bool isBigEndian = true) {
        // 엔디안 분기
        // branch on endianness
        if (isBigEndian) {
            // 최상위 바이트 기록
            // write most significant byte
            span[0] = (byte)(value >> 24);
            // 두 번째 바이트 기록
            // write second byte
            span[1] = (byte)(value >> 16);
            // 세 번째 바이트 기록
            // write third byte
            span[2] = (byte)(value >> 8);
            // 최하위 바이트 기록
            // write least significant byte
            span[3] = (byte)value;
        } else {
            // 최하위 바이트 기록
            // write least significant byte
            span[0] = (byte)value;
            // 두 번째 바이트 기록
            // write second byte
            span[1] = (byte)(value >> 8);
            // 세 번째 바이트 기록
            // write third byte
            span[2] = (byte)(value >> 16);
            // 최상위 바이트 기록
            // write most significant byte
            span[3] = (byte)(value >> 24);
        }
    }

    /// <summary>
    ///     ushort 값을 스팬의 시작 위치에 기록합니다.
    ///     write ushort value to the start of the span.
    /// </summary>
    /// <param name="span">대상 스팬 (최소 2바이트) / destination span (minimum 2 bytes)</param>
    /// <param name="value">기록할 ushort 값 / ushort value to write</param>
    /// <param name="isBigEndian">빅엔디안 여부 (기본값: true, MODBUS) / big-endian flag (default: true, MODBUS)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16(Span<byte> span, ushort value, bool isBigEndian = true) {
        // 엔디안 분기
        // branch on endianness
        if (isBigEndian) {
            // 상위 바이트 먼저 기록
            // write high byte first
            span[0] = (byte)(value >> 8);
            // 하위 바이트 기록
            // write low byte
            span[1] = (byte)value;
        } else {
            // 하위 바이트 먼저 기록
            // write low byte first
            span[0] = (byte)value;
            // 상위 바이트 기록
            // write high byte
            span[1] = (byte)(value >> 8);
        }
    }

    /// <summary>
    ///     float 값을 스팬의 시작 위치에 기록합니다.
    ///     write float value to the start of the span.
    /// </summary>
    /// <param name="span">대상 스팬 (최소 4바이트) / destination span (minimum 4 bytes)</param>
    /// <param name="value">기록할 float 값 / float value to write</param>
    /// <param name="isBigEndian">빅엔디안 여부 (기본값: true, MODBUS) / big-endian flag (default: true, MODBUS)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteFloat(Span<byte> span, float value, bool isBigEndian = true) {
        // float을 int 비트로 재해석
        // reinterpret float as int bits
        var bits = BitConverter.SingleToInt32Bits(value);
        // int32 기록 로직에 위임
        // delegate to int32 writer
        WriteInt32(span, bits, isBigEndian);
    }
}