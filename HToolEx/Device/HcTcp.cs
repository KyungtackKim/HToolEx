using System.Collections.Concurrent;
using System.Net;
using System.Timers;
using HToolEx.Type;
using SuperSimpleTcp;
using Timer = System.Timers.Timer;

namespace HToolEx.Device;

/// <summary>
///     Hantas serial communication TCP class
/// </summary>
public class HcTcp : IHComm {
    private static int ProcessPeriod => 100;
    private static int ProcessLockTime => 500;
    private static int ProcessTimeout => 3000;

    private SimpleTcpClient? Client { get; set; }
    private ConcurrentQueue<byte> ReceiveBuf { get; } = [];
    private List<byte> AnalyzeBuf { get; } = [];
    private Timer ProcessTimer { get; set; } = default!;
    private bool IsStopTimer { get; set; }
    private DateTime AnalyzeTimeout { get; set; }

    /*          NOT USE
    /// <summary>
    ///     Transaction ID
    /// </summary>
    public int TransactionId { get; set; }
    */

    /// <summary>
    ///     Received message event
    /// </summary>
    public event IHComm.PerformReceiveMsg? ReceivedMsg;

    /// <summary>
    ///     Received data event
    /// </summary>
    public event IHComm.PerformReceiveData? ReceivedData;

    /// <summary>
    ///     Connection changed message event
    /// </summary>
    public event IHComm.PerformConnectMsg? ConnectedMsg;

    /// <summary>
    ///     Device ID
    /// </summary>
    public byte DeviceId { get; set; }

    /// <summary>
    ///     Connection state
    /// </summary>
    public bool IsConnected => Client?.IsConnected ?? false;

    /// <summary>
    ///     Connect device
    /// </summary>
    /// <param name="target">target COM port</param>
    /// <param name="option">baud rate</param>
    /// <param name="id">device id</param>
    /// <returns>result</returns>
    public bool Connect(string target, int option, byte id = 0x01) {
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
            Client.Events.Connected += ClientOnConnectionChanged;
            Client.Events.Disconnected += ClientOnConnectionChanged;
            Client.Events.DataReceived += ClientOnDataReceived;
            // set keep alive
            Client.Keepalive.EnableTcpKeepAlives = true;
            Client.Keepalive.TcpKeepAliveInterval = 5;
            Client.Keepalive.TcpKeepAliveTime = 5;
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
            ProcessTimer.AutoReset = true;
            ProcessTimer.Interval = ProcessPeriod;
            ProcessTimer.Elapsed += ProcessTimerOnElapsed;
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
    ///     Close device
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
    ///     Write packet
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
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public byte[] GetReadHoldingRegPacket(ushort addr, ushort count, int id = 1) {
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((id >> 8) & 0xFF),
            (byte)(id & 0xFF),
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
            (byte)(addr & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF)
        };
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Get input register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public byte[] GetReadInputRegPacket(ushort addr, ushort count, int id = 1) {
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((id >> 8) & 0xFF),
            (byte)(id & 0xFF),
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
            (byte)(addr & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF)
        };
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set single register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public byte[] SetSingleRegPacket(ushort addr, ushort value, int id = 1) {
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((id >> 8) & 0xFF),
            (byte)(id & 0xFF),
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
            (byte)(addr & 0xFF),
            /*COUNT */
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF)
        };
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set multiple register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public byte[] SetMultiRegPacket(ushort addr, ushort[] values, int id = 1) {
        // get count
        var count = values.Length;
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((id >> 8) & 0xFF),
            (byte)(id & 0xFF),
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
            (byte)(addr & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF),
            /*LENGTH*/
            (byte)(count * 2)
        };
        // check values
        foreach (var value in values) {
            packet.Add((byte)((value >> 8) & 0xFF));
            packet.Add((byte)(value & 0xFF));
        }

        // get total length
        var len = packet.Count - 6;
        // change length
        packet[4] = (byte)((len >> 8) & 0xFF);
        packet[5] = (byte)(len & 0xFF);

        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set multiple register ascii packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string</param>
    /// <param name="length">length</param>
    /// <param name="id">id</param>
    /// <returns>result</returns>
    public byte[] SetMultiRegStrPacket(ushort addr, string str, int length, int id = 1) {
        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // get count
        var count = length / 2;
        // create packet
        var packet = new List<byte> {
            /*TID   */
            (byte)((id >> 8) & 0xFF),
            (byte)(id & 0xFF),
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
            (byte)(addr & 0xFF),
            /*COUNT */
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF),
            /*LENGTH*/
            (byte)length
        };
        // add string
        packet.AddRange(str.Select(c => (byte)c));
        // check string length
        if (str.Length < length)
            // add dummy data
            packet.AddRange(new byte[length - str.Length]);

