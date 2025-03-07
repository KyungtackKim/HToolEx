using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format barcode setting class
/// </summary>
public class FormatSetBarcode {
    /// <summary>
    ///     Operation setting size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [130];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetBarcode() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetBarcode(byte[] values, int revision = 0) {
        // check revision
        if (revision >= Size.Length)
            // reset revision
            revision = 0;
        // check size
        if (values.Length < Size[revision])
            return;

        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set check sum
        CheckSum = values.Sum(x => x);
        // get revision.0 information
        Tool = Convert.ToInt32(bin.ReadByte());
        Enable = Convert.ToInt32(bin.ReadByte());
        FilePath = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
    }

    /// <summary>
    ///     Tool index
    /// </summary>
    [PublicAPI]
    public int Tool { get; set; }

    /// <summary>
    ///     Barcode enable
    /// </summary>
    [PublicAPI]
    public int Enable { get; set; }

    /// <summary>
    ///     Barcode file path
    /// </summary>
    [PublicAPI]
    public string FilePath { get; set; } = default!;

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }
}