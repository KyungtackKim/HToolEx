using System.Collections.Concurrent;
using System.Net;
using System.Timers;
using HToolEx.ProEx.Format;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;
using SuperSimpleTcp;
using DataReceivedEventArgs = SuperSimpleTcp.DataReceivedEventArgs;
using Timer = System.Timers.Timer;

namespace HToolEx.ProEx.Manager;

/// <summary>
///     Session management class for ParaMon-Pro X
/// </summary>
public class SessionManager {
    /// <summary>
    ///     Connection message delegate
    /// </summary>
    public delegate void PerformConnectMsg(bool state);

    /// <summary>
    ///     Received data delegate
    /// </summary>
    public delegate void PerformReceiveData(byte[] packet);

    /// <summary>
    ///     Received message delegate
    /// </summary>
    public delegate void PerformReceiveMsg(FormatMessage msg);

    /// <summary>
    ///     Constructor
    /// </summary>
    public SessionManager() { }

    private static int ProcessPeriod => 100;
    private static int ProcessLockTime => 500;
    private static int ProcessTimeout => 3000;

    private SimpleTcpClient? Client { get; set; }
    private ConcurrentQueue<byte> ReceiveBuf { get; } = [];
    private List<byte> AnalyzeBuf { get; } = [];
    private Timer ProcessTimer { get; set; } = default!;
    private bool IsStopTimer { get; set; }
    private DateTime AnalyzeTimeout { get; set; }

    /// <summary>
    ///     Connection state
    /// </summary>
    [PublicAPI]
    public bool IsConnected => Client?.IsConnected ?? false;

    /// <summary>
    ///     Received message event
    /// </summary>
    [PublicAPI]
    public event PerformReceiveMsg? ReceivedMsg;

    /// <summary>
    ///     Received data event
    /// </summary>
    [PublicAPI]
    public event PerformReceiveData? ReceivedData;

    /// <summary>
    ///     Connection changed message event
    /// </summary>
    [PublicAPI]
    public event PerformConnectMsg? ConnectedMsg;

    /// <summary>
    ///     Connect to TCP/IP host
    /// </summary>
    /// <param name="ip">ip address</param>
    /// <param name="port">port</param>
    /// <param name="error">error</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool Connect(string ip, int port, out string error) {
        var res = false;
        // set error
        error = "Invalid ip address format";
        // check target
        if (!IPAddress.TryParse(ip, out var address))
            return false;

        // try catch
        try {
            // reset error
            error = string.Empty;
            // close client
            Disconnect();
            // create client
            Client = new SimpleTcpClient(address, port);
            // set event
            Client.Events.Connected    += ClientOnConnectionChanged;
            Client.Events.Disconnected += ClientOnConnectionChanged;
            Client.Events.DataReceived += ClientOnDataReceived;
            // set keep alive
            Client.Keepalive.EnableTcpKeepAlives    = true;
            Client.Keepalive.TcpKeepAliveInterval   = 5;
            Client.Keepalive.TcpKeepAliveTime       = 5;
            Client.Keepalive.TcpKeepAliveRetryCount = 5;
            // set the setting WARNING:해당 기능을 비활성화 하지 않으면 수신 데이터에 오류가 발생할 수 있음
            Client.Settings.UseAsyncDataReceivedEvents = false;
            // connect
            Client.Connect();

            // clear receive buffer
            ReceiveBuf.Clear();
            AnalyzeBuf.Clear();

            // create process timer
            ProcessTimer = new Timer();
            // set timer options
            ProcessTimer.AutoReset =  true;
            ProcessTimer.Interval  =  ProcessPeriod;
            ProcessTimer.Elapsed   += ProcessTimerOnElapsed;
            // start timer
            ProcessTimer.Start();
            // set result
            res = true;
        } catch (Exception e) {
            // set error
            error = e.Message;
            // dispose
            Client?.Dispose();
            // clear
            Client = null;
        }

        return res;
    }

    /// <summary>
    ///     Disconnect for TCP/IP client
    /// </summary>
    /// <returns>result</returns>
    [PublicAPI]
    public void Disconnect() {
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
    ///     Write packet value to TCP/IP host
    /// </summary>
    /// <param name="values">values</param>
    [PublicAPI]
    public bool Write(byte[] values) {
        // check session
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;

        // try catch
        try {
            // send
            Client.Send(values);
            // result
            return true;
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }

        return false;
    }

    private void ClientOnConnectionChanged(object? sender, ConnectionEventArgs e) {
        // check reason
        if (e.Reason != DisconnectReason.None)
            // close
            Disconnect();

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
            // reset stop
            IsStopTimer = false;
            // reset event
            ProcessTimer.Elapsed -= ProcessTimerOnElapsed;
            // stop timer
            ProcessTimer.Stop();
            // dispose timer
            ProcessTimer.Dispose();
            // clear buffer
            ReceiveBuf.Clear();
            // return
            return;
        }

        if (!Monitor.TryEnter(AnalyzeBuf, ProcessLockTime))
            // return
            return;
        // try finally
        try {
#if NOT_USE
            // check empty for receive buffer
            while (!ReceiveBuf.IsEmpty) {
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
                // get data
                if (ReceiveBuf.TryDequeue(out var d))
                    // add data
                    AnalyzeBuf.Add(d);
            }
#else
            // get the data
            while (ReceiveBuf.TryDequeue(out var d)) {
                // add the data
                AnalyzeBuf.Add(d);
                // set analyze time
                AnalyzeTimeout = DateTime.Now;
            }
#endif

            // check analyze count
            if (AnalyzeBuf.Count > 0)
                // check analyze timeout
                if ((DateTime.Now - AnalyzeTimeout).TotalMilliseconds > ProcessTimeout)
                    // clear buffer
                    AnalyzeBuf.Clear();

            // check data header length    [LEN[2) MID(2) REV(2) RESERVE(10)]
            if (AnalyzeBuf.Count < FormatMessageInfo.Size)
                return;
#if NOT_USE
            // get packet values
            var packet = AnalyzeBuf.ToArray();
#else
            // create the header span
            Span<byte> headerSpan = stackalloc byte[FormatMessageInfo.Size];
            // check the header size
            for (var i = 0; i < FormatMessageInfo.Size; i++)
                // set the data
                headerSpan[i] = AnalyzeBuf[i];
#endif
            // get frame header
            var header = new FormatMessageInfo(headerSpan.ToArray());
            // check frame length
            if (header.Length > AnalyzeBuf.Count)
                return;
            // check message id
            if (header.Id == MessageIdTypes.None)
                return;

#if NOT_USE
            // get message data
            var msg = new FormatMessage(AnalyzeBuf.Take(header.Length).ToArray());
#else
            // create the message buffer
            var buf = new byte[header.Length];
            // copy to the buffer
            AnalyzeBuf.CopyTo(0, buf, 0, header.Length);

            // get message data
            var msg = new FormatMessage(buf);
#endif
            // check frame length
            if (header.Length <= AnalyzeBuf.Count)
                // remove analyze buffer
                AnalyzeBuf.RemoveRange(0, header.Length);
            else
                // clear analyze buffer
                AnalyzeBuf.Clear();
            // update event
            ReceivedMsg?.Invoke(msg);
        } finally {
            // exit monitor
            Monitor.Exit(AnalyzeBuf);
        }
    }
}