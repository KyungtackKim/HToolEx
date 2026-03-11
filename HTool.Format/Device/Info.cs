using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Device;

/// <summary>
///     HANTAS 장치의 상세 정보 (200바이트, Gen.2 전용). 함수 코드 0x04로 읽어옵니다.
///     detailed HANTAS device information (200 bytes, Gen.2 only). read via function code 0x04.
/// </summary>
/// <remarks>
///     드라이버, 컨트롤러, 펌웨어, MAC 주소 등 상세 장치 정보를 포함합니다.
///     includes detailed device info such as driver, controller, firmware, MAC address, etc.
/// </remarks>
public readonly struct Info {
	/// <summary>
	///     정보 데이터 크기 (바이트)
	///     information data size (bytes)
	/// </summary>
	public static int Size => 200;

	/// <summary>
	///     원시 패킷 데이터에서 상세 정보를 파싱합니다.
	///     parses detailed information from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Info(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data length meets minimum requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 시스템 정보 읽기 (2바이트, 예약)
        // read system info (2 bytes, reserved)
        SystemInfo = BinarySpanReader.ReadUInt16(data, ref pos);

        // 드라이버 ID 읽기
        // read driver ID
        DriverId = BinarySpanReader.ReadUInt16(data, ref pos);
        // 드라이버 모델 번호 읽기
        // read driver model number
        DriverModelNumber = BinarySpanReader.ReadUInt16(data, ref pos);
        // 드라이버 모델명 읽기 (32바이트 ASCII)
        // read driver model name (32 bytes ASCII)
        DriverModelName = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 드라이버 시리얼 번호 읽기 (10바이트 ASCII)
        // read driver serial number (10 bytes ASCII)
        DriverSerialNumber = BinarySpanReader.ReadAsciiString(data, ref pos, 10);

        // 컨트롤러 모델 번호 읽기
        // read controller model number
        ControllerModelNumber = BinarySpanReader.ReadUInt16(data, ref pos);
        // 컨트롤러 모델명 읽기 (32바이트 ASCII)
        // read controller model name (32 bytes ASCII)
        ControllerModelName = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 컨트롤러 시리얼 번호 읽기 (10바이트 ASCII)
        // read controller serial number (10 bytes ASCII)
        ControllerSerialNumber = BinarySpanReader.ReadAsciiString(data, ref pos, 10);

        // 펌웨어 주 버전 읽기
        // read firmware version major
        FirmwareVersionMajor = BinarySpanReader.ReadUInt16(data, ref pos);
        // 펌웨어 부 버전 읽기
        // read firmware version minor
        FirmwareVersionMinor = BinarySpanReader.ReadUInt16(data, ref pos);
        // 펌웨어 패치 버전 읽기
        // read firmware version patch
        FirmwareVersionPatch = BinarySpanReader.ReadUInt16(data, ref pos);

        // 생산 일자 읽기 (YYYYMMDD)
        // read production date (YYYYMMDD)
        ProductionDate = BinarySpanReader.ReadUInt32(data, ref pos);
        // 고급 타입 읽기
        // read advance type
        AdvanceType = BinarySpanReader.ReadUInt16(data, ref pos);

        // MAC 주소 읽기 (6바이트)
        // read MAC address (6 bytes)
        MacAddress = data.Slice(pos, 6).ToArray();
        // MAC 주소 이후로 위치 이동
        // advance position past MAC address
        pos += 6;

        // 이벤트 데이터 리비전 읽기
        // read event data revision
        EventDataRevision = BinarySpanReader.ReadUInt16(data, ref pos);
        // 제조사 코드 읽기
        // read manufacturer code
        ManufacturerCode = BinarySpanReader.ReadUInt16(data, ref pos);

        // 예약 영역 건너뛰기 (86바이트)
        // skip reserved area (86 bytes)
        pos += 86;

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
        // 모든 데이터를 읽었는지 확인
        // ensure all data has been consumed
        if (pos != data.Length)
            // 미소비 데이터 예외 발생
            // throw format exception for unconsumed data
            throw new FormatException($"Not all bytes have been consumed. " +
                                      $"{data.Length - pos} byte(s) remain.");
    }

	/// <summary>
	///     원시 데이터에서 상세 정보를 파싱합니다.
	///     attempts to parse detailed information from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Info result) {
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
            result = new Info(data);
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
	///     시스템 정보 (예약됨)
	///     system info (reserved)
	/// </summary>
	[Browsable(false)]
    public ushort SystemInfo { get; init; }

	/// <summary>
	///     드라이버 ID
	///     driver ID
	/// </summary>
	public ushort DriverId { get; init; }

	/// <summary>
	///     드라이버 모델 번호
	///     driver model number
	/// </summary>
	public ushort DriverModelNumber { get; init; }

	/// <summary>
	///     드라이버 모델명
	///     driver model name
	/// </summary>
	public string DriverModelName { get; init; }

	/// <summary>
	///     드라이버 시리얼 번호
	///     driver serial number
	/// </summary>
	public string DriverSerialNumber { get; init; }

	/// <summary>
	///     컨트롤러 모델 번호
	///     controller model number
	/// </summary>
	public ushort ControllerModelNumber { get; init; }

	/// <summary>
	///     컨트롤러 모델명
	///     controller model name
	/// </summary>
	public string ControllerModelName { get; init; }

	/// <summary>
	///     컨트롤러 시리얼 번호
	///     controller serial number
	/// </summary>
	public string ControllerSerialNumber { get; init; }

	/// <summary>
	///     펌웨어 버전 주 번호
	///     firmware version major
	/// </summary>
	public ushort FirmwareVersionMajor { get; init; }

	/// <summary>
	///     펌웨어 버전 부 번호
	///     firmware version minor
	/// </summary>
	public ushort FirmwareVersionMinor { get; init; }

	/// <summary>
	///     펌웨어 버전 패치 번호
	///     firmware version patch
	/// </summary>
	public ushort FirmwareVersionPatch { get; init; }

	/// <summary>
	///     펌웨어 버전 문자열
	///     firmware version string
	/// </summary>
	public string FirmwareVersion => $"{FirmwareVersionMajor}.{FirmwareVersionMinor}.{FirmwareVersionPatch}";

	/// <summary>
	///     생산 일자 (YYYYMMDD)
	///     production date (YYYYMMDD)
	/// </summary>
	public uint ProductionDate { get; init; }

	/// <summary>
	///     고급 타입 (0=일반, 1=플러스)
	///     advance type (0=Normal, 1=Plus)
	/// </summary>
	public ushort AdvanceType { get; init; }

	/// <summary>
	///     MAC 주소 바이트 배열
	///     MAC address bytes
	/// </summary>
	public byte[] MacAddress { get; init; }

	/// <summary>
	///     MAC 주소 문자열 (xx:xx:xx:xx:xx:xx)
	///     MAC address string (xx:xx:xx:xx:xx:xx)
	/// </summary>
	public string MacAddressString => string.Join(":", MacAddress.Select(b => b.ToString("X2")));

	/// <summary>
	///     이벤트 데이터 리비전
	///     event data revision
	/// </summary>
	public ushort EventDataRevision { get; init; }

	/// <summary>
	///     제조사 코드 (소비자가 Manufacturer enum에 매핑)
	///     manufacturer code (consumer maps to Manufacturer enum)
	/// </summary>
	public ushort ManufacturerCode { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }
}