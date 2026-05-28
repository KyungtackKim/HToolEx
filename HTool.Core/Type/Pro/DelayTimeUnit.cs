using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 지연 스텝의 시간 단위
///     time unit for Pro X delay step
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Pro.Job.DelayBody</c>.
///     used by: <c>HTool.Format.Pro.Job.DelayBody</c>.
/// </remarks>
public enum DelayTimeUnit {
	/// <summary>
	///     초 단위
	///     second
	/// </summary>
	[Description("Second")]
    Second,

	/// <summary>
	///     밀리초 단위
	///     millisecond
	/// </summary>
	[Description("Millisecond")]
    Millisecond
}