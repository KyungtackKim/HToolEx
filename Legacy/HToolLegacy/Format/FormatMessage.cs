using HTool.Type;

namespace HTool.Format;

/// <summary>
///     MODBUS 요청 메시지를 나타내는 클래스. KeyedQueue에서 중복 방지 및 타임아웃 관리를 위해 사용됩니다.
///     Class representing MODBUS request message. Used in KeyedQueue for duplicate prevention and timeout management.
/// </summary>
/// <remarks>
///     메시지는 함수 코드와 주소로 구성된 키를 통해 중복이 식별되며, 1초 타임아웃 후 최대 1회 재시도됩니다.
///     Messages are identified as duplicates by key composed of function code and address, retried once after 1s timeout.
/// </remarks>
public sealed class FormatMessage {
    /// <summary>
    ///     기본 주소값 (주소 없음을 나타냄)
    ///     Default address value indicating no specific address
    /// </summary>
    public const int EmptyAddr = 0;

    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="code">MODBUS 함수 코드 / MODBUS function code</param>
    /// <param name="addr">레지스터 주소 / register address</param>
    /// <param name="packet">전송할 패킷 데이터 / packet data to transmit</param>
    /// <param name="retry">재시도 횟수 / retry count</param>
    /// <param name="notCheck">응답 확인 생략 여부 / skip response check flag</param>
    public FormatMessage(CodeTypes code, int addr, byte[] packet, int retry = 1, bool notCheck = false) {
        // set the information
        Code     = code;
        Address  = addr;
        Retry    = retry;
        NotCheck = notCheck;
        // set the packet
        Packet = packet;
        // get the hash
        var hash = FastHash(Packet.Span);
        // set the key
        Key = new MessageKey(code, addr, hash);
    }

    /// <summary>
    ///     MODBUS 함수 코드
    ///     MODBUS function code
    /// </summary>
    public CodeTypes Code { get; }

    /// <summary>
    ///     레지스터 시작 주소
    ///     Register start address
    /// </summary>
    public int Address { get; }

    /// <summary>
    ///     남은 재시도 횟수 (<see cref="Activate" /> 호출 시 감소)
    ///     Remaining retry count (decremented by <see cref="Activate" />)
    /// </summary>
    public int Retry { get; private set; }

    /// <summary>
    ///     응답 확인 생략 옵션
    ///     Skip response check option
    /// </summary>
    public bool NotCheck { get; }

    /// <summary>
    ///     메시지 활성화 상태
    ///     Message activation state
    /// </summary>
    public bool Activated { get; private set; }

    /// <summary>
    ///     메시지 활성화 시간
    ///     Message activation time
    /// </summary>
    public DateTime ActiveTime { get; private set; }

    /// <summary>
    ///     전송 패킷 데이터
    ///     Packet data to transmit
    /// </summary>
    public ReadOnlyMemory<byte> Packet { get; }

    /// <summary>
    ///     메시지 고유 키 (중복 검사용)
    ///     Unique message key for duplicate detection
    /// </summary>
    public MessageKey Key { get; }

    /// <summary>
    ///     메시지 활성화 (전송 시작)
    ///     Activate message (start transmission)
    /// </summary>
    public void Activate() {
        // 활성화 상태 설정
        // set activation state
        Activated  = true;
        ActiveTime = DateTime.Now;
        Retry--;
    }

    /// <summary>
    ///     메시지 비활성화 (전송 완료 또는 취소)
    ///     Deactivate message (transmission complete or cancelled)
    /// </summary>
    /// <returns>남은 재시도 횟수 / remaining retry count</returns>
    public int Deactivate() {
        // 비활성화 상태 설정
        // set deactivation state
        Activated = false;
        // 남은 재시도 횟수 반환
        // return remaining retry count
        return Retry;
    }

    /// <summary>
    ///     패킷 해시 계산 (FNV-1a 알고리즘)
    ///     Calculate packet hash using FNV-1a algorithm
    /// </summary>
    /// <param name="packet">패킷 데이터 / packet data</param>
    /// <returns>해시 값 / hash value</returns>
    private static int FastHash(ReadOnlySpan<byte> packet) {
        var hash = 2166136261u;
        // 패킷 바이트별 해시 계산
        // calculate hash for each byte
        foreach (var b in packet) {
            // XOR 및 곱셈 연산
            // XOR and multiply operations
            hash ^= b;
            hash *= 16777619;
        }

        return unchecked((int)hash);
    }

    /// <summary>
    ///     메시지 고유 키 구조체 (중복 메시지 검출용)
    ///     Compact message key struct for duplicate detection
    /// </summary>
    /// <param name="Code">MODBUS 함수 코드 / MODBUS function code</param>
    /// <param name="Address">레지스터 주소 / register address</param>
    /// <param name="Hash">패킷 해시 / packet hash</param>
    public readonly record struct MessageKey(CodeTypes Code, int Address, int Hash);
}