using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ToolRunner.ViewModels;

namespace ToolRunner;

/// <summary>
///     the runner window. keeps only view concerns: wiring the view model, auto-scrolling the log,
///     restricting numeric inputs to digits, resizing when the details panel is collapsed, and
///     holding the close until the sequence has stopped.
///     러너 메인 창 (뷰 관심사만 담당)
/// </summary>
public partial class MainWindow : Window {
    // window width used while only the control column is shown — control column plus chrome.
    // the window starts at this width because the details panel starts collapsed
    private const double CompactWidth = 400;

    // window width used while the details panel is visible
    private const double ExpandedWidth = 900;

    // minimum width enforced while the details panel is visible
    private const double ExpandedMinWidth = 880;

    // view model backing this window — kept for the load, resize and shutdown paths
    private readonly MainViewModel _viewModel;

    // width to restore when the details panel is shown again
    private double _expandedWidth = ExpandedWidth;

    // set once the async shutdown has run, so the second Close() is not intercepted again
    private bool _shutdownDone;

    /// <summary>
    ///     builds the window and binds it to the injected view model.
    ///     창 생성 및 뷰모델 바인딩
    /// </summary>
    /// <param name="viewModel">view model resolved from the DI container</param>
    public MainWindow(MainViewModel viewModel) {
        // build the visual tree from XAML
        InitializeComponent();
        // keep the view model for the load, resize and shutdown paths
        _viewModel = viewModel;
        // bind every control in the window
        DataContext = viewModel;
    }

    /// <summary>
    ///     window loaded handler — starts the view model, follows the log tail and watches the
    ///     details-panel flag.
    /// </summary>
    /// <param name="sender">the window</param>
    /// <param name="e">load event args</param>
    private void OnLoaded(object sender, RoutedEventArgs e) {
        // follow the newest log line as entries arrive
        ((INotifyCollectionChanged)_viewModel.Logs).CollectionChanged += OnLogsChanged;
        // resize the window whenever the details panel is collapsed or restored — subscribed before
        // Initialize so restoring a saved "expanded" state also restores the wide window
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        // subscribe the view model to the tool and runner events and restore the saved settings
        _viewModel.Initialize();
        // WindowStartupLocation alone lands off-centre on multi-monitor setups, so centre explicitly
        CenterOnScreen();
    }

    /// <summary>
    ///     centres the window in the primary screen's work area. the work area excludes the taskbar,
    ///     so the window never starts partly underneath it.
    /// </summary>
    private void CenterOnScreen() {
        // only a normal window can be repositioned meaningfully
        if (WindowState is not WindowState.Normal)
            // maximized or minimized — nothing to centre
            return;

        // work area in WPF device-independent units, so no DPI conversion is needed
        var work = SystemParameters.WorkArea;
        // centre horizontally within the work area
        Left = work.Left + (work.Width - Width) / 2;
        // centre vertically within the work area
        Top = work.Top + (work.Height - Height) / 2;
    }

    /// <summary>
    ///     view model property handler — mirrors the details-panel flag onto the window width.
    /// </summary>
    /// <param name="sender">the view model</param>
    /// <param name="e">changed property name</param>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        // only the details-visibility flag affects the window size
        if (e.PropertyName is not nameof(MainViewModel.IsDetailsVisible))
            // unrelated property — nothing to do
            return;

        // apply the width that matches the new panel state
        ApplyDetailsWidth();
    }

    /// <summary>
    ///     shrinks the window to the control column when the details panel is collapsed, and
    ///     restores the previous width when it is shown again.
    /// </summary>
    private void ApplyDetailsWidth() {
        // a maximized or minimized window keeps its size — only the panel disappears
        if (WindowState is not WindowState.Normal)
            // no resize to apply
            return;

        // showing the panel again needs the wide minimum back before the width is restored
        if (_viewModel.IsDetailsVisible) {
            // restore the wide minimum
            MinWidth = ExpandedMinWidth;
            // restore the remembered width
            Width = _expandedWidth;
            // keep the window centred at its new width
            CenterOnScreen();
            // the expanded state is applied
            return;
        }

        // remember the current width so it can be restored on the way back
        _expandedWidth = Width;
        // lower the minimum first, otherwise the width cannot shrink past it
        MinWidth = CompactWidth;
        // shrink to the control column
        Width = CompactWidth;
        // keep the window centred at its new width
        CenterOnScreen();
    }

    /// <summary>
    ///     log collection handler — scrolls the newest entry into view.
    /// </summary>
    /// <param name="sender">the log collection</param>
    /// <param name="e">collection change detail</param>
    private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        // only an append moves the tail
        if (e.Action is not NotifyCollectionChangedAction.Add)
            // removals come from the log cap and must not steal the scroll position
            return;

        // nothing to scroll to on an empty list
        if (_viewModel.Logs.Count is 0)
            // no entries — nothing to do
            return;

        // bring the newest entry into view
        LogList.ScrollIntoView(_viewModel.Logs[^1]);
    }

    /// <summary>
    ///     window closing handler — cancels the close once, stops the sequence and closes the tool,
    ///     then closes for real. this guarantees the motor is never left turning after the window
    ///     disappears.
    /// </summary>
    /// <param name="sender">the window</param>
    /// <param name="e">cancellable close event args</param>
    private async void OnClosing(object? sender, CancelEventArgs e) {
        // guard: the shutdown path is async void, so nothing may escape it
        try {
            // let the second, post-shutdown close through untouched
            if (_shutdownDone)
                // already unwound — proceed with the close
                return;

            // hold the close until the tool has been stopped
            e.Cancel = true;
            // stop the sequence, close the socket and unsubscribe every event
            await _viewModel.ShutdownAsync();
        } catch (Exception) {
            // shutdown failed — the window must still close, so fall through to the finally block
        } finally {
            // only the first pass re-issues the close; the second pass must not recurse
            if (!_shutdownDone) {
                // stop following the log tail
                ((INotifyCollectionChanged)_viewModel.Logs).CollectionChanged -= OnLogsChanged;
                // stop mirroring the details-panel flag
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                // mark the shutdown done so the next close is not intercepted
                _shutdownDone = true;
                // queue the real close — WPF rejects Close() while a Closing handler is still on the
                // stack, which is exactly where we are whenever ShutdownAsync completed synchronously
                _ = Dispatcher.InvokeAsync(Close);
            }
        }
    }

    /// <summary>
    ///     numeric input filter — rejects anything that is not an ASCII digit.
    /// </summary>
    /// <param name="sender">the text box being typed into</param>
    /// <param name="e">composition event carrying the typed text</param>
    private void OnDigitOnlyInput(object sender, TextCompositionEventArgs e) {
        // swallow the input unless every typed character is a digit
        e.Handled = !e.Text.All(char.IsAsciiDigit);
    }
}
