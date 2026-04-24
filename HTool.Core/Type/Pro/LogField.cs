using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 로그 필드 유형
///     log field type for Pro X
/// </summary>
/// <remarks>
///     Pro X 로그 설정(LogSettings)의 선택 항목을 식별하는 정의이며, 본 라이브러리 내부에서 현재 참조되지 않습니다.
///     definition identifying selectable entries of Pro X log settings (LogSettings); currently unreferenced within this library.
/// </remarks>
public enum LogField {
    #region Rev.0

    /// <summary>
    ///     작업 이름
    ///     job name
    /// </summary>
    [Description("Job name")]
    JobName,

    /// <summary>
    ///     스텝 이름
    ///     step name
    /// </summary>
    [Description("Step name")]
    StepName,

    /// <summary>
    ///     공구 이름
    ///     tool name
    /// </summary>
    [Description("Tool name")]
    ToolName,

    /// <summary>
    ///     바코드 (Rev.0 전용)
    ///     barcode (Rev.0 only)
    /// </summary>
    [Description("Barcode")]
    [Obsolete("Support Rev.0 only")]
    Barcode,

    /// <summary>
    ///     체결 시간
    ///     fasten time
    /// </summary>
    [Description("Fasten time")]
    FastenTime,

    /// <summary>
    ///     프리셋 번호
    ///     preset no.
    /// </summary>
    [Description("Preset no.")]
    PresetNo,

    /// <summary>
    ///     토크 단위
    ///     torque unit
    /// </summary>
    [Description("Torque unit")]
    TorqueUnit,

    /// <summary>
    ///     남은 나사 수
    ///     remain screw
    /// </summary>
    [Description("Remain screw")]
    RemainScrew,

    /// <summary>
    ///     체결 방향
    ///     direction
    /// </summary>
    [Description("Direction")]
    Direction,

    /// <summary>
    ///     에러 코드
    ///     error
    /// </summary>
    [Description("Error")]
    Error,

    /// <summary>
    ///     체결 상태
    ///     status
    /// </summary>
    [Description("Status")]
    Status,

    /// <summary>
    ///     목표 토크
    ///     target torque
    /// </summary>
    [Description("Target torque")]
    TargetTorque,

    /// <summary>
    ///     변환된 토크
    ///     converted torque
    /// </summary>
    [Description("Converted torque")]
    ConvertedTorque,

    /// <summary>
    ///     시팅 토크
    ///     seating torque
    /// </summary>
    [Description("Seating torque")]
    SeatingTorque,

    /// <summary>
    ///     클램프 토크
    ///     clamp torque
    /// </summary>
    [Description("Clamp torque")]
    ClampTorque,

    /// <summary>
    ///     프리베일링 토크
    ///     prevailing torque
    /// </summary>
    [Description("Prevailing torque")]
    PrevailingTorque,

    /// <summary>
    ///     스너그 토크
    ///     snug torque
    /// </summary>
    [Description("Snug torque")]
    SnugTorque,

    /// <summary>
    ///     속도
    ///     speed
    /// </summary>
    [Description("Speed")]
    Speed,

    /// <summary>
    ///     각도 1
    ///     angle 1
    /// </summary>
    [Description("Angle 1")]
    A1,

    /// <summary>
    ///     각도 2
    ///     angle 2
    /// </summary>
    [Description("Angle 2")]
    A2,

    /// <summary>
    ///     각도 3
    ///     angle 3
    /// </summary>
    [Description("Angle 3")]
    A3,

    /// <summary>
    ///     스너그 각도
    ///     snug angle
    /// </summary>
    [Description("Snug angle")]
    SnugAngle,

    #endregion

    #region Rev.1

    /// <summary>
    ///     NG 원인
    ///     NG cause
    /// </summary>
    [Description("NG cause")]
    NgCause,

    /// <summary>
    ///     ID1 이름
    ///     ID1 name
    /// </summary>
    [Description("ID1 name")]
    Id1Name,

    /// <summary>
    ///     ID1 값
    ///     ID1
    /// </summary>
    [Description("ID1")]
    Id1,

    /// <summary>
    ///     ID2 이름
    ///     ID2 name
    /// </summary>
    [Description("ID2 name")]
    Id2Name,

    /// <summary>
    ///     ID2 값
    ///     ID2
    /// </summary>
    [Description("ID2")]
    Id2,

    /// <summary>
    ///     ID3 이름
    ///     ID3 name
    /// </summary>
    [Description("ID3 name")]
    Id3Name,

    /// <summary>
    ///     ID3 값
    ///     ID3
    /// </summary>
    [Description("ID3")]
    Id3,

    /// <summary>
    ///     ID4 이름
    ///     ID4 name
    /// </summary>
    [Description("ID4 name")]
    Id4Name,

    /// <summary>
    ///     ID4 값
    ///     ID4
    /// </summary>
    [Description("ID4")]
    Id4,

    /// <summary>
    ///     ID5 이름
    ///     ID5 name
    /// </summary>
    [Description("ID5 name")]
    Id5Name,

    /// <summary>
    ///     ID5 값
    ///     ID5
    /// </summary>
    [Description("ID5")]
    Id5,

    /// <summary>
    ///     ID6 이름
    ///     ID6 name
    /// </summary>
    [Description("ID6 name")]
    Id6Name,

    /// <summary>
    ///     ID6 값
    ///     ID6
    /// </summary>
    [Description("ID6")]
    Id6

    #endregion
}