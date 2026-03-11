using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     모터 회전 방향 타입
///     Motor rotation direction types
/// </summary>
public enum DirectionTypes {
    [Description("Fastening")]
    Fastening,
    [Description("Loosening")]
    Loosening
}