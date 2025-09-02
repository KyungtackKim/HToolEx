using System.Collections.Concurrent;
using System.Net;
using System.Timers;
using HTool.Type;
using HTool.Util;
using JetBrains.Annotations;
using SuperSimpleTcp;
using Timer = System.Timers.Timer;

namespace HTool.Device;

/// <summary>
///     MODBUS TCP class
/// </summary>
[PublicAPI]
public class HcTcp : ITool {
    private const int ErrorFrameSize = 9;
    private SimpleTcpClient? Client { get; set; }
    private ConcurrentQueue<byte> ReceiveBuf { get; } = [];
    private RingBuffer AnalyzeBuf { get; } = new(16 * 1024);
    private Timer ProcessTimer { get; set; } = null!;
    private DateTime AnalyzeTimeout { get; set; }
    private bool IsStopTimer { get; set; }

    /// <summary>
    ///     Header size
    /// </summary>
    public int HeaderSize { get; } = 8;

    /// <summary>
    ///     Function code position
    /// </summary>
    public int FunctionPos { get; } = 7;

    /// <summary>
    ///     Tool connection state
    /// </summary>
    public bool Connected { get; set; }

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
    ///     Received raw data event
    /// </summary>
    public event ITool.PerformRawData? ReceivedRaw;

    /// <summary>
    ///     Transmitted raw data event
    /// </summary>
    public event ITool.PerformRawData? TransmitRaw;

