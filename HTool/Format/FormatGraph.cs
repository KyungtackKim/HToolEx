using System.Buffers.Binary;
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
        Channel = BinaryPrimitives.ReadUInt16BigEndian(span);
        Count   = BinaryPrimitives.ReadUInt16BigEndian(span[2..]);
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
        for (int i = 0, offset = 0; i < Count; i++, offset += 4) {
            // get the value
            var value = BinaryPrimitives.ReadUInt32BigEndian(s[offset..]);
            // set the value
            Values[i] = BitConverter.Int32BitsToSingle((int)value);
        }

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