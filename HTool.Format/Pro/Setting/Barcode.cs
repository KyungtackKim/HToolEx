using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     바코드 설정 데이터 클래스 (130바이트).
///     barcode setting data class (130 bytes).
/// </summary>
/// <remarks>
///     툴 인덱스, 활성화 여부, 바코드 파일 경로 설정을 포함합니다.
///     contains tool index, enable flag, and barcode file path settings.
/// </remarks>
public sealed class Barcode {
	/// <summary>
	///     기본 생성자. 문자열 속성을 빈 문자열로 초기화합니다.
	///     default constructor. initializes string properties to empty strings.
	/// </summary>
	public Barcode() {
        // 파일 경로 초기화
        // initialize file path
        FilePath = string.Empty;
    }

	/// <summary>
	///     원시 패킷 데이터에서 바코드 설정을 파싱합니다.
	///     parses barcode setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Barcode(ReadOnlySpan<byte> data, int revision = 0) : this() {
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

        // 툴 인덱스 읽기
        // read tool index
        Tool = BinarySpanReader.ReadByte(data, ref pos);
        // 바코드 활성화 여부 읽기
        // read barcode enable flag
        Enable = BinarySpanReader.ReadByte(data, ref pos);
        // 바코드 파일 경로 읽기 (128바이트 ASCII)
        // read barcode file path (128 bytes ASCII)
        FilePath = BinarySpanReader.ReadAsciiString(data, ref pos, 128);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [130];

	/// <summary>
	///     툴 인덱스
	///     tool index
	/// </summary>
	public int Tool { get; set; }

	/// <summary>
	///     바코드 활성화 여부
	///     barcode enable flag
	/// </summary>
	public int Enable { get; set; }

	/// <summary>
	///     바코드 파일 경로
	///     barcode file path
	/// </summary>
	public string FilePath { get; set; }

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
	///     설정 값을 바이트 배열로 직렬화합니다.
	///     serializes setting values to a byte array.
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] GetValues(int revision = 0) {
        // 결과 목록 초기화
        // initialize result list
        var values = new List<byte>();

        // 파일 경로 직렬화 (128바이트 고정)
        // serialize file path (128 bytes fixed)
        var path = Encoding.ASCII.GetBytes(FilePath);

        // 툴 인덱스 추가
        // add tool index
        values.Add(Convert.ToByte(Tool));
        // 바코드 활성화 여부 추가
        // add barcode enable flag
        values.Add(Convert.ToByte(Enable));
        // 파일 경로 데이터 추가
        // add file path data
        values.AddRange(path);
        // 파일 경로 패딩 추가
        // add file path padding
        values.AddRange(new byte[128 - path.Length]);

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }

	/// <summary>
	///     원시 데이터에서 바코드 설정을 파싱합니다.
	///     attempts to parse barcode setting from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Barcode? result) {
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
            result = new Barcode(data, revision);
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
}