using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     32비트 MODBUS 레지스터 값의 워드 순서
///     word order for 32-bit MODBUS register values
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Core.Util.ByteOrder" />, <see cref="HTool.Core.Util.Packing" />.
///     used by: <see cref="HTool.Core.Util.ByteOrder" />, <see cref="HTool.Core.Util.Packing" />.
/// </remarks>
public enum WordOrder {
	/// <summary>
	///     상위 워드 먼저 (ABCD) — 가장 일반적
	///     high word first (ABCD) — most common
	///     예시 / Example: 0x12345678 → [0x12, 0x34, 0x56, 0x78]
	/// </summary>
	[Description("High-Low (ABCD)")]
    HighLow,

	/// <summary>
	///     하위 워드 먼저 (CDAB) — 일부 장치
	///     low word first (CDAB) — some devices
	///     예시 / Example: 0x12345678 → [0x56, 0x78, 0x12, 0x34]
	/// </summary>
	[Description("Low-High (CDAB)")]
    LowHigh
}