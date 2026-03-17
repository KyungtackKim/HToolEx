using HTool.Type;

namespace HTool.Device.Protocol;

/// <summary>
///     MODBUS 응답 데이터를 담는 레코드 구조체.
///     Record struct containing MODBUS response data.
/// </summary>
/// <param name="Code">함수 코드 / function code</param>
/// <param name="Address">
///     요청과 매칭된 주소. 비요청 응답(FC 0x64, 0x65, 0x66)의 경우 0.
///     Address matched from request. 0 for unsolicited responses (FC 0x64, 0x65, 0x66).
/// </param>
/// <param name="Payload">
///     페이로드 데이터 (헤더/CRC 제외).
///     Payload data (excluding header/CRC).
/// </param>
public readonly record struct ModbusResponse(
    FunctionCode         Code,
    int                  Address,
    ReadOnlyMemory<byte> Payload
) {
	/// <summary>
	///     MODBUS 예외 코드. 오류 응답이 아니면 null.
	///     MODBUS exception code. null if not an error response.
	/// </summary>
	public ModbusExceptionCode? Exception =>
        // 오류 응답이고 페이로드가 있으면 예외 코드 추출
        // extract exception code if error response with payload
        Code is FunctionCode.Error && Payload.Length > 0
            ? (ModbusExceptionCode)Payload.Span[0]
            : null;
}