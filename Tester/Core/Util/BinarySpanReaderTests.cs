using HTool.Core.Util;
using Tester.TestHelpers;

namespace Tester.Core.Util;

/// <summary>
///     BinarySpanReader의 Big-Endian 바이너리 읽기 기능을 검증하는 테스트 클래스.
///     test class verifying Big-Endian binary reading functionality of BinarySpanReader.
/// </summary>
public sealed class BinarySpanReaderTests {
    // ──────────────────────────────────────────────
    // ReadByte / 바이트 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadByte가 올바른 값을 읽고 위치를 전진하는지 검증한다.
    ///     verifies that ReadByte reads correct value and advances position.
    /// </summary>
    [Fact]
    public void ReadByte_ValidSpan_ReadsValueAndAdvancesPos() {
        // 테스트 데이터 생성
        // create test data
        ReadOnlySpan<byte> span = [0xAB, 0xCD];
        // 초기 위치
        // initial position
        var pos = 0;

        // 첫 번째 바이트 읽기
        // read first byte
        var first = BinarySpanReader.ReadByte(span, ref pos);
        // 두 번째 바이트 읽기
        // read second byte
        var second = BinarySpanReader.ReadByte(span, ref pos);

        // 첫 번째 값 확인
        // verify first value
        Assert.Equal(0xAB, first);
        // 두 번째 값 확인
        // verify second value
        Assert.Equal(0xCD, second);
        // 위치가 2인지 확인
        // verify position is 2
        Assert.Equal(2, pos);
    }

    // ──────────────────────────────────────────────
    // ReadInt16 / Int16 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadInt16(span)이 위치 전진 없이 Big-Endian Int16을 읽는지 검증한다.
    ///     verifies that ReadInt16(span) reads Big-Endian Int16 without advancing position.
    /// </summary>
    [Fact]
    public void ReadInt16_NoPos_ReadsBigEndianValue() {
        // Big-Endian 0x0100 = 256 데이터 생성
        // create Big-Endian 0x0100 = 256 data
        ReadOnlySpan<byte> span = [0x01, 0x00];

        // Int16 읽기
        // read Int16
        var value = BinarySpanReader.ReadInt16(span);

        // 값이 256인지 확인
        // verify value is 256
        Assert.Equal(256, value);
    }

    /// <summary>
    ///     ReadInt16(span, ref pos)이 위치를 전진하며 Big-Endian Int16을 읽는지 검증한다.
    ///     verifies that ReadInt16(span, ref pos) reads Big-Endian Int16 and advances position.
    /// </summary>
    [Fact]
    public void ReadInt16_WithPos_ReadsBigEndianAndAdvances() {
        // 4바이트 데이터 (첫 2바이트 = -1, 다음 2바이트 = 0x0201)
        // 4-byte data (first 2 = -1, next 2 = 0x0201)
        ReadOnlySpan<byte> span = [0xFF, 0xFF, 0x02, 0x01];
        // 초기 위치
        // initial position
        var pos = 0;

        // 첫 번째 Int16 읽기
        // read first Int16
        var first = BinarySpanReader.ReadInt16(span, ref pos);
        // 두 번째 Int16 읽기
        // read second Int16
        var second = BinarySpanReader.ReadInt16(span, ref pos);

        // 첫 번째 값이 -1인지 확인
        // verify first value is -1
        Assert.Equal(-1, first);
        // 두 번째 값이 513인지 확인
        // verify second value is 513
        Assert.Equal(513, second);
        // 위치가 4인지 확인
        // verify position is 4
        Assert.Equal(4, pos);
    }

    // ──────────────────────────────────────────────
    // ReadUInt16 / UInt16 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadUInt16(span)이 위치 전진 없이 Big-Endian UInt16을 읽는지 검증한다.
    ///     verifies that ReadUInt16(span) reads Big-Endian UInt16 without advancing position.
    /// </summary>
    [Fact]
    public void ReadUInt16_NoPos_ReadsBigEndianValue() {
        // Big-Endian 0xFF00 = 65280 데이터 생성
        // create Big-Endian 0xFF00 = 65280 data
        ReadOnlySpan<byte> span = [0xFF, 0x00];

        // UInt16 읽기
        // read UInt16
        var value = BinarySpanReader.ReadUInt16(span);

        // 값이 65280인지 확인
        // verify value is 65280
        Assert.Equal((ushort)65280, value);
    }

    /// <summary>
    ///     ReadUInt16(span, ref pos)이 위치를 전진하며 Big-Endian UInt16을 읽는지 검증한다.
    ///     verifies that ReadUInt16(span, ref pos) reads Big-Endian UInt16 and advances position.
    /// </summary>
    [Fact]
    public void ReadUInt16_WithPos_ReadsBigEndianAndAdvances() {
        // Big-Endian 0x1234 = 4660
        // Big-Endian 0x1234 = 4660
        ReadOnlySpan<byte> span = [0x12, 0x34];
        // 초기 위치
        // initial position
        var pos = 0;

        // UInt16 읽기
        // read UInt16
        var value = BinarySpanReader.ReadUInt16(span, ref pos);

        // 값이 0x1234인지 확인
        // verify value is 0x1234
        Assert.Equal((ushort)0x1234, value);
        // 위치가 2인지 확인
        // verify position is 2
        Assert.Equal(2, pos);
    }

    // ──────────────────────────────────────────────
    // ReadInt32 / Int32 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadInt32(span)이 위치 전진 없이 Big-Endian Int32를 읽는지 검증한다.
    ///     verifies that ReadInt32(span) reads Big-Endian Int32 without advancing position.
    /// </summary>
    [Fact]
    public void ReadInt32_NoPos_ReadsBigEndianValue() {
        // ByteBuilder로 Big-Endian 305419896 (0x12345678) 생성
        // build Big-Endian 305419896 (0x12345678) with ByteBuilder
        var data = new ByteBuilder().Int32BigEndian(305419896).Build();

        // Int32 읽기
        // read Int32
        var value = BinarySpanReader.ReadInt32(data);

        // 값 확인
        // verify value
        Assert.Equal(305419896, value);
    }

    /// <summary>
    ///     ReadInt32(span, ref pos)이 위치를 전진하며 Big-Endian Int32를 읽는지 검증한다.
    ///     verifies that ReadInt32(span, ref pos) reads Big-Endian Int32 and advances position.
    /// </summary>
    [Fact]
    public void ReadInt32_WithPos_ReadsBigEndianAndAdvances() {
        // ByteBuilder로 음수 값 생성
        // build negative value with ByteBuilder
        var data = new ByteBuilder().Int32BigEndian(-1).Build();
        // 초기 위치
        // initial position
        var pos = 0;

        // Int32 읽기
        // read Int32
        var value = BinarySpanReader.ReadInt32(data, ref pos);

        // 값이 -1인지 확인
        // verify value is -1
        Assert.Equal(-1, value);
        // 위치가 4인지 확인
        // verify position is 4
        Assert.Equal(4, pos);
    }

    // ──────────────────────────────────────────────
    // ReadUInt32 / UInt32 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadUInt32(span, ref pos)이 Big-Endian UInt32를 읽고 위치를 전진하는지 검증한다.
    ///     verifies that ReadUInt32(span, ref pos) reads Big-Endian UInt32 and advances position.
    /// </summary>
    [Fact]
    public void ReadUInt32_WithPos_ReadsBigEndianAndAdvances() {
        // ByteBuilder로 0xDEADBEEF 생성
        // build 0xDEADBEEF with ByteBuilder
        var data = new ByteBuilder().UInt32BigEndian(0xDEADBEEF).Build();
        // 초기 위치
        // initial position
        var pos = 0;

        // UInt32 읽기
        // read UInt32
        var value = BinarySpanReader.ReadUInt32(data, ref pos);

        // 값이 0xDEADBEEF인지 확인
        // verify value is 0xDEADBEEF
        Assert.Equal(0xDEADBEEFu, value);
        // 위치가 4인지 확인
        // verify position is 4
        Assert.Equal(4, pos);
    }

    // ──────────────────────────────────────────────
    // ReadInt64 / Int64 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadInt64(span)이 Big-Endian Int64를 읽는지 검증한다.
    ///     verifies that ReadInt64(span) reads Big-Endian Int64.
    /// </summary>
    [Fact]
    public void ReadInt64_NoPos_ReadsBigEndianValue() {
        // Big-Endian 1 (Int64) 수동 생성
        // manually create Big-Endian 1 (Int64)
        ReadOnlySpan<byte> span = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01];

        // Int64 읽기
        // read Int64
        var value = BinarySpanReader.ReadInt64(span);

        // 값이 1인지 확인
        // verify value is 1
        Assert.Equal(1L, value);
    }

    /// <summary>
    ///     ReadInt64(span, ref pos)이 위치를 전진하며 Big-Endian Int64를 읽는지 검증한다.
    ///     verifies that ReadInt64(span, ref pos) reads Big-Endian Int64 and advances position.
    /// </summary>
    [Fact]
    public void ReadInt64_WithPos_ReadsBigEndianAndAdvances() {
        // Big-Endian -1 (Int64) = 모든 비트 1
        // Big-Endian -1 (Int64) = all bits set
        ReadOnlySpan<byte> span = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
        // 초기 위치
        // initial position
        var pos = 0;

        // Int64 읽기
        // read Int64
        var value = BinarySpanReader.ReadInt64(span, ref pos);

        // 값이 -1인지 확인
        // verify value is -1
        Assert.Equal(-1L, value);
        // 위치가 8인지 확인
        // verify position is 8
        Assert.Equal(8, pos);
    }

    // ──────────────────────────────────────────────
    // ReadUInt64 / UInt64 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadUInt64(span, ref pos)이 Big-Endian UInt64를 읽고 위치를 전진하는지 검증한다.
    ///     verifies that ReadUInt64(span, ref pos) reads Big-Endian UInt64 and advances position.
    /// </summary>
    [Fact]
    public void ReadUInt64_WithPos_ReadsBigEndianAndAdvances() {
        // Big-Endian 0x0102030405060708
        // Big-Endian 0x0102030405060708
        ReadOnlySpan<byte> span = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        // 초기 위치
        // initial position
        var pos = 0;

        // UInt64 읽기
        // read UInt64
        var value = BinarySpanReader.ReadUInt64(span, ref pos);

        // 값 확인
        // verify value
        Assert.Equal(0x0102030405060708UL, value);
        // 위치가 8인지 확인
        // verify position is 8
        Assert.Equal(8, pos);
    }

    // ──────────────────────────────────────────────
    // ReadSingle / float 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadSingle(span)이 Big-Endian float를 읽는지 검증한다.
    ///     verifies that ReadSingle(span) reads Big-Endian float.
    /// </summary>
    [Fact]
    public void ReadSingle_NoPos_ReadsBigEndianFloat() {
        // ByteBuilder로 Big-Endian 3.14f 생성
        // build Big-Endian 3.14f with ByteBuilder
        var data = new ByteBuilder().SingleBigEndian(3.14f).Build();

        // float 읽기
        // read float
        var value = BinarySpanReader.ReadSingle(data);

        // 값이 3.14f와 근사한지 확인 (정밀도 5자리)
        // verify value approximates 3.14f (5-digit precision)
        Assert.Equal(3.14f, value, 5);
    }

    /// <summary>
    ///     ReadSingle(span, ref pos)이 위치를 전진하며 Big-Endian float를 읽는지 검증한다.
    ///     verifies that ReadSingle(span, ref pos) reads Big-Endian float and advances position.
    /// </summary>
    [Fact]
    public void ReadSingle_WithPos_ReadsBigEndianAndAdvances() {
        // ByteBuilder로 Big-Endian -100.5f 생성
        // build Big-Endian -100.5f with ByteBuilder
        var data = new ByteBuilder().SingleBigEndian(-100.5f).Build();
        // 초기 위치
        // initial position
        var pos = 0;

        // float 읽기
        // read float
        var value = BinarySpanReader.ReadSingle(data, ref pos);

        // 값 확인
        // verify value
        Assert.Equal(-100.5f, value, 5);
        // 위치가 4인지 확인
        // verify position is 4
        Assert.Equal(4, pos);
    }

    // ──────────────────────────────────────────────
    // ReadDouble / double 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadDouble(span, ref pos)이 위치를 전진하며 Big-Endian double을 읽는지 검증한다.
    ///     verifies that ReadDouble(span, ref pos) reads Big-Endian double and advances position.
    /// </summary>
    [Fact]
    public void ReadDouble_WithPos_ReadsBigEndianAndAdvances() {
        // ByteBuilder로 Big-Endian 123456.789 생성
        // build Big-Endian 123456.789 with ByteBuilder
        var data = new ByteBuilder().DoubleBigEndian(123456.789).Build();
        // 초기 위치
        // initial position
        var pos = 0;

        // double 읽기
        // read double
        var value = BinarySpanReader.ReadDouble(data, ref pos);

        // 값 확인 (정밀도 10자리)
        // verify value (10-digit precision)
        Assert.Equal(123456.789, value, 10);
        // 위치가 8인지 확인
        // verify position is 8
        Assert.Equal(8, pos);
    }

    // ──────────────────────────────────────────────
    // ReadAsciiString / ASCII 문자열 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadAsciiString이 트림된 ASCII 문자열을 읽고 위치를 전진하는지 검증한다.
    ///     verifies that ReadAsciiString reads trimmed ASCII string and advances position.
    /// </summary>
    [Fact]
    public void ReadAsciiString_WithNullPadding_ReturnsTrimmedString() {
        // ByteBuilder로 제로 패딩된 ASCII 문자열 생성
        // build zero-padded ASCII string with ByteBuilder
        var data = new ByteBuilder().Ascii("HELLO", 10).Build();
        // 초기 위치
        // initial position
        var pos = 0;

        // ASCII 문자열 읽기
        // read ASCII string
        var value = BinarySpanReader.ReadAsciiString(data, ref pos, 10);

        // 트림된 문자열 확인
        // verify trimmed string
        Assert.Equal("HELLO", value);
        // 위치가 10인지 확인
        // verify position is 10
        Assert.Equal(10, pos);
    }

    /// <summary>
    ///     연속된 다중 타입 읽기에서 위치가 올바르게 누적되는지 검증한다.
    ///     verifies that position accumulates correctly across multiple consecutive reads.
    /// </summary>
    [Fact]
    public void SequentialReads_MultipleTypes_PositionAccumulatesCorrectly() {
        // 혼합 타입 데이터 구성: UInt16(2) + Int32(4) + Single(4) = 10바이트
        // build mixed type data: UInt16(2) + Int32(4) + Single(4) = 10 bytes
        var data = new ByteBuilder()
            .UInt16BigEndian(42)
            .Int32BigEndian(-99)
            .SingleBigEndian(1.5f)
            .Build();
        // 초기 위치
        // initial position
        var pos = 0;

        // UInt16 읽기
        // read UInt16
        var u16 = BinarySpanReader.ReadUInt16(data, ref pos);
        // Int32 읽기
        // read Int32
        var i32 = BinarySpanReader.ReadInt32(data, ref pos);
        // float 읽기
        // read float
        var f32 = BinarySpanReader.ReadSingle(data, ref pos);

        // UInt16 값 확인
        // verify UInt16 value
        Assert.Equal((ushort)42, u16);
        // Int32 값 확인
        // verify Int32 value
        Assert.Equal(-99, i32);
        // float 값 확인
        // verify float value
        Assert.Equal(1.5f, f32, 5);
        // 최종 위치가 10인지 확인
        // verify final position is 10
        Assert.Equal(10, pos);
    }
}