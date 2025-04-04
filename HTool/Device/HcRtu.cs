﻿using System.Collections.Concurrent;
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
    private static readonly int[] BaudRates = [9600, 19200, 38400, 57600, 115200, 230400];
    private SerialPort Port { get; } = new();
    private ConcurrentQueue<byte> ReceiveBuf { get; } = [];
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
            ProcessTimer.Interval = Constants.ProcessPeriod;
            ProcessTimer.Elapsed += ProcessTimerOnElapsed;
            // start timer
            ProcessTimer.Start();

            // connection changed event
            ChangedConnect?.Invoke(true);

            // result ok
            return true;
        }
        catch (Exception e) {
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
        }
        catch (Exception ex) {
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
        }
        catch (Exception ex) {
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
            DeviceId, (byte)CodeTypes.ReadHoldingReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF)
        };
        // get crc
        packet.AddRange(Utils.CalculateCrc(packet));
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
            DeviceId, (byte)CodeTypes.ReadInputReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((count >> 8) & 0xFF),
            (byte)(count & 0xFF)
        };
        // get crc
        packet.AddRange(Utils.CalculateCrc(packet));
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
            DeviceId, (byte)CodeTypes.WriteSingleReg,
            (byte)((addr >> 8) & 0xFF),
            (byte)(addr & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF)
        };
        // get crc
        packet.AddRange(Utils.CalculateCrc(packet));
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
            DeviceId, (byte)CodeTypes.WriteMultiReg,
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
        packet.AddRange(Utils.CalculateCrc(packet));
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
            DeviceId, (byte)CodeTypes.WriteMultiReg,
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
        packet.AddRange(Utils.CalculateCrc(packet));
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
            DeviceId, (byte)CodeTypes.ReadInfoReg
        };
        // get crc
        packet.AddRange(Utils.CalculateCrc(packet));
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
        // get all data from buffer
        var data = Port.Encoding.GetBytes(Port.ReadExisting());
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
            // check empty for receive buffer
            while (!ReceiveBuf.IsEmpty) {
                // get data
                if (ReceiveBuf.TryDequeue(out var d))
                    // add data
                    AnalyzeBuf.Write(d);
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
            }

            // check analyze count
            if (AnalyzeBuf.Available > 0)
                // check analyze timeout
                if ((DateTime.Now - AnalyzeTimeout).TotalMilliseconds > Constants.ProcessTimeout)
                    // clear buffer
                    AnalyzeBuf.Clear();

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
            var cmd = (CodeTypes)value;
            // check error
            if ((value & (int)CodeTypes.Error) == (int)CodeTypes.Error) {
                // get error command
                cmd = (CodeTypes)((byte)cmd & 0x7F);
                // get error code
                var code = AnalyzeBuf.Available > HeaderSize ? AnalyzeBuf.Peek(HeaderSize) : (byte)0x00;
                // error invoke
                ReceivedData?.Invoke(CodeTypes.Error, [DeviceId, (byte)cmd, 0x02, 0x00, code, 0x00, 0x00]);
                // clear buffer
                AnalyzeBuf.Clear();
            }
            else {
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
                    CodeTypes.Error => 5,
                    // exception
                    _ => throw new ArgumentOutOfRangeException(string.Empty)
                };

                // check frame length
                if (AnalyzeBuf.Available < frame)
                    return;
                // get packet
                var packet = AnalyzeBuf.ReadBytes(frame);
                // get crc
                var crc = Utils.CalculateCrc(packet[..^2]).ToArray();
                // check crc
                if (crc[0] == packet[^2] && crc[1] == packet[^1])
                    // update event
                    ReceivedData?.Invoke(cmd, packet);
            }
        }
        finally {
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}