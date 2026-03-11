using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HTool.Type;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     연결 페이지 ViewModel. RTU/TCP/PRO X 연결 관리.
///     Connection page ViewModel. Manages RTU/TCP/PRO X connections.
/// </summary>
public sealed partial class ConnectionViewModel(
    HToolService       htool,
    IDispatcherService dispatcher) : ObservableObject, IPageViewModel {
    // 사용 가능한 COM 포트 목록
    // available COM port list
    [ObservableProperty]
    private string[] _availableComPorts = [];

    // TCP/PRO X: IP 주소
    // TCP/PRO X: IP address
    [ObservableProperty]
    private string _ipAddress = "192.168.1.1";

    // 연결 상태
    // connection state
    [ObservableProperty]
    private bool _isConnected;

    // 연결 시도 중 상태
    // connecting in progress state
    [ObservableProperty]
    private bool _isConnecting;

    // Keep-Alive 활성화 여부
    // Keep-Alive enabled flag
    [ObservableProperty]
    private bool _keepAliveEnabled;

    // 메시지 타임아웃 설정 (ms)
    // message timeout setting (ms)
    [ObservableProperty]
    private int _messageTimeout = 1000;

    // TCP 포트 (TCP 기본값 502)
    // TCP port (TCP default 502)
    [ObservableProperty]
    private int _port = 502;

    // RTU: 선택된 보드레이트
    // RTU: selected baud rate
    [ObservableProperty]
    private int _selectedBaudRate = 115200;

    // RTU: 선택된 COM 포트
    // RTU: selected COM port
    [ObservableProperty]
    private string _selectedComPort = "";

    // 선택된 통신 유형
    // selected communication type
    [ObservableProperty]
    private ComType _selectedComType = ComType.Tcp;

    // MODBUS 슬레이브 ID
    // MODBUS slave ID
    [ObservableProperty]
    private byte _slaveId = 0x01;

    /// <summary>
    ///     지원하는 보드레이트 목록.
    ///     Supported baud rate list.
    /// </summary>
    public int[] BaudRates => [9600, 19200, 38400, 57600, 115200, 230400];

    /// <inheritdoc />
    public void Activate() {
        // 연결 상태 변경 이벤트 구독
        // subscribe to connection state changed event
        htool.ConnectionChanged += OnConnectionChanged;

        // 현재 연결 상태 반영
        // reflect current connection state
        IsConnected = htool.Tool.ConnectionState is Connection.Connected;
        // 연결 중 상태 반영
        // reflect connecting state
        IsConnecting = htool.Tool.ConnectionState is Connection.Connecting;

        // COM 포트 목록 갱신
        // refresh COM port list
        RefreshComPorts();
    }

    /// <inheritdoc />
    public void Deactivate() {
        // 연결 상태 변경 이벤트 구독 해제
        // unsubscribe from connection state changed event
        htool.ConnectionChanged -= OnConnectionChanged;
    }

    /// <summary>
    ///     비동기 연결을 시도한다.
    ///     Attempts async connection.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync() {
        // 설정 적용 — 메시지 타임아웃
        // apply settings — message timeout
        htool.Tool.Settings.Pipeline.MessageTimeout = MessageTimeout;
        // 설정 적용 — Keep-Alive 활성화
        // apply settings — Keep-Alive enabled
        htool.Tool.Settings.KeepAlive.Enabled = KeepAliveEnabled;

        // 연결 시도 중 상태 설정
        // set connecting in progress state
        IsConnecting = true;

        // 통신 유형에 따라 연결 옵션 결정
        // determine connection option based on communication type
        var option = SelectedComType is ComType.Rtu ? SelectedBaudRate : Port;
        // 연결 대상 결정
        // determine connection target
        var target = SelectedComType is ComType.Rtu ? SelectedComPort : IpAddress;

        // 비동기 연결 시도
        // attempt async connection
        await htool.Tool.ConnectAsync(SelectedComType, target, option, SlaveId);
    }

    /// <summary>
    ///     연결 가능 여부.
    ///     Whether connection is possible.
    /// </summary>
    private bool CanConnect() {
        return !IsConnected && !IsConnecting;
    }

    /// <summary>
    ///     연결을 해제한다.
    ///     Disconnects the connection.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect() {
        // 연결 해제 호출
        // call close
        htool.Tool.Close();
    }

    /// <summary>
    ///     연결 해제 가능 여부.
    ///     Whether disconnection is possible.
    /// </summary>
    private bool CanDisconnect() {
        return IsConnected || IsConnecting;
    }

    /// <summary>
    ///     COM 포트 목록을 갱신한다.
    ///     Refreshes COM port list.
    /// </summary>
    [RelayCommand]
    private void RefreshComPorts() {
        // 시스템 COM 포트 목록 조회
        // query system COM port list
        AvailableComPorts = SerialPort.GetPortNames();
        // 첫 번째 포트 자동 선택
        // auto-select first port
        if (AvailableComPorts.Length > 0 && string.IsNullOrEmpty(SelectedComPort))
            // 첫 포트 선택
            // select first port
            SelectedComPort = AvailableComPorts[0];
    }

    /// <inheritdoc />
    partial void OnSelectedComTypeChanged(ComType value) {
        // PRO X 기본 포트 80, TCP 기본 포트 502
        // PRO X default port 80, TCP default port 502
        Port = value is ComType.Pro ? 80 : 502;
        // 커맨드 실행 가능 상태 갱신
        // refresh command can-execute state
        ConnectCommand.NotifyCanExecuteChanged();
        // 연결 해제 커맨드 상태 갱신
        // refresh disconnect command state
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     연결 상태 변경 핸들러.
    ///     Connection state changed handler.
    /// </summary>
    private void OnConnectionChanged(bool connected) {
        // UI 스레드에서 상태 갱신
        // update state on UI thread
        dispatcher.Invoke(() => {
            // 연결 상태 갱신
            // update connection state
            IsConnected = connected;
            // 연결 시도 중 상태 해제
            // clear connecting state
            IsConnecting = false;
            // 커맨드 상태 갱신
            // refresh command states
            ConnectCommand.NotifyCanExecuteChanged();
            // 연결 해제 커맨드 상태 갱신
            // refresh disconnect command state
            DisconnectCommand.NotifyCanExecuteChanged();
        });
    }
}