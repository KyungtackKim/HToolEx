using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HComm.Common;
using HidLibrary;

namespace HComm.Device
{
    public class HcUsb : IHComm
    {
        private const int ProcessTime = 10;
        private const int UsbTimeout = 100;
        private const int Vid = 0x0483;
        private const int Pid = 0x5710;
        private HidDevice UsbDevice { get; set; }
        private Timer ProcessTimer { get; set; }
        private ConcurrentQueue<byte> ReceiveBuf { get; } = new ConcurrentQueue<byte>();
        private List<byte> AnalyzeBuf { get; } = new List<byte>();
        private byte Id { get; set; }

        /// <summary>
        ///     HComm usb connection state
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     HComm usb received data event
        /// </summary>
        public AckData AckReceived { get; set; }

        /// <summary>
        ///     HCommInterface usb raw acknowledge event
        /// </summary>
        public AckRawData AckRawReceived { get; set; }

        /// <summary>
        ///     HCommInterface usb monitor acknowledge event
        /// </summary>
        public AckMorData AckMorReceived { get; set; }

        /// <summary>
        ///     HCommInterface connection state changed
        /// </summary>
        public ChangedConnection ConnectionChanged { get; set; }

        /// <summary>
        ///     HComm usb connect
        /// </summary>
        /// <param name="target">not use: target</param>
        /// <param name="option">not use: option</param>
        /// <param name="id">id</param>
        /// <returns>result</returns>
        public bool Connect(string target, int option, byte id = 2)
        {
            try
            {
                // check id
                if (id > 0x0F)
                    return false;

                // get devices
                var devices = HidDevices.Enumerate(Vid, Pid).ToArray();
                // check devices
                for (var i = 0; i < devices.Length; i++)
                {
                    // get product data
                    if (!devices[i].ReadProduct(out var data))
                        continue;
                    // get name
                    var name = $@"{Encoding.Unicode.GetString(data).Replace("\0", "")}-{i + 1}";
                    // check name
                    if (target != name)
                        continue;
                    // set device
                    UsbDevice = devices[i];
                    // break
                    break;
                }

                // check device
                if (UsbDevice == null)
                    return false;
                // set report
                UsbDevice.ReadReport(UsbDevice_DataReceived, UsbTimeout);
                // set id
                Id = id;
                // clear buffer
                AnalyzeBuf.Clear();
                // check empty
                while (!ReceiveBuf.IsEmpty)
                    // remove
                    ReceiveBuf.TryDequeue(out _);
                // check timer
                if (ProcessTimer == null)
                    // new timer
                    ProcessTimer = new Timer(ProcessTimerCallback);
                // start timer
                ProcessTimer.Change(ProcessTime, ProcessTime);

                // result
                return true;
            }
            catch (Exception e)
            {
                // error
                Console.WriteLine($@"{this}_Connect: {e.Message}");
            }

            return false;
        }

        /// <summary>
        ///     HComm usb close
        /// </summary>
        /// <returns>result</returns>
        public bool Close()
        {
            // stop timer
            ProcessTimer.Change(Timeout.Infinite, Timeout.Infinite);
            // check empty
            while (!ReceiveBuf.IsEmpty)
                // remove
                ReceiveBuf.TryDequeue(out _);
            // clear buffer
            AnalyzeBuf.Clear();

            // check device
            if (UsbDevice != null)
            {
                // check state
                if (UsbDevice.IsOpen)
                    // disconnect
                    UsbDevice.CloseDevice();
                // dispose
                UsbDevice.Dispose();
                // clear
                UsbDevice = null;
            }

            // clear state
            if (IsConnected)
                // update event
                ConnectionChanged?.Invoke(IsConnected = false);

            // result
            return true;
        }

        /// <summary>
        ///     HComm usb packet write
        /// </summary>
        /// <param name="packet">packet</param>
        /// <param name="length">length</param>
        /// <returns>result</returns>
        public bool Write(byte[] packet, int length)
        {
            // check connection
            if (UsbDevice == null || !UsbDevice.IsConnected || !UsbDevice.IsOpen)
                return false;

            try
            {
                // write packet
                var res = UsbDevice.Write(packet, UsbTimeout);

                // result
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{e.Message}");
            }

            return false;
        }

