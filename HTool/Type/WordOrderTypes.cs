using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     32비트 MODBUS 레지스터 값의 워드 순서 타입
///     Word order types for 32-bit MODBUS register values
/// </summary>
public enum WordOrderTypes {
    /// <summary>
    ///     상위 워드 먼저 (ABCD) - 가장 일반적
    ///     High word first (ABCD) - Most common
    ///     예시 / Example: 0x12345678 → [0x12, 0x34, 0x56, 0x78]
    /// </summary>
    [Description("상위-하위 / High-Low (ABCD)")]
    HighLow,

    /// <summary>
    ///     하위 워드 먼저 (CDAB) - 일부 장치
    ///     Low word first (CDAB) - Some devices
    ///     예시 / Example: 0x12345678 → [0x56, 0x78, 0x12, 0x34]
    /// </summary>
    [Description("하위-상위 / Low-High (CDAB)")]
    LowHigh
}