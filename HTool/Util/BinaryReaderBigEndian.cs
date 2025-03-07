using JetBrains.Annotations;

namespace HTool.Util;

/// <summary>
///     This class extends BinaryReader to read data in Big-Endian byte order.
/// </summary>
/// <param name="input">stream</param>
[PublicAPI]
public class BinaryReaderBigEndian(Stream input) : BinaryReader(input) {
    // The constructor takes a Stream object as input.
    // This helper method converts a portion of a byte array to an indicated type in Big-Endian order.
    private static T ToBigEndian<T>(Func<byte[], int, T> converter, byte[] value, int startIndex, int bytesToReverse) {
        // Check little-endian
        if (!BitConverter.IsLittleEndian)
            // On Big-Endian system, no need to reverse, directly convert the byte portion.
            return converter(value, startIndex);
        // When running on a little-endian system, reverse the byte portion before converting.
        var temp = new byte[bytesToReverse];
        // Check byte count
        for (var i = 0; i < bytesToReverse; i++)
            // Set byte value
            temp[i] = value[startIndex + bytesToReverse - 1 - i];
        // Return Big-Endian values
        return converter(temp, 0);
    }

    // Overridden methods to read data in Big-Endian order.

    /// <summary>
    ///     Reads an Int16 in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override short ReadInt16() {
        return ToBigEndian(BitConverter.ToInt16, base.ReadBytes(2), 0, 2);
    }

    /// <summary>
    ///     Reads a UInt16 in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override ushort ReadUInt16() {
        return ToBigEndian(BitConverter.ToUInt16, base.ReadBytes(2), 0, 2);
    }

    /// <summary>
    ///     Reads an Int32 in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override int ReadInt32() {
        return ToBigEndian(BitConverter.ToInt32, base.ReadBytes(4), 0, 4);
    }

    /// <summary>
    ///     Reads a UInt32 in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override uint ReadUInt32() {
        return ToBigEndian(BitConverter.ToUInt32, base.ReadBytes(4), 0, 4);
    }

    /// <summary>
    ///     Reads a UInt64 in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override ulong ReadUInt64() {
        return ToBigEndian(BitConverter.ToUInt64, base.ReadBytes(8), 0, 8);
    }

    /// <summary>
    ///     Reads a Single (float) in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override float ReadSingle() {
        return ToBigEndian(BitConverter.ToSingle, base.ReadBytes(4), 0, 4);
    }

    /// <summary>
    ///     Reads a Double (double) in Big-Endian.
    /// </summary>
    /// <returns>values</returns>
    public override double ReadDouble() {
        return ToBigEndian(BitConverter.ToDouble, ReadBytes(8), 0, 8);
    }
}