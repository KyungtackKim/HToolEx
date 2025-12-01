using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     실시간 토크/각도 그래프 데이터를 담는 클래스. 함수 코드 0x64/0x65/0x66으로 수신하며, 각 채널당 최대 2000개의 샘플을 포함합니다.
///     Class containing real-time torque/angle graph data. Received via function codes 0x64/0x65/0x66, contains up to 2000
///     samples per channel.
/// </summary>
/// <remarks>
///     채널0 (토크), 채널1 (각도), 속도, 타임스탬프 등의 그래프 샘플 데이터를 포함합니다. 세대별로 데이터 형식이 다릅니다.
///     Includes graph sample data for Channel0 (torque), Channel1 (angle), speed, timestamp, etc. Data format varies by
///     generation.
/// </remarks>
public sealed class FormatGraph {
    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    /// <param name="type">도구 세대 타입 / tool generation type</param>
    public FormatGraph(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        // 세대 타입 설정
        // set generation type
        Type = type;
        // 세대 타입 확인
        // check generation type
        if (type != GenerationTypes.GenRev2)
            return;
        // Span 초기화
        // initialize span
        var span = values.AsSpan();
        // 최소 길이 확인
        // check minimum length
        if (span.Length < 4)
            throw new InvalidDataException("Too short.");
        // 채널 정보 읽기
        // read channel information
        Channel = BinarySpanReader.ReadUInt16(span);
        Count   = BinarySpanReader.ReadUInt16(span[2..]);
        // 페이로드 크기 계산
        // calculate payload size
        var payload  = checked(Count * 4);
        var expected = 4 + payload;
        // 데이터 길이 검증
        // validate data length
        if (span.Length != expected)
            // 예외 발생
            // throw exception
            throw new InvalidDataException($"Invalid length: got {span.Length}, expected {expected}.");

        // 데이터 값 Span 추출
        // extract data values span
        var s = span[4..];
        // 값 배열 할당
        // allocate values array
        Values = GC.AllocateUninitializedArray<float>(Count);
        // 그래프 데이터 파싱
        // parse graph data
        for (int i = 0, offset = 0; i < Count; i++, offset += 4)
            // 값 설정
            // set value
            Values[i] = BinarySpanReader.ReadSingle(s[offset..]);

        // 체크섬 계산
        // calculate checksum
        CheckSum = Utils.CalculateCheckSumFast(span);
    }

    /// <summary>
    ///     도구 세대 타입
    ///     Tool generation type
    /// </summary>
    public GenerationTypes Type { get; private set; }

    /// <summary>
    ///     채널 번호
    ///     Channel number
    /// </summary>
    public int Channel { get; }

    /// <summary>
    ///     채널 데이터 포인트 수
    ///     Channel data point count
    /// </summary>
    public int Count { get; }

    /// <summary>
    ///     그래프 데이터 값 배열
    ///     Graph data values array
    /// </summary>
    public float[] Values { get; } = null!;

    /// <summary>
    ///     체크섬 값
    ///     Checksum value
    /// </summary>
    public int CheckSum { get; }

    /// <summary>
    ///     포맷 헤더 크기 (바이트)
    ///     Format header size (bytes)
    /// </summary>
    public static int Size => 4;
}