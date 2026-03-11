using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 작업의 스텝 유형
///     step type within a Pro X job sequence
/// </summary>
public enum JobStep {
	/// <summary>
	///     체결 스텝
	///     fastening step
	/// </summary>
	[Description("Fastening")]
    Fastening,

	/// <summary>
	///     입력 대기 스텝
	///     input wait step
	/// </summary>
	[Description("Input")]
    Input,

	/// <summary>
	///     출력 스텝
	///     output step
	/// </summary>
	[Description("Output")]
    Output,

	/// <summary>
	///     지연 스텝
	///     delay step
	/// </summary>
	[Description("Delay")]
    Delay,

	/// <summary>
	///     메시지 스텝
	///     message step
	/// </summary>
	[Description("Message")]
    Message
}