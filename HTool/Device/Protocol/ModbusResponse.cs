using HTool.Core.Util;
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
/// <param name="Length">
///     프레임에 명시된 길이 필드 값. 그래프 계열(FC 0x64/0x65/0x66)의 LEN(2바이트, BigEndian)을 보존한다.
///     해당 필드가 없는 FC의 경우 0. Payload의 실제 바이트 수와는 별개이므로 자체 검증에도 사용 가능하다.
///     Length field value as declared in the frame. Preserves the LEN(2 bytes, BigEndian) for graph-family
///     FCs (0x64/0x65/0x66). 0 for FCs without an explicit length field. Independent of Payload byte count,
///     so usable for self-validation.
/// </param>
/// <param name="Payload">
///     실제 데이터 (헤더/CRC/길이 필드 제외).
///     Payload data (header / CRC / length field stripped).
/// </param>
public readonly record struct ModbusResponse(
    FunctionCode         Code,
    int                  Address,
    int                  Length,
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

    /// <summary>
    ///     코덱 헤더가 제거된 원시 페이로드에서 길이 필드를 분리해 응답을 생성한다.
    ///     decodes a response from a codec-stripped raw payload, separating any length field.
    /// </summary>
    /// <remarks>
    ///     그래프 계열 FC(0x64/0x65/0x66)는 데이터 앞 2바이트가 BigEndian uint16 LEN이므로 Length로 노출하고
    ///     페이로드에서는 제거한다. 그 외 FC는 Length=0, Payload는 원본 그대로 보존한다. 상위 파서가 기대하는
    ///     data-only 페이로드 계약을 보장하기 위한 진입점이다.
    ///     graph-family FCs (0x64/0x65/0x66) carry a BigEndian uint16 LEN(2 bytes) at the head of the payload;
    ///     this surfaces it as Length and strips it from Payload. other FCs get Length=0 and pass through unchanged.
    ///     this is the entry point that satisfies the data-only payload contract expected by upper parsers.
    /// </remarks>
    /// <param name="code">함수 코드 / function code</param>
    /// <param name="address">매칭된 요청 주소 (비요청 응답은 0) / matched request address (0 for unsolicited)</param>
    /// <param name="rawPayload">코덱 헤더가 제거된 페이로드 / payload with codec header stripped</param>
    /// <returns>분리 처리된 응답 / response with length split out</returns>
    public static ModbusResponse Decode(FunctionCode code, int address, ReadOnlyMemory<byte> rawPayload) {
        // 명시적 LEN 필드를 갖는 FC만 분리 대상
        // only FCs that carry an explicit LEN field are split
        var hasLength = code is FunctionCode.GraphData
            or FunctionCode.GraphRes
            or FunctionCode.HighResGraph;
        // 분리 대상이 아니거나 LEN 필드가 잘려있으면 원본 그대로 노출
        // pass through unchanged if not a length-bearing FC or if the LEN field itself is truncated
        if (!hasLength || rawPayload.Length < 2)
            // Length는 필드 없음을 의미하는 0으로 표시
            // Length 0 signals "no length field"
            return new ModbusResponse(code, address, 0, rawPayload);
        // 앞 2바이트를 BigEndian uint16 LEN으로 해석
        // interpret the leading 2 bytes as a BigEndian uint16 LEN
        var length = ByteOrder.ReadUInt16(rawPayload.Span);
        // LEN 뒤 데이터만 페이로드로 슬라이스
        // slice off the data portion that follows LEN
        var data = rawPayload[2..];
        // 분리 결과 응답 생성
        // build the response from the split parts
        return new ModbusResponse(code, address, length, data);
    }
}