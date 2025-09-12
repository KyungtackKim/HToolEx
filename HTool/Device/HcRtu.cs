using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Timers;
using HTool.Type;
using HTool.Util;
using JetBrains.Annotations;
using Timer = System.Timers.Timer;

namespace HTool.Device;

/// <summary>
///     MODBUS RTU class
/// </summary>
[PublicAPI]
public class HcRtu : ITool {
    private const           int   ErrorFrameSize = 5;
    private static readonly int[] BaudRates      = [9600, 19200, 38400, 57600, 115200, 230400];
    private SerialPort Port { get; } = new();
    private ConcurrentQueue<(byte[] buf, int len)> ReceiveBuf { get; } = [];
    private RingBuffer AnalyzeBuf { get; } = new(16 * 1024);
    private Timer ProcessTimer { get; set; } = null!;
    private DateTime AnalyzeTimeout { get; set; }
    private bool IsStopTimer { get; set; }

    /// <summary>
    ///     Header size
    /// </summary>
    public int HeaderSize { get; } = 2;

    /// <summary>
    ///     Function code position
    /// </summary>
    public int FunctionPos { get; } = 1;

    /// <summary>
    ///     Tool device id
    /// </summary>
    public byte DeviceId { get; set; }

    /// <summary>
    ///     Tool generation revision
    /// </summary>
    public GenerationTypes Revision { get; set; }

    /// <summary>
    ///     Connection changed event
    /// </summary>
    public event ITool.PerformConnect? ChangedConnect;

    /// <summary>
    ///     Received data event
    /// </summary>
    public event ITool.PerformReceiveData? ReceivedData;

    /// <summary>
    ///     Received error data event
    /// </summary>
    public event ITool.PerformReceiveError? ReceivedError;

    /// <summary>
    ///     Received raw data event
    /// </summary>
    public event ITool.PerformRawData? ReceivedRaw;

    /// <summary>
    ///     Transmitted raw data event
    /// </summary>
    public event ITool.PerformRawData? TransmitRaw;

