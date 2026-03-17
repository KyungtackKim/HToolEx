using HTool.Core.Type.Pro;

namespace HTool.Device.Protocol;

/// <summary>
///     PRO X 수신 메시지를 담는 레코드 구조체.
///     Record struct containing a received PRO X message.
/// </summary>
/// <param name="Mid">메시지 ID / message ID</param>
/// <param name="Revision">프로토콜 리비전 / protocol revision</param>
/// <param name="Payload">
///     페이로드 데이터 (16바이트 헤더 제외).
///     Payload data (excluding 16-byte header).
/// </param>
public readonly record struct ProMessage(
    MessageId            Mid,
    int                  Revision,
    ReadOnlyMemory<byte> Payload
);