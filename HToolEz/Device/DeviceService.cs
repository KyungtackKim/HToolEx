using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using HToolEz.Type;
using HToolEz.Util;
using Timer = System.Timers.Timer;

namespace HToolEz.Device;

/// <summary>
///     Device service class
/// </summary>
public class DeviceService : IDeviceService {
    private          DateTime        _analyzeTimeout;
    private          bool            _isDisposed;
    private          bool            _isStopTimer;
    private volatile DeviceModeTypes _mode = DeviceModeTypes.Operation;

    /// <summary>
    ///     Constructor
    /// </summary>
    public DeviceService() { }

    private SerialPort? Port { get; set; }
    private Timer? ProcessTimer { get; set; }
    private ConcurrentQueue<(int len, byte[] buf)> ReceiveBuf { get; } = new();
    private RingBuffer AnalyzeBuf { get; } = new(12 * 1024);

    /// <inheritdoc />
    public DeviceModeTypes Mode {
        get => _mode;
        set => _mode = value;
    }

    /// <inheritdoc />
    public void Dispose() {
        // check the disposed
        if (_isDisposed)
            return;
        // dispose
        Dispose(true);
        // GC finalize
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public bool IsConnected { get; private set; }

    /// <inheritdoc />
    public bool Connect(string target) {
        // check the target
        if (string.IsNullOrEmpty(target))
            return false;
        // try catch
        try {
            // clear the buffer
            ResetBuffer();
            // create the port
            Port = new SerialPort();
            // set the options
            Port.PortName        = target;
            Port.BaudRate        = 115200;
            Port.ReadBufferSize  = 16 * 1024;
            Port.WriteBufferSize = 16 * 1024;
            Port.Handshake       = Handshake.None;
            Port.Encoding        = Encoding.GetEncoding("iso-8859-1");
            // set the event
            Port.DataReceived += OnDataReceived;
            // open
            Port.Open();

            // create the process timer
            ProcessTimer = new Timer();
            // set the options
            ProcessTimer.AutoReset =  true;
            ProcessTimer.Interval  =  Constants.ProcessPeriod;
            ProcessTimer.Elapsed   += TimerOnElapsed;
            // start timer
            ProcessTimer.Start();

            // set the connected state
            IsConnected = true;
            // success
            return true;
        } catch (Exception ex) {
            // check the port
            if (Port != null) {
                // check the open state
                if (Port.IsOpen)
                    // close
                    Port.Close();
                // reset the event
                Port.DataReceived -= OnDataReceived;
                // dispose
                Port.Dispose();
                // clear the object
                Port = null;
            }

            // check the process timer
            if (ProcessTimer != null) {
                // check timer start state
                if (ProcessTimer.Enabled)
                    // stop timer
                    ProcessTimer.Stop();
                // reset the event
                ProcessTimer.Elapsed -= TimerOnElapsed;
                // dispose
                ProcessTimer.Dispose();
                // clear the object
                ProcessTimer = null;
            }

            // debug log
            Console.WriteLine($"Failed to connect to the device : {ex}");
        }

        // failed to connect
        return false;
    }

    /// <inheritdoc />
    public void Disconnect() {
        // try catch
        try {
            var port = Port;
            // clear the object
            Port = null;
            // check process timer
            if (ProcessTimer is { Enabled: true })
                // set the stop timer
                _isStopTimer = true;
            // check the port
            if (port == null)
                return;
            // check the open state
            if (port.IsOpen)
                // close
                port.Close();
            // reset the event
            port.DataReceived -= OnDataReceived;
            // dispose
            port.Dispose();
            // reset the buffer
            ResetBuffer();
            // reset the connection state
            IsConnected = false;
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"Failed to disconnect to the device : {ex}");
        }
    }

    /// <inheritdoc />
    public bool Write(ReadOnlyMemory<byte> packet) {
        // get the port
        var port = Port;
        // check the port
        if (port is not { IsOpen: true })
            return false;
        // get the packet length
        var length = packet.Length;
        // check the packet length
        if (length < 1)
            return true;
        // try catch
        try {
            // try to get underlying array without copying
            if (MemoryMarshal.TryGetArray(packet, out var segment)) {
                // write to device
                port.Write(segment.Array!, segment.Offset, segment.Count);
            } else {
                // get the buffer
                var buf = GC.AllocateUninitializedArray<byte>(length);
                // copy to
                packet.CopyTo(buf);
                // write to device
                port.Write(buf, 0, buf.Length);
            }

            // success
            return true;
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"Failed to write the packet : {ex.Message}");
        }

