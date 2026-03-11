using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     엔코더 설정 (리비전별 128바이트).
///     encoder setting (128 bytes per revision).
/// </summary>
/// <remarks>
///     4채널 사용/허용오차, 2피더 × 2코너 위치, 제로/레스트 포지션을 포함합니다.
///     includes 4-channel use/tolerance, 2-feeder x 2-corner positions, zero/rest positions.
/// </remarks>
public sealed class Encoder {
	/// <summary>
	///     기본 생성자 — 기본값으로 초기화합니다.
	///     default constructor -- initializes with default values.
	/// </summary>
	public Encoder() {
        // 4채널 사용/허용오차 초기화 (기본: use=0, zone=200, ok=50)
        // initialize 4-channel use/tolerance (default: use=0, zone=200, ok=50)
        for (var i = 0; i < 4; i++) {
            // 채널별 기본 허용오차 추가
            // add default tolerance per channel
            UseAndTolerance.Add(i, (0, 200, 50));
            // 채널별 기본 제로 포지션 추가
            // add default zero position per channel
            ZeroPos.Add(i, 0);
            // 채널별 기본 레스트 포지션 추가
            // add default rest position per channel
            RestPos.Add(i, 0);
        }

        // 2피더 × 2코너 기본값 초기화
        // initialize 2 feeders x 2 corners with defaults
        for (var i = 0; i < 2; i++) {
            // 피더별 코너 딕셔너리 생성
            // create corner dictionary per feeder
            Feeder.Add(i, new Dictionary<int, (int use, int ch1, int ch2)>());
            // 각 피더의 2개 코너 초기화
            // initialize 2 corners per feeder
            for (var j = 0; j < 2; j++)
                // 코너별 기본값 추가
                // add default values per corner
                Feeder[i].Add(j, (0, 0, 0));
        }
    }

	/// <summary>
	///     원시 패킷 데이터에서 엔코더 설정을 파싱합니다.
	///     parses encoder setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Encoder(ReadOnlySpan<byte> data, int revision = 0) : this() {
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

        // 4채널 사용/허용오차 읽기 (채널당 12바이트: use + zone + ok)
        // read 4-channel use/tolerance (12 bytes per channel: use + zone + ok)
        for (var i = 0; i < 4; i++)
            // 사용 여부, 존, OK 값 읽기
            // read use, zone, and ok values
            UseAndTolerance[i] = (
                BinarySpanReader.ReadInt32(data, ref pos),
                BinarySpanReader.ReadInt32(data, ref pos),
                BinarySpanReader.ReadInt32(data, ref pos)
            );

        // 2피더 × 2코너 위치 읽기 (코너당 12바이트: use + ch1 + ch2)
        // read 2 feeders x 2 corners (12 bytes per corner: use + ch1 + ch2)
        for (var i = 0; i < 2; i++)
            // 각 피더의 2개 코너 읽기
            // read 2 corners per feeder
        for (var j = 0; j < 2; j++)
            // 사용 여부, 채널1, 채널2 값 읽기
            // read use, ch1, and ch2 values
            Feeder[i][j] = (
                BinarySpanReader.ReadInt32(data, ref pos),
                BinarySpanReader.ReadInt32(data, ref pos),
                BinarySpanReader.ReadInt32(data, ref pos)
            );

        // 4채널 제로 포지션 읽기 (채널당 4바이트)
        // read 4-channel zero positions (4 bytes per channel)
        for (var i = 0; i < 4; i++)
            // 제로 포지션 읽기
            // read zero position
            ZeroPos[i] = BinarySpanReader.ReadUInt32(data, ref pos);

        // 4채널 레스트 포지션 읽기 (채널당 4바이트)
        // read 4-channel rest positions (4 bytes per channel)
        for (var i = 0; i < 4; i++)
            // 레스트 포지션 읽기
            // read rest position
            RestPos[i] = BinarySpanReader.ReadInt32(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [128];

	/// <summary>
	///     채널별 사용 여부 및 허용오차 (키: 0~3)
	///     use flag and tolerance per channel (keys: 0~3)
	/// </summary>
	public Dictionary<int, (int use, int zone, int ok)> UseAndTolerance { get; set; } = new();

	/// <summary>
	///     피더 픽업 위치 (피더 0~1 × 코너 0~1)
	///     feeder pick-up position (feeder 0~1 x corner 0~1)
	/// </summary>
	public Dictionary<int, Dictionary<int, (int use, int ch1, int ch2)>> Feeder { get; set; } = new();

	/// <summary>
	///     채널별 제로 포지션 (키: 0~3)
	///     zero position per channel (keys: 0~3)
	/// </summary>
	public Dictionary<int, uint> ZeroPos { get; set; } = new();

	/// <summary>
	///     채널별 레스트 포지션 (키: 0~3)
	///     rest position per channel (keys: 0~3)
	/// </summary>
	public Dictionary<int, int> RestPos { get; set; } = new();

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
	///     원시 데이터에서 엔코더 설정을 파싱합니다.
	///     attempts to parse encoder setting from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Encoder? result) {
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
            result = new Encoder(data, revision);
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

        // 4채널 사용/허용오차 직렬화
        // serialize 4-channel use/tolerance
        for (var i = 0; i < 4; i++) {
            // 패딩 바이트 1 추가
            // add padding byte 1
            values.Add(0x00);
            // 패딩 바이트 2 추가
            // add padding byte 2
            values.Add(0x00);
            // 패딩 바이트 3 추가
            // add padding byte 3
            values.Add(0x00);
            // 사용 여부 값 추가
            // add use flag value
            values.Add((byte)UseAndTolerance[i].use);
            // 존 값 상위 바이트 추가
            // add zone value high byte
            values.Add((byte)((UseAndTolerance[i].zone >> 24) & 0xFF));
            // 존 값 2번째 바이트 추가
            // add zone value 2nd byte
            values.Add((byte)((UseAndTolerance[i].zone >> 16) & 0xFF));
            // 존 값 3번째 바이트 추가
            // add zone value 3rd byte
            values.Add((byte)((UseAndTolerance[i].zone >> 8) & 0xFF));
            // 존 값 하위 바이트 추가
            // add zone value low byte
            values.Add((byte)(UseAndTolerance[i].zone & 0xFF));
            // OK 값 상위 바이트 추가
            // add ok value high byte
            values.Add((byte)((UseAndTolerance[i].ok >> 24) & 0xFF));
            // OK 값 2번째 바이트 추가
            // add ok value 2nd byte
            values.Add((byte)((UseAndTolerance[i].ok >> 16) & 0xFF));
            // OK 값 3번째 바이트 추가
            // add ok value 3rd byte
            values.Add((byte)((UseAndTolerance[i].ok >> 8) & 0xFF));
            // OK 값 하위 바이트 추가
            // add ok value low byte
            values.Add((byte)(UseAndTolerance[i].ok & 0xFF));
        }

        // 2피더 × 2코너 직렬화
        // serialize 2 feeders x 2 corners
        for (var i = 0; i < 2; i++) {
            // 피더 항목 가져오기
            // get feeder item
            var feeder = Feeder[i];
            // 각 피더의 2개 코너 직렬화
            // serialize 2 corners per feeder
            for (var j = 0; j < 2; j++) {
                // 패딩 바이트 1 추가
                // add padding byte 1
                values.Add(0x00);
                // 패딩 바이트 2 추가
                // add padding byte 2
                values.Add(0x00);
                // 패딩 바이트 3 추가
                // add padding byte 3
                values.Add(0x00);
                // 사용 여부 값 추가
                // add use flag value
                values.Add((byte)feeder[j].use);
                // 채널1 값 상위 바이트 추가
                // add ch1 value high byte
                values.Add((byte)((feeder[j].ch1 >> 24) & 0xFF));
                // 채널1 값 2번째 바이트 추가
                // add ch1 value 2nd byte
                values.Add((byte)((feeder[j].ch1 >> 16) & 0xFF));
                // 채널1 값 3번째 바이트 추가
                // add ch1 value 3rd byte
                values.Add((byte)((feeder[j].ch1 >> 8) & 0xFF));
                // 채널1 값 하위 바이트 추가
                // add ch1 value low byte
                values.Add((byte)(feeder[j].ch1 & 0xFF));
                // 채널2 값 상위 바이트 추가
                // add ch2 value high byte
                values.Add((byte)((feeder[j].ch2 >> 24) & 0xFF));
                // 채널2 값 2번째 바이트 추가
                // add ch2 value 2nd byte
                values.Add((byte)((feeder[j].ch2 >> 16) & 0xFF));
                // 채널2 값 3번째 바이트 추가
                // add ch2 value 3rd byte
                values.Add((byte)((feeder[j].ch2 >> 8) & 0xFF));
                // 채널2 값 하위 바이트 추가
                // add ch2 value low byte
                values.Add((byte)(feeder[j].ch2 & 0xFF));
            }
        }

        // 4채널 제로 포지션 직렬화 (빅엔디안 uint32)
        // serialize 4-channel zero positions (big-endian uint32)
        for (var i = 0; i < 4; i++) {
            // 제로 포지션 상위 바이트 추가
            // add zero position high byte
            values.Add((byte)((ZeroPos[i] >> 24) & 0xFF));
            // 제로 포지션 2번째 바이트 추가
            // add zero position 2nd byte
            values.Add((byte)((ZeroPos[i] >> 16) & 0xFF));
            // 제로 포지션 3번째 바이트 추가
            // add zero position 3rd byte
            values.Add((byte)((ZeroPos[i] >> 8) & 0xFF));
            // 제로 포지션 하위 바이트 추가
            // add zero position low byte
            values.Add((byte)(ZeroPos[i] & 0xFF));
        }

        // 4채널 레스트 포지션 직렬화 (빅엔디안 int32)
        // serialize 4-channel rest positions (big-endian int32)
        for (var i = 0; i < 4; i++) {
            // 레스트 포지션 상위 바이트 추가
            // add rest position high byte
            values.Add((byte)((RestPos[i] >> 24) & 0xFF));
            // 레스트 포지션 2번째 바이트 추가
            // add rest position 2nd byte
            values.Add((byte)((RestPos[i] >> 16) & 0xFF));
            // 레스트 포지션 3번째 바이트 추가
            // add rest position 3rd byte
            values.Add((byte)((RestPos[i] >> 8) & 0xFF));
            // 레스트 포지션 하위 바이트 추가
            // add rest position low byte
            values.Add((byte)(RestPos[i] & 0xFF));
        }

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }
}