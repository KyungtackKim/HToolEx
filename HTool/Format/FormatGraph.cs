using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     Graph data format class
/// </summary>
public sealed class FormatGraph {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatGraph(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        // set generation type
        Type = type;
        // check generation type
        if (type != GenerationTypes.GenRev2)
            return;
        // get the span
        var span = values.AsSpan();
        // check the length
        if (span.Length < 4)
            throw new InvalidDataException("Too short.");
        // get the information
        Channel = BinarySpanReader.ReadUInt16(span);
        Count   = BinarySpanReader.ReadUInt16(span[2..]);
        // get the information
        var payload  = checked(Count * 4);
        var expected = 4 + payload;
        // check the length
        if (span.Length != expected)
            // throw exception
            throw new InvalidDataException($"Invalid length: got {span.Length}, expected {expected}.");

        // get the data values span
        var s = span[4..];
        // values
        Values = GC.AllocateUninitializedArray<float>(Count);
        // check the count
        for (int i = 0, offset = 0; i < Count; i++, offset += 4)
            // set the value
            Values[i] = BinarySpanReader.ReadSingle(s[offset..]);

        // set the checksum
        CheckSum = Utils.CalculateCheckSum(span);
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
    ///     Graph data values
    /// </summary>
    public float[] Values { get; } = null!;

    /// <summary>
    ///     Check sum
    /// </summary>
    public int CheckSum { get; }

    /// <summary>
    ///     Format header size
    /// </summary>
    public static int Size => 4;
}