    /// <summary>
    ///     Connect for TCP
    /// </summary>
    /// <param name="target">ip address</param>
    /// <param name="option">port number</param>
    /// <param name="id">id</param>
    /// <returns>result</returns>
    public bool Connect(string target, int option, byte id = 1) {
        // check target
        if (!IPAddress.TryParse(target, out var ip))
            return false;
        // check option
        if (option is < 0 or > 65535)
            return false;
        // check id
        if (id > 0x0F)
            return false;

        // try catch
        try {
            // close client
            Close();
            // create client
            Client = new SimpleTcpClient(ip, option);
            // set event
            Client.Events.Connected    += ClientOnConnectionChanged;
            Client.Events.Disconnected += ClientOnConnectionChanged;
            Client.Events.DataReceived += ClientOnDataReceived;
            // set keep alive
            Client.Keepalive.EnableTcpKeepAlives    = true;
            Client.Keepalive.TcpKeepAliveInterval   = 5;
            Client.Keepalive.TcpKeepAliveTime       = 5;
            Client.Keepalive.TcpKeepAliveRetryCount = 5;
            // connect
            Client.Connect();

            // set device id
            DeviceId = id;
            // clear receive buffer
            ReceiveBuf.Clear();

            // create process timer
            ProcessTimer = new Timer();
            // set timer options
            ProcessTimer.AutoReset =  true;
            ProcessTimer.Interval  =  Constants.ProcessPeriod;
            ProcessTimer.Elapsed   += ProcessTimerOnElapsed;
            // start timer
            ProcessTimer.Start();

            // result ok
            return true;
        } catch (Exception ex) {
            // console
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     Close for TCP
    /// </summary>
    public void Close() {
        // try catch
        try {
            // check process timer
            if (ProcessTimer is { Enabled: true })
                // stop timer
                IsStopTimer = true;

            // check client
            if (Client is { IsConnected: true })
                // close
                Client.Disconnect();

            // dispose
            Client?.Dispose();
            // clear
            Client = null;
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
        // check client
        if (Client is not { IsConnected: true })
            return false;

        // try catch
        try {
            // write packet
            Client.Send(packet);
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
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((DeviceId >> 8) & 0xFF),
            (byte)(DeviceId        & 0xFF),
            /*PID   */
            0x00,
            0x00,
            /*LENGTH*/
            0x00,
            0x06,
            /*UID   */
            0x00,
            /*FC    */
            (byte)CodeTypes.ReadHoldingReg,
            /*ADDR  */
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr        & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count        & 0xFF)
        };
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
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((DeviceId >> 8) & 0xFF),
            (byte)(DeviceId        & 0xFF),
            /*PID   */
            0x00,
            0x00,
            /*LENGTH*/
            0x00,
            0x06,
            /*UID   */
            0x00,
            /*FC    */
            (byte)CodeTypes.ReadInputReg,
            /*ADDR  */
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr        & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count        & 0xFF)
        };
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
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((DeviceId >> 8) & 0xFF),
            (byte)(DeviceId        & 0xFF),
            /*PID   */
            0x00,
            0x00,
            /*LENGTH*/
            0x00,
            0x06,
            /*UID   */
            0x00,
            /*FC    */
            (byte)CodeTypes.WriteSingleReg,
            /*ADDR  */
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr        & 0xFF),
            /*COUNT */
            (byte)((value >> 8) & 0xFF),
            (byte)(value        & 0xFF)
        };
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
        // get count
        var count = values.Length;
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((DeviceId >> 8) & 0xFF),
            (byte)(DeviceId        & 0xFF),
            /*PID   */
            0x00,
            0x00,
            /*LENGTH*/
            0x00,
            0x00,
            /*UID   */
            0x00,
            /*FC    */
            (byte)CodeTypes.WriteMultiReg,
            /*ADDR  */
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr        & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count        & 0xFF),
            /*LENGTH*/
            (byte)(count * 2)
        };
        // check values
        foreach (var value in values) {
            packet.Add((byte)((value >> 8) & 0xFF));
            packet.Add((byte)(value        & 0xFF));
        }

        // get the length
        var len = packet.Count - 6;
        // set the length
        packet[4] = (byte)((len >> 8) & 0xFF);
        packet[5] = (byte)(len        & 0xFF);
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
        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // get count
        var count = length / 2;
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((DeviceId >> 8) & 0xFF),
            (byte)(DeviceId        & 0xFF),
            /*PID   */
            0x00,
            0x00,
            /*LENGTH*/
            0x00,
            0x06,
            /*UID   */
            0x00,
            /*FC    */
            (byte)CodeTypes.WriteMultiReg,
            /*ADDR  */
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr        & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count        & 0xFF),
            /*LENGTH*/
            (byte)length
        };
        // add string
        packet.AddRange(str.Select(c => (byte)c));
        // check string length
        if (str.Length < length)
            // add dummy data
            packet.AddRange(new byte[length - str.Length]);
        // get the length
        var len = packet.Count - 6;
        // set the length
        packet[4] = (byte)((len >> 8) & 0xFF);
        packet[5] = (byte)(len        & 0xFF);
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Get information register packet
    /// </summary>
    /// <returns>result</returns>
    public byte[] GetInfoRegPacket() {
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((DeviceId >> 8) & 0xFF),
            (byte)(DeviceId        & 0xFF),
            /*PID   */
            0x00,
            0x00,
            /*LENGTH*/
            0x00,
            0x06,
            /*UID   */
            0x00,
            /*FC    */
            (byte)CodeTypes.ReadInfoReg
        };
        // packet
        return packet.ToArray();
    }

    private void ClientOnConnectionChanged(object? sender, ConnectionEventArgs e) {
        // check reason
        if (e.Reason != DisconnectReason.None)
            // close
            Close();

        // connection changed event
        ChangedConnect?.Invoke(e.Reason == DisconnectReason.None);
    }

    private void ClientOnDataReceived(object? sender, DataReceivedEventArgs e) {
        // get all data from buffer
        var data = e.Data.ToArray();
        // check data length
        foreach (var b in data)
            // enqueue data
            ReceiveBuf.Enqueue(b);
        // received raw data
        ReceivedRaw?.Invoke(data);
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
#if NOT_USE
            // check empty for receive buffer
            while (!ReceiveBuf.IsEmpty) {
                // get data
                if (ReceiveBuf.TryDequeue(out var d))
                    // add data
                    AnalyzeBuf.Write(d);
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
            }
#else
            // get the data
            while (ReceiveBuf.TryDequeue(out var d)) {
                // add data
                AnalyzeBuf.Write(d);
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
            }
#endif

            // check analyze count
            if (AnalyzeBuf.Available > 0)
                // check analyze timeout
                if ((DateTime.Now - AnalyzeTimeout).TotalMilliseconds > Constants.ProcessTimeout)
                    // clear buffer
                    AnalyzeBuf.Clear();
            // check data header length    [TID(2) PID(2) LEN(2) UID(1) FC(1)]
            if (AnalyzeBuf.Available < HeaderSize)
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
                // TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(1)=DATA_LEN DATA(N)
                CodeTypes.ReadHoldingReg or CodeTypes.ReadInputReg or CodeTypes.ReadInfoReg =>
                    AnalyzeBuf.Peek(HeaderSize) + 9,
                // TID(2) PID(2) LEN(2) UID(1) FC(1) ADDR(2) VALUE(2)/COUNT(2)
                CodeTypes.WriteSingleReg or CodeTypes.WriteMultiReg => 12,
                // TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN DATA(N)
                CodeTypes.Graph or CodeTypes.GraphRes =>
                    ((AnalyzeBuf.Peek(HeaderSize) << 8) | AnalyzeBuf.Peek(HeaderSize + 1)) + 10,
                // TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN REV(2) DATA(N)
                CodeTypes.HighResGraph =>
                    ((AnalyzeBuf.Peek(HeaderSize) << 8) | AnalyzeBuf.Peek(HeaderSize + 1)) + 10,
                // TID(2) PID(2) LEN(2) UID(1) FC(1) ERROR(1)
                CodeTypes.Error => ErrorFrameSize,
                // exception
                _ => throw new ArgumentOutOfRangeException(string.Empty)
            };

            // check frame length
            if (AnalyzeBuf.Available < frame)
                return;

            // get packet
            var packet = AnalyzeBuf.ReadBytes(frame);
            // update event
            ReceivedData?.Invoke(cmd, packet);
        } finally {
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}