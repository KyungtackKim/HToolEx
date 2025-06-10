using HToolEz.Defines.Enums;

namespace HToolEz.Defines.Entities;

/// <summary>
///     Calibration data class
/// </summary>
public sealed class CalibrationData {
    /// <summary>
    ///     Data size
    /// </summary>
    public static readonly int[] Size = [41, 68];

    /// <summary>
    ///     Constructor
    /// </summary>
    public CalibrationData() {
        // get calibration point types
        var types = Enum.GetValues(typeof(CalPointTypes)).Cast<CalPointTypes>();
        // check types
        foreach (var type in types)
            // add the point
            CalPoints.Add(type, 0);
    }

    /// <summary>
    ///     Body type
    /// </summary>
    public DeviceBodyTypes Body { get; private set; }

    /// <summary>
    ///     Model number (EZtorQ-III {model}i
    /// </summary>
    public int Model { get; private set; }

    /// <summary>
    ///     Max torque
    /// </summary>
    public float MaxTorque { get; set; }

    /// <summary>
    ///     Body serial number
    /// </summary>
    public string BodySerial { get; set; } = "00000000";

    /// <summary>
    ///     Sensor serial number
    /// </summary>
    public string SensorSerial { get; set; } = "00000000";

    /// <summary>
    ///     Torque unit
    /// </summary>
    public UnitTypes Unit { get; set; }

    /// <summary>
    ///     Point type
    /// </summary>
    public CalPointModeTypes CalPointMode { get; set; }

    /// <summary>
    ///     Point values
    /// </summary>
    public Dictionary<CalPointTypes, int> CalPoints { get; } = new();

