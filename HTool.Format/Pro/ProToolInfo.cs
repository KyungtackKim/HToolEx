using HTool.Core.Util;

namespace HTool.Format.Pro;

/// <summary>
///     Pro X 게이트웨이의 멤버/스캔 툴 정보 (스캔 47바이트, 멤버 80바이트).
///     member/scan tool information in Pro X gateway (scan 47 bytes, member 80 bytes).
/// </summary>
/// <remarks>
///     스캔 모드는 네트워크에서 발견된 툴의 기본 정보, 멤버 모드는 등록된 툴의 이름과 상태를 추가로 포함합니다.
///     scan mode contains basic info for discovered tools; member mode adds registered tool name and status.
/// </remarks>
public readonly record struct ProToolInfo {
	/// <summary>
	///     원시 패킷 데이터에서 툴 정보를 파싱합니다.
	///     parses tool information from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public ProToolInfo(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data length meets minimum scan size
        if (data.Length < ScanSize)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {ScanSize} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 툴 타입 읽기
        // read tool type
        ToolType = BinarySpanReader.ReadByte(data, ref pos);
        // 모델명 읽기 (16바이트 ASCII)
        // read model name (16 bytes ASCII)
        Model = BinarySpanReader.ReadAsciiString(data, ref pos, 16);
        // 시리얼 번호 읽기 (16바이트 ASCII)
        // read serial number (16 bytes ASCII)
        Serial = BinarySpanReader.ReadAsciiString(data, ref pos, 16);
        // 버전 읽기
        // read version
        Version = BinarySpanReader.ReadUInt16(data, ref pos);

        // IP 주소 읽기 (4바이트 → 점 표기법)
        // read IP address (4 bytes -> dotted notation)
        IpAddress = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                    $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                    $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                    $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // 포트 번호 읽기
        // read port number
        Port = BinarySpanReader.ReadUInt16(data, ref pos);
        // MAC 주소 읽기 (6바이트 → 콜론 구분)
        // read MAC address (6 bytes -> colon-separated)
        Mac = $"{BinarySpanReader.ReadByte(data, ref pos):X2}"  +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}" +
              $":{BinarySpanReader.ReadByte(data, ref pos):X2}";

        // 멤버 크기 이상이면 추가 정보 파싱
        // parse additional info if data contains member-size payload
        if (data.Length >= MemberSize) {
            // 툴 이름 읽기 (32바이트 ASCII)
            // read tool name (32 bytes ASCII)
            Name = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
            // 상태 읽기 (바이트 → bool)
            // read status (byte -> bool)
            Status = BinarySpanReader.ReadByte(data, ref pos) > 0;
        }

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     멤버 툴 정보 크기 (바이트)
	///     member tool information size (bytes)
	/// </summary>
	public static int MemberSize => 80;

	/// <summary>
	///     스캔 툴 정보 크기 (바이트)
	///     scan tool information size (bytes)
	/// </summary>
	public static int ScanSize => 47;

	/// <summary>
	///     툴 타입
	///     tool type
	/// </summary>
	public byte ToolType { get; init; }

	/// <summary>
	///     모델명
	///     model name
	/// </summary>
	public string Model { get; init; }

	/// <summary>
	///     시리얼 번호
	///     serial number
	/// </summary>
	public string Serial { get; init; }

	/// <summary>
	///     펌웨어 버전
	///     firmware version
	/// </summary>
	public ushort Version { get; init; }

	/// <summary>
	///     IP 주소
	///     IP address
	/// </summary>
	public string IpAddress { get; init; }

	/// <summary>
	///     포트 번호
	///     port number
	/// </summary>
	public ushort Port { get; init; }

	/// <summary>
	///     MAC 주소
	///     MAC address
	/// </summary>
	public string Mac { get; init; }

	/// <summary>
	///     툴 이름 (멤버 전용, 스캔 시 null)
	///     tool name (member only, null for scan)
	/// </summary>
	public string? Name { get; init; }

	/// <summary>
	///     툴 상태 (멤버 전용)
	///     tool status (member only)
	/// </summary>
	public bool Status { get; init; }

	/// <summary>
	///     멤버 툴 여부 (Name이 존재하면 멤버)
	///     whether this is a member tool (member if Name exists)
	/// </summary>
	public bool IsMember => Name is not null;

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	public ulong Hash { get; init; }

	/// <summary>
	///     원시 데이터에서 툴 정보를 파싱합니다.
	///     attempts to parse tool information from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out ProToolInfo result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length < ScanSize) {
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
            result = new ProToolInfo(data);
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