        // failed to write
        return false;
    }

    /// <inheritdoc />
    public event IDeviceService.PerformTorque? ReceivedTorque;

    /// <inheritdoc />
    public event IDeviceService.PerformData? ReceivedData;

    private void Dispose(bool disposing) {
        // check disposing
        if (disposing) {
            // get the port
            var port = Port;
            // clear the object
            Port = null;
            // check the port
            if (port != null) {
                // check the open state
                if (port.IsOpen)
                    // close
                    port.Close();
                // reset the event
                port.DataReceived -= OnDataReceived;
                // dispose
                port.Dispose();
            }

            // get the timer
            var timer = ProcessTimer;
            // clear the object
            ProcessTimer = null;
            // check the timer
            if (timer != null) {
                // check timer start state
                if (timer.Enabled)
                    // stop timer
                    timer.Stop();
                // reset the event
                timer.Elapsed -= TimerOnElapsed;
                // dispose
                timer.Dispose();
            }
        }

        // reset the buffer
        ResetBuffer();
        // reset the state
        _isStopTimer = false;
        // set the disposed
        _isDisposed = true;
    }

    private void ResetBuffer() {
        // check the empty
        if (!ReceiveBuf.IsEmpty)
            // get the buffer
            while (ReceiveBuf.TryDequeue(out var value))
                // return the pool
                ArrayPool<byte>.Shared.Return(value.buf);
        // clear the buffer
        ReceiveBuf.Clear();
        AnalyzeBuf.Clear();
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e) {
        // get the port
        var port = Port;
        // check the port
        if (port is not { IsOpen: true })
            return;
        // try catch
        try {
            // get the length
            var length = port.BytesToRead;
            // check the length
            if (length < 1)
                return;
            // check the operation mode
            switch (Mode) {
                case DeviceModeTypes.Operation:
                    // process the operation mode
                    ProcessOperationMode(port, length);
                    break;
                case DeviceModeTypes.Calibration:
                    // process the calibration mode
                    ProcessCalibrationMode(port, length);
                    break;
                default:
                    // clear the buffer
                    port.ReadExisting();
                    break;
            }
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"Received data error : {ex.Message}");
        }
    }

    private void ProcessOperationMode(SerialPort port, int length) {
        // rent buffer from pool
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        // try finally
        try {
            // read data as bytes
            var read = port.Read(buffer, 0, length);
            // check read length
            if (read < 1)
                // end of processing
                return;

            // check first byte to determine data type
            if (buffer[0] >= 0x30 && buffer[0] <= 0x39)
                // ASCII torque data (starts with digit 0-9)
                ProcessAsciiTorque(buffer, read);
            else if (read      >= 9    &&
                     buffer[0] == 0x5A && buffer[1] == 0xA5 && buffer[2] == 0x05 &&
                     buffer[3] == 0x00 && buffer[4] == 0x85)
                // binary torque data (Header: 5A A5 05 00 85 + 4-byte float)
                ProcessBinaryTorque(buffer);
            // else: discard invalid data
        } finally {
            // return buffer to pool
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void ProcessAsciiTorque(byte[] buffer, int length) {
        // convert bytes to string
        var text = Encoding.ASCII.GetString(buffer, 0, length);
        // split the data
        var values = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        // check the length
        if (values.Length < 1)
            return;
        // split the value
        var data = values[^1].Split(',');
        // check the length
        if (data.Length != 2)
            return;
        // convert to torque
        if (!float.TryParse(data[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var torque))
            return;
        // convert to unit
        var unit = Utils.ParseToUnit(data[1].Trim());
        // invoke the received torque
        ReceivedTorque?.Invoke(torque, unit);
    }

    private void ProcessBinaryTorque(byte[] buffer) {
        // rent buffer for 9 bytes
        var chunk = ArrayPool<byte>.Shared.Rent(9);
        // copy 9 bytes to chunk
        buffer.AsSpan(0, 9).CopyTo(chunk);
        // enqueue for timer processing
        ReceiveBuf.Enqueue((9, chunk));
    }

    private void ProcessCalibrationMode(SerialPort port, int length) {
        // rent buffer from pool
        var chunk = ArrayPool<byte>.Shared.Rent(length);
        // try catch
        try {
            // read the data
            var read = port.Read(chunk, 0, length);
            // check the read length
            if (read < 1) {
                // return the pool
                ArrayPool<byte>.Shared.Return(chunk);
                // end of processing
                return;
            }
            // enqueue for timer processing
            ReceiveBuf.Enqueue((read, chunk));
        } catch (Exception ex) {
            // return the pool
            ArrayPool<byte>.Shared.Return(chunk);
            // debug log
            Console.WriteLine($"Read the data error: {ex.Message}");
        }
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e) {
        // check the timer
        if (ProcessTimer == null)
            return;
        // check the timer stop state
        if (_isStopTimer) {
            // reset the stop state
            _isStopTimer = false;
            // get the process timer
            var timer = ProcessTimer;
            // check timer start state
            if (timer.Enabled)
                // stop timer
                timer.Stop();
            // reset the event
            timer.Elapsed -= TimerOnElapsed;
            // dispose
            timer.Dispose();
            return;
        }

        // enter the monitor
        if (!Monitor.TryEnter(AnalyzeBuf, Constants.ProcessLockTime))
            return;
        // process without lock
        try {
            var isUpdateForAnalyzeBuf = false;
            // get the data block
            while (ReceiveBuf.TryDequeue(out var block)) {
                // get the data block
                var (length, data) = block;
                // write the data
                AnalyzeBuf.WriteBytes(data.AsSpan(0, length));
                // return the data block
                ArrayPool<byte>.Shared.Return(data);
                // set the update analyze buf
                isUpdateForAnalyzeBuf = true;
            }

            // check the analyze buf changed
            if (isUpdateForAnalyzeBuf)
                // set analyze time
                _analyzeTimeout = DateTime.Now;

            // get the total length
            var total = AnalyzeBuf.Available;
            // check the analyze buffer
            if (total > 0)
                // check the analyze timeout
                if ((DateTime.Now - _analyzeTimeout).TotalMilliseconds >= Constants.ProcessTimeout)
                    // clear the buffer
                    AnalyzeBuf.Clear();
            // check the data length
            if (total < DeviceHelper.HeaderSize)
                return;

            // check the STX bytes
            if (AnalyzeBuf.Peek(0) != DeviceHelper.HeaderStx[0] || AnalyzeBuf.Peek(1) != DeviceHelper.HeaderStx[1]) {
                var foundIndex = -1;
                // find STX pattern in buffer
                var buffer = AnalyzeBuf.PeekBytes();
                // check the length
                for (var i = 1; i < buffer.Length - 1; i++) {
                    // check the STX bytes
                    if (buffer[i] != DeviceHelper.HeaderStx[0] || buffer[i + 1] != DeviceHelper.HeaderStx[1])
                        continue;
                    // set the index
                    foundIndex = i;
                    // end of find
                    break;
                }

                // check the index
                if (foundIndex > 0)
                    // found STX - remove bytes before it
                    AnalyzeBuf.RemoveBytes(foundIndex);
                else
                    // STX not found - keep only last byte (might be start of STX)
                    AnalyzeBuf.RemoveBytes(buffer.Length - 1);
                // end of parse
                return;
            }

            // get the frame length
            var frame = ((AnalyzeBuf.Peek(3) << 8) | AnalyzeBuf.Peek(2)) + DeviceHelper.HeaderSize;
            // check the frame length
            if (total < frame)
                return;

            // get the command and packet data
            var cmd    = (DeviceCommandTypes)AnalyzeBuf.Peek(4);
            var packet = AnalyzeBuf.ReadBytes(frame);
            // invoke the received data event
            ReceivedData?.Invoke(cmd, packet);
        } catch (Exception ex) {
            // debug log
            Console.WriteLine($"[DeviceService] TimerOnElapsed error: {ex.Message}");
        } finally {
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}