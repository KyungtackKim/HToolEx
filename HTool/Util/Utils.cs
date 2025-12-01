using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HTool.Type;

namespace HTool.Util;

/// <summary>
///     Utilities class
/// </summary>
public static class Utils {
    /// <summary>
    ///     MODBUS-RTU CRC-16 lookup table (256 entries)
    /// </summary>
    private static readonly ushort[] ModbusCrc16Table = [
        0x0000,
        0xC0C1,
        0xC181,
        0x0140,
        0xC301,
        0x03C0,
        0x0280,
        0xC241,
        0xC601,
        0x06C0,
        0x0780,
        0xC741,
        0x0500,
        0xC5C1,
        0xC481,
        0x0440,
        0xCC01,
        0x0CC0,
        0x0D80,
        0xCD41,
        0x0F00,
        0xCFC1,
        0xCE81,
        0x0E40,
        0x0A00,
        0xCAC1,
        0xCB81,
        0x0B40,
        0xC901,
        0x09C0,
        0x0880,
        0xC841,
        0xD801,
        0x18C0,
        0x1980,
        0xD941,
        0x1B00,
        0xDBC1,
        0xDA81,
        0x1A40,
        0x1E00,
        0xDEC1,
        0xDF81,
        0x1F40,
        0xDD01,
        0x1DC0,
        0x1C80,
        0xDC41,
        0x1400,
        0xD4C1,
        0xD581,
        0x1540,
        0xD701,
        0x17C0,
        0x1680,
        0xD641,
        0xD201,
        0x12C0,
        0x1380,
        0xD341,
        0x1100,
        0xD1C1,
        0xD081,
        0x1040,
        0xF001,
        0x30C0,
        0x3180,
        0xF141,
        0x3300,
        0xF3C1,
        0xF281,
        0x3240,
        0x3600,
        0xF6C1,
        0xF781,
        0x3740,
        0xF501,
        0x35C0,
        0x3480,
        0xF441,
        0x3C00,
        0xFCC1,
        0xFD81,
        0x3D40,
        0xFF01,
        0x3FC0,
        0x3E80,
        0xFE41,
        0xFA01,
        0x3AC0,
        0x3B80,
        0xFB41,
        0x3900,
        0xF9C1,
        0xF881,
        0x3840,
        0x2800,
        0xE8C1,
        0xE981,
        0x2940,
        0xEB01,
        0x2BC0,
        0x2A80,
        0xEA41,
        0xEE01,
        0x2EC0,
        0x2F80,
        0xEF41,
        0x2D00,
        0xEDC1,
        0xEC81,
        0x2C40,
        0xE401,
        0x24C0,
        0x2580,
        0xE541,
        0x2700,
        0xE7C1,
        0xE681,
        0x2640,
        0x2200,
        0xE2C1,
        0xE381,
        0x2340,
        0xE101,
        0x21C0,
        0x2080,
        0xE041,
        0xA001,
        0x60C0,
        0x6180,
        0xA141,
        0x6300,
        0xA3C1,
        0xA281,
        0x6240,
        0x6600,
        0xA6C1,
        0xA781,
        0x6740,
        0xA501,
        0x65C0,
        0x6480,
        0xA441,
        0x6C00,
        0xACC1,
        0xAD81,
        0x6D40,
        0xAF01,
        0x6FC0,
        0x6E80,
        0xAE41,
        0xAA01,
        0x6AC0,
        0x6B80,
        0xAB41,
        0x6900,
        0xA9C1,
        0xA881,
        0x6840,
        0x7800,
        0xB8C1,
        0xB981,
        0x7940,
        0xBB01,
        0x7BC0,
        0x7A80,
        0xBA41,
        0xBE01,
        0x7EC0,
        0x7F80,
        0xBF41,
        0x7D00,
        0xBDC1,
        0xBC81,
        0x7C40,
        0xB401,
        0x74C0,
        0x7580,
        0xB541,
        0x7700,
        0xB7C1,
        0xB681,
        0x7640,
        0x7200,
        0xB2C1,
        0xB381,
        0x7340,
        0xB101,
        0x71C0,
        0x7080,
        0xB041,
        0x5000,
        0x90C1,
        0x9181,
        0x5140,
        0x9301,
        0x53C0,
        0x5280,
        0x9241,
        0x9601,
        0x56C0,
        0x5780,
        0x9741,
        0x5500,
        0x95C1,
        0x9481,
        0x5440,
        0x9C01,
        0x5CC0,
        0x5D80,
        0x9D41,
        0x5F00,
        0x9FC1,
        0x9E81,
        0x5E40,
        0x5A00,
        0x9AC1,
        0x9B81,
        0x5B40,
        0x9901,
        0x59C0,
        0x5880,
        0x9841,
        0x8801,
        0x48C0,
        0x4980,
        0x8941,
        0x4B00,
        0x8BC1,
        0x8A81,
        0x4A40,
        0x4E00,
        0x8EC1,
        0x8F81,
        0x4F40,
        0x8D01,
        0x4DC0,
        0x4C80,
        0x8C41,
        0x4400,
        0x84C1,
        0x8581,
        0x4540,
        0x8701,
        0x47C0,
        0x4680,
        0x8641,
        0x8201,
        0x42C0,
        0x4380,
        0x8341,
        0x4100,
        0x81C1,
        0x8081,
        0x4040
    ];

