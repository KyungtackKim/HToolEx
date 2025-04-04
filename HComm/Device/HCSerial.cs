﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using HComm.Common;

namespace HComm.Device
{
    public class HcSerial : IHComm
    {
        private const int ProcessTime = 10;
        private static readonly int[] BaudRates = { 9600, 19200, 38400, 57600, 115200, 230400 };
        private SerialPort Port { get; } = new SerialPort();

        private Timer ProcessTimer { get; set; }

        private ConcurrentQueue<byte> ReceiveBuf { get; } = new ConcurrentQueue<byte>();
        private List<byte> AnalyzeBuf { get; } = new List<byte>();
        private byte Id { get; set; }

        /// <summary>
        ///     HCommInterface serial connection state
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     HCommInterface serial acknowledge event
        /// </summary>
        public AckData AckReceived { get; set; }

        /// <summary>
        ///     HCommInterface serial raw acknowledge event
        /// </summary>
        public AckRawData AckRawReceived { get; set; }

        /// <summary>
        ///     HCommInterface serial monitor acknowledge event
        /// </summary>
        public AckMorData AckMorReceived { get; set; }

        /// <summary>
        ///     HCommInterface connection state changed
        /// </summary>
        public ChangedConnection ConnectionChanged { get; set; }

        /// <summary>
        ///     HCommInterface serial connect
        /// </summary>
        /// <param name="target">target port</param>
        /// <param name="option">baud-rate</param>
        /// <param name="id">id</param>
        /// <returns>result</returns>
        public bool Connect(string target, int option, byte id = 1)
        {
            // check target
            if (string.IsNullOrWhiteSpace(target))
                return false;
            // check option
            if (!BaudRates.Contains(option))
                return false;
            // check id
            if (id > 0x0F)
                return false;

            try
            {
                // set com port
                Port.PortName = target;
                Port.BaudRate = option;
                Port.Encoding = Encoding.GetEncoding(28591);
                // open
                Port.Open();
                // set id
                // set id
                Id = id;
                // check empty
                while (!ReceiveBuf.IsEmpty)
                    // remove
                    ReceiveBuf.TryDequeue(out _);
                // clear buffer
                AnalyzeBuf.Clear();
                // set event
                Port.DataReceived += Port_DataReceived;
                // check timer
                if (ProcessTimer == null)
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
        ///     HCommInterface serial close
        /// </summary>
        /// <returns>result</returns>
        public bool Close()
        {
            // stop timer
            ProcessTimer.Change(Timeout.Infinite, Timeout.Infinite);
            // close port
            Port.Close();
            // reset event
            Port.DataReceived -= Port_DataReceived;
            // clear state
            if (IsConnected)
                // update event
                ConnectionChanged?.Invoke(IsConnected = false);
            // result
            return true;
        }

        /// <summary>
        ///     HCommInterface serial write
        /// </summary>
        /// <param name="packet">packet</param>
        /// <param name="length">packet length</param>
        /// <returns>result</returns>
        public bool Write(byte[] packet, int length)
        {
            // check connection
            if (!Port.IsOpen)
                return false;

            try
            {
                // write packet
                Port.Write(packet, 0, length);
                // result
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     HCommInterface serial get parameter packet make
        /// </summary>
        /// <param name="addr">address</param>
        /// <param name="count">count</param>
        /// <returns>packet</returns>
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
            // crc
            packet.AddRange(GetCrc(packet.ToArray()));
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HCommInterface serial set parameter packet make
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
            // crc
            packet.AddRange(GetCrc(packet.ToArray()));
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HCommInterface serial get state packet make
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
            // crc
            packet.AddRange(GetCrc(packet.ToArray()));
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HCommInterface serial get info packet make
        /// </summary>
        /// <returns>packet</returns>
        public IEnumerable<byte> PacketGetInfo()
        {
            var packet = new List<byte>
            {
                Id, (byte)Command.Info
            };
            // crc
            packet.AddRange(GetCrc(packet.ToArray()));
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HCommInterface serial get graph packet make
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
            // crc
            packet.AddRange(GetCrc(packet.ToArray()));
            // packet
            return packet.ToArray();
        }

        /// <summary>
        ///     HComm serial get port names
        /// </summary>
        /// <returns>port list</returns>
        public static IEnumerable<string> GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        ///     HComm serial get port baud-rates
        /// </summary>
        /// <returns>baud-rate list</returns>
        public static IEnumerable<int> GetBaudRates()
        {
            return BaudRates;
        }

        private static IEnumerable<byte> GetCrc(IEnumerable<byte> packet)
        {
            var crc = new byte[] { 0xFF, 0xFF };
            ushort crcFull = 0xFFFF;
            // check total packet
            foreach (var data in packet)
            {
                // XOR 1 byte
                crcFull = (ushort)(crcFull ^ data);
                // cyclic redundancy check
                for (var j = 0; j < 8; j++)
                {
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

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // get data
            var data = Port.Encoding.GetBytes(Port.ReadExisting());
            // check data length
            foreach (var t in data)
                // add receive data
                ReceiveBuf.Enqueue(t);
            // update raw event
            AckRawReceived?.Invoke(data);
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

                // check header length
                if (AnalyzeBuf.Count < 3)
                    return;

                // set command
                var cmd = (Command)AnalyzeBuf[1];
                var error = (byte)cmd & 0x80;
                // check error
                if (error == 0x80 && cmd != Command.GraphAd)
                    // check header length
                    if (AnalyzeBuf.Count >= 5)
                        // set error
                        cmd = (Command)error;
                int length;
                // check header length
                if (AnalyzeBuf.Count < 4)
                    return;
                // check command
                switch (cmd)
                {
                    case Command.Write:
                        // set length
                        length = 8;
                        break;
                    case Command.Graph:
                    case Command.GraphRes:
                        // set length
                        length = (AnalyzeBuf[2] << 8) | (AnalyzeBuf[3] + 6);
                        break;
                    case Command.GraphAd:
                    case Command.Read:
                    case Command.Mor:
                    case Command.Info:
                        // set length
                        length = AnalyzeBuf[2] + 5;
                        break;
                    case Command.Error:
                        // set length
                        length = 5;
                        break;
                    case Command.None:
                    default:
                        // clear analyze buffer
                        AnalyzeBuf.Clear();
                        // exit
                        return;
                }

                // check body length
                if (length > AnalyzeBuf.Count)
                    return;
                // get packet
                var packet = AnalyzeBuf.Take(length).ToArray();
                // get crc
                var crc = GetCrc(packet.Take(length - 2).ToArray()).ToArray();
                // check crc
                if (packet.Length < 2 || crc[0] != packet[packet.Length - 2] || crc[1] != packet[packet.Length - 1])
                {
                    // error
                    AckReceived?.Invoke(Command.Error, new byte[] { 0xFF });
                }
                else
                {
                    // check state
                    if (!IsConnected)
                        // update event
                        ConnectionChanged?.Invoke(IsConnected = true);
                    // process acknowledge
                    AckReceived?.Invoke(cmd, packet.Skip(2).Take(length - 4).ToArray());
                }

                // check analyze buf length
                if (AnalyzeBuf.Count >= length)
                    // remove analyze buffer
                    AnalyzeBuf.RemoveRange(0, length);
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