using HTool.Core.Type.Process;
using HTool.Core.Util;
using EventType = HTool.Core.Type.Process.Event;

namespace HTool.Format.Process;

/// <summary>
///     체결 분석 공통 필드 블록 (64바이트). 이벤트(0x65)·고해상도 그래프(0x66)·PRO X 고해상도가 공유한다.
///     fastening analysis common-field block (64 bytes). shared by event (0x65), high-res graph (0x66), and PRO X high-res.
/// </summary>
/// <remarks>
///     상위 프레임이 자신의 헤더를 읽은 뒤 호출하는 블록 리더이며, 마지막 16바이트는 예약 영역 14바이트 + <see cref="SyncId" /> 2바이트로 구성된다.
///     <see cref="SyncId" />는 Rev.1 펌웨어에서만 의미 있는 값을 가지며 Rev.0에서는 항상 0이다.
///     a block reader invoked by the enclosing frame after its header; the final 16 bytes are 14 reserved bytes followed by
///     a 2-byte <see cref="SyncId" />. <see cref="SyncId" /> only carries meaning on Rev.1 firmware and is always 0 on Rev.0.
/// </remarks>
public readonly struct Analysis {
    /// <summary>
    ///     분석 블록의 고정 크기 (바이트)
    ///     fixed size of the analysis block (bytes)
    /// </summary>
    public static int Size => 64;

    /// <summary>
    ///     주어진 위치에서 분석 공통 필드를 파싱하고 위치를 64바이트 전진시킨다.
    ///     parses the analysis common fields at the given position and advances it by 64 bytes.
    /// </summary>
    /// <param name="data">원시 데이터 / raw data</param>
    /// <param name="pos">현재 읽기 위치 (ref) / current read position (ref)</param>
    public Analysis(ReadOnlySpan<byte> data, ref int pos) {
        // 체결 소요 시간 파싱
        // parse fastening duration
        FastenTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 프리셋 번호 파싱
        // parse preset number
        Preset = BinarySpanReader.ReadUInt16(data, ref pos);

        // 토크 단위 원시값 읽기
        // read raw torque unit value
        var unit = BinarySpanReader.ReadUInt16(data, ref pos);
        // 정의된 범위면 단위 설정, 아니면 기본값
        // set unit when within defined range, else default
        TorqueUnit = unit <= (int)Unit.LbfFt ? (Unit)unit : default;

        // 남은 스크류 카운트 파싱
        // parse remaining screw count
        RemainScrew = BinarySpanReader.ReadUInt16(data, ref pos);

        // 회전 방향 원시값 읽기
        // read raw rotation direction value
        var dir = BinarySpanReader.ReadUInt16(data, ref pos);
        // 정의된 범위면 방향 설정, 아니면 기본값
        // set direction when within defined range, else default
        Direction = dir <= (int)Direction.Loosening ? (Direction)dir : default;

        // 에러 코드 파싱
        // parse error code
        Error = BinarySpanReader.ReadUInt16(data, ref pos);

        // 이벤트 상태 원시값 읽기
        // read raw event status value
        var status = BinarySpanReader.ReadUInt16(data, ref pos);
        // 직접(0~9) 또는 PRO X(100~109) 범위면 상태 설정, 아니면 기본값
        // set status when in direct (0~9) or PRO X (100~109) range, else default
        EventStatus = status <= (int)EventType.ScrewCountReset || status is >= 100 and <= 109
            ? (EventType)status
            : default;

        // 목표 토크 파싱
        // parse target torque
        TargetTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 실측 토크 파싱
        // parse actual torque
        Torque = BinarySpanReader.ReadSingle(data, ref pos);
        // 착좌 토크 파싱
        // parse seating torque
        SeatingTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 클램프 토크 파싱
        // parse clamp torque
        ClampTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 프리베일링 토크 파싱
        // parse prevailing torque
        PrevailingTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 스너그 토크 파싱
        // parse snug torque
        SnugTorque = BinarySpanReader.ReadSingle(data, ref pos);

        // 모터 속도 파싱
        // parse motor speed
        Speed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 각도 1 파싱 (스너그 전)
        // parse angle 1 (before snug)
        Angle1 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 각도 2 파싱 (스너그 후)
        // parse angle 2 (after snug)
        Angle2 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 총 각도 파싱
        // parse total angle
        Angle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 스너그 각도 파싱
        // parse snug angle
        SnugAngle = BinarySpanReader.ReadUInt16(data, ref pos);

        // 예약 영역 건너뛰기 (14바이트)
        // skip reserved area (14 bytes)
        pos += 14;
        // 동기 ID 파싱 (2바이트, Rev.1 전용; Rev.0에서는 항상 0)
        // parse SyncId (2 bytes, Rev.1-only; always 0 on Rev.0)
        SyncId = BinarySpanReader.ReadUInt16(data, ref pos);
    }

    /// <summary>
    ///     체결 소요 시간 (밀리초)
    ///     fastening duration (milliseconds)
    /// </summary>
    public int FastenTime { get; }

    /// <summary>
    ///     선택된 프리셋 번호 (0-31, MA=32, MB=33)
    ///     selected preset number (0-31, MA=32, MB=33)
    /// </summary>
    public int Preset { get; }

    /// <summary>
    ///     토크 단위
    ///     torque unit
    /// </summary>
    public Unit TorqueUnit { get; }

    /// <summary>
    ///     남은 스크류 카운트
    ///     remaining screw count
    /// </summary>
    public int RemainScrew { get; }

    /// <summary>
    ///     모터 회전 방향 (체결/풀림)
    ///     motor rotation direction (fastening/loosening)
    /// </summary>
    public Direction Direction { get; }

    /// <summary>
    ///     에러 코드 (0=정상)
    ///     error code (0=normal)
    /// </summary>
    public int Error { get; }

    /// <summary>
    ///     이벤트 상태 (OK/NG/에러 등)
    ///     event status (OK/NG/Error etc.)
    /// </summary>
    public EventType EventStatus { get; }

    /// <summary>
    ///     목표 토크 값
    ///     target torque value
    /// </summary>
    public float TargetTorque { get; }

    /// <summary>
    ///     실제 측정 토크 값
    ///     actual measured torque value
    /// </summary>
    public float Torque { get; }

    /// <summary>
    ///     착좌 토크 (볼트 착좌 시점의 토크)
    ///     seating torque (torque at bolt seating point)
    /// </summary>
    public float SeatingTorque { get; }

    /// <summary>
    ///     클램프 토크 (부품 밀착 시점의 토크)
    ///     clamp torque (torque at part clamping point)
    /// </summary>
    public float ClampTorque { get; }

    /// <summary>
    ///     프리베일링 토크 (나사산 마찰 토크)
    ///     prevailing torque (thread friction torque)
    /// </summary>
    public float PrevailingTorque { get; }

    /// <summary>
    ///     스너그 토크 (초기 접촉 토크)
    ///     snug torque (initial contact torque)
    /// </summary>
    public float SnugTorque { get; }

    /// <summary>
    ///     모터 회전 속도 (RPM)
    ///     motor rotation speed (RPM)
    /// </summary>
    public int Speed { get; }

    /// <summary>
    ///     각도 1 (스너그 전 각도)
    ///     angle 1 (angle before snug)
    /// </summary>
    public int Angle1 { get; }

    /// <summary>
    ///     각도 2 (스너그 후 각도)
    ///     angle 2 (angle after snug)
    /// </summary>
    public int Angle2 { get; }

    /// <summary>
    ///     총 각도 (Angle1 + Angle2)
    ///     total angle (Angle1 + Angle2)
    /// </summary>
    public int Angle { get; }

    /// <summary>
    ///     스너그 각도 (스너그 시점까지의 각도)
    ///     snug angle (angle until snug point)
    /// </summary>
    public int SnugAngle { get; }

    /// <summary>
    ///     동기 ID (Rev.1 펌웨어에서만 의미 있는 값을 가지며 Rev.0에서는 항상 0)
    ///     sync ID (carries meaning only on Rev.1 firmware; always 0 on Rev.0)
    /// </summary>
    public int SyncId { get; }
}
