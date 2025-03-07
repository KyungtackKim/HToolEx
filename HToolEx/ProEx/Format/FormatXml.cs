using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format XML data class
/// </summary>
public class FormatXml {
    /// <summary>
    ///     Operation setting size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [20];

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatXml(byte[] values, int revision = 0) {
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
        // get revision.0 values
        Version = Encoding.ASCII.GetString(bin.ReadBytes(8)).TrimEnd('\0');
        ReleaseDate = Encoding.ASCII.GetString(bin.ReadBytes(12)).TrimEnd('\0');
    }

    /// <summary>
    ///     XML version
    /// </summary>
    [PublicAPI]
    public string Version { get; set; } = default!;

    /// <summary>
    ///     XML release date
    /// </summary>
    [PublicAPI]
    public string ReleaseDate { get; set; } = default!;

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }
}