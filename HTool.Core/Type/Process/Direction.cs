using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     모터 회전 방향 열거형
///     motor rotation direction enumeration
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Format.Device.Status"/>.
///     used by: <see cref="HTool.Format.Device.Status"/>.
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