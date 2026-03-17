using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Pro X 메시지 스텝의 동작 유형
///     action type for Pro X message step
/// </summary>
public enum StepMessage {
	/// <summary>
	///     입력 검증 대기
	///     validation
	/// </summary>
	[Description("Validation")]
    Validation,

	/// <summary>
	///     지연 시간 (초)
	///     delay time (sec)
	/// </summary>
	[Description("Delay time (sec)")]
    DelayTime,

	/// <summary>
	///     다음 스텝으로 자동 진행
	///     next step
	/// </summary>
	[Description("Next step")]
    NextStep
}