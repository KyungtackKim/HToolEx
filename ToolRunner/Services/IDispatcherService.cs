namespace ToolRunner.Services;

/// <summary>
///     marshals work onto the UI thread. HTool raises its events from timer / socket threads, so
///     every observable-property update triggered by those events has to be dispatched.
///     UI 스레드 디스패치 추상화
/// </summary>
public interface IDispatcherService {
    /// <summary>
    ///     runs the action on the UI thread and blocks until it completes.
    ///     UI 스레드에서 동기 실행
    /// </summary>
    /// <param name="action">work to run on the UI thread</param>
    void Invoke(Action action);

    /// <summary>
    ///     queues the action on the UI thread and returns a task that completes with it.
    ///     UI 스레드에 비동기 큐잉
    /// </summary>
    /// <param name="action">work to queue on the UI thread</param>
    /// <returns>task completing when the queued action has run</returns>
    Task InvokeAsync(Action action);
}
