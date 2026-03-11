using HTool.Core.Util;

namespace HTool.Format.Pro;

/// <summary>
///     Pro X 게이트웨이의 시스템 정보 (리비전별 88/90/91바이트).
///     Pro X gateway system information (88/90/91 bytes per revision).
/// </summary>
/// <remarks>
///     버전, 시리얼, 저장소 용량, 네트워크 설정 등 게이트웨이 시스템 상태를 포함합니다.
///     includes gateway system state such as version, serial, storage capacity, network settings, etc.
/// </remarks>
public sealed class SystemInfo {
	/// <summary>
	///     원시 패킷 데이터에서 시스템 정보를 파싱합니다.
	///     parses system information from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public SystemInfo(ReadOnlySpan<byte> data, int revision = 0) {
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

        // 소프트웨어 버전 읽기 (16바이트 ASCII)
        // read software version (16 bytes ASCII)
        Version = BinarySpanReader.ReadAsciiString(data, ref pos, 16);
        // 시리얼 번호 읽기 (16바이트 ASCII)
        // read serial number (16 bytes ASCII)
        Serial = BinarySpanReader.ReadAsciiString(data, ref pos, 16);

        // 내부 저장소 용량 읽기
        // read internal storage capacity
        InternalCapacity = BinarySpanReader.ReadUInt32(data, ref pos);
        // 내부 저장소 여유 공간 읽기
        // read internal storage free space
        InternalFree = BinarySpanReader.ReadUInt32(data, ref pos);
        // 내부 저장소 사용량 읽기
        // read internal storage used space
        InternalUsed = BinarySpanReader.ReadUInt32(data, ref pos);

        // SD 카드 용량 읽기
        // read SD card capacity
        SdCapacity = BinarySpanReader.ReadUInt32(data, ref pos);
        // SD 카드 여유 공간 읽기
        // read SD card free space
        SdFree = BinarySpanReader.ReadUInt32(data, ref pos);
        // SD 카드 사용량 읽기
        // read SD card used space
        SdUsed = BinarySpanReader.ReadUInt32(data, ref pos);

        // IP 주소 읽기 (4바이트 → 점 표기법)
        // read IP address (4 bytes -> dotted notation)
        IpAddress = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                    $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                    $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                    $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // 서브넷 마스크 읽기 (4바이트 → 점 표기법)
        // read subnet mask (4 bytes -> dotted notation)
        NetMask = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                  $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                  $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                  $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // 게이트웨이 주소 읽기 (4바이트 → 점 표기법)
        // read gateway address (4 bytes -> dotted notation)
        Gateway = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                  $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                  $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                  $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // MAC 주소 읽기 (6바이트 → 콜론 구분)
        // read MAC address (6 bytes -> colon-separated)
        Mac = $"{BinarySpanReader.ReadByte(data, ref pos):X2}"  +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}";

        // Wi-Fi IP 주소 읽기 (4바이트 → 점 표기법)
        // read Wi-Fi IP address (4 bytes -> dotted notation)
        WiFiIpAddress = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                        $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                        $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                        $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // Wi-Fi 서브넷 마스크 읽기 (4바이트 → 점 표기법)
        // read Wi-Fi subnet mask (4 bytes -> dotted notation)
        WifiNetMask = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                      $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                      $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                      $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // Wi-Fi MAC 주소 읽기 (6바이트 → 콜론 구분)
        // read Wi-Fi MAC address (6 bytes -> colon-separated)
        WifiMac = $"{BinarySpanReader.ReadByte(data, ref pos):X2}"  +
                  $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
                  $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
                  $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
                  $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
                  $":{BinarySpanReader.ReadByte(data, ref pos):X2}";

        // 리비전 1 이상 데이터 파싱
        // parse revision 1+ data
        if (revision >= 1)
            // 최신 이벤트 리비전 읽기
            // read latest event revision
            LatestEventRevision = BinarySpanReader.ReadUInt16(data, ref pos);

        // 리비전 2 이상 데이터 파싱
        // parse revision 2+ data
        if (revision >= 2)
            // 위치 제어 지원 여부 읽기
            // read position control support flag
            IsSupportPos = BinarySpanReader.ReadByte(data, ref pos) > 0;

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [88, 90, 91];

	/// <summary>
	///     소프트웨어 버전
	///     software version
	/// </summary>
	public string Version { get; }

	/// <summary>
	///     시리얼 번호
	///     serial number
	/// </summary>
	public string Serial { get; }

	/// <summary>
	///     내부 저장소 용량 (바이트)
	///     internal storage capacity (bytes)
	/// </summary>
	public uint InternalCapacity { get; }

	/// <summary>
	///     내부 저장소 여유 공간 (바이트)
	///     internal storage free space (bytes)
	/// </summary>
	public uint InternalFree { get; }

	/// <summary>
	///     내부 저장소 사용량 (바이트)
	///     internal storage used space (bytes)
	/// </summary>
	public uint InternalUsed { get; }

	/// <summary>
	///     SD 카드 용량 (바이트)
	///     SD card capacity (bytes)
	/// </summary>
	public uint SdCapacity { get; }

	/// <summary>
	///     SD 카드 여유 공간 (바이트)
	///     SD card free space (bytes)
	/// </summary>
	public uint SdFree { get; }

	/// <summary>
	///     SD 카드 사용량 (바이트)
	///     SD card used space (bytes)
	/// </summary>
	public uint SdUsed { get; }

	/// <summary>
	///     IP 주소
	///     IP address
	/// </summary>
	public string IpAddress { get; }

	/// <summary>
	///     서브넷 마스크
	///     subnet mask
	/// </summary>
	public string NetMask { get; }

	/// <summary>
	///     게이트웨이 주소
	///     gateway address
	/// </summary>
	public string Gateway { get; }

	/// <summary>
	///     MAC 주소
	///     MAC address
	/// </summary>
	public string Mac { get; }

	/// <summary>
	///     Wi-Fi IP 주소
	///     Wi-Fi IP address
	/// </summary>
	public string WiFiIpAddress { get; }

	/// <summary>
	///     Wi-Fi 서브넷 마스크
	///     Wi-Fi subnet mask
	/// </summary>
	public string WifiNetMask { get; }

	/// <summary>
	///     Wi-Fi MAC 주소
	///     Wi-Fi MAC address
	/// </summary>
	public string WifiMac { get; }

	/// <summary>
	///     최신 이벤트 리비전 번호 (Rev.1+)
	///     latest event revision number (Rev.1+)
	/// </summary>
	public ushort LatestEventRevision { get; }

	/// <summary>
	///     위치 제어 기능 지원 여부 (Rev.2+)
	///     position control feature support flag (Rev.2+)
	/// </summary>
	public bool IsSupportPos { get; }

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
	///     원시 데이터에서 시스템 정보를 파싱합니다.
	///     attempts to parse system information from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out SystemInfo? result) {
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
            result = new SystemInfo(data, revision);
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