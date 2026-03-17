using System.Buffers.Binary;
using System.Text;

namespace HTool.Format.Pro.Job;

/// <summary>
///     Job 파일 헤더. 시그니처, 버전, 스텝 카운트 등 메타데이터를 포함합니다.
///     job file header. contains metadata such as signature, version, step counts.
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. 기본 크기 172바이트, v0.3+/v1.0+ 에서 CountOfId 4바이트 추가.
///     little-endian binary format. base size 172 bytes, CountOfId adds 4 bytes in v0.3+/v1.0+.
/// </remarks>
public sealed class JobHeader {
	/// <summary>
	///     시그니처 크기 (바이트)
	///     signature size (bytes)
	/// </summary>
	private const int SignatureSize = 8;

	/// <summary>
	///     이름 필드 크기 (바이트)
	///     name field size (bytes)
	/// </summary>
	private const int NameFieldSize = 128;

	/// <summary>
	///     헤더 기본 크기 (CountOfId 제외, 바이트)
	///     header base size (without CountOfId, bytes)
	/// </summary>
	public static int BaseSize => 172;

	/// <summary>
	///     파일 시그니처
	///     file signature
	/// </summary>
	public string Signature { get; } = "bmc.job.";

	/// <summary>
	///     메이저 버전
	///     major version
	/// </summary>
	public int Major { get; set; } = 1;

	/// <summary>
	///     마이너 버전
	///     minor version
	/// </summary>
	public int Minor { get; set; }

	/// <summary>
	///     리비전 번호 (Major >= 1 이면 1, 아니면 0)
	///     revision number (1 if Major >= 1, otherwise 0)
	/// </summary>
	public int Revision => Major >= 1 ? 1 : 0;

	/// <summary>
	///     버전 문자열
	///     version string
	/// </summary>
	public string Version => $"{Major}.{Minor}";

	/// <summary>
	///     Job 인덱스 (1-based)
	///     job index (1-based)
	/// </summary>
	public int Index { get; set; }

	/// <summary>
	///     Job 이름
	///     job name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///     총 스텝 수
	///     total step count
	/// </summary>
	public int CountOfStep { get; set; }

	/// <summary>
	///     총 나사 수
	///     total screw count
	/// </summary>
	public int CountOfScrew { get; set; }

	/// <summary>
	///     체결 스텝 수
	///     fastening step count
	/// </summary>
	public int CountOfFasten { get; set; }

	/// <summary>
	///     입력 스텝 수
	///     input step count
	/// </summary>
	public int CountOfInput { get; set; }

	/// <summary>
	///     출력 스텝 수
	///     output step count
	/// </summary>
	public int CountOfOutput { get; set; }

	/// <summary>
	///     지연 스텝 수
	///     delay step count
	/// </summary>
	public int CountOfDelay { get; set; }

	/// <summary>
	///     메시지 스텝 수
	///     message step count
	/// </summary>
	public int CountOfMessage { get; set; }

	/// <summary>
	///     ID 스텝 수 (v0.3+/v1.0+)
	///     ID step count (v0.3+/v1.0+)
	/// </summary>
	public int CountOfId { get; set; }

	/// <summary>
	///     파일 경로 및 이름 (런타임 전용, 직렬화하지 않음)
	///     file path and name (runtime-only, not serialized)
	/// </summary>
	public string FileName { get; set; } = string.Empty;

	/// <summary>
	///     지정한 버전의 전체 헤더 크기를 반환합니다.
	///     returns total header size for the specified version.
	/// </summary>
	/// <param name="major">메이저 버전 / major version</param>
	/// <param name="minor">마이너 버전 / minor version</param>
	/// <returns>헤더 크기 (바이트) / header size (bytes)</returns>
	public static int GetLength(int major, int minor) {
        // 버전에 따른 헤더 크기 반환
        // return header size based on version
        return major >= 1 || (major is 0 && minor >= 3) ? BaseSize + 4 : BaseSize;
    }

