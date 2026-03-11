using HTool.Format.Process;
using Tester.TestHelpers;

namespace Tester.Format.Process;

/// <summary>
///     Barcode 구조체의 파싱 동작을 검증한다.
///     verifies parsing behavior of the Barcode struct.
/// </summary>
public sealed class BarcodeTests {
    /// <summary>
    ///     유효한 64바이트 데이터로 바코드 값을 올바르게 파싱하는지 검증한다.
    ///     verifies that barcode value is correctly parsed from valid 64-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesValueCorrectly() {
        // 64바이트 테스트 데이터 생성
        // create 64-byte test data
        var data = TestData.Barcode();

        // Barcode 구조체 파싱
        // parse Barcode struct
        var barcode = new Barcode(data);

        // Value가 "TEST-BARCODE-001"인지 확인 (널 트림 후)
        // verify Value is "TEST-BARCODE-001" (after null trimming)
        Assert.Equal("TEST-BARCODE-001", barcode.Value);
    }

    /// <summary>
    ///     파싱된 바코드의 해시 값이 0이 아닌지 검증한다.
    ///     verifies that parsed barcode Hash is non-zero.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ComputesNonZeroHash() {
        // 64바이트 테스트 데이터 생성
        // create 64-byte test data
        var data = TestData.Barcode();

        // Barcode 구조체 파싱
        // parse Barcode struct
        var barcode = new Barcode(data);

        // Hash 값이 0이 아닌지 확인
        // verify Hash value is non-zero
        Assert.NotEqual(0UL, barcode.Hash);
    }

    /// <summary>
    ///     Size 정적 속성이 64를 반환하는지 검증한다.
    ///     verifies that the static Size property returns 64.
    /// </summary>
    [Fact]
    public void Size_Always_Returns64() {
        // Size 값이 64인지 확인
        // verify Size value is 64
        Assert.Equal(64, Barcode.Size);
    }

    /// <summary>
    ///     데이터 길이가 부족할 때 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when data length is insufficient.
    /// </summary>
    [Fact]
    public void Constructor_UndersizedData_ThrowsFormatException() {
        // 63바이트 배열 생성 (최소 64바이트 미만)
        // create 63-byte array (below minimum 64 bytes)
        var data = new byte[63];

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new Barcode(data));
    }

    /// <summary>
    ///     TryParse가 부족한 데이터에서 false를 반환하는지 검증한다.
    ///     verifies that TryParse returns false for undersized data.
    /// </summary>
    [Fact]
    public void TryParse_UndersizedData_ReturnsFalse() {
        // 32바이트 배열 생성 (최소 64바이트 미만)
        // create 32-byte array (below minimum 64 bytes)
        var data = new byte[32];

        // TryParse가 false를 반환하는지 확인
        // verify TryParse returns false
        var success = Barcode.TryParse(data, out _);

        // 파싱 실패 확인
        // verify parsing failed
        Assert.False(success);
    }
}