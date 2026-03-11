using HTool.Device.Protocol;
using HTool.Type;

namespace Tester.Device.Protocol;

/// <summary>
///     ModbusResponse 단위 테스트.
///     Unit tests for ModbusResponse.
/// </summary>
public sealed class ModbusResponseTests {
    #region Basic Construction

    /// <summary>
    ///     생성자가 Code, Address, Payload를 올바르게 설정하는지 검증한다.
    ///     Verifies constructor sets Code, Address, and Payload correctly.
    /// </summary>
    [Fact]
    public void Constructor_ValidParams_PropertiesSet() {
        // 테스트 페이로드 생성
        // create test payload
        byte[] payload = [0x04, 0x00, 0x0A, 0x00, 0x0B];

        // ModbusResponse 생성
        // create ModbusResponse
        var response = new ModbusResponse(FunctionCode.ReadHoldingReg, 100, payload);

        // 함수 코드 확인
        // verify function code
        Assert.Equal(FunctionCode.ReadHoldingReg, response.Code);
        // 주소 확인
        // verify address
        Assert.Equal(100, response.Address);
        // 페이로드 길이 확인
        // verify payload length
        Assert.Equal(5, response.Payload.Length);
    }

    #endregion

    #region Exception Property

    /// <summary>
    ///     정상 응답에서 Exception이 null인지 검증한다.
    ///     Verifies Exception is null for non-error responses.
    /// </summary>
    [Fact]
    public void Exception_NormalResponse_ReturnsNull() {
        // 정상 응답 생성 (FC 0x03)
        // create normal response (FC 0x03)
        byte[] payload  = [0x04, 0x00, 0x0A, 0x00, 0x0B];
        var    response = new ModbusResponse(FunctionCode.ReadHoldingReg, 100, payload);

        // 예외 코드 조회
        // get exception code
        var exception = response.Exception;

        // 정상 응답이므로 null
        // null for normal response
        Assert.Null(exception);
    }

    /// <summary>
    ///     오류 응답에서 Exception이 올바른 예외 코드를 반환하는지 검증한다.
    ///     Verifies Exception returns correct exception code for error responses.
    /// </summary>
    [Fact]
    public void Exception_ErrorResponse_ReturnsExceptionCode() {
        // 오류 응답 생성 (FC=Error, 페이로드=[0x02] = IllegalDataAddress)
        // create error response (FC=Error, payload=[0x02] = IllegalDataAddress)
        byte[] payload  = [0x02];
        var    response = new ModbusResponse(FunctionCode.Error, 0, payload);

        // 예외 코드 조회
        // get exception code
        var exception = response.Exception;

        // IllegalDataAddress 예외 코드 확인
        // verify IllegalDataAddress exception code
        Assert.Equal(ModbusExceptionCode.IllegalDataAddress, exception);
    }

    /// <summary>
    ///     오류 응답이지만 페이로드가 비어있으면 Exception이 null인지 검증한다.
    ///     Verifies Exception is null when error response has empty payload.
    /// </summary>
    [Fact]
    public void Exception_ErrorResponseEmptyPayload_ReturnsNull() {
        // 빈 페이로드의 오류 응답 생성
        // create error response with empty payload
        var response = new ModbusResponse(FunctionCode.Error, 0, ReadOnlyMemory<byte>.Empty);

        // 예외 코드 조회
        // get exception code
        var exception = response.Exception;

        // 페이로드가 비어있으면 null
        // null when payload is empty
        Assert.Null(exception);
    }

    /// <summary>
    ///     여러 MODBUS 예외 코드를 올바르게 파싱하는지 검증한다.
    ///     Verifies various MODBUS exception codes are parsed correctly.
    /// </summary>
    [Theory]
    [InlineData(0x01, ModbusExceptionCode.IllegalFunction)]
    [InlineData(0x03, ModbusExceptionCode.IllegalDataValue)]
    [InlineData(0x04, ModbusExceptionCode.SlaveDeviceFailure)]
    [InlineData(0x06, ModbusExceptionCode.SlaveDeviceBusy)]
    public void Exception_VariousCodes_ParsedCorrectly(byte code, ModbusExceptionCode expected) {
        // 지정된 예외 코드로 오류 응답 생성
        // create error response with specified exception code
        byte[] payload  = [code];
        var    response = new ModbusResponse(FunctionCode.Error, 0, payload);

        // 예외 코드 조회
        // get exception code
        var exception = response.Exception;

        // 기대되는 예외 코드와 일치 확인
        // verify matches expected exception code
        Assert.Equal(expected, exception);
    }

    #endregion
}