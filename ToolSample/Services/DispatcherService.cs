using System.Windows;

namespace ToolSample.Services;

/// <summary>
///     WPF Dispatcher 기반 UI 스레드 디스패치 서비스.
///     WPF Dispatcher-based UI thread dispatch service.
/// </summary>
public sealed class DispatcherService : IDispatcherService {
    /// <inheritdoc />
    public void Invoke(Action action) {
        Application.Current.Dispatcher.Invoke(action);
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action) {
        return Application.Current.Dispatcher.InvokeAsync(action).Task;
    }
}