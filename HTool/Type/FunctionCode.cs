using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     MODBUS 함수 코드를 정의한다.
///     Defines MODBUS function codes.
/// </summary>
public enum FunctionCode : byte {
	/// <summary>
	///     보유 레지스터 읽기 (FC 0x03).
	///     Read Holding Registers (FC 0x03).
	/// </summary>
	[Description("Read Holding Registers (0x03)")]
	ReadHoldingReg = 0x03,

	/// <summary>
	///     입력 레지스터 읽기 (FC 0x04).
	///     Read Input Registers (FC 0x04).
	/// </summary>
	[Description("Read Input Registers (0x04)")]
	ReadInputReg = 0x04,

	/// <summary>
	///     단일 레지스터 쓰기 (FC 0x06).
	///     Write Single Register (FC 0x06).
	/// </summary>
	[Description("Write Single Register (0x06)")]
	WriteSingleReg = 0x06,

	/// <summary>
	///     다중 레지스터 쓰기 (FC 0x10).
	///     Write Multiple Registers (FC 0x10).
	/// </summary>
	[Description("Write Multiple Registers (0x10)")]
	WriteMultiReg = 0x10,

	/// <summary>
	///     장치 정보 읽기 (FC 0x11, HANTAS 전용).
	///     Read Device Information (FC 0x11, HANTAS custom).
	/// </summary>
	[Description("Read Device Info (0x11)")]
	ReadInfoReg = 0x11,

	/// <summary>
	///     그래프 데이터 (FC 0x64, HANTAS 전용).
	///     Graph Data (FC 0x64, HANTAS custom).
	/// </summary>
	[Description("Graph Data (0x64)")]
	GraphData = 0x64,

	/// <summary>
	///     그래프 결과 (FC 0x65, HANTAS 전용).
	///     Graph Result (FC 0x65, HANTAS custom).
	/// </summary>
	[Description("Graph Result (0x65)")]
	GraphRes = 0x65,

	/// <summary>
	///     고해상도 그래프 (FC 0x66, HANTAS 전용).
	///     High Resolution Graph (FC 0x66, HANTAS custom).
	/// </summary>
	[Description("High Resolution Graph (0x66)")]
	HighResGraph = 0x66,

	/// <summary>
	///     오류 응답 (FC 0x80 이상, 원래 FC + 0x80).
	///     Error Response (FC 0x80+, original FC + 0x80).
	/// </summary>
	[Description("Error Response (0x80+)")]
	Error = 0x80
}
