using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HTool.Type;

namespace HTool.Util;

/// <summary>
///     MODBUS-RTU CRC-16 계산 및 기타 범용 유틸리티 클래스. 룩업 테이블 기반 고속 CRC 계산을 제공합니다.
///     MODBUS-RTU CRC-16 calculation and other general utilities class. Provides high-speed lookup table-based CRC
///     calculation.
/// </summary>
/// <remarks>
///     CRC-16/MODBUS 알고리즘 (Polynomial: 0xA001)을 사용합니다. RTU 프로토콜의 프레임 검증에 필수적입니다.
///     Uses CRC-16/MODBUS algorithm (Polynomial: 0xA001). Essential for RTU protocol frame validation.
/// </remarks>
public static class Utils {
    /// <summary>
    ///     MODBUS-RTU CRC-16 룩업 테이블 (256개 항목)
    ///     MODBUS-RTU CRC-16 lookup table (256 entries)
    /// </summary>
    private static readonly ushort[] ModbusCrc16Table = [
        0x0000,
        0xC0C1,
        0xC181,
        0x0140,
        0xC301,
        0x03C0,
        0x0280,
        0xC241,
        0xC601,
        0x06C0,
        0x0780,
        0xC741,
        0x0500,
        0xC5C1,
        0xC481,
        0x0440,
        0xCC01,
        0x0CC0,
        0x0D80,
        0xCD41,
        0x0F00,
        0xCFC1,
        0xCE81,
        0x0E40,
        0x0A00,
        0xCAC1,
        0xCB81,
        0x0B40,
        0xC901,
        0x09C0,
        0x0880,
        0xC841,
        0xD801,
        0x18C0,
        0x1980,
        0xD941,
        0x1B00,
        0xDBC1,
        0xDA81,
        0x1A40,
        0x1E00,
        0xDEC1,
        0xDF81,
        0x1F40,
        0xDD01,
        0x1DC0,
        0x1C80,
        0xDC41,
        0x1400,
        0xD4C1,
        0xD581,
        0x1540,
        0xD701,
        0x17C0,
        0x1680,
        0xD641,
        0xD201,
        0x12C0,
        0x1380,
        0xD341,
        0x1100,
        0xD1C1,
        0xD081,
        0x1040,
        0xF001,
        0x30C0,
        0x3180,
        0xF141,
        0x3300,
        0xF3C1,
        0xF281,
        0x3240,
        0x3600,
        0xF6C1,
        0xF781,
        0x3740,
        0xF501,
        0x35C0,
        0x3480,
        0xF441,
        0x3C00,
        0xFCC1,
        0xFD81,
        0x3D40,
        0xFF01,
        0x3FC0,
        0x3E80,
        0xFE41,
        0xFA01,
        0x3AC0,
        0x3B80,
        0xFB41,
        0x3900,
        0xF9C1,
        0xF881,
        0x3840,
        0x2800,
        0xE8C1,
        0xE981,
        0x2940,
        0xEB01,
        0x2BC0,
        0x2A80,
        0xEA41,
        0xEE01,
        0x2EC0,
        0x2F80,
        0xEF41,
        0x2D00,
        0xEDC1,
        0xEC81,
        0x2C40,
        0xE401,
        0x24C0,
        0x2580,
        0xE541,
        0x2700,
        0xE7C1,
        0xE681,
        0x2640,
        0x2200,
        0xE2C1,
        0xE381,
        0x2340,
        0xE101,
        0x21C0,
        0x2080,
        0xE041,
        0xA001,
        0x60C0,
        0x6180,
        0xA141,
        0x6300,
        0xA3C1,
        0xA281,
        0x6240,
        0x6600,
        0xA6C1,
        0xA781,
        0x6740,
        0xA501,
        0x65C0,
        0x6480,
        0xA441,
        0x6C00,
        0xACC1,
        0xAD81,
        0x6D40,
        0xAF01,
        0x6FC0,
        0x6E80,
        0xAE41,
        0xAA01,
        0x6AC0,
        0x6B80,
        0xAB41,
        0x6900,
        0xA9C1,
        0xA881,
        0x6840,
        0x7800,
        0xB8C1,
        0xB981,
        0x7940,
        0xBB01,
        0x7BC0,
        0x7A80,
        0xBA41,
        0xBE01,
        0x7EC0,
        0x7F80,
        0xBF41,
        0x7D00,
        0xBDC1,
        0xBC81,
        0x7C40,
        0xB401,
        0x74C0,
        0x7580,
        0xB541,
        0x7700,
        0xB7C1,
        0xB681,
        0x7640,
        0x7200,
        0xB2C1,
        0xB381,
        0x7340,
        0xB101,
        0x71C0,
        0x7080,
        0xB041,
        0x5000,
        0x90C1,
        0x9181,
        0x5140,
        0x9301,
        0x53C0,
        0x5280,
        0x9241,
        0x9601,
        0x56C0,
        0x5780,
        0x9741,
        0x5500,
        0x95C1,
        0x9481,
        0x5440,
        0x9C01,
        0x5CC0,
        0x5D80,
        0x9D41,
        0x5F00,
        0x9FC1,
        0x9E81,
        0x5E40,
        0x5A00,
        0x9AC1,
        0x9B81,
        0x5B40,
        0x9901,
        0x59C0,
        0x5880,
        0x9841,
        0x8801,
        0x48C0,
        0x4980,
        0x8941,
        0x4B00,
        0x8BC1,
        0x8A81,
        0x4A40,
        0x4E00,
        0x8EC1,
        0x8F81,
        0x4F40,
        0x8D01,
        0x4DC0,
        0x4C80,
        0x8C41,
        0x4400,
        0x84C1,
        0x8581,
        0x4540,
        0x8701,
        0x47C0,
        0x4680,
        0x8641,
        0x8201,
        0x42C0,
        0x4380,
        0x8341,
        0x4100,
        0x81C1,
        0x8081,
        0x4040
    ];

