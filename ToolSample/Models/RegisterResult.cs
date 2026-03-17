namespace ToolSample.Models;

/// <summary>
///     레지스터 읽기 결과 모델. DataGrid 표시용.
///     Register read result model. For DataGrid display.
/// </summary>
public sealed class RegisterResult {
	/// <summary>
	///     레지스터 주소.
	///     Register address.
	/// </summary>
	public ushort Address { get; init; }

	/// <summary>
	///     16진수 값 문자열.
	///     Hexadecimal value string.
	/// </summary>
	public string HexValue { get; init; } = "";

	/// <summary>
	///     10진수 값.
	///     Decimal value.
	/// </summary>
	public ushort DecValue { get; init; }

	/// <summary>
	///     ASCII 문자열 표현.
	///     ASCII string representation.
	/// </summary>
	public string AsciiValue { get; init; } = "";
}