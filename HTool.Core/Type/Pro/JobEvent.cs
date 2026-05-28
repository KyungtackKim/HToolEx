using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 작업 이벤트 상태
///     job event status for Pro X operation
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Pro.JobEvent</c>, <c>HTool.Device.Pro.ProService</c>.
///     used by: <c>HTool.Format.Pro.JobEvent</c>, <c>HTool.Device.Pro.ProService</c>.
/// </remarks>
public enum JobEvent {
	/// <summary>
	///     스텝 진입
	///     step into
	/// </summary>
	[Description("Step into")]
    StepInto,

	/// <summary>
	///     체결 OK
	///     fastening OK
	/// </summary>
	[Description("Fastening OK")]
    FastenOk,

	/// <summary>
	///     체결 NG
	///     fastening NG
	/// </summary>
	[Description("Fastening NG")]
    FastenNg,

	/// <summary>
	///     스텝 OK
	///     step OK
	/// </summary>
	[Description("Step OK")]
    StepOk,

	/// <summary>
	///     스텝 NG
	///     step NG
	/// </summary>
	[Description("Step NG")]
    StepNg,

	/// <summary>
	///     작업 OK
	///     job OK
	/// </summary>
	[Description("Job OK")]
    JobOk,

	/// <summary>
	///     작업 NG
	///     job NG
	/// </summary>
	[Description("Job NG")]
    JobNg,

	/// <summary>
	///     작업 중단
	///     job aborted
	/// </summary>
	[Description("Job aborted")]
    JobAborted,

	/// <summary>
	///     스텝 건너뛰기
	///     skip
	/// </summary>
	[Description("Skip")]
    Skip,

	/// <summary>
	///     이전 스텝으로 되돌아감
	///     back
	/// </summary>
	[Description("Back")]
    Back,

	/// <summary>
	///     현재 스텝 초기화
	///     reset step
	/// </summary>
	[Description("Reset step")]
    ResetStep,

	/// <summary>
	///     현재 작업 초기화
	///     reset job
	/// </summary>
	[Description("Reset job")]
    ResetJob
}