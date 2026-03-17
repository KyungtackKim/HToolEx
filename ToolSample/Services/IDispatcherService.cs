namespace ToolSample.Services;

/// <summary>
///     UI 스레드 디스패치 추상화 인터페이스.
///     UI thread dispatch abstraction interface.
/// </summary>
public interface IDispatcherService {
	/// <summary>
	///     UI 스레드에서 동기적으로 실행한다.
	///     Executes synchronously on the UI thread.
	/// </summary>
	/// <param name="action">실행할 액션 / action to execute</param>
	void Invoke(Action action);

	/// <summary>
	///     UI 스레드에서 비동기적으로 실행한다.
	///     Executes asynchronously on the UI thread.
	/// </summary>
	/// <param name="action">실행할 액션 / action to execute</param>
	/// <returns>완료 대기 태스크 / completion awaitable task</returns>
	Task InvokeAsync(Action action);
}