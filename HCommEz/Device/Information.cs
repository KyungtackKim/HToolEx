using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HCommEz.Device {
    /// <summary>
    ///     EZtorQ information class
    /// </summary>
    public class Information {
        /// <summary>
        ///     Calibration
        /// </summary>
        public CalInfo Cal { get; } = new CalInfo();

        /// <summary>
        ///     Setting
        /// </summary>
        public SetInfo Set { get; } = new SetInfo();

        /// <summary>
        ///     EZTorQ calibration class
        /// </summary>
        public class CalInfo {
            /// <summary>
            ///     Constructor
            /// </summary>
            public CalInfo() {
                // add dummy
                Values.AddRange(new byte[LargeSize]);
            }

            /// <summary>
            ///     Calibration data size : Separation type
            /// </summary>
            public static int LargeSize => 63;

            /// <summary>
            ///     Calibration data size : Integration type
            /// </summary>
            public static int ShortSize => 41;

            private List<byte> Values { get; } = new List<byte>();

            /// <summary>
            ///     Body type
            /// </summary>
            public BodyTypes Type {
                get => (BodyTypes)Values[0];
                set => Values[0] = (byte)value;
            }

            /// <summary>
            ///     Model number : EZTorQ-III OOOi
            /// </summary>
            public uint Model {
                get => BitConverter.ToUInt32(Values.Skip(1).Take(4).ToArray(), 0);
                set {
                    // get bytes values
                    var val = BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[1 + i] = val[i];
                }
            }

            /// <summary>
            ///     Max torque
            /// </summary>
            public float Torque {
                get => BitConverter.ToUInt32(Values.Skip(5).Take(4).ToArray(), 0) / 100.0f;
                set {
                    // get bytes values
                    var val = BitConverter.GetBytes(Convert.ToUInt32(value * 100));
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[5 + i] = val[i];
                }
            }

            /// <summary>
            ///     Max torque to UInt32
            /// </summary>
            public uint TorqueUInt32 => BitConverter.ToUInt32(Values.Skip(5).Take(4).ToArray(), 0);

            /// <summary>
            ///     Body serial number
            /// </summary>
            public string BodySerial {
                get => $@"{BitConverter.ToUInt32(Values.Skip(9).Take(4).ToArray(), 0)}";
                set {
                    // get bytes values
                    var val = BitConverter.GetBytes(Convert.ToUInt32(value));
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[9 + i] = val[i];
                }
            }

            /// <summary>
            ///     Body serial number To UInt32
            /// </summary>
            public uint BodySerialUint32 => BitConverter.ToUInt32(Values.Skip(9).Take(4).ToArray(), 0);

            /// <summary>
            ///     Sensor serial number
            /// </summary>
            public string SensorSerial {
                get => $@"{BitConverter.ToUInt32(Values.Skip(13).Take(4).ToArray(), 0)}";
                set {
                    // get bytes values
                    var val = BitConverter.GetBytes(Convert.ToUInt32(value));
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[13 + i] = val[i];
                }
            }

            /// <summary>
            ///     Sensor serial number To UInt32
            /// </summary>
            public uint SensorSerialUInt32 => BitConverter.ToUInt32(Values.Skip(13).Take(4).ToArray(), 0);

            /// <summary>
            ///     Unit
            /// </summary>
            public UnitTypes Unit {
                get => (UnitTypes)Values[17];
                set => Values[17] = (byte)value;
            }

            /// <summary>
            ///     Point
            /// </summary>
            public PointTypes Point {
                get => (PointTypes)Values[18];
                set => Values[18] = (byte)value;
            }

            /// <summary>
            ///     Positive
            /// </summary>
            public List<int> Positive {
                get =>
                    new List<int> {
                        Positive1,
                        Positive2,
                        Positive3,
                        Positive4,
                        Positive5
                    };
                set {
                    // check value
                    if (value == null)
                        return;
                    // check length
                    if (value.Count != 3 && value.Count != 5)
                        return;
                    // check count
                    switch (value.Count) {
                        case 3:
                            Positive1 = value[0];
                            Positive2 = value[1];
                            Positive3 = value[2];
                            Positive4 = 0;
                            Positive5 = 0;
                            break;
                        case 5:
                            Positive1 = value[0];
                            Positive2 = value[1];
                            Positive3 = value[2];
                            Positive4 = value[3];
                            Positive5 = value[4];
                            break;
                    }
                }
            }

            /// <summary>
            ///     Negative
            /// </summary>
            public List<int> Negative {
                get =>
                    new List<int> {
                        Negative1,
                        Negative2,
                        Negative3,
                        Negative4,
                        Negative5
                    };
                set {
                    // check value
                    if (value == null)
                        return;
                    // check length
                    if (value.Count != 3 && value.Count != 5)
                        return;
                    // check count
                    switch (value.Count) {
                        case 3:
                            Negative1 = value[0];
                            Negative2 = value[1];
                            Negative3 = value[2];
                            Negative4 = 0;
                            Negative5 = 0;
                            break;
                        case 5:
                            Negative1 = value[0];
                            Negative2 = value[1];
                            Negative3 = value[2];
                            Negative4 = value[3];
                            Negative5 = value[4];
                            break;
                    }
                }
            }

            /// <summary>
            ///     Offset
            /// </summary>
            public int Offset {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(19).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(19).Take(4).ToArray(), 0);
                set {
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[19 + i] = val[i];
                }
            }

            private int Positive1 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(21).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(23).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 2 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[21 + offset + i] = val[i];
                }
            }

            private int Positive2 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(23).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(27).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 4 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[23 + offset + i] = val[i];
                }
            }

            private int Positive3 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(25).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(31).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 6 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[25 + offset + i] = val[i];
                }
            }

            private int Positive4 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(27).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(35).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 8 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[27 + offset + i] = val[i];
                }
            }

            private int Positive5 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(29).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(39).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 10 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[29 + offset + i] = val[i];
                }
            }

            private int Negative1 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(31).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(43).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 12 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[31 + offset + i] = val[i];
                }
            }

            private int Negative2 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(33).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(47).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 14 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[33 + offset + i] = val[i];
                }
            }

            private int Negative3 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(35).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(51).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 16 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[35 + offset + i] = val[i];
                }
            }

            private int Negative4 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(37).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(55).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 18 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[37 + offset + i] = val[i];
                }
            }

            private int Negative5 {
                get =>
                    Type == BodyTypes.Integration
                        ? BitConverter.ToInt16(Values.Skip(39).Take(2).ToArray(), 0)
                        : BitConverter.ToInt32(Values.Skip(59).Take(4).ToArray(), 0);
                set {
                    // offset
                    var offset = Type == BodyTypes.Separation ? 20 : 0;
                    // get bytes values
                    var val = Type == BodyTypes.Integration
                        ? BitConverter.GetBytes(Convert.ToInt16(value))
                        : BitConverter.GetBytes(value);
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[39 + offset + i] = val[i];
                }
            }

            /// <summary>
            ///     Set values
            /// </summary>
            public void SetValues(byte[] values) {
                // check values
                if (values == null || (values.Length != ShortSize && values.Length != LargeSize))
                    return;
                // check size
                for (var i = 0; i < values.Length; i++)
                    // set value
                    Values[i] = values[i];
            }

            /// <summary>
            ///     Get values
            /// </summary>
            /// <returns></returns>
            public byte[] GetValues() {
                return Values.ToArray();
            }

            /// <summary>
            ///     Reset values
            /// </summary>
            public void ResetValues() {
                // check size
                for (var i = 0; i < LargeSize; i++)
                    // set value
                    Values[i] = 0;
            }
        }

        /// <summary>
        ///     EZtorQ setting class
        /// </summary>
        public class SetInfo {
            /// <summary>
            ///     Constructor
            /// </summary>
            public SetInfo() {
                // add dummy
                Values.AddRange(new byte[Size]);
            }

            /// <summary>
            ///     Calibration data size
            /// </summary>
            public static int Size => 14;

            private List<byte> Values { get; } = new List<byte>();

            /// <summary>
            ///     Target torque enable state
            /// </summary>
            public bool TargetEnable {
                get => Values[0] == 1;
                set => Values[0] = value ? (byte)0x01 : (byte)0x00;
            }

            /// <summary>
            ///     Target torque
            /// </summary>
            public float Torque {
                get => BitConverter.ToUInt32(Values.Skip(1).Take(4).ToArray(), 0) / 100.0f;
                set {
                    // get bytes values
                    var val = BitConverter.GetBytes(Convert.ToUInt32(value * 100));
                    // check index
                    for (var i = 0; i < val.Length; i++)
                        // set value
                        Values[1 + i] = val[i];
                }
            }

            /// <summary>
            ///     Target torque to UInt32
            /// </summary>
            public uint TorqueUInt32 => BitConverter.ToUInt32(Values.Skip(1).Take(4).ToArray(), 0);

            /// <summary>
            ///     Auto clear
            /// </summary>
            public AutoClearTypes ClearType {
                get => (AutoClearTypes)Values[5];
                set => Values[5] = (byte)value;
            }

            /// <summary>
            ///     Tolerance
            /// </summary>
            public byte Tolerance {
                get => Values[6];
                set => Values[6] = value;
            }

            /// <summary>
            ///     Unit
            /// </summary>
            public UnitTypes Unit {
                get => (UnitTypes)Values[7];
                set => Values[7] = (byte)value;
            }

            /// <summary>
            ///     Mode
            /// </summary>
            public ModeTypes Mode {
                get => (ModeTypes)Values[8];
                set => Values[8] = (byte)value;
            }

            /// <summary>
            ///     Frequency
            /// </summary>
            public FrequencyTypes Frequency {
                get => (FrequencyTypes)Values[9];
                set => Values[9] = (byte)value;
            }

            /// <summary>
            ///     Direction
            /// </summary>
            public DirectionTypes Direction {
                get => (DirectionTypes)Values[10];
                set => Values[10] = (byte)value;
            }

            /// <summary>
            ///     Version
            /// </summary>
            public string Version {
                get => $@"{Values[11]}.{Values[12]}.{Values[13]}";
                set {
                    // get values
                    var values = value.Split('.');
                    // check length
                    if (values.Length != 3)
                        return;
                    // set value
                    Values[11] = Convert.ToByte(values[0]);
                    Values[12] = Convert.ToByte(values[1]);
                    Values[13] = Convert.ToByte(values[2]);
                }
            }

            /// <summary>
            ///     Set values
            /// </summary>
            /// <param name="values">values</param>
            public void SetValues(byte[] values) {
                // check length
                if (values == null || values.Length != Size)
                    return;
                // check size
                for (var i = 0; i < Size; i++)
                    // set value
                    Values[i] = values[i];
            }

            /// <summary>
            ///     Get values
            /// </summary>
            /// <returns></returns>
            public byte[] GetValues() {
                return Values.ToArray();
            }
        }
    }

    /// <summary>
    ///     Connect mode types
    /// </summary>
    public enum ConnectMode { Serial, Bluetooth }

    /// <summary>
    ///     Working command types
    /// </summary>
    public enum WorkCommand {
        ReqCalData = 0x00,
        ReqCalSetPoint = 0x01,
        ReqCalSave = 0x02,
        ReqCalTerminate = 0x03,
        ReqSetting = 0x04,
        ResCalData = 0x80,
        ResCalSetPoint = 0x81,
        ResCalSave = 0x82,
        ResSetting = 0x84,
        RepCurAdc = 0xA0
    }

    /// <summary>
    ///     Position types
    /// </summary>
    public enum WorkPosition {
        Offset = 0x00,
        Positive1,
        Positive2,
        Positive3,
        Positive4,
        Positive5,
        Negative1,
        Negative2,
        Negative3,
        Negative4,
        Negative5
    }

    /// <summary>
    ///     Working mode types
    /// </summary>
    public enum WorkMode { Torque, Calibration }

    /// <summary>
    ///     Connecting state types
    /// </summary>
    public enum ConnectionState {
        Disconnected,
        ConnectCalibration,
        ConnectSetting,
        Connected
    }

    /// <summary>
    ///     EZtorQ body types
    /// </summary>
    public enum BodyTypes {
        [Description(@"Built-in")]
        Integration,
        [Description(@"Built-out")]
        Separation
    }

    /// <summary>
    ///     EZTorQ unit types
    /// </summary>
    public enum UnitTypes {
        [Description("Kgf.cm")]
        KgfCm,
        [Description("Kgf.m")]
        KgfM,
        [Description("N.m")]
        Nm,
        [Description("cN.m")]
        CNm,
        [Description("Ozf.in")]
        OzfIn,
        [Description("Lbf.in")]
        LbfIn,
        [Description("Lbf.ft")]
        LbfFt
    }

    /// <summary>
    ///     Point types
    /// </summary>
    public enum PointTypes { Point3, Point5 }

    /// <summary>
    ///     Auto clear time types
    /// </summary>
    public enum AutoClearTypes {
        [Description(@"Disable")]
        Disable,
        [Description(@"500ms")]
        MSec500,
        [Description(@"1 sec")]
        Sec1,
        [Description(@"2 sec")]
        Sec2,
        [Description(@"3 sec")]
        Sec3,
        [Description(@"4 sec")]
        Sec4,
        [Description(@"5 sec")]
        Sec5
    }

    /// <summary>
    ///     Mode types
    /// </summary>
    public enum ModeTypes {
        [Description(@"Peak")]
        Peak,
        [Description(@"First-Peak")]
        FirstPeak,
        [Description(@"Track")]
        Track
    }

    /// <summary>
    ///     Frequency types
    /// </summary>
    public enum FrequencyTypes {
        [Description(@"100 Hz")]
        Hz100,
        [Description(@"500 Hz")]
        Hz500,
        [Description(@"1500 Hz")]
        Hz1500,
        [Description(@"2000 Hz")]
        Hz2000,
        [Description(@"3000 Hz")]
        Hz3000
    }

    /// <summary>
    ///     Direction types
    /// </summary>
    public enum DirectionTypes {
        [Description(@"CW")]
        Cw,
        [Description(@"CCW")]
        Ccw,
        [Description(@"Both")]
        Both
    }
}