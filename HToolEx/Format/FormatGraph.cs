using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.Format;

/// <summary>
///     Graph data format class
/// </summary>
[PublicAPI]
public class FormatGraph {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatGraph(byte[] values) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);
        // get channel no.
        Channel = bin.ReadUInt16();
        // get values count
        Count = bin.ReadUInt16();
        // create values
        Values = new float[Count];
        // check count
        for (var i = 0; i < Count; i++)
            // read single
            Values[i] = bin.ReadSingle();
        // get check sum
        CheckSum = values.Sum(v => v);
        // throw an error if not all data has been read
        if (bin.BaseStream.Position != bin.BaseStream.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{bin.BaseStream.Length - bin.BaseStream.Position} byte(s) remain");
    }

    /// <summary>
    ///     Channel number
    /// </summary>

    public int Channel { get; }

    /// <summary>
    ///     Channel point count
    /// </summary>

    public int Count { get; }

    /// <summary>
    ///     Values
    /// </summary>

    public float[] Values { get; }

    /// <summary>
    ///     Check sum
    /// </summary>

    public int CheckSum { get; }

    /// <summary>
    ///     Format size
    /// </summary>

    public static int Size => 4;
}