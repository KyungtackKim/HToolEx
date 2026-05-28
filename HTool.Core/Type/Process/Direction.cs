using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     모터 회전 방향 열거형
///     motor rotation direction enumeration
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Device.Status</c>.
///     used by: <c>HTool.Format.Device.Status</c>.
/// </remarks>
public enum Direction {
    /// <summary>
    ///     체결
    ///     fastening
    /// </summary>
    [Description("Fastening")]
    Fastening,

    /// <summary>
    ///     풀림
    ///     loosening
    /// </summary>
    [Description("Loosening")]
    Loosening
}