using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.Format;

/// <summary>
///     Barcode data format class
/// </summary>
[PublicAPI]
public class FormatBarcode {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatBarcode() {
        // reset default value
        Preset = 1;
        Barcode = string.Empty;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatBarcode(byte[] values) : this() {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // preset/model
        Preset = bin.ReadUInt16();
        // mask
        Mask = bin.ReadUInt64();
        // reserved
        bin.ReadBytes(6);
        // read barcode
        Barcode = Encoding.ASCII.GetString(bin.ReadBytes(64)).TrimEnd('\0');
    }

    /// <summary>
    ///     Format size
    /// </summary>

    public static int Size => 80;

    /// <summary>
    ///     Barcode index
    /// </summary>

    public int Index { get; set; }

    /// <summary>
    ///     Preset/Model
    /// </summary>

    public int Preset { get; set; }

    /// <summary>
    ///     Mask
    /// </summary>

    public ulong Mask { get; set; }

    /// <summary>
    ///     Barcode
    /// </summary>

    public string Barcode { get; set; }

    /// <summary>
    ///     Get values
    /// </summary>
    /// <returns></returns>
    public ushort[] GetValues() {
        var values = new ushort[Size / 2];
        // set preset/model
        values[0] = (ushort)Preset;
        // get mask byte values
        var mask = BitConverter.GetBytes(Mask).Reverse().ToArray();
        // check mask value size
        for (var i = 0; i < mask.Length / 2; i++)
            // set mask value
            values[1 + i] = (ushort)((mask[i * 2] << 8) | mask[i * 2 + 1]);
        // get barcode byte values
        var barcode = Encoding.ASCII.GetBytes(Barcode);
        // check barcode value size
        for (var i = 0; i < barcode.Length / 2; i++)
            // set barcode value
            values[8 + i] = (ushort)((barcode[i * 2] << 8) | barcode[i * 2 + 1]);
        // check barcode value offset
        if (barcode.Length % 2 > 0)
            // set barcode value
            values[8 + barcode.Length / 2] = (ushort)(barcode[^1] << 8);

        // result
        return values;
    }
}