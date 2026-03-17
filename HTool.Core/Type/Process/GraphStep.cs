using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     그래프 스텝 열거형
///     graph step enumeration
/// </summary>
public enum GraphStep {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None,

    /// <summary>
    ///     자유 역회전
    ///     free reverse rotation
    /// </summary>
    [Description("Free reverse rotation")]
    FreeReverseRotation,

    /// <summary>
    ///     나사산 탭
    ///     thread tap
    /// </summary>
    [Description("Thread tap")]
    ThreadTap,

    /// <summary>
    ///     체결 진입
    ///     engaging
    /// </summary>
    [Description("Engaging")]
    Engaging,

    /// <summary>
    ///     자유 회전
    ///     free rotation
    /// </summary>
    [Description("Free rotation")]
    FreeRotation,

    /// <summary>
    ///     체결
    ///     fastening
    /// </summary>
    [Description("Fastening")]
    Fastening,

    /// <summary>
    ///     스너그 토크
    ///     snug torque
    /// </summary>
    [Description("Snug torque")]
    SnugTorque,

    /// <summary>
    ///     프리베일링 시작
    ///     prevailing start
    /// </summary>
    [Description("Prevailing start")]
    Prevailing,

    /// <summary>
    ///     안착
    ///     seating
    /// </summary>
    [Description("Seating")]
    Seating,

    /// <summary>
    ///     클램프
    ///     clamp
    /// </summary>
    [Description("Clamp")]
    Clamp,

    /// <summary>
    ///     토크 완료
    ///     torque complete
    /// </summary>
    [Description("Torque complete")]
    TorqueComplete,

    /// <summary>
    ///     토크업 후 회전
    ///     rotation after torque-up
    /// </summary>
    [Description("Rotation after torque-up")]
    RotationAfterTorqueUp
}