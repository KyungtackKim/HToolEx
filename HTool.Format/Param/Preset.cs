using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Param;

/// <summary>
///     체결 프리셋 설정 데이터 (60바이트). 함수 코드 0x03으로 레지스터 40002~40031 전체를 읽어 파싱한다.
///     fastening preset configuration data (60 bytes). parsed from the full register range 40002–40031 via function code
///     0x03.
/// </summary>
public readonly struct Preset {
	/// <summary>
	///     프리셋 데이터 크기 (바이트, 레지스터 40002~40031)
	///     preset data size (bytes, registers 40002–40031)
	/// </summary>
	public static int Size => 60;

	/// <summary>
	///     원시 패킷 데이터에서 프리셋 설정을 파싱합니다.
	///     parses preset configuration from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Preset(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data meets minimum size requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 체결 모드 읽기 (0=TC/AM, 1=AC/TM)
        // read fastening mode (0=TC/AM, 1=AC/TM)
        Mode = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크 / 최대 토크 읽기 (모드에 따라 의미 다름)
        // read torque / max torque (meaning differs by mode)
        Torque = BinarySpanReader.ReadSingle(data, ref pos);
        // 토크 리밋 / 최소 토크 읽기 (모드에 따라 의미 다름)
        // read torque limit / min torque (meaning differs by mode)
        TorqueLimit = BinarySpanReader.ReadSingle(data, ref pos);
        // 목표 각도 읽기
        // read target angle
        TargetAngle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 최소 각도 읽기
        // read minimum angle
        MinAngle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 최대 각도 읽기
        // read maximum angle
        MaxAngle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 스너그 토크 읽기
        // read snug torque
        SnugTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 체결 속도 읽기
        // read fastening speed
        Speed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 프리 각도 읽기 (일부 모델 전용)
        // read free angle (selected models only)
        FreeAngle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 프리 속도 읽기 (일부 모델 전용)
        // read free speed (selected models only)
        FreeSpeed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 소프트 스타트 읽기
        // read soft start
        SoftStart = BinarySpanReader.ReadUInt16(data, ref pos);
        // 착좌 포인트 읽기
        // read seating point
        SeatingPoint = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크 상승 시간 읽기
        // read torque rising time
        TorqueRisingTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 램프업 속도 한계 읽기
        // read ramp-up speed limit
        RampUpSpeedLimit = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크 보상 읽기
        // read torque compensation
        TorqueCompensation = BinarySpanReader.ReadUInt16(data, ref pos);
        // 목표 토크 오프셋 읽기 (BMT 전용)
        // read target torque offset (BMT only)
        TargetTorqueOffset = BinarySpanReader.ReadUInt16(data, ref pos);
        // 최대 펄스 카운트 읽기 (BMT 전용)
        // read maximum pulse count (BMT only)
        MaxPulseCount = BinarySpanReader.ReadUInt16(data, ref pos);
        // 스크류 타입 읽기 (0=CW, 1=CCW)
        // read screw type (0=CW, 1=CCW)
        ScrewType = BinarySpanReader.ReadUInt16(data, ref pos);
        // 소프트 스톱 읽기 (0=비활성, 1=활성)
        // read soft stop (0=disabled, 1=enabled)
        SoftStop = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (16바이트, 40024~40031)
        // skip reserved area (16 bytes, 40024–40031)
        pos += 16;

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..pos]);
    }

	/// <summary>
	///     원시 데이터에서 프리셋 설정을 파싱합니다.
	///     attempts to parse preset configuration from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Preset result) {
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
            result = new Preset(data);
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
	///     체결 모드 (0=토크 제어 TC/AM, 1=각도 제어 AC/TM)
	///     fastening mode (0=torque control TC/AM, 1=angle control AC/TM)
	/// </summary>
	public int Mode { get; init; }

	/// <summary>
	///     토크 (TC/AM 모드: 목표 토크, AC/TM 모드: 최대 토크) [unit]
	///     torque (TC/AM mode: target torque, AC/TM mode: max torque) [unit]
	/// </summary>
	public float Torque { get; init; }

	/// <summary>
	///     최대 토크 — <see cref="Torque" />의 별칭 (AC/TM 모드에서 사용)
	///     max torque — alias for <see cref="Torque" /> (used in AC/TM mode)
	/// </summary>
	public float MaxTorque => Torque;

	/// <summary>
	///     토크 한계 (TC/AM 모드: 토크 리밋, AC/TM 모드: 최소 토크) [unit]
	///     torque limit (TC/AM mode: torque limit, AC/TM mode: min torque) [unit]
	/// </summary>
	public float TorqueLimit { get; init; }

	/// <summary>
	///     최소 토크 — <see cref="TorqueLimit" />의 별칭 (AC/TM 모드에서 사용)
	///     min torque — alias for <see cref="TorqueLimit" /> (used in AC/TM mode)
	/// </summary>
	public float MinTorque => TorqueLimit;

	/// <summary>
	///     목표 각도 [degree]
	///     target angle [degree]
	/// </summary>
	public int TargetAngle { get; init; }

	/// <summary>
	///     최소 각도 [degree]
	///     minimum angle [degree]
	/// </summary>
	public int MinAngle { get; init; }

	/// <summary>
	///     최대 각도 [degree]
	///     maximum angle [degree]
	/// </summary>
	public int MaxAngle { get; init; }

	/// <summary>
	///     스너그 토크 (초기 접촉 토크) [unit]
	///     snug torque (initial contact torque) [unit]
	/// </summary>
	public float SnugTorque { get; init; }

	/// <summary>
	///     체결 속도 [rpm]
	///     fastening speed [rpm]
	/// </summary>
	public int Speed { get; init; }

	/// <summary>
	///     프리 각도 — 일부 모델 전용 [degree]
	///     free angle — selected models only [degree]
	/// </summary>
	public int FreeAngle { get; init; }

	/// <summary>
	///     프리 속도 — 일부 모델 전용 [rpm]
	///     free speed — selected models only [rpm]
	/// </summary>
	public int FreeSpeed { get; init; }

	/// <summary>
	///     소프트 스타트 [ms]
	///     soft start [ms]
	/// </summary>
	public int SoftStart { get; init; }

	/// <summary>
	///     착좌 포인트 [%]
	///     seating point [%]
	/// </summary>
	public int SeatingPoint { get; init; }

	/// <summary>
	///     토크 상승 시간 [ms]
	///     torque rising time [ms]
	/// </summary>
	public int TorqueRisingTime { get; init; }

	/// <summary>
	///     램프업 속도 한계 [rpm]
	///     ramp-up speed limit [rpm]
	/// </summary>
	public int RampUpSpeedLimit { get; init; }

	/// <summary>
	///     토크 보상 [%]
	///     torque compensation [%]
	/// </summary>
	public int TorqueCompensation { get; init; }

	/// <summary>
	///     목표 토크 오프셋 — BMT 전용 [%]
	///     target torque offset — BMT only [%]
	/// </summary>
	public int TargetTorqueOffset { get; init; }

	/// <summary>
	///     최대 펄스 카운트 — BMT 전용 (0~200)
	///     maximum pulse count — BMT only (0–200)
	/// </summary>
	public int MaxPulseCount { get; init; }

	/// <summary>
	///     스크류 타입 (0=CW, 1=CCW)
	///     screw type (0=CW, 1=CCW)
	/// </summary>
	public int ScrewType { get; init; }

	/// <summary>
	///     소프트 스톱 (0=비활성, 1=활성)
	///     soft stop (0=disabled, 1=enabled)
	/// </summary>
	public int SoftStop { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }
}