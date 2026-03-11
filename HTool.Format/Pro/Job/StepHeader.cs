using System.Buffers.Binary;
using System.Text;
using HTool.Core.Type.Pro;
using HTool.Core.Util;

namespace HTool.Format.Pro.Job;

/// <summary>
///     스텝 헤더 (Type 4바이트 + Name 128바이트 = 132바이트).
///     step header (Type 4 bytes + Name 128 bytes = 132 bytes).
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. MODBUS 빅엔디안이 아닙니다.
///     little-endian binary format. NOT MODBUS big-endian.
/// </remarks>
public sealed class StepHeader {
	/// <summary>
	///     스텝 이름 필드 크기 (바이트)
	///     step name field size (bytes)
	/// </summary>
	private const int NameFieldSize = 128;

	/// <summary>
	///     헤더 크기 (바이트)
	///     header size (bytes)
	/// </summary>
	public static int Size => 132;

	/// <summary>
	///     스텝 유형
	///     step type
	/// </summary>
	public JobStep Type { get; set; }

	/// <summary>
	///     스텝 이름
	///     step name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///     원시 스팬에서 헤더를 파싱합니다 (리틀엔디안).
	///     parses header from raw span (little-endian).
	/// </summary>
	/// <param name="data">원시 데이터 스팬 / raw data span</param>
	/// <returns>파싱된 헤더 / parsed header</returns>
	/// <exception cref="JobParseException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public static StepHeader Parse(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data length is sufficient
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new JobParseException($"Step header requires {Size} bytes, got {data.Length}.");

        // 헤더 인스턴스 생성
        // create header instance
        var header = new StepHeader();

        // 스텝 유형 읽기 (int32 리틀엔디안)
        // read step type (int32 little-endian)
        var typeValue = BinaryPrimitives.ReadInt32LittleEndian(data);
        // 정의된 유형인지 확인
        // check if type is defined
        if (EnumUtil.IsDefined<JobStep>(typeValue))
            // 스텝 유형 설정
            // set step type
            header.Type = (JobStep)typeValue;

        // 스텝 이름 읽기 (128바이트 ASCII)
        // read step name (128 bytes ASCII)
        header.Name = Encoding.ASCII.GetString(data.Slice(4, NameFieldSize)).TrimEnd('\0');

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
        // 결과 배열 할당
        // allocate result array
        var result = new byte[Size];

        // 스텝 유형 쓰기 (int32 리틀엔디안)
        // write step type (int32 little-endian)
        BinaryPrimitives.WriteInt32LittleEndian(result, (int)Type);

        // 스텝 이름 쓰기 (128바이트 ASCII, 널 패딩)
        // write step name (128 bytes ASCII, null-padded)
        var nameBytes = Encoding.ASCII.GetBytes(Name);
        // 이름 길이만큼 복사
        // copy up to name length
        var copyLen = Math.Min(nameBytes.Length, NameFieldSize);
        // 이름 바이트 복사
        // copy name bytes
        Array.Copy(nameBytes, 0, result, 4, copyLen);

        // 직렬화된 배열 반환
        // return serialized array
        return result;
    }
}