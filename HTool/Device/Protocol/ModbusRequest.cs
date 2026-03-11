using HTool.Type;

namespace HTool.Device.Protocol;

/// <summary>
///     MODBUS 요청 메시지. 큐에 적재되어 타이머 주기로 전송된다.
///     MODBUS request message. Enqueued and transmitted on timer tick.
/// </summary>
public sealed class ModbusRequest {
    // 현재 재시도 횟수
    // current retry count
    private int _retry;

    /// <summary>
    ///     MODBUS 요청을 생성한다.
    ///     Creates a MODBUS request.
    /// </summary>
    /// <param name="code">함수 코드 / function code</param>
    /// <param name="address">시작 주소 / start address</param>
    /// <param name="packet">완성된 프레임 바이트 / complete frame bytes</param>
    public ModbusRequest(FunctionCode code, int address, ReadOnlyMemory<byte> packet) {
        // 함수 코드 설정
        // set function code
        Code = code;
        // 시작 주소 설정
        // set start address
        Address = address;
        // 패킷 데이터 설정
        // set packet data
        Packet = packet;
        // 패킷 해시를 포함한 요청 키 생성
        // create request key including packet hash
        Key = new RequestKey(code, address, GetPacketHash(packet.Span));
    }

    /// <summary>
    ///     함수 코드.
    ///     Function code.
    /// </summary>
    public FunctionCode Code { get; }

    /// <summary>
    ///     시작 주소.
    ///     Start address.
    /// </summary>
    public int Address { get; }

    /// <summary>
    ///     완성된 프레임 패킷 데이터.
    ///     Complete frame packet data.
    /// </summary>
    public ReadOnlyMemory<byte> Packet { get; }

    /// <summary>
    ///     요청의 고유 키 (큐 중복 방지용).
    ///     Unique key for the request (queue deduplication).
    /// </summary>
    public RequestKey Key { get; }

    /// <summary>
    ///     전송 활성화 여부. true이면 응답 대기 중.
    ///     Whether the request is activated (transmitted). true means waiting for response.
    /// </summary>
    public bool Activated { get; private set; }

    /// <summary>
    ///     전송 활성화 시각.
    ///     Time when the request was activated (transmitted).
    /// </summary>
    public DateTime ActiveTime { get; private set; }

    /// <summary>
    ///     요청을 활성화한다 (전송 시 호출).
    ///     Activates the request (called on transmission).
    /// </summary>
    public void Activate() {
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
    public int Deactivate() {
        // 비활성화 상태로 전환
        // set deactivated state
        Activated = false;
        // 재시도 횟수 증가 후 반환
        // increment and return retry count
        return ++_retry;
    }

    /// <summary>
    ///     패킷 데이터의 해시를 계산한다.
    ///     Computes a hash of the packet data.
    /// </summary>
    /// <param name="data">패킷 데이터 / packet data</param>
    /// <returns>해시 값 / hash value</returns>
    private static int GetPacketHash(ReadOnlySpan<byte> data) {
        // FNV-1a 해시 초기값
        // FNV-1a hash initial value
        var hash = unchecked((int)2166136261);
        // 모든 바이트에 대해 해시 계산
        // compute hash over all bytes
        foreach (var b in data)
            // XOR 후 소수 곱셈
            // XOR then multiply by prime
            hash = unchecked((hash ^ b) * 16777619);
        // 계산된 해시 반환
        // return computed hash
        return hash;
    }

    /// <summary>
    ///     요청의 고유 키. 큐 내 중복 방지에 사용된다.
    ///     Unique key for the request. Used to prevent duplicates in the queue.
    /// </summary>
    /// <param name="Code">함수 코드 / function code</param>
    /// <param name="Address">시작 주소 / start address</param>
    /// <param name="Hash">패킷 해시 (쓰기 값 구별용) / packet hash (to distinguish write values)</param>
    public readonly record struct RequestKey(FunctionCode Code, int Address, int Hash);
}