        // packet
        return packet.ToArray();
    }

    private void ClientOnConnectionChanged(object? sender, ConnectionEventArgs e) {
        // check reason
        if (e.Reason != DisconnectReason.None)
            // close
            Close();

        // connection changed event
        ConnectedMsg?.Invoke(e.Reason == DisconnectReason.None);
    }

    private void ClientOnDataReceived(object? sender, DataReceivedEventArgs e) {
        // get all data from buffer
        var data = e.Data.ToArray();
        // check data length
        foreach (var b in data)
            // enqueue data
            ReceiveBuf.Enqueue(b);
        // received raw data
        ReceivedData?.Invoke(data);
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
        }

        // try enter
        if (!Monitor.TryEnter(AnalyzeBuf, ProcessLockTime))
            // return
            return;
        // try finally
        try {
            // check empty for receive buffer
            while (!ReceiveBuf.IsEmpty) {
                // get data
                if (ReceiveBuf.TryDequeue(out var d))
                    // add data
                    AnalyzeBuf.Add(d);
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
            }

            // check analyze count
            if (AnalyzeBuf.Count > 0)
                // check analyze timeout
                if ((DateTime.Now - AnalyzeTimeout).TotalMilliseconds > ProcessTimeout)
                    // clear buffer
                    AnalyzeBuf.Clear();

            // check data header length    [TID(2) PID(2) LEN(2) UID(1) FC(1)]
            if (AnalyzeBuf.Count < 8)
                return;
            // check command
            if (!Enum.IsDefined(typeof(CodeTypes), (int)AnalyzeBuf[7]) &&
                (AnalyzeBuf[7] & 0x80) != (int)CodeTypes.Error)
                return;
            // get command
            var cmd = (CodeTypes)AnalyzeBuf[7];
            // check error
            if (((byte)cmd & 0x80) == (int)CodeTypes.Error) {
                // get error command
                cmd = (CodeTypes)((byte)cmd & 0x7F);
                // get error code
                var code = AnalyzeBuf.Count > 8 ? AnalyzeBuf[8] : (byte)0x00;
                // error invoke
                ReceivedMsg?.Invoke(CodeTypes.Error, new byte[] {
                    // (byte)((TransactionId >> 8) & 0xFF), (byte)(TransactionId & 0xFF),   // NOT USE
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, (byte)cmd, 0x00, code, 0x00, 0x00
                });
                // clear buffer
                AnalyzeBuf.Clear();
            } else {
                int frame;
                // check function code
                switch (cmd) {
                    case CodeTypes.ReadHoldingReg:
                    case CodeTypes.ReadInputReg:
                        // TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(1)=DATA_LEN DATA(N)
                        frame = AnalyzeBuf[8] + 9;
                        break;
                    case CodeTypes.WriteSingleReg:
                    case CodeTypes.WriteMultiReg:
                        // TID(2) PID(2) LEN(2) UID(1) FC(1) ADDR(2) VALUE(2)/COUNT(2)
                        frame = 12;
                        break;
                    case CodeTypes.Graph:
                    case CodeTypes.GraphRes:
                        // TID(2) PID(2) LEN(2) UID(1) FC(1) LEN(2)=DATA_LEN DATA(N)
                        frame = ((AnalyzeBuf[8] << 8) | AnalyzeBuf[9]) + 10;
                        break;
                    case CodeTypes.Error:
                        // TID(2) PID(2) LEN(2) UID(1) FC(1) ERROR(1)
                        frame = 9;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(string.Empty);
                }

                // check frame length
                if (AnalyzeBuf.Count < frame)
                    return;

                // get packet
                var packet = AnalyzeBuf.Take(frame).ToArray();
                // check length
                if (AnalyzeBuf.Count >= frame)
                    // remove analyze buffer
                    AnalyzeBuf.RemoveRange(0, frame);
                else
                    // clear analyze buffer
                    AnalyzeBuf.Clear();
                // update event
                ReceivedMsg?.Invoke(cmd, packet);
                // reset analyze time
                AnalyzeTimeout = DateTime.Now;
            }
        } finally {
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}