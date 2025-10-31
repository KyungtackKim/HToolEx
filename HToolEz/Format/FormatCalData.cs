using HToolEz.Type;
using HToolEz.Util;

namespace HToolEz.Format;

/// <summary>
///     Format device calibration data class
/// </summary>
public sealed class FormatCalData {
    /// <summary>
    ///     Format calibration data size
    /// </summary>
    private static readonly int[] Size = [41, 63];

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="data">data packet</param>
    /// <exception cref="ArgumentException">Thrown when data size is not 41 or 63 bytes</exception>
    public FormatCalData(ReadOnlyMemory<byte> data) {
        // get the data size (only valid data)
        var size = data.Length;
        // check the data size
        if (!Size.Contains(size))
            throw new ArgumentException($"Invalid calibration data size: {size} bytes. Expected size: {string.Join(" or ", Size)} bytes.",
                nameof(data));

        // get the data
        var d = data.Span;
        /*      common information      */
        // check the body item
        if (Utils.IsKnownItem<BodyTypes>(d[0]))
            // set the body
            Body = (BodyTypes)d[0];
        // convert values
        Utils.ConvertValue(d[1..5], out int model);
        Utils.ConvertValue(d[5..9], out float max);
        Utils.ConvertValue(d[9..13], out int body);
        Utils.ConvertValue(d[13..17], out int sensor);
        // set the values
        Model        = (uint)model;
        MaxTorque    = max;
        BodySerial   = (uint)body;
        SensorSerial = (uint)sensor;
        // check the unit item
        if (Utils.IsKnownItem<UnitTypes>(d[17]))
            // set the unit
            Unit = (UnitTypes)d[17];
        // check the point type
        if (Utils.IsKnownItem<CalibrationTypes>(d[18]))
            // set the point type
            CalType = (CalibrationTypes)d[18];

        /*      calibration information     */
        // check body type
        switch (Body) {
            // validate body type and data size consistency
            case BodyTypes.Integrated when size != Size[0]:
                throw new ArgumentException($"Integrated body type requires {Size[0]} bytes, but received {size} bytes.", nameof(data));
            case BodyTypes.Separated when size != Size[1]:
                throw new ArgumentException($"Separated body type requires {Size[1]} bytes, but received {size} bytes.", nameof(data));
            case BodyTypes.Separated:
                // convert values (4 bytes each, starting from index 19, protocol v0.1)
                Utils.ConvertValue(d[19..23], out var offsetSep);
                Utils.ConvertValue(d[23..27], out var p1Sep);
                Utils.ConvertValue(d[27..31], out var p2Sep);
                Utils.ConvertValue(d[31..35], out var p3Sep);
                Utils.ConvertValue(d[35..39], out var p4Sep);
                Utils.ConvertValue(d[39..43], out var p5Sep);
                Utils.ConvertValue(d[43..47], out var n1Sep);
                Utils.ConvertValue(d[47..51], out var n2Sep);
                Utils.ConvertValue(d[51..55], out var n3Sep);
                Utils.ConvertValue(d[55..59], out var n4Sep);
                Utils.ConvertValue(d[59..63], out var n5Sep);
                // set the values
                Offset    = offsetSep;
                Positives = [p1Sep, p2Sep, p3Sep, p4Sep, p5Sep];
                Negatives = [n1Sep, n2Sep, n3Sep, n4Sep, n5Sep];
                break;
            case BodyTypes.Integrated:
            default:
                // convert values (2 bytes each, starting from index 19, protocol v0.9)
                Utils.ConvertValue(d[19..21], out var offsetInt);
                Utils.ConvertValue(d[21..23], out var p1Int);
                Utils.ConvertValue(d[23..25], out var p2Int);
                Utils.ConvertValue(d[25..27], out var p3Int);
                Utils.ConvertValue(d[27..29], out var p4Int);
                Utils.ConvertValue(d[29..31], out var p5Int);
                Utils.ConvertValue(d[31..33], out var n1Int);
                Utils.ConvertValue(d[33..35], out var n2Int);
                Utils.ConvertValue(d[35..37], out var n3Int);
                Utils.ConvertValue(d[37..39], out var n4Int);
                Utils.ConvertValue(d[39..41], out var n5Int);
                // set the values
                Offset    = offsetInt;
                Positives = [p1Int, p2Int, p3Int, p4Int, p5Int];
                Negatives = [n1Int, n2Int, n3Int, n4Int, n5Int];
                break;
        }
    }

    /// <summary>
    ///     Body type
    /// </summary>
    public BodyTypes Body { get; } = BodyTypes.Integrated;

    /// <summary>
    ///     Model number
    /// </summary>
    public uint Model { get; }

    /// <summary>
    ///     Max torque
    /// </summary>
    public float MaxTorque { get; }

    /// <summary>
    ///     Body serial number
    /// </summary>
    public uint BodySerial { get; }

    /// <summary>
    ///     Sensor serial number
    /// </summary>
    public uint SensorSerial { get; }

    /// <summary>
    ///     Unit
    /// </summary>
    public UnitTypes Unit { get; } = UnitTypes.KgfCm;

    /// <summary>
    ///     Calibration type
    /// </summary>
    public CalibrationTypes CalType { get; } = CalibrationTypes.ThreePoint;

    /// <summary>
    ///     Offset
    /// </summary>
    public int Offset { get; }

    /// <summary>
    ///     Positive values
    /// </summary>
    public int[] Positives { get; }

    /// <summary>
    ///     Negative values
    /// </summary>
    public int[] Negatives { get; }

    /// <summary>
    ///     Convert calibration data to byte array
    /// </summary>
    /// <returns>byte array (41 bytes for Integrated, 63 bytes for Separated)</returns>
    public byte[] ToBytes() {
        // determine size based on body type
        var size  = Body == BodyTypes.Separated ? Size[1] : Size[0];
        var bytes = new byte[size];

        // common information (0-18)
        bytes[0] = (byte)Body;
        Utils.WriteValue(bytes, 1, (int)Model);
        Utils.WriteValue(bytes, 5, MaxTorque);
        Utils.WriteValue(bytes, 9, (int)BodySerial);
        Utils.WriteValue(bytes, 13, (int)SensorSerial);
        bytes[17] = (byte)Unit;
        bytes[18] = (byte)CalType;

        // calibration data
        switch (Body) {
            case BodyTypes.Separated:
                // 4 bytes each (int32, big-endian)
                Utils.WriteValue(bytes, 19, Offset);
                for (var i = 0; i < 5; i++)
                    Utils.WriteValue(bytes, 23 + i * 4, Positives[i]);
                for (var i = 0; i < 5; i++)
                    Utils.WriteValue(bytes, 43 + i * 4, Negatives[i]);
                break;

            case BodyTypes.Integrated:
            default:
                // 2 bytes each (ushort, big-endian)
                Utils.WriteValue(bytes, 19, (ushort)Offset);
                for (var i = 0; i < 5; i++)
                    Utils.WriteValue(bytes, 21 + i * 2, (ushort)Positives[i]);
                for (var i = 0; i < 5; i++)
                    Utils.WriteValue(bytes, 31 + i * 2, (ushort)Negatives[i]);
                break;
        }

        return bytes;
    }
}