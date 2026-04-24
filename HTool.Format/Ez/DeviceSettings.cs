using System.ComponentModel;
using HTool.Core.Type.Ez;
using HTool.Core.Type.Process;
using HTool.Core.Util;
using Direction = HTool.Core.Type.Ez.Direction;

namespace HTool.Format.Ez;

/// <summary>
///     EZTorQ-III 장치 설정 데이터 (14바이트, 0x84 RES_SETTING_DATA).
///     EZTorQ-III device settings data (14 bytes, 0x84 RES_SETTING_DATA).
/// </summary>
/// <remarks>
///     목표값 설정(5B) + 운전 설정(6B) + 펌웨어 버전(3B)으로 구성됩니다.
///     읽기는 리틀엔디안, 쓰기는 빅엔디안입니다 (원본 프로토콜 동작 유지).
///     composed of target settings (5B) + operation settings (6B) + firmware version (3B).
///     reads are little-endian, writes are big-endian (preserving original protocol behavior).
/// </remarks>
public readonly struct DeviceSettings {
	/// <summary>
	///     설정 데이터 크기 (바이트)
	///     settings data size (bytes)
	/// </summary>
	public static int Size => 14;

	/// <summary>
	///     원시 패킷 데이터에서 장치 설정을 파싱합니다.
	///     parses device settings from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 (14바이트) / raw packet data (14 bytes)</param>
	/// <exception cref="FormatException">데이터 크기가 14가 아닐 때 / when data size is not 14</exception>
	public DeviceSettings(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data size is 14 bytes
        if (data.Length != Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException(
                $"Invalid settings data size: {data.Length} bytes. Expected {Size} bytes.");

        /*      target information (0-4) / 목표값 정보 (0-4)      */

        // 목표값 활성화 읽기
        // read target enable
        TargetEnable = EnumUtil.IsDefined<TargetEnable>(data[0]) ? (TargetEnable)data[0] : TargetEnable.Disable;
        // 목표 토크 읽기 (float, LE)
        // read target torque (float, LE)
        TargetTorque = ByteOrder.ReadFloat(data[1..5], isBigEndian: false);

        /*      operation settings (5-10) / 운전 설정 (5-10)      */

        // 자동 클리어 시간 읽기
        // read auto clear time
        AutoClearTime = EnumUtil.IsDefined<AutoClearTime>(data[5]) ? (AutoClearTime)data[5] : AutoClearTime.Disable;
        // 허용 오차 읽기
        // read tolerance
        Tolerance = data[6];
        // 단위 읽기
        // read unit
        Unit = EnumUtil.IsDefined<Unit>(data[7]) ? (Unit)data[7] : Unit.KgfCm;
        // 운전 모드 읽기
        // read operation mode
        Mode = EnumUtil.IsDefined<OperationMode>(data[8]) ? (OperationMode)data[8] : OperationMode.Peak;
        // 주파수 읽기
        // read frequency
        Frequency = EnumUtil.IsDefined<Frequency>(data[9]) ? (Frequency)data[9] : Frequency.Hz100;
        // 토크 방향 읽기
        // read direction
        Direction = EnumUtil.IsDefined<Direction>(data[10]) ? (Direction)data[10] : Direction.CW;

        /*      firmware version (11-13) / 펌웨어 버전 (11-13)      */

        // 펌웨어 주 버전 읽기
        // read firmware major version
        FwMajor = data[11];
        // 펌웨어 부 버전 읽기
        // read firmware minor version
        FwMinor = data[12];
        // 펌웨어 마이크로 버전 읽기
        // read firmware micro version
        FwMicro = data[13];

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     목표값 활성화 여부
	///     target enable/disable
	/// </summary>
	public TargetEnable TargetEnable { get; init; }

	/// <summary>
	///     목표 토크 값
	///     target torque value
	/// </summary>
	public float TargetTorque { get; init; }

	/// <summary>
	///     자동 클리어 시간
	///     auto clear time
	/// </summary>
	public AutoClearTime AutoClearTime { get; init; }

	/// <summary>
	///     허용 오차 (백분율)
	///     tolerance percentage
	/// </summary>
	public byte Tolerance { get; init; }

	/// <summary>
	///     토크 단위
	///     torque unit
	/// </summary>
	public Unit Unit { get; init; }

	/// <summary>
	///     운전 모드
	///     operation mode
	/// </summary>
	public OperationMode Mode { get; init; }

	/// <summary>
	///     샘플링 주파수
	///     sampling frequency
	/// </summary>
	public Frequency Frequency { get; init; }

	/// <summary>
	///     토크 방향
	///     torque direction
	/// </summary>
	public Direction Direction { get; init; }

	/// <summary>
	///     펌웨어 주 버전
	///     firmware major version
	/// </summary>
	public byte FwMajor { get; init; }

	/// <summary>
	///     펌웨어 부 버전
	///     firmware minor version
	/// </summary>
	public byte FwMinor { get; init; }

	/// <summary>
	///     펌웨어 마이크로 버전
	///     firmware micro version
	/// </summary>
	public byte FwMicro { get; init; }

	/// <summary>
	///     펌웨어 버전 문자열 (Major.Minor.Micro)
	///     firmware version string (Major.Minor.Micro)
	/// </summary>
	public string Firmware => $"{FwMajor}.{FwMinor}.{FwMicro}";

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }

	/// <summary>
	///     장치 설정을 바이트 배열로 직렬화합니다.
	///     serializes device settings to byte array.
	/// </summary>
	/// <returns>바이트 배열 (14바이트) / byte array (14 bytes)</returns>
	public byte[] ToBytes() {
        // 배열 생성
        // create byte array
        var bytes = new byte[Size];
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = bytes.AsSpan();

        // 목표값 정보 직렬화 (0-4)
        // serialize target information (0-4)
        bytes[0] = (byte)TargetEnable;
        // 목표 토크 기록 (빅엔디안)
        // write target torque (big-endian)
        ByteOrder.WriteFloat(s[1..], TargetTorque);

        // 운전 설정 직렬화 (5-10)
        // serialize operation settings (5-10)
        bytes[5]  = (byte)AutoClearTime;
        bytes[6]  = Tolerance;
        bytes[7]  = (byte)Unit;
        bytes[8]  = (byte)Mode;
        bytes[9]  = (byte)Frequency;
        bytes[10] = (byte)Direction;

        // 펌웨어 버전 직렬화 (11-13)
        // serialize firmware version (11-13)
        bytes[11] = FwMajor;
        bytes[12] = FwMinor;
        bytes[13] = FwMicro;

        // 직렬화된 바이트 반환
        // return serialized bytes
        return bytes;
    }

	/// <summary>
	///     원시 데이터에서 장치 설정을 파싱합니다.
	///     attempts to parse device settings from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out DeviceSettings result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length != Size) {
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
            result = new DeviceSettings(data);
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