using System.ComponentModel;
using System.Globalization;
using HTool.Core.Util;
using HTool.Format.Process;

namespace HTool.Format.Pro;

/// <summary>
///     PRO X 가공 고해상도 그래프를 담는 구조체. direct-tool 고해상도를 PRO X가 재포장한 별개 데이터다.
///     readonly struct holding a PRO X-processed high-res graph. a re-packaged variant of the direct-tool high-res data.
/// </summary>
/// <remarks>
///     리비전은 PRO X 펌웨어가 결정하며 스트림이 아닌 MID 헤더(파라미터)로 전달된다. Rev.0/Rev.1 둘 다
///     [Tool 2 + Length 2 + Date ASCII 20 + Id 4] + <see cref="Process.Analysis" />(64) + ID쌍(1536) + (Rev.1 전용 Names 512)
///     + <see cref="Process.GraphMeta" />(74) + 채널 커브 가변. 고정 본문 크기 = Rev.0 1702, Rev.1 2214.
///     공유 분석 블록은 <see cref="Process" /> 네임스페이스의 타입을 재사용한다.
///     the revision is determined by PRO X firmware and passed via the MID header (parameter), not the stream. Both revisions:
///     [Tool 2 + Length 2 + Date ASCII 20 + Id 4] + <see cref="Process.Analysis" />(64) + IdPairs(1536) + (Rev.1-only Names 512)
///     + <see cref="Process.GraphMeta" />(74) + variable channel curves. fixed body size = 1702 (Rev.0), 2214 (Rev.1).
///     reuses the shared analysis blocks from the <see cref="Process" /> namespace.
/// </remarks>
public readonly struct ProHighResGraph : IHighResGraph {
    /// <summary>
    ///     PRO X ID 필드 길이 (바이트, name/value 공통)
    ///     PRO X ID field length (bytes, common to name/value)
    /// </summary>
    private const int IdFieldLength = 128;

    /// <summary>
    ///     PRO X ID 쌍 수 (ID1~ID6)
    ///     PRO X ID pair count (ID1~ID6)
    /// </summary>
    private const int IdPairCount = 6;

    /// <summary>
    ///     PRO X 시간 필드 길이 (바이트, ASCII "yyyy-MM-dd HH:mm:ss")
    ///     PRO X time field length (bytes, ASCII "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    private const int ProTimeLength = 20;

    /// <summary>
    ///     PRO X Rev.1 이름 필드 길이 (바이트, JobName/StepName/ToolName/NgCause 공통)
    ///     PRO X Rev.1 name field length (bytes, common to JobName/StepName/ToolName/NgCause)
    /// </summary>
    private const int NameFieldLength = 128;

    /// <summary>
    ///     리비전별 고정 본문 크기 (바이트, 커브 제외). Rev.0=1702, Rev.1=2214.
    ///     fixed body size per revision (bytes, excluding curves). Rev.0=1702, Rev.1=2214.
    /// </summary>
    public static ReadOnlySpan<int> Sizes => [1702, 2214];

    /// <summary>
    ///     지정 리비전의 고정 본문 크기를 반환한다.
    ///     returns the fixed body size for the specified revision.
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>고정 본문 크기 (바이트) / fixed body size (bytes)</returns>
    public static int SizeOf(int revision) {
        // 지정 리비전 크기 반환
        // return size for the given revision
        return Sizes[revision];
    }

    /// <summary>
    ///     원시 데이터에서 PRO X 고해상도 그래프를 파싱한다. 리비전은 호출자가 PRO X MID 헤더에서 전달한다.
    ///     parses a PRO X high-res graph from raw data. the revision is supplied by the caller from the PRO X MID header.
    /// </summary>
    /// <param name="data">원시 데이터 / raw data</param>
    /// <param name="revision">PRO X 리비전 (0 또는 1) / PRO X revision (0 or 1)</param>
    /// <exception cref="ArgumentOutOfRangeException">리비전이 범위 밖일 때 / when revision is out of range</exception>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    public ProHighResGraph(ReadOnlySpan<byte> data, int revision) {
        // 리비전 범위 검증 (호출자 실수)
        // validate revision range (caller error)
        if (revision is < 0 || revision >= Sizes.Length)
            // 리비전 범위 초과 예외 발생
            // throw revision out-of-range exception
            throw new ArgumentOutOfRangeException(nameof(revision),
                $"Revision {revision} is out of range. Max: {Sizes.Length - 1}");
        // 고정 본문 크기 검증
        // validate fixed body size
        if (data.Length < Sizes[revision])
            // 데이터 부족 예외 발생
            // throw insufficient data exception
            throw new FormatException(
                $"Data length {data.Length} is less than required {Sizes[revision]} bytes for revision {revision}.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 툴 인덱스 파싱 (signed, -1 = JOB 이벤트)
        // parse tool index (signed, -1 = JOB event)
        Tool = BinarySpanReader.ReadInt16(data, ref pos);
        // 애플리케이션 길이 파싱 (PRO X가 부여, 정보성)
        // parse app-level length (PRO X-assigned, informational)
        Length = BinarySpanReader.ReadUInt16(data, ref pos);
        // 리비전은 호출자 인자에서 가져옴 (스트림 아님)
        // revision comes from the caller argument (not from the stream)
        Revision = revision;

        // ASCII 시간 문자열 파싱
        // parse ASCII time string
        var timeText = BinarySpanReader.ReadAsciiString(data, ref pos, ProTimeLength);
        // 시간 문자열을 DateTime으로 변환
        // convert the time string to DateTime
        Time = DateTime.TryParseExact(timeText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt)
            ? dt
            : default;

        // 이벤트 ID 파싱 (uint32)
        // parse event ID (uint32)
        Id = BinarySpanReader.ReadUInt32(data, ref pos);

        // 분석 공통 필드 블록 파싱 (64B)
        // parse the analysis common-field block (64B)
        Analysis = new Analysis(data, ref pos);

        // ID 이름/값 쌍 배열 할당
        // allocate ID name/value pair arrays
        var idNames = new string[IdPairCount];
        var idValues = new string[IdPairCount];
        // ID 쌍 순회 (6쌍)
        // iterate ID pairs (6 pairs)
        for (var i = 0; i < IdPairCount; i++) {
            // ID 이름 파싱
            // parse ID name
            idNames[i] = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
            // ID 값 파싱
            // parse ID value
            idValues[i] = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
        }

        // ID 이름 노출
        // expose ID names
        IdNames = idNames;
        // ID 값 노출 (인터페이스 Ids)
        // expose ID values (interface Ids)
        Ids = idValues;

        // Rev.1 전용 이름 초기값 (Rev.0에서는 빈 문자열)
        // Rev.1-only names default (empty strings for Rev.0)
        var jobName = string.Empty;
        var stepName = string.Empty;
        var toolName = string.Empty;
        var ngCause = string.Empty;
        // Rev.1 이상일 때 이름 4종 파싱
        // parse the 4 name fields when Rev.1 or higher
        if (revision >= 1) {
            // 작업 이름 파싱
            // parse job name
            jobName = BinarySpanReader.ReadAsciiString(data, ref pos, NameFieldLength);
            // 스텝 이름 파싱
            // parse step name
            stepName = BinarySpanReader.ReadAsciiString(data, ref pos, NameFieldLength);
            // 툴 이름 파싱
            // parse tool name
            toolName = BinarySpanReader.ReadAsciiString(data, ref pos, NameFieldLength);
            // NG 사유 파싱
            // parse NG cause
            ngCause = BinarySpanReader.ReadAsciiString(data, ref pos, NameFieldLength);
        }
        // Rev.1 전용 필드 저장
        // store the Rev.1-only fields
        JobName = jobName;
        StepName = stepName;
        ToolName = toolName;
        NgCause = ngCause;

        // 그래프 메타데이터 블록 파싱 (74B)
        // parse graph metadata block (74B)
        Meta = new GraphMeta(data, ref pos);

        // 채널 1 커브 샘플 수 (메타에서)
        // channel 1 curve sample count (from metadata)
        var count1 = Meta.CountOfChannel1;
        // 채널 2 커브 샘플 수 (메타에서)
        // channel 2 curve sample count (from metadata)
        var count2 = Meta.CountOfChannel2;

        // 기대 전체 크기 (고정 본문 + 커브)
        // expected total size (fixed body + curves)
        var expected = Sizes[revision] + checked((count1 + count2) * 4);
        // 전체 길이 검증
        // validate total length
        if (data.Length < expected)
            // 커브 데이터 부족 예외 발생
            // throw insufficient curve data exception
            throw new FormatException($"Data length {data.Length} is less than required {expected} bytes for curves.");

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
    ///     툴 인덱스 (0~7=일반, 8~15=I/O 툴, -1=JOB 이벤트)
    ///     tool index (0~7=normal, 8~15=I/O tool, -1=JOB event)
    /// </summary>
    public int Tool { get; }

    /// <summary>
    ///     PRO X가 부여한 애플리케이션 레벨 길이 (정보성)
    ///     PRO X-assigned application-level length (informational)
    /// </summary>
    public int Length { get; }

    /// <summary>
    ///     ID1~ID6 이름 (PRO X 전용)
    ///     ID1~ID6 names (PRO X-only)
    /// </summary>
    public IReadOnlyList<string> IdNames { get; }

    /// <summary>
    ///     작업 이름 (Rev.1 전용, Rev.0에서는 빈 문자열)
    ///     job name (Rev.1-only, empty string for Rev.0)
    /// </summary>
    public string JobName { get; }

    /// <summary>
    ///     스텝 이름 (Rev.1 전용, Rev.0에서는 빈 문자열)
    ///     step name (Rev.1-only, empty string for Rev.0)
    /// </summary>
    public string StepName { get; }

    /// <summary>
    ///     툴 이름 (Rev.1 전용, Rev.0에서는 빈 문자열)
    ///     tool name (Rev.1-only, empty string for Rev.0)
    /// </summary>
    public string ToolName { get; }

    /// <summary>
    ///     NG 사유 (Rev.1 전용, Rev.0에서는 빈 문자열)
    ///     NG cause (Rev.1-only, empty string for Rev.0)
    /// </summary>
    public string NgCause { get; }

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    [Browsable(false)]
    public ulong Hash { get; }

    /// <inheritdoc />
    public uint Id { get; }

    /// <inheritdoc />
    public DateTime Time { get; }

    /// <inheritdoc />
    public int Revision { get; }

    /// <inheritdoc />
    public GraphSource Source => GraphSource.Pro;

    /// <inheritdoc />
    public Analysis Analysis { get; }

    /// <inheritdoc />
    public GraphMeta Meta { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Ids { get; }

    /// <inheritdoc />
    [Browsable(false)]
    public float[] Channel1 { get; }

    /// <inheritdoc />
    [Browsable(false)]
    public float[] Channel2 { get; }

    /// <summary>
    ///     원시 데이터에서 PRO X 고해상도 그래프 파싱을 시도한다. 데이터 크기 부족은 false 반환, 리비전 범위 위반은 예외.
    ///     attempts to parse a PRO X high-res graph from raw data. insufficient data returns false; invalid revision throws.
    /// </summary>
    /// <param name="data">원시 데이터 / raw data</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <param name="revision">PRO X 리비전 / PRO X revision</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    /// <exception cref="ArgumentOutOfRangeException">리비전이 범위 밖일 때 (호출자 실수) / when revision is out of range (caller error)</exception>
    public static bool TryParse(ReadOnlySpan<byte> data, out ProHighResGraph result, int revision) {
        // 리비전 범위 검증 (호출자 실수는 false가 아닌 예외)
        // validate revision range (caller error is an exception, not false)
        if (revision is < 0 || revision >= Sizes.Length)
            // 리비전 범위 초과 예외 발생
            // throw revision out-of-range exception
            throw new ArgumentOutOfRangeException(nameof(revision),
                $"Revision {revision} is out of range. Max: {Sizes.Length - 1}");
        // 최소 본문 크기 확인
        // check minimum body size
        if (data.Length < Sizes[revision]) {
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
            result = new ProHighResGraph(data, revision);
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
