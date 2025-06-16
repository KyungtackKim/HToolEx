using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Timers;
using HToolEx.Type;
using Timer = System.Timers.Timer;

namespace HToolEx.Device;

/// <summary>
///     Hantas serial communication RTU class
/// </summary>
public class HcRtu : IHComm {
    private static readonly int[] BaudRates = [9600, 19200, 38400, 57600, 115200, 230400];
    private static int ProcessPeriod => 100;
    private static int ProcessLockTime => 500;
    private static int ProcessTimeout => 3000;

    private SerialPort Port { get; } = new();
    private ConcurrentQueue<byte> ReceiveBuf { get; } = [];
    private List<byte> AnalyzeBuf { get; } = new(4096);
    private Timer ProcessTimer { get; set; } = default!;
    private bool IsStopTimer { get; set; }
    private DateTime AnalyzeTimeout { get; set; }

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
    public bool IsConnected => Port.IsOpen;

    /// <summary>
    ///     Connect device
    /// </summary>
    /// <param name="target">target COM port</param>
    /// <param name="option">baud rate</param>
    /// <param name="id">device id</param>
    /// <returns>result</returns>
    public bool Connect(string target, int option, byte id = 0x1) {
        // check target
        if (string.IsNullOrWhiteSpace(target))
            return false;
        // check option
        if (!BaudRates.Contains(option))
            return false;
        // check id
        if (id > 0x0F)
            return false;

        // try catch
        try {
            // set com port
            Port.PortName = target;
            Port.BaudRate = option;
            Port.Encoding = Encoding.GetEncoding(@"iso-8859-1");
            // open
            Port.Open();

            // set device id
            DeviceId = id;
            // clear receive buffer
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();
            // set event
            Port.DataReceived += PortOnDataReceived;

            // create process timer
            ProcessTimer = new Timer();
            // set timer options
            ProcessTimer.AutoReset = true;
            ProcessTimer.Interval = ProcessPeriod;
            ProcessTimer.Elapsed += ProcessTimerOnElapsed;
            // start timer
            ProcessTimer.Start();

            // connection changed event
            ConnectedMsg?.Invoke(true);

            // result ok
            return true;
        } catch (Exception e) {
            // console
            Console.WriteLine(e.Message);
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
            if (ProcessTimer.Enabled)
                // stop timer
                IsStopTimer = true;
            // close
            Port.Close();
            // reset event
            Port.DataReceived -= PortOnDataReceived;
            // connection changed event
            ConnectedMsg?.Invoke(false);
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
        // check connection state
        if (!Port.IsOpen)
            return false;

        // try catch
        try {
            // write packet
            Port.Write(packet, 0, length);
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
            (byte)id,
            (byte)CodeTypes.ReadHoldingReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF)
        };
        // get crc
        packet.AddRange(GetCrc(packet.ToArray()));
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
            (byte)id,
            (byte)CodeTypes.ReadInputReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF)
        };
        // get crc
        packet.AddRange(GetCrc(packet.ToArray()));
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
            (byte)id,
            (byte)CodeTypes.WriteSingleReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF)
        };
        // get crc
        packet.AddRange(GetCrc(packet.ToArray()));
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
            (byte)id,
            (byte)CodeTypes.WriteMultiReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF),
            (byte)(count * 2)
        };
        // check values
        foreach (var value in values) {
            packet.Add((byte)((value >> 8) & 0xFF));
            packet.Add((byte)(value & 0xFF));
        }

        // get crc
        packet.AddRange(GetCrc(packet.ToArray()));
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
            (byte)id,
            (byte)CodeTypes.WriteMultiReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF),
            (byte)length
        };
        // add string
        packet.AddRange(str.Select(c => (byte)c));
        // check string length
        if (str.Length < length)
            // add dummy data
            packet.AddRange(new byte[length - str.Length]);

        // get crc
        packet.AddRange(GetCrc(packet.ToArray()));
        // packet
        return packet.ToArray();
    }

    private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e) {
        // get all data from buffer
        var data = Port.Encoding.GetBytes(Port.ReadExisting());
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
            // return
            return;
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

            // check data header length    [ID(1) FUNC(1) LEN(1)]
            if (AnalyzeBuf.Count < 3)
                return;
            // check id
            if (AnalyzeBuf[0] != DeviceId)
                return;
            // check command
            if (!Enum.IsDefined(typeof(CodeTypes), (int)AnalyzeBuf[1]) &&
                (AnalyzeBuf[1] & 0x80) != (int)CodeTypes.Error)
                return;
            // get command
            var cmd = (CodeTypes)AnalyzeBuf[1];
            // check error
            if (((byte)cmd & 0x80) == (int)CodeTypes.Error) {
                // get error command
                cmd = (CodeTypes)((byte)cmd & 0x7F);
                // get error code
                var code = AnalyzeBuf.Count > 3 ? AnalyzeBuf[3] : (byte)0x00;
                // error invoke
                ReceivedMsg?.Invoke(CodeTypes.Error, new[] { DeviceId, (byte)cmd, (byte)0x02, (byte)0x00, code, (byte)0x00, (byte)0x00 });
                // clear buffer
                AnalyzeBuf.Clear();
            } else {
                int frame;
                // check function code
                switch (cmd) {
                    case CodeTypes.ReadHoldingReg:
                    case CodeTypes.ReadInputReg:
                        // ID(1) + FC(1) + LEN(1)=DATA_LEN + DATA(N) + CRC(2)
                        frame = AnalyzeBuf[2] + 5;
                        break;
                    case CodeTypes.WriteSingleReg:
                    case CodeTypes.WriteMultiReg:
                        // ID(1) + FC(1) + ADDR(2) + VALUE(2)/COUNT(2) + CRC(2)
                        frame = 8;
                        break;
                    case CodeTypes.Graph:
                    case CodeTypes.GraphRes:
                        // ID(1) + FC(1) + LEN(2)=DATA_LEN + DATA(N) + CRC(2)
                        frame = ((AnalyzeBuf[2] << 8) | AnalyzeBuf[3]) + 6;
                        break;
                    case CodeTypes.Error:
                        // ID(1) + FC(1) + ERROR(1) + CRC(2)
                        frame = 5;
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

                // get crc
                var crc = GetCrc(packet.Take(frame - 2)).ToArray();
                // check crc
                if (crc[0] != packet[^2] || crc[1] != packet[^1]) {
                    // debug
                    Debug.WriteLine("CRC ERROR!");
                    return;
                }

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

    /// <summary>
    ///     Get crc data
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>result</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<byte> GetCrc(IEnumerable<byte> packet) {
        var crc = new byte[] { 0xFF, 0xFF };
        ushort crcFull = 0xFFFF;
        // check total packet
        foreach (var data in packet) {
            // XOR 1 byte
            crcFull = (ushort)(crcFull ^ data);
            // cyclic redundancy check
            for (var j = 0; j < 8; j++) {
                // get LSB
                var lsb = (ushort)(crcFull & 0x0001);
                // check AND
                crcFull = (ushort)((crcFull >> 1) & 0x7FFF);
                // check LSB
                if (lsb == 0x01)
                    // XOR
                    crcFull = (ushort)(crcFull ^ 0xA001);
            }
        }

        // set CRC
        crc[1] = (byte)((crcFull >> 8) & 0xFF);
        crc[0] = (byte)(crcFull & 0xFF);

        return crc;
    }

    /// <summary>
    ///     Get port names
    /// </summary>
    /// <returns>port name</returns>
    public static IEnumerable<string> GetPortNames() {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    ///     Get baud rate
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<int> GetBaudRates() {
        return BaudRates;
    }
}