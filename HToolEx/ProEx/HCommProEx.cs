using System.Diagnostics;
using System.Timers;
using HToolEx.Data;
using HToolEx.ProEx.Format;
using HToolEx.ProEx.Manager;
using HToolEx.ProEx.Type;
using HToolEx.ProEx.Util;
using HToolEx.Type;
using HToolEx.Util;
using JetBrains.Annotations;
using Timer = System.Timers.Timer;

namespace HToolEx.ProEx;

/// <summary>
///     Hantas extension-comm pro-x library
/// </summary>
public class HCommProEx {
    /// <summary>
    ///     Changes tool list delegate
    /// </summary>
    public delegate void PerformChangeTools(IReadOnlyList<FormatToolInfo>? tools);

    /// <summary>
    ///     Received event data message delegate
    /// </summary>
    public delegate void PerformReceiveDataEvent(FormatEventExtended data);

    /// <summary>
    ///     Received job event data message delegate
    /// </summary>
    public delegate void PerformReceiveJobEvent(FormatJobEvent data);

    /// <summary>
    ///     Received message delegate
    /// </summary>
    public delegate void PerformReceiveMsg(FormatMessage msg);

    /// <summary>
    ///     Constructor
    /// </summary>
    public HCommProEx() { }

    private static int ProcessPeriod => 100;
    private static int MessageTimeout => 1000;
    private static int KeepAliveTimeout => 10;

    private SessionManager Session { get; } = new();

    /// <summary>
    ///     File manager
    /// </summary>
    [PublicAPI]
    public FtpManager Ftp { get; } = new();

    private Timer ProcessTimer { get; set; } = null!;
    private ConcurrentQueueWithCheck<FormatMessageRequest> MessageQue { get; } = new();

    private bool IsStopTimer { get; set; }
    private DateTime KeepAliveTime { get; set; }

    /// <summary>
    ///     Remote-Pro X revision
    /// </summary>
    [PublicAPI]
    public static int Revision => 0;

    /// <summary>
    ///     Communication connection state
    /// </summary>
    [PublicAPI]
    public ConnectionStateTypes CommState { get; private set; } = ConnectionStateTypes.Closed;

    /// <summary>
    ///     Received message event
    /// </summary>
    [PublicAPI]
    public event PerformReceiveMsg? ReceivedMsg;

    /// <summary>
    ///     Received event data message event
    /// </summary>
    [PublicAPI]
    public event PerformReceiveDataEvent? ReceivedEventData;

    /// <summary>
    ///     Received job event data message event
    /// </summary>
    [PublicAPI]
    public event PerformReceiveJobEvent? ReceivedJobEventData;

    /// <summary>
    ///     Changes member tool list event
    /// </summary>
    [PublicAPI]
    public event PerformChangeTools? ChangesMemberTools;

    /// <summary>
    ///     Change scan tool list event
    /// </summary>
    [PublicAPI]
    public event PerformChangeTools? ChangesScanTools;

    /// <summary>
    ///     Received data event
    /// </summary>
    [PublicAPI]
    public event HCommEx.PerformReceiveMsgHandler? ReceivedData;

    /// <summary>
    ///     Changed connection state event
    /// </summary>
    [PublicAPI]
    public event HCommEx.PerformConnectionMsgHandler? ConnectionState;

    /// <summary>
    ///     Connect to ParaMon-Pro X
    /// </summary>
    /// <param name="ip">ip address</param>
    /// <param name="port">port</param>
    /// <returns></returns>
    [PublicAPI]
    public async Task<bool> Connect(string ip, int port) {
        // check connection state
        if (CommState == ConnectionStateTypes.Connected)
            // disconnect
            Disconnect();

        // set connection state
        CommState = ConnectionStateTypes.Connecting;
        // connect session
        if (Session.Connect(ip, port, out _)) {
            // connect ftp
            var ftp = await Ftp.Connect(ip, FtpManager.FtpPort);
            // connect ftp
            if (!ftp.result) {
                // disconnect session
                Session.Disconnect();
            } else {
                // set event
                Session.ReceivedMsg += OnReceivedMsg;
                // clear queue
                MessageQue.Clear();
                // create process timer
                ProcessTimer = new Timer();
                // set timer options
                ProcessTimer.AutoReset = true;
                ProcessTimer.Interval = ProcessPeriod;
                ProcessTimer.Elapsed += ProcessTimerOnElapsed;
                // start timer
                ProcessTimer.Start();
                // set connection state
                CommState = ConnectionStateTypes.Connected;
                // connection changed event
                ConnectionState?.Invoke(true);
                // result
                return true;
            }
        }

        // set connection state
        CommState = ConnectionStateTypes.Closed;
        // connection changed event
        ConnectionState?.Invoke(false);

        return false;
    }

    /// <summary>
    ///     Disconnect to ParaMon-Pro X
    /// </summary>
    [PublicAPI]
    public void Disconnect() {
        // reset event
        Session.ReceivedMsg -= OnReceivedMsg;
        // check process timer
        if (ProcessTimer is { Enabled: true })
            // stop timer
            IsStopTimer = true;

        // check session
        if (Session.IsConnected)
            // disconnect
            Session.Disconnect();
        // check ftp
        if (Ftp.IsConnected)
            // disconnect
            Ftp.Disconnect();

        // set connection state
        CommState = ConnectionStateTypes.Closed;
        // connection changed event
        ConnectionState?.Invoke(false);
    }

    /// <summary>
    ///     Request message
    /// </summary>
    /// <param name="msg">message</param>
    [PublicAPI]
    public bool RequestMessage(FormatMessageRequest msg) {
        // enqueue message
        return MessageQue.Enqueue(msg);
    }

    /// <summary>
    ///     Read holding register to ParaMon-Pro X
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="split">split address count</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool ReadHoldingReg(ushort addr, ushort count, int split = 0) {
        // check communication
        if (CommState != ConnectionStateTypes.Connected)
            return false;
        // check selected id
        if (ToolManager.SelectedTool < 0 || ToolManager.SelectedTool >= 0xFF)
            return false;

        var res = false;
        var address = addr;
        // check split count
        if (split == 0)
            // set max split count
            split = HCommEx.ReadRegMaxSize;
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
            var msg = CreateRequestPacket(MessageIdTypes.ModbusRequest, Revision,
                ProPacket.GetReadHoldingRegPacketFromPro(address, request, (byte)ToolManager.SelectedTool));
            // request message
            res |= RequestMessage(msg);
            // change address
            address += request;
        }

        return res;
    }

    /// <summary>
    ///     Read input register to ParaMon-Pro X
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="split">split address count</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool ReadInputReg(ushort addr, ushort count, int split = 0) {
        // check communication
        if (CommState != ConnectionStateTypes.Connected)
            return false;
        // check selected id
        if (ToolManager.SelectedTool < 0 || ToolManager.SelectedTool >= 0xFF)
            return false;

        var res = false;
        var address = addr;
        // check split count
        if (split == 0)
            // set max split count
            split = HCommEx.ReadRegMaxSize;
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
            var msg = CreateRequestPacket(MessageIdTypes.ModbusRequest, Revision,
                ProPacket.GetReadInputRegPacketFromPro(address, request, (byte)ToolManager.SelectedTool));
            // request message
            res |= RequestMessage(msg);
            // change address
            address += request;
        }

        return res;
    }

    /// <summary>
    ///     Write single register to ParaMon-Pro X
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool WriteSingleReg(ushort addr, ushort value) {
        // check communication
        if (CommState != ConnectionStateTypes.Connected)
            return false;
        // check selected id
        if (ToolManager.SelectedTool < 0 || ToolManager.SelectedTool >= 0xFF)
            return false;

        // create message
        var msg = CreateRequestPacket(MessageIdTypes.ModbusRequest, Revision,
            ProPacket.SetSingleRegPacketFromPro(addr, value, (byte)ToolManager.SelectedTool));
        // enqueue message
        return RequestMessage(msg);
    }

    /// <summary>
    ///     Write multiple register to ParaMon-Pro X
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool WriteMultiReg(ushort addr, ushort[] values) {
        // check communication
        if (CommState != ConnectionStateTypes.Connected)
            return false;
        // check selected id
        if (ToolManager.SelectedTool < 0 || ToolManager.SelectedTool >= 0xFF)
            return false;

        var res = false;
        var count = values.Length;
        var offset = 0;
        // get block
        var block = count / (HCommEx.WriteRegMaxSize + 1) + 1;
        // check block count
        for (var i = 0; i < block; i++) {
            // get address
            var address = (ushort)(addr + offset);
            // create message
            var msg = CreateRequestPacket(MessageIdTypes.ModbusRequest, Revision,
                ProPacket.SetMultiRegPacketFromPro(address, values.Skip(offset).Take(HCommEx.WriteRegMaxSize).ToArray(),
                    (byte)ToolManager.SelectedTool));
            // request message
            res |= RequestMessage(msg);
            // change address
            offset += i < block - 1 ? HCommEx.WriteRegMaxSize : count % HCommEx.WriteRegMaxSize;
        }

        return res;
    }

    /// <summary>
    ///     Write string register to ParaMon-Pro X
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string value</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool WriteStrReg(ushort addr, string str, int length = 0) {
        // check communication
        if (CommState != ConnectionStateTypes.Connected)
            return false;
        // check selected id
        if (ToolManager.SelectedTool < 0 || ToolManager.SelectedTool >= 0xFF)
            return false;

        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // create message
        var msg = CreateRequestPacket(MessageIdTypes.ModbusRequest, Revision,
            ProPacket.SetMultiRegStrPacketFromPro(addr, str, length, (byte)ToolManager.SelectedTool));
        // enqueue message
        return RequestMessage(msg);
    }

    private void ProcessTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        // check stop
        if (IsStopTimer) {
            // reset stop
            IsStopTimer = false;
            // reset event
            ProcessTimer.Elapsed -= ProcessTimerOnElapsed;
            // stop timer
            ProcessTimer.Stop();
            // dispose timer
            ProcessTimer.Dispose();
            // clear timer
            ProcessTimer = null!;
        } else {
            // check empty queue
            if (MessageQue.Count() == 0) {
                // check request member tools time
                if (!((DateTime.Now - KeepAliveTime).TotalSeconds > KeepAliveTimeout))
                    return;
                // set time
                KeepAliveTime = DateTime.Now;
                // request member tools
                RequestMessage(CreateRequestPacket(MessageIdTypes.KeepAlive, Revision));
            }

            // peek queue
            if (!MessageQue.Peek(out var msg))
                return;

            // check activation status
            if (!msg.IsActive) {
                var values = msg.Message.GetValues();
                // write packet
                if (!Session.Write(values))
                    return;
                // set activation status
                msg.IsActive = true;
                msg.ActiveTime = DateTime.Now;
            }
            // check activation timeout
            else if ((DateTime.Now - msg.ActiveTime).TotalMilliseconds > MessageTimeout) {
                // reset activation status
                msg.IsActive = false;
                msg.ActiveTime = DateTime.Now;
                // change retry count
                if (--msg.Retry == 0)
                    // disconnect
                    Disconnect();
            }

            // check not acknowledge
            if (msg.IsNotAck)
                // remove message
                MessageQue.TryDequeue(out _);
        }
    }

    private void OnReceivedMsg(FormatMessage msg) {
        // debug
        Debug.WriteLine($@"RECEIVED MESSAGE: {msg.Header?.Id}");
        // check id
        if (msg.Header == null)
            return;
        var addr = 0;
        // check queue
        if (MessageQue.Count() > 0)
            // peek queue
            if (MessageQue.Peek(out var request))
                // check header
                if (request.Message.Header != null)
                    // check id
                    if (msg.Header.Id == request.Message.Header.Id + 1 ||
                        msg.Header.Id == MessageIdTypes.CommandAccepted ||
                        msg.Header.Id == MessageIdTypes.CommandError ||
                        msg.Header.Id == MessageIdTypes.KeepAlive) {
                        // check modbus id
                        if (msg.Header.Id == MessageIdTypes.ModbusReply)
                            // set address
                            addr = request.Address;
                        // remove queue
                        MessageQue.TryDequeue(out _);
                    }

        bool changed;
        // check message id
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (msg.Header.Id) {
            case MessageIdTypes.MemberToolReply:
                // get member tools
                var members = ToolManager.GetMemberTools();
                // check members
                if (members == null)
                    break;
                // get member tool count
                var member = (msg.Values[0] << 8) | msg.Values[1];
                // get changed status
                changed = member != members.Count;
                // check tool count
                for (var i = 0; i < member; i++) {
                    // get tool information
                    var tool = new FormatToolInfo(msg.Values.Skip(2 + FormatToolInfo.MemberSize * i)
                        .Take(FormatToolInfo.MemberSize).ToArray());
                    // check tool information
                    if (string.IsNullOrWhiteSpace(tool.Name))
                        continue;
                    // check member tools
                    if (members.Contains(tool, new FormatToolInfoComparer()))
                        continue;
                    // set changed
                    changed = true;
                }

                // check changed
                if (!changed)
                    break;

                // clear member tools
                ToolManager.ClearMemberTools();
                // check tool count
                for (var i = 0; i < member; i++) {
                    // get tool information
                    var tool = new FormatToolInfo(msg.Values.Skip(2 + FormatToolInfo.MemberSize * i)
                        .Take(FormatToolInfo.MemberSize).ToArray());
                    // check tool information
                    if (string.IsNullOrWhiteSpace(tool.Name))
                        continue;
                    // add tool
                    ToolManager.AddMemberTools(tool);
                }

                // member tool list event
                ChangesMemberTools?.Invoke(ToolManager.GetMemberTools());

                break;
            case MessageIdTypes.ScanToolReply:
                // get scan tools
                var scans = ToolManager.GetScanTools();
                // check scans
                if (scans == null)
                    break;
                // get scan tool count
                var scan = (msg.Values[0] << 8) | msg.Values[1];
                // get changed status
                changed = scan != scans.Count;
                // check tool count
                for (var i = 0; i < scan; i++) {
                    // get tool information
                    var tool = new FormatToolInfo(msg.Values.Skip(2 + FormatToolInfo.ScanSize * i)
                        .Take(FormatToolInfo.ScanSize).ToArray());
                    // check scan tools
                    if (scans.Contains(tool, new FormatToolInfoComparer()))
                        continue;
                    // set changed
                    changed = true;
                }

                // check changed
                if (!changed)
                    break;

                // clear scan tools
                ToolManager.ClearScanTools();
                // check tool count
                for (var i = 0; i < scan; i++) {
                    // get tool information
                    var tool = new FormatToolInfo(msg.Values.Skip(2 + FormatToolInfo.ScanSize * i)
                        .Take(FormatToolInfo.ScanSize).ToArray());
                    // add tool
                    ToolManager.AddScanTools(tool);
                }

                // scan tool list event
                ChangesScanTools?.Invoke(ToolManager.GetScanTools());

                break;
            case MessageIdTypes.JobEvent:
                // get job event data
                var jobData = new FormatJobEvent(msg.Values);
                // check date-time
                if (jobData.Date == DateTime.MinValue || jobData.Time == DateTime.MinValue)
                    return;
                // create message
                var ackJobEvent = CreateRequestPacket(MessageIdTypes.JobEventAcknowledge);
                // not acknowledge state
                ackJobEvent.IsNotAck = true;
                // acknowledge
                RequestMessage(ackJobEvent);
                // event message
                ReceivedJobEventData?.Invoke(jobData);
                // check job event id
                if (jobData.Id > 0)
                    // request event data
                    RequestMessage(CreateRequestPacket(MessageIdTypes.OldEventRequest,
                        values: [
                            (byte)((jobData.Id >> 24) & 0xFF),
                            (byte)((jobData.Id >> 16) & 0xFF),
                            (byte)((jobData.Id >> 8) & 0xFF),
                            (byte)(jobData.Id & 0xFF)
                        ]));

                break;
            case MessageIdTypes.LastEvent:
            case MessageIdTypes.OldEventReply:
                // get event data
                var eventData = new FormatEventExtended(msg.Values, $"0.{msg.Header?.Revision}");
                // check event data id
                if (eventData.Id == 0)
                    break;
                // check message id
                if (msg.Header?.Id == MessageIdTypes.LastEvent) {
                    // create message
                    var ackEvent = CreateRequestPacket(MessageIdTypes.LastEventAcknowledge,
                        values: [
                            (byte)((eventData.Id >> 24) & 0xFF),
                            (byte)((eventData.Id >> 16) & 0xFF),
                            (byte)((eventData.Id >> 8) & 0xFF),
                            (byte)(eventData.Id & 0xFF)
                        ]);
                    // not acknowledge state
                    ackEvent.IsNotAck = true;
                    // acknowledge
                    RequestMessage(ackEvent);
                }

                // event message
                ReceivedEventData?.Invoke(eventData);

                break;
            case MessageIdTypes.ModbusReply:
                // create message
                IHCommData data = new HCommTcpData(msg.Values);
                // update message
                ReceivedData?.Invoke(data.CodeTypes, addr, data);

                break;
        }

#if DEBUG
        // debug
        Debug.WriteLine($"{msg.Header?.Id}. {msg.Header?.Length}");
#endif
        // reset keep alive time
        KeepAliveTime = DateTime.Now;
        // message
        ReceivedMsg?.Invoke(msg);
    }

    /// <summary>
    ///     Create message packet
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="revision">revision</param>
    /// <param name="values">values</param>
    /// <returns>message</returns>
    [PublicAPI]
    public static FormatMessageRequest CreateRequestPacket(MessageIdTypes id, int revision = 0, byte[]? values = null) {
        // get length
        var length = FormatMessageInfo.Size + (values?.Length ?? 0);
        // create request message
        var message = new FormatMessageRequest {
            // set message
            Message = new FormatMessage([
                // length
                (byte)((length >> 8) & 0xFF),
                (byte)(length & 0xFF),
                // message id
                (byte)(((int)id >> 8) & 0xFF),
                (byte)((int)id & 0xFF),
                // revision
                (byte)((revision >> 8) & 0xFF),
                (byte)(revision & 0xFF),
                // dummy
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00
            ])
        };
        // check values
        if (values == null)
            return message;

        // check message id
        if (id == MessageIdTypes.ModbusRequest) {
            // get code
            var code = (CodeTypes)values[7];
            // get address
            var address = (values[8] << 8) | values[9];
            // set code and address
            message.Code = code;
            message.Address = address;
        }

        // create values
        message.Message.Values = new byte[values.Length];
        // check length
        for (var i = 0; i < values.Length; i++)
            // set value
            message.Message.Values[i] = values[i];

        // return message
        return message;
    }
}