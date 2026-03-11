using HTool.Format.Param;
using Tester.TestHelpers;

namespace Tester.Format.Param;

/// <summary>
///     Preset 구조체의 파싱 동작을 검증한다.
///     verifies parsing behavior of the Preset struct.
/// </summary>
public sealed class PresetTests {
    /// <summary>
    ///     유효한 60바이트 데이터로 모드, 토크, 토크 리밋을 올바르게 파싱하는지 검증한다.
    ///     verifies that Mode, Torque, TorqueLimit are correctly parsed from valid 60-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesModeTorqueTorqueLimitCorrectly() {
        // 60바이트 테스트 데이터 생성
        // create 60-byte test data
        var data = TestData.Preset();

        // Preset 구조체 파싱
        // parse Preset struct
        var preset = new Preset(data);

        // Mode 값이 0 (TC/AM)인지 확인
        // verify Mode value is 0 (TC/AM)
        Assert.Equal(0, preset.Mode);
        // Torque 값이 15.0인지 확인
        // verify Torque value is 15.0
        Assert.Equal(15.0f, preset.Torque);
        // TorqueLimit 값이 20.0인지 확인
        // verify TorqueLimit value is 20.0
        Assert.Equal(20.0f, preset.TorqueLimit);
    }

    /// <summary>
    ///     각도 관련 필드들을 올바르게 파싱하는지 검증한다.
    ///     verifies that angle-related fields are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesAngleFieldsCorrectly() {
        // 60바이트 테스트 데이터 생성
        // create 60-byte test data
        var data = TestData.Preset();

        // Preset 구조체 파싱
        // parse Preset struct
        var preset = new Preset(data);

        // TargetAngle 값이 360인지 확인
        // verify TargetAngle value is 360
        Assert.Equal(360, preset.TargetAngle);
        // MinAngle 값이 10인지 확인
        // verify MinAngle value is 10
        Assert.Equal(10, preset.MinAngle);
        // MaxAngle 값이 720인지 확인
        // verify MaxAngle value is 720
        Assert.Equal(720, preset.MaxAngle);
        // SnugTorque 값이 5.0인지 확인
        // verify SnugTorque value is 5.0
        Assert.Equal(5.0f, preset.SnugTorque);
    }

    /// <summary>
    ///     속도 및 모터 제어 필드들을 올바르게 파싱하는지 검증한다.
    ///     verifies that speed and motor control fields are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesSpeedAndControlFieldsCorrectly() {
        // 60바이트 테스트 데이터 생성
        // create 60-byte test data
        var data = TestData.Preset();

        // Preset 구조체 파싱
        // parse Preset struct
        var preset = new Preset(data);

        // Speed 값이 500인지 확인
        // verify Speed value is 500
        Assert.Equal(500, preset.Speed);
        // FreeAngle 값이 30인지 확인
        // verify FreeAngle value is 30
        Assert.Equal(30, preset.FreeAngle);
        // FreeSpeed 값이 200인지 확인
        // verify FreeSpeed value is 200
        Assert.Equal(200, preset.FreeSpeed);
        // SoftStart 값이 50인지 확인
        // verify SoftStart value is 50
        Assert.Equal(50, preset.SoftStart);
        // SeatingPoint 값이 30인지 확인
        // verify SeatingPoint value is 30
        Assert.Equal(30, preset.SeatingPoint);
    }

    /// <summary>
    ///     스크류 타입과 소프트 스톱 필드를 올바르게 파싱하는지 검증한다.
    ///     verifies that ScrewType and SoftStop are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesScrewTypeAndSoftStopCorrectly() {
        // 60바이트 테스트 데이터 생성
        // create 60-byte test data
        var data = TestData.Preset();

        // Preset 구조체 파싱
        // parse Preset struct
        var preset = new Preset(data);

        // ScrewType 값이 0 (CW)인지 확인
        // verify ScrewType value is 0 (CW)
        Assert.Equal(0, preset.ScrewType);
        // SoftStop 값이 0 (disabled)인지 확인
        // verify SoftStop value is 0 (disabled)
        Assert.Equal(0, preset.SoftStop);
    }

    /// <summary>
    ///     데이터 길이가 부족할 때 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when data length is insufficient.
    /// </summary>
    [Fact]
    public void Constructor_UndersizedData_ThrowsFormatException() {
        // 59바이트 배열 생성 (최소 60바이트 미만)
        // create 59-byte array (below minimum 60 bytes)
        var data = new byte[59];

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new Preset(data));
    }

    /// <summary>
    ///     TryParse가 부족한 데이터에서 false를 반환하는지 검증한다.
    ///     verifies that TryParse returns false for undersized data.
    /// </summary>
    [Fact]
    public void TryParse_UndersizedData_ReturnsFalse() {
        // 30바이트 배열 생성 (최소 60바이트 미만)
        // create 30-byte array (below minimum 60 bytes)
        var data = new byte[30];

        // TryParse가 false를 반환하는지 확인
        // verify TryParse returns false
        var success = Preset.TryParse(data, out _);

        // 파싱 실패 확인
        // verify parsing failed
        Assert.False(success);
    }
}