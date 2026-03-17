using HTool.Device.Protocol;
using HTool.Type;

namespace Tester.Device.Protocol;

/// <summary>
///     ModbusRequest 단위 테스트.
///     Unit tests for ModbusRequest.
/// </summary>
public sealed class ModbusRequestTests {
    #region Activate

    /// <summary>
    ///     Activate 호출 후 Activated가 true로 변경되고 ActiveTime이 기록되는지 검증한다.
    ///     Verifies Activate sets Activated to true and records ActiveTime.
    /// </summary>
    [Fact]
    public void Activate_Called_ActivatedTrueAndTimeRecorded() {
        // 테스트 패킷 데이터 생성
        // create test packet data
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00];
        // ModbusRequest 생성
        // create ModbusRequest
        var request = new ModbusRequest(FunctionCode.ReadHoldingReg, 0, packet);
        // 활성화 전 시각 기록
        // record time before activation
        var before = DateTime.UtcNow;

        // 요청 활성화
        // activate request
        request.Activate();

        // Activated가 true인지 확인
        // verify Activated is true
        Assert.True(request.Activated);
        // ActiveTime이 호출 시각 이후인지 확인
        // verify ActiveTime is after the call time
        Assert.True(request.ActiveTime >= before);
    }

    #endregion

    #region Constructor / Properties

    /// <summary>
    ///     생성자가 Code, Address, Packet을 올바르게 설정하는지 검증한다.
    ///     Verifies constructor sets Code, Address, and Packet correctly.
    /// </summary>
    [Fact]
    public void Constructor_ValidParams_PropertiesSet() {
        // 테스트 패킷 데이터 생성
        // create test packet data
        byte[] packet = [0x01, 0x03, 0x00, 0x64, 0x00, 0x0A, 0x00, 0x00];

        // ModbusRequest 생성
        // create ModbusRequest
        var request = new ModbusRequest(FunctionCode.ReadHoldingReg, 100, packet);

        // 함수 코드 확인
        // verify function code
        Assert.Equal(FunctionCode.ReadHoldingReg, request.Code);
        // 주소 확인
        // verify address
        Assert.Equal(100, request.Address);
        // 패킷 길이 확인
        // verify packet length
        Assert.Equal(8, request.Packet.Length);
    }

    /// <summary>
    ///     생성자가 고유 키를 올바르게 생성하는지 검증한다.
    ///     Verifies constructor generates unique key correctly.
    /// </summary>
    [Fact]
    public void Constructor_ValidParams_KeyGenerated() {
        // 테스트 패킷 데이터 생성
        // create test packet data
        byte[] packet = [0x01, 0x06, 0x00, 0x32, 0x04, 0xD2, 0x00, 0x00];

        // ModbusRequest 생성
        // create ModbusRequest
        var request = new ModbusRequest(FunctionCode.WriteSingleReg, 50, packet);

        // 키의 함수 코드 확인
        // verify key's function code
        Assert.Equal(FunctionCode.WriteSingleReg, request.Key.Code);
        // 키의 주소 확인
        // verify key's address
        Assert.Equal(50, request.Key.Address);
    }

    /// <summary>
    ///     초기 상태에서 Activated가 false인지 검증한다.
    ///     Verifies Activated is false in initial state.
    /// </summary>
    [Fact]
    public void Constructor_InitialState_ActivatedIsFalse() {
        // 테스트 패킷 데이터 생성
        // create test packet data
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00];

        // ModbusRequest 생성
        // create ModbusRequest
        var request = new ModbusRequest(FunctionCode.ReadHoldingReg, 0, packet);

        // 초기 Activated 상태 확인
        // verify initial Activated state
        Assert.False(request.Activated);
    }

    #endregion

    #region Deactivate

    /// <summary>
    ///     Deactivate 호출 후 Activated가 false로 변경되고 재시도 횟수가 증가하는지 검증한다.
    ///     Verifies Deactivate sets Activated to false and increments retry count.
    /// </summary>
    [Fact]
    public void Deactivate_AfterActivate_ReturnIncrementedRetryCount() {
        // 테스트 패킷 데이터 생성
        // create test packet data
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00];
        // ModbusRequest 생성
        // create ModbusRequest
        var request = new ModbusRequest(FunctionCode.ReadHoldingReg, 0, packet);
        // 요청 활성화
        // activate request
        request.Activate();

        // 비활성화하고 재시도 횟수 받기
        // deactivate and get retry count
        var retryCount = request.Deactivate();

        // Activated가 false인지 확인
        // verify Activated is false
        Assert.False(request.Activated);
        // 첫 번째 비활성화 후 재시도 횟수 1 확인
        // verify retry count is 1 after first deactivation
        Assert.Equal(1, retryCount);
    }

    /// <summary>
    ///     Deactivate를 여러 번 호출하면 재시도 횟수가 누적되는지 검증한다.
    ///     Verifies multiple Deactivate calls accumulate retry count.
    /// </summary>
    [Fact]
    public void Deactivate_CalledMultipleTimes_RetryCountAccumulates() {
        // 테스트 패킷 데이터 생성
        // create test packet data
        byte[] packet = [0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00];
        // ModbusRequest 생성
        // create ModbusRequest
        var request = new ModbusRequest(FunctionCode.ReadHoldingReg, 0, packet);

        // 첫 번째 비활성화
        // first deactivation
        request.Activate();
        var first = request.Deactivate();
        // 두 번째 비활성화
        // second deactivation
        request.Activate();
        var second = request.Deactivate();
        // 세 번째 비활성화
        // third deactivation
        request.Activate();
        var third = request.Deactivate();

        // 재시도 횟수 누적 확인
        // verify retry count accumulation
        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(3, third);
    }

    #endregion

    #region Key Uniqueness

    /// <summary>
    ///     동일한 파라미터로 생성한 두 요청의 키가 같은지 검증한다.
    ///     Verifies two requests with same parameters produce equal keys.
    /// </summary>
    [Fact]
    public void Key_SameParams_KeysEqual() {
        // 동일한 패킷 데이터로 두 요청 생성
        // create two requests with identical packet data
        byte[] packet = [0x01, 0x03, 0x00, 0x64, 0x00, 0x0A, 0x00, 0x00];
        var    req1   = new ModbusRequest(FunctionCode.ReadHoldingReg, 100, packet);
        var    req2   = new ModbusRequest(FunctionCode.ReadHoldingReg, 100, packet);

        // 두 키가 동일한지 확인
        // verify both keys are equal
        Assert.Equal(req1.Key, req2.Key);
    }

    /// <summary>
    ///     다른 값을 가진 두 쓰기 요청의 키가 해시로 구별되는지 검증한다.
    ///     Verifies two write requests with different values are distinguished by hash.
    /// </summary>
    [Fact]
    public void Key_DifferentValues_KeysNotEqual() {
        // 서로 다른 값을 가진 쓰기 패킷 생성
        // create write packets with different values
        byte[] packet1 = [0x01, 0x06, 0x00, 0x32, 0x00, 0x01, 0x00, 0x00];
        byte[] packet2 = [0x01, 0x06, 0x00, 0x32, 0x00, 0x02, 0x00, 0x00];
        var    req1    = new ModbusRequest(FunctionCode.WriteSingleReg, 50, packet1);
        var    req2    = new ModbusRequest(FunctionCode.WriteSingleReg, 50, packet2);

        // 두 키가 다른지 확인 (해시 차이)
        // verify keys differ (hash difference)
        Assert.NotEqual(req1.Key, req2.Key);
    }

    #endregion
}