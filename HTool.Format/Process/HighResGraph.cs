using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Process;

/// <summary>
///     직접 연결 고해상도 그래프(FC 0x66, Event-TCP)를 담는 구조체. 분석 본문(<see cref="EventFrame" />)과 채널 커브를 함께 가진다.
///     readonly struct holding a direct-connection high-res graph (FC 0x66, Event-TCP). carries the analysis body
///     (<see cref="EventFrame" />) together with the channel curves.
/// </summary>
/// <remarks>
///     레이아웃 = 이벤트 본문(214B, <see cref="EventFrame" /> 0x65와 동일) + 채널1 커브 + 채널2 커브. 커브는 헤더가 없으며
///     샘플 수는 본문의 메타(<see cref="GraphMeta.CountOfChannel1" />/<see cref="GraphMeta.CountOfChannel2" />)에서 가져온다.
///     layout = event body (214B, identical to <see cref="EventFrame" /> 0x65) + channel-1 curve + channel-2 curve. curves
///     are
///     headerless; sample counts come from the body metadata (<see cref="GraphMeta.CountOfChannel1" />/
///     <see cref="GraphMeta.CountOfChannel2" />).
/// </remarks>
public readonly struct HighResGraph : IHighResGraph {
    /// <summary>
    ///     원시 데이터에서 고해상도 그래프를 파싱한다. 선두 214바이트를 이벤트 본문으로 파싱한 뒤 커브를 읽는다.
    ///     parses a high-res graph from raw data. parses the leading 214 bytes as the event body, then reads the curves.
    /// </summary>
    /// <param name="data">원시 고해상도 데이터 / raw high-res data</param>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    public HighResGraph(ReadOnlySpan<byte> data) {
        // 이벤트 본문 최소 크기 검증
        // validate minimum size for the event body
        if (data.Length < EventFrame.Size)
            // 데이터 부족 예외 발생
            // throw insufficient data exception
            throw new FormatException($"Data length {data.Length} is less than required {EventFrame.Size} bytes.");

        // 이벤트 본문 파싱 (선두 214바이트 소비)
        // parse the event body (consumes the leading 214 bytes)
        Event = new EventFrame(data);

        // 채널 1 커브 샘플 수 (메타에서)
        // channel 1 curve sample count (from metadata)
        var count1 = Event.Meta.CountOfChannel1;
        // 채널 2 커브 샘플 수 (메타에서)
        // channel 2 curve sample count (from metadata)
        var count2 = Event.Meta.CountOfChannel2;

        // 기대 전체 크기 계산 (본문 + 커브)
        // compute expected total size (body + curves)
        var expected = EventFrame.Size + checked((count1 + count2) * 4);
        // 전체 길이 검증
        // validate total length
        if (data.Length < expected)
            // 커브 데이터 부족 예외 발생
            // throw insufficient curve data exception
            throw new FormatException($"Data length {data.Length} is less than required {expected} bytes for curves.");

        // 본문 이후 읽기 위치 설정
        // set read position past the body
        var pos = EventFrame.Size;

        // 채널 1 커브 배열 할당
        // allocate channel 1 curve array
        Channel1 = GC.AllocateUninitializedArray<float>(count1);
        // 채널 1 커브 샘플 순회
        // iterate channel 1 curve samples
        for (var i = 0; i < count1; i++)
            // 채널 1 샘플 값 읽기
            // read channel 1 sample value
            Channel1[i] = BinarySpanReader.ReadSingle(data, ref pos);

        // 채널 2 커브 배열 할당
        // allocate channel 2 curve array
        Channel2 = GC.AllocateUninitializedArray<float>(count2);
        // 채널 2 커브 샘플 순회
        // iterate channel 2 curve samples
        for (var i = 0; i < count2; i++)
            // 채널 2 샘플 값 읽기
            // read channel 2 sample value
            Channel2[i] = BinarySpanReader.ReadSingle(data, ref pos);

        // 전체 데이터 해시 계산 (변경 감지용)
        // compute hash over the whole frame (for change detection)
        Hash = DataHash.Compute(data[..expected]);
    }

    /// <summary>
    ///     임베드된 이벤트 분석 본문 (0x65와 동일 레이아웃)
    ///     embedded event analysis body (same layout as 0x65)
    /// </summary>
    public EventFrame Event { get; }

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    [Browsable(false)]
    public ulong Hash { get; }

    /// <inheritdoc />
    [Browsable(false)]
    public float[] Channel1 { get; }

    /// <inheritdoc />
    [Browsable(false)]
    public float[] Channel2 { get; }

    /// <inheritdoc />
    public uint Id => Event.Id;

    /// <inheritdoc />
    public DateTime Time => Event.Time;

    /// <inheritdoc />
    public int Revision => Event.Revision;

    /// <inheritdoc />
    public GraphSource Source => GraphSource.Direct;

    /// <inheritdoc />
    public Analysis Analysis => Event.Analysis;

    /// <inheritdoc />
    public GraphMeta Meta => Event.Meta;

    /// <inheritdoc />
    public IReadOnlyList<string> Ids => Event.Ids;

    /// <summary>
    ///     원시 데이터에서 고해상도 그래프 파싱을 시도한다. 예외 없이 성공/실패를 반환한다.
    ///     attempts to parse a high-res graph from raw data. returns success/failure without throwing.
    /// </summary>
    /// <param name="data">원시 고해상도 데이터 / raw high-res data</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, out HighResGraph result) {
        // 데이터 크기 확인 (최소 이벤트 본문)
        // check data size (at least the event body)
        if (data.Length < EventFrame.Size) {
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: 비정상 데이터의 파싱 예외 방지
        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new HighResGraph(data);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // 크기 검증을 통과했으나 파싱에 실패한 비정상 데이터
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }
}