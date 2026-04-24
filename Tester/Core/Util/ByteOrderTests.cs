using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     ByteOrder 클래스의 바이너리 읽기/쓰기 및 엔디안·워드 순서 테스트.
///     tests for binary read/write and endian/word-order handling in ByteOrder class.
/// </summary>
public class ByteOrderTests {
    #region ReadFloat

    /// <summary>
    ///     빅엔디안 HighLow 워드 순서로 float 읽기가 올바른지 검증합니다.
    ///     verifies float read with big-endian HighLow word order is correct.
    /// </summary>
    [Fact]
    public void ReadFloat_BigEndianHighLow_ReturnsCorrectValue() {
        // 1.0f의 IEEE 754 빅엔디안 바이트 배열 준비
        // prepare IEEE 754 big-endian byte array for 1.0f
        byte[] bytes = [0x3F, 0x80, 0x00, 0x00];

        // 빅엔디안 HighLow 방식으로 float 읽기
        // read float in big-endian HighLow mode
        var result = ByteOrder.ReadFloat(bytes);

        // 결과가 1.0f인지 확인
        // verify result is 1.0f
        Assert.Equal(1.0f, result);
    }

    /// <summary>
    ///     빅엔디안 LowHigh 워드 순서로 float 읽기가 워드 스왑을 적용하는지 검증합니다.
    ///     verifies float read with big-endian LowHigh word order applies word swap.
    /// </summary>
    [Fact]
    public void ReadFloat_BigEndianLowHigh_ReturnsCorrectValue() {
        // 1.0f = 0x3F800000 → LowHigh(CDAB): [0x00, 0x00, 0x3F, 0x80] 준비
        // prepare 1.0f = 0x3F800000 as LowHigh(CDAB): [0x00, 0x00, 0x3F, 0x80]
        byte[] bytes = [0x00, 0x00, 0x3F, 0x80];

        // 빅엔디안 LowHigh 방식으로 float 읽기
        // read float in big-endian LowHigh mode
        var result = ByteOrder.ReadFloat(bytes, WordOrder.LowHigh);

        // 결과가 1.0f인지 확인
        // verify result is 1.0f
        Assert.Equal(1.0f, result);
    }

    /// <summary>
    ///     4바이트 미만의 입력에 대해 ReadFloat가 0을 반환하는지 검증합니다.
    ///     verifies ReadFloat returns 0 for input shorter than 4 bytes.
    /// </summary>
    [Fact]
    public void ReadFloat_InsufficientBytes_ReturnsZero() {
        // 3바이트 부족한 배열 준비
        // prepare insufficient 3-byte array
        byte[] bytes = [0x3F, 0x80, 0x00];

        // 부족한 바이트로 float 읽기 실행
        // execute float read with insufficient bytes
        var result = ByteOrder.ReadFloat(bytes);

        // 0이 반환되는지 확인
        // verify zero is returned
        Assert.Equal(0f, result);
    }

    /// <summary>
    ///     리틀엔디안 HighLow 워드 순서로 float 읽기가 올바른지 검증합니다.
    ///     verifies float read with little-endian HighLow word order is correct.
    /// </summary>
    [Fact]
    public void ReadFloat_LittleEndianHighLow_ReturnsCorrectValue() {
        // 1.0f = 0x3F800000 → 리틀엔디안(DCBA): [0x00, 0x00, 0x80, 0x3F] 준비
        // prepare 1.0f = 0x3F800000 as little-endian(DCBA): [0x00, 0x00, 0x80, 0x3F]
        byte[] bytes = [0x00, 0x00, 0x80, 0x3F];

        // 리틀엔디안 HighLow 방식으로 float 읽기
        // read float in little-endian HighLow mode
        var result = ByteOrder.ReadFloat(bytes, WordOrder.HighLow, false);

        // 결과가 1.0f인지 확인
        // verify result is 1.0f
        Assert.Equal(1.0f, result);
    }

    #endregion

    #region ReadUInt16

    /// <summary>
    ///     빅엔디안 방식으로 ushort 읽기가 올바른지 검증합니다.
    ///     verifies ushort read with big-endian byte order is correct.
    /// </summary>
    [Fact]
    public void ReadUInt16_BigEndian_ReturnsCorrectValue() {
        // 0x0102 빅엔디안 바이트 배열 준비
        // prepare big-endian byte array for 0x0102
        byte[] bytes = [0x01, 0x02];

        // 빅엔디안 방식으로 ushort 읽기
        // read ushort in big-endian mode
        var result = ByteOrder.ReadUInt16(bytes);

        // 결과가 0x0102 (258)인지 확인
        // verify result is 0x0102 (258)
        Assert.Equal((ushort)0x0102, result);
    }

    /// <summary>
    ///     리틀엔디안 방식으로 ushort 읽기가 올바른지 검증합니다.
    ///     verifies ushort read with little-endian byte order is correct.
    /// </summary>
    [Fact]
    public void ReadUInt16_LittleEndian_ReturnsCorrectValue() {
        // 0x0201의 리틀엔디안 바이트 배열 준비 (low=0x01, high=0x02)
        // prepare little-endian byte array for 0x0201 (low=0x01, high=0x02)
        byte[] bytes = [0x01, 0x02];

        // 리틀엔디안 방식으로 ushort 읽기
        // read ushort in little-endian mode
        var result = ByteOrder.ReadUInt16(bytes, false);

        // 결과가 0x0201 (513)인지 확인
        // verify result is 0x0201 (513)
        Assert.Equal((ushort)0x0201, result);
    }

