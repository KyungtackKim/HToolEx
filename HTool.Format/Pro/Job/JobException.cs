namespace HTool.Format.Pro.Job;

/// <summary>
///     Job 처리 중 발생하는 예외의 기본 클래스.
///     base class for exceptions that occur during job processing.
/// </summary>
public abstract class JobException : Exception {
	/// <summary>
	///     메시지를 지정하여 생성합니다.
	///     creates an instance with the specified message.
	/// </summary>
	/// <param name="message">오류 메시지 / error message</param>
	protected JobException(string message) : base(message) { }

	/// <summary>
	///     메시지와 내부 예외를 지정하여 생성합니다.
	///     creates an instance with the specified message and inner exception.
	/// </summary>
	/// <param name="message">오류 메시지 / error message</param>
	/// <param name="innerException">내부 예외 / inner exception</param>
	protected JobException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
///     Job 파싱 중 발생하는 예외.
///     exception that occurs during job parsing.
/// </summary>
public class JobParseException : JobException {
	/// <summary>
	///     메시지를 지정하여 생성합니다.
	///     creates an instance with the specified message.
	/// </summary>
	/// <param name="message">오류 메시지 / error message</param>
	public JobParseException(string message) : base(message) { }

	/// <summary>
	///     메시지와 내부 예외를 지정하여 생성합니다.
	///     creates an instance with the specified message and inner exception.
	/// </summary>
	/// <param name="message">오류 메시지 / error message</param>
	/// <param name="innerException">내부 예외 / inner exception</param>
	public JobParseException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
///     지원하지 않는 리비전일 때 발생하는 예외.
///     exception thrown when an unknown revision is encountered.
/// </summary>
public class UnknownRevisionException : JobParseException {
	/// <summary>
	///     리비전 정보를 포함하여 생성합니다.
	///     creates an instance with the revision information.
	/// </summary>
	/// <param name="major">메이저 버전 / major version</param>
	/// <param name="minor">마이너 버전 / minor version</param>
	public UnknownRevisionException(int major, int minor)
        : base($"Unknown job revision: {major}.{minor}") {
        // 메이저 버전 저장
        // store major version
        Major = major;
        // 마이너 버전 저장
        // store minor version
        Minor = minor;
    }

	/// <summary>
	///     메이저 버전
	///     major version
	/// </summary>
	public int Major { get; }

	/// <summary>
	///     마이너 버전
	///     minor version
	/// </summary>
	public int Minor { get; }
}

/// <summary>
///     알 수 없는 스텝 유형일 때 발생하는 예외.
///     exception thrown when an unknown step type is encountered.
/// </summary>
public class UnknownStepTypeException : JobParseException {
	/// <summary>
	///     스텝 유형 값을 포함하여 생성합니다.
	///     creates an instance with the step type value.
	/// </summary>
	/// <param name="typeValue">알 수 없는 스텝 유형 값 / unknown step type value</param>
	public UnknownStepTypeException(int typeValue)
        : base($"Unknown step type: {typeValue}") {
        // 유형 값 저장
        // store type value
        TypeValue = typeValue;
    }

	/// <summary>
	///     알 수 없는 스텝 유형 값
	///     the unknown step type value
	/// </summary>
	public int TypeValue { get; }
}

/// <summary>
///     Job 직렬화 중 발생하는 예외.
///     exception that occurs during job serialization.
/// </summary>
public class JobSerializeException : JobException {
	/// <summary>
	///     메시지를 지정하여 생성합니다.
	///     creates an instance with the specified message.
	/// </summary>
	/// <param name="message">오류 메시지 / error message</param>
	public JobSerializeException(string message) : base(message) { }

	/// <summary>
	///     메시지와 내부 예외를 지정하여 생성합니다.
	///     creates an instance with the specified message and inner exception.
	/// </summary>
	/// <param name="message">오류 메시지 / error message</param>
	/// <param name="innerException">내부 예외 / inner exception</param>
	public JobSerializeException(string message, Exception innerException) : base(message, innerException) { }
}