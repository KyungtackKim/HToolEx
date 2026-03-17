using HTool.Core.Type.Process;
using HTool.Format.Device;
using Tester.TestHelpers;

namespace Tester.Format.Device;

/// <summary>
///     Status 구조체의 파싱 동작을 검증한다.
///     verifies parsing behavior of the Status struct.
/// </summary>
public sealed class StatusTests {
    /// <summary>
    ///     유효한 38바이트 데이터로 토크와 속도를 올바르게 파싱하는지 검증한다.
    ///     verifies that Torque and Speed are correctly parsed from valid 38-byte data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesTorqueAndSpeedCorrectly() {
        // 38바이트 테스트 데이터 생성
        // create 38-byte test data
        var data = TestData.Status();

        // Status 구조체 파싱
        // parse Status struct
        var status = new Status(data);

        // Torque 값이 10.5인지 확인
        // verify Torque value is 10.5
        Assert.Equal(10.5f, status.Torque);
        // Speed 값이 300인지 확인
        // verify Speed value is 300
        Assert.Equal((ushort)300, status.Speed);
    }

    /// <summary>
    ///     전류, 프리셋, 모델 필드를 올바르게 파싱하는지 검증한다.
    ///     verifies that Current, Preset, Model fields are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesCurrentPresetModelCorrectly() {
        // 38바이트 테스트 데이터 생성
        // create 38-byte test data
        var data = TestData.Status();

        // Status 구조체 파싱
        // parse Status struct
        var status = new Status(data);

        // Current 값이 1.2인지 확인
        // verify Current value is 1.2
        Assert.Equal(1.2f, status.Current);
        // Preset 값이 5인지 확인
        // verify Preset value is 5
        Assert.Equal((ushort)5, status.Preset);
        // Model 값이 1인지 확인
        // verify Model value is 1
        Assert.Equal((ushort)1, status.Model);
    }

    /// <summary>
    ///     상태 플래그(TorqueUp, FastenOk, Ready, Run)를 올바르게 파싱하는지 검증한다.
    ///     verifies that state flags (TorqueUp, FastenOk, Ready, Run) are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesStateFlagsCorrectly() {
        // 38바이트 테스트 데이터 생성
        // create 38-byte test data
        var data = TestData.Status();

        // Status 구조체 파싱
        // parse Status struct
        var status = new Status(data);

        // TorqueUp이 true인지 확인 (nonzero = true)
        // verify TorqueUp is true (nonzero = true)
        Assert.True(status.TorqueUp);
        // FastenOk이 false인지 확인 (zero = false)
        // verify FastenOk is false (zero = false)
        Assert.False(status.FastenOk);
        // Ready가 true인지 확인 (nonzero = true)
        // verify Ready is true (nonzero = true)
        Assert.True(status.Ready);
        // Run이 true인지 확인 (nonzero = true)
        // verify Run is true (nonzero = true)
        Assert.True(status.Run);
    }

    /// <summary>
    ///     방향과 남은 나사 개수를 올바르게 파싱하는지 검증한다.
    ///     verifies that Direction and RemainScrew are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesDirectionAndRemainScrewCorrectly() {
        // 38바이트 테스트 데이터 생성
        // create 38-byte test data
        var data = TestData.Status();

        // Status 구조체 파싱
        // parse Status struct
        var status = new Status(data);

        // Direction이 Fastening인지 확인 (0 = Fastening)
        // verify Direction is Fastening (0 = Fastening)
        Assert.Equal(Direction.Fastening, status.Direction);
        // RemainScrew 값이 3인지 확인
        // verify RemainScrew value is 3
        Assert.Equal((ushort)3, status.RemainScrew);
    }

    /// <summary>
    ///     입출력 신호 비트 배열을 올바르게 파싱하는지 검증한다.
    ///     verifies that Input and Output signal bit arrays are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesInputOutputSignalsCorrectly() {
        // 38바이트 테스트 데이터 생성
        // create 38-byte test data
        var data = TestData.Status();

        // Status 구조체 파싱
        // parse Status struct
        var status = new Status(data);

        // Input 배열이 16비트인지 확인
        // verify Input array has 16 bits
        Assert.Equal(16, status.Input.Length);
        // Input bit0이 true인지 확인 (0x0003 bit0=1)
        // verify Input bit0 is true (0x0003 bit0=1)
        Assert.True(status.Input[0]);
        // Input bit1이 true인지 확인 (0x0003 bit1=1)
        // verify Input bit1 is true (0x0003 bit1=1)
        Assert.True(status.Input[1]);
        // Input bit2가 false인지 확인 (0x0003 bit2=0)
        // verify Input bit2 is false (0x0003 bit2=0)
        Assert.False(status.Input[2]);
        // Output bit0이 true인지 확인 (0x0001 bit0=1)
        // verify Output bit0 is true (0x0001 bit0=1)
        Assert.True(status.Output[0]);
        // Output bit1이 false인지 확인 (0x0001 bit1=0)
        // verify Output bit1 is false (0x0001 bit1=0)
        Assert.False(status.Output[1]);
    }

    /// <summary>
    ///     온도와 잠금 상태를 올바르게 파싱하는지 검증한다.
    ///     verifies that Temperature and IsLock are correctly parsed.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesTemperatureAndLockCorrectly() {
        // 38바이트 테스트 데이터 생성
        // create 38-byte test data
        var data = TestData.Status();

        // Status 구조체 파싱
        // parse Status struct
        var status = new Status(data);

        // Temperature 값이 35.5인지 확인
        // verify Temperature value is 35.5
        Assert.Equal(35.5f, status.Temperature);
        // IsLock이 false인지 확인 (zero = false)
        // verify IsLock is false (zero = false)
        Assert.False(status.IsLock);
    }

    /// <summary>
    ///     데이터 길이가 부족할 때 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when data length is insufficient.
    /// </summary>
    [Fact]
    public void Constructor_UndersizedData_ThrowsFormatException() {
        // 37바이트 배열 생성 (최소 38바이트 미만)
        // create 37-byte array (below minimum 38 bytes)
        var data = new byte[37];

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new Status(data));
    }

    /// <summary>
    ///     TryParse가 부족한 데이터에서 false를 반환하는지 검증한다.
    ///     verifies that TryParse returns false for undersized data.
    /// </summary>
    [Fact]
    public void TryParse_UndersizedData_ReturnsFalse() {
        // 10바이트 배열 생성 (최소 38바이트 미만)
        // create 10-byte array (below minimum 38 bytes)
        var data = new byte[10];

        // TryParse가 false를 반환하는지 확인
        // verify TryParse returns false
        var success = Status.TryParse(data, out _);

        // 파싱 실패 확인
        // verify parsing failed
        Assert.False(success);
    }
}