using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro;

/// <summary>
///     Pro X 스텝/작업의 NG 원인 코멘트 (1280바이트, 10개 × 128바이트). 직렬화/역직렬화를 모두 지원합니다.
///     Pro X step/job NG cause comments (1280 bytes, 10 entries x 128 bytes). supports both serialization and
///     deserialization.
/// </summary>
public sealed class NgCause {
    /// <summary>
    ///     코멘트 필드 1개의 바이트 크기
    ///     byte size of a single comment field
    /// </summary>
    private const int CommentFieldSize = 128;

    // 코멘트 내부 저장소
    // internal comment storage
    private readonly string[] _comments;

    /// <summary>
    ///     빈 NG 원인 인스턴스를 생성합니다.
    ///     creates an empty NG cause instance.
    /// </summary>
    public NgCause() {
        // 빈 코멘트 배열 생성 (리비전 0 기준)
        // create empty comment array (revision 0)
        _comments = new string[CommentCount[0]];
        // 빈 문자열로 초기화
        // initialize with empty strings
        Array.Fill(_comments, string.Empty);
    }

    /// <summary>
    ///     원시 패킷 데이터에서 NG 원인을 파싱합니다.
    ///     parses NG cause from raw packet data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    public NgCause(ReadOnlySpan<byte> data, int revision = 0) {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= Sizes.Length)
            // 리비전 기본 값 설정
            // reset the revision
            revision = 0;

        // 데이터 크기 확인
        // ensure data length meets minimum requirement
        if (data.Length < Sizes[revision])
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Sizes[revision]} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 코멘트 배열 생성
        // create comment array
        var count = CommentCount[revision];
        _comments = new string[count];

        // 코멘트를 순회하며 읽기
        // iterate and read each comment
        for (var i = 0; i < count; i++)
            // 코멘트 읽기 (128바이트 ASCII)
            // read comment (128 bytes ASCII)
            _comments[i] = BinarySpanReader.ReadAsciiString(data, ref pos, CommentFieldSize);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

    /// <summary>
    ///     리비전별 코멘트 수 배열
    ///     comment count array per revision
    /// </summary>
    public static ReadOnlySpan<int> CommentCount => [10];

    /// <summary>
    ///     리비전별 데이터 크기 배열 (바이트)
    ///     data size array per revision (bytes)
    /// </summary>
    public static ReadOnlySpan<int> Sizes => [1280];

    /// <summary>
    ///     NG 원인 코멘트 목록 (읽기 전용)
    ///     NG cause comment list (read-only)
    /// </summary>
    public IReadOnlyList<string> Comments => _comments;

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    public ulong Hash { get; }

    /// <summary>
    ///     지정한 리비전의 데이터 크기를 반환합니다.
    ///     returns data size for the specified revision.
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>데이터 크기 (바이트) / data size (bytes)</returns>
    public static int SizeOf(int revision) {
        // 리비전에 해당하는 크기 반환
        // return size for revision
        return Sizes[revision];
    }

    /// <summary>
    ///     원시 데이터에서 NG 원인을 파싱합니다.
    ///     attempts to parse NG cause from raw data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, int revision, out NgCause? result) {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= Sizes.Length)
            // 리비전 기본 값 설정
            // reset the revision
            revision = 0;

        // 데이터 크기 확인
        // check data size
        if (data.Length < Sizes[revision]) {
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new NgCause(data, revision);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

    /// <summary>
    ///     현재 코멘트를 바이트 배열로 직렬화합니다.
    ///     serializes current comments to a byte array.
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
    public byte[] GetValues(int revision = 0) {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= Sizes.Length)
            // 리비전 기본 값 설정
            // reset the revision
            revision = 0;

        // 결과 바이트 배열 생성
        // create result byte array
        var values = new byte[Sizes[revision]];
        // 쓰기 위치 초기화
        // initialize write position
        var offset = 0;

        // 해당 리비전의 코멘트 수 조회
        // get comment count for the revision
        var count = CommentCount[revision];

        // 코멘트를 순회하며 직렬화
        // iterate and serialize each comment
        for (var i = 0; i < count; i++) {
            // 코멘트를 ASCII 바이트로 변환
            // convert comment to ASCII bytes
            var bytes = Encoding.ASCII.GetBytes(_comments[i]);
            // 필드 크기 이내에서 복사
            // copy within field size bounds
            var copyLen = Math.Min(bytes.Length, CommentFieldSize);
            // 바이트 복사
            // copy bytes
            Array.Copy(bytes, 0, values, offset, copyLen);
            // 쓰기 위치 전진
            // advance write position
            offset += CommentFieldSize;
        }

        // 직렬화된 바이트 배열 반환
        // return serialized byte array
        return values;
    }
}