    /// <summary>
    ///     Write to buffer
    /// </summary>
    /// <param name="buffer"></param>
    public void WriteTo(Span<byte> buffer) {
        // get data buffer
        var buf = buffer[5..];
        // get the serial
        var body = int.TryParse(BodySerial, out var b) ? b : 0;
        var sensor = int.TryParse(SensorSerial, out var s) ? s : 0;

        // write values
        buf[0] = (byte)Body;
        BitConverter.TryWriteBytes(buf[1..5], Model);
        BitConverter.TryWriteBytes(buf[5..9], MaxTorque);
        BitConverter.TryWriteBytes(buf[9..13], body);
        BitConverter.TryWriteBytes(buf[13..17], sensor);
        buf[17] = (byte)Unit;
        buf[18] = (byte)CalPointMode;
        // check body mode
        switch (Body) {
            case DeviceBodyTypes.Integrated:
                BitConverter.TryWriteBytes(buf[19..21], (short)CalPoints[CalPointTypes.PointZero]);
                // check point mode
                switch (CalPointMode) {
                    case CalPointModeTypes.PointThird:
                        BitConverter.TryWriteBytes(buf[21..23], (short)CalPoints[CalPointTypes.Point10P]);
                        BitConverter.TryWriteBytes(buf[23..25], (short)CalPoints[CalPointTypes.Point50P]);
                        BitConverter.TryWriteBytes(buf[25..27], (short)CalPoints[CalPointTypes.Point100P]);
                        BitConverter.TryWriteBytes(buf[27..29], (short)0);
                        BitConverter.TryWriteBytes(buf[29..31], (short)0);
                        BitConverter.TryWriteBytes(buf[31..33], (short)CalPoints[CalPointTypes.Point10M]);
                        BitConverter.TryWriteBytes(buf[33..35], (short)CalPoints[CalPointTypes.Point50M]);
                        BitConverter.TryWriteBytes(buf[35..37], (short)CalPoints[CalPointTypes.Point100M]);
                        BitConverter.TryWriteBytes(buf[37..39], (short)0);
                        BitConverter.TryWriteBytes(buf[39..41], (short)0);
                        break;
                    case CalPointModeTypes.PointFifth:
                        BitConverter.TryWriteBytes(buf[21..23], (short)CalPoints[CalPointTypes.Point20P]);
                        BitConverter.TryWriteBytes(buf[23..25], (short)CalPoints[CalPointTypes.Point40P]);
                        BitConverter.TryWriteBytes(buf[25..27], (short)CalPoints[CalPointTypes.Point60P]);
                        BitConverter.TryWriteBytes(buf[27..29], (short)CalPoints[CalPointTypes.Point80P]);
                        BitConverter.TryWriteBytes(buf[29..31], (short)CalPoints[CalPointTypes.Point100P]);
                        BitConverter.TryWriteBytes(buf[31..33], (short)CalPoints[CalPointTypes.Point20M]);
                        BitConverter.TryWriteBytes(buf[33..35], (short)CalPoints[CalPointTypes.Point40M]);
                        BitConverter.TryWriteBytes(buf[35..37], (short)CalPoints[CalPointTypes.Point60M]);
                        BitConverter.TryWriteBytes(buf[37..39], (short)CalPoints[CalPointTypes.Point80M]);
                        BitConverter.TryWriteBytes(buf[39..41], (short)CalPoints[CalPointTypes.Point100M]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{CalPointMode} 는 지원하지 않습니다.");
                }

                break;
            case DeviceBodyTypes.Sperated:
                BitConverter.TryWriteBytes(buf[19..23], CalPoints[CalPointTypes.PointZero]);
                // check point mode
                switch (CalPointMode) {
                    case CalPointModeTypes.PointThird:
                        BitConverter.TryWriteBytes(buf[23..27], CalPoints[CalPointTypes.Point10P]);
                        BitConverter.TryWriteBytes(buf[27..31], CalPoints[CalPointTypes.Point50P]);
                        BitConverter.TryWriteBytes(buf[31..35], CalPoints[CalPointTypes.Point100P]);
                        BitConverter.TryWriteBytes(buf[35..39], 0);
                        BitConverter.TryWriteBytes(buf[39..43], 0);
                        BitConverter.TryWriteBytes(buf[43..47], CalPoints[CalPointTypes.Point10M]);
                        BitConverter.TryWriteBytes(buf[47..51], CalPoints[CalPointTypes.Point50M]);
                        BitConverter.TryWriteBytes(buf[51..55], CalPoints[CalPointTypes.Point100M]);
                        BitConverter.TryWriteBytes(buf[55..59], 0);
                        BitConverter.TryWriteBytes(buf[59..63], 0);
                        break;
                    case CalPointModeTypes.PointFifth:
                        BitConverter.TryWriteBytes(buf[23..27], CalPoints[CalPointTypes.Point20P]);
                        BitConverter.TryWriteBytes(buf[27..31], CalPoints[CalPointTypes.Point40P]);
                        BitConverter.TryWriteBytes(buf[31..35], CalPoints[CalPointTypes.Point60P]);
                        BitConverter.TryWriteBytes(buf[35..39], CalPoints[CalPointTypes.Point80P]);
                        BitConverter.TryWriteBytes(buf[39..43], CalPoints[CalPointTypes.Point100P]);
                        BitConverter.TryWriteBytes(buf[43..47], CalPoints[CalPointTypes.Point20M]);
                        BitConverter.TryWriteBytes(buf[47..51], CalPoints[CalPointTypes.Point40M]);
                        BitConverter.TryWriteBytes(buf[51..55], CalPoints[CalPointTypes.Point60M]);
                        BitConverter.TryWriteBytes(buf[55..59], CalPoints[CalPointTypes.Point80M]);
                        BitConverter.TryWriteBytes(buf[59..63], CalPoints[CalPointTypes.Point100M]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{CalPointMode} 는 지원하지 않습니다.");
                }

                break;
            default:
                throw new ArgumentOutOfRangeException($"{Body} 는 지원하지 않습니다.");
        }
    }

    /// <summary>
    ///     Read from the packet
    /// </summary>
    /// <param name="buffer">buffer</param>
    public void ReadFrom(ReadOnlySpan<byte> buffer) {
        // get data buffer
        var buf = buffer[5..];
        // read values
        Body = (DeviceBodyTypes)buf[0];
        Model = BitConverter.ToInt32(buf[1..5]);
        MaxTorque = BitConverter.ToSingle(buf[5..9]);
        BodySerial = BitConverter.ToInt32(buf[9..13]).ToString("D8");
        SensorSerial = BitConverter.ToInt32(buf[13..17]).ToString("D8");
        Unit = (UnitTypes)buf[17];
        CalPointMode = (CalPointModeTypes)buf[18];
        // check body mode
        switch (Body) {
            case DeviceBodyTypes.Integrated:
                CalPoints[CalPointTypes.PointZero] = BitConverter.ToInt32(buf[19..21]);
                // check point mode
                switch (CalPointMode) {
                    case CalPointModeTypes.PointThird:
                        CalPoints[CalPointTypes.Point10P] = BitConverter.ToInt32(buf[21..23]);
                        CalPoints[CalPointTypes.Point50P] = BitConverter.ToInt32(buf[23..25]);
                        CalPoints[CalPointTypes.Point100P] = BitConverter.ToInt32(buf[25..27]);
                        CalPoints[CalPointTypes.Point20P] = 0;
                        CalPoints[CalPointTypes.Point40P] = 0;
                        CalPoints[CalPointTypes.Point60P] = 0;
                        CalPoints[CalPointTypes.Point80P] = 0;
                        CalPoints[CalPointTypes.Point10M] = BitConverter.ToInt32(buf[31..33]);
                        CalPoints[CalPointTypes.Point50M] = BitConverter.ToInt32(buf[33..35]);
                        CalPoints[CalPointTypes.Point100M] = BitConverter.ToInt32(buf[35..37]);
                        CalPoints[CalPointTypes.Point20M] = 0;
                        CalPoints[CalPointTypes.Point40M] = 0;
                        CalPoints[CalPointTypes.Point60M] = 0;
                        CalPoints[CalPointTypes.Point80M] = 0;

                        break;
                    case CalPointModeTypes.PointFifth:
                        CalPoints[CalPointTypes.Point10P] = 0;
                        CalPoints[CalPointTypes.Point50P] = 0;
                        CalPoints[CalPointTypes.Point20P] = BitConverter.ToInt32(buf[21..23]);
                        CalPoints[CalPointTypes.Point40P] = BitConverter.ToInt32(buf[23..25]);
                        CalPoints[CalPointTypes.Point60P] = BitConverter.ToInt32(buf[25..27]);
                        CalPoints[CalPointTypes.Point80P] = BitConverter.ToInt32(buf[27..29]);
                        CalPoints[CalPointTypes.Point100P] = BitConverter.ToInt32(buf[29..31]);
                        CalPoints[CalPointTypes.Point10M] = 0;
                        CalPoints[CalPointTypes.Point50M] = 0;
                        CalPoints[CalPointTypes.Point20M] = BitConverter.ToInt32(buf[31..33]);
                        CalPoints[CalPointTypes.Point40M] = BitConverter.ToInt32(buf[33..35]);
                        CalPoints[CalPointTypes.Point60M] = BitConverter.ToInt32(buf[35..37]);
                        CalPoints[CalPointTypes.Point80M] = BitConverter.ToInt32(buf[37..39]);
                        CalPoints[CalPointTypes.Point100M] = BitConverter.ToInt32(buf[39..41]);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{CalPointMode} 는 지원하지 않습니다.");
                }

                break;
            case DeviceBodyTypes.Sperated:
                CalPoints[CalPointTypes.PointZero] = BitConverter.ToInt32(buf[19..23]);
                // check point mode
                switch (CalPointMode) {
                    case CalPointModeTypes.PointThird:
                        CalPoints[CalPointTypes.Point10P] = BitConverter.ToInt32(buf[23..27]);
                        CalPoints[CalPointTypes.Point50P] = BitConverter.ToInt32(buf[27..31]);
                        CalPoints[CalPointTypes.Point100P] = BitConverter.ToInt32(buf[31..35]);
                        CalPoints[CalPointTypes.Point20P] = 0;
                        CalPoints[CalPointTypes.Point40P] = 0;
                        CalPoints[CalPointTypes.Point60P] = 0;
                        CalPoints[CalPointTypes.Point80P] = 0;
                        CalPoints[CalPointTypes.Point10M] = BitConverter.ToInt32(buf[43..47]);
                        CalPoints[CalPointTypes.Point50M] = BitConverter.ToInt32(buf[47..51]);
                        CalPoints[CalPointTypes.Point100M] = BitConverter.ToInt32(buf[51..55]);
                        CalPoints[CalPointTypes.Point20M] = 0;
                        CalPoints[CalPointTypes.Point40M] = 0;
                        CalPoints[CalPointTypes.Point60M] = 0;
                        CalPoints[CalPointTypes.Point80M] = 0;

                        break;
                    case CalPointModeTypes.PointFifth:
                        CalPoints[CalPointTypes.Point10P] = 0;
                        CalPoints[CalPointTypes.Point50P] = 0;
                        CalPoints[CalPointTypes.Point20P] = BitConverter.ToInt32(buf[23..27]);
                        CalPoints[CalPointTypes.Point40P] = BitConverter.ToInt32(buf[27..31]);
                        CalPoints[CalPointTypes.Point60P] = BitConverter.ToInt32(buf[31..35]);
                        CalPoints[CalPointTypes.Point80P] = BitConverter.ToInt32(buf[35..39]);
                        CalPoints[CalPointTypes.Point100P] = BitConverter.ToInt32(buf[39..43]);
                        CalPoints[CalPointTypes.Point10M] = 0;
                        CalPoints[CalPointTypes.Point50M] = 0;
                        CalPoints[CalPointTypes.Point20M] = BitConverter.ToInt32(buf[43..47]);
                        CalPoints[CalPointTypes.Point40M] = BitConverter.ToInt32(buf[47..51]);
                        CalPoints[CalPointTypes.Point60M] = BitConverter.ToInt32(buf[51..55]);
                        CalPoints[CalPointTypes.Point80M] = BitConverter.ToInt32(buf[55..59]);
                        CalPoints[CalPointTypes.Point100M] = BitConverter.ToInt32(buf[59..63]);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{CalPointMode} 는 지원하지 않습니다.");
                }

                break;
            default:
                throw new ArgumentOutOfRangeException($"{Body} 는 지원하지 않습니다.");
        }
    }
}