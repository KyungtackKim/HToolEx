using System.Timers;
using HToolEz.Device;
using HToolEz.Format;
using HToolEz.Type;
using HToolEz.Util;
using Timer = System.Timers.Timer;

namespace HToolEz;

/// <summary>
///     EZ-TORQ hantas torque meter management class
/// </summary>
public class HToolEz : IDisposable {
    /// <summary>
    ///     Connection changed delegate
    /// </summary>
    public delegate void PerformChangedConnect(bool state);

    private readonly double _emaAlpha;
    private readonly double _emaNegAlpha;

    private DateTime _connectTime;
    private double   _emaValue;
    private bool     _hasFirstValue;

    private bool _isDisposed;

    /// <summary>
    ///     Constructor
    /// </summary>
    public HToolEz(double emaAlpha = 0.6f) {
        // check valid EMA alpha
        if (emaAlpha is < 0.0f or > 1.0f)
            // set the default EMA alpha
            emaAlpha = 0.6f;
        // set the EMA alpha
        _emaAlpha    = emaAlpha;
        _emaNegAlpha = 1.0f - emaAlpha;
        // create the device
        Device = DeviceServiceFactory.Create(DeviceServiceFactory.ConnectionTypes.Serial);
        // set timer option
        ProcessTimer.Interval  =  Constants.ProcessPeriod;
        ProcessTimer.AutoReset =  true;
        ProcessTimer.Elapsed   += OnProcessTimerElapsed;
    }

    private IDeviceService Device { get; }
    private Timer ProcessTimer { get; } = new();

    private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQueue { get; } =
        KeyedQueue<FormatMessage, FormatMessage.MessageKey>.Create(static m => m.Key, capacity: 32);

    /// <summary>
    ///     Connection timeout (default: 5 seconds)
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Message timeout (default: from Constants.MessageTimeout)
    /// </summary>
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromMilliseconds(Constants.MessageTimeout);

    /// <summary>
    ///     Connection state
    /// </summary>
    public ConnectionTypes ConnectionState { get; private set; }

    /// <summary>
    ///     Device mode type (Operation or Calibration)
    /// </summary>
    public DeviceModeTypes Mode => Device.Mode;

    /// <summary>
    ///     Device body type (Integrated or Separated)
    /// </summary>
    public BodyTypes Body { get; private set; } = BodyTypes.Separated;

    /// <summary>
    ///     Device calibration data
    /// </summary>
    public FormatCalData? CalInfo { get; private set; }

    /// <summary>
    ///     Device sensor value
    /// </summary>
    public int Sensor => (int)Math.Round(_emaValue);

    /// <summary>
    ///     Dispose
    /// </summary>
    public void Dispose() {
        // check the disposed
        if (_isDisposed)
            return;
        // dispose
        Dispose(true);
        // GC suppress finalize
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Connection state changed event
    /// </summary>
    public event PerformChangedConnect? ChangedConnect;

    /// <summary>
    ///     Received data event
    /// </summary>
    public event IDeviceService.PerformData? ReceivedData;

    /// <summary>
    ///     Received torque event
    /// </summary>
    public event IDeviceService.PerformTorque? ReceivedTorque;

    /// <summary>
    ///     Connect to the device
    /// </summary>
    /// <param name="target">COM port</param>
    /// <returns>result</returns>
    public bool Connect(string target) {
        // connect async
        return ConnectAsync(target, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Connect to the device asynchronously
    /// </summary>
    /// <param name="target">COM port</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>result</returns>
    public Task<bool> ConnectAsync(string target, CancellationToken token = default) {
        // try catch
        try {
            // reset the information
            CalInfo = null;
            // reset the state
            Body           = BodyTypes.Integrated;
            _hasFirstValue = false;
            // connect to device
            if (!Device.Connect(target))
                return Task.FromResult(false);
            // clear message queue
            MessageQueue.Clear();
            // change connection state
            ConnectionState = ConnectionTypes.Connecting;
            // save connection start time
            _connectTime = DateTime.Now;
            // set event
            Device.ReceivedData   += OnDeviceReceivedData;
            Device.ReceivedTorque += OnDeviceReceivedTorque;
            // start process timer
            ProcessTimer.Start();
            // request the calibration data
            SendCommandAsync(DeviceCommandTypes.ReqCalData, DeviceHelper.CreateReqCalPacket(), token: token);
            // result
            return Task.FromResult(true);
        } catch (Exception ex) {
            // debug
            Console.WriteLine(ex);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    ///     Disconnect from the device
    /// </summary>
    public void Disconnect() {
        // disconnect async
        DisconnectAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Disconnect from the device asynchronously
    /// </summary>
    /// <param name="token">Cancellation token</param>
    public async Task DisconnectAsync(CancellationToken token = default) {
        // if in calibration mode, send terminate command
        if (Device.Mode == DeviceModeTypes.Calibration) {
            // create terminate packet
            var terminatePacket = DeviceHelper.CreateTerminatePacket();
            // send terminate command without waiting for response
            await SendCommandAsync(DeviceCommandTypes.ReqCalTerminate, terminatePacket, 0, true, token: token);
            // give a short time for the message to be sent
            await Task.Delay(50, token).ConfigureAwait(false);
        }

        // stop timer
        ProcessTimer.Stop();
        // clear message queue
        MessageQueue.Clear();
        // reset event
        Device.ReceivedData   -= OnDeviceReceivedData;
        Device.ReceivedTorque -= OnDeviceReceivedTorque;
        // disconnect
        Device.Disconnect();
        // change connection state
        ConnectionState = ConnectionTypes.Close;
        // invoke connection changed event
        ChangedConnect?.Invoke(false);
    }

    /// <summary>
    ///     Send command to device
    /// </summary>
    /// <param name="cmd">command</param>
    /// <param name="packet">packet data</param>
    /// <param name="retry">retry count</param>
    /// <param name="notCheck">not check response (send only)</param>
    /// <param name="allowDuplicate">allow duplicate message in queue</param>
    /// <returns>true if message was enqueued, false otherwise</returns>
    public bool SendCommand(DeviceCommandTypes cmd, byte[] packet, int retry = 1, bool notCheck = false, bool allowDuplicate = false) {
        // use async version synchronously
        return SendCommandAsync(cmd, packet, retry, notCheck, allowDuplicate, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Send command to device asynchronously
    /// </summary>
    /// <param name="cmd">command</param>
    /// <param name="packet">packet data</param>
    /// <param name="retry">retry count</param>
    /// <param name="notCheck">not check response (send only)</param>
    /// <param name="allowDuplicate">allow duplicate message in queue</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>true if message was enqueued, false otherwise</returns>
    public Task<bool> SendCommandAsync(
        DeviceCommandTypes cmd,
        byte[]             packet,
        int                retry          = 1,
        bool               notCheck       = false,
        bool               allowDuplicate = false,
        CancellationToken  token          = default) {
        // try catch
        try {
            // create message
            var msg = new FormatMessage(cmd, packet.AsMemory(), retry, notCheck);
            // get the unique check option
            var mode = allowDuplicate ? EnqueueMode.AllowDuplicate : EnqueueMode.EnforceUnique;
            // enqueue message
            return Task.FromResult(MessageQueue.TryEnqueue(msg, mode));
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"[HToolEz] SendCommand error: {ex.Message}");
        }

        // failed to send
        return Task.FromResult(false);
    }

    private void Dispose(bool disposing) {
        // check the disposing
        if (!disposing)
            return;

        // dispose timer resources
        try {
            // stop process timer
            ProcessTimer.Stop();
            // reset event
            ProcessTimer.Elapsed -= OnProcessTimerElapsed;
            // dispose timer
            ProcessTimer.Dispose();
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"[HToolEz] Timer disposal error: {ex.Message}");
        }

        // dispose device resources
        try {
            // disconnect device
            Device.Disconnect();
            // dispose device
            Device.Dispose();
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"[HToolEz] Device disposal error: {ex.Message}");
        }

        // set the disposed
        _isDisposed = true;
    }

    private void OnProcessTimerElapsed(object? sender, ElapsedEventArgs e) {
        // try catch
        try {
            // get current time once for efficiency
            var now = DateTime.Now;

            // check the connecting state
            if (ConnectionState == ConnectionTypes.Connecting)
                // check connection timeout
                if (now - _connectTime > ConnectionTimeout) {
                    // debug log
                    Console.WriteLine("[HToolEz] Connection timeout - disconnecting");
                    // disconnect
                    Disconnect();
                    // end of connecting
                    return;
                }

            // check the empty
            if (MessageQueue.IsEmpty)
                return;
            // peek the message
            if (!MessageQueue.TryPeek(out var msg))
                return;
            // check activated state
            if (!msg.Activated) {
                // check the command
                if (msg.Command == DeviceCommandTypes.ReqCalData)
                    // set the calibration mode
                    Device.Mode = DeviceModeTypes.Calibration;
                // get packet values
                var packet = msg.Packet;
                // write packet
                if (!Device.Write(packet))
                    return;
                // set activate
                msg.Activate();
                // check the command
                if (msg.Command == DeviceCommandTypes.ReqCalTerminate)
                    // reset the operation mode
                    Device.Mode = DeviceModeTypes.Operation;
                // check the not check
                if (!msg.NotCheck)
                    return;
            } else {
                // check timeout
                if (now - msg.ActiveTime < MessageTimeout)
                    return;
                // deactivate message
                if (msg.Deactivate() > 0)
                    return;
            }

            // try dequeue (remove the message)
            MessageQueue.TryDequeue(out _);
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"[HToolEz] OnProcessTimerElapsed error: {ex.Message}");
        }
    }

    private void OnDeviceReceivedData(DeviceCommandTypes cmd, ReadOnlyMemory<byte> data) {
        // try catch
        try {
            // set connected state on any response
            if (ConnectionState == ConnectionTypes.Connecting) {
                // create terminate packet
                var terminatePacket = DeviceHelper.CreateTerminatePacket();
                // send terminate command without waiting for response
                SendCommand(DeviceCommandTypes.ReqCalTerminate, terminatePacket, 0, true);
                // set the connected
                ConnectionState = ConnectionTypes.Connected;
                // invoke connection changed event
                ChangedConnect?.Invoke(true);
            }

            // peek the message
            if (MessageQueue.TryPeek(out var msg))
                // check queue
                if (msg is { Activated: true })
                    // check command (response is request + 0x80)
                    if ((byte)cmd == ((byte)msg.Command | 0x80))
                        // try dequeue
                        MessageQueue.TryDequeue(out _);

            // check the command
            switch (cmd) {
                // detect body type from RES_CAL_DATA (0x80) response
                case DeviceCommandTypes.ResCalData when data.Length > 5: {
                    // check the information state
                    CalInfo ??= new FormatCalData(data[5..]);
                    // check the body type
                    if (CalInfo.Body != Body) {
                        // set the body type
                        Body = CalInfo.Body;
                        // debug long
                        Console.WriteLine($"[HToolEz] Body type detected: {CalInfo.Body}");
                    }

                    break;
                }
                // detect sensor value from REP_CURRENT_ADC (0xA0)
                case DeviceCommandTypes.RepAdc:
                    var value = 0;
                    var isValidRange = true;
                    // check the mode
                    if (Body == BodyTypes.Separated) {
                        // convert to sensor value (integer-32bit)
                        Utils.ConvertValue(data.Span[5..], out int v);
                        // check the value range
                        if (v is < -131072 or > 131072) {
                            // out of range - skip EMA update
                            isValidRange = false;
                        } else {
                            value = v;
                        }
                    } else {
                        // convert to sensor value (ushort-16bit)
                        Utils.ConvertValue(data.Span[5..], out ushort v);
                        // set the value
                        value = v;
                    }

                    // update EMA only for valid range
                    if (isValidRange) {
                        // check first value
                        if (!_hasFirstValue) {
                            // set value
                            _emaValue = value;
                            // set first value
                            _hasFirstValue = true;
                        } else {
                            // set value
                            _emaValue = value * _emaAlpha + _emaValue * _emaNegAlpha;
                        }
                    }

                    break;
                case DeviceCommandTypes.ReqCalData:
                case DeviceCommandTypes.ReqCalSetPoint:
                case DeviceCommandTypes.ReqCalSave:
                case DeviceCommandTypes.ReqCalTerminate:
                case DeviceCommandTypes.ReqSetData:
                case DeviceCommandTypes.ReqTorque:
                case DeviceCommandTypes.ResCalSetPoint:
                case DeviceCommandTypes.ResCalSave:
                case DeviceCommandTypes.ResSetData:
                case DeviceCommandTypes.ResTorque:
                default:
                    break;
            }

            // invoke received data event
            ReceivedData?.Invoke(cmd, data);
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"[HToolEz] OnDeviceReceivedData error: {ex.Message}");
        }
    }

    private void OnDeviceReceivedTorque(float torque, UnitTypes unit) {
        // invoke received torque event
        ReceivedTorque?.Invoke(torque, unit);
    }
}