    /// <summary>
    ///     패킷의 CRC 값 계산
    ///     Calculate CRC value for the packet
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <returns>결과 (low, high) / result (low, high)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte low, byte high) CalculateCrc(ReadOnlySpan<byte> packet) {
        ushort crc = 0xFFFF;
        // 패킷 순회
        // iterate through packet
        foreach (var b in packet) {
            // 인덱스 계산
            // calculate index
            var index = (byte)(crc ^ b);
            // CRC 계산
            // calculate CRC
            crc = (ushort)((crc >> 8) ^ ModbusCrc16Table[index]);
        }

        // 결과 반환
        // return result
        return ((byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF));
    }

    /// <summary>
    ///     패킷의 CRC 값을 계산하여 버퍼에 저장
    ///     Calculate CRC value for the packet and store in buffer
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <param name="buffer">CRC 버퍼 / CRC buffer</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CalculateCrcTo(ReadOnlySpan<byte> packet, Span<byte> buffer) {
        // 버퍼 크기 확인
        // check buffer size
        if (buffer.Length < 2)
            // 예외 발생
            // throw exception
            throw new ArgumentException("Buffer must be at least 2 bytes", nameof(buffer));

        ushort crc = 0xFFFF;
        // 패킷 순회
        // iterate through packet
        foreach (var b in packet) {
            // 인덱스 계산
            // calculate index
            var index = (byte)(crc ^ b);
            // CRC 계산
            // calculate CRC
            crc = (ushort)((crc >> 8) ^ ModbusCrc16Table[index]);
        }

        // CRC 설정
        // set CRC
        buffer[0] = (byte)(crc        & 0xFF);
        buffer[1] = (byte)((crc >> 8) & 0xFF);
    }

    /// <summary>
    ///     패킷의 CRC 값 검증
    ///     Validate the CRC value from the packet
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <returns>검증 결과 / validation result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ValidateCrc(ReadOnlySpan<byte> packet) {
        // 길이 확인
        // check length
        if (packet.Length < 3)
            return false;
        // 데이터 스팬 가져오기
        // get data span
        var data = packet[..^2];
        // 수신된 CRC 값 가져오기
        // get received CRC value
        var receivedLow  = packet[^2];
        var receivedHigh = packet[^1];
        // CRC 값 계산
        // calculate CRC value
        var (low, high) = CalculateCrc(data);
        // 결과 반환
        // return result
        return receivedLow == low && receivedHigh == high;
    }

    /// <summary>
    ///     체크섬 계산
    ///     Calculate the check sum
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <returns>합계 / sum</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateCheckSum(ReadOnlySpan<byte> packet) {
        var sum = 0;
        // 패킷 순회
        // iterate through packet
        foreach (var value in packet)
            // 합계에 추가
            // add to sum
            sum += value;
        // 합계 반환
        // return sum
        return sum;
    }

    /// <summary>
    ///     체크섬 계산 (고속)
    ///     Calculate the check sum (fast)
    /// </summary>
    /// <param name="span">스팬 / span</param>
    /// <returns>합계 / sum</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateCheckSumFast(ReadOnlySpan<byte> span) {
        var sum = 0;

        // 4바이트 값으로 변환
        // cast to 4-byte values
        var intSpan = MemoryMarshal.Cast<byte, int>(span);
        // int 스팬 순회
        // iterate through int span
        foreach (var intVal in intSpan)
            // 값 추가
            // add value
            sum += (intVal & 0xFF) + ((intVal >> 8)  & 0xFF) +
                   ((intVal                   >> 16) & 0xFF) + ((intVal >> 24) & 0xFF);

        // 남은 길이 계산
        // get remaining length
        var remaining = span.Length % 4;
        // 남은 길이 확인
        // check remaining length
        if (remaining < 1)
            // 합계 반환
            // return sum
            return sum;
        // 오프셋 계산
        // calculate offset
        var offset = span.Length - remaining;
        // 남은 바이트 처리
        // process remaining bytes
        for (var i = 0; i < remaining; i++)
            // 값 추가
            // add value
            sum += span[offset + i];
        // 합계 반환
        // return sum
        return sum;
    }

    /// <summary>
    ///     토크 단위 변환
    ///     Convert torque unit
    /// </summary>
    /// <param name="value">토크 값 / torque value</param>
    /// <param name="src">원본 단위 / source unit</param>
    /// <param name="dst">대상 단위 / destination unit</param>
    /// <returns>변환된 토크 / converted torque</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertTorqueUnit(float value, UnitTypes src, UnitTypes dst) {
        // N.m 단위 변환 상수
        // conversion constants for N.m unit
        const float nCm   = 100.0f;
        const float kgfM  = 0.101971621f;
        const float kgfCm = 10.1971621f;
        const float lbfIn = 8.85074579f;
        const float lbfFt = 0.737562149f;
        const float ozfIn = 141.611932f;
        // 원본 단위를 N.m으로 변환
        // convert source to N.m
        var valueInNm = src switch {
            UnitTypes.KgfCm => value / kgfCm,
            UnitTypes.KgfM  => value / kgfM,
            UnitTypes.Nm    => value,
            UnitTypes.NCm   => value / nCm,
            UnitTypes.LbfIn => value / lbfIn,
            UnitTypes.OzfIn => value / ozfIn,
            UnitTypes.LbfFt => value / lbfFt,
            _               => value
        };
        // N.m을 대상 단위로 변환
        // convert N.m to destination
        return dst switch {
            UnitTypes.KgfCm => valueInNm * kgfCm,
            UnitTypes.KgfM  => valueInNm * kgfM,
            UnitTypes.Nm    => valueInNm,
            UnitTypes.NCm   => valueInNm * nCm,
            UnitTypes.LbfIn => valueInNm * lbfIn,
            UnitTypes.OzfIn => valueInNm * ozfIn,
            UnitTypes.LbfFt => valueInNm * lbfFt,
            _               => valueInNm
        };
    }

    /// <summary>
    ///     문자열에서 단위로 변환
    ///     Convert string to unit
    /// </summary>
    /// <param name="value">값 / value</param>
    /// <returns>단위 / unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnitTypes ParseToUnit(string value) {
        // null 또는 빈 문자열 확인
        // check null or empty
        if (string.IsNullOrEmpty(value))
            return UnitTypes.KgfCm;
        // 단위로 변환
        // convert to unit
        return value.ToLowerInvariant() switch {
            "kgf.cm" => UnitTypes.KgfCm,
            "kgf.m"  => UnitTypes.KgfM,
            "n.m"    => UnitTypes.Nm,
            "n.cm"   => UnitTypes.NCm,
            "lbf.in" => UnitTypes.LbfIn,
            "ozf.in" => UnitTypes.OzfIn,
            "lbf.ft" => UnitTypes.LbfFt,
            _        => UnitTypes.KgfCm
        };
    }

    /// <summary>
    ///     단위 타입을 문자열로 변환
    ///     Convert unit type to string representation
    /// </summary>
    /// <param name="value">정수형 단위 타입 (0=kgf.cm, 1=kgf.m, ...) / unit type as integer (0=kgf.cm, 1=kgf.m, ...)</param>
    /// <returns>단위 문자열 / unit string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseToUnit(int value) {
        return value switch {
            0 => "kgf.cm",
            1 => "kgf.m",
            2 => "N.m",
            3 => "N.cm",
            4 => "ozf.in",
            5 => "lbf.ft",
            6 => "ozf.ft",
            _ => "kgf.cm"
        };
    }

    /// <summary>
    ///     현재 틱 가져오기
    ///     Get the current tick
    /// </summary>
    /// <returns>현재 틱 / current tick</returns>
    public static long GetCurrentTicks() {
        return Environment.TickCount64;
    }

    /// <summary>
    ///     밀리초 단위 경과 시간 가져오기
    ///     Get the elapsed time in milliseconds
    /// </summary>
    /// <param name="from">시작 시간 / from time</param>
    /// <returns>총 밀리초 / total milliseconds</returns>
    public static long TimeLapsMs(DateTime from) {
        return (long)(DateTime.Now - from).TotalMilliseconds;
    }

    /// <summary>
    ///     초 단위 경과 시간 가져오기
    ///     Get the elapsed time in seconds
    /// </summary>
    /// <param name="from">시작 시간 / from time</param>
    /// <returns>총 초 / total seconds</returns>
    public static long TimeLapsSec(DateTime from) {
        return (long)(DateTime.Now - from).TotalSeconds;
    }

    /// <summary>
    ///     두 시간의 차이를 초 단위로 가져오기
    ///     Get the time difference in seconds
    /// </summary>
    /// <param name="src">원본 시간 / source time</param>
    /// <param name="dst">대상 시간 / destination time</param>
    /// <returns>초 / seconds</returns>
    public static ulong TimeLapsDifference(DateTime src, DateTime dst) {
        // 시간 차이 계산
        // calculate time difference
        return (ulong)Math.Abs((long)(src - dst).TotalSeconds);
    }

    /// <summary>
    ///     바이트 배열에서 float 값으로 변환
    ///     Convert byte array to float value
    /// </summary>
    /// <param name="values">값 배열 / value array</param>
    /// <param name="value">float 값 / float value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(ReadOnlySpan<byte> values, out float value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // 길이 확인
        // check length
        if (values.Length < 4) {
            // 값 초기화
            // reset value
            value = 0;
        } else {
            // 워드 순서에 따라 int 값 가져오기
            // get int value based on word order
            var intValue = wordOrder == WordOrderTypes.HighLow
                // ABCD: 상위 워드 먼저 (가장 일반적)
                // ABCD: High word first (most common)
                ? (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3]
                // CDAB: 하위 워드 먼저
                // CDAB: Low word first
                : (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
            // 값 설정
            // set value
            value = BitConverter.Int32BitsToSingle(intValue);
        }
    }

    /// <summary>
    ///     바이트 배열에서 ushort 값으로 변환
    ///     Convert byte array to ushort value
    /// </summary>
    /// <param name="values">값 배열 / value array</param>
    /// <param name="value">ushort 값 / ushort value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(ReadOnlySpan<byte> values, out ushort value) {
        // 값 설정
        // set value
        value = values.Length >= 2 ? (ushort)((values[0] << 8) | values[1]) : (ushort)0;
    }

    /// <summary>
    ///     바이트 배열에서 int 값으로 변환
    ///     Convert byte array to int value
    /// </summary>
    /// <param name="values">값 배열 / value array</param>
    /// <param name="value">int 값 / int value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(ReadOnlySpan<byte> values, out int value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // 기본값 설정
        // set default value
        value = 0;
        // 길이 확인
        // check length
        if (values.Length != 4)
            return;
        // 워드 순서 확인
        // check word order
        if (wordOrder == WordOrderTypes.HighLow)
            // ABCD: 상위 워드 먼저 (가장 일반적)
            // ABCD: High word first (most common)
            value = (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3];
        else
            // CDAB: 하위 워드 먼저
            // CDAB: Low word first
            value = (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
    }

    /// <summary>
    ///     바이트 스팬을 16진수 문자열로 변환
    ///     Convert byte span to hex string
    /// </summary>
    /// <param name="values">바이트 스팬 / byte span</param>
    /// <param name="separator">구분자 (기본값: 공백) / separator (default: space)</param>
    /// <param name="lineBreakAt">줄바꿈 위치 (0 = 줄바꿈 없음) / line break position (0 = no break)</param>
    /// <returns>16진수 문자열 / hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ConvertHexString(ReadOnlySpan<byte> values, string separator = " ", int lineBreakAt = 16) {
        // 빈 값 확인
        // check empty
        if (values.IsEmpty)
            // 빈 문자열 반환
            // return empty string
            return string.Empty;

        // StringBuilder 생성
        // create StringBuilder
        var sb = new StringBuilder(values.Length * 3);
        // 배열 순회
        // iterate through array
        for (var i = 0; i < values.Length; i++) {
            // 16진수 문자열 추가
            // append hex string
            sb.Append(values[i].ToString("X2"));
            // 인덱스 확인
            // check index
            if (i < values.Length - 1)
                // 구분자 추가
                // append separator
                sb.Append(separator);
            // 줄바꿈 확인
            // check line break
            if (lineBreakAt > 0 && (i + 1) % lineBreakAt == 0)
                // 줄바꿈 추가
                // append line break
                sb.AppendLine();
        }

        // 16진수 문자열 반환
        // return hex string
        return sb.ToString();
    }

    /// <summary>
    ///     바이트 배열에서 float 값으로 변환
    ///     Convert byte array to float value
    /// </summary>
    /// <param name="values">값 배열 / value array</param>
    /// <param name="value">float 값 / float value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(byte[] values, out float value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        ConvertValue(values.AsSpan(), out value, wordOrder);
    }

    /// <summary>
    ///     바이트 배열에서 ushort 값으로 변환
    ///     Convert byte array to ushort value
    /// </summary>
    /// <param name="values">값 배열 / value array</param>
    /// <param name="value">ushort 값 / ushort value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(byte[] values, out ushort value) {
        ConvertValue(values.AsSpan(), out value);
    }

    /// <summary>
    ///     바이트 배열에서 int 값으로 변환
    ///     Convert byte array to int value
    /// </summary>
    /// <param name="values">값 배열 / value array</param>
    /// <param name="value">int 값 / int value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(byte[] values, out int value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        ConvertValue(values.AsSpan(), out value, wordOrder);
    }

    /// <summary>
    ///     바이트 배열을 16진수 문자열로 변환
    ///     Convert byte array to hex string
    /// </summary>
    /// <param name="values">바이트 배열 / byte array</param>
    /// <param name="separator">구분자 (기본값: 공백) / separator (default: space)</param>
    /// <param name="lineBreakAt">줄바꿈 위치 (0 = 줄바꿈 없음) / line break position (0 = no break)</param>
    /// <returns>16진수 문자열 / hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ConvertHexString(byte[] values, string separator = " ", int lineBreakAt = 16) {
        return ConvertHexString(values.AsSpan(), separator, lineBreakAt);
    }

    /// <summary>
    ///     텍스트에서 ushort 배열 가져오기
    ///     Get ushort array from text
    /// </summary>
    /// <param name="text">텍스트 / text</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetWordValuesFromText(string text) {
        // 텍스트 확인
        // check text
        if (string.IsNullOrEmpty(text))
            return [];

        // 텍스트 스팬 가져오기
        // get text span
        var span = text.AsSpan();
        // 길이 계산
        // calculate length
        var length = span.Length / 2;
        // 남은 길이 계산
        // calculate remaining length
        var remain = span.Length % 2;
        // 결과 배열 생성
        // create result array
        var result = new ushort[length + remain];
        // 배열 길이만큼 반복
        // iterate through array length
        for (var i = 0; i < length; i++)
            // 값 설정
            // set value
            result[i] = (ushort)((span[i * 2] << 8) | span[i * 2 + 1]);
        // 남은 오프셋 확인
        // check remaining offset
        if (remain > 0)
            // 값 설정
            // set value
            result[length] = (ushort)(span[^1] << 8);
        // 결과 반환
        // return result
        return result;
    }

    /// <summary>
    ///     텍스트에서 정수 배열 가져오기
    ///     Get integer array from text
    /// </summary>
    /// <param name="text">텍스트 / text</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetIntValuesFromText(string text) {
        // 텍스트 확인
        // check text
        if (string.IsNullOrEmpty(text))
            return [];
        // 길이 계산
        // calculate length
        var len = text.Length >> 1;
        // 결과 배열 생성
        // create result array
        var result = new ushort[len];
        // 스팬 가져오기
        // get span
        var span = text.AsSpan();
        // 길이만큼 반복
        // iterate through length
        for (int i = 0, j = 0; i < len; i++, j += 2) {
            // 데이터 가져오기
            // get data
            var d1 = span[j]     - '0';
            var d2 = span[j + 1] - '0';
            // 결과 설정
            // set result
            result[i] = (ushort)(d1 * 10 + d2);
        }

        // 값 배열 반환
        // return value array
        return result;
    }

    /// <summary>
    ///     네트워크 주소에서 ushort 배열 가져오기
    ///     Get ushort array from network address
    /// </summary>
    /// <param name="addr">주소 / address</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetValuesFromAddr(string addr) {
        var count = 0;
        var acc   = 0;
        // 스팬 가져오기
        // get span
        var span = addr.AsSpan();
        // 공간 할당
        // allocate space
        Span<ushort> values = stackalloc ushort[8];
        // 길이만큼 반복
        // iterate through length
        for (var i = 0; i <= span.Length; i++)
            // 길이 및 점 확인
            // check length and dot
            if (i == span.Length || span[i] == '.') {
                // 값 설정
                // set value
                values[count++] = (ushort)acc;
                // 누적값 초기화
                // reset accumulator
                acc = 0;
            } else {
                // 누적값 설정
                // set accumulator
                acc = acc * 10 + (span[i] - '0');
            }

        // 결과 배열 생성
        // create result array
        var result = new ushort[count];
        // 데이터 복사
        // copy data
        values[..count].CopyTo(result);
        // 데이터 반환
        // return data
        return result;
    }

    /// <summary>
    ///     int32 값에서 ushort 배열 가져오기
    ///     Get ushort array from int32 value
    /// </summary>
    /// <param name="value">int 값 / int value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetValuesFromValue(int value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // 상위 및 하위 워드 가져오기
        // get high and low words
        var highWord = (ushort)(value >> 16);
        var lowWord  = (ushort)value;
        // 워드 순서에 따라 반환
        // return based on word order
        return wordOrder == WordOrderTypes.HighLow
            ? [highWord, lowWord]  // ABCD: 상위 워드 먼저 / High word first
            : [lowWord, highWord]; // CDAB: 하위 워드 먼저 / Low word first
    }

    /// <summary>
    ///     float 값에서 ushort 배열 가져오기
    ///     Get ushort array from float value
    /// </summary>
    /// <param name="value">float 값 / float value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetValuesFromValue(float value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // 비트 가져오기
        // get bits
        var bits = BitConverter.SingleToInt32Bits(value);
        // 상위 및 하위 워드 가져오기
        // get high and low words
        var highWord = (ushort)(bits >> 16);
        var lowWord  = (ushort)bits;
        // 워드 순서에 따라 반환
        // return based on word order
        return wordOrder == WordOrderTypes.HighLow
            ? [highWord, lowWord]  // ABCD: 상위 워드 먼저 / High word first
            : [lowWord, highWord]; // CDAB: 하위 워드 먼저 / Low word first
    }

    /// <summary>
    ///     리스트 항목 교환
    ///     Swap list items
    /// </summary>
    /// <param name="list">리스트 / list</param>
    /// <param name="source">원본 인덱스 / source index</param>
    /// <param name="dest">대상 인덱스 / destination index</param>
    /// <typeparam name="T">항목 타입 / item type</typeparam>
    /// <returns>결과 / result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Swap<T>(List<T> list, int source, int dest) {
        // 리스트 확인
        // check list
        ArgumentNullException.ThrowIfNull(list);
        // 인덱스 확인
        // check index
        if (source < 0 || source >= list.Count || dest < 0 || dest >= list.Count)
            return false;
        // 교환
        // swap
        (list[source], list[dest]) = (list[dest], list[source]);
        // 결과 반환
        // return result
        return true;
    }

    /// <summary>
    ///     바이트 배열을 ASCII 문자열로 변환 (후행 null 문자 제거)
    ///     Convert bytes to ASCII string, trimming trailing null characters
    /// </summary>
    /// <param name="span">스팬 / span</param>
    /// <returns>문자열 / string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToAsciiTrimEnd(ReadOnlySpan<byte> span) {
        // 길이 가져오기
        // get length
        var end = span.Length;
        // null 확인
        // check null
        while (end > 0 && span[end - 1] == 0)
            // null 제거
            // trim null
            end--;
        // 문자열 반환
        // return string
        return end == 0 ? string.Empty : Encoding.ASCII.GetString(span[..end]);
    }
}