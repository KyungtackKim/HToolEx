namespace ToolSample.ViewModels;

/// <summary>
///     페이지 ViewModel 생명주기 인터페이스.
///     Page ViewModel lifecycle interface.
/// </summary>
public interface IPageViewModel {
	/// <summary>
	///     페이지 활성화. 이벤트 구독 및 타이머 시작.
	///     Activate page. Subscribe events and start timers.
	/// </summary>
	void Activate();

	/// <summary>
	///     페이지 비활성화. 이벤트 해제 및 타이머 정지.
	///     Deactivate page. Unsubscribe events and stop timers.
	/// </summary>
	void Deactivate();
}