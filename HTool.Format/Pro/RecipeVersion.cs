using HTool.Core.Util;

namespace HTool.Format.Pro;

/// <summary>
///     Pro X 레시피(XML) 버전 정보 (20바이트). 레시피 파일의 버전과 릴리즈 날짜를 담습니다.
///     Pro X recipe (XML) version information (20 bytes). contains recipe file version and release date.
/// </summary>
public readonly struct RecipeVersion {
	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [20];

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
	///     원시 패킷 데이터에서 레시피 버전 정보를 파싱합니다.
	///     parses recipe version information from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public RecipeVersion(ReadOnlySpan<byte> data, int revision = 0) {
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

        // 버전 문자열 읽기 (8바이트 ASCII)
        // read version string (8 bytes ASCII)
        Version = BinarySpanReader.ReadAsciiString(data, ref pos, 8);
        // 릴리즈 날짜 문자열 읽기 (12바이트 ASCII)
        // read release date string (12 bytes ASCII)
        ReleaseDate = BinarySpanReader.ReadAsciiString(data, ref pos, 12);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     원시 데이터에서 레시피 버전 정보를 파싱합니다.
	///     attempts to parse recipe version information from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out RecipeVersion result) {
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
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new RecipeVersion(data, revision);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

	/// <summary>
	///     레시피 버전 문자열
	///     recipe version string
	/// </summary>
	public string Version { get; init; }

	/// <summary>
	///     릴리즈 날짜 문자열
	///     release date string
	/// </summary>
	public string ReleaseDate { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	public ulong Hash { get; init; }
}