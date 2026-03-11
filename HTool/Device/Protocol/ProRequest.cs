using HTool.Core.Type.Pro;

namespace HTool.Device.Protocol;

/// <summary>
///     PRO X 요청 메시지. 큐에 적재되어 타이머 주기로 전송된다.
///     PRO X request message. Enqueued and transmitted on timer tick.
/// </summary>
internal sealed class ProRequest {
    // 현재 재시도 횟수
    // current retry count
    private int _retry;

    /// <summary>
    ///     PRO X 요청을 생성한다.
    ///     Creates a PRO X request.
    /// </summary>
    /// <param name="mid">메시지 ID / message ID</param>
    /// <param name="revision">프로토콜 리비전 / protocol revision</param>
    /// <param name="packet">완성된 프레임 바이트 / complete frame bytes</param>
    /// <param name="expectReply">
    ///     응답 대기 여부. false이면 전송 즉시 큐에서 제거.
    ///     Whether to wait for a reply. If false, dequeued immediately after send.
    /// </param>
    internal ProRequest(MessageId mid, int revision, ReadOnlyMemory<byte> packet, bool expectReply = true) {
        // 메시지 ID 설정
        // set message ID
        Mid = mid;
        // 리비전 설정
        // set revision
        Revision = revision;
        // 패킷 데이터 설정
        // set packet data
        Packet = packet;
        // 응답 대기 여부 설정
        // set whether to expect reply
        ExpectReply = expectReply;
        // 요청 키 생성
        // create request key
        Key = new RequestKey(mid, revision);
    }

    /// <summary>
    ///     메시지 ID.
    ///     Message ID.
    /// </summary>
    internal MessageId Mid { get; }

    /// <summary>
    ///     프로토콜 리비전.
    ///     Protocol revision.
    /// </summary>
    internal int Revision { get; }

    /// <summary>
    ///     완성된 프레임 패킷 데이터.
    ///     Complete frame packet data.
    /// </summary>
    internal ReadOnlyMemory<byte> Packet { get; }

    /// <summary>
    ///     응답 대기 여부. false이면 전송 후 큐에서 즉시 제거 (fire-and-forget).
    ///     Whether to wait for a reply. If false, dequeued immediately after send (fire-and-forget).
    /// </summary>
    internal bool ExpectReply { get; }

    /// <summary>
    ///     요청의 고유 키 (큐 중복 방지용).
    ///     Unique key for the request (queue deduplication).
    /// </summary>
    internal RequestKey Key { get; }

    /// <summary>
    ///     전송 활성화 여부. true이면 응답 대기 중.
    ///     Whether the request is activated (transmitted). true means waiting for response.
    /// </summary>
    internal bool Activated { get; private set; }

    /// <summary>
    ///     전송 활성화 시각.
    ///     Time when the request was activated (transmitted).
    /// </summary>
    internal DateTime ActiveTime { get; private set; }

    /// <summary>
    ///     요청을 활성화한다 (전송 시 호출).
    ///     Activates the request (called on transmission).
    /// </summary>
    internal void Activate() {
        // 활성화 상태로 전환
        // set activated state
        Activated = true;
        // 활성화 시각 기록
        // record activation time
        ActiveTime = DateTime.UtcNow;
    }

    /// <summary>
    ///     요청을 비활성화하고 재시도 횟수를 증가시킨다 (타임아웃 시 호출).
    ///     Deactivates the request and increments retry count (called on timeout).
    /// </summary>
    /// <returns>증가된 재시도 횟수 / incremented retry count</returns>
    internal int Deactivate() {
        // 비활성화 상태로 전환
        // set deactivated state
        Activated = false;
        // 재시도 횟수 증가 후 반환
        // increment and return retry count
        return ++_retry;
    }

    /// <summary>
    ///     요청의 고유 키. 큐 내 중복 방지에 사용된다.
    ///     Unique key for the request. Used to prevent duplicates in the queue.
    /// </summary>
    /// <param name="Mid">메시지 ID / message ID</param>
    /// <param name="Revision">리비전 / revision</param>
    internal readonly record struct RequestKey(MessageId Mid, int Revision);
}