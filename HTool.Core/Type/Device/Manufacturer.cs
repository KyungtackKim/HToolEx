using System.ComponentModel;

namespace HTool.Core.Type.Device;

/// <summary>
///     제조사 열거형. 장치 제조사를 구분합니다.
///     manufacturer enumeration. Distinguishes device manufacturers.
/// </summary>
/// <remarks>
///     사용처: <see cref="ModelNames"/>, <see cref="HTool.Format.Device.Info"/>.
///     used by: <see cref="ModelNames"/>, <see cref="HTool.Format.Device.Info"/>.
/// </remarks>
public enum Manufacturer {
	/// <summary>
	///     알 수 없는 제조사
	///     unknown manufacturer
	/// </summary>
	[Description("Unknown")]
    Unknown = 0,

	/// <summary>
	///     한타스
	///     HANTAS
	/// </summary>
	[Description("Hantas")]
    Hantas = 1,

	/// <summary>
	///     마운츠
	///     Mountz
	/// </summary>
	[Description("Mountz")]
    Mountz = 2
}