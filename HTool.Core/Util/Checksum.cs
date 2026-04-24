using System.Runtime.CompilerServices;

namespace HTool.Core.Util;

/// <summary>
///     프레임 무결성 검사용 체크섬 계산 유틸리티. 현재 MODBUS-RTU CRC-16을 구현합니다.
///     checksum utility for frame integrity validation. currently implements MODBUS-RTU CRC-16.
/// </summary>
/// <remarks>
///     CRC-16/MODBUS 알고리즘 (Polynomial: 0xA001)을 룩업 테이블 기반으로 고속 계산합니다. RTU 프로토콜의 프레임 검증에 필수적입니다.
///     다른 체크섬(CRC32, Fletcher 등)이 필요할 때 이 클래스에 메서드를 추가하십시오.
///     uses CRC-16/MODBUS algorithm (Polynomial: 0xA001) with lookup table for high-speed calculation. Essential for RTU
///     protocol frame validation. Add methods here when additional checksums (CRC32, Fletcher, etc.) are needed.
/// </remarks>
public static class Checksum {
    /// <summary>
    ///     MODBUS-RTU CRC-16 룩업 테이블 (256개 항목).
    ///     MODBUS-RTU CRC-16 lookup table (256 entries).
    /// </summary>
    private static readonly ushort[] Table = [
        0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
        0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
        0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
        0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
        0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
        0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
        0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
        0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
        0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
        0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
        0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
        0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
        0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
        0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
        0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
        0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
        0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
        0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
        0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
        0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
        0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
        0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
        0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
        0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
        0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
        0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
        0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
        0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
        0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
        0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
        0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
        0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
    ];

    /// <summary>
    ///     패킷의 CRC 값을 계산합니다.
    ///     calculate CRC value for the packet.
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <returns>결과 (low, high) / result (low, high)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte low, byte high) Calculate(ReadOnlySpan<byte> packet) {
        // CRC 초기값 설정
        // initialize CRC to default value
        ushort crc = 0xFFFF;
        // 패킷의 각 바이트를 순회
        // iterate through each byte in packet
        foreach (var b in packet) {
            // 룩업 테이블 인덱스 계산
            // calculate lookup table index
            var index = (byte)(crc ^ b);
            // 룩업 테이블을 사용해 CRC 업데이트
            // update CRC using lookup table
            crc = (ushort)((crc >> 8) ^ Table[index]);
        }

        // CRC 하위/상위 바이트를 튜플로 반환
        // return low and high CRC bytes
        return ((byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF));
    }

    /// <summary>
    ///     패킷의 CRC 값을 계산하여 버퍼에 저장합니다.
    ///     calculate CRC value for the packet and store in buffer.
    /// </summary>
    /// <param name="packet">패킷 / packet</param>
    /// <param name="buffer">CRC 버퍼 (최소 2바이트) / CRC buffer (minimum 2 bytes)</param>
    /// <exception cref="ArgumentException">버퍼 크기가 2바이트 미만일 때 / when buffer is smaller than 2 bytes</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CalculateTo(ReadOnlySpan<byte> packet, Span<byte> buffer) {
        // 버퍼가 CRC 2바이트를 담을 수 있는지 확인
        // ensure buffer can hold the 2 CRC bytes
        if (buffer.Length < 2)
            // 버퍼 크기 부족 예외 발생
            // throw exception for insufficient buffer size
            throw new ArgumentException("Buffer must be at least 2 bytes", nameof(buffer));

        // CRC 초기값 설정
        // initialize CRC to default value
        ushort crc = 0xFFFF;
        // 패킷의 각 바이트를 순회
        // iterate through each byte in packet
        foreach (var b in packet) {
            // 룩업 테이블 인덱스 계산
            // calculate lookup table index
            var index = (byte)(crc ^ b);
            // 룩업 테이블을 사용해 CRC 업데이트
            // update CRC using lookup table
            crc = (ushort)((crc >> 8) ^ Table[index]);
        }

        // CRC 하위 바이트 저장
        // store low CRC byte
        buffer[0] = (byte)(crc & 0xFF);
        // CRC 상위 바이트 저장
        // store high CRC byte
        buffer[1] = (byte)((crc >> 8) & 0xFF);
    }

    /// <summary>
    ///     패킷의 CRC 값을 검증합니다.
    ///     validate the CRC value of the packet.
    /// </summary>
    /// <param name="packet">CRC 포함 패킷 (최소 3바이트) / packet including CRC (minimum 3 bytes)</param>
    /// <returns>CRC 일치 여부 / true if CRC matches</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Validate(ReadOnlySpan<byte> packet) {
        // 최소 패킷 길이 확인 (데이터 1바이트 + CRC 2바이트)
        // check minimum packet length (1 data byte + 2 CRC bytes)
        if (packet.Length < 3)
            // 검증 불가 상태로 실패 반환
            // return failure since validation is impossible
            return false;

        // CRC 제외한 데이터 영역
        // data portion excluding CRC bytes
        var data = packet[..^2];
        // 수신된 CRC 하위 바이트
        // received low CRC byte
        var receivedLow = packet[^2];
        // 수신된 CRC 상위 바이트
        // received high CRC byte
        var receivedHigh = packet[^1];
        // 기대되는 CRC 값 계산
        // calculate expected CRC value
        var (low, high) = Calculate(data);
        // 수신된 CRC와 계산된 CRC 비교 결과 반환
        // return whether received CRC matches calculated CRC
        return receivedLow == low && receivedHigh == high;
    }
}
