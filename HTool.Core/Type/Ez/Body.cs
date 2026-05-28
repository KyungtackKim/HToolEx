using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     본체 형태 열거형
///     body type enumeration
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Ez.CalibrationData</c>, <c>HTool.Format.Ez.CalibrationSettings</c>.
///     used by: <c>HTool.Format.Ez.CalibrationData</c>, <c>HTool.Format.Ez.CalibrationSettings</c>.
/// </remarks>
public enum Body : byte {
	/// <summary>
	///     일체형
	///     integrated type
	/// </summary>
	[Description("Integrated")]
    Integrated = 0x00,

	/// <summary>
	///     분리형
	///     separated type
	/// </summary>
	[Description("Separated")]
    Separated = 0x01
}