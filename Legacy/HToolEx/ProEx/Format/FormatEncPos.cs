using System.ComponentModel;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format encoder position class
/// </summary>
[PublicAPI]
public class FormatEncPos {
    /// <summary>
    ///     Encoder setting size each version
    /// </summary>
    public static readonly int[] Size = [32];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatEncPos() {
        // create default values
        Values       = [0, 0, 0, 0];
        ValuesOfPure = [0, 0, 0, 0];
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatEncPos(byte[] values, int revision = 0) : this() {
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

        // check channel
        for (var i = 0; i < 4; i++)
            // set with zero position encoder values
            Values[i] = bin.ReadInt32();
        // check channel
        for (var i = 0; i < 4; i++)
            // set without zero position encoder values
            ValuesOfPure[i] = bin.ReadUInt32();
    }

    /// <summary>
    ///     Values with zero position
    /// </summary>
    public int[] Values { get; }

    /// <summary>
    ///     Values without zero position
    /// </summary>
    public uint[] ValuesOfPure { get; }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; set; }
}