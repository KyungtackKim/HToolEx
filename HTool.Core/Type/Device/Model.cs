using System.ComponentModel;

namespace HTool.Core.Type.Device;

/// <summary>
///     장치 모델 코드 열거형. 장치가 보고하는 원시 값을 사용합니다.
///     device model code enumeration. Uses raw values as reported by the device.
/// </summary>
/// <remarks>
///     사용처: <see cref="ModelNames" />, <c>HTool.Format.Device.SimpleInfo</c>, <c>HTool.Format.Device.Info</c>,
///     <c>HTool.Format.Device.Status</c>.
///     used by: <see cref="ModelNames" />, <c>HTool.Format.Device.SimpleInfo</c>, <c>HTool.Format.Device.Info</c>,
///     <c>HTool.Format.Device.Status</c>.
/// </remarks>
public enum Model {
	/// <summary>
	///     MD 모델
	///     MD model
	/// </summary>
	[Description("MD")]
    Md = 1,

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
    Bt = 32,

	/// <summary>
	///     ADE 모델. xml 4.2.3에서 AD(2)가 ADE/ADT로 분할되었습니다.
	///     ADE model. AD(2) was split into ADE/ADT as of xml 4.2.3.
	/// </summary>
	[Description("ADE")]
    Ade = 35,

	/// <summary>
	///     ADT 모델. xml 4.2.3에서 AD(2)가 ADE/ADT로 분할되었습니다.
	///     ADT model. AD(2) was split into ADE/ADT as of xml 4.2.3.
	/// </summary>
	[Description("ADT")]
    Adt = 36
}