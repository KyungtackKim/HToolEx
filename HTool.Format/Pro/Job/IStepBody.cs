using HTool.Core.Type.Pro;

namespace HTool.Format.Pro.Job;

/// <summary>
///     스텝 본문(body) 인터페이스. 각 스텝 유형은 이 인터페이스를 구현합니다.
///     step body interface. each step type implements this interface.
/// </summary>
/// <remarks>
///     v2 조합(composition) 기반 설계: Step = StepHeader + IStepBody.
///     v2 composition-based design: Step = StepHeader + IStepBody.
/// </remarks>
public interface IStepBody {
	/// <summary>
	///     이 본문의 스텝 유형을 선언합니다.
	///     declares the step type this body represents.
	/// </summary>
	JobStep StepType { get; }

	/// <summary>
	///     본문을 바이트 배열로 직렬화합니다 (리틀엔디안).
	///     serializes the body to a byte array (little-endian).
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	byte[] ToBytes(int revision = 0);
}