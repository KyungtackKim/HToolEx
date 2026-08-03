using HTool.Device.Protocol;
using HTool.Type;

namespace ToolRunner.Services;

/// <summary>
///     singleton wrapper around a single <see cref="HTool.HTool" /> instance fixed to direct
///     MODBUS-TCP. re-publishes the library events so view models never subscribe to the tool
///     directly and unsubscription stays in one place.
///     HTool(TCP 직접 연결) 단일 인스턴스 래퍼
/// </summary>
public sealed class HToolService : IDisposable {
    /// <summary>
    ///     creates the TCP-mode tool instance and subscribes to every library event.
    ///     TCP 모드 인스턴스 생성 및 이벤트 구독
    /// </summary>
    public HToolService() {
        // connection established / lost
        Tool.ChangedConnect += OnConnectionChanged;
        // four-state connection transitions (Close / Closed / Connecting / Connected)
        Tool.ConnectionStateChanged += OnConnectionStateChanged;
        // successful MODBUS responses, including the echo of every write
        Tool.ReceivedData += OnDataReceived;
        // timeouts and protocol errors
        Tool.ReceiveError += OnErrorReceived;

        // keep only the categories worth showing in a control tester — packet dumps would flood
        Tool.Logger.EnabledCategories = LogCategories.Connection | LogCategories.KeepAlive | LogCategories.Error;
        // information level is enough; Debug would add per-request pipeline noise
        Tool.Logger.MinLevel = LogLevel.Info;
        // forward library log entries to the on-screen log
        Tool.Logger.LogReceived += OnLogReceived;
    }

    /// <summary>
    ///     the wrapped tool instance, fixed to <see cref="ComType.Tcp" /> at construction so the
    ///     tester can never accidentally open a serial port.
    ///     TCP 모드로 고정된 HTool 인스턴스
    /// </summary>
    public HTool.HTool Tool { get; } = new(ComType.Tcp);

    /// <inheritdoc />
    public void Dispose() {
        // dropping the tool closes the socket and stops the pipeline timers
        Tool.Dispose();
    }

    /// <summary>
    ///     connection established (true) or lost (false).
    ///     연결 성립/해제 이벤트
    /// </summary>
    public event Action<bool>? ConnectionChanged;

    /// <summary>
    ///     four-state connection transition, used to drive the status-bar indicator.
    ///     4-state 연결 상태 전이 이벤트
    /// </summary>
    public event Action<Connection>? ConnectionStateChanged;

    /// <summary>
    ///     a MODBUS response arrived — for this tester that is mostly the echo of a control write.
    ///     MODBUS 응답 수신 이벤트
    /// </summary>
    public event Action<ModbusResponse>? DataReceived;

    /// <summary>
    ///     a communication error occurred (timeout, protocol exception, socket failure).
    ///     통신 오류 이벤트
    /// </summary>
    public event Action<ComError>? ErrorReceived;

    /// <summary>
    ///     a library log entry was produced by one of the enabled categories.
    ///     라이브러리 로그 수신 이벤트
    /// </summary>
    public event Action<LogEntry>? LogReceived;

    /// <summary>
    ///     connection state changed handler — forwards the boolean form.
    /// </summary>
    /// <param name="connected">true when the tool is connected</param>
    private void OnConnectionChanged(bool connected) {
        // deliver to subscribers
        ConnectionChanged?.Invoke(connected);
    }

    /// <summary>
    ///     four-state connection handler — forwards the detailed state.
    /// </summary>
    /// <param name="state">new connection state</param>
    private void OnConnectionStateChanged(Connection state) {
        // deliver to subscribers
        ConnectionStateChanged?.Invoke(state);
    }

    /// <summary>
    ///     MODBUS response handler — forwards the decoded response.
    /// </summary>
    /// <param name="response">decoded MODBUS response</param>
    private void OnDataReceived(ModbusResponse response) {
        // deliver to subscribers
        DataReceived?.Invoke(response);
    }

    /// <summary>
    ///     communication error handler — forwards the error detail.
    /// </summary>
    /// <param name="error">communication error detail</param>
    private void OnErrorReceived(ComError error) {
        // deliver to subscribers
        ErrorReceived?.Invoke(error);
    }

    /// <summary>
    ///     library log handler — forwards the log entry.
    /// </summary>
    /// <param name="entry">library log entry</param>
    private void OnLogReceived(LogEntry entry) {
        // deliver to subscribers
        LogReceived?.Invoke(entry);
    }
}
