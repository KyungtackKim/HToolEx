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

#if NOT_USE
    /// <summary>
    ///     Constructor by ParaMon-Pro X format
    /// </summary>
    /// <param name="channel">channel</param>
    /// <param name="src">source</param>
    public FormatGraph(int channel, FormatData src) {
        // set channel
        Channel = channel;
        // check channel
        switch (channel) {
            case 0:
                // set count
                Count = src.CountOfChannel1;
                // create values
                Values = new float[Count];
                // check count
                for (var i = 0; i < Count; i++)
                    // set value
                    Values[i] = src.ValueOfChannel1[i];
                break;
            case 1:
                // set count
                Count = src.CountOfChannel2;
                // create values
                Values = new float[Count];
                // check count
                for (var i = 0; i < Count; i++)
                    // set value
                    Values[i] = src.ValueOfChannel2[i];
                break;
        }

        // get check sum
        CheckSum = src.CheckSum;
    }
#endif

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