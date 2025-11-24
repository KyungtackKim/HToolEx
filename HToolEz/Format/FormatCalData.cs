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
    ///     Mapping from CalPointTypes to array index for 3-point calibration (positive)
    /// </summary>
    private static readonly Dictionary<CalPointTypes, int> ThreePointPositiveMap = new() {
        [CalPointTypes.Point10P] = 0, [CalPointTypes.Point50P] = 1, [CalPointTypes.Point100P] = 2
    };

    /// <summary>
    ///     Mapping from CalPointTypes to array index for 3-point calibration (negative)
    /// </summary>
    private static readonly Dictionary<CalPointTypes, int> ThreePointNegativeMap = new() {
        [CalPointTypes.Point10M] = 0, [CalPointTypes.Point50M] = 1, [CalPointTypes.Point100M] = 2
    };

    /// <summary>
    ///     Mapping from CalPointTypes to array index for 5-point calibration (positive)
    /// </summary>
    private static readonly Dictionary<CalPointTypes, int> FivePointPositiveMap = new() {
        [CalPointTypes.Point20P]  = 0,
        [CalPointTypes.Point40P]  = 1,
        [CalPointTypes.Point60P]  = 2,
        [CalPointTypes.Point80P]  = 3,
        [CalPointTypes.Point100P] = 4
    };

    /// <summary>
    ///     Mapping from CalPointTypes to array index for 5-point calibration (negative)
    /// </summary>
    private static readonly Dictionary<CalPointTypes, int> FivePointNegativeMap = new() {
        [CalPointTypes.Point20M]  = 0,
        [CalPointTypes.Point40M]  = 1,
        [CalPointTypes.Point60M]  = 2,
        [CalPointTypes.Point80M]  = 3,
        [CalPointTypes.Point100M] = 4
    };

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
        // convert values (EZTorQ-III uses little-endian)
        Utils.ConvertValue(d[1..5], out int model);
        Utils.ConvertValue(d[5..9], out int max);
        Utils.ConvertValue(d[9..13], out int body);
        Utils.ConvertValue(d[13..17], out int sensor);
        // set the values
        Model        = (uint)model;
        MaxTorque    = max / 100.0f; // Convert integer to float (scaled by 100)
        BodySerial   = (uint)body;
        SensorSerial = (uint)sensor;
        // check the unit item
        if (Utils.IsKnownItem<UnitTypes>(d[17]))
            // set the unit
            Unit = (UnitTypes)d[17];
        // check the point type
        if (Utils.IsKnownItem<CalPointModeTypes>(d[18]))
            // set the point type
            CalType = (CalPointModeTypes)d[18];

        /*      calibration information     */
        // check body type
        switch (Body) {
            // validate body type and data size consistency
            case BodyTypes.Integrated when size != Size[0]:
                throw new ArgumentException($"Integrated body type requires {Size[0]} bytes, but received {size} bytes.", nameof(data));
            case BodyTypes.Separated when size != Size[1]:
                throw new ArgumentException($"Separated body type requires {Size[1]} bytes, but received {size} bytes.", nameof(data));
            case BodyTypes.Separated:
                // convert values (4 bytes each, starting from index 19, protocol v0.1, little-endian)
                Utils.ConvertValue(d[19..23], out int offsetSep);
                Utils.ConvertValue(d[23..27], out int p1Sep);
                Utils.ConvertValue(d[27..31], out int p2Sep);
                Utils.ConvertValue(d[31..35], out int p3Sep);
                Utils.ConvertValue(d[35..39], out int p4Sep);
                Utils.ConvertValue(d[39..43], out int p5Sep);
                Utils.ConvertValue(d[43..47], out int n1Sep);
                Utils.ConvertValue(d[47..51], out int n2Sep);
                Utils.ConvertValue(d[51..55], out int n3Sep);
                Utils.ConvertValue(d[55..59], out int n4Sep);
                Utils.ConvertValue(d[59..63], out int n5Sep);
                // set the values
                Offset    = offsetSep;
                Positives = [p1Sep, p2Sep, p3Sep, p4Sep, p5Sep];
                Negatives = [n1Sep, n2Sep, n3Sep, n4Sep, n5Sep];
                break;
            case BodyTypes.Integrated:
            default:
                // convert values (2 bytes each, starting from index 19, protocol v0.9, little-endian)
                Utils.ConvertValue(d[19..21], out ushort offsetInt);
                Utils.ConvertValue(d[21..23], out ushort p1Int);
                Utils.ConvertValue(d[23..25], out ushort p2Int);
                Utils.ConvertValue(d[25..27], out ushort p3Int);
                Utils.ConvertValue(d[27..29], out ushort p4Int);
                Utils.ConvertValue(d[29..31], out ushort p5Int);
                Utils.ConvertValue(d[31..33], out ushort n1Int);
                Utils.ConvertValue(d[33..35], out ushort n2Int);
                Utils.ConvertValue(d[35..37], out ushort n3Int);
                Utils.ConvertValue(d[37..39], out ushort n4Int);
                Utils.ConvertValue(d[39..41], out ushort n5Int);
                // set the values
                Offset    = offsetInt;
                Positives = [p1Int, p2Int, p3Int, p4Int, p5Int];
                Negatives = [n1Int, n2Int, n3Int, n4Int, n5Int];
                break;
        }
    }

    /// <summary>
    ///     Constructor that creates a new instance based on source data with updated calibration points
    /// </summary>
    /// <param name="src">Source calibration data</param>
    /// <param name="calPoints">Calibration points to update (key: point type, value: new calibration value)</param>
    public FormatCalData(FormatCalData src, Dictionary<CalPointTypes, int> calPoints) {
        // Copy all values from source
        Body         = src.Body;
        Model        = src.Model;
        MaxTorque    = src.MaxTorque;
        BodySerial   = src.BodySerial;
        SensorSerial = src.SensorSerial;
        Unit         = src.Unit;
        CalType      = src.CalType;

        // Copy calibration values from source
        Offset    = src.Offset;
        Positives = [.. src.Positives];
        Negatives = [.. src.Negatives];

        // Select mapping tables based on calibration type
        var positiveMap = CalType == CalPointModeTypes.ThreePoint ? ThreePointPositiveMap : FivePointPositiveMap;
        var negativeMap = CalType == CalPointModeTypes.ThreePoint ? ThreePointNegativeMap : FivePointNegativeMap;

        // Apply calibration point updates
        foreach (var (point, value) in calPoints)
            // check the point
            if (point == CalPointTypes.PointZero)
                // set the offset value
                Offset = value;
            else if (positiveMap.TryGetValue(point, out var posIdx))
                // set the positive value
                Positives[posIdx] = value;
            else if (negativeMap.TryGetValue(point, out var negIdx))
                // set the negative value
                Negatives[negIdx] = value;

        // For 3-point calibration, ensure unused points are zero
        if (CalType != CalPointModeTypes.ThreePoint)
            return;
        // reset the not used value
        Positives[3] = Positives[4] = 0;
        Negatives[3] = Negatives[4] = 0;
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
    public CalPointModeTypes CalType { get; } = CalPointModeTypes.ThreePoint;

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

        // common information (0-18, little-endian)
        bytes[0] = (byte)Body;
        Utils.WriteValue(bytes, 1, (int)Model);
        Utils.WriteValue(bytes, 5, (int)(MaxTorque * 100.0f));
        Utils.WriteValue(bytes, 9, (int)BodySerial);
        Utils.WriteValue(bytes, 13, (int)SensorSerial);
        bytes[17] = (byte)Unit;
        bytes[18] = (byte)CalType;

        // calibration data (little-endian)
        switch (Body) {
            case BodyTypes.Separated:
                // 4 bytes each (int32, little-endian)
                Utils.WriteValue(bytes, 19, Offset);
                for (var i = 0; i < 5; i++)
                    Utils.WriteValue(bytes, 23 + i * 4, Positives[i]);
                for (var i = 0; i < 5; i++)
                    Utils.WriteValue(bytes, 43 + i * 4, Negatives[i]);
                break;

            case BodyTypes.Integrated:
            default:
                // 2 bytes each (ushort, little-endian)
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