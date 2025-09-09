using HTool.Type;
using HTool.Util;
using JetBrains.Annotations;

namespace HTool.Format;

/// <summary>
///     Graph data format class
/// </summary>
[PublicAPI]
public class FormatGraph {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatGraph(byte[] values, GenerationTypes type = GenerationTypes.GenRev1) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);
        // set generation type
        Type = type;
        // check generation type rev.2
        if (type != GenerationTypes.GenRev2)
            return;
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
    ///     Generation type
    /// </summary>
    public GenerationTypes Type { get; private set; }

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

    public float[] Values { get; } = null!;

    /// <summary>
    ///     Check sum
    /// </summary>

    public int CheckSum { get; }

    /// <summary>
    ///     Format size
    /// </summary>

    public static int Size => 4;
}