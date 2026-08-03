using System.Windows;

namespace ToolRunner.Services;

/// <summary>
///     WPF <see cref="System.Windows.Threading.Dispatcher" />-based implementation of
///     <see cref="IDispatcherService" />.
///     WPF Dispatcher 기반 구현
/// </summary>
public sealed class DispatcherService : IDispatcherService {
    /// <inheritdoc />
    public void Invoke(Action action) {
        // hand the work to the application dispatcher and wait for it
        Application.Current.Dispatcher.Invoke(action);
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action) {
        // queue the work on the application dispatcher and expose its task
        return Application.Current.Dispatcher.InvokeAsync(action).Task;
    }
}