        /// <summary>
        ///     HComm usb get parameter packet make
        /// </summary>
        /// <param name="addr">address</param>
        /// <param name="count">count</param>
        /// <returns>result</returns>
        public IEnumerable<byte> PacketGetParam(ushort addr, ushort count)
        {
            var packet = new List<byte>
            {
                Id, (byte)Command.Read,
                (byte)((addr >> 8) & 0xFF),
                (byte)(addr & 0xFF),
                (byte)((count >> 8) & 0xFF),
                (byte)(count & 0xFF)
            };
            // add dummy
            packet.AddRange(new byte[64 - packet.Count]);
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HComm usb set parameter packet make
        /// </summary>
        /// <param name="addr">address</param>
        /// <param name="value">value</param>
        /// <returns>packet</returns>
        public IEnumerable<byte> PacketSetParam(ushort addr, ushort value)
        {
            var packet = new List<byte>
            {
                Id, (byte)Command.Write,
                (byte)((addr >> 8) & 0xFF),
                (byte)(addr & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)(value & 0xFF)
            };
            // add dummy
            packet.AddRange(new byte[64 - packet.Count]);
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HComm usb get state packet make
        /// </summary>
        /// <param name="addr">address</param>
        /// <param name="count">count</param>
        /// <returns>packet</returns>
        public IEnumerable<byte> PacketGetState(ushort addr, ushort count)
        {
            var packet = new List<byte>
            {
                Id, (byte)Command.Mor,
                (byte)((addr >> 8) & 0xFF),
                (byte)(addr & 0xFF),
                (byte)((count >> 8) & 0xFF),
                (byte)(count & 0xFF)
            };
            // add dummy
            packet.AddRange(new byte[64 - packet.Count]);
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HComm usb get info packet make
        /// </summary>
        /// <returns>packet</returns>
        public IEnumerable<byte> PacketGetInfo()
        {
            var packet = new List<byte>
            {
                Id, (byte)Command.Info
            };
            // add dummy
            packet.AddRange(new byte[64 - packet.Count]);
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HComm usb get graph packet make
        /// </summary>
        /// <param name="addr">not use: address</param>
        /// <param name="count">not use: count</param>
        /// <returns>packet</returns>
        public IEnumerable<byte> PacketGetGraph(ushort addr, ushort count)
        {
            var packet = new List<byte>
            {
                Id, (byte)Command.GraphAd, 0x00
            };
            // add dummy
            packet.AddRange(new byte[64 - packet.Count]);
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HComm usb get device list
        /// </summary>
        /// <returns>list</returns>
        public static List<string> GetDeviceNames()
        {
            // name buf
            var names = new List<string>();
            // get devices
            var devices = HidDevices.Enumerate(Vid, Pid).ToArray();
            // check devices count
            foreach (var device in devices)
            {
                // read product data
                if (!device.ReadProduct(out var data))
                    continue;
                // get name
                var name = $"{Encoding.Unicode.GetString(data).Replace("\0", "")}-{device.DevicePath.Split('#')[2]}";

                // add name
                names.Add(name);
            }

            // convert name list
            return names;
        }

        private void UsbDevice_DataReceived(HidReport report)
        {
            // check connection state
            if (UsbDevice == null || !UsbDevice.IsConnected)
                return;
            // check data length
            foreach (var t in report.Data)
                // add receive data
                ReceiveBuf.Enqueue(t);
            // update raw event
            AckRawReceived?.Invoke(report.Data);
            // set report
            UsbDevice?.ReadReport(UsbDevice_DataReceived, UsbTimeout);
        }

        private void ProcessTimerCallback(object state)
        {
            // check state
            if (state == null)
                return;
            // try catch finally
            try
            {
                // pause timer
                ProcessTimer.Change(Timeout.Infinite, Timeout.Infinite);
                // enter monitor
                if (!Monitor.TryEnter(AnalyzeBuf, 100))
                    return;
                // check buffer empty
                while (!ReceiveBuf.IsEmpty)
                    // get data
                    if (ReceiveBuf.TryDequeue(out var d))
                        // add data
                        AnalyzeBuf.Add(d);

                // check frame length
                if (AnalyzeBuf.Count < UsbDevice.Capabilities.OutputReportByteLength - 1)
                    return;
                // set command
                var cmd = (Command)AnalyzeBuf[0];
                var error = (byte)cmd & 0x80;
                // check error
                if (error == 0x80 && cmd != Command.GraphAd)
                {
                    // error
                    AckReceived?.Invoke(Command.Error, AnalyzeBuf.Skip(2).Take(1).ToArray());
                    // remove analyze buffer
                    AnalyzeBuf.Clear();
                    // break
                    return;
                }

                var length = 0;
                // check command
                switch (cmd)
                {
                    case Command.Write:
                        // set length
                        length = 4;
                        break;
                    case Command.Graph:
                    case Command.GraphRes:
                    case Command.GraphAd:
                    case Command.Read:
                    case Command.Mor:
                    case Command.Info:
                        // set length
                        length = AnalyzeBuf[1] + 1;
                        break;
                    case Command.Error:
                        break;
                    case Command.None:
                    default:
                        // clear analyze buffer
                        AnalyzeBuf.Clear();
                        return;
                }

                // check body length
                if (length > AnalyzeBuf.Count)
                    return;
                // check state
                if (!IsConnected)
                    // update event
                    ConnectionChanged?.Invoke(IsConnected = true);
                // get packet
                var packet = AnalyzeBuf.Take(UsbDevice.Capabilities.OutputReportByteLength - 1).ToArray();
                // process acknowledge
                AckReceived?.Invoke(cmd, packet.Skip(1).Take(length).ToArray());
                // check analyze buf length
                if (AnalyzeBuf.Count >= UsbDevice.Capabilities.OutputReportByteLength - 1)
                    // remove analyze buffer
                    AnalyzeBuf.RemoveRange(0, UsbDevice.Capabilities.OutputReportByteLength - 1);
            }
            catch (Exception)
            {
                // clear buffer
                AnalyzeBuf.Clear();
            }
            finally
            {
                // exit monitor
                Monitor.Exit(AnalyzeBuf);
                // resume timer
                ProcessTimer.Change(ProcessTime, ProcessTime);
            }
        }
    }
}