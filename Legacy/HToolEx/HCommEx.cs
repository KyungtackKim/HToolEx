using System.Timers;
using HToolEx.Data;
using HToolEx.Device;
using HToolEx.Format;
using HToolEx.Type;
using HToolEx.Util;
using JetBrains.Annotations;
using Timer = System.Timers.Timer;

namespace HToolEx;

/// <summary>
///     Hantas extension-comm library
/// </summary>
[PublicAPI]
public class HCommEx {
    /// <summary>
    ///     Connection changed message delegate
    /// </summary>
    public delegate void PerformConnectionMsgHandler(bool state);

    /// <summary>
    ///     Receive raw data delegate
    /// </summary>
    public delegate void PerformReceiveDataHandle(byte[] data, bool isTransmit = false);

    /// <summary>
    ///     Received message data delegate
    /// </summary>
    public delegate void PerformReceiveMsgHandler(CodeTypes codeTypes, int addr, IHCommData data);

    /// <summary>
    ///     Constructor
    /// </summary>
    public HCommEx() {
        // set timer option
        MessageTimer.Interval  =  MessagePeriod;
        MessageTimer.AutoReset =  true;
        MessageTimer.Elapsed   += MessageTimerOnElapsed;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="type">communication type</param>
    public HCommEx(CommTypes type) : this() {
        // setup device
        SetUp(type);
    }

    private static int MessagePeriod => 50;
    private static int MessageLockTime => 500;
    private static int MessageTimeout => 1000;
    private IHComm? Comm { get; set; }
    private Timer MessageTimer { get; } = new();
    private ConcurrentQueueWithCheck<HCommMsg> MessageQue { get; } = new();
    private DateTime ConnectionTime { get; set; }
    private DateTime KeepAliveRequestTime { get; set; } = DateTime.Now;
    private DateTime KeepAliveTime { get; set; } = DateTime.Now;
    private int Id { get; set; } = 0x01;
    private bool IsInfo { get; set; }

    public static int ReadRegMaxSize => 125;
    public static int WriteRegMaxSize => 123;

    /// <summary>
    ///     Communication type
    /// </summary>
    public CommTypes CommType { get; private set; } = CommTypes.None;

    /// <summary>
    ///     Communication connection state
    /// </summary>
    public ConnectionStateTypes CommState { get; private set; } = ConnectionStateTypes.Closed;

    /// <summary>
    ///     Enable for keep alive
    /// </summary>
    public bool EnableKeepAlive { get; set; }

    /// <summary>
    ///     Received message data event
    /// </summary>
    public event PerformReceiveMsgHandler? ReceivedData;

    /// <summary>
    ///     Received raw data event
    /// </summary>
    public event PerformReceiveDataHandle? ReceivedRawData;

    /// <summary>
    ///     Changed connection state event
    /// </summary>
    public event PerformConnectionMsgHandler? ConnectionState;

    /// <summary>
    ///     Setup communication type
    /// </summary>
    /// <param name="type">communication type</param>
    public void SetUp(CommTypes type) {
        // check communication
        if (Comm != null) {
            // reset event
            Comm.ReceivedData -= OnReceivedData;
            Comm.ReceivedMsg  -= OnReceivedMsg;
            Comm.ConnectedMsg -= CommOnConnectedMsg;
            // disable communication
            Comm = null;
        }

        // reset communication type
        CommType = CommTypes.None;
        // create communication
        Comm = CommunicationFactory.Create(type);
        // check communication
        if (Comm == null)
            return;
        // change communication
        CommType = type;
        // set connection changed event
        Comm.ConnectedMsg += CommOnConnectedMsg;
    }

    /// <summary>
    ///     Try connect to device
    /// </summary>
    /// <param name="target">target name</param>
    /// <param name="option">option</param>
    /// <param name="id">id</param>
    /// <param name="isInfo">not used for information function</param>
    /// <returns>result</returns>
    public bool Connect(string target, int option, byte id = 0x01, bool isInfo = false) {
        // try catch
        try {
            // set id
            Id = id;
            // set information use
            IsInfo = isInfo;
            // check communication
            if (Comm == null)
                return false;
            // check state
            if (CommState != ConnectionStateTypes.Closed)
                return false;
            // try connect
            if (!Comm.Connect(target, option, id))
                return false;
            // clear message
            MessageQue.Clear();
            // change connection state
            CommState = ConnectionStateTypes.Connecting;
            // set connection time
            ConnectionTime = DateTime.Now;
            // set event
            Comm.ReceivedMsg  += OnReceivedMsg;
            Comm.ReceivedData += OnReceivedData;
            // start timer
            MessageTimer.Start();

            return true;
        } catch (Exception ex) {
            // console
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    /// <summary>
    ///     Close to device
    /// </summary>
    public void Close() {
        // stop timer
        MessageTimer.Stop();
        // check communication
        if (Comm == null)
            return;
        // reset event
        Comm.ReceivedMsg  -= OnReceivedMsg;
        Comm.ReceivedData -= OnReceivedData;
        // close
        Comm.Close();
    }

    /// <summary>
    ///     Read holding register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="split">split address count</param>
    /// <returns>result</returns>
    public bool ReadHoldingReg(ushort addr, ushort count, int split = 0) {
        // check communication
        if (Comm == null)
            return false;
        // check connection state
        if (CommState != ConnectionStateTypes.Connected)
            return false;

        var res     = false;
        var address = addr;
        // check split count
        if (split == 0)
            // set max split count
            split = ReadRegMaxSize;
        // get block
        var block = count / (split + 1) + 1;
        // check block count
        for (var i = 0; i < block; i++) {
            // get request count
            var request = (ushort)split;
            // check index
            if (i == block - 1 && count % split > 0)
                // change request count
                request = (ushort)(count % split);
            // create message
            var msg = new HCommMsg(CodeTypes.ReadHoldingReg, address,
                Comm.GetReadHoldingRegPacket(address, request, Id));
            // enqueue message
            res |= EnqueueMsg(msg);
            // change address
            address += request;
        }

        return res;
    }

    /// <summary>
    ///     Read input register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="split">split address count</param>
    /// <returns>result</returns>
    public bool ReadInputReg(ushort addr, ushort count, int split = 0) {
        // check communication
        if (Comm == null)
            return false;
        // check connection state
        if (CommState != ConnectionStateTypes.Connected)
            return false;

        var res     = false;
        var address = addr;
        // check split count
        if (split == 0)
            // set max split count
            split = ReadRegMaxSize;
        // get block
        var block = count / (split + 1) + 1;
        // check block count
        for (var i = 0; i < block; i++) {
            var request = (ushort)split;
            // check index
            if (i == block - 1 && count % split > 0)
                // change request count
                request = (ushort)(count % split);
            // create message
            var msg = new HCommMsg(CodeTypes.ReadInputReg, address, Comm.GetReadInputRegPacket(address, request, Id));
            // enqueue message
            res |= EnqueueMsg(msg);
            // change address
            address += request;
        }

        return res;
    }

    /// <summary>
    ///     Write single register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <returns>result</returns>
    public bool WriteSingleReg(ushort addr, ushort value) {
        // check communication
        if (Comm == null)
            return false;
        // check connection state
        if (CommState != ConnectionStateTypes.Connected)
            return false;

        // create message
        var msg = new HCommMsg(CodeTypes.WriteSingleReg, addr, Comm.SetSingleRegPacket(addr, value, Id));
        // enqueue message
        return EnqueueMsg(msg, true);
    }

    /// <summary>
    ///     Write multiple register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <returns>result</returns>
    public bool WriteMultiReg(ushort addr, ushort[] values) {
        // check communication
        if (Comm == null)
            return false;
        // check connection state
        if (CommState != ConnectionStateTypes.Connected)
            return false;

        var res    = false;
        var count  = values.Length;
        var offset = 0;
        // get block
        var block = count / (WriteRegMaxSize + 1) + 1;
        // check block count
        for (var i = 0; i < block; i++) {
            // get address
            var address = (ushort)(addr + offset);
            // create message
            var msg = new HCommMsg(CodeTypes.WriteMultiReg, address,
                Comm.SetMultiRegPacket(address, values.Skip(offset).Take(WriteRegMaxSize).ToArray(), Id));
            // enqueue message
            res |= EnqueueMsg(msg);
            // change address
            offset += i < block - 1 ? WriteRegMaxSize : count % WriteRegMaxSize;
        }

        return res;
    }

    /// <summary>
    ///     Write string register
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string value</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    public bool WriteStrReg(ushort addr, string str, int length = 0) {
        // check communication
        if (Comm == null)
            return false;
        // check connection state
        if (CommState != ConnectionStateTypes.Connected)
            return false;

        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // create message
        var msg = new HCommMsg(CodeTypes.WriteMultiReg, addr, Comm.SetMultiRegStrPacket(addr, str, length, Id));

        // enqueue message
        return EnqueueMsg(msg);
    }

    /// <summary>
    ///     Enqueue message queue
    /// </summary>
    /// <param name="msg">message</param>
    /// <param name="duplicate">check duplicate</param>
    /// <returns>result</returns>
    private bool EnqueueMsg(HCommMsg msg, bool duplicate = false) {
        // check duplicate
        if (duplicate)
            // enqueue message
            return MessageQue.Enqueue(msg);
        // check contains item
        return MessageQue.Contains(msg, new HCommMsgComparer()) || MessageQue.Enqueue(msg);
    }

    private void MessageTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        // check communication
        if (Comm == null)
            return;
        // check communication state
        switch (CommState) {
            case ConnectionStateTypes.Connecting when (DateTime.Now - ConnectionTime).TotalSeconds < 5:
                // check using information
                if (IsInfo) {
                    // change connection state
                    CommState = ConnectionStateTypes.Connected;
                    // connection changed event
                    ConnectionState?.Invoke(true);
                } else {
                    // enqueue message
                    EnqueueMsg(new HCommMsg(CodeTypes.ReadInputReg, 1,
                        Comm.GetReadInputRegPacket(1, FormatInfo.Count, Comm.DeviceId)));
                }

                break;
            case ConnectionStateTypes.Connecting:
                // close
                Close();
                break;
            case ConnectionStateTypes.Connected:
                // check enable for keep-alive
                if (EnableKeepAlive) {
                    // check request keep-alive time laps
                    if ((DateTime.Now - KeepAliveRequestTime).TotalMilliseconds >= 3000)
                        // check queue count
                        if (MessageQue.Count() == 0)
                            // insert the keep alive message
                            if (EnqueueMsg(new HCommMsg(CodeTypes.ReadInputReg, 1,
                                    Comm.GetReadInputRegPacket(1, FormatInfo.Count, Comm.DeviceId))))
                                // set keep alive time
                                KeepAliveRequestTime = DateTime.Now;
                    // check keep-alive timeout
                    if ((DateTime.Now - KeepAliveTime).TotalSeconds >= 10)
                        // disconnect for tool
                        Close();
                }

                break;
            case ConnectionStateTypes.Closed:
                break;
            default:
                throw new ArgumentOutOfRangeException(string.Empty);
        }

        // check queue count
        if (MessageQue.Count() == 0)
            return;
        // try peek queue
        if (!MessageQue.Peek(out var queue))
            return;
        // check activation state
        if (queue.IsActive) {
            // check timeout
            if (!((DateTime.Now - queue.Time).TotalMilliseconds > MessageTimeout))
                return;
            // reset activation
            queue.IsActive = false;
            // reset timeout time
            queue.Time = DateTime.Now;
            // change retry count
            queue.RetryCount -= 1;
            // check retry count
            if (queue.RetryCount > 0)
                return;

            // timeout error
            Console.WriteLine($"Timeout message: {queue.CodeTypes}, {queue.Address}");
            // try dequeue
            MessageQue.TryDequeue(out _);
        } else {
            // get packet array
            var packet = queue.Packet.ToArray();
            // write packet
            if (!Comm.Write(packet, packet.Length))
                return;
            // transmit event
            ReceivedRawData?.Invoke(packet, true);
            // set timeout time
            queue.Time = DateTime.Now;
            // set activation
            queue.IsActive = true;
        }
    }

    private void CommOnConnectedMsg(bool state) {
        // check state
        if (state)
            return;
        // change state
        CommState = ConnectionStateTypes.Closed;
        // connection changed event
        ConnectionState?.Invoke(false);
    }

    private void OnReceivedMsg(CodeTypes codeTypes, byte[] packet) {
        var addr = 0;
        // check count and queue
        if (MessageQue.Peek(out var queue))
            // check queue
            if (queue is { IsActive: true })
                // check code
                if (codeTypes == queue.CodeTypes) {
                    // set address
                    addr = queue.Address;
                    // try dequeue
                    while (!MessageQue.TryDequeue(out _)) { }
                }

        // check connection state
        if (CommState == ConnectionStateTypes.Connecting) {
            // change connection state
            CommState = ConnectionStateTypes.Connected;
            // connection changed event
            ConnectionState?.Invoke(true);
        }

        // new message
        IHCommData? msg = CommType switch {
            // create rtu message data
            CommTypes.Rtu => new HCommRtuData(packet),
            // create tcp message data
            CommTypes.Tcp => new HCommTcpData(packet),
            // default
            _ => null
        };

        // check message
        if (msg == null)
            return;
        // update message
        ReceivedData?.Invoke(codeTypes, addr, msg);
        // check enable the keep-alive
        if (!EnableKeepAlive)
            return;
        // update the keep alive time
        KeepAliveTime = DateTime.Now;
    }

    private void OnReceivedData(byte[] packet) {
        // received event
        ReceivedRawData?.Invoke(packet);
    }
}