	/// <summary>
	///     원시 데이터에서 Job 헤더를 파싱합니다 (리틀엔디안).
	///     parses job header from raw data (little-endian).
	/// </summary>
	/// <param name="data">원시 데이터 스팬 / raw data span</param>
	/// <param name="headerOnly">헤더만 파싱 (CountOfId 건너뜀) / parse header only (skip CountOfId)</param>
	/// <returns>파싱된 헤더 / parsed header</returns>
	/// <exception cref="JobParseException">데이터 길이 부족 또는 시그니처 불일치 / insufficient data length or signature mismatch</exception>
	public static JobHeader Parse(ReadOnlySpan<byte> data, bool headerOnly = false) {
        // 데이터 크기가 최소 요구치를 충족하는지 확인
        // ensure data length meets minimum requirement
        if (data.Length < BaseSize)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new JobParseException($"Job header requires at least {BaseSize} bytes, got {data.Length}.");

        // 헤더 인스턴스 생성
        // create header instance
        var header = new JobHeader();
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 시그니처 읽기 (8바이트 ASCII)
        // read signature (8 bytes ASCII)
        var sign = Encoding.ASCII.GetString(data.Slice(pos, SignatureSize)).TrimEnd('\0');
        // 위치 이동
        // advance position
        pos += SignatureSize;
        // 시그니처가 기대값과 일치하는지 확인
        // ensure signature matches expected value
        if (sign != header.Signature)
            // 잘못된 시그니처 예외 발생
            // throw format exception for invalid signature
            throw new JobParseException($"Invalid signature: '{sign}', expected '{header.Signature}'.");

        // 메이저 버전 읽기
        // read major version
        header.Major = data[pos++];
        // 마이너 버전 읽기
        // read minor version
        header.Minor = data[pos++];
        // 예약 바이트 건너뛰기 (2바이트)
        // skip reserved bytes (2 bytes)
        pos += 2;
        // Job 인덱스 읽기 (파일에는 0-based로 저장)
        // read job index (stored as 0-based in file)
        header.Index = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) + 1;
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // Job 이름 읽기 (128바이트 ASCII)
        // read job name (128 bytes ASCII)
        header.Name = Encoding.ASCII.GetString(data.Slice(pos, NameFieldSize)).TrimEnd('\0');
        // 위치 128바이트 이동
        // advance position by 128 bytes
        pos += NameFieldSize;
        // 총 스텝 수 읽기
        // read total step count
        header.CountOfStep = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 총 나사 수 읽기
        // read total screw count
        header.CountOfScrew = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 체결 스텝 수 읽기
        // read fastening step count
        header.CountOfFasten = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 입력 스텝 수 읽기
        // read input step count
        header.CountOfInput = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 출력 스텝 수 읽기
        // read output step count
        header.CountOfOutput = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 지연 스텝 수 읽기
        // read delay step count
        header.CountOfDelay = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 메시지 스텝 수 읽기
        // read message step count
        header.CountOfMessage = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;

        // CountOfId 포함 여부 확인 (v0.3+/v1.0+, headerOnly가 아닐 때)
        // check if CountOfId is included (v0.3+/v1.0+, when not headerOnly)
        if (!headerOnly && (header.Major >= 1 || (header.Major is 0 && header.Minor >= 3)))
            // ID 스텝 수 읽기
            // read ID step count
            header.CountOfId = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);

        // 파싱된 헤더 반환
        // return parsed header
        return header;
    }

	/// <summary>
	///     헤더를 바이트 배열로 직렬화합니다 (리틀엔디안).
	///     serializes header to a byte array (little-endian).
	/// </summary>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] ToBytes() {
        // 결과 목록 초기화
        // initialize result list
        var values = new List<byte>();

        // 시그니처 쓰기
        // write signature
        values.AddRange(Encoding.ASCII.GetBytes(Signature));
        // 메이저 버전 쓰기
        // write major version
        values.Add(Convert.ToByte(Major));
        // 마이너 버전 쓰기
        // write minor version
        values.Add(Convert.ToByte(Minor));
        // 예약 바이트 쓰기 (2바이트)
        // write reserved bytes (2 bytes)
        values.AddRange(new byte[2]);
        // Job 인덱스 쓰기 (0-based로 저장)
        // write job index (stored as 0-based)
        values.AddRange(BitConverter.GetBytes(Index - 1));
        // Job 이름 바이트 변환
        // convert job name to bytes
        var nameBytes = Encoding.ASCII.GetBytes(Name);
        // Job 이름 데이터 쓰기
        // write job name data
        values.AddRange(nameBytes);
        // Job 이름 패딩 쓰기
        // write job name padding
        values.AddRange(new byte[NameFieldSize - nameBytes.Length]);
        // 총 스텝 수 쓰기
        // write total step count
        values.AddRange(BitConverter.GetBytes(CountOfStep));
        // 총 나사 수 쓰기
        // write total screw count
        values.AddRange(BitConverter.GetBytes(CountOfScrew));
        // 체결 스텝 수 쓰기
        // write fastening step count
        values.AddRange(BitConverter.GetBytes(CountOfFasten));
        // 입력 스텝 수 쓰기
        // write input step count
        values.AddRange(BitConverter.GetBytes(CountOfInput));
        // 출력 스텝 수 쓰기
        // write output step count
        values.AddRange(BitConverter.GetBytes(CountOfOutput));
        // 지연 스텝 수 쓰기
        // write delay step count
        values.AddRange(BitConverter.GetBytes(CountOfDelay));
        // 메시지 스텝 수 쓰기
        // write message step count
        values.AddRange(BitConverter.GetBytes(CountOfMessage));

        // CountOfId 포함 여부 확인 (v0.3+/v1.0+)
        // check if CountOfId should be included (v0.3+/v1.0+)
        if (Major >= 1 || (Major is 0 && Minor >= 3))
            // ID 스텝 수 쓰기
            // write ID step count
            values.AddRange(BitConverter.GetBytes(CountOfId));

        // 직렬화된 배열 반환
        // return serialized array
        return values.ToArray();
    }
}