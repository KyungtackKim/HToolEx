using HTool.Device.Protocol;
using HTool.Format.Pro;
using HTool.Type;
using Event = HTool.Format.Process.Event;

namespace ToolSample.Services;

/// <summary>
///     HTool.HTool 싱글턴 래퍼. 모든 HTool 이벤트를 ViewModel용으로 재발행한다.
///     Singleton wrapper for HTool.HTool. Re-publishes all HTool events for ViewModel consumption.
/// </summary>
public sealed class HToolService : IDisposable {
	/// <summary>
	///     HToolService 인스턴스를 생성하고 HTool 이벤트를 구독한다.
	///     Creates HToolService instance and subscribes to HTool events.
	/// </summary>
	public HToolService() {
        // HTool 연결 상태 변경 이벤트 구독
        // subscribe to HTool connection changed event
        Tool.ChangedConnect += OnConnectionChanged;
        // HTool 4-state 연결 상태 변경 이벤트 구독
        // subscribe to HTool 4-state connection state changed event
        Tool.ConnectionStateChanged += OnConnectionStateChanged;
        // HTool 데이터 수신 이벤트 구독
        // subscribe to HTool data received event
        Tool.ReceivedData += OnDataReceived;
        // HTool 오류 수신 이벤트 구독
        // subscribe to HTool error received event
        Tool.ReceiveError += OnErrorReceived;

        // 로거 전체 카테고리 활성화
        // enable all logger categories
        Tool.Logger.EnabledCategories = LogCategories.All;
        // 로거 최소 레벨을 Debug로 설정
        // set minimum logger level to Debug
        Tool.Logger.MinLevel = LogLevel.Debug;
        // 로거 수신 이벤트 구독
        // subscribe to logger received event
        Tool.Logger.LogReceived += OnLogReceived;
    }

	/// <summary>
	///     HTool 인스턴스.
	///     HTool instance.
	/// </summary>
	public HTool.HTool Tool { get; } = new();

    /// <inheritdoc />
    public void Dispose() {
        // PRO X 이벤트 구독 해제
        // unsubscribe PRO X events
        UnsubscribeProEvents();
        // HTool 인스턴스 해제
        // dispose HTool instance
        Tool.Dispose();
    }

    /// <summary>
    ///     연결 상태 변경 이벤트 (bool).
    ///     Connection state changed event (bool).
    /// </summary>
    public event Action<bool>? ConnectionChanged;

    /// <summary>
    ///     연결 상태 변경 이벤트 (4-state).
    ///     Connection state changed event (4-state).
    /// </summary>
    public event Action<Connection>? ConnectionStateChanged;

    /// <summary>
    ///     MODBUS 응답 수신 이벤트.
    ///     MODBUS response received event.
    /// </summary>
    public event Action<ModbusResponse>? DataReceived;

    /// <summary>
    ///     통신 오류 수신 이벤트.
    ///     Communication error received event.
    /// </summary>
    public event Action<ComError>? ErrorReceived;

    /// <summary>
    ///     로그 엔트리 수신 이벤트.
    ///     Log entry received event.
    /// </summary>
    public event Action<LogEntry>? LogReceived;

    /// <summary>
    ///     PRO X 툴 이벤트 수신.
    ///     PRO X tool event received.
    /// </summary>
    public event Action<Event>? ToolEventReceived;

    /// <summary>
    ///     PRO X 잡 이벤트 수신.
    ///     PRO X job event received.
    /// </summary>
    public event Action<JobEvent>? JobEventReceived;

    /// <summary>
    ///     PRO X 멤버 툴 목록 변경.
    ///     PRO X member tools changed.
    /// </summary>
    public event Action<IReadOnlyList<ProToolInfo>>? MemberToolsChanged;

    /// <summary>
    ///     PRO X 스캔 툴 목록 변경.
    ///     PRO X scan tools changed.
    /// </summary>
    public event Action<IReadOnlyList<ProToolInfo>>? ScanToolsChanged;

    /// <summary>
    ///     PRO X 이벤트를 구독한다. ComType.Pro 연결 후 호출.
    ///     Subscribes to PRO X events. Call after ComType.Pro connection.
    /// </summary>
    public void SubscribeProEvents() {
        // ProService가 null이면 무시
        // ignore if ProService is null
        if (Tool.Pro is null)
            // PRO X 모드 아님 — 반환
            // not in PRO X mode — return
            return;

        // PRO X 툴 이벤트 구독
        // subscribe to PRO X tool event
        Tool.Pro.EventDataReceived += OnToolEventReceived;
        // PRO X 잡 이벤트 구독
        // subscribe to PRO X job event
        Tool.Pro.JobEventReceived += OnJobEventReceived;
        // PRO X 멤버 툴 변경 구독
        // subscribe to PRO X member tools changed
        Tool.Pro.MemberToolsChanged += OnMemberToolsChanged;
        // PRO X 스캔 툴 변경 구독
        // subscribe to PRO X scan tools changed
        Tool.Pro.ScanToolsChanged += OnScanToolsChanged;
    }

    /// <summary>
    ///     PRO X 이벤트 구독을 해제한다.
    ///     Unsubscribes from PRO X events.
    /// </summary>
    public void UnsubscribeProEvents() {
        // ProService가 null이면 무시
        // ignore if ProService is null
        if (Tool.Pro is null)
            // PRO X 모드 아님 — 반환
            // not in PRO X mode — return
            return;

        // PRO X 툴 이벤트 구독 해제
        // unsubscribe from PRO X tool event
        Tool.Pro.EventDataReceived -= OnToolEventReceived;
        // PRO X 잡 이벤트 구독 해제
        // unsubscribe from PRO X job event
        Tool.Pro.JobEventReceived -= OnJobEventReceived;
        // PRO X 멤버 툴 변경 구독 해제
        // unsubscribe from PRO X member tools changed
        Tool.Pro.MemberToolsChanged -= OnMemberToolsChanged;
        // PRO X 스캔 툴 변경 구독 해제
        // unsubscribe from PRO X scan tools changed
        Tool.Pro.ScanToolsChanged -= OnScanToolsChanged;
    }

    /// <summary>
    ///     4-state 연결 상태 변경 핸들러.
    ///     4-state connection state changed handler.
    /// </summary>
    private void OnConnectionStateChanged(Connection state) {
        // 구독자에게 4-state 이벤트 전달
        // deliver 4-state event to subscribers
        ConnectionStateChanged?.Invoke(state);
    }

    /// <summary>
    ///     연결 상태 변경 핸들러.
    ///     Connection state changed handler.
    /// </summary>
    private void OnConnectionChanged(bool connected) {
        // PRO X 연결 시 PRO X 이벤트 구독
        // subscribe PRO X events on PRO X connection
        if (connected && Tool.Type is ComType.Pro)
            // PRO X 이벤트 구독
            // subscribe PRO X events
            SubscribeProEvents();

        // PRO X 연결 해제 시 PRO X 이벤트 구독 해제
        // unsubscribe PRO X events on PRO X disconnection
        if (!connected)
            // PRO X 이벤트 구독 해제
            // unsubscribe PRO X events
            UnsubscribeProEvents();

        // 구독자에게 이벤트 전달
        // deliver event to subscribers
        ConnectionChanged?.Invoke(connected);
    }

    /// <summary>
    ///     데이터 수신 핸들러.
    ///     Data received handler.
    /// </summary>
    private void OnDataReceived(ModbusResponse response) {
        DataReceived?.Invoke(response);
    }

    /// <summary>
    ///     오류 수신 핸들러.
    ///     Error received handler.
    /// </summary>
    private void OnErrorReceived(ComError error) {
        ErrorReceived?.Invoke(error);
    }

    /// <summary>
    ///     로그 수신 핸들러.
    ///     Log received handler.
    /// </summary>
    private void OnLogReceived(LogEntry entry) {
        LogReceived?.Invoke(entry);
    }

    /// <summary>
    ///     PRO X 툴 이벤트 핸들러.
    ///     PRO X tool event handler.
    /// </summary>
    private void OnToolEventReceived(Event ev) {
        ToolEventReceived?.Invoke(ev);
    }

    /// <summary>
    ///     PRO X 잡 이벤트 핸들러.
    ///     PRO X job event handler.
    /// </summary>
    private void OnJobEventReceived(JobEvent ev) {
        JobEventReceived?.Invoke(ev);
    }

    /// <summary>
    ///     PRO X 멤버 툴 변경 핸들러.
    ///     PRO X member tools changed handler.
    /// </summary>
    private void OnMemberToolsChanged(IReadOnlyList<ProToolInfo> tools) {
        MemberToolsChanged?.Invoke(tools);
    }

    /// <summary>
    ///     PRO X 스캔 툴 변경 핸들러.
    ///     PRO X scan tools changed handler.
    /// </summary>
    private void OnScanToolsChanged(IReadOnlyList<ProToolInfo> tools) {
        ScanToolsChanged?.Invoke(tools);
    }
}