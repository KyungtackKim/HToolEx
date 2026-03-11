using System.ComponentModel;
using HTool.Core.Type.Device;
using HTool.Core.Util;

namespace HTool.Format.Device;

/// <summary>
///     HANTAS 장치의 기본 정보 (13바이트). 함수 코드 0x11로 읽어오며, 장치 세대 판별에 사용됩니다.
///     basic HANTAS device information (13 bytes). read via function code 0x11, used to determine device generation.
/// </summary>
public readonly struct SimpleInfo {
	/// <summary>
	///     정보 데이터 크기 (바이트)
	///     information data size (bytes)
	/// </summary>
	public static int Size => 13;

	/// <summary>
	///     원시 패킷 데이터에서 기본 정보를 파싱합니다.
	///     parses basic information from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public SimpleInfo(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data meets minimum size requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 장치 ID 읽기
        // read device ID
        Id = BinarySpanReader.ReadUInt16(data, ref pos);
        // 컨트롤러 모델 번호 읽기
        // read controller model number
        Controller = BinarySpanReader.ReadUInt16(data, ref pos);
        // 드라이버 모델 번호 읽기
        // read driver model number
        Driver = BinarySpanReader.ReadUInt16(data, ref pos);
        // 펌웨어 버전 읽기
        // read firmware version
        Firmware = BinarySpanReader.ReadUInt16(data, ref pos);

        // 시리얼 원시 데이터 읽기
        // read serial raw data (5 bytes)
        var s = data.Slice(pos, 5);
        // 인덱스 변화
        // update position index
        pos += 5;
        // 시리얼 번호를 역순으로 조합
        // compose serial number in reverse order
        Serial = $"{s[^1]:D2}{s[^2]:D2}{s[^3]:D2}{s[^4]:D2}{s[^5]:D2}";
        // 255255xx255255 패턴 확인
        // check for 255255xx255255 pattern
        if (Serial.Length is 14)
            // 중앙 바이트만 0 패딩 적용
            // overwrite with zero-padded center byte
            Serial = $"0000{s[^3]:D2}0000";

        // 사용 횟수 잔여 바이트 확인
        // check remaining bytes for used count
        if (pos <= data.Length - sizeof(uint))
            // 4바이트 사용 횟수 읽기
            // read 4-byte used count
            Used = BinarySpanReader.ReadUInt32(data, ref pos);

        // 시리얼 번호에서 모델 코드 추출
        // extract model code from serial number
        ModelCode = Convert.ToUInt16(Serial[4..6]);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     원시 데이터에서 기본 정보를 파싱합니다.
	///     attempts to parse basic information from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out SimpleInfo result) {
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
            result = new SimpleInfo(data);
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
	///     장치 ID
	///     device ID
	/// </summary>
	public ushort Id { get; init; }

	/// <summary>
	///     컨트롤러 모델 번호
	///     controller model number
	/// </summary>
	public ushort Controller { get; init; }

	/// <summary>
	///     드라이버 모델 번호
	///     driver model number
	/// </summary>
	public ushort Driver { get; init; }

	/// <summary>
	///     펌웨어 버전
	///     firmware version
	/// </summary>
	public ushort Firmware { get; init; }

	/// <summary>
	///     시리얼 번호
	///     serial number
	/// </summary>
	public string Serial { get; init; }

	/// <summary>
	///     사용 횟수
	///     used count
	/// </summary>
	public uint Used { get; init; }

	/// <summary>
	///     모델 코드 (소비자가 Model enum에 매핑)
	///     model code (consumer maps to Model enum)
	/// </summary>
	public ushort ModelCode { get; init; }

	/// <summary>
	///     모델 표시 이름 (제조사별 변환).
	///     model display name (manufacturer-specific conversion).
	/// </summary>
	public string ModelName => ModelNames.ToName((Model)ModelCode);

	/// <summary>
	///     펌웨어 버전 표시 문자열.
	///     firmware version display string.
	/// </summary>
	public string FirmwareText => Firmware.ToString();

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }
}