using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format network setting class
/// </summary>
public class FormatSetSound {
    /// <summary>
    ///     Operation setting size each version
    /// </summary>
    [PublicAPI]
    public static readonly int[] Size = [512];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetSound() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetSound(byte[] values, int revision = 0) {
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
        OkPath = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        NgPath = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        EtcPath = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        TapPath = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
    }

    /// <summary>
    ///     OK file path
    /// </summary>
    [PublicAPI]
    public string OkPath { get; set; } = default!;

    /// <summary>
    ///     NG file path
    /// </summary>
    [PublicAPI]
    public string NgPath { get; set; } = default!;

    /// <summary>
    ///     ETC file path
    /// </summary>
    [PublicAPI]
    public string EtcPath { get; set; } = default!;

    /// <summary>
    ///     TAP file path
    /// </summary>
    [PublicAPI]
    public string TapPath { get; set; } = default!;

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }

    /// <summary>
    ///     Get values
    /// </summary>
    /// <param name="revision">revision</param>
    /// <returns>values</returns>
    [PublicAPI]
    public byte[] GetValues(int revision = 0) {
        var values = new List<byte>();
        // get string values
        var ok = Encoding.ASCII.GetBytes(OkPath).ToList();
        var ng = Encoding.ASCII.GetBytes(NgPath).ToList();
        var etc = Encoding.ASCII.GetBytes(EtcPath).ToList();
        var tap = Encoding.ASCII.GetBytes(TapPath).ToList();
        // string length offset
        ok.AddRange(new byte[128 - OkPath.Length]);
        ng.AddRange(new byte[128 - NgPath.Length]);
        etc.AddRange(new byte[128 - EtcPath.Length]);
        tap.AddRange(new byte[128 - TapPath.Length]);
        // get revision.0 values
        values.AddRange(ok.ToArray());
        values.AddRange(ng.ToArray());
        values.AddRange(etc.ToArray());
        values.AddRange(tap.ToArray());
        // values
        return values.ToArray();
    }
}