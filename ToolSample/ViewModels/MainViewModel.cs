using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HTool.Type;
using Microsoft.Extensions.DependencyInjection;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     메인 윈도우 ViewModel. 사이드바 네비게이션 및 StatusBar 상태를 관리한다.
///     Main window ViewModel. Manages sidebar navigation and StatusBar state.
/// </summary>
public sealed partial class MainViewModel(
    HToolService       htool,
    IDispatcherService dispatcher,
    IServiceProvider   services) : ObservableObject {
    // 통신 유형 표시 텍스트
    // communication type display text
    [ObservableProperty]
    private string _comTypeText = "---";

    // 현재 연결 상태
    // current connection state
    [ObservableProperty]
    private Connection _connectionState = Connection.Closed;

    // 현재 표시 중인 페이지 ViewModel
    // currently displayed page ViewModel
    [ObservableProperty]
    private object? _currentPage;

    // 오류 메시지 표시 텍스트
    // error message display text
    [ObservableProperty]
    private string _errorText = "";

    // 펌웨어 버전 표시 텍스트
    // firmware version display text
    [ObservableProperty]
    private string _firmwareText = "---";

    // 모델명 표시 텍스트
    // model name display text
    [ObservableProperty]
    private string _modelText = "---";

    // 사이드바에서 선택된 페이지 항목
    // selected page item in sidebar
    [ObservableProperty]
    private PageItem? _selectedPage;

    // 시리얼 번호 표시 텍스트
    // serial number display text
    [ObservableProperty]
    private string _serialText = "---";

    /// <summary>
    ///     네비게이션 페이지 목록.
    ///     Navigation page list.
    /// </summary>
    public ObservableCollection<PageItem> Pages { get; } = [
        new("Connection", typeof(ConnectionViewModel)),
        new("Register", typeof(RegisterViewModel)),
        new("Event", typeof(EventViewModel)),
        new("Graph", typeof(GraphViewModel)),
        new("Log", typeof(LogViewModel)),
        new("PRO X", typeof(ProViewModel))
    ];

    /// <summary>
    ///     초기화. 이벤트 구독 및 첫 페이지 선택.
    ///     Initialize. Subscribe events and select first page.
    /// </summary>
    public void Initialize() {
        // 4-state 연결 상태 변경 이벤트 구독
        // subscribe to 4-state connection state changed event
        htool.ConnectionStateChanged += OnConnectionStateUpdated;
        // 오류 수신 이벤트 구독
        // subscribe to error received event
        htool.ErrorReceived += OnErrorReceived;

        // 첫 페이지 선택
        // select first page
        SelectedPage = Pages[0];
    }

    /// <summary>
    ///     정리. 이벤트 구독 해제.
    ///     Cleanup. Unsubscribe events.
    /// </summary>
    public void Cleanup() {
        // 4-state 연결 상태 변경 이벤트 구독 해제
        // unsubscribe from 4-state connection state changed event
        htool.ConnectionStateChanged -= OnConnectionStateUpdated;
        // 오류 수신 이벤트 구독 해제
        // unsubscribe from error received event
        htool.ErrorReceived -= OnErrorReceived;

        // 현재 페이지 비활성화
        // deactivate current page
        if (CurrentPage is IPageViewModel page)
            // 페이지 비활성화
            // deactivate page
            page.Deactivate();
    }

    /// <inheritdoc />
    partial void OnSelectedPageChanged(PageItem? value) {
        // 선택 해제 시 무시
        // ignore when deselected
        if (value is null)
            // 선택 없음 — 반환
            // no selection — return
            return;

        // 이전 페이지 비활성화
        // deactivate previous page
        if (CurrentPage is IPageViewModel oldPage)
            // 이전 페이지 비활성화
            // deactivate previous page
            oldPage.Deactivate();

        // DI에서 새 ViewModel 인스턴스 생성
        // create new ViewModel instance from DI
        var vm = services.GetRequiredService(value.ViewModelType);

        // 새 페이지 활성화
        // activate new page
        if (vm is IPageViewModel newPage)
            // 새 페이지 활성화
            // activate new page
            newPage.Activate();

        // 현재 페이지 설정
        // set current page
        CurrentPage = vm;
    }

    /// <summary>
    ///     4-state 연결 상태 변경 핸들러.
    ///     4-state connection state changed handler.
    /// </summary>
    private void OnConnectionStateUpdated(Connection state) {
        // UI 스레드에서 상태 갱신
        // update state on UI thread
        dispatcher.Invoke(() => {
            // 연결 상태 갱신
            // update connection state
            ConnectionState = state;
            // 통신 유형 갱신
            // update communication type
            ComTypeText = htool.Tool.Type.ToString();

            // 연결 완료 시 장치 정보 갱신
            // update device info on connected
            if (state is Connection.Connected) {
                // 모델 표시명 갱신
                // update model display name
                ModelText = htool.Tool.Info.ModelName;
                // 시리얼 번호 갱신
                // update serial number
                SerialText = htool.Tool.Info.Serial;
                // 펌웨어 버전 갱신
                // update firmware version
                FirmwareText = htool.Tool.Info.FirmwareText;
            }

            // 연결 해제 완료 시 기본값으로 초기화
            // reset to defaults on closed
            if (state is Connection.Closed) {
                // 모델명 초기화
                // reset model name
                ModelText = "---";
                // 시리얼 번호 초기화
                // reset serial number
                SerialText = "---";
                // 펌웨어 버전 초기화
                // reset firmware version
                FirmwareText = "---";
            }

            // 오류 메시지 초기화
            // clear error message
            ErrorText = "";
        });
    }

    /// <summary>
    ///     오류 수신 핸들러.
    ///     Error received handler.
    /// </summary>
    private void OnErrorReceived(ComError error) {
        // UI 스레드에서 오류 표시
        // display error on UI thread
        dispatcher.Invoke(() => {
            // 오류 메시지 설정
            // set error message
            ErrorText = $"{error.Reason}: {error.Detail}";
        });
    }
}