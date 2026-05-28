using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Process;

/// <summary>
///     직접 연결 체결 이벤트(분석 데이터, FC 0x65 / reg 폴링)를 담는 구조체. 214바이트 고정(Gen.2).
///     readonly struct holding a direct-connection fastening event (analysis data, FC 0x65 / register polling). 214-byte fixed (Gen.2).
/// </summary>
/// <remarks>
///     레이아웃: 헤더(Rev 2 + Id 2 + Date 8) + <see cref="Process.Analysis" />(64) + 바코드(64) + <see cref="Process.GraphMeta" />(74).
///     커브 데이터는 포함하지 않는다(직접 그래프는 <see cref="GraphFrame" /> 0x64, 분석+커브 일괄은 <see cref="HighResGraph" /> 0x66).
///     리비전은 direct-tool 펌웨어가 결정하며 현재 단일 레이아웃이다. 타입명은 .NET <c>event</c> 키워드/델리게이트와의 혼동을 피하기 위해 <c>Frame</c> 접미사를 사용한다.
///     layout: header (Rev 2 + Id 2 + Date 8) + <see cref="Process.Analysis" />(64) + barcode(64) + <see cref="Process.GraphMeta" />(74).
///     no curve data (direct curve = <see cref="GraphFrame" /> 0x64; combined analysis+curve = <see cref="HighResGraph" /> 0x66).
///     the revision is determined by direct-tool firmware and is currently a single layout. the <c>Frame</c> suffix avoids confusion
///     with the .NET <c>event</c> keyword/delegates.
/// </remarks>
public readonly struct EventFrame : IFastenEvent {
    /// <summary>
    ///     바코드 필드 길이 (바이트)
    ///     barcode field length (bytes)
    /// </summary>
    private const int BarcodeLength = 64;

    /// <summary>
    ///     원시 데이터에서 직접 이벤트를 파싱한다. 선두 214바이트만 소비하므로 상위 프레임(0x66)에 임베드할 수 있다.
    ///     parses a direct event from raw data. consumes only the leading 214 bytes, so it can be embedded in an enclosing frame (0x66).
    /// </summary>
    /// <param name="data">원시 이벤트 데이터 / raw event data</param>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    public EventFrame(ReadOnlySpan<byte> data) {
        // 최소 크기 검증
        // validate minimum size
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw insufficient data exception
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 리비전 major 바이트 읽기
        // read revision major byte
        var major = BinarySpanReader.ReadByte(data, ref pos);
        // 리비전 minor 바이트 읽기
        // read revision minor byte
        var minor = BinarySpanReader.ReadByte(data, ref pos);
        // 리비전 원시값 구성 (major<<8 | minor)
        // compose raw revision value (major<<8 | minor)
        Revision = (major << 8) | minor;

        // 이벤트 ID 파싱
        // parse event ID
        Id = BinarySpanReader.ReadUInt16(data, ref pos);

        // 연도 파싱
        // parse year
        var year = BinarySpanReader.ReadUInt16(data, ref pos);
        // 월 파싱
        // parse month
        var month = BinarySpanReader.ReadByte(data, ref pos);
        // 일 파싱
        // parse day
        var day = BinarySpanReader.ReadByte(data, ref pos);
        // 시 파싱
        // parse hour
        var hour = BinarySpanReader.ReadByte(data, ref pos);
        // 분 파싱
        // parse minute
        var minute = BinarySpanReader.ReadByte(data, ref pos);
        // 초 파싱
        // parse second
        var second = BinarySpanReader.ReadByte(data, ref pos);
        // 밀리초 파싱
        // parse millisecond
        var millisecond = BinarySpanReader.ReadByte(data, ref pos);
        // 이벤트 발생 시각 설정
        // set event occurrence time
        Time = new DateTime(year, month, day, hour, minute, second, millisecond);

        // 분석 공통 필드 블록 파싱 (64B)
        // parse analysis common-field block (64B)
        Analysis = new Analysis(data, ref pos);

        // 바코드 파싱 (64B)
        // parse barcode (64B)
        var barcode = BinarySpanReader.ReadAsciiString(data, ref pos, BarcodeLength);
        // 단일 바코드를 ID 목록으로 노출
        // expose single barcode as the ID list
        Ids = [barcode];

        // 그래프 메타데이터 블록 파싱 (74B)
        // parse graph metadata block (74B)
        Meta = new GraphMeta(data, ref pos);

        // 본문 214바이트 해시 계산 (변경 감지용)
        // compute hash over the 214-byte body (for change detection)
        Hash = DataHash.Compute(data[..Size]);

        // 내부 정합성 검증 — 정확히 214바이트를 소비해야 함
        // internal consistency check — must consume exactly 214 bytes
        if (pos != Size)
            // 파싱 불일치 예외 발생
            // throw parsing mismatch exception
            throw new FormatException($"EventFrame parsing consumed {pos} bytes, expected {Size}.");
    }

    /// <summary>
    ///     직접 이벤트의 고정 크기 (바이트, Gen.2)
    ///     fixed size of the direct event (bytes, Gen.2)
    /// </summary>
    public static int Size => 214;

    /// <summary>
    ///     리비전 표시 문자열 ("major.minor")
    ///     revision display string ("major.minor")
    /// </summary>
    public string RevisionText => $"{Revision >> 8}.{Revision & 0xFF}";

    /// <summary>
    ///     이벤트 발생 날짜 (<see cref="Time" />와 동일)
    ///     event occurrence date (same as <see cref="Time" />)
    /// </summary>
    public DateTime Date => Time;

    /// <summary>
    ///     바코드 (직접 이벤트의 단일 바코드, <see cref="Ids" />[0])
    ///     barcode (single barcode of the direct event, <see cref="Ids" />[0])
    /// </summary>
    public string Barcode => Ids.Count > 0 ? Ids[0] : string.Empty;

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    [Browsable(false)]
    public ulong Hash { get; }

    /// <inheritdoc />
    public uint Id { get; }

    /// <inheritdoc />
    public int Revision { get; }

    /// <inheritdoc />
    public DateTime Time { get; }

    /// <inheritdoc />
    public GraphSource Source => GraphSource.Direct;

    /// <inheritdoc />
    public Analysis Analysis { get; }

    /// <inheritdoc />
    public GraphMeta Meta { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Ids { get; }

    /// <summary>
    ///     원시 데이터에서 직접 이벤트 파싱을 시도한다. 예외 없이 성공/실패를 반환한다.
    ///     attempts to parse a direct event from raw data. returns success/failure without throwing.
    /// </summary>
    /// <param name="data">원시 이벤트 데이터 / raw event data</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, out EventFrame result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length < Size) {
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
            result = new EventFrame(data);
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