    /// <summary>
    ///     Connect for RTU
    /// </summary>
    /// <param name="target">COM port</param>
    /// <param name="option">baud rate</param>
    /// <param name="id">id</param>
    /// <returns>result</returns>
    public bool Connect(string target, int option, byte id = 1) {
        // check target
        if (string.IsNullOrWhiteSpace(target))
            return false;
        // check option
        if (!Constants.BaudRates.Contains(option))
            return false;
        // check id
        if (id > 0x0F)
            return false;

        // try catch
        try {
            // check the received buffer
            foreach (var value in ReceiveBuf)
                // return the pool
                ArrayPool<byte>.Shared.Return(value.buf);
            // clear receive buffer
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();
            // set com port
            Port.PortName               = target;
            Port.BaudRate               = option;
            Port.ReadBufferSize         = 16 * 1024;
            Port.WriteBufferSize        = 16 * 1024;
            Port.Handshake              = Handshake.None;
            Port.Encoding               = Encoding.GetEncoding(@"iso-8859-1");
            // open
            Port.Open();
            // set device id
            DeviceId = id;
            // set event
            Port.DataReceived += PortOnDataReceived;

            // create process timer
            ProcessTimer = new Timer();
            // set timer options
            ProcessTimer.AutoReset =  true;
            ProcessTimer.Interval  =  Constants.ProcessPeriod;
            ProcessTimer.Elapsed   += ProcessTimerOnElapsed;
            // start timer
            ProcessTimer.Start();

            // connection changed event
            ChangedConnect?.Invoke(true);

            // result ok
            return true;
        } catch (Exception e) {
            // console
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     Close for RTU
    /// </summary>
    public void Close() {
        // try catch
        try {
            // check process timer
            if (ProcessTimer.Enabled)
                // stop timer
                IsStopTimer = true;
            // close
            Port.Close();
            // reset event
            Port.DataReceived -= PortOnDataReceived;
            // connection changed event
            ChangedConnect?.Invoke(false);
        } catch (Exception ex) {
            // console
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    ///     Write the packet data
    /// </summary>
    /// <param name="packet">packet</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    public bool Write(byte[] packet, int length) {
        // check connection state
        if (!Port.IsOpen)
            return false;

        // try catch
        try {
            // write packet
            Port.Write(packet, 0, length);
            // transmit data event
            TransmitRaw?.Invoke(packet);
            // result ok
            return true;
        } catch (Exception ex) {
            // console
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     Get holding register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <returns>packet</returns>
    public byte[] GetReadHoldingRegPacket(ushort addr, ushort count) {
        // allocation the packet
        Span<byte> packet = stackalloc byte[8];
        // set values
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.ReadHoldingReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);

        // set the crc
        Utils.CalculateCrcTo(packet[..6], packet[6..]);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Get input register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <returns>packet</returns>
    public byte[] GetReadInputRegPacket(ushort addr, ushort count) {
        // allocation the packet
        Span<byte> packet = stackalloc byte[8];
        // set values
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.ReadInputReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);

        // set the crc
        Utils.CalculateCrcTo(packet[..6], packet[6..]);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set single register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <returns>packet</returns>
    public byte[] SetSingleRegPacket(ushort addr, ushort value) {
        // allocation the packet
        Span<byte> packet = stackalloc byte[8];
        // set values
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.WriteSingleReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((value >> 8) & 0xFF);
        packet[5] = (byte)(value        & 0xFF);

        // set the crc
        Utils.CalculateCrcTo(packet[..6], packet[6..]);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set multiple register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <returns>packet</returns>
    public byte[] SetMultiRegPacket(ushort addr, ushort[] values) {
        var index = 7;
        // get count
        var count = values.Length;
        // get the packet size
        var size = index + count * 2 + 2;
        // allocation the packet
        var packet = size < 1024 ? stackalloc byte[size] : new byte[size];
        // set the header
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.WriteMultiReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);
        packet[6] = (byte)(count * 2);

        // check the values
        foreach (var value in values) {
            // set the value
            packet[index++] = (byte)((value >> 8) & 0xFF);
            packet[index++] = (byte)(value        & 0xFF);
        }

        // set the crc
        Utils.CalculateCrcTo(packet[..^2], packet[^2..]);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set multiple register ascii packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    public byte[] SetMultiRegStrPacket(ushort addr, string str, int length) {
        var index = 7;
        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // get count
        var count = length / 2;
        // get the packet size
        var size = index + count * 2 + 2;
        // allocation the packet
        var packet = size < 1024 ? stackalloc byte[size] : new byte[size];
        // set the header
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.WriteMultiReg;
        packet[2] = (byte)((addr >> 8)  & 0xFF);
        packet[3] = (byte)(addr         & 0xFF);
        packet[4] = (byte)((count >> 8) & 0xFF);
        packet[5] = (byte)(count        & 0xFF);
        packet[6] = (byte)length;
        // set the string values
        foreach (var c in str)
            // set the value
            packet[index++] = (byte)c;

        // set the padding zeros
        while (index < 7 + length)
            // set the padding value
            packet[index++] = 0;

        // set the crc
        Utils.CalculateCrcTo(packet[..^2], packet[^2..]);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Get information register packet
    /// </summary>
    /// <returns>result</returns>
    public byte[] GetInfoRegPacket() {
        // allocation the packet
        Span<byte> packet = stackalloc byte[4];
        // set values
        packet[0] = DeviceId;
        packet[1] = (byte)CodeTypes.ReadInfoReg;

        // set the crc
        Utils.CalculateCrcTo(packet[..2], packet[2..]);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     HComm serial get port names
    /// </summary>
    /// <returns>port list</returns>
    public static IEnumerable<string> GetPortNames() {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    ///     HComm serial get port baud-rates
    /// </summary>
    /// <returns>baud-rate list</returns>
    public static IEnumerable<int> GetBaudRates() {
        return BaudRates;
    }

    private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e) {
        // loop the infinite
        while (true) {
            // get the length
            var length = Port.BytesToRead;
            // check the length
            if (length < 1)
                break;
            // rent the pool
            var chunk = ArrayPool<byte>.Shared.Rent(length);
            // read all data
            var read = Port.Read(chunk, 0, length);
            // check length
            if (read < 1) {
                // return the pool
                ArrayPool<byte>.Shared.Return(chunk);
                // end of receive
                break;
            }

            // enqueue the data block
            ReceiveBuf.Enqueue((chunk, read));
            // check the received raw event
            if (ReceivedRaw is null)
                continue;
            // create the space
            var raw = GC.AllocateUninitializedArray<byte>(read);
            // copy the data
            Buffer.BlockCopy(chunk, 0, raw, 0, read);
            // event
            ReceivedRaw(raw);
        }
    }

    private void ProcessTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        // check stop
        if (IsStopTimer) {
            // reset event
            ProcessTimer.Elapsed -= ProcessTimerOnElapsed;
            // stop timer
            ProcessTimer.Stop();
            // dispose timer
            ProcessTimer.Dispose();
            // clear buffer
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();
            // return
            return;
        }

        // try enter
        if (!Monitor.TryEnter(AnalyzeBuf, Constants.ProcessLockTime))
            // return
            return;
        // try finally
        try {
            var isUpdateForAnalyzeBuf = false;
            // get the data block
            while (ReceiveBuf.TryDequeue(out var block)) {
                // get the data block
                var (buf, length) = block;
                // write the data
                AnalyzeBuf.WriteBytes(buf.AsSpan(0, length));
                // return the data block
                ArrayPool<byte>.Shared.Return(buf);
                // set the update analyze buf
                isUpdateForAnalyzeBuf = true;
            }

            // check the analyze buf changed
            if (isUpdateForAnalyzeBuf)
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
            // check analyze count
            if (AnalyzeBuf.Available > 0)
                // check analyze timeout
                if ((DateTime.Now - AnalyzeTimeout).TotalMilliseconds > Constants.ProcessTimeout) {
                    // get the cleared buffer length
                    var len = AnalyzeBuf.Available;
                    // clear buffer
                    AnalyzeBuf.Clear();
                    // error data
                    ReceivedError?.Invoke(ComErrorTypes.Timeout, len);
                }

            // check data header length    [ID(1) FUNC(1)]
            if (AnalyzeBuf.Available < HeaderSize)
                return;
            // check id
            if (AnalyzeBuf.Peek(0) != DeviceId)
                return;

            // get command value
            var value = (int)AnalyzeBuf.Peek(FunctionPos);
            // check command
            if (!Enum.IsDefined(typeof(CodeTypes), value) && (value & (int)CodeTypes.Error) != (int)CodeTypes.Error)
                return;
            // get command
            var cmd = Enum.IsDefined(typeof(CodeTypes), value) ? (CodeTypes)value : CodeTypes.Error;
            // check function code
            var frame = cmd switch {
                // ID(1) + FC(1) + LEN(1)=DATA_LEN + DATA(N) + CRC(2)
                CodeTypes.ReadHoldingReg or CodeTypes.ReadInputReg or CodeTypes.ReadInfoReg =>
                    AnalyzeBuf.Peek(HeaderSize) + 5,
                // ID(1) + FC(1) + ADDR(2) + VALUE(2)/COUNT(2) + CRC(2)
                CodeTypes.WriteSingleReg or CodeTypes.WriteMultiReg => 8,
                // ID(1) + FC(1) + LEN(2)=DATA_LEN + DATA(N) + CRC(2)
                CodeTypes.Graph or CodeTypes.GraphRes =>
                    ((AnalyzeBuf.Peek(HeaderSize) << 8) | AnalyzeBuf.Peek(HeaderSize + 1)) + 6,
                // ID(1) + FC(1) + ERROR(1) + CRC(2)
                CodeTypes.Error => ErrorFrameSize,
                // exception
                _ => throw new ArgumentOutOfRangeException(string.Empty)
            };

            // check frame length
            if (AnalyzeBuf.Available < frame)
                return;
            // get packet
            var packet = AnalyzeBuf.ReadBytes(frame);
            // validate the crc
            if (Utils.ValidateCrc(packet))
                // update event
                ReceivedData?.Invoke(cmd, packet);
            else
                // error data
                ReceivedError?.Invoke(ComErrorTypes.InvalidCrc);

            // update analyze time
            AnalyzeTimeout = DateTime.Now;
        } finally {
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}