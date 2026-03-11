using System.Windows;
using System.Windows.Controls;
using HTool.Type;
using ToolSample.ViewModels;

namespace ToolSample.Views;

/// <summary>
///     연결 페이지 View.
///     Connection page View.
/// </summary>
public partial class ConnectionView : UserControl {
	/// <summary>
	///     ConnectionView 인스턴스를 생성한다.
	///     Creates a ConnectionView instance.
	/// </summary>
	public ConnectionView() {
        // XAML 컴포넌트 초기화
        // initialize XAML components
        InitializeComponent();
    }

	/// <summary>
	///     ComType RadioButton 클릭 핸들러.
	///     ComType RadioButton click handler.
	/// </summary>
	private void OnComTypeRadioClick(object sender, RoutedEventArgs e) {
        // ViewModel 참조 획득
        // get ViewModel reference
        if (DataContext is not ConnectionViewModel vm)
            // ViewModel 없음 — 반환
            // no ViewModel — return
            return;

        // RadioButton Content로 ComType 결정
        // determine ComType from RadioButton Content
        if (sender is RadioButton rb)
            // Content 텍스트로 ComType 매핑
            // map ComType from Content text
            vm.SelectedComType = rb.Content?.ToString() switch {
                // RTU 선택
                // RTU selected
                "RTU" => ComType.Rtu,
                // TCP 선택
                // TCP selected
                "TCP" => ComType.Tcp,
                // PRO X 선택
                // PRO X selected
                "PRO X" => ComType.Pro,
                // 기본값 — TCP
                // default — TCP
                _ => ComType.Tcp
            };
    }
}