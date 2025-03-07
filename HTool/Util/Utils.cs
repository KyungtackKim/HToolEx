using HTool.Type;
using JetBrains.Annotations;

namespace HTool.Util;

/// <summary>
///     Utilities class
/// </summary>
[PublicAPI]
public static class Utils {
    /// <summary>
    ///     Calculate CRC value for the packet
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>result</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<byte> CalculateCrc(IEnumerable<byte> packet) {
        var crc = new byte[] { 0xFF, 0xFF };
        ushort crcFull = 0xFFFF;
        // check total packet
        foreach (var data in packet) {
            // XOR 1 byte
            crcFull = (ushort)(crcFull ^ data);
            // cyclic redundancy check
            for (var j = 0; j < 8; j++) {
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

        // result
        return crc;
    }

    /// <summary>
    ///     Calculate check-sum value for the packet
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>result</returns>
    public static int CalculateCheckSum(IEnumerable<byte> packet) {
        // get 
        return packet.Sum(x => x);
    }

    /// <summary>
    ///     Convert to unit for torque
    /// </summary>
    /// <param name="value">torque</param>
    /// <param name="src">source unit</param>
    /// <param name="dst">destination unit</param>
    /// <returns>converted torque</returns>
    public static float ConvertToUnit(float value, UnitTypes src, UnitTypes dst) {
        // const value for unit N.m
        const float nCm = 100.0f;
        const float kgfM = 0.101971621f;
        const float kgfCm = 10.1971621f;
        const float lbfIn = 8.85074579f;
        const float lbfFt = 0.737562149f;
        const float ozfIn = 141.611932f;
        // check defined source types
        if (Enum.IsDefined(typeof(UnitTypes), src) == false)
            // throw exception
            throw new ArgumentOutOfRangeException(nameof(src), src, null);
        // check defined destination types
        if (Enum.IsDefined(typeof(UnitTypes), dst) == false)
            // throw exception
            throw new ArgumentOutOfRangeException(nameof(dst), dst, null);
        // convert source to N.m
        var valueInNm = src switch {
            UnitTypes.KgfCm => value / kgfCm,
            UnitTypes.KgfM => value / kgfM,
            UnitTypes.Nm => value,
            UnitTypes.NCm => value / nCm,
            UnitTypes.LbfIn => value / lbfIn,
            UnitTypes.OzfIn => value / ozfIn,
            UnitTypes.LbfFt => value / lbfFt,
            _ => value
        };
        // convert N.m to destination
        return dst switch {
            UnitTypes.KgfCm => valueInNm * kgfCm,
            UnitTypes.KgfM => valueInNm * kgfM,
            UnitTypes.Nm => valueInNm,
            UnitTypes.NCm => valueInNm * nCm,
            UnitTypes.LbfIn => valueInNm * lbfIn,
            UnitTypes.OzfIn => valueInNm * ozfIn,
            UnitTypes.LbfFt => valueInNm * lbfFt,
            _ => valueInNm
        };
    }

    /// <summary>
    ///     Get the time laps for milliseconds
    /// </summary>
    /// <param name="from">from time</param>
    /// <returns>total milliseconds</returns>
    public static double TimeLapsMs(DateTime from) {
        return (DateTime.Now - from).TotalMilliseconds;
    }

    /// <summary>
    ///     Get the time laps for seconds
    /// </summary>
    /// <param name="from">from time</param>
    /// <returns>total seconds</returns>
    public static double TimeLapsSec(DateTime from) {
        return (DateTime.Now - from).TotalSeconds;
    }

    /// <summary>
    ///     Get the time laps for diff for seconds
    /// </summary>
    /// <param name="src">source</param>
    /// <param name="dst">destination</param>
    /// <returns>second</returns>
    public static ulong TimeLapsDifference(DateTime src, DateTime dst) {
        // get time laps
        return Convert.ToUInt64(Math.Abs((src - dst).TotalSeconds));
    }

    /// <summary>
    ///     Set value for single type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">float value</param>
    public static void ConvertValue(byte[] values, out float value) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set value
        value = bin.ReadSingle();
    }

    /// <summary>
    ///     Set value for ushort type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">ushort value</param>
    public static void ConvertValue(byte[] values, out ushort value) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set value
        value = bin.ReadUInt16();
    }

    /// <summary>
    ///     Set value for int type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">int value</param>
    public static void ConvertValue(byte[] values, out int value) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set value
        value = bin.ReadInt32();
    }

    /// <summary>
    ///     Get ushort values from text
    /// </summary>
    /// <param name="text">value</param>
    /// <returns>values</returns>
    public static ushort[] GetWordValuesFromText(string text) {
        var values = new List<ushort>();
        // get text array
        var array = text.ToCharArray();
        // get array length
        var length = array.Length / 2;
        // get remain offset
        var remain = array.Length % 2 > 0;
        // check array length
        for (var i = 0; i < length; i++)
            // add values
            values.Add((ushort)((array[i * 2] << 8) | array[i * 2 + 1]));
        // check remain offset
        if (remain)
            // add value
            values.Add(Convert.ToUInt16(text[^1] << 8));
        // result
        return values.ToArray();
    }

    /// <summary>
    ///     Get ushort values from text
    /// </summary>
    /// <param name="text">value</param>
    /// <returns>values</returns>
    public static ushort[] GetIntValuesFromText(string text) {
        var values = new List<ushort>();
        // check length
        for (var i = 0; i < text.Length / 2; i++)
            // set values
            values.Add(Convert.ToUInt16(text.Substring(i * 2, 2)));
        // result
        return values.ToArray();
    }

    /// <summary>
    ///     Get ushort values from network address
    /// </summary>
    /// <param name="addr">address</param>
    /// <returns>values</returns>
    public static ushort[] GetValuesFromAddr(string addr) {
        // split data
        var data = addr.Split('.');
        // check length
        var values = data.Select(x => Convert.ToUInt16(x)).ToList();
        // result
        return values.ToArray();
    }

    /// <summary>
    ///     Get ushort values from single value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ushort[] GetValuesFromSingle(float value) {
        // get values
        var values = BitConverter.GetBytes(value).Reverse().ToArray();
        // result
        return [(ushort)((values[0] << 8) | values[1]), (ushort)((values[2] << 8) | values[3])];
    }

    /// <summary>
    ///     Swap the item
    /// </summary>
    /// <param name="list">list</param>
    /// <param name="source">source</param>
    /// <param name="dest">destination</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>result</returns>
    public static bool Swap<T>(List<T> list, int source, int dest) {
        // check list
        ArgumentNullException.ThrowIfNull(list);
        // check index
        if (source < 0 || source >= list.Count || dest < 0 || dest >= list.Count)
            return false;
        // swap
        (list[source], list[dest]) = (list[dest], list[source]);
        // result
        return true;
    }
}