    /// <summary>
    ///     2바이트 미만의 입력에 대해 ReadUInt16이 0을 반환하는지 검증합니다.
    ///     verifies ReadUInt16 returns 0 for input shorter than 2 bytes.
    /// </summary>
    [Fact]
    public void ReadUInt16_InsufficientBytes_ReturnsZero() {
        // 1바이트 부족한 배열 준비
        // prepare insufficient 1-byte array
        byte[] bytes = [0x01];

        // 부족한 바이트로 ushort 읽기 실행
        // execute ushort read with insufficient bytes
        var result = ByteOrder.ReadUInt16(bytes);

        // 0이 반환되는지 확인
        // verify zero is returned
        Assert.Equal((ushort)0, result);
    }

    #endregion

    #region ReadInt32

    /// <summary>
    ///     빅엔디안 HighLow 워드 순서로 int32 읽기가 올바른지 검증합니다.
    ///     verifies int32 read with big-endian HighLow word order is correct.
    /// </summary>
    [Fact]
    public void ReadInt32_BigEndianHighLow_ReturnsCorrectValue() {
        // 0x12345678 빅엔디안 바이트 배열 준비
        // prepare big-endian byte array for 0x12345678
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];

        // 빅엔디안 HighLow 방식으로 int32 읽기
        // read int32 in big-endian HighLow mode
        var result = ByteOrder.ReadInt32(bytes);

        // 결과가 0x12345678인지 확인
        // verify result is 0x12345678
        Assert.Equal(0x12345678, result);
    }

    /// <summary>
    ///     정확히 4바이트가 아닌 입력에 대해 ReadInt32가 0을 반환하는지 검증합니다.
    ///     verifies ReadInt32 returns 0 for input that is not exactly 4 bytes.
    /// </summary>
    [Fact]
    public void ReadInt32_NotExactly4Bytes_ReturnsZero() {
        // 3바이트 부족한 배열 준비
        // prepare insufficient 3-byte array
        byte[] bytes = [0x12, 0x34, 0x56];

        // 부족한 바이트로 int32 읽기 실행
        // execute int32 read with insufficient bytes
        var result = ByteOrder.ReadInt32(bytes);

        // 0이 반환되는지 확인
        // verify zero is returned
        Assert.Equal(0, result);
    }

    #endregion

    #region WriteInt32 / WriteUInt16 / WriteFloat

    /// <summary>
    ///     빅엔디안 WriteInt32 후 ReadInt32로 왕복 검증합니다.
    ///     verifies round-trip via big-endian WriteInt32 followed by ReadInt32.
    /// </summary>
    [Fact]
    public void WriteInt32_BigEndian_RoundTripsCorrectly() {
        // 테스트 int32 값 설정
        // set test int32 value
        const int value = 0x12345678;
        // 4바이트 쓰기 버퍼 할당
        // allocate 4-byte write buffer
        var buffer = new byte[4];

        // 빅엔디안으로 int32 쓰기
        // write int32 in big-endian
        ByteOrder.WriteInt32(buffer, value);
        // 빅엔디안으로 int32 읽기
        // read int32 in big-endian
        var result = ByteOrder.ReadInt32(buffer);

        // 왕복 결과가 원본과 일치하는지 확인
        // verify round-trip result matches original
        Assert.Equal(value, result);
    }

    /// <summary>
    ///     빅엔디안 WriteUInt16 후 ReadUInt16으로 왕복 검증합니다.
    ///     verifies round-trip via big-endian WriteUInt16 followed by ReadUInt16.
    /// </summary>
    [Fact]
    public void WriteUInt16_BigEndian_RoundTripsCorrectly() {
        // 테스트 ushort 값 설정
        // set test ushort value
        const ushort value = 0xABCD;
        // 2바이트 쓰기 버퍼 할당
        // allocate 2-byte write buffer
        var buffer = new byte[2];

        // 빅엔디안으로 ushort 쓰기
        // write ushort in big-endian
        ByteOrder.WriteUInt16(buffer, value);
        // 빅엔디안으로 ushort 읽기
        // read ushort in big-endian
        var result = ByteOrder.ReadUInt16(buffer);

        // 왕복 결과가 원본과 일치하는지 확인
        // verify round-trip result matches original
        Assert.Equal(value, result);
    }

    /// <summary>
    ///     빅엔디안 WriteFloat 후 ReadFloat로 왕복 검증합니다.
    ///     verifies round-trip via big-endian WriteFloat followed by ReadFloat.
    /// </summary>
    [Fact]
    public void WriteFloat_BigEndian_RoundTripsCorrectly() {
        // 테스트 float 값 설정
        // set test float value
        const float value = 3.14f;
        // 4바이트 쓰기 버퍼 할당
        // allocate 4-byte write buffer
        var buffer = new byte[4];

        // 빅엔디안으로 float 쓰기
        // write float in big-endian
        ByteOrder.WriteFloat(buffer, value);
        // 빅엔디안으로 float 읽기
        // read float in big-endian
        var result = ByteOrder.ReadFloat(buffer);

        // 왕복 결과가 원본과 일치하는지 확인 (float 정밀도 허용)
        // verify round-trip result matches original (float precision tolerance)
        Assert.Equal(value, result, 5);
    }

    #endregion
}
