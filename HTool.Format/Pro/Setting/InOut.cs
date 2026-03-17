using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     입출력 포트 설정 (리비전별 96바이트).
///     input/output port setting (96 bytes per revision).
/// </summary>
/// <remarks>
///     16개 입력/출력 포트의 기능 타입 및 출력 지속 시간을 저장합니다.
///     stores function types and output duration for 16 input/output ports.
/// </remarks>
public sealed class InOut {
	/// <summary>
	///     기본 생성자
	///     default constructor
	/// </summary>
	public InOut() { }

	/// <summary>
	///     원시 패킷 데이터에서 입출력 설정을 파싱합니다.
	///     parses input/output setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public InOut(ReadOnlySpan<byte> data, int revision = 0) {
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

        // 입력 기능 타입 읽기 (16포트 × 2바이트)
        // read input function types (16 ports x 2 bytes each)
        for (var i = 0; i < PortCount; i++)
            // 각 입력 포트의 기능 타입 읽기
            // read function type for each input port
            FunctionForInput[i] = BinarySpanReader.ReadUInt16(data, ref pos);

        // 출력 기능 타입 읽기 (16포트 × 2바이트)
        // read output function types (16 ports x 2 bytes each)
        for (var i = 0; i < PortCount; i++)
            // 각 출력 포트의 기능 타입 읽기
            // read function type for each output port
            FunctionForOutput[i] = BinarySpanReader.ReadUInt16(data, ref pos);

        // 출력 지속 시간 읽기 (16포트 × 2바이트)
        // read output duration times (16 ports x 2 bytes each)
        for (var i = 0; i < PortCount; i++)
            // 각 출력 포트의 지속 시간 읽기
            // read duration time for each output port
            DurationForOutput[i] = BinarySpanReader.ReadUInt16(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [96];

	/// <summary>
	///     입출력 포트 수
	///     input/output port count
	/// </summary>
	public static int PortCount => 16;

	/// <summary>
	///     입력 포트 기능 타입 배열
	///     input port function type array
	/// </summary>
	public int[] FunctionForInput { get; set; } = new int[PortCount];

	/// <summary>
	///     출력 포트 기능 타입 배열
	///     output port function type array
	/// </summary>
	public int[] FunctionForOutput { get; set; } = new int[PortCount];

	/// <summary>
	///     출력 포트 지속 시간 배열
	///     output port duration time array
	/// </summary>
	public int[] DurationForOutput { get; set; } = new int[PortCount];

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
	///     원시 데이터에서 입출력 설정을 파싱합니다.
	///     attempts to parse input/output setting from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out InOut? result) {
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
            result = new InOut(data, revision);
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
	///     serializes setting values to byte array.
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] GetValues(int revision = 0) {
        // 결과 바이트 목록 생성
        // create result byte list
        var values = new List<byte>();

        // 입력 기능 타입 직렬화 (16포트 × 빅엔디안 2바이트)
        // serialize input function types (16 ports x big-endian 2 bytes)
        for (var i = 0; i < PortCount; i++) {
            // 상위 바이트 추가
            // add high byte
            values.Add((byte)((FunctionForInput[i] >> 8) & 0xFF));
            // 하위 바이트 추가
            // add low byte
            values.Add((byte)(FunctionForInput[i] & 0xFF));
        }

        // 출력 기능 타입 직렬화 (16포트 × 빅엔디안 2바이트)
        // serialize output function types (16 ports x big-endian 2 bytes)
        for (var i = 0; i < PortCount; i++) {
            // 상위 바이트 추가
            // add high byte
            values.Add((byte)((FunctionForOutput[i] >> 8) & 0xFF));
            // 하위 바이트 추가
            // add low byte
            values.Add((byte)(FunctionForOutput[i] & 0xFF));
        }

        // 출력 지속 시간 직렬화 (16포트 × 빅엔디안 2바이트)
        // serialize output duration times (16 ports x big-endian 2 bytes)
        for (var i = 0; i < PortCount; i++) {
            // 상위 바이트 추가
            // add high byte
            values.Add((byte)((DurationForOutput[i] >> 8) & 0xFF));
            // 하위 바이트 추가
            // add low byte
            values.Add((byte)(DurationForOutput[i] & 0xFF));
        }

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }
}