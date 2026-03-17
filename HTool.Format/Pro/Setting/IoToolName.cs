using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     I/O 툴 이름 설정 (리비전별 256바이트).
///     I/O tool name setting (256 bytes per revision).
/// </summary>
/// <remarks>
///     최대 8개 툴의 이름을 저장합니다. 각 이름은 32바이트 ASCII 고정 길이입니다.
///     stores names for up to 8 tools. each name is a 32-byte fixed-length ASCII string.
/// </remarks>
public sealed class IoToolName {
	/// <summary>
	///     기본 생성자
	///     default constructor
	/// </summary>
	public IoToolName() { }

	/// <summary>
	///     원시 패킷 데이터에서 I/O 툴 이름 설정을 파싱합니다.
	///     parses I/O tool name setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public IoToolName(ReadOnlySpan<byte> data, int revision = 0) {
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

        // 툴 이름 목록 초기화
        // initialize tool name list
        ToolNames = new List<string>();

        // 각 툴 이름 읽기 (32바이트 ASCII × 툴 수)
        // read each tool name (32 bytes ASCII x tool count)
        for (var i = 0; i < ToolCounts[revision]; i++)
            // 32바이트 ASCII 문자열로 읽기
            // read as 32-byte ASCII string
            ToolNames.Add(BinarySpanReader.ReadAsciiString(data, ref pos, 32));

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [256];

	/// <summary>
	///     리비전별 툴 수 배열
	///     tool count array per revision
	/// </summary>
	public static ReadOnlySpan<int> ToolCounts => [8];

	/// <summary>
	///     툴 이름 목록
	///     tool name list
	/// </summary>
	public List<string> ToolNames { get; set; } = [];

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	public ulong Hash { get; set; }

	/// <summary>
	///     지정한 리비전의 데이터 크기를 반환합니다.
	///     returns data size for the specified revision.
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>데이터 크기 (바이트) / data size (bytes)</returns>
	public static int SizeOf(int revision) {
        // 지정 리비전의 크기 반환
        // return size for given revision
        return Sizes[revision];
    }

	/// <summary>
	///     원시 데이터에서 I/O 툴 이름 설정을 파싱합니다.
	///     attempts to parse I/O tool name setting from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out IoToolName? result) {
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
            result = new IoToolName(data, revision);
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
	///     설정 값을 바이트 배열로 직렬화합니다.
	///     serializes setting values to byte array.
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] GetValues(int revision = 0) {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= ToolCounts.Length)
            revision = 0;

        // 결과 바이트 목록 생성
        // create result byte list
        var values = new List<byte>();

        // 각 툴 이름 직렬화 (32바이트 고정 길이)
        // serialize each tool name (32-byte fixed length)
        for (var i = 0; i < ToolCounts[revision]; i++) {
            // 현재 인덱스에 이름이 존재하면 사용, 없으면 빈 문자열
            // use name at current index if available, otherwise empty string
            var name = i < ToolNames.Count ? ToolNames[i] : string.Empty;
            // ASCII 바이트로 변환
            // convert to ASCII bytes
            var nameBytes = Encoding.ASCII.GetBytes(name);
            // 이름 바이트 추가
            // add name bytes
            values.AddRange(nameBytes);
            // 나머지 영역 제로 패딩 (32바이트까지)
            // zero-pad remaining area (up to 32 bytes)
            values.AddRange(new byte[32 - nameBytes.Length]);
        }

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }
}