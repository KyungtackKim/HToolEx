using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace HCommEz.Device {
    /// <summary>
    ///     Device serial class
    /// </summary>
    public class DevSerial : IDevice {
        private const int TimerInterval = 100;

        /// <summary>
        ///     Constructor
        /// </summary>
        public DevSerial() {
            // serial port
            Port = new SerialPort();
            // set received event
            Port.DataReceived += OnDataReceived;
            // connect timer
            ConnectTimer = new Timer(ConnectTimerCallback);
            // work timer
            WorkTimer = new Timer(WorkTimerCallback);
        }

        private SerialPort Port { get; }
        private Timer ConnectTimer { get; }
        private Timer WorkTimer { get; }
        private ConcurrentQueue<byte> ReceiveBuf { get; } = new ConcurrentQueue<byte>();
        private List<byte> AnalyzeBuf { get; } = new List<byte>();
        private WorkMode Mode { get; set; }

        /// <summary>
        ///     OnReceived calibration event
        /// </summary>
        public ReceivedCal OnReceivedCal { get; set; }

        /// <summary>
        ///     OnReceived torque event
        /// </summary>
        public ReceivedTorque OnReceivedTorque { get; set; }

        /// <summary>
        ///     Connection mode
        /// </summary>
        public ConnectMode ConnectionMode => ConnectMode.Serial;

        /// <summary>
        ///     Connecting state
        /// </summary>
        public ConnectionState State { get; set; } = ConnectionState.Disconnected;

        /// <summary>
        ///     Connection state
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///     EZTorQ information
        /// </summary>
        public Information Info { get; } = new Information();

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="target">target</param>
        /// <param name="option">option</param>
        /// <returns>result</returns>
        public bool Connect(string target, int option = 115200) {
            // check target / option
            if (string.IsNullOrEmpty(target))
                return false;
            // try catch
            try {
                // set port
                Port.PortName = target;
                Port.BaudRate = option;
                Port.Encoding = Encoding.GetEncoding(28591);
                // clear queue
                while (!ReceiveBuf.IsEmpty)
                    // remove data
                    ReceiveBuf.TryDequeue(out _);
                // clear buffer
                AnalyzeBuf.Clear();
                // open
                Port.Open();
                // start timer
                ConnectTimer.Change(TimerInterval, TimerInterval);
                WorkTimer.Change(0, TimerInterval);

                return true;
            } catch (Exception ex) {
                // debug
                Debug.WriteLine(ex.Message);
            }

            return IsConnected = false;
        }

        /// <summary>
        ///     Disconnect
        /// </summary>
        /// <returns>result</returns>
        public bool Disconnect() {
            // try catch
            try {
                // stop timer
                WorkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                // close
                Port.Close();
                // reset connection state
                IsConnected = false;
                // change state
                State = ConnectionState.Disconnected;

                return true;
            } catch (Exception ex) {
                // debug
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        /// <summary>
        ///     Request calibration data
        /// </summary>
        /// <returns>result</returns>
        public bool RequestCal() {
            // check connect
            if (!Port.IsOpen)
                return false;
            // packet
            var index  = 0;
            var packet = new byte[5];
            // header
            packet[index++] = 0x5A;
            packet[index++] = 0xA5;
            // length
            packet[index++] = 0x01;
            packet[index++] = 0x00;
            // command
            packet[index++] = (byte)WorkCommand.ReqCalData;

            try {
                // transmit
                Port.Write(packet, 0, index);
                // change mode
                Mode = WorkMode.Calibration;
                // result
                return true;
            } catch (Exception ex) {
                // debug
                Console.WriteLine($@"{ex.Message}");
            }

            return false;
        }

        /// <summary>
        ///     Request calibration terminate
        /// </summary>
        /// <returns>result</returns>
        public bool RequestCalTerminate() {
            // check connect
            if (!Port.IsOpen)
                return false;
            // packet
            var index  = 0;
            var packet = new byte[5];
            // header
            packet[index++] = 0x5A;
            packet[index++] = 0xA5;
            // length
            packet[index++] = 0x01;
            packet[index++] = 0x00;
            // command
            packet[index++] = (byte)WorkCommand.ReqCalTerminate;

            try {
                // transmit
                Port.Write(packet, 0, index);
                // change mode
                Mode = WorkMode.Torque;
                // result
                return true;
            } catch (Exception ex) {
                // debug
                Console.WriteLine($@"{ex.Message}");
            }

            return false;
        }

        /// <summary>
        ///     Request setting
        /// </summary>
        /// <returns>result</returns>
        public bool RequestSet() {
            // check connect
            if (!Port.IsOpen)
                return false;
            // packet
            var index  = 0;
            var packet = new byte[5];
            // header
            packet[index++] = 0x5A;
            packet[index++] = 0xA5;
            // length
            packet[index++] = 0x01;
            packet[index++] = 0x00;
            // command
            packet[index++] = (byte)WorkCommand.ReqSetting;

            try {
                // transmit
                Port.Write(packet, 0, index);
                // change mode
                Mode = WorkMode.Torque;
                // result
                return true;
            } catch (Exception ex) {
                // debug
                Console.WriteLine($@"{ex.Message}");
            }

            return false;
        }

        /// <summary>
        ///     Save calibration data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>result</returns>
        public bool SaveCalPoint(Information.CalInfo data) {
            // check connect
            if (!Port.IsOpen)
                return false;
            // packet
            var index  = 0;
            var packet = new byte[37];
            // header
            packet[index++] = 0x5A;
            packet[index++] = 0xA5;
            // length
            packet[index++] = 0x21;
            packet[index++] = 0x00;
            // command
            packet[index++] = (byte)WorkCommand.ReqCalSave;
            // body type
            packet[index++] = Convert.ToByte(data.Type);
            // model
            packet[index++] = Convert.ToByte(data.Model         & 0xFF);
            packet[index++] = Convert.ToByte((data.Model >> 8)  & 0xFF);
            packet[index++] = Convert.ToByte((data.Model >> 16) & 0xFF);
            packet[index++] = Convert.ToByte((data.Model >> 24) & 0xFF);
            // max torque
            packet[index++] = Convert.ToByte(data.TorqueUInt32         & 0xFF);
            packet[index++] = Convert.ToByte((data.TorqueUInt32 >> 8)  & 0xFF);
            packet[index++] = Convert.ToByte((data.TorqueUInt32 >> 16) & 0xFF);
            packet[index++] = Convert.ToByte((data.TorqueUInt32 >> 24) & 0xFF);
            // body serial
            packet[index++] = Convert.ToByte(data.BodySerialUint32         & 0xFF);
            packet[index++] = Convert.ToByte((data.BodySerialUint32 >> 8)  & 0xFF);
            packet[index++] = Convert.ToByte((data.BodySerialUint32 >> 16) & 0xFF);
            packet[index++] = Convert.ToByte((data.BodySerialUint32 >> 24) & 0xFF);
            // sensor serial
            packet[index++] = Convert.ToByte(data.SensorSerialUInt32         & 0xFF);
            packet[index++] = Convert.ToByte((data.SensorSerialUInt32 >> 8)  & 0xFF);
            packet[index++] = Convert.ToByte((data.SensorSerialUInt32 >> 16) & 0xFF);
            packet[index++] = Convert.ToByte((data.SensorSerialUInt32 >> 24) & 0xFF);
            // unit
            packet[index++] = Convert.ToByte(data.Unit);
            // point
            packet[index++] = Convert.ToByte(data.Point);
            // offset
            packet[index++] = Convert.ToByte(data.Offset        & 0xFF);
            packet[index++] = Convert.ToByte((data.Offset >> 8) & 0xFF);
            // check type
            if (data.Type == BodyTypes.Separation) {
                packet[index++] = Convert.ToByte((data.Offset >> 16) & 0xFF);
                packet[index++] = Convert.ToByte((data.Offset >> 24) & 0xFF);
            }

            // positive data
            for (var i = 0; i < 5; i++) {
                packet[index++] = Convert.ToByte(data.Positive[i]        & 0xFF);
                packet[index++] = Convert.ToByte((data.Positive[i] >> 8) & 0xFF);
                // check type
                if (data.Type != BodyTypes.Separation)
                    continue;
                packet[index++] = Convert.ToByte((data.Positive[i] >> 16) & 0xFF);
                packet[index++] = Convert.ToByte((data.Positive[i] >> 24) & 0xFF);
            }

            // negative data
            for (var i = 0; i < 5; i++) {
                packet[index++] = Convert.ToByte(data.Negative[i]        & 0xFF);
                packet[index++] = Convert.ToByte((data.Negative[i] >> 8) & 0xFF);
                // check type
                if (data.Type != BodyTypes.Separation)
                    continue;
                packet[index++] = Convert.ToByte((data.Negative[i] >> 16) & 0xFF);
                packet[index++] = Convert.ToByte((data.Negative[i] >> 24) & 0xFF);
            }

            try {
                // transmit
                Port.Write(packet, 0, index);
                // result
                return true;
            } catch (Exception ex) {
                // debug
                Console.WriteLine($@"{ex.Message}");
            }

            return false;
        }

        /// <summary>
        ///     Set calibration position
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="pos">position</param>
        /// <returns>result</returns>
        public bool SetCalPoint(int type, WorkPosition pos) {
            // check connect
            if (!Port.IsOpen)
                return false;
            // packet
            var index  = 0;
            var packet = new byte[7];
            // header
            packet[index++] = 0x5A;
            packet[index++] = 0xA5;
            // length
            packet[index++] = 0x03;
            packet[index++] = 0x00;
            // command
            packet[index++] = (byte)WorkCommand.ReqCalSetPoint;
            // type
            packet[index++] = (byte)type;
            // point
            packet[index++] = (byte)pos;

            try {
                // transmit
                Port.Write(packet, 0, index);
                // result
                return true;
            } catch (Exception ex) {
                // debug
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e) {
            // get data
            var data = Port.Encoding.GetBytes(Port.ReadExisting());
            // check data
            foreach (var b in data)
                // add queue
                ReceiveBuf.Enqueue(b);
        }

        private void WorkTimerCallback(object state) {
            // check receive buffer count
            while (!ReceiveBuf.IsEmpty) {
                // try dequeue
                if (!ReceiveBuf.TryDequeue(out var b))
                    break;
                // add analyze buffer
                AnalyzeBuf.Add(b);
            }

            // timeout
            var timeout = DateTime.Now;
            // check analyze buffer count
            while (AnalyzeBuf.Count > 0) {
                // get laps
                var laps = DateTime.Now - timeout;
                // check timeout
                if (laps.TotalMilliseconds > TimerInterval)
                    // clear analyze buffer
                    AnalyzeBuf.Clear();

                // check length
                if (AnalyzeBuf.Count < 3)
                    return;

                // check mode
                switch (Mode) {
                    case WorkMode.Torque:
                        // check header
                        if (AnalyzeBuf[0] == 0x5A && AnalyzeBuf[1] == 0xA5) {
                            // check length
                            if (AnalyzeBuf.Count < 5)
                                return;
                            // get length
                            var cLength = (AnalyzeBuf[3] << 8) | AnalyzeBuf[2];
                            // get total frame length
                            var cFrame = cLength + 4;
                            // check length
                            if (AnalyzeBuf.Count < cFrame)
                                return;
                            if (cLength == Information.SetInfo.Size + 1)
                                // set setting values
                                Info.Set.SetValues(AnalyzeBuf.Skip(5).Take(cLength - 1).ToArray());
                            // get command
                            var cCmd = (WorkCommand)AnalyzeBuf[4];
                            // event
                            OnReceivedCal?.Invoke(cCmd, AnalyzeBuf.Skip(5).Take(cLength - 1).ToArray());
                            // remove buffer
                            AnalyzeBuf.RemoveRange(0, cFrame);
                        } else if (AnalyzeBuf[AnalyzeBuf.Count - 2] == 0x0D && AnalyzeBuf[AnalyzeBuf.Count - 1] == 0x0A) {
                            // get values
                            var values = Port.Encoding.GetString(AnalyzeBuf.ToArray()).Split(',');
                            // check values
                            if (values.Length > 1)
                                // event
                                OnReceivedTorque?.Invoke(Convert.ToDouble(values[0]),
                                    values[1].Substring(0, values[1].Length - 2));
                            // remove buffer
                            AnalyzeBuf.Clear();
                        }

                        break;
                    case WorkMode.Calibration:
                        // check header
                        if (AnalyzeBuf[0] != 0x5A || AnalyzeBuf[1] != 0xA5) {
                            // debug
                            Debug.WriteLine(@"failed header packet");
                            // clear buffer
                            AnalyzeBuf.Clear();
                            // exit
                            return;
                        }

                        // check length
                        if (AnalyzeBuf.Count < 5)
                            return;
                        // get length
                        var tLength = (AnalyzeBuf[3] << 8) | AnalyzeBuf[2];
                        // get total frame length
                        var tFrame = tLength + 4;
                        // check length
                        if (AnalyzeBuf.Count < tFrame)
                            return;
                        // check size
                        if (tLength == Information.CalInfo.ShortSize + 1 ||
                            tLength == Information.CalInfo.LargeSize + 1)
                            // set calibration values
                            Info.Cal.SetValues(AnalyzeBuf.Skip(5).Take(tLength - 1).ToArray());

                        // get command
                        var tCmd = (WorkCommand)AnalyzeBuf[4];
                        // event
                        OnReceivedCal?.Invoke(tCmd, AnalyzeBuf.Skip(5).Take(tLength - 1).ToArray());
                        // remove buffer
                        AnalyzeBuf.RemoveRange(0, tFrame);

                        break;
                }
            }
        }

        private void ConnectTimerCallback(object state) {
            // check state
            switch (State) {
                case ConnectionState.Disconnected:
                    // reset values
                    Info.Cal.ResetValues();
                    // change state
                    State = ConnectionState.ConnectCalibration;
                    break;
                case ConnectionState.ConnectCalibration:
                    // request calibration
                    RequestCal();
                    // check info
                    if (Info.Cal.Torque != 0) {
                        // terminate
                        RequestCalTerminate();
                        // change state
                        State = ConnectionState.ConnectSetting;
                    }

                    break;
                case ConnectionState.ConnectSetting:
                    // request setting
                    RequestSet();
                    // check info
                    if (Info.Set.Version != @"0.0.0")
                        // change state
                        State = ConnectionState.Connected;
                    break;
                case ConnectionState.Connected:
                    // change connection state
                    IsConnected = true;
                    // terminate
                    RequestCalTerminate();
                    // disable timer
                    ConnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    break;
            }
        }
    }
}