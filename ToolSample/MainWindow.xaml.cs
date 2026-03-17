using System.ComponentModel;
using System.Windows;
using ToolSample.ViewModels;

namespace ToolSample;

/// <summary>
///     메인 윈도우. 사이드바 네비게이션 + ContentControl + StatusBar 셸.
///     Main window. Sidebar navigation + ContentControl + StatusBar shell.
/// </summary>
public partial class MainWindow : Window {
	/// <summary>
	///     MainWindow 인스턴스를 생성한다.
	///     Creates a MainWindow instance.
	/// </summary>
	public MainWindow() {
        // XAML 컴포넌트 초기화
        // initialize XAML components
        InitializeComponent();

        // DataContext 설정 후 ViewModel 초기화
        // initialize ViewModel after DataContext is set
        Loaded += OnLoaded;
        // 윈도우 닫힘 시 ViewModel 정리
        // cleanup ViewModel on window closing
        Closing += OnClosing;
    }

	/// <summary>
	///     윈도우 로드 완료 핸들러.
	///     Window loaded handler.
	/// </summary>
	private void OnLoaded(object sender, RoutedEventArgs e) {
        // MainViewModel 초기화 (이벤트 구독, 첫 페이지 선택)
        // initialize MainViewModel (subscribe events, select first page)
        if (DataContext is MainViewModel vm)
            // ViewModel 초기화
            // initialize ViewModel
            vm.Initialize();
    }

	/// <summary>
	///     윈도우 닫힘 핸들러.
	///     Window closing handler.
	/// </summary>
	private void OnClosing(object? sender, CancelEventArgs e) {
        // MainViewModel 정리 (이벤트 해제)
        // cleanup MainViewModel (unsubscribe events)
        if (DataContext is MainViewModel vm)
            // ViewModel 정리
            // cleanup ViewModel
            vm.Cleanup();
    }
}