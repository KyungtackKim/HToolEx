namespace HTool.Format.Pro.Job;

/// <summary>
///     Job 파싱 또는 검증 시 발생할 수 있는 오류 유형.
///     error types that can occur during job parsing or validation.
/// </summary>
public enum JobError {
	/// <summary>
	///     오류 없음
	///     no error
	/// </summary>
	None,

	/// <summary>
	///     파일 크기가 헤더보다 작음
	///     file is too small to contain a valid header
	/// </summary>
	FileTooSmall,

	/// <summary>
	///     시그니처가 유효하지 않음
	///     signature does not match expected value
	/// </summary>
	InvalidSignature,

	/// <summary>
	///     지원하지 않는 버전
	///     unknown or unsupported version
	/// </summary>
	UnknownVersion,

	/// <summary>
	///     스텝 수가 유효하지 않음
	///     step count is out of valid range
	/// </summary>
	InvalidStepCount,

	/// <summary>
	///     개별 스텝 파싱 실패
	///     individual step parsing failed
	/// </summary>
	InvalidStep,

	/// <summary>
	///     헤더의 카운트와 실제 스텝 수 불일치
	///     header counts do not match actual step counts
	/// </summary>
	CountMismatch
}