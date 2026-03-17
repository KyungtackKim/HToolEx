using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Process;

/// <summary>
///     실시간 토크/각도 그래프 데이터를 담는 구조체. 함수 코드 0x64/0x65/0x66으로 수신하며, 각 채널당 최대 2000개의 샘플을 포함합니다.
///     readonly struct containing real-time torque/angle graph data. received via function codes 0x64/0x65/0x66,
///     contains up to 2000 samples per channel.
/// </summary>
/// <remarks>
///     헤더(4바이트)에 채널 번호와 데이터 포인트 수를 포함하며, 이후 float 배열로 그래프 데이터를 저장합니다. Gen.2 전용.
///     header (4 bytes) contains channel number and data point count, followed by float array of graph data. Gen.2 only.
/// </remarks>
public readonly struct Graph {
	/// <summary>
	///     포맷 헤더 크기 (바이트)
	///     format header size (bytes)
	/// </summary>
	public static int Size => 4;

	/// <summary>
	///     원시 패킷 데이터에서 그래프 데이터를 파싱합니다.
	///     parses graph data from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	/// <exception cref="FormatException">데이터 길이가 일치하지 않을 때 / when data length does not match</exception>
	public Graph(ReadOnlySpan<byte> data) {
        // 최소 길이 확인
        // ensure minimum data length
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 채널 번호 읽기
        // read channel number
        Channel = BinarySpanReader.ReadUInt16(data, ref pos);
        // 데이터 포인트 수 읽기
        // read data point count
        Count = BinarySpanReader.ReadUInt16(data, ref pos);

        // 페이로드 크기 계산
        // calculate payload size
        var payload  = checked(Count * 4);
        var expected = Size + payload;
        // 데이터 길이 검증
        // ensure data length matches expected size
        if (data.Length != expected)
            // 잘못된 데이터 예외 발생
            // throw invalid data exception
            throw new FormatException($"Invalid length: got {data.Length}, expected {expected}.");

        // 데이터 값 Span 추출
        // extract data values span
        var s = data[Size..];
        // 값 배열 할당
        // allocate values array
        Values = GC.AllocateUninitializedArray<float>(Count);
        // 그래프 데이터 파싱
        // parse graph data values
        for (int i = 0, offset = 0; i < Count; i++, offset += 4)
            Values[i] = BinarySpanReader.ReadSingle(s[offset..]);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     채널 번호
	///     channel number
	/// </summary>
	public int Channel { get; }

	/// <summary>
	///     채널 데이터 포인트 수
	///     channel data point count
	/// </summary>
	public int Count { get; }

	/// <summary>
	///     그래프 데이터 값 배열
	///     graph data values array
	/// </summary>
	public float[] Values { get; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; }

	/// <summary>
	///     원시 데이터에서 그래프 데이터를 파싱합니다.
	///     attempts to parse graph data from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Graph result) {
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
            result = new Graph(data);
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