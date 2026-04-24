using System.ComponentModel;

namespace HTool.Core.Type.Device;

/// <summary>
///     장치 모델 코드 열거형. 장치가 보고하는 원시 값을 사용합니다.
///     device model code enumeration. Uses raw values as reported by the device.
/// </summary>
/// <remarks>
///     사용처: <see cref="ModelNames"/>, <see cref="HTool.Format.Device.SimpleInfo"/>, <see cref="HTool.Format.Device.Info"/>, <see cref="HTool.Format.Device.Status"/>.
///     used by: <see cref="ModelNames"/>, <see cref="HTool.Format.Device.SimpleInfo"/>, <see cref="HTool.Format.Device.Info"/>, <see cref="HTool.Format.Device.Status"/>.
/// </remarks>
public enum Model {
	/// <summary>
	///     MD 모델
	///     MD model
	/// </summary>
	[Description("MD")]
    Md = 1,

	/// <summary>
	///     AD 모델
	///     AD model
	/// </summary>
	[Description("AD")]
    Ad = 2,

	/// <summary>
	///     BM 모델
	///     BM model
	/// </summary>
	[Description("BM")]
    Bm = 10,

	/// <summary>
	///     MDT 모델
	///     MDT model
	/// </summary>
	[Description("MDT")]
    Mdt = 15,

	/// <summary>
	///     BMT 모델
	///     BMT model
	/// </summary>
	[Description("BMT")]
    Bmt = 19,

	/// <summary>
	///     BPT 모델
	///     BPT model
	/// </summary>
	[Description("BPT")]
    Bpt = 20,

	/// <summary>
	///     MDT40 모델
	///     MDT40 model
	/// </summary>
	[Description("MDT40")]
    Mdt40 = 27,

	/// <summary>
	///     BMT40 모델
	///     BMT40 model
	/// </summary>
	[Description("BMT40")]
    Bmt40 = 29,

	/// <summary>
	///     ET 모델
	///     ET model
	/// </summary>
	[Description("ET")]
    Et = 30,

	/// <summary>
	///     BT 모델
	///     BT model
	/// </summary>
	[Description("BT")]
    Bt = 32
}