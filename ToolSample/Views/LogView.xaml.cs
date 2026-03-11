using System.Collections.Specialized;
using System.Windows.Controls;
using ToolSample.ViewModels;

namespace ToolSample.Views;

/// <summary>
///     로그 뷰어 페이지 View. DataGrid 자동 스크롤 지원.
///     Log viewer page View. Supports DataGrid auto-scroll.
/// </summary>
public partial class LogView : UserControl {
	/// <summary>
	///     LogView 인스턴스를 생성한다.
	///     Creates a LogView instance.
	/// </summary>
	public LogView() {
        // XAML 컴포넌트 초기화
        // initialize XAML components
        InitializeComponent();

        // DataContext 변경 시 자동 스크롤 구독
        // subscribe auto-scroll on DataContext change
        DataContextChanged += (_, _) => {
            // ViewModel에서 로그 컬렉션 변경 이벤트 구독
            // subscribe to log collection changed event from ViewModel
            if (DataContext is LogViewModel vm)
                // 컬렉션 변경 이벤트 구독
                // subscribe to collection changed event
                vm.Logs.CollectionChanged += OnLogsCollectionChanged;
        };
    }

	/// <summary>
	///     로그 컬렉션 변경 핸들러. 마지막 항목으로 자동 스크롤.
	///     Log collection changed handler. Auto-scrolls to last item.
	/// </summary>
	private void OnLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        // 추가 이벤트일 때만 스크롤
        // scroll only on add event
        if (e.Action is not NotifyCollectionChangedAction.Add)
            // 추가가 아님 — 반환
            // not an add — return
            return;

        // 일시정지 중이면 스크롤하지 않음
        // skip scroll if paused
        if (DataContext is LogViewModel { IsPaused: true })
            // 일시정지 — 반환
            // paused — return
            return;

        // 마지막 항목으로 스크롤
        // scroll to last item
        if (LogGrid.Items.Count > 0)
            // 마지막 항목으로 스크롤
            // scroll to last item
            LogGrid.ScrollIntoView(LogGrid.Items[^1]);
    }
}