using System.Buffers.Binary;
using HTool.Core.Type.Pro;
using HTool.Core.Util;

namespace HTool.Format.Pro.Job;

/// <summary>
///     출력 스텝 본문. I/O 포트 상태, 출력 신호 유형, 지속 시간을 포함합니다.
///     output step body. contains I/O port states, output signal types, and duration.
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. 포트별 int32 (bool) + 포트별 신호 유형 int32 + 지속 시간 int32.
///     little-endian binary format. per-port int32 (bool) + per-port signal type int32 + duration int32.
/// </remarks>
public sealed class OutputBody : IStepBody {
	/// <summary>
	///     포트 수
	///     port count
	/// </summary>
	public const int PortCount = 16;

	/// <summary>
	///     포트별 활성화 상태
	///     per-port enable status
	/// </summary>
	public bool[] IsPort { get; } = new bool[PortCount];

	/// <summary>
	///     포트별 출력 신호 유형
	///     per-port output signal type
	/// </summary>
	public OutputSignal[] PortType { get; } = new OutputSignal[PortCount];

	/// <summary>
	///     출력 신호 지속 시간
	///     output signal duration
	/// </summary>
	public int Duration { get; set; }

	/// <summary>
	///     스텝 유형 (출력)
	///     step type (output)
	/// </summary>
	public JobStep StepType => JobStep.Output;

	/// <summary>
	///     본문을 바이트 배열로 직렬화합니다 (리틀엔디안).
	///     serializes body to a byte array (little-endian).
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] ToBytes(int revision = 0) {
        // 스트림 생성
        // create stream
        using var stream = new MemoryStream();
        // 바이너리 라이터 생성
        // create binary writer
        using var bin = new BinaryWriter(stream);

        // 포트별 활성화 상태 쓰기 (bool → int32)
        // write per-port enable status (bool -> int32)
        for (var i = 0; i < PortCount; i++)
            // 포트 값 쓰기
            // write port value
            bin.Write(Convert.ToInt32(IsPort[i]));

        // 포트별 출력 신호 유형 쓰기
        // write per-port output signal type
        for (var i = 0; i < PortCount; i++)
            // 유형 값 쓰기
            // write type value
            bin.Write((int)PortType[i]);

        // 지속 시간 쓰기
        // write duration
        bin.Write(Duration);

        // 직렬화된 배열 반환
        // return serialized array
        return stream.ToArray();
    }

	/// <summary>
	///     원시 스팬에서 출력 본문을 파싱합니다 (리틀엔디안).
	///     parses output body from raw span (little-endian).
	/// </summary>
	/// <param name="data">헤더 이후의 데이터 스팬 / data span after header</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>파싱된 출력 본문 / parsed output body</returns>
	public static OutputBody Parse(ReadOnlySpan<byte> data, int revision = 0) {
        // 본문 인스턴스 생성
        // create body instance
        var body = new OutputBody();
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 포트별 활성화 상태 읽기 (int32 리틀엔디안 → bool)
        // read per-port enable status (int32 little-endian -> bool)
        for (var i = 0; i < PortCount; i++) {
            // 포트 상태 읽기
            // read port status
            body.IsPort[i] = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
        }

        // 포트별 출력 신호 유형 읽기
        // read per-port output signal type
        for (var i = 0; i < PortCount; i++) {
            // 출력 유형 읽기
            // read output type
            var output = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // 정의된 유형인지 확인
            // check if type is defined
            if (EnumUtil.IsDefined<OutputSignal>(output))
                // 출력 신호 유형 설정
                // set output signal type
                body.PortType[i] = (OutputSignal)output;
        }

        // 지속 시간 읽기
        // read duration
        body.Duration = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);

        // 파싱된 본문 반환
        // return parsed body
        return body;
    }
}