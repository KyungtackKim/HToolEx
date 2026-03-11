using System.Runtime.CompilerServices;
using System.Text;
using HTool.Core.Type.Process;

namespace HTool.Core.Util;

/// <summary>
///     MODBUS-RTU CRC-16 계산 및 바이트 변환 유틸리티 클래스. 룩업 테이블 기반 고속 CRC 계산과 빅/리틀 엔디안 바이트 변환을 제공합니다.
///     MODBUS-RTU CRC-16 calculation and byte conversion utility class. Provides high-speed lookup table-based CRC
///     calculation and big/little-endian byte conversions.
/// </summary>
/// <remarks>
///     CRC-16/MODBUS 알고리즘 (Polynomial: 0xA001)을 사용합니다. RTU 프로토콜의 프레임 검증에 필수적입니다.
///     Read/Write 메서드는 MODBUS 빅엔디안(isBigEndian=true)을 기본값으로 사용합니다.
///     uses CRC-16/MODBUS algorithm (Polynomial: 0xA001). Essential for RTU protocol frame validation.
///     Read/Write methods default to MODBUS big-endian (isBigEndian=true).
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

    #region Torque Conversion

    /// <summary>
    ///     토크 단위 변환
    ///     convert torque unit
    /// </summary>
    /// <param name="value">토크 값 / torque value</param>
    /// <param name="src">원본 단위 / source unit</param>
    /// <param name="dst">대상 단위 / destination unit</param>
    /// <returns>변환된 토크 / converted torque</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertTorque(float value, Unit src, Unit dst) {
        // N.cm per N.m conversion factor / N.cm ↔ N.m 변환 계수
        const float nCm = 100.0f;
        // kgf.m per N.m conversion factor / kgf.m ↔ N.m 변환 계수
        const float kgfM = 0.101971621f;
        // kgf.cm per N.m conversion factor / kgf.cm ↔ N.m 변환 계수
        const float kgfCm = 10.1971621f;
        // lbf.in per N.m conversion factor / lbf.in ↔ N.m 변환 계수
        const float lbfIn = 8.85074579f;
        // lbf.ft per N.m conversion factor / lbf.ft ↔ N.m 변환 계수
        const float lbfFt = 0.737562149f;
        // ozf.in per N.m conversion factor / ozf.in ↔ N.m 변환 계수
        const float ozfIn = 141.611932f;
        // convert source unit to N.m / 원본 단위를 N.m으로 변환
        var valueInNm = src switch {
            Unit.KgfCm => value / kgfCm,
            Unit.KgfM  => value / kgfM,
            Unit.Nm    => value,
            Unit.NCm   => value / nCm,
            Unit.LbfIn => value / lbfIn,
            Unit.OzfIn => value / ozfIn,
            Unit.LbfFt => value / lbfFt,
            _          => value
        };
        // convert N.m to destination unit / N.m을 대상 단위로 변환
        return dst switch {
            Unit.KgfCm => valueInNm * kgfCm,
            Unit.KgfM  => valueInNm * kgfM,
            Unit.Nm    => valueInNm,
            Unit.NCm   => valueInNm * nCm,
            Unit.LbfIn => valueInNm * lbfIn,
            Unit.OzfIn => valueInNm * ozfIn,
            Unit.LbfFt => valueInNm * lbfFt,
            _          => valueInNm
        };
    }

    #endregion

    #region CRC

    /// <summary>
    ///     패킷의 CRC 값 계산
    ///     calculate CRC value for the packet
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <returns>결과 (low, high) / result (low, high)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte low, byte high) CalculateCrc(ReadOnlySpan<byte> packet) {
        // initialize CRC to default value / CRC 초기값 설정
        ushort crc = 0xFFFF;
        // iterate through each byte in packet / 패킷 순회
        foreach (var b in packet) {
            // calculate lookup table index / 인덱스 계산
            var index = (byte)(crc ^ b);
            // compute CRC using lookup table / CRC 계산
            crc = (ushort)((crc >> 8) ^ ModbusCrc16Table[index]);
        }

        // return low and high CRC bytes / 결과 반환
        return ((byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF));
    }

    /// <summary>
    ///     패킷의 CRC 값을 계산하여 버퍼에 저장
    ///     calculate CRC value for the packet and store in buffer
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <param name="buffer">CRC 버퍼 / CRC buffer</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CalculateCrcTo(ReadOnlySpan<byte> packet, Span<byte> buffer) {
        // ensure buffer has at least 2 bytes / 버퍼 크기 확인
        if (buffer.Length < 2)
            // throw argument exception for insufficient buffer size / 버퍼 크기 부족 인자 예외 발생
            throw new ArgumentException("Buffer must be at least 2 bytes", nameof(buffer));

        // initialize CRC to default value / CRC 초기값 설정
        ushort crc = 0xFFFF;
        // iterate through each byte in packet / 패킷 순회
        foreach (var b in packet) {
            // calculate lookup table index / 인덱스 계산
            var index = (byte)(crc ^ b);
            // compute CRC using lookup table / CRC 계산
            crc = (ushort)((crc >> 8) ^ ModbusCrc16Table[index]);
        }

        // set low CRC byte / CRC 하위 바이트 설정
        buffer[0] = (byte)(crc & 0xFF);
        // set high CRC byte / CRC 상위 바이트 설정
        buffer[1] = (byte)((crc >> 8) & 0xFF);
    }

    /// <summary>
    ///     패킷의 CRC 값 검증
    ///     validate the CRC value from the packet
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <returns>검증 결과 / validation result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ValidateCrc(ReadOnlySpan<byte> packet) {
        // check minimum packet length / 길이 확인
        if (packet.Length < 3)
            // packet too short for CRC validation / 패킷이 CRC 검증에 너무 짧음
            return false;
        // get data portion excluding CRC bytes / 데이터 스팬 가져오기
        var data = packet[..^2];
        // get received low CRC byte / 수신된 CRC 하위 바이트 가져오기
        var receivedLow = packet[^2];
        // get received high CRC byte / 수신된 CRC 상위 바이트 가져오기
        var receivedHigh = packet[^1];
        // calculate expected CRC value / CRC 값 계산
        var (low, high) = CalculateCrc(data);
        // compare received and calculated CRC / 결과 반환
        return receivedLow == low && receivedHigh == high;
    }

    #endregion

    #region Read (byte → value)

    /// <summary>
    ///     바이트 스팬에서 float 값 읽기 (빅/리틀 엔디안, 워드 순서 지원)
    ///     read float value from byte span (supports big/little-endian and word order)
    /// </summary>
    /// <param name="values">바이트 스팬 (최소 4바이트) / byte span (minimum 4 bytes)</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <param name="isBigEndian">
    ///     true: 빅엔디안 (MODBUS 기본), false: 리틀엔디안 / true: big-endian (MODBUS default), false:
    ///     little-endian
    /// </param>
    /// <returns>float 값 / float value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadFloat(ReadOnlySpan<byte> values, WordOrder wordOrder = WordOrder.HighLow, bool isBigEndian = true) {
        // check minimum length requirement / 길이 확인
        if (values.Length < 4)
            // insufficient bytes for float conversion / float 변환에 바이트 부족
            return 0;

        // intermediate int representation of float bits / float 비트의 중간 int 표현
        int intValue;
        // check byte order endianness / 엔디안 확인
        if (isBigEndian)
            // assemble int from big-endian byte order / 빅엔디안 바이트 순서
            intValue = wordOrder == WordOrder.HighLow
                // ABCD: high word first (most common) / ABCD: 상위 워드 먼저 (가장 일반적)
                ? (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3]
                // CDAB: low word first / CDAB: 하위 워드 먼저
                : (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
        else
            // assemble int from little-endian byte order / 리틀엔디안 바이트 순서
            intValue = wordOrder == WordOrder.HighLow
                // DCBA: standard little-endian / DCBA: 리틀엔디안
                ? values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24)
                // BADC: little-endian with word swap / BADC: 리틀엔디안 + 워드 스왑
                : values[2] | (values[3] << 8) | (values[0] << 16) | (values[1] << 24);

        // convert int bits to float / float 값 반환
        return BitConverter.Int32BitsToSingle(intValue);
    }

    /// <summary>
    ///     바이트 스팬에서 ushort 값 읽기 (빅/리틀 엔디안 지원)
    ///     read ushort value from byte span (supports big/little-endian)
    /// </summary>
    /// <param name="values">바이트 스팬 (최소 2바이트) / byte span (minimum 2 bytes)</param>
    /// <param name="isBigEndian">
    ///     true: 빅엔디안 (MODBUS 기본), false: 리틀엔디안 / true: big-endian (MODBUS default), false:
    ///     little-endian
    /// </param>
    /// <returns>ushort 값 / ushort value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ReadOnlySpan<byte> values, bool isBigEndian = true) {
        // check minimum length requirement / 길이 확인
        if (values.Length < 2)
            // insufficient bytes for ushort conversion / ushort 변환에 바이트 부족
            return 0;

        // return value based on endianness / 엔디안에 따라 값 반환
        return isBigEndian
            ? (ushort)((values[0] << 8) | values[1])
            : (ushort)(values[0]        | (values[1] << 8));
    }

    /// <summary>
    ///     바이트 스팬에서 int 값 읽기 (빅/리틀 엔디안, 워드 순서 지원)
    ///     read int value from byte span (supports big/little-endian and word order)
    /// </summary>
    /// <param name="values">바이트 스팬 (정확히 4바이트) / byte span (exactly 4 bytes)</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <param name="isBigEndian">
    ///     true: 빅엔디안 (MODBUS 기본), false: 리틀엔디안 / true: big-endian (MODBUS default), false:
    ///     little-endian
    /// </param>
    /// <returns>int 값 / int value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ReadOnlySpan<byte> values, WordOrder wordOrder = WordOrder.HighLow, bool isBigEndian = true) {
        // check exact length requirement / 길이 확인
        if (values.Length != 4)
            // span must be exactly 4 bytes for int32 / int32에는 정확히 4바이트 필요
            return 0;

        // check byte order endianness / 엔디안 확인
        if (isBigEndian) {
            // check word order for big-endian / 워드 순서 확인
            if (wordOrder == WordOrder.HighLow)
                // ABCD: high word first (most common) / ABCD: 상위 워드 먼저 (가장 일반적)
                return (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3];
            // CDAB: low word first / CDAB: 하위 워드 먼저
            return (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
        }

        // check word order for little-endian / 워드 순서 확인
        if (wordOrder == WordOrder.HighLow)
            // DCBA: standard little-endian / DCBA: 리틀엔디안
            return values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24);
        // BADC: little-endian with word swap / BADC: 리틀엔디안 + 워드 스왑
        return values[2] | (values[3] << 8) | (values[0] << 16) | (values[1] << 24);
    }

    #endregion

    #region Write (value → byte[])

    /// <summary>
    ///     int32 값을 스팬의 시작 위치에 기록
    ///     write int32 value to the start of the span
    /// </summary>
    /// <param name="span">대상 스팬 (최소 4바이트) / destination span (minimum 4 bytes)</param>
    /// <param name="value">기록할 int32 값 / int32 value to write</param>
    /// <param name="isBigEndian">
    ///     true: 빅엔디안 (MODBUS 기본), false: 리틀엔디안 / true: big-endian (MODBUS default), false:
    ///     little-endian
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32(Span<byte> span, int value, bool isBigEndian = true) {
        // check byte order endianness / 엔디안 확인
        if (isBigEndian) {
            // write byte 3 (most significant) / 빅엔디안으로 기록
            span[0] = (byte)(value >> 24);
            // write byte 2 / 두 번째 바이트 기록
            span[1] = (byte)(value >> 16);
            // write byte 1 / 세 번째 바이트 기록
            span[2] = (byte)(value >> 8);
            // write byte 0 (least significant) / 네 번째 바이트 기록
            span[3] = (byte)value;
        } else {
            // write byte 0 (least significant) / 리틀엔디안으로 기록
            span[0] = (byte)value;
            // write byte 1 / 두 번째 바이트 기록
            span[1] = (byte)(value >> 8);
            // write byte 2 / 세 번째 바이트 기록
            span[2] = (byte)(value >> 16);
            // write byte 3 (most significant) / 네 번째 바이트 기록
            span[3] = (byte)(value >> 24);
        }
    }

    /// <summary>
    ///     ushort 값을 스팬의 시작 위치에 기록
    ///     write ushort value to the start of the span
    /// </summary>
    /// <param name="span">대상 스팬 (최소 2바이트) / destination span (minimum 2 bytes)</param>
    /// <param name="value">기록할 ushort 값 / ushort value to write</param>
    /// <param name="isBigEndian">
    ///     true: 빅엔디안 (MODBUS 기본), false: 리틀엔디안 / true: big-endian (MODBUS default), false:
    ///     little-endian
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16(Span<byte> span, ushort value, bool isBigEndian = true) {
        // check byte order endianness / 엔디안 확인
        if (isBigEndian) {
            // write high byte first / 빅엔디안으로 기록
            span[0] = (byte)(value >> 8);
            // write low byte / 하위 바이트 기록
            span[1] = (byte)value;
        } else {
            // write low byte first / 리틀엔디안으로 기록
            span[0] = (byte)value;
            // write high byte / 상위 바이트 기록
            span[1] = (byte)(value >> 8);
        }
    }

    /// <summary>
    ///     float 값을 스팬의 시작 위치에 기록
    ///     write float value to the start of the span
    /// </summary>
    /// <param name="span">대상 스팬 (최소 4바이트) / destination span (minimum 4 bytes)</param>
    /// <param name="value">기록할 float 값 / float value to write</param>
    /// <param name="isBigEndian">
    ///     true: 빅엔디안 (MODBUS 기본), false: 리틀엔디안 / true: big-endian (MODBUS default), false:
    ///     little-endian
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteFloat(Span<byte> span, float value, bool isBigEndian = true) {
        // convert float to int bit representation / float를 int 비트로 변환
        var bits = BitConverter.SingleToInt32Bits(value);
        // write as int32 with specified endianness / int32로 기록
        WriteInt32(span, bits, isBigEndian);
    }

    #endregion

    #region Pack (value → ushort[])

    /// <summary>
    ///     int32 값을 ushort 배열로 변환 (MODBUS 레지스터 패킹)
    ///     convert int32 value to ushort array (MODBUS register packing)
    /// </summary>
    /// <param name="value">int 값 / int value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackInt32(int value, WordOrder wordOrder = WordOrder.HighLow) {
        // extract high and low words / 상위 및 하위 워드 가져오기
        var highWord = (ushort)(value >> 16);
        var lowWord  = (ushort)value;
        // return array based on word order / 워드 순서에 따라 반환
        return wordOrder == WordOrder.HighLow
            ? [highWord, lowWord]
            : [lowWord, highWord];
    }

    /// <summary>
    ///     float 값을 ushort 배열로 변환 (MODBUS 레지스터 패킹)
    ///     convert float value to ushort array (MODBUS register packing)
    /// </summary>
    /// <param name="value">float 값 / float value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackFloat(float value, WordOrder wordOrder = WordOrder.HighLow) {
        // convert float to int bit representation / float를 int 비트로 변환
        var bits = BitConverter.SingleToInt32Bits(value);
        // extract high and low words / 상위 및 하위 워드 가져오기
        var highWord = (ushort)(bits >> 16);
        var lowWord  = (ushort)bits;
        // return array based on word order / 워드 순서에 따라 반환
        return wordOrder == WordOrder.HighLow
            ? [highWord, lowWord]
            : [lowWord, highWord];
    }

    /// <summary>
    ///     텍스트를 워드 단위 ushort 배열로 변환 (문자 2개 = 1워드)
    ///     convert text to word-sized ushort array (2 chars = 1 word)
    /// </summary>
    /// <param name="text">텍스트 / text</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackString(string text) {
        // check for null or empty text / 텍스트 확인
        if (string.IsNullOrEmpty(text))
            // nothing to pack / 패킹할 텍스트 없음
            return [];

        // get text as span / 텍스트 스팬 가져오기
        var span = text.AsSpan();
        // calculate number of full word pairs / 길이 계산
        var length = span.Length / 2;
        // calculate remaining single character / 남은 길이 계산
        var remain = span.Length % 2;
        // create result array with room for remainder / 결과 배열 생성
        var result = new ushort[length + remain];
        // iterate through character pairs / 배열 길이만큼 반복
        for (var i = 0; i < length; i++)
            // pack two characters into one ushort / 값 설정
            result[i] = (ushort)((span[i * 2] << 8) | span[i * 2 + 1]);
        // check if there is a remaining character / 남은 오프셋 확인
        if (remain > 0)
            // pack last character into high byte / 값 설정
            result[length] = (ushort)(span[^1] << 8);
        // return packed word array / 결과 반환
        return result;
    }

    /// <summary>
    ///     숫자 문자열을 두 자리씩 묶어 ushort 배열로 변환
    ///     convert digit string to ushort array by pairing consecutive digits
    /// </summary>
    /// <param name="text">숫자 텍스트 / digit text</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackDigitPairs(string text) {
        // check for null or empty text / 텍스트 확인
        if (string.IsNullOrEmpty(text))
            // nothing to pack / 패킹할 텍스트 없음
            return [];
        // calculate number of digit pairs / 길이 계산
        var len = text.Length >> 1;
        // create result array / 결과 배열 생성
        var result = new ushort[len];
        // get text as span / 스팬 가져오기
        var span = text.AsSpan();
        // iterate through digit pairs / 길이만큼 반복
        for (int i = 0, j = 0; i < len; i++, j += 2) {
            // get first digit of pair / 첫째 자리 가져오기
            var d1 = span[j] - '0';
            // get second digit of pair / 둘째 자리 가져오기
            var d2 = span[j + 1] - '0';
            // combine digits into decimal value / 결과 설정
            result[i] = (ushort)(d1 * 10 + d2);
        }

        // return digit pair array / 값 배열 반환
        return result;
    }

    /// <summary>
    ///     네트워크 주소 문자열을 ushort 배열로 변환 (점 구분)
    ///     convert dot-delimited network address string to ushort array
    /// </summary>
    /// <param name="addr">주소 (예: "192.168.0.1") / address (e.g. "192.168.0.1")</param>
    /// <returns>값 배열 / value array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackAddress(string addr) {
        var count = 0;
        var acc   = 0;
        // get address string as span / 스팬 가져오기
        var span = addr.AsSpan();
        // allocate stack space for parsed octets / 공간 할당
        Span<ushort> values = stackalloc ushort[8];
        // iterate through each character / 길이만큼 반복
        for (var i = 0; i <= span.Length; i++)
            // check for end of string or dot separator / 길이 및 점 확인
            if (i == span.Length || span[i] == '.') {
                // store accumulated value / 값 설정
                values[count++] = (ushort)acc;
                // reset accumulator for next octet / 누적값 초기화
                acc = 0;
            } else {
                // accumulate digit into current octet / 누적값 설정
                acc = acc * 10 + (span[i] - '0');
            }

        // create result array with exact count / 결과 배열 생성
        var result = new ushort[count];
        // copy parsed values to result / 데이터 복사
        values[..count].CopyTo(result);
        // return parsed address array / 데이터 반환
        return result;
    }

    #endregion

    #region Format / Parse

    /// <summary>
    ///     바이트 스팬을 16진수 문자열로 변환
    ///     convert byte span to hex string
    /// </summary>
    /// <param name="values">바이트 스팬 / byte span</param>
    /// <param name="separator">구분자 (기본값: 공백) / separator (default: space)</param>
    /// <param name="lineBreakAt">줄바꿈 위치 (0 = 줄바꿈 없음) / line break position (0 = no break)</param>
    /// <returns>16진수 문자열 / hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatHex(ReadOnlySpan<byte> values, string separator = " ", int lineBreakAt = 16) {
        // check for empty input / 빈 값 확인
        if (values.IsEmpty)
            // no bytes to format / 포맷할 바이트 없음
            return string.Empty;

        // create StringBuilder with estimated capacity / StringBuilder 생성
        var sb = new StringBuilder(values.Length * 3);
        // iterate through each byte / 배열 순회
        for (var i = 0; i < values.Length; i++) {
            // append two-digit hex representation / 16진수 문자열 추가
            sb.Append(values[i].ToString("X2"));
            // check if separator should be appended / 인덱스 확인
            if (i < values.Length - 1)
                // append separator between bytes / 구분자 추가
                sb.Append(separator);
            // check if line break is needed / 줄바꿈 확인
            if (lineBreakAt > 0 && (i + 1) % lineBreakAt == 0)
                // append line break / 줄바꿈 추가
                sb.AppendLine();
        }

        // return formatted hex string / 16진수 문자열 반환
        return sb.ToString();
    }

    /// <summary>
    ///     정수형 단위 코드를 문자열로 변환
    ///     convert integer unit code to string representation
    /// </summary>
    /// <param name="unitCode">정수형 단위 코드 (0=kgf.cm, 1=kgf.m, ...) / unit code as integer (0=kgf.cm, 1=kgf.m, ...)</param>
    /// <returns>단위 문자열 / unit string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatUnit(int unitCode) {
        // map unit code to display string / 단위 코드를 표시 문자열로 변환
        return unitCode switch {
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
    ///     문자열에서 토크 단위로 변환
    ///     parse string to torque unit
    /// </summary>
    /// <param name="text">단위 문자열 / unit string</param>
    /// <returns>단위 / unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit ParseUnit(string text) {
        // check for null or empty string / null 또는 빈 문자열 확인
        if (string.IsNullOrEmpty(text))
            // default to kgf.cm when no unit specified / 단위 미지정 시 kgf.cm 기본값
            return Unit.KgfCm;
        // convert string to matching unit enum / 단위로 변환
        return text.ToLowerInvariant() switch {
            "kgf.cm" => Unit.KgfCm,
            "kgf.m"  => Unit.KgfM,
            "n.m"    => Unit.Nm,
            "n.cm"   => Unit.NCm,
            "lbf.in" => Unit.LbfIn,
            "ozf.in" => Unit.OzfIn,
            "lbf.ft" => Unit.LbfFt,
            _        => Unit.KgfCm
        };
    }

    /// <summary>
    ///     바이트 스팬을 ASCII 문자열로 변환 (후행 null 문자 제거)
    ///     decode byte span to ASCII string, trimming trailing null characters
    /// </summary>
    /// <param name="span">바이트 스팬 / byte span</param>
    /// <returns>문자열 / string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DecodeAscii(ReadOnlySpan<byte> span) {
        // get initial length / 길이 가져오기
        var end = span.Length;
        // trim trailing null bytes / null 문자 제거
        while (end > 0 && span[end - 1] is 0)
            // decrement end position / 끝 위치 감소
            end--;
        // return decoded string, or empty if all bytes were null / 문자열 반환, 전체 null이면 빈 문자열
        return end is not 0 ? Encoding.ASCII.GetString(span[..end]) : string.Empty;
    }

    #endregion
}