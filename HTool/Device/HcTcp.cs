using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Timers;
using HTool.Type;
using HTool.Util;
using SuperSimpleTcp;
using Timer = System.Timers.Timer;

namespace HTool.Device;

/// <summary>
///     MODBUS TCP class
/// </summary>
public sealed class HcTcp : ITool {
    private const int ErrorFrameSize = 9;
    private SimpleTcpClient? Client { get; set; }
    private ConcurrentQueue<(byte[] buf, int len)> ReceiveBuf { get; } = [];
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
    ///     Received data error event
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
            // check the received buffer
            foreach (var value in ReceiveBuf)
                // return the pool
                ArrayPool<byte>.Shared.Return(value.buf);
            // clear receive buffer
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();
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
            // set the settings (WARNING: 반드시 아래 설정을 해줘야 하며, 안할 경우 데이터 수신시 데이터 오류가 발생할 수 있다)
            Client.Settings.UseAsyncDataReceivedEvents = false;
            Client.Settings.StreamBufferSize           = 16 * 1024;
            // connect
            Client.Connect();

            // set device id
            DeviceId = id;
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
        // allocation the packet
        var packet = GC.AllocateUninitializedArray<byte>(12);
        // set the MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x06;
        packet[6] = 0x00;
        // set the PDU values
        packet[7]  = (byte)CodeTypes.ReadHoldingReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        // packet
        return packet;
    }

    /// <summary>
    ///     Get input register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <returns>packet</returns>
    public byte[] GetReadInputRegPacket(ushort addr, ushort count) {
        // allocation the packet
        var packet = GC.AllocateUninitializedArray<byte>(12);
        // set the MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x06;
        packet[6] = 0x00;
        // set the PDU values
        packet[7]  = (byte)CodeTypes.ReadInputReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        // packet
        return packet;
    }

    /// <summary>
    ///     Set single register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <returns>packet</returns>
    public byte[] SetSingleRegPacket(ushort addr, ushort value) {
        // allocation the packet
        var packet = GC.AllocateUninitializedArray<byte>(12);
        // set the MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x06;
        packet[6] = 0x00;
        // set the PDU values
        packet[7]  = (byte)CodeTypes.WriteSingleReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((value >> 8) & 0xFF);
        packet[11] = (byte)(value        & 0xFF);
        // packet
        return packet;
    }

    /// <summary>
    ///     Set multiple register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <returns>packet</returns>
    public byte[] SetMultiRegPacket(ushort addr, ReadOnlySpan<ushort> values) {
        var index = 13;
        // get count
        var count = values.Length;
        // get the pdu size
        var pdu = 7 + count * 2;
        // get the packet size
        var size = 6 + pdu;
        // allocation the packet
        var packet = GC.AllocateUninitializedArray<byte>(size);
        // set the MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = (byte)((pdu >> 8) & 0xFF);
        packet[5] = (byte)(pdu        & 0xFF);
        packet[6] = 0x00;
        // set the PDU values
        packet[7]  = (byte)CodeTypes.WriteMultiReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        packet[12] = (byte)(count * 2);

        // check the values
        foreach (var value in values) {
            // set the value
            packet[index++] = (byte)((value >> 8) & 0xFF);
            packet[index++] = (byte)(value        & 0xFF);
        }

        // packet
        return packet;
    }

    /// <summary>
    ///     Set multiple register ascii packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    public byte[] SetMultiRegStrPacket(ushort addr, string str, int length) {
        var index = 13;
        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // get count
        var count = length / 2;
        // get the pdu size
        var pdu = index + count * 2;
        // get the packet size
        var size = 6 + pdu;
        // allocation the packet
        var packet = GC.AllocateUninitializedArray<byte>(size);
        // set the MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = (byte)((pdu >> 8) & 0xFF);
        packet[5] = (byte)(pdu        & 0xFF);
        packet[6] = 0x00;
        // set the PDU values
        packet[7]  = (byte)CodeTypes.WriteMultiReg;
        packet[8]  = (byte)((addr >> 8)  & 0xFF);
        packet[9]  = (byte)(addr         & 0xFF);
        packet[10] = (byte)((count >> 8) & 0xFF);
        packet[11] = (byte)(count        & 0xFF);
        packet[12] = (byte)length;
        // set the string values
        foreach (var c in str)
            // set the value
            packet[index++] = (byte)c;

        // set the padding zeros
        while (index < 13 + length)
            // set the padding value
            packet[index++] = 0;
        // packet
        return packet;
    }

    /// <summary>
    ///     Get information register packet
    /// </summary>
    /// <returns>result</returns>
    public byte[] GetInfoRegPacket() {
        // allocation the packet
        var packet = GC.AllocateUninitializedArray<byte>(8);
        // set the MBAP header
        packet[0] = (byte)((DeviceId >> 8) & 0xFF);
        packet[1] = (byte)(DeviceId        & 0xFF);
        packet[2] = 0x00;
        packet[3] = 0x00;
        packet[4] = 0x00;
        packet[5] = 0x02;
        packet[6] = 0x00;
        // set the PDU values
        packet[7] = (byte)CodeTypes.ReadInfoReg;
        // packet
        return packet;
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
        // get the data
        var data = e.Data;
        // check the data
        if (data.Array is null)
            return;
        // get the length
        var read = data.Count;
        // check the length
        if (read < 1)
            return;

        // rent the pool
        var chunk = ArrayPool<byte>.Shared.Rent(read);
        // copy the data
        Buffer.BlockCopy(data.Array, data.Offset, chunk, 0, read);
        // enqueue the data block
        ReceiveBuf.Enqueue((chunk, read));
        // check the received raw event
        if (ReceivedRaw is null)
            return;
        // create the space
        var raw = GC.AllocateUninitializedArray<byte>(read);
        // copy the data
        Buffer.BlockCopy(chunk, 0, raw, 0, read);
        // event
        ReceivedRaw(raw);
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
            // check the received buffer
            foreach (var value in ReceiveBuf)
                // return the pool
                ArrayPool<byte>.Shared.Return(value.buf);
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

            // check data header length    [TID(2) PID(2) LEN(2) UID(1) FC(1)]
            if (AnalyzeBuf.Available < HeaderSize)
                return;
            // get command value
            var value = AnalyzeBuf.Peek(FunctionPos);
            // get the error
            var isError = (value & 0x80) != 0x00;
            // get the base function
            var baseFc = (byte)(value & 0x7F);
            // check the command
            if (!Tool.IsKnownCode(baseFc) && !isError)
                return;
            // get the command
            var cmd = !isError ? (CodeTypes)baseFc : CodeTypes.Error;
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