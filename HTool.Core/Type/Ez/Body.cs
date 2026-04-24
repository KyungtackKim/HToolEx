using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     본체 형태 열거형
///     body type enumeration
/// </summary>
/// <remarks>
///     사용처: <see cref="HTool.Format.Ez.CalibrationData"/>, <see cref="HTool.Format.Ez.CalibrationSettings"/>.
///     used by: <see cref="HTool.Format.Ez.CalibrationData"/>, <see cref="HTool.Format.Ez.CalibrationSettings"/>.
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