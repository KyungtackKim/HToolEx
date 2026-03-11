using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Process;

/// <summary>
///     바코드 데이터를 담는 구조체. ASCII 문자열과 해시를 포함합니다.
///     readonly struct containing barcode data. holds ASCII string value and hash.
/// </summary>
/// <remarks>
///     바코드 필드는 64바이트 고정 길이이며, 널 문자로 패딩됩니다.
///     barcode field is 64-byte fixed length, padded with null characters.
/// </remarks>
public readonly struct Barcode {
	/// <summary>
	///     바코드 데이터 크기 (바이트)
	///     barcode data size (bytes)
	/// </summary>
	public static int Size => 64;

	/// <summary>
	///     원시 패킷 데이터에서 바코드 데이터를 파싱합니다.
	///     parses barcode data from raw packet data.
	/// </summary>
	/// <param name="data">원시 바코드 데이터 / raw barcode data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Barcode(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data length meets minimum requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 바코드 ASCII 문자열 파싱 (널 문자 트림)
        // parse barcode ASCII string (null-trimmed)
        Value = BinarySpanReader.ReadAsciiString(data, ref pos, Size);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Size]);
    }

	/// <summary>
	///     바코드 문자열 값
	///     barcode string value
	/// </summary>
	public string Value { get; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; }

	/// <summary>
	///     원시 데이터에서 바코드 데이터를 파싱합니다.
	///     attempts to parse barcode data from raw data.
	/// </summary>
	/// <param name="data">원시 바코드 데이터 / raw barcode data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Barcode result) {
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

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new Barcode(data);
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
}