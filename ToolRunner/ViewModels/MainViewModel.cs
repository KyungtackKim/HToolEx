using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HTool.Device.Protocol;
using HTool.Type;
using ToolRunner.Models;
using ToolRunner.Services;

namespace ToolRunner.ViewModels;

/// <summary>
///     the single view model behind the runner window. owns the connection settings, the controller
///     Information block shown in the tool panel, the four sequence delays, the manual control
///     commands and the activity log.
///     러너 창 전체를 담당하는 뷰모델
/// </summary>
/// <param name="htool">tool wrapper providing the connection and MODBUS traffic</param>
/// <param name="runner">sequence runner driving the repeating control cycle</param>
/// <param name="settingsStore">store that keeps the operator input across restarts</param>
/// <param name="dispatcher">UI-thread dispatcher for events raised on background threads</param>
public sealed partial class MainViewModel(
    HToolService       htool,
    SequenceRunner     runner,
    SettingsStore      settingsStore,
    IDispatcherService dispatcher) : ObservableObject {
    // placeholder shown wherever no value has been read yet
    private const string Empty = "---";

    // maximum number of log lines kept in memory
    private const int MaxLogs = 2000;

    // smallest accepted run duration — keeps an all-zero configuration from spinning the loop
    private const int MinRunDurationMs = 10;

    // MODBUS response wait timeout, fixed rather than operator-tunable: a link that cannot be
    // established is reported as an error instead of being nursed with a longer deadline
    private const int ResponseTimeoutMs = 1000;

    // how long the motor runs in each counter-clockwise (loosening) cycle (ms)
    [ObservableProperty]
    private int _ccwRunDurationMs = SequenceSettings.Default.CcwRunDurationMs;

    // how long the motor runs in each clockwise (fastening) cycle (ms)
    [ObservableProperty]
    private int _cwRunDurationMs = SequenceSettings.Default.CwRunDurationMs;

    // four-state connection status driving the status-bar indicator
    [ObservableProperty]
    private Connection _connectionState = Connection.Closed;

    // completed cycle count of the active sequence
    [ObservableProperty]
    private int _cycleCount;

    // rotation direction currently written to the controller
    [ObservableProperty]
    private string _directionText = Empty;

    // driver ID (30002)
    [ObservableProperty]
    private string _driverIdText = Empty;

    // driver model name and number (30003–30019)
    [ObservableProperty]
    private string _driverModelText = Empty;

    // driver serial number (30020–30024)
    [ObservableProperty]
    private string _driverSerialText = Empty;

    // last communication error shown in the status bar
    [ObservableProperty]
    private string _errorText = "";

    // controller firmware version (30047–30049)
    [ObservableProperty]
    private string _firmwareText = Empty;

    // direction written once before the first run command
    [ObservableProperty]
    private RunDirection _initialDirection = SequenceSettings.Default.InitialDirection;

    // target IP address of the tool controller
    [ObservableProperty]
    private string _ipAddress = "192.168.1.100";

    // whether the tool is connected
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshToolInformationCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartSequenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ManualRunCommand))]
    [NotifyCanExecuteChangedFor(nameof(ManualStopCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetDirectionCommand))]
    private bool _isConnected;

    // whether a connection attempt is in flight
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    private bool _isConnecting;

    // whether the tool information and log panel is shown next to the control column — the window
    // starts collapsed so the default form is the control-only view
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailsToggleText))]
    private bool _isDetailsVisible;

    // whether the repeating sequence is running
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSequenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopSequenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(ManualRunCommand))]
    [NotifyCanExecuteChangedFor(nameof(ManualStopCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetDirectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    private bool _isSequenceRunning;

    // whether keep-alive polling is enabled before connecting
    [ObservableProperty]
    private bool _keepAliveEnabled = true;

    // controller MAC address (30053–30055)
    [ObservableProperty]
    private string _macAddressText = Empty;

    // target MODBUS-TCP port of the tool controller — HANTAS controllers listen on 5000, not the
    // MODBUS standard 502
    [ObservableProperty]
    private int _port = 5000;

    // MODBUS slave id of the tool controller
    [ObservableProperty]
    private byte _slaveId = 1;

    // step the sequence is currently in
    [ObservableProperty]
    private string _stepText = nameof(SequenceStep.Idle);

    // wait inserted before every run command — after Start and after each direction switch (ms)
    [ObservableProperty]
    private int _waitBeforeRunMs = SequenceSettings.Default.WaitBeforeRunMs;

    /// <summary>
    ///     activity log shown in the log list — newest entry last, capped at
    ///     <see cref="MaxLogs" /> lines.
    ///     활동 로그 목록
    /// </summary>
    public ObservableCollection<LogRecord> Logs { get; } = [];

    /// <summary>
    ///     the directions offered in the starting-direction selector.
    ///     시작 방향 선택 목록
    /// </summary>
    public RunDirection[] Directions => [RunDirection.Cw, RunDirection.Ccw];

    /// <summary>
    ///     application version shown at the right end of the status bar. read from the assembly so
    ///     the csproj <c>&lt;Version&gt;</c> tag stays the single place the version is maintained.
    ///     상태바 우측에 표시되는 프로그램 버전 (csproj Version 태그가 유일한 출처)
    /// </summary>
    public string VersionText { get; } =
        $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?"}";

    /// <summary>
    ///     caption of the panel toggle — states the action the click performs, not the current
    ///     state, so the button reads as a command.
    ///     패널 토글 버튼 캡션
    /// </summary>
    public string DetailsToggleText => IsDetailsVisible
        ? "◀ Hide info / log"
        : "Show info / log ▶";

    /// <summary>
    ///     subscribes to the tool and runner events. call once when the window has loaded.
    ///     이벤트 구독 (창 로드 시 1회 호출)
    /// </summary>
    public void Initialize() {
        // connection transitions drive the status bar and the Information request
        htool.ConnectionStateChanged += OnToolConnectionStateChanged;
        // communication errors are surfaced in the status bar and the log
        htool.ErrorReceived += OnErrorReceived;
        // library log entries (connection, keep-alive, error) go to the on-screen log
        htool.LogReceived += OnLogReceived;
        // responses carry both the Information block and the control write echoes
        htool.DataReceived += OnDataReceived;
        // sequence step transitions drive the cycle / step / direction display
        runner.StateChanged += OnSequenceStateChanged;
        // sequence command and failure messages go to the on-screen log
        runner.Logged += OnSequenceLogged;

        // record the startup line so the log is never empty on first open
        AddLog("app", "ready — enter the controller address and connect");

        // restore the connection target, timings and panel state saved by the previous run
        ApplyStoredSettings();
    }

    /// <summary>
    ///     applies the settings saved by the previous run, rejecting values that would break a
    ///     connection attempt. must be called on the UI thread.
    /// </summary>
    private void ApplyStoredSettings() {
        // read whatever was stored — defaults on first run or after a corrupt file
        var stored = settingsStore.Load();

        // a blank stored address would leave the field empty, so fall back to the default
        IpAddress = string.IsNullOrWhiteSpace(stored.IpAddress) ? "192.168.1.100" : stored.IpAddress;
        // reject a port outside the TCP range
        Port = stored.Port is > 0 and <= 65535 ? stored.Port : 5000;
        // the slave id is a byte, so every stored value is already in range
        SlaveId = stored.SlaveId;
        // keep-alive flag
        KeepAliveEnabled = stored.KeepAliveEnabled;

        // cycle timings — BuildSettings clamps them again, so restore them as stored
        WaitBeforeRunMs = stored.WaitBeforeRunMs;
        // clockwise run duration
        CwRunDurationMs = stored.CwRunDurationMs;
        // counter-clockwise run duration
        CcwRunDurationMs = stored.CcwRunDurationMs;
        // starting direction
        InitialDirection = stored.InitialDirection;

        // restore the panel state last — the window resizes itself in response
        IsDetailsVisible = stored.IsDetailsVisible;
    }

    /// <summary>
    ///     snapshots the current input into <see cref="AppSettings" /> for persistence.
    /// </summary>
    /// <returns>settings to store</returns>
    private AppSettings CaptureSettings() {
        // mirror every persisted field from the live view model state
        return new AppSettings {
            IpAddress = IpAddress,
            Port = Port,
            SlaveId = SlaveId,
            KeepAliveEnabled = KeepAliveEnabled,
            WaitBeforeRunMs = WaitBeforeRunMs,
            CwRunDurationMs = CwRunDurationMs,
            CcwRunDurationMs = CcwRunDurationMs,
            InitialDirection = InitialDirection,
            IsDetailsVisible = IsDetailsVisible
        };
    }

    /// <summary>
    ///     writes the current input to disk, reporting a failed save in the log.
    /// </summary>
    private void PersistSettings() {
        // store the snapshot; a false return means the file could not be written
        if (!settingsStore.Save(CaptureSettings()))
            // surface the failure so silently lost settings are explainable
            AddLog("warn", $"settings could not be saved to {settingsStore.FilePath}");
    }

    /// <summary>
    ///     stops the sequence, closes the connection and unsubscribes every event. awaited by the
    ///     window on close so the motor is never left turning behind a closed window.
    ///     시퀀스 정지·연결 해제·구독 해제 (창 종료 시 await)
    /// </summary>
    /// <returns>task completing when the tool has been stopped and closed</returns>
    public async Task ShutdownAsync() {
        // keep the current input for the next run before anything is torn down
        PersistSettings();

        // stop the cycle loop and wait for its final stop command
        await runner.StopAsync();

        // close the socket so the controller sees a clean disconnect
        htool.Tool.Close();

        // connection transitions no longer needed
        htool.ConnectionStateChanged -= OnToolConnectionStateChanged;
        // error events no longer needed
        htool.ErrorReceived -= OnErrorReceived;
        // library log events no longer needed
        htool.LogReceived -= OnLogReceived;
        // response events no longer needed
        htool.DataReceived -= OnDataReceived;
        // sequence step events no longer needed
        runner.StateChanged -= OnSequenceStateChanged;
        // sequence log events no longer needed
        runner.Logged -= OnSequenceLogged;
    }

    /// <summary>
    ///     opens the MODBUS-TCP connection to the controller.
    ///     컨트롤러 TCP 연결
    /// </summary>
    /// <returns>task completing when the connection attempt has been made</returns>
    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync() {
        // apply the fixed response timeout before the pipeline starts
        htool.Tool.Settings.Pipeline.MessageTimeout = ResponseTimeoutMs;
        // apply keep-alive so a silently dropped link is detected
        htool.Tool.Settings.KeepAlive.Enabled = KeepAliveEnabled;

        // mark the attempt in progress so the buttons disable
        IsConnecting = true;
        // clear the previous error
        ErrorText = "";
        // record the attempt
        AddLog("connection", $"connecting to {IpAddress}:{Port} (slave {SlaveId}) ...");

        // guard: an invalid address or a refused connection must not fault the command
        try {
            // open the connection — the library reports Connected once the link is confirmed
            var started = await htool.Tool.ConnectAsync(IpAddress, Port, SlaveId);

            // a false return means the link could not be established (refused, unreachable, bad state)
            if (!started) {
                // surface the failure in the status bar
                ErrorText = $"connection to {IpAddress}:{Port} failed";
                // record the failure
                AddLog("error", ErrorText);
                // clear the in-progress flag so the button re-enables immediately
                IsConnecting = false;
            }
        } catch (Exception ex) {
            // socket failure, bad address, or a timeout confirming the link
            ErrorText = ex.Message;
            // record the failure
            AddLog("error", $"connect failed: {ex.Message}");
            // clear the in-progress flag so the button re-enables
            IsConnecting = false;
        }
    }

    /// <summary>
    ///     whether a connection attempt can be started.
    /// </summary>
    /// <returns>true when the tool is neither connected nor connecting</returns>
    private bool CanConnect() {
        // connecting is only possible from a fully closed state
        return !IsConnected && !IsConnecting;
    }

    /// <summary>
    ///     closes the connection to the controller.
    ///     연결 해제
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect() {
        // record the request
        AddLog("connection", "disconnecting ...");
        // close the socket — the library raises the state transition
        htool.Tool.Close();
    }

    /// <summary>
    ///     whether the connection can be closed.
    /// </summary>
    /// <returns>true when connected or connecting and no sequence is running</returns>
    private bool CanDisconnect() {
        // block the disconnect while the sequence still drives the motor
        return (IsConnected || IsConnecting) && !IsSequenceRunning;
    }

    /// <summary>
    ///     re-reads the controller Information block, for when the panel needs refreshing without
    ///     reconnecting.
    ///     장치 정보 다시 읽기
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefreshToolInformation))]
    private void RefreshToolInformation() {
        // issue the FC 0x04 read
        RequestToolInformation();
    }

    /// <summary>
    ///     whether the Information block can be re-read.
    /// </summary>
    /// <returns>true while connected</returns>
    private bool CanRefreshToolInformation() {
        // reading requires a live connection
        return IsConnected;
    }

    /// <summary>
    ///     starts the repeating run / stop / reverse cycle with the configured delays.
    ///     반복 시퀀스 시작
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartSequence))]
    private void StartSequence() {
        // build the immutable settings snapshot the loop runs against
        var settings = BuildSettings();

        // record the timings so the log explains what the cycle is doing
        AddLog("sequence",
            $"start — wait before run {settings.WaitBeforeRunMs} ms, CW run {settings.CwRunDurationMs} ms, " +
            $"CCW run {settings.CcwRunDurationMs} ms, initial {settings.InitialDirection}");

        // save the timings now as well as on exit, so a crash mid-run cannot lose them
        PersistSettings();

        // mark the sequence running so the manual controls disable
        IsSequenceRunning = true;
        // launch the loop
        runner.Start(settings);
    }

    /// <summary>
    ///     whether the sequence can be started.
    /// </summary>
    /// <returns>true when connected and no sequence is running</returns>
    private bool CanStartSequence() {
        // a sequence needs a live connection and no loop already running
        return IsConnected && !IsSequenceRunning;
    }

    /// <summary>
    ///     stops the repeating cycle and waits until the final stop command has been issued.
    ///     반복 시퀀스 정지 (최종 정지 명령까지 대기)
    /// </summary>
    /// <returns>task completing when the loop has fully unwound</returns>
    [RelayCommand(CanExecute = nameof(CanStopSequence))]
    private async Task StopSequenceAsync() {
        // record the request
        AddLog("sequence", "stop requested by operator");
        // cancel the loop and wait for its stop-and-cleanup path
        await runner.StopAsync();
        // clear the running flag so the manual controls re-enable
        IsSequenceRunning = false;
    }

    /// <summary>
    ///     whether the sequence can be stopped.
    /// </summary>
    /// <returns>true while a sequence is running</returns>
    private bool CanStopSequence() {
        // stopping only makes sense while the loop runs
        return IsSequenceRunning;
    }

    /// <summary>
    ///     issues a single run command for manual verification.
    ///     수동 Run 명령
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanControlManually))]
    private void ManualRun() {
        // delegate to the runner so the write is logged the same way as a sequence write
        runner.ManualRun();
    }

    /// <summary>
    ///     issues a single stop command for manual verification.
    ///     수동 Stop 명령
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanControlManually))]
    private void ManualStop() {
        // delegate to the runner so the write is logged the same way as a sequence write
        runner.ManualStop();
    }

    /// <summary>
    ///     switches the rotation direction for manual verification.
    ///     수동 방향 전환
    /// </summary>
    /// <param name="direction">direction to write to the controller</param>
    [RelayCommand(CanExecute = nameof(CanControlManually))]
    private void SetDirection(RunDirection direction) {
        // delegate to the runner so the current direction display stays in sync
        runner.ManualDirection(direction);
    }

    /// <summary>
    ///     whether the manual control commands are available.
    /// </summary>
    /// <returns>true when connected and the sequence is not driving the tool</returns>
    private bool CanControlManually() {
        // manual writes would fight the loop, so they are only allowed while it is idle
        return IsConnected && !IsSequenceRunning;
    }

    /// <summary>
    ///     shows or hides the tool information and log panel, so the window can be reduced to the
    ///     control column when the operator only wants to drive the tool.
    ///     툴 정보·로그 패널 접기/펼치기
    /// </summary>
    [RelayCommand]
    private void ToggleDetails() {
        // flip the panel state — the window resizes itself to match
        IsDetailsVisible = !IsDetailsVisible;
    }

    /// <summary>
    ///     clears the activity log.
    ///     로그 지우기
    /// </summary>
    [RelayCommand]
    private void ClearLog() {
        // drop every entry
        Logs.Clear();
    }

    /// <summary>
    ///     snapshots the current delay inputs into an immutable settings value, clamping negative
    ///     numbers to zero and forcing a minimum run duration.
    /// </summary>
    /// <returns>settings the sequence loop runs against</returns>
    private SequenceSettings BuildSettings() {
        // a negative wait makes no sense — treat it as "no wait"
        var wait = Math.Max(0, WaitBeforeRunMs);
        // both run durations carry the tight-loop floor
        var cwRun = Math.Max(MinRunDurationMs, CwRunDurationMs);
        // clamp the counter-clockwise run duration the same way
        var ccwRun = Math.Max(MinRunDurationMs, CcwRunDurationMs);

        // hand the clamped values to the loop
        return new SequenceSettings(wait, cwRun, ccwRun, InitialDirection);
    }

    /// <summary>
    ///     enqueues the FC 0x04 read of the controller Information block.
    /// </summary>
    private void RequestToolInformation() {
        // read the Information block starting at the documented address
        var queued = htool.Tool.ReadInputReg(ToolInformation.StartAddress, ToolInformation.RegisterCount);

        // a false return means the pipeline refused the request
        if (!queued) {
            // explain why the panel stays empty
            AddLog("warn", $"device information read rejected (FC 0x04, addr {ToolInformation.StartAddress})");
            // nothing more to do for a rejected read
            return;
        }

        // record the request
        AddLog("modbus",
            $"reading device information — FC 0x04, addr {ToolInformation.StartAddress}, " +
            $"{ToolInformation.RegisterCount} regs");
    }

    /// <summary>
    ///     fills the tool panel from a parsed Information block. must be called on the UI thread.
    /// </summary>
    /// <param name="payload">FC 0x04 response payload</param>
    private void ApplyToolInformation(byte[] payload) {
        // a short or malformed block leaves the panel as it was
        if (!ToolInformation.TryParse(payload, out var info)) {
            // report the unusable payload
            AddLog("warn", $"device information payload unusable ({payload.Length} of {ToolInformation.Size} bytes)");
            // nothing to display
            return;
        }

        // driver ID
        DriverIdText = info.DriverId.ToString();
        // driver model name and number
        DriverModelText = info.DriverText;
        // driver serial number
        DriverSerialText = info.DriverSerialNumber;
        // controller firmware version
        FirmwareText = info.FirmwareText;
        // controller MAC address
        MacAddressText = info.MacAddressText;

        // record the identity that was read
        AddLog("connection",
            $"device information — driver {info.DriverText} ID {info.DriverId} " +
            $"S/N {info.DriverSerialNumber}, FW {info.FirmwareText}");
    }

    /// <summary>
    ///     clears every field of the tool panel. must be called on the UI thread.
    /// </summary>
    private void ResetToolInformation() {
        // clear the driver ID
        DriverIdText = Empty;
        // clear the driver model
        DriverModelText = Empty;
        // clear the driver serial number
        DriverSerialText = Empty;
        // clear the firmware version
        FirmwareText = Empty;
        // clear the MAC address
        MacAddressText = Empty;
    }

    /// <summary>
    ///     four-state connection handler — refreshes the status bar and requests or clears the
    ///     controller Information block.
    /// </summary>
    /// <param name="state">new connection state</param>
    private void OnToolConnectionStateChanged(Connection state) {
        // the library raises this from a socket / pipeline thread
        dispatcher.Invoke(() => {
            // publish the state for the status-bar indicator
            ConnectionState = state;
            // connected drives the command availability
            IsConnected = state is Connection.Connected;
            // the attempt is over once the state settles either way
            IsConnecting = state is Connection.Connecting;

            // read the Information block as soon as the link is up
            if (state is Connection.Connected) {
                // a confirmed link clears whatever error the previous attempt left behind
                ErrorText = "";
                // record the successful connection
                AddLog("connection", $"connected to {IpAddress}:{Port}");
                // request the identity shown in the tool panel
                RequestToolInformation();
            }

            // clear the panel once the link is fully down
            if (state is Connection.Closed) {
                // drop the stale identity
                ResetToolInformation();
                // record the disconnect
                AddLog("connection", "disconnected");
            }
        });
    }

    /// <summary>
    ///     communication error handler — shows the reason in the status bar and the log.
    /// </summary>
    /// <param name="error">communication error detail</param>
    private void OnErrorReceived(ComError error) {
        // the library raises this from a pipeline thread
        dispatcher.Invoke(() => {
            // publish the reason for the status bar
            ErrorText = $"{error.Reason}: {error.Detail}";
            // record the error in the log
            AddLog("error", ErrorText);

            // an error while the link is still unconfirmed means the connection never came up —
            // abandon it instead of sitting in Connecting, so the operator can correct the address
            // and press Connect again right away
            if (ConnectionState is Connection.Connecting) {
                // report that the attempt is being given up
                AddLog("connection", "connection attempt abandoned");
                // close the half-open link; the resulting transition re-enables Connect
                htool.Tool.Close();
            }
        });
    }

    /// <summary>
    ///     library log handler — forwards the entry to the on-screen log.
    /// </summary>
    /// <param name="entry">library log entry</param>
    private void OnLogReceived(LogEntry entry) {
        // the library raises this from arbitrary threads
        dispatcher.Invoke(() => {
            // record the entry under its library category
            AddLog(entry.Category.ToString().ToLowerInvariant(), entry.Message);
        });
    }

    /// <summary>
    ///     MODBUS response handler — routes the Information block to the tool panel, logs the echoed
    ///     value that confirms a control write was applied, reports the direction read-back, and
    ///     surfaces exception responses instead of dropping them.
    /// </summary>
    /// <param name="response">decoded MODBUS response</param>
    private void OnDataReceived(ModbusResponse response) {
        // the Information block arrives as an FC 0x04 read of the documented start address
        if (response.Code is FunctionCode.ReadInputReg && response.Address == ToolInformation.StartAddress) {
            // copy the payload out of the pooled pipeline buffer before leaving this thread
            var payload = response.Payload.ToArray();
            // apply the block on the UI thread
            dispatcher.Invoke(() => ApplyToolInformation(payload));
            // the response is handled
            return;
        }

        // an exception response means the controller refused the request outright
        if (response.Code is FunctionCode.Error) {
            // the payload carries the single MODBUS exception code
            var exception = ReadTrailingByte(response.Payload.Span);
            // the library raises this from a pipeline thread
            dispatcher.Invoke(() => {
                // record the refusal against the register the request targeted
                AddLog("error", $"controller refused reg {response.Address} — MODBUS exception 0x{exception:X2}");
            });
            // the response is handled
            return;
        }

        // a holding-register read of the direction register is the per-cycle read-back
        if (response.Code is FunctionCode.ReadHoldingReg && response.Address == ControlRegisters.Direction) {
            // pull the register value out of the payload
            var latched = ReadTrailingUInt16(response.Payload.Span);
            // the library raises this from a pipeline thread
            dispatcher.Invoke(() => {
                // record what the controller actually has latched after the switch
                AddLog("readback", $"reg {ControlRegisters.Direction} = {latched} ({DirectionNameOf(latched)})");
            });
            // the response is handled
            return;
        }

        // everything else is only interesting when it echoes a control write
        if (response.Code is not FunctionCode.WriteSingleReg)
            // not a control write echo — ignore
            return;

        // the FC 0x06 echo repeats the value the controller accepted
        var echoed = ReadTrailingUInt16(response.Payload.Span);

        // the library raises this from a pipeline thread
        dispatcher.Invoke(() => {
            // record the confirmed write together with the echoed value
            AddLog("ack", $"echo — reg {response.Address} = {echoed} accepted");
        });
    }

    /// <summary>
    ///     reads the big-endian 16-bit value at the end of a response payload. taking the trailing
    ///     bytes keeps the read correct whether or not the frame carried a leading byte-count field.
    /// </summary>
    /// <param name="payload">response payload</param>
    /// <returns>the trailing register value, or zero when the payload is too short</returns>
    private static ushort ReadTrailingUInt16(ReadOnlySpan<byte> payload) {
        // a payload shorter than one register cannot carry a value
        if (payload.Length < 2)
            // nothing to read
            return 0;

        // combine the last two bytes big-endian
        return (ushort)((payload[^2] << 8) | payload[^1]);
    }

    /// <summary>
    ///     reads the last byte of a response payload, used for the MODBUS exception code.
    /// </summary>
    /// <param name="payload">response payload</param>
    /// <returns>the trailing byte, or zero when the payload is empty</returns>
    private static byte ReadTrailingByte(ReadOnlySpan<byte> payload) {
        // an empty payload carries no code
        return payload.Length > 0 ? payload[^1] : (byte)0;
    }

    /// <summary>
    ///     names a raw direction register value for the log.
    /// </summary>
    /// <param name="value">value read back from the direction register</param>
    /// <returns>short direction label</returns>
    private static string DirectionNameOf(ushort value) {
        // the register value maps straight onto the direction enum
        return value == (ushort)RunDirection.Ccw ? "CCW" : "CW";
    }

    /// <summary>
    ///     sequence step handler — refreshes the cycle, step and direction display.
    /// </summary>
    /// <param name="step">step the loop entered</param>
    /// <param name="cycle">cycle number in progress</param>
    /// <param name="direction">direction currently written to the controller</param>
    private void OnSequenceStateChanged(SequenceStep step, int cycle, RunDirection direction) {
        // the runner raises this from the loop thread
        dispatcher.Invoke(() => {
            // publish the step text
            StepText = step.ToString();
            // publish the cycle count
            CycleCount = cycle;
            // publish the direction
            DirectionText = direction.ToString().ToUpperInvariant();
        });
    }

    /// <summary>
    ///     sequence log handler — forwards runner messages to the on-screen log.
    /// </summary>
    /// <param name="category">runner log category</param>
    /// <param name="message">runner log message</param>
    private void OnSequenceLogged(string category, string message) {
        // the runner raises this from the loop thread
        dispatcher.Invoke(() => {
            // record the runner message
            AddLog(category, message);
        });
    }

    /// <summary>
    ///     appends a log entry and trims the collection to <see cref="MaxLogs" /> lines. must be
    ///     called on the UI thread — handlers for background events dispatch before calling it.
    ///     로그 추가 (UI 스레드에서만 호출)
    /// </summary>
    /// <param name="category">source tag</param>
    /// <param name="message">message body</param>
    private void AddLog(string category, string message) {
        // append the timestamped entry
        Logs.Add(new LogRecord(DateTime.Now, category, message));

        // drop the oldest entries once the cap is exceeded
        while (Logs.Count > MaxLogs)
            // remove from the front so the newest lines survive
            Logs.RemoveAt(0);
    }
}
