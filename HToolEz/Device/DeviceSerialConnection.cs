using System.Buffers;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading.Channels;
using HToolEz.Defines.Enums;
using HToolEz.Utils;

namespace HToolEz.Device;

/// <summary>
///     Device connection class using RS-232
/// </summary>
public sealed class DeviceSerialConnection : IDeviceConnection {
    private const    int                           ReceivePeriod = 10;
    private readonly Channel<ReadOnlyMemory<byte>> _channel;
    private readonly CancellationTokenSource       _cts;
    private readonly SerialPort                    _port;
    private          bool                          _isDisconnected;
    private          Task?                         _processTask;
    private          Task?                         _receiveTask;

    /// <summary>
    ///     Constructor
    /// </summary>
    public DeviceSerialConnection() {
        // initialize
        _port = new SerialPort();
        _channel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>(
            new UnboundedChannelOptions { SingleReader = false, SingleWriter = true });
        _cts = new CancellationTokenSource();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        // disconnect
        await DisconnectAsync();
        // dispose
        _port.Dispose();
        _cts.Dispose();
        // reset disconnected
        _isDisconnected = false;
    }

    /// <inheritdoc />
    public DeviceConnectionTypes ConnectionState { get; private set; } = DeviceConnectionTypes.Disconnected;

    /// <inheritdoc />
    public Task<bool> ConnectAsync(string target, object? option = null, CancellationToken token = default) {
        // check target
        if (string.IsNullOrWhiteSpace(target))
            return Task.FromResult(false);
        // get baud rate
        var baud = option is int b ? b : 115200;
        // try catch
        try {
            // check open
            if (_port.IsOpen)
                // close port
                _port.Close();
            // set state
            ConnectionState = DeviceConnectionTypes.Connecting;
            // set port options
            _port.PortName = target;
            _port.BaudRate = baud;
            _port.Encoding = Encoding.GetEncoding("ISO-8859-1");
            // open port
            _port.Open();
            // reset disconnected
            _isDisconnected = false;
            // set state
            ConnectionState = DeviceConnectionTypes.Connected;
            // start the task
            _receiveTask = Task.Run(ReadLoopAsync, _cts.Token);
            _processTask = Task.Run(ProcessLoopAsync, _cts.Token);
            // success
            return Task.FromResult(true);
        } catch (Exception ex) {
            // debug
            Debug.WriteLine(ex.Message);
        }

        // reset state
        ConnectionState = DeviceConnectionTypes.Disconnected;
        // fail
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task DisconnectAsync() {
        // check disconnected
        if (_isDisconnected)
            return;
        // set disconnected
        _isDisconnected = true;

        // cancel the task
        await _cts.CancelAsync();
        // try catch
        try {
            // check the task
            if (_receiveTask != null)
                // wait the task
                await _receiveTask.ConfigureAwait(false);
            if (_processTask != null)
                // wait the task
                await _processTask.ConfigureAwait(false);
        } catch (Exception ex) {
            // debug
            Debug.WriteLine(ex.Message);
        }

        // check open
        if (_port.IsOpen)
            // close
            _port.Close();
        // reset state
        ConnectionState = DeviceConnectionTypes.Disconnected;
    }

    /// <inheritdoc />
    public async ValueTask<bool> SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default) {
        // check open
        if (!_port.IsOpen)
            return false;
        // try catch
        try {
            // write
            await _port.BaseStream.WriteAsync(data, token).ConfigureAwait(false);
            // flush
            await _port.BaseStream.FlushAsync(token).ConfigureAwait(false);
            // success
            return true;
        } catch (Exception ex) {
            // debug
            Debug.WriteLine(ex.Message);
        }

        // fail
        return false;
    }

    private async Task ReadLoopAsync() {
        // rent a pool
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        // try catch
        try {
            // check open and cancellation state
            while (_port.IsOpen && !_cts.IsCancellationRequested)
                // check the bytes to read
                if (_port.BytesToRead > 0) {
                    // read the data
                    var length = _port.Read(buffer, 0, buffer.Length);
                    // check length
                    if (length == 0)
                        continue;
                    // create the data
                    var data = new byte[length];
                    // copy to the data
                    Buffer.BlockCopy(buffer, 0, data, 0, length);
                    // write
                    await _channel.Writer.WriteAsync(data, _cts.Token).ConfigureAwait(false);
                } else {
                    // delay
                    await Task.Delay(ReceivePeriod, _cts.Token).ConfigureAwait(false);
                }
        } catch (Exception ex) {
            // debug
            Debug.WriteLine(ex.Message);
        } finally {
            // return the pool
            ArrayPool<byte>.Shared.Return(buffer);
            // complete the writer
            _channel.Writer.TryComplete();
        }
    }

    private async Task ProcessLoopAsync() {
        // try catch
        try {
            // buffer
            var buffer = new CircularBuffer<byte>(4096);

            // wait to read async
            while (await _channel.Reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
            while (_channel.Reader.TryRead(out var data)) {
                // add data
                buffer.EnqueueRange(data.Span);
                // check data
                while (true) {
                    // check length
                    if (buffer.Count < 4)
                        break;
                    // check header 0x5A
                    if (buffer[0] != DeviceProtocolConstants.Header[0]) {
                        // remove
                        buffer.Dequeue();
                        // break
                        continue;
                    }

                    // check header 0xA5
                    if (buffer[1] != DeviceProtocolConstants.Header[1]) {
                        // remove
                        buffer.Dequeue();
                        // break
                        continue;
                    }

                    // get length
                    var length = buffer[2] | (buffer[3] << 8);
                    var frame  = length + DeviceProtocolConstants.Header.Length + 2;
                    // check length
                    if (buffer.Count < frame)
                        break;

                    // create the packet
                    var packet = new byte[frame];
                    // copy the data
                    buffer.DequeueRange(packet);

                    // TODO : event invoke
                }
            }
        } catch (Exception ex) {
            // debug
            Debug.WriteLine(ex.Message);
        }
    }
}