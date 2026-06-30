using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 작업의 스텝 유형
///     step type within a Pro X job sequence
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Pro.Job.StepHeader</c>, <c>HTool.Format.Pro.Job.FastenBody</c>,
///     <c>HTool.Format.Pro.Job.DelayBody</c>, <c>HTool.Format.Pro.Job.InputBody</c>,
///     <c>HTool.Format.Pro.Job.OutputBody</c>, <c>HTool.Format.Pro.Job.MessageBody</c>.
///     used by: <c>HTool.Format.Pro.Job.StepHeader</c>, <c>HTool.Format.Pro.Job.FastenBody</c>,
///     <c>HTool.Format.Pro.Job.DelayBody</c>, <c>HTool.Format.Pro.Job.InputBody</c>,
///     <c>HTool.Format.Pro.Job.OutputBody</c>, <c>HTool.Format.Pro.Job.MessageBody</c>.
/// </remarks>
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