using HTool.Format.Device;
using Tester.TestHelpers;

namespace Tester.Format.Device;

/// <summary>
///     Info 구조체의 파싱 동작을 검증한다.
///     verifies parsing behavior of the Info struct.
/// </summary>
public sealed class InfoTests {
    /// <summary>
    ///     유효한 200바이트 데이터로 드라이버 필드들을 올바르게 파싱하는지 검증한다.
    ///     verifies that driver fields are correctly parsed from valid 200-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesDriverFieldsCorrectly() {
        // 200바이트 테스트 데이터 생성
        // create 200-byte test data
        var data = TestData.Info();

        // Info 구조체 파싱
        // parse Info struct
        var info = new Info(data);

        // DriverId 값이 1인지 확인
        // verify DriverId value is 1
        Assert.Equal((ushort)1, info.DriverId);
        // DriverModelNumber 값이 15인지 확인
        // verify DriverModelNumber value is 15
        Assert.Equal((ushort)15, info.DriverModelNumber);
        // DriverModelName 값 확인
        // verify DriverModelName value
        Assert.Equal("MDT-100", info.DriverModelName);
        // DriverSerialNumber 값 확인
        // verify DriverSerialNumber value
        Assert.Equal("SN12345678", info.DriverSerialNumber);
    }

    /// <summary>
    ///     유효한 200바이트 데이터로 컨트롤러 필드들을 올바르게 파싱하는지 검증한다.
    ///     verifies that controller fields are correctly parsed from valid 200-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesControllerFieldsCorrectly() {
        // 200바이트 테스트 데이터 생성
        // create 200-byte test data
        var data = TestData.Info();

        // Info 구조체 파싱
        // parse Info struct
        var info = new Info(data);

        // ControllerModelNumber 값이 15인지 확인
        // verify ControllerModelNumber value is 15
        Assert.Equal((ushort)15, info.ControllerModelNumber);
        // ControllerModelName 값 확인
        // verify ControllerModelName value
        Assert.Equal("MDT-CTRL", info.ControllerModelName);
        // ControllerSerialNumber 값 확인
        // verify ControllerSerialNumber value
        Assert.Equal("CTRL00001", info.ControllerSerialNumber);
    }

    /// <summary>
    ///     펌웨어 버전 필드와 계산된 문자열을 올바르게 파싱하는지 검증한다.
    ///     verifies that firmware version fields and computed string are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesFirmwareVersionCorrectly() {
        // 200바이트 테스트 데이터 생성
        // create 200-byte test data
        var data = TestData.Info();

        // Info 구조체 파싱
        // parse Info struct
        var info = new Info(data);

        // FirmwareVersionMajor 값이 2인지 확인
        // verify FirmwareVersionMajor value is 2
        Assert.Equal((ushort)2, info.FirmwareVersionMajor);
        // FirmwareVersionMinor 값이 1인지 확인
        // verify FirmwareVersionMinor value is 1
        Assert.Equal((ushort)1, info.FirmwareVersionMinor);
        // FirmwareVersionPatch 값이 5인지 확인
        // verify FirmwareVersionPatch value is 5
        Assert.Equal((ushort)5, info.FirmwareVersionPatch);
        // 조합된 FirmwareVersion 문자열 확인
        // verify computed FirmwareVersion string
        Assert.Equal("2.1.5", info.FirmwareVersion);
    }

    /// <summary>
    ///     MAC 주소 바이트와 문자열 표현을 올바르게 파싱하는지 검증한다.
    ///     verifies that MAC address bytes and string representation are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesMacAddressCorrectly() {
        // 200바이트 테스트 데이터 생성
        // create 200-byte test data
        var data = TestData.Info();

        // Info 구조체 파싱
        // parse Info struct
        var info = new Info(data);

        // MAC 주소 바이트 배열 확인
        // verify MAC address byte array
        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF }, info.MacAddress);
        // MAC 주소 문자열 확인
        // verify MAC address string representation
        Assert.Equal("AA:BB:CC:DD:EE:FF", info.MacAddressString);
    }

    /// <summary>
    ///     생산 일자와 기타 메타데이터 필드를 올바르게 파싱하는지 검증한다.
    ///     verifies that production date and other metadata fields are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesMetadataFieldsCorrectly() {
        // 200바이트 테스트 데이터 생성
        // create 200-byte test data
        var data = TestData.Info();

        // Info 구조체 파싱
        // parse Info struct
        var info = new Info(data);

        // ProductionDate 값이 20250101인지 확인
        // verify ProductionDate value is 20250101
        Assert.Equal(20250101u, info.ProductionDate);
        // EventDataRevision 값이 1인지 확인
        // verify EventDataRevision value is 1
        Assert.Equal((ushort)1, info.EventDataRevision);
        // ManufacturerCode 값이 1인지 확인
        // verify ManufacturerCode value is 1
        Assert.Equal((ushort)1, info.ManufacturerCode);
    }

    /// <summary>
    ///     Size 정적 속성이 200을 반환하는지 검증한다.
    ///     verifies that the static Size property returns 200.
    /// </summary>
    [Fact]
    public void Size_Always_Returns200() {
        // Size 값이 200인지 확인
        // verify Size value is 200
        Assert.Equal(200, Info.Size);
    }

    /// <summary>
    ///     데이터 길이가 부족할 때 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when data length is insufficient.
    /// </summary>
    [Fact]
    public void Constructor_UndersizedData_ThrowsFormatException() {
        // 199바이트 배열 생성 (최소 200바이트 미만)
        // create 199-byte array (below minimum 200 bytes)
        var data = new byte[199];

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new Info(data));
    }

    /// <summary>
    ///     TryParse가 부족한 데이터에서 false를 반환하는지 검증한다.
    ///     verifies that TryParse returns false for undersized data.
    /// </summary>
    [Fact]
    public void TryParse_UndersizedData_ReturnsFalse() {
        // 100바이트 배열 생성 (최소 200바이트 미만)
        // create 100-byte array (below minimum 200 bytes)
        var data = new byte[100];

        // TryParse가 false를 반환하는지 확인
        // verify TryParse returns false
        var success = Info.TryParse(data, out _);

        // 파싱 실패 확인
        // verify parsing failed
        Assert.False(success);
    }
}