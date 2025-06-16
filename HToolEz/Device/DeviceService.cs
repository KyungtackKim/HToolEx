using System.Diagnostics;
using System.Timers;
using HToolEz.Defines.Entities;
using HToolEz.Defines.Enums;
using HToolEz.Utils;
using Timer = System.Timers.Timer;

namespace HToolEz.Device;

/// <summary>
///     Device service class
/// </summary>
public sealed class DeviceService : IAsyncDisposable {
    private const int MessageTimeout = 1000;
    private readonly IDeviceConnection _comm;
    private readonly IDeviceController _con;
    private readonly CancellationTokenSource _messageCts = new();
    private readonly Timer _processTimer = new();
    private readonly MessageQueue<MessageData> _queue = new();
    private bool _isDisposed;
    private bool _isProcessing;

    /// <summary>
    ///     Constructor
    /// </summary>
    public DeviceService(IDeviceConnection comm, IDeviceController con) {
        // inject
        _comm = comm;
        _con = con;
        // set timer option
        _processTimer.Interval = 100;
        _processTimer.AutoReset = true;
        _processTimer.Elapsed += ProcessTimerOnElapsed;
        // reset disposed
        _isDisposed = false;
        // start timer
        _processTimer.Start();
    }

    /// <summary>
    ///     Device connection state
    /// </summary>
    public DeviceConnectionTypes ConnectionState => _comm.ConnectionState;

    /// <inheritdoc />
    public ValueTask DisposeAsync() {
        // check disposed
        if (_isDisposed)
            return ValueTask.CompletedTask;
        // set disposed
        _isDisposed = true;
        // cancel
        _messageCts.Cancel();
        // reset event
        _processTimer.Elapsed -= ProcessTimerOnElapsed;
        // stop timer
        _processTimer.Stop();
        // dispose
        _processTimer.Dispose();
        _queue.Dispose();
        _messageCts.Dispose();
        // success
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Connect to the device
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="option">option</param>
    /// <param name="token">token</param>
    /// <returns>result</returns>
    public Task<bool> ConnectAsync(string target, object? option = null, CancellationToken token = default) {
        return _comm.ConnectAsync(target, option, token);
    }

    /// <summary>
    ///     Disconnect from the device
    /// </summary>
    /// <returns>result</returns>
    public Task DisconnectAsync() {
        return _comm.DisconnectAsync();
    }

    /// <summary>
    ///     Enqueue the message
    /// </summary>
    /// <param name="command">command</param>
    /// <param name="payload">payload</param>
    /// <returns>result</returns>
    public void Enqueue(DeviceCommandTypes command, object? payload) {
        // create the packet
        var packet = _con.Build(command, payload);
        // create the message
        var msg = new MessageData { Command = command, Packet = packet.ToArray() };
        // enqueue the message
        _queue.Enqueue(msg);
    }

    private void ProcessTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        // message process task
        _ = MessageProcessAsync();
    }

    private async Task MessageProcessAsync() {
        // check processing
        if (_isProcessing)
            return;
        // try catch
        try {
            // set the processing state
            _isProcessing = true;
            // check empty
            if (_queue.IsEmpty)
                return;
            // try peek
            if (_queue.TryPeek(out var msg) == false || msg == null)
                return;
            // check activated
            if (msg.Activated == false) {
                // send async
                if (await _comm.SendAsync(msg.Packet, _messageCts.Token))
                    // set the activated state
                    _queue.Active(msg);
            } else {
                // check timeout
                if (_queue.IsTimeout(msg, MessageTimeout))
                    // deactivate
                    if (_queue.Deactivate(msg))
                        // remove the message
                        _queue.TryDequeue(out _);
            }
        } catch (Exception ex) {
            // debug
            Debug.WriteLine(ex.Message);
        } finally {
            // reset the processing state
            _isProcessing = false;
        }
    }
}