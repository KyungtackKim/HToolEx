using HTool.Format.Device;
using Tester.TestHelpers;

namespace Tester.Format.Device;

/// <summary>
///     SimpleInfo 구조체의 파싱 동작을 검증한다.
///     verifies parsing behavior of the SimpleInfo struct.
/// </summary>
public sealed class SimpleInfoTests {
    /// <summary>
    ///     유효한 13바이트 데이터로 Id를 올바르게 파싱하는지 검증한다.
    ///     verifies that Id is correctly parsed from valid 13-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesIdCorrectly() {
        // 13바이트 테스트 데이터 생성
        // create 13-byte test data
        var data = TestData.SimpleInfo();

        // SimpleInfo 구조체 파싱
        // parse SimpleInfo struct
        var info = new SimpleInfo(data);

        // Id 값이 1인지 확인
        // verify Id value is 1
        Assert.Equal((ushort)1, info.Id);
    }

    /// <summary>
    ///     유효한 13바이트 데이터로 Controller, Driver, Firmware를 올바르게 파싱하는지 검증한다.
    ///     verifies that Controller, Driver, Firmware are correctly parsed from valid 13-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesDeviceFieldsCorrectly() {
        // 13바이트 테스트 데이터 생성
        // create 13-byte test data
        var data = TestData.SimpleInfo();

        // SimpleInfo 구조체 파싱
        // parse SimpleInfo struct
        var info = new SimpleInfo(data);

        // Controller 값이 100인지 확인
        // verify Controller value is 100
        Assert.Equal((ushort)100, info.Controller);
        // Driver 값이 200인지 확인
        // verify Driver value is 200
        Assert.Equal((ushort)200, info.Driver);
        // Firmware 값이 10인지 확인
        // verify Firmware value is 10
        Assert.Equal((ushort)10, info.Firmware);
    }

    /// <summary>
    ///     시리얼 바이트가 역순으로 조합되는지 검증한다.
    ///     verifies that serial bytes are composed in reverse order.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesSerialInReverseOrder() {
        // 13바이트 테스트 데이터 생성
        // create 13-byte test data
        var data = TestData.SimpleInfo();

        // SimpleInfo 구조체 파싱
        // parse SimpleInfo struct
        var info = new SimpleInfo(data);

        // 시리얼이 역순(0x05,0x04,0x03,0x02,0x01)으로 조합되었는지 확인
        // verify serial is composed in reverse order (0x05,0x04,0x03,0x02,0x01)
        Assert.Equal("0504030201", info.Serial);
    }

    /// <summary>
    ///     시리얼 번호에서 모델 코드가 올바르게 추출되는지 검증한다.
    ///     verifies that ModelCode is correctly extracted from serial number.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ExtractsModelCodeFromSerial() {
        // 13바이트 테스트 데이터 생성
        // create 13-byte test data
        var data = TestData.SimpleInfo();

        // SimpleInfo 구조체 파싱
        // parse SimpleInfo struct
        var info = new SimpleInfo(data);

        // 시리얼 "0504030201"의 [4..6] = "03" -> ModelCode = 3
        // serial "0504030201"[4..6] = "03" -> ModelCode = 3
        Assert.Equal((ushort)3, info.ModelCode);
    }

    /// <summary>
    ///     17바이트 데이터에서 Used 필드가 올바르게 파싱되는지 검증한다.
    ///     verifies that Used field is correctly parsed from 17-byte data.
    /// </summary>
    [Fact]
    public void Constructor_DataWithUsed_ParsesUsedField() {
        // 17바이트 테스트 데이터 생성 (Used=500 포함)
        // create 17-byte test data (with Used=500)
        var data = TestData.SimpleInfoWithUsed();

        // SimpleInfo 구조체 파싱
        // parse SimpleInfo struct
        var info = new SimpleInfo(data);

        // Used 값이 500인지 확인
        // verify Used value is 500
        Assert.Equal(500u, info.Used);
    }

    /// <summary>
    ///     13바이트 데이터에서 Used 필드가 0으로 유지되는지 검증한다.
    ///     verifies that Used field remains 0 when data is exactly 13 bytes.
    /// </summary>
    [Fact]
    public void Constructor_DataWithoutUsed_UsedIsZero() {
        // 13바이트 테스트 데이터 생성 (Used 없음)
        // create 13-byte test data (no Used field)
        var data = TestData.SimpleInfo();

        // SimpleInfo 구조체 파싱
        // parse SimpleInfo struct
        var info = new SimpleInfo(data);

        // Used 기본값이 0인지 확인
        // verify Used defaults to 0
        Assert.Equal(0u, info.Used);
    }

    /// <summary>
    ///     데이터 길이가 부족할 때 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when data length is insufficient.
    /// </summary>
    [Fact]
    public void Constructor_UndersizedData_ThrowsFormatException() {
        // 12바이트 배열 생성 (최소 13바이트 미만)
        // create 12-byte array (below minimum 13 bytes)
        var data = new byte[12];

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new SimpleInfo(data));
    }

    /// <summary>
    ///     TryParse가 부족한 데이터에서 false를 반환하는지 검증한다.
    ///     verifies that TryParse returns false for undersized data.
    /// </summary>
    [Fact]
    public void TryParse_UndersizedData_ReturnsFalse() {
        // 12바이트 배열 생성 (최소 13바이트 미만)
        // create 12-byte array (below minimum 13 bytes)
        var data = new byte[12];

        // TryParse가 false를 반환하는지 확인
        // verify TryParse returns false
        var success = SimpleInfo.TryParse(data, out _);

        // 파싱 실패 확인
        // verify parsing failed
        Assert.False(success);
    }
}