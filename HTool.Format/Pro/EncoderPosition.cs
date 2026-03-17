using HTool.Core.Util;

namespace HTool.Format.Pro;

/// <summary>
///     Pro X 인코더 위치 데이터 (32바이트). 영점 보정된 값과 미보정 값을 4채널씩 포함합니다.
///     Pro X encoder position data (32 bytes). contains zero-compensated and pure values for 4 channels.
/// </summary>
public readonly struct EncoderPosition {
	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [32];

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
	///     채널 수
	///     number of channels
	/// </summary>
	private const int ChannelCount = 4;

	/// <summary>
	///     원시 패킷 데이터에서 인코더 위치를 파싱합니다.
	///     parses encoder position from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public EncoderPosition(ReadOnlySpan<byte> data, int revision = 0) {
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

        // 영점 보정된 값 배열 생성
        // create zero-compensated values array
        var values = new int[ChannelCount];
        // 4채널 영점 보정 값 읽기
        // read zero-compensated values for 4 channels
        for (var i = 0; i < ChannelCount; i++)
            // int32 값 읽기
            // read int32 value
            values[i] = BinarySpanReader.ReadInt32(data, ref pos);
        // 영점 보정된 값 저장
        // store zero-compensated values
        Values = values;

        // 미보정 값 배열 생성
        // create pure values array
        var valuesOfPure = new uint[ChannelCount];
        // 4채널 미보정 값 읽기
        // read pure values for 4 channels
        for (var i = 0; i < ChannelCount; i++)
            // uint32 값 읽기
            // read uint32 value
            valuesOfPure[i] = BinarySpanReader.ReadUInt32(data, ref pos);
        // 미보정 값 저장
        // store pure values
        ValuesOfPure = valuesOfPure;

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     원시 데이터에서 인코더 위치를 파싱합니다.
	///     attempts to parse encoder position from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out EncoderPosition result) {
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
            result = new EncoderPosition(data, revision);
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
	///     영점 보정된 인코더 값 (4채널)
	///     zero-compensated encoder values (4 channels)
	/// </summary>
	public int[] Values { get; init; }

	/// <summary>
	///     미보정 인코더 값 (4채널)
	///     pure encoder values without zero compensation (4 channels)
	/// </summary>
	public uint[] ValuesOfPure { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	public ulong Hash { get; init; }
}