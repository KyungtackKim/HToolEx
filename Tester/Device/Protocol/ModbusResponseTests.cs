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
    ///     생성자가 Code, Address, Length, Payload를 올바르게 설정하는지 검증한다.
    ///     Verifies constructor sets Code, Address, Length, and Payload correctly.
    /// </summary>
    [Fact]
    public void Constructor_ValidParams_PropertiesSet() {
        // 테스트 페이로드 생성
        // create test payload
        byte[] payload = [0x04, 0x00, 0x0A, 0x00, 0x0B];

        // ModbusResponse 생성
        // create ModbusResponse
        var response = new ModbusResponse(FunctionCode.ReadHoldingReg, 100, 0, payload);

        // 함수 코드 확인
        // verify function code
        Assert.Equal(FunctionCode.ReadHoldingReg, response.Code);
        // 주소 확인
        // verify address
        Assert.Equal(100, response.Address);
        // 길이 필드 확인 (LEN 분리 대상이 아닌 FC는 0)
        // verify length field (FCs without an explicit LEN are 0)
        Assert.Equal(0, response.Length);
        // 페이로드 길이 확인
        // verify payload length
        Assert.Equal(5, response.Payload.Length);
    }

    #endregion

    #region Decode

    /// <summary>
    ///     LEN 필드가 없는 FC는 페이로드를 원본 그대로 노출하는지 검증한다.
    ///     Verifies Decode passes payload through unchanged for FCs without a LEN field.
    /// </summary>
    [Fact]
    public void Decode_NonLengthBearingFc_PassesPayloadThrough() {
        // 비-그래프 FC 페이로드 (ByteCount + Data)
        // non-graph FC payload (ByteCount + Data)
        byte[] payload = [0x04, 0x00, 0x0A, 0x00, 0x0B];

        // Decode 호출
        // invoke Decode
        var response = ModbusResponse.Decode(FunctionCode.ReadHoldingReg, 100, payload);

        // Length는 0 (필드 없음)
        // Length is 0 (no field)
        Assert.Equal(0, response.Length);
        // 페이로드는 원본 그대로
        // payload preserved as-is
        Assert.Equal(payload.Length, response.Payload.Length);
        // 첫 바이트 일치 확인
        // verify first byte matches
        Assert.Equal(payload[0], response.Payload.Span[0]);
    }

    /// <summary>
    ///     그래프 계열 FC는 앞 2바이트 LEN을 분리하고 페이로드는 그 뒤 데이터만 남기는지 검증한다.
    ///     Verifies Decode splits the leading 2-byte LEN out of graph-family FCs and leaves only the data in payload.
    /// </summary>
    [Theory]
    [InlineData((byte)FunctionCode.GraphData)]
    [InlineData((byte)FunctionCode.GraphRes)]
    [InlineData((byte)FunctionCode.HighResGraph)]
    public void Decode_GraphFamily_SplitsLengthAndStripsPrefix(byte fc) {
        // 원시 페이로드: LEN(BigEndian 0x0003) + Data(3바이트)
        // raw payload: LEN(BigEndian 0x0003) + Data(3 bytes)
        byte[] raw = [0x00, 0x03, 0xAA, 0xBB, 0xCC];

        // Decode 호출
        // invoke Decode
        var response = ModbusResponse.Decode((FunctionCode)fc, 0, raw);

        // LEN 필드가 3으로 분리되었는지
        // LEN field surfaces as 3
        Assert.Equal(3, response.Length);
        // 페이로드는 LEN을 제외한 3바이트
        // payload contains only the 3 data bytes
        Assert.Equal(3, response.Payload.Length);
        // 페이로드 첫 바이트가 데이터 시작과 일치
        // first payload byte matches the start of the data section
        Assert.Equal(0xAA, response.Payload.Span[0]);
    }

    /// <summary>
    ///     그래프 FC라도 LEN 필드가 잘려있으면 분리하지 않고 원본 그대로 노출하는지 검증한다.
    ///     Verifies Decode does not split when the LEN field itself is truncated, even for graph FCs.
    /// </summary>
    [Fact]
    public void Decode_GraphFamily_TruncatedLengthField_PassesThrough() {
        // LEN 2바이트가 없는 잘린 페이로드
        // truncated payload missing the 2-byte LEN
        byte[] raw = [0xAA];

        // Decode 호출
        // invoke Decode
        var response = ModbusResponse.Decode(FunctionCode.HighResGraph, 0, raw);

        // 분리 불가능 — Length는 0
        // cannot split — Length is 0
        Assert.Equal(0, response.Length);
        // 페이로드 원본 그대로 보존
        // payload preserved as-is
        Assert.Equal(1, response.Payload.Length);
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
        var    response = new ModbusResponse(FunctionCode.ReadHoldingReg, 100, 0, payload);

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
        var    response = new ModbusResponse(FunctionCode.Error, 0, 0, payload);

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
        var response = new ModbusResponse(FunctionCode.Error, 0, 0, ReadOnlyMemory<byte>.Empty);

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
        var    response = new ModbusResponse(FunctionCode.Error, 0, 0, payload);

        // 예외 코드 조회
        // get exception code
        var exception = response.Exception;

        // 기대되는 예외 코드와 일치 확인
        // verify matches expected exception code
        Assert.Equal(expected, exception);
    }

    #endregion
}