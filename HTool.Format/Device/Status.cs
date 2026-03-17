using System.ComponentModel;
using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace HTool.Format.Device;

/// <summary>
///     HANTAS 장치의 현재 상태 (Gen.2 전용, 38바이트). 레지스터 주소 100부터 읽어옵니다.
///     current HANTAS device status (Gen.2 only, 38 bytes). read from register address 100.
/// </summary>
/// <remarks>
///     토크, 각도, 속도, 입출력 신호 등의 실시간 상태를 포함합니다.
///     includes real-time status such as torque, angle, speed, I/O signals, etc.
/// </remarks>
public readonly struct Status {
	/// <summary>
	///     상태 데이터 크기 (바이트)
	///     status data size (bytes)
	/// </summary>
	public static int Size => 38;

	/// <summary>
	///     원시 패킷 데이터에서 상태 정보를 파싱합니다.
	///     parses status information from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Status(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data length meets minimum requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 토크 값 읽기 (4바이트 float)
        // read torque value (4 bytes float)
        Torque = BinarySpanReader.ReadSingle(data, ref pos);
        // 속도 값 읽기
        // read speed value
        Speed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 전류 값 읽기 (4바이트 float)
        // read current value (4 bytes float)
        Current = BinarySpanReader.ReadSingle(data, ref pos);
        // 프리셋 번호 읽기
        // read preset number
        Preset = BinarySpanReader.ReadUInt16(data, ref pos);
        // 모델 번호 읽기
        // read model number
        Model = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크업 상태 읽기
        // read torque-up state
        TorqueUp = Convert.ToBoolean(BinarySpanReader.ReadUInt16(data, ref pos));
        // 체결 OK 상태 읽기
        // read fastening OK state
        FastenOk = Convert.ToBoolean(BinarySpanReader.ReadUInt16(data, ref pos));
        // 준비 상태 읽기
        // read ready state
        Ready = Convert.ToBoolean(BinarySpanReader.ReadUInt16(data, ref pos));
        // 동작 상태 읽기
        // read running state
        Run = Convert.ToBoolean(BinarySpanReader.ReadUInt16(data, ref pos));
        // 알람 코드 읽기
        // read alarm code
        Alarm = BinarySpanReader.ReadUInt16(data, ref pos);

        // 회전 방향 값 읽기
        // read rotation direction value
        var dirVal = BinarySpanReader.ReadUInt16(data, ref pos);
        // 정의된 방향 값인지 확인
        // check if direction value is defined
        Direction = Enum.IsDefined(typeof(Direction), (int)dirVal)
            ? (Direction)dirVal
            : Direction.Fastening;

        // 남은 나사 개수 읽기
        // read remaining screw count
        RemainScrew = BinarySpanReader.ReadUInt16(data, ref pos);

        // 입력 신호 비트 읽기
        // read input signal bits
        var input = BinarySpanReader.ReadUInt16(data, ref pos);
        // 출력 신호 비트 읽기
        // read output signal bits
        var output = BinarySpanReader.ReadUInt16(data, ref pos);
        // 입력 비트를 bool 배열로 변환
        // convert input bits to bool array
        Input = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((input >> i) & 0x1)).ToArray();
        // 출력 비트를 bool 배열로 변환
        // convert output bits to bool array
        Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((output >> i) & 0x1)).ToArray();

        // 온도 값 읽기 (4바이트 float)
        // read temperature value (4 bytes float)
        Temperature = BinarySpanReader.ReadSingle(data, ref pos);
        // 잠금 상태 읽기
        // read lock state
        IsLock = Convert.ToBoolean(BinarySpanReader.ReadUInt16(data, ref pos));

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
	///     원시 데이터에서 상태 정보를 파싱합니다.
	///     attempts to parse status information from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Status result) {
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
            result = new Status(data);
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
	///     토크 값 (N.m 또는 설정 단위)
	///     torque value (N.m or configured unit)
	/// </summary>
	public float Torque { get; init; }

	/// <summary>
	///     속도 값 (RPM)
	///     speed value (RPM)
	/// </summary>
	public ushort Speed { get; init; }

	/// <summary>
	///     전류 값 (A)
	///     current value (A)
	/// </summary>
	public float Current { get; init; }

	/// <summary>
	///     선택된 프리셋 번호 (0~31, MA=32, MB=33)
	///     selected preset number (0~31, MA=32, MB=33)
	/// </summary>
	public ushort Preset { get; init; }

	/// <summary>
	///     선택된 모델 번호 (0~15)
	///     selected model number (0~15)
	/// </summary>
	public ushort Model { get; init; }

	/// <summary>
	///     토크업 상태 (true: 토크 도달)
	///     torque-up state (true: torque reached)
	/// </summary>
	public bool TorqueUp { get; init; }

	/// <summary>
	///     체결 OK 상태 (true: 체결 성공)
	///     fastening OK state (true: fastening successful)
	/// </summary>
	public bool FastenOk { get; init; }

	/// <summary>
	///     준비 상태 (true: 동작 가능)
	///     ready state (true: ready to operate)
	/// </summary>
	public bool Ready { get; init; }

	/// <summary>
	///     동작 상태 (true: 모터 동작 중)
	///     running state (true: motor running)
	/// </summary>
	public bool Run { get; init; }

	/// <summary>
	///     알람 코드 (0: 정상)
	///     alarm code (0: normal)
	/// </summary>
	public ushort Alarm { get; init; }

	/// <summary>
	///     회전 방향 상태
	///     rotation direction state
	/// </summary>
	public Direction Direction { get; init; }

	/// <summary>
	///     남은 나사 개수
	///     remaining screw count
	/// </summary>
	public ushort RemainScrew { get; init; }

	/// <summary>
	///     입력 신호 상태 배열 (16비트)
	///     input signal state array (16 bits)
	/// </summary>
	public bool[] Input { get; init; }

	/// <summary>
	///     출력 신호 상태 배열 (16비트)
	///     output signal state array (16 bits)
	/// </summary>
	public bool[] Output { get; init; }

	/// <summary>
	///     온도 값 (deg C)
	///     temperature value (deg C)
	/// </summary>
	public float Temperature { get; init; }

	/// <summary>
	///     잠금 상태 (true: 잠금됨)
	///     lock state (true: locked)
	/// </summary>
	public bool IsLock { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }
}