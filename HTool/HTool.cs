using System.Timers;
using HTool.Data;
using HTool.Device;
using HTool.Format;
using HTool.Type;
using HTool.Util;
using Timer = System.Timers.Timer;

namespace HTool;

/// <summary>
///     HTool library class
/// </summary>
public sealed class HTool {
    /// <summary>
    ///     Changed connection state delegate
    /// </summary>
    /// <param name="state">Connection state (true: connected, false: disconnected)</param>
    public delegate void PerformChangedConnect(bool state);

    /// <summary>
    ///     Received raw data delegate
    /// </summary>
    /// <param name="data">Raw packet data</param>
    public delegate void PerformRawData(byte[] data);

    /// <summary>
    ///     Received data delegate
    /// </summary>
    /// <param name="codeTypes">MODBUS function code</param>
    /// <param name="addr">Register address</param>
    /// <param name="data">Received data</param>
    public delegate void PerformReceivedData(CodeTypes codeTypes, int addr, IReceivedData data);

    /// <summary>
    ///     Constructor
    /// </summary>
    public HTool() {
        // set timer option
        ProcessTimer.Interval  =  Constants.ProcessPeriod;
        ProcessTimer.AutoReset =  true;
        ProcessTimer.Elapsed   += OnElapsed;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="type">type</param>
    public HTool(ComTypes type) : this() {
        // set the communication type
        SetType(type);
    }

    private ITool? Tool { get; set; }
    private Timer ProcessTimer { get; } = new();

    private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue { get; } =
        KeyedQueue<FormatMessage, FormatMessage.MessageKey>.Create(static m => m.Key, capacity: 64);

    private DateTime ConnectionTime { get; set; }
    private DateTime KeepAliveRequestTime { get; set; } = DateTime.Now;
    private DateTime KeepAliveTime { get; set; } = DateTime.Now;

    /// <summary>
    ///     Communication type
    /// </summary>
    public ComTypes Type { get; private set; }

    /// <summary>
    ///     Connection state
    /// </summary>
    public ConnectionTypes ConnectionState { get; set; }

    /// <summary>
    ///     Communication tool generation type
    /// </summary>
    public GenerationTypes Gen { get; private set; } = GenerationTypes.GenRev2;

    /// <summary>
    ///     Device information
    /// </summary>
    public FormatInfo Info { get; private set; } = new();

    /// <summary>
    ///     Enable for keep alive
    /// </summary>
    public bool EnableKeepAlive { get; set; }

    /// <summary>
    ///     Read max register count
    /// </summary>
    public static int ReadRegMaxSize => 125;

    /// <summary>
    ///     Write max register count
    /// </summary>
    public static int WriteRegMaxSize => 123;

    /// <summary>
    ///     Changed connection state event
    /// </summary>
    public event PerformChangedConnect? ChangedConnect;

    /// <summary>
    ///     Received message data event
    /// </summary>
    public event PerformReceivedData? ReceivedData;

    /// <summary>
    ///     Received message data error event
    /// </summary>
    public event ITool.PerformReceiveError? ReceiveError;

    /// <summary>
    ///     Received raw data event
    /// </summary>
    public event PerformRawData? ReceivedRawData;

    /// <summary>
    ///     Transmitted raw data event
    /// </summary>
    public event PerformRawData? TransmitRawData;

    /// <summary>
    ///     Set the communication type
    /// </summary>
    /// <param name="type">Communication type (RTU or TCP)</param>
    public void SetType(ComTypes type) {
        // check communication tool
        if (Tool != null) {
            // check connection state
            if (ConnectionState == ConnectionTypes.Connected)
                return;
            // reset event
            Tool.ChangedConnect -= OnChangedConnect;
            Tool.ReceivedData   -= OnReceivedData;
            Tool.ReceivedRaw    -= OnReceivedRaw;
            Tool.TransmitRaw    -= OnTransmitRaw;
            // dispose tool
            Tool = null;
        }

        // try catch
        try {
            // set communication type
            Type = type;
            // create communication tool
            Tool = Device.Tool.Create(type);
            // check tool
            if (Tool == null)
                // exception for tool
                throw new Exception("Unable to create a Tool communication object.");
            // set event
            Tool.ChangedConnect += OnChangedConnect;
        } catch (Exception e) {
            // console
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    ///     Connect to the device
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="option">option</param>
    /// <param name="id">id</param>
    /// <returns>result</returns>
    public bool Connect(string target, int option, byte id = 0x01) {
        // try catch
        try {
            // check communication
            if (Tool == null)
                return false;
            // connect
            if (!Tool.Connect(target, option, id))
                return false;
            // clear message queue
            MessageQue.Clear();
            // change connection state
            ConnectionState = ConnectionTypes.Connecting;
            // set connection time
            ConnectionTime = DateTime.Now;
            // set event
            Tool.ReceivedData  += OnReceivedData;
            Tool.ReceivedError += OnReceivedError;
            Tool.ReceivedRaw   += OnReceivedRaw;
            Tool.TransmitRaw   += OnTransmitRaw;
            // start process timer
            ProcessTimer.Start();
            // result
            return true;
        } catch (Exception ex) {
            // debug
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     Close the device
    /// </summary>
    public void Close() {
        // stop timer
        ProcessTimer.Stop();
        // check communication tool
        if (Tool == null)
            return;
        // reset information
        Info = new FormatInfo();
        // reset event
        Tool.ReceivedData  -= OnReceivedData;
        Tool.ReceivedError -= OnReceivedError;
        Tool.ReceivedRaw   -= OnReceivedRaw;
        Tool.TransmitRaw   -= OnTransmitRaw;
        // close
        Tool.Close();
    }

    /// <summary>
    ///     Insert message at the message
    /// </summary>
    /// <param name="msg">message</param>
    /// <param name="check">check contains for the message</param>
    /// <returns>result</returns>
    private bool Insert(FormatMessage msg, bool check = true) {
        // get the unique check option
        var mode = check ? EnqueueMode.EnforceUnique : EnqueueMode.AllowDuplicate;
        // try enqueue
        return MessageQue.TryEnqueue(msg, mode);
    }

    /// <summary>
    ///     Insert messages at the message
    /// </summary>
    /// <param name="messages">messages</param>
    /// <param name="check">check contains for the message</param>
    /// <returns>result</returns>
    private bool InsertRange(IReadOnlyList<FormatMessage> messages, bool check = true) {
        // get the unique check option
        var mode = check ? EnqueueMode.EnforceUnique : EnqueueMode.AllowDuplicate;
        // try enqueue the messages
        return MessageQue.TryEnqueueRange(messages, mode).Accepted > 0;
    }

    /// <summary>
    ///     Read holding register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="split">split address count</param>
    /// <param name="check">check the duplicate</param>
    /// <returns>result</returns>
    public bool ReadHoldingReg(ushort addr, ushort count, int split = 0, bool check = true) {
        // check communication
        if (Tool == null)
            return false;
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;
        // check the count
        if (count == 0)
            return true;

        var address = addr;
        // check split count
        if (split <= 0)
            // set max split count
            split = ReadRegMaxSize;
        // get block
        var block = (count + split - 1) / split;
        // create the messages
        var messages = new List<FormatMessage>(block);
        // check block count
        for (var i = 0; i < block; i++) {
            // get the remaining
            var remaining = count - i * split;
            // get the request count
            var request = (ushort)Math.Min(split, remaining);
            // add message
            messages.Add(new FormatMessage(CodeTypes.ReadHoldingReg, address, Tool.GetReadHoldingRegPacket(address, request)));
            // update the address
            address += request;
        }

        // insert messages
        return InsertRange(messages, check);
    }

    /// <summary>
    ///     Read input register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="split">split address count</param>
    /// <param name="check">check the duplicate</param>
    /// <returns>result</returns>
    public bool ReadInputReg(ushort addr, ushort count, int split = 0, bool check = true) {
        // check communication
        if (Tool == null)
            return false;
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;
        // check the count
        if (count == 0)
            return true;

        var address = addr;
        // check split count
        if (split <= 0)
            // set max split count
            split = ReadRegMaxSize;
        // get block
        var block = (count + split - 1) / split;
        // create the messages
        var messages = new List<FormatMessage>(block);
        // check block count
        for (var i = 0; i < block; i++) {
            // get the remaining
            var remaining = count - i * split;
            // get the request count
            var request = (ushort)Math.Min(split, remaining);
            // add message
            messages.Add(new FormatMessage(CodeTypes.ReadInputReg, address, Tool.GetReadInputRegPacket(address, request)));
            // update the address
            address += request;
        }

        // insert messages
        return InsertRange(messages, check);
    }

    /// <summary>
    ///     Write single register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <param name="check">check contains for the message</param>
    /// <returns>result</returns>
    public bool WriteSingleReg(ushort addr, ushort value, bool check = true) {
        // check communication
        if (Tool == null)
            return false;
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;

        // create message
        var msg = new FormatMessage(CodeTypes.WriteSingleReg, addr, Tool.SetSingleRegPacket(addr, value));
        // insert message
        return Insert(msg, check);
    }

    /// <summary>
    ///     Write multiple register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <param name="check">check contains for the message</param>
    /// <returns>result</returns>
    public bool WriteMultiReg(ushort addr, ushort[] values, bool check = true) {
        return WriteMultiReg(addr, values.AsSpan(), check);
    }

    /// <summary>
    ///     Write multiple register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <param name="check">check contains for the message</param>
    /// <returns>result</returns>
    public bool WriteMultiReg(ushort addr, ReadOnlySpan<ushort> values, bool check = true) {
        // check communication
        if (Tool == null)
            return false;
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;
        // check the values
        if (values.Length == 0)
            return true;

        var offset   = 0;
        var total    = values.Length;
        var blocks   = (total + WriteRegMaxSize - 1) / WriteRegMaxSize;
        var messages = new List<FormatMessage>(blocks);
        // check the offset
        while (offset < total) {
            // get the length
            var len = Math.Min(total - offset, WriteRegMaxSize);
            // get the address
            var address = (ushort)(addr + offset);
            // get the buf
            var buf = GC.AllocateUninitializedArray<ushort>(len);
            // copy to the buf
            values.Slice(offset, len).CopyTo(buf);
            // get the packet
            var packet = Tool.SetMultiRegPacket(address, buf);
            // add message
            messages.Add(new FormatMessage(CodeTypes.WriteMultiReg, address, packet));

            // update the information
            offset += len;
        }

        // insert messages
        return InsertRange(messages, check);
    }

    /// <summary>
    ///     Write string register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string value</param>
    /// <param name="length">length</param>
    /// <param name="check">check contains for the message</param>
    /// <returns>result</returns>
    public bool WriteStrReg(ushort addr, string str, int length = 0, bool check = true) {
        // check communication
        if (Tool == null)
            return false;
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;

        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // create message
        var msg = new FormatMessage(CodeTypes.WriteMultiReg, addr, Tool.SetMultiRegStrPacket(addr, str, length));
        // insert message
        return Insert(msg, check);
    }

    /// <summary>
    ///     Read information register
    /// </summary>
    /// <param name="check">Check the duplicate</param>
    /// <returns>Result of the operation</returns>
    public bool ReadInfoReg(bool check = true) {
        // check communication
        if (Tool == null)
            return false;
        // check connection state
        if (ConnectionState != ConnectionTypes.Connected)
            return false;

        // create message
        var msg = new FormatMessage(CodeTypes.ReadInfoReg, FormatMessage.EmptyAddr, Tool.GetInfoRegPacket());
        // insert message
        return Insert(msg, check);
    }

    private void OnElapsed(object? sender, ElapsedEventArgs e) {
        // check communication tool
        if (Tool == null)
            return;
        // check state
        switch (ConnectionState) {
            case ConnectionTypes.Connecting
                when (DateTime.Now - ConnectionTime).TotalSeconds < Constants.ConnectTimeout:
                // request information
                Insert(new FormatMessage(CodeTypes.ReadInfoReg, FormatMessage.EmptyAddr, Tool.GetInfoRegPacket()));
                break;
            case ConnectionTypes.Connecting:
                // close
                Close();
                break;
            case ConnectionTypes.Close:
            case ConnectionTypes.Connected:
                // check enable for keep-alive
                if (EnableKeepAlive) {
                    // check request keep-alive time laps
                    if ((DateTime.Now - KeepAliveRequestTime).TotalMilliseconds >= Constants.KeepAlivePeriod)
                        // check the empty
                        if (MessageQue.IsEmpty)
                            // insert the keep alive message
                            if (ReadInfoReg())
                                // set keep alive time
                                KeepAliveRequestTime = DateTime.Now;
                    // check keep-alive timeout
                    if ((DateTime.Now - KeepAliveTime).TotalSeconds >= Constants.KeepAliveTimeout)
                        // disconnect for tool
                        Close();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(string.Empty);
        }

        // check the empty
        if (MessageQue.IsEmpty)
            return;
        // peek the message
        if (!MessageQue.TryPeek(out var msg))
            return;
        // check activated state
        if (!msg.Activated) {
            // get packet values
            var packet = msg.Packet.ToArray();
            // write packet
            if (!Tool.Write(packet, packet.Length))
                return;
            // set activate
            msg.Activate();
            // check the not check
            if (!msg.NotCheck)
                return;
        } else {
            // check timeout
            if ((DateTime.Now - msg.ActiveTime).TotalMilliseconds < Constants.MessageTimeout)
                return;
            // deactivate message
            if (msg.Deactivate() > 0)
                return;
        }

        // try dequeue (remove the message)
        MessageQue.TryDequeue(out _);
    }

    private void OnChangedConnect(bool state) {
        // update keep-alive time
        KeepAliveRequestTime = DateTime.Now;
        KeepAliveTime        = DateTime.Now;
        // check state
        if (state)
            return;
        // change state
        ConnectionState = ConnectionTypes.Close;
        // changed connection state event
        ChangedConnect?.Invoke(false);
    }

    private void OnReceivedData(CodeTypes code, byte[] packet) {
        // check communication tool
        if (Tool == null)
            return;

        var addr = FormatMessage.EmptyAddr;
        // peek the message
        if (MessageQue.TryPeek(out var msg))
            // check queue
            if (msg is { Activated: true })
                // check code
                if (code == msg.Code || code == CodeTypes.Error) {
                    // set address
                    addr = msg.Address;
                    // try dequeue
                    MessageQue.TryDequeue(out _);
                }

        // new message
        IReceivedData? data = Type switch {
            ComTypes.Rtu => new HcRtuData(packet),
            ComTypes.Tcp => new HcTcpData(packet),
            _            => null
        };

        // check read information register
        if (code == CodeTypes.ReadInfoReg && data != null) {
            // set information
            Info = new FormatInfo(data.Data);
            // check connection state
            if (ConnectionState == ConnectionTypes.Connecting) {
                // set revision
                Tool.Revision = Info.Firmware switch {
                    > (int)GenerationTypes.GenRev2                                  => GenerationTypes.GenRev2,
                    > (int)GenerationTypes.GenRev1Plus                              => GenerationTypes.GenRev1Plus,
                    > (int)GenerationTypes.GenRev1 when Info.Model == ModelTypes.Ad => GenerationTypes.GenRev1Ad,
                    _                                                               => GenerationTypes.GenRev1
                };
                Gen = Tool.Revision;
                // change state
                ConnectionState = ConnectionTypes.Connected;
                // changed connection state event
                ChangedConnect?.Invoke(true);
            }
        }

        // check received data
        if (data == null)
            return;
        // received data
        ReceivedData?.Invoke(code, addr, data);
        // check enable the keep-alive
        if (!EnableKeepAlive)
            return;
        // update the keep alive time
        KeepAliveTime = DateTime.Now;
    }

    private void OnReceivedError(ComErrorTypes reason, object? param) {
        // error event
        ReceiveError?.Invoke(reason, param);
    }

    private void OnReceivedRaw(byte[] packet) {
        // received event
        ReceivedRawData?.Invoke(packet);
    }

    private void OnTransmitRaw(byte[] packet) {
        // transmit event
        TransmitRawData?.Invoke(packet);
    }
}