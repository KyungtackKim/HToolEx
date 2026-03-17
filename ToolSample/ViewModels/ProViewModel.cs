using CommunityToolkit.Mvvm.ComponentModel;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     PRO X 페이지 ViewModel. 멀티툴 관리, 이벤트 구독, FTP 동기화.
///     PRO X page ViewModel. Multi-tool management, event subscription, FTP sync.
/// </summary>
public sealed class ProViewModel(
    HToolService       htool,
    IDispatcherService dispatcher) : ObservableObject, IPageViewModel {
    /// <inheritdoc />
    public void Activate() { }

    /// <inheritdoc />
    public void Deactivate() { }
}