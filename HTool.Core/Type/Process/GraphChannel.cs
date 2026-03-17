using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     그래프 채널 열거형
///     graph channel enumeration
/// </summary>
public enum GraphChannel {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None,

    /// <summary>
    ///     토크
    ///     torque
    /// </summary>
    [Description("Torque")]
    Torque,

    /// <summary>
    ///     전류
    ///     current
    /// </summary>
    [Description("Current")]
    Current,

    /// <summary>
    ///     속도
    ///     speed
    /// </summary>
    [Description("Speed")]
    Speed,

    /// <summary>
    ///     각도
    ///     angle
    /// </summary>
    [Description("Angle")]
    Angle,

    /// <summary>
    ///     속도 지령
    ///     speed command
    /// </summary>
    [Description("Speed command")]
    SpeedCmd,

    /// <summary>
    ///     전류 지령
    ///     current command
    /// </summary>
    [Description("Current command")]
    CurrentCmd,

    /// <summary>
    ///     스너그 각도
    ///     snug angle
    /// </summary>
    [Description("Snug angle")]
    SnugAngle,

    /// <summary>
    ///     토크 / 각도
    ///     torque-angle
    /// </summary>
    [Description("Torque / Angle")]
    TorqueAngle
}