    /// <summary>
    ///     Calculate CRC value for the packet
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte low, byte high) CalculateCrc(ReadOnlySpan<byte> packet) {
        ushort crc = 0xFFFF;
        // check the length
        foreach (var b in packet) {
            // get the index
            var index = (byte)(crc ^ b);
            // get the crc
            crc = (ushort)((crc >> 8) ^ ModbusCrc16Table[index]);
        }

        // result
        return ((byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF));
    }

    /// <summary>
    ///     Calculate to CRC value for the packet
    /// </summary>
    /// <param name="packet">packet</param>
    /// <param name="buffer">crc buffer</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CalculateCrcTo(ReadOnlySpan<byte> packet, Span<byte> buffer) {
        // check the buffer size
        if (buffer.Length < 2)
            // throw the exception
            throw new ArgumentException("Buffer must be at least 2 bytes", nameof(buffer));

        ushort crc = 0xFFFF;
        // check the length
        foreach (var b in packet) {
            // get the index
            var index = (byte)(crc ^ b);
            // get the crc
            crc = (ushort)((crc >> 8) ^ ModbusCrc16Table[index]);
        }

        // set the crc
        buffer[0] = (byte)(crc        & 0xFF);
        buffer[1] = (byte)((crc >> 8) & 0xFF);
    }

    /// <summary>
    ///     Validate the CRC value from the packet
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ValidateCrc(ReadOnlySpan<byte> packet) {
        // check the length
        if (packet.Length < 3)
            return false;
        // get the data span
        var data = packet[..^2];
        // get the received crc value
        var receivedLow  = packet[^2];
        var receivedHigh = packet[^1];
        // get the crc value
        var (low, high) = CalculateCrc(data);
        // result
        return receivedLow == low && receivedHigh == high;
    }

    /// <summary>
    ///     Calculate the check sum
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>sum</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateCheckSum(ReadOnlySpan<byte> packet) {
        var sum = 0;
        // check the length
        foreach (var value in packet)
            // add the sum
            sum += value;
        // return the sum value
        return sum;
    }

    /// <summary>
    ///     Calculate the check sum (fast)
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>sum</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateCheckSumFast(ReadOnlySpan<byte> span) {
        var sum = 0;

        // get the 4-byte values
        var intSpan = MemoryMarshal.Cast<byte, int>(span);
        // check the int span
        foreach (var intVal in intSpan)
            // add the value
            sum += (intVal & 0xFF) + ((intVal >> 8)  & 0xFF) +
                   ((intVal                   >> 16) & 0xFF) + ((intVal >> 24) & 0xFF);

        // get the remain length
        var remaining = span.Length % 4;
        // check the remain length
        if (remaining < 1)
            // return the sum
            return sum;
        // get the offset
        var offset = span.Length - remaining;
        // check the length
        for (var i = 0; i < remaining; i++)
            // add the value
            sum += span[offset + i];
        // return the sum
        return sum;
    }

    /// <summary>
    ///     Convert to unit for torque
    /// </summary>
    /// <param name="value">torque</param>
    /// <param name="src">source unit</param>
    /// <param name="dst">destination unit</param>
    /// <returns>converted torque</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertTorqueUnit(float value, UnitTypes src, UnitTypes dst) {
        // const value for unit N.m
        const float nCm   = 100.0f;
        const float kgfM  = 0.101971621f;
        const float kgfCm = 10.1971621f;
        const float lbfIn = 8.85074579f;
        const float lbfFt = 0.737562149f;
        const float ozfIn = 141.611932f;
        // convert source to N.m
        var valueInNm = src switch {
            UnitTypes.KgfCm => value / kgfCm,
            UnitTypes.KgfM  => value / kgfM,
            UnitTypes.Nm    => value,
            UnitTypes.NCm   => value / nCm,
            UnitTypes.LbfIn => value / lbfIn,
            UnitTypes.OzfIn => value / ozfIn,
            UnitTypes.LbfFt => value / lbfFt,
            _               => value
        };
        // convert N.m to destination
        return dst switch {
            UnitTypes.KgfCm => valueInNm * kgfCm,
            UnitTypes.KgfM  => valueInNm * kgfM,
            UnitTypes.Nm    => valueInNm,
            UnitTypes.NCm   => valueInNm * nCm,
            UnitTypes.LbfIn => valueInNm * lbfIn,
            UnitTypes.OzfIn => valueInNm * ozfIn,
            UnitTypes.LbfFt => valueInNm * lbfFt,
            _               => valueInNm
        };
    }

    /// <summary>
    ///     Convert to unit from string
    /// </summary>
    /// <param name="value">value</param>
    /// <returns>unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnitTypes ParseToUnit(string value) {
        // check null or empty
        if (string.IsNullOrEmpty(value))
            return UnitTypes.KgfCm;
        // convert to unit
        return value.ToLowerInvariant() switch {
            "kgf.cm" => UnitTypes.KgfCm,
            "kgf.m"  => UnitTypes.KgfM,
            "n.m"    => UnitTypes.Nm,
            "n.cm"   => UnitTypes.NCm,
            "lbf.in" => UnitTypes.LbfIn,
            "ozf.in" => UnitTypes.OzfIn,
            "lbf.ft" => UnitTypes.LbfFt,
            _        => UnitTypes.KgfCm
        };
    }

    /// <summary>
    ///     Convert unit type to string representation
    /// </summary>
    /// <param name="value">unit type as integer (0=kgf.cm, 1=kgf.m, ...)</param>
    /// <returns>unit string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseToUnit(int value) {
        return value switch {
            0 => "kgf.cm",
            1 => "kgf.m",
            2 => "N.m",
            3 => "N.cm",
            4 => "ozf.in",
            5 => "lbf.ft",
            6 => "ozf.ft",
            _ => "kgf.cm"
        };
    }

    /// <summary>
    ///     Get the current tick
    /// </summary>
    /// <returns></returns>
    public static long GetCurrentTicks() {
        return Environment.TickCount64;
    }

    /// <summary>
    ///     Get the time laps for milliseconds
    /// </summary>
    /// <param name="from">from time</param>
    /// <returns>total milliseconds</returns>
    public static long TimeLapsMs(DateTime from) {
        return (long)(DateTime.Now - from).TotalMilliseconds;
    }

    /// <summary>
    ///     Get the time laps for seconds
    /// </summary>
    /// <param name="from">from time</param>
    /// <returns>total seconds</returns>
    public static long TimeLapsSec(DateTime from) {
        return (long)(DateTime.Now - from).TotalSeconds;
    }

    /// <summary>
    ///     Get the time laps for diff for seconds
    /// </summary>
    /// <param name="src">source</param>
    /// <param name="dst">destination</param>
    /// <returns>second</returns>
    public static ulong TimeLapsDifference(DateTime src, DateTime dst) {
        // get time laps
        return (ulong)Math.Abs((long)(src - dst).TotalSeconds);
    }

    /// <summary>
    ///     Set value for single type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">float value</param>
    /// <param name="wordOrder">word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(ReadOnlySpan<byte> values, out float value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // check the length
        if (values.Length < 4) {
            // reset the value
            value = 0;
        } else {
            // get the int value based on word order
            var intValue = wordOrder == WordOrderTypes.HighLow
                // ABCD: High word first (most common)
                ? (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3]
                // CDAB: Low word first
                : (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
            // set the value
            value = BitConverter.Int32BitsToSingle(intValue);
        }
    }

    /// <summary>
    ///     Set value for ushort type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">ushort value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(ReadOnlySpan<byte> values, out ushort value) {
        // set the value
        value = values.Length >= 2 ? (ushort)((values[0] << 8) | values[1]) : (ushort)0;
    }

    /// <summary>
    ///     Set value for int type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">int value</param>
    /// <param name="wordOrder">word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(ReadOnlySpan<byte> values, out int value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // set the default value
        value = 0;
        // check the length
        if (values.Length != 4)
            return;
        // check the word order
        if (wordOrder == WordOrderTypes.HighLow)
            // ABCD: High word first (most common)
            value = (values[0] << 24) | (values[1] << 16) | (values[2] << 8) | values[3];
        else
            // CDAB: Low word first
            value = (values[2] << 24) | (values[3] << 16) | (values[0] << 8) | values[1];
    }

    /// <summary>
    ///     Convert byte span to hex string
    /// </summary>
    /// <param name="values">byte span</param>
    /// <param name="separator">separator (default: space)</param>
    /// <param name="lineBreakAt">line break position (0 = no break)</param>
    /// <returns>hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ConvertHexString(ReadOnlySpan<byte> values, string separator = " ", int lineBreakAt = 16) {
        // check the empty
        if (values.IsEmpty)
            // return the empty hex string
            return string.Empty;

        // create the string builder
        var sb = new StringBuilder(values.Length * 3);
        // check the length
        for (var i = 0; i < values.Length; i++) {
            // append hex string
            sb.Append(values[i].ToString("X2"));
            // check the index
            if (i < values.Length - 1)
                // append the separator
                sb.Append(separator);
            // check the line break
            if (lineBreakAt > 0 && (i + 1) % lineBreakAt == 0)
                // append the line break
                sb.AppendLine();
        }

        // return the hex string
        return sb.ToString();
    }

    /// <summary>
    ///     Set value for single type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">float value</param>
    /// <param name="wordOrder">word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(byte[] values, out float value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        ConvertValue(values.AsSpan(), out value, wordOrder);
    }

    /// <summary>
    ///     Set value for ushort type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">ushort value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(byte[] values, out ushort value) {
        ConvertValue(values.AsSpan(), out value);
    }

    /// <summary>
    ///     Set value for int type
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="value">int value</param>
    /// <param name="wordOrder">word order for 32-bit value (default: HighLow/ABCD)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertValue(byte[] values, out int value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        ConvertValue(values.AsSpan(), out value, wordOrder);
    }

    /// <summary>
    ///     Convert byte span to hex string
    /// </summary>
    /// <param name="values">byte span</param>
    /// <param name="separator">separator (default: space)</param>
    /// <param name="lineBreakAt">line break position (0 = no break)</param>
    /// <returns>hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ConvertHexString(byte[] values, string separator = " ", int lineBreakAt = 16) {
        return ConvertHexString(values.AsSpan(), separator, lineBreakAt);
    }

    /// <summary>
    ///     Get ushort values from text
    /// </summary>
    /// <param name="text">value</param>
    /// <returns>values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetWordValuesFromText(string text) {
        // check the text
        if (string.IsNullOrEmpty(text))
            return [];

        // get the text span
        var span = text.AsSpan();
        // get the length
        var length = span.Length / 2;
        // get the remain length
        var remain = span.Length % 2;
        // create the result array
        var result = new ushort[length + remain];
        // check array length
        for (var i = 0; i < length; i++)
            // set the value
            result[i] = (ushort)((span[i * 2] << 8) | span[i * 2 + 1]);
        // check remain offset
        if (remain > 0)
            // set the value
            result[length] = (ushort)(span[^1] << 8);
        // result
        return result;
    }

    /// <summary>
    ///     Get ushort values from text
    /// </summary>
    /// <param name="text">value</param>
    /// <returns>values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetIntValuesFromText(string text) {
        // check the text
        if (string.IsNullOrEmpty(text))
            return [];
        // get the length
        var len = text.Length >> 1;
        // create the space
        var result = new ushort[len];
        // get the span
        var span = text.AsSpan();
        // check the length
        for (int i = 0, j = 0; i < len; i++, j += 2) {
            // get the data
            var d1 = span[j]     - '0';
            var d2 = span[j + 1] - '0';
            // set the result
            result[i] = (ushort)(d1 * 10 + d2);
        }

        // return the values
        return result;
    }

    /// <summary>
    ///     Get ushort values from network address
    /// </summary>
    /// <param name="addr">address</param>
    /// <returns>values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetValuesFromAddr(string addr) {
        var count = 0;
        var acc   = 0;
        // get the span
        var span = addr.AsSpan();
        // allocate the space
        Span<ushort> values = stackalloc ushort[8];
        // check the length
        for (var i = 0; i <= span.Length; i++)
            // check the length and comma
            if (i == span.Length || span[i] == '.') {
                // set the value
                values[count++] = (ushort)acc;
                // reset the acc
                acc = 0;
            } else {
                // set the acc
                acc = acc * 10 + (span[i] - '0');
            }

        // create the space
        var result = new ushort[count];
        // copy the data
        values[..count].CopyTo(result);
        // return the data
        return result;
    }

    /// <summary>
    ///     Get ushort values from int32 value
    /// </summary>
    /// <param name="value">int value</param>
    /// <param name="wordOrder">word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetValuesFromValue(int value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // get high and low words
        var highWord = (ushort)(value >> 16);
        var lowWord  = (ushort)value;
        // return based on word order
        return wordOrder == WordOrderTypes.HighLow
            ? [highWord, lowWord]  // ABCD: High word first
            : [lowWord, highWord]; // CDAB: Low word first
    }

    /// <summary>
    ///     Get ushort values from single value
    /// </summary>
    /// <param name="value">single value</param>
    /// <param name="wordOrder">word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] GetValuesFromValue(float value, WordOrderTypes wordOrder = WordOrderTypes.HighLow) {
        // get the bits
        var bits = BitConverter.SingleToInt32Bits(value);
        // get high and low words
        var highWord = (ushort)(bits >> 16);
        var lowWord  = (ushort)bits;
        // return based on word order
        return wordOrder == WordOrderTypes.HighLow
            ? [highWord, lowWord]  // ABCD: High word first
            : [lowWord, highWord]; // CDAB: Low word first
    }

    /// <summary>
    ///     Swap the item
    /// </summary>
    /// <param name="list">list</param>
    /// <param name="source">source</param>
    /// <param name="dest">destination</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    ///     Convert bytes to ASCII string, trimming trailing null characters
    /// </summary>
    /// <param name="span">span</param>
    /// <returns>string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToAsciiTrimEnd(ReadOnlySpan<byte> span) {
        // get the length
        var end = span.Length;
        // check the null
        while (end > 0 && span[end - 1] == 0)
            // trim the null
            end--;
        // return the string
        return end == 0 ? string.Empty : Encoding.ASCII.GetString(span[..end]);
    }
}