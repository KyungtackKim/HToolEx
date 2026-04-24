using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     Checksum 클래스의 CRC-16/MODBUS 계산 및 검증 테스트.
///     tests for CRC-16/MODBUS calculation and validation in Checksum class.
/// </summary>
public class ChecksumTests {
    /// <summary>
    ///     알려진 MODBUS CRC 벡터로 CalculateCrc의 정확성을 검증합니다.
    ///     verifies CalculateCrc correctness with a known MODBUS CRC vector.
    /// </summary>
    [Fact]
    public void CalculateCrc_KnownModbusVector_ReturnsExpectedCrcBytes() {
        // MODBUS 요청 패킷 준비 (slave=1, FC=3, addr=0, count=1)
        // prepare MODBUS request packet (slave=1, FC=3, addr=0, count=1)
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01];

        // CRC 계산 실행
        // execute CRC calculation
        var (low, high) = Checksum.Calculate(packet);

        // 하위 바이트가 0x84인지 확인
        // verify low byte is 0x84
        Assert.Equal(0x84, low);
        // 상위 바이트가 0x0A인지 확인
        // verify high byte is 0x0A
        Assert.Equal(0x0A, high);
    }

    /// <summary>
    ///     빈 패킷에 대한 CRC 계산이 초기값 0xFFFF의 하위/상위 바이트를 반환하는지 검증합니다.
    ///     verifies CRC calculation on empty packet returns low/high bytes of initial value 0xFFFF.
    /// </summary>
    [Fact]
    public void CalculateCrc_EmptyPacket_ReturnsInitialCrcValue() {
        // 빈 바이트 배열 준비
        // prepare empty byte array
        byte[] packet = [];

        // 빈 패킷의 CRC 계산
        // calculate CRC on empty packet
        var (low, high) = Checksum.Calculate(packet);

        // 초기값 0xFFFF의 하위 바이트 (0xFF) 확인
        // verify low byte of initial value 0xFFFF (0xFF)
        Assert.Equal(0xFF, low);
        // 초기값 0xFFFF의 상위 바이트 (0xFF) 확인
        // verify high byte of initial value 0xFFFF (0xFF)
        Assert.Equal(0xFF, high);
    }

    /// <summary>
    ///     단일 바이트 패킷에 대한 CRC 계산이 올바르게 동작하는지 검증합니다.
    ///     verifies CRC calculation works correctly for a single byte packet.
    /// </summary>
    [Fact]
    public void CalculateCrc_SingleByte_ReturnsDeterministicResult() {
        // 단일 바이트 패킷 준비
        // prepare single byte packet
        byte[] packet = [0x01];

        // CRC 계산 실행
        // execute CRC calculation
        var (low, high) = Checksum.Calculate(packet);

        // CRC 결과가 결정적임을 확인 (재계산 시 동일)
        // verify CRC result is deterministic (same on recalculation)
        var (low2, high2) = Checksum.Calculate(packet);
        // 하위 바이트 일관성 확인
        // verify low byte consistency
        Assert.Equal(low, low2);
        // 상위 바이트 일관성 확인
        // verify high byte consistency
        Assert.Equal(high, high2);
    }

    /// <summary>
    ///     CalculateCrcTo가 버퍼에 올바른 CRC 바이트를 기록하는지 검증합니다.
    ///     verifies CalculateCrcTo writes correct CRC bytes to buffer.
    /// </summary>
    [Fact]
    public void CalculateCrcTo_KnownVector_WritesCrcToBuffer() {
        // MODBUS 요청 패킷 준비
        // prepare MODBUS request packet
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01];
        // CRC 결과를 저장할 2바이트 버퍼 할당
        // allocate 2-byte buffer for CRC result
        var buffer = new byte[2];

        // 버퍼에 CRC 기록
        // write CRC to buffer
        Checksum.CalculateTo(packet, buffer);

        // 버퍼의 하위 CRC 바이트 확인
        // verify low CRC byte in buffer
        Assert.Equal(0x84, buffer[0]);
        // 버퍼의 상위 CRC 바이트 확인
        // verify high CRC byte in buffer
        Assert.Equal(0x0A, buffer[1]);
    }

    /// <summary>
    ///     CalculateCrcTo에 1바이트 버퍼를 전달하면 ArgumentException이 발생하는지 검증합니다.
    ///     verifies CalculateCrcTo throws ArgumentException when buffer is less than 2 bytes.
    /// </summary>
    [Fact]
    public void CalculateCrcTo_BufferTooSmall_ThrowsArgumentException() {
        // MODBUS 패킷 준비
        // prepare MODBUS packet
        byte[] packet = [0x01, 0x03];
        // 부족한 크기의 1바이트 버퍼 할당
        // allocate insufficient 1-byte buffer
        var buffer = new byte[1];

        // 크기 부족 시 ArgumentException 발생 확인
        // verify ArgumentException is thrown for insufficient size
        Assert.Throws<ArgumentException>(() => Checksum.CalculateTo(packet, buffer));
    }

    /// <summary>
    ///     CalculateCrc와 CalculateCrcTo가 동일한 결과를 반환하는지 검증합니다.
    ///     verifies CalculateCrc and CalculateCrcTo produce identical results.
    /// </summary>
    [Fact]
    public void CalculateCrcTo_SamePacket_MatchesCalculateCrc() {
        // 테스트 패킷 준비
        // prepare test packet
        byte[] packet = [0x01, 0x04, 0x00, 0x10, 0x00, 0x02];
        // CRC 결과를 저장할 버퍼 할당
        // allocate buffer for CRC result
        var buffer = new byte[2];

        // 튜플 반환 방식으로 CRC 계산
        // calculate CRC via tuple return
        var (low, high) = Checksum.Calculate(packet);
        // 버퍼 기록 방식으로 CRC 계산
        // calculate CRC via buffer write
        Checksum.CalculateTo(packet, buffer);

        // 하위 바이트 일치 확인
        // verify low byte matches
        Assert.Equal(low, buffer[0]);
        // 상위 바이트 일치 확인
        // verify high byte matches
        Assert.Equal(high, buffer[1]);
    }

    /// <summary>
    ///     올바른 CRC가 포함된 패킷에 대해 ValidateCrc가 true를 반환하는지 검증합니다.
    ///     verifies ValidateCrc returns true for a packet with correct CRC appended.
    /// </summary>
    [Fact]
    public void ValidateCrc_ValidPacket_ReturnsTrue() {
        // CRC 포함 MODBUS 패킷 준비 (data + CRC low + CRC high)
        // prepare MODBUS packet with CRC (data + CRC low + CRC high)
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x84, 0x0A];

        // CRC 검증 실행
        // execute CRC validation
        var result = Checksum.Validate(packet);

        // 유효한 CRC에 대해 true 반환 확인
        // verify true is returned for valid CRC
        Assert.True(result);
    }

    /// <summary>
    ///     잘못된 CRC가 포함된 패킷에 대해 ValidateCrc가 false를 반환하는지 검증합니다.
    ///     verifies ValidateCrc returns false for a packet with incorrect CRC.
    /// </summary>
    [Fact]
    public void ValidateCrc_InvalidCrc_ReturnsFalse() {
        // 잘못된 CRC 바이트가 포함된 패킷 준비
        // prepare packet with incorrect CRC bytes
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF];

        // CRC 검증 실행
        // execute CRC validation
        var result = Checksum.Validate(packet);

        // 잘못된 CRC에 대해 false 반환 확인
        // verify false is returned for invalid CRC
        Assert.False(result);
    }

    /// <summary>
    ///     2바이트 이하의 패킷에 대해 ValidateCrc가 false를 반환하는지 검증합니다.
    ///     verifies ValidateCrc returns false for packets shorter than 3 bytes.
    /// </summary>
    [Fact]
    public void ValidateCrc_PacketTooShort_ReturnsFalse() {
        // 2바이트 패킷 준비 (CRC 검증에 최소 3바이트 필요)
        // prepare 2-byte packet (minimum 3 bytes required for CRC validation)
        byte[] packet = [0x01, 0x03];

        // CRC 검증 실행
        // execute CRC validation
        var result = Checksum.Validate(packet);

        // 너무 짧은 패킷에 대해 false 반환 확인
        // verify false is returned for too-short packet
        Assert.False(result);
    }

    /// <summary>
    ///     데이터가 변경된 패킷에 대해 ValidateCrc가 false를 반환하는지 검증합니다.
    ///     verifies ValidateCrc returns false when data bytes are tampered with.
    /// </summary>
    [Fact]
    public void ValidateCrc_TamperedData_ReturnsFalse() {
        // 원본 유효 패킷 복사
        // copy original valid packet
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x84, 0x0A];
        // 데이터 바이트를 변조
        // tamper with data byte
        packet[4] = 0x02;

        // CRC 검증 실행
        // execute CRC validation
        var result = Checksum.Validate(packet);

        // 변조된 데이터에 대해 false 반환 확인
        // verify false is returned for tampered data
        Assert.False(result);
    }

    /// <summary>
    ///     정확히 3바이트 최소 크기 패킷에 대해 ValidateCrc가 올바르게 동작하는지 검증합니다.
    ///     verifies ValidateCrc works correctly for minimum 3-byte packet.
    /// </summary>
    [Fact]
    public void ValidateCrc_MinimumValidLength_WorksCorrectly() {
        // 1바이트 데이터로 CRC 계산
        // calculate CRC for 1 byte of data
        byte[] data = [0x01];
        // CRC 값 계산
        // compute CRC value
        var (low, high) = Checksum.Calculate(data);
        // CRC 포함 3바이트 패킷 구성
        // construct 3-byte packet with CRC
        byte[] packet = [0x01, low, high];

        // CRC 검증 실행
        // execute CRC validation
        var result = Checksum.Validate(packet);

        // 최소 유효 길이 패킷에 대해 true 반환 확인
        // verify true is returned for minimum valid length packet
        Assert.True(result);
    }
}