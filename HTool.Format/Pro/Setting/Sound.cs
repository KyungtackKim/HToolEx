using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     사운드 설정 데이터 클래스 (512바이트).
///     sound setting data class (512 bytes).
/// </summary>
/// <remarks>
///     OK/NG/ETC/TAP 사운드 파일 경로 설정을 포함합니다. UI에서 수정 후 GetValues로 직렬화합니다.
///     contains OK/NG/ETC/TAP sound file path settings. modified in UI and serialized via GetValues.
/// </remarks>
public sealed class Sound {
	/// <summary>
	///     기본 생성자. 문자열 속성을 빈 문자열로 초기화합니다.
	///     default constructor. initializes string properties to empty strings.
	/// </summary>
	public Sound() {
        // OK 경로 초기화
        // initialize OK path
        OkPath = string.Empty;
        // NG 경로 초기화
        // initialize NG path
        NgPath = string.Empty;
        // ETC 경로 초기화
        // initialize ETC path
        EtcPath = string.Empty;
        // TAP 경로 초기화
        // initialize TAP path
        TapPath = string.Empty;
    }

	/// <summary>
	///     원시 패킷 데이터에서 사운드 설정을 파싱합니다.
	///     parses sound setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Sound(ReadOnlySpan<byte> data, int revision = 0) : this() {
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

        // OK 사운드 경로 읽기 (128바이트 ASCII)
        // read OK sound path (128 bytes ASCII)
        OkPath = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // NG 사운드 경로 읽기 (128바이트 ASCII)
        // read NG sound path (128 bytes ASCII)
        NgPath = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ETC 사운드 경로 읽기 (128바이트 ASCII)
        // read ETC sound path (128 bytes ASCII)
        EtcPath = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // TAP 사운드 경로 읽기 (128바이트 ASCII)
        // read TAP sound path (128 bytes ASCII)
        TapPath = BinarySpanReader.ReadAsciiString(data, ref pos, 128);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [512];

	/// <summary>
	///     OK 사운드 파일 경로
	///     OK sound file path
	/// </summary>
	public string OkPath { get; set; }

	/// <summary>
	///     NG 사운드 파일 경로
	///     NG sound file path
	/// </summary>
	public string NgPath { get; set; }

	/// <summary>
	///     ETC 사운드 파일 경로
	///     ETC sound file path
	/// </summary>
	public string EtcPath { get; set; }

	/// <summary>
	///     TAP 사운드 파일 경로
	///     TAP sound file path
	/// </summary>
	public string TapPath { get; set; }

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
	///     원시 데이터에서 사운드 설정을 파싱합니다.
	///     attempts to parse sound setting from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Sound? result) {
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
            result = new Sound(data, revision);
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
	///     serializes setting values to a byte array.
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] GetValues(int revision = 0) {
        // 결과 목록 초기화
        // initialize result list
        var values = new List<byte>();

        // OK 경로 직렬화 (128바이트 고정)
        // serialize OK path (128 bytes fixed)
        var ok = Encoding.ASCII.GetBytes(OkPath);
        // NG 경로 직렬화 (128바이트 고정)
        // serialize NG path (128 bytes fixed)
        var ng = Encoding.ASCII.GetBytes(NgPath);
        // ETC 경로 직렬화 (128바이트 고정)
        // serialize ETC path (128 bytes fixed)
        var etc = Encoding.ASCII.GetBytes(EtcPath);
        // TAP 경로 직렬화 (128바이트 고정)
        // serialize TAP path (128 bytes fixed)
        var tap = Encoding.ASCII.GetBytes(TapPath);

        // OK 경로 데이터 추가
        // add OK path data
        values.AddRange(ok);
        // OK 경로 패딩 추가
        // add OK path padding
        values.AddRange(new byte[128 - ok.Length]);
        // NG 경로 데이터 추가
        // add NG path data
        values.AddRange(ng);
        // NG 경로 패딩 추가
        // add NG path padding
        values.AddRange(new byte[128 - ng.Length]);
        // ETC 경로 데이터 추가
        // add ETC path data
        values.AddRange(etc);
        // ETC 경로 패딩 추가
        // add ETC path padding
        values.AddRange(new byte[128 - etc.Length]);
        // TAP 경로 데이터 추가
        // add TAP path data
        values.AddRange(tap);
        // TAP 경로 패딩 추가
        // add TAP path padding
        values.AddRange(new byte[128 - tap.Length]);

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }
}