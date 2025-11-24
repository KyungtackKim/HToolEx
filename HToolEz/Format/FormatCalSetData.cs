using HToolEz.Type;
using HToolEz.Util;

namespace HToolEz.Format;

/// <summary>
///     Format device calibration set data class
/// </summary>
public sealed class FormatCalSetData {
    /// <summary>
    ///     Format device calibration set data size
    /// </summary>
    private static readonly int[] Size = [4, 6];
    private readonly BodyTypes _body;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="data">data packet</param>
    /// <param name="body">data body type</param>
    /// <exception cref="ArgumentException">Thrown when data size is not 4 or 6 bytes</exception>
    public FormatCalSetData(ReadOnlyMemory<byte> data, BodyTypes body = BodyTypes.Integrated) {
        // get the data size (only valid data)
        var size = data.Length;
        // check the data size
        if (!Size.Contains(size))
            throw new ArgumentException($"Invalid calibration data size: {size} bytes. Expected size: {string.Join(" or ", Size)} bytes.",
                nameof(data));
        // get the data
        var d = data.Span;
        // set the body type
        _body = body;
        /*      common information      */
        // check the point type
        if (Utils.IsKnownItem<CalPointModeTypes>(d[0]))
            // set the point type
            Point = (CalPointModeTypes)d[0];
        // set the index
        Index = d[1];
        /*      value information       */
        // check the body type
        if (body == BodyTypes.Separated) {
            // convert value
            Utils.ConvertValue(d[2..], out int value);
            // set the value
            Value = value;
        } else {
            // convert value
            Utils.ConvertValue(d[2..], out ushort value);
            // set the value
            Value = value;
        }
    }

    /// <summary>
    ///     Calibration point type
    /// </summary>
    public CalPointModeTypes Point { get; }

    /// <summary>
    ///     Calibration point index
    /// </summary>
    public byte Index { get; }

    /// <summary>
    ///     Calibration ADC value
    /// </summary>
    public int Value { get; }

    /// <summary>
    ///     Convert calibration set data to byte array
    /// </summary>
    /// <returns>byte array (4 bytes for Integrated, 6 bytes for Separated)</returns>
    public byte[] ToBytes(bool withoutValue = false) {
        // determine size based on body type
        var size  = (_body == BodyTypes.Separated ? Size[1] : Size[0]) - (withoutValue ? _body == BodyTypes.Separated ? 4 : 2 : 0);
        var bytes = new byte[size];

        // set point type and index
        bytes[0] = (byte)Point;
        bytes[1] = Index;

        // check the without value option
        if (withoutValue)
            return bytes;
        // set value based on body type
        switch (_body) {
            case BodyTypes.Separated:
                // 4 bytes (int32, big-endian)
                Utils.WriteValue(bytes, 2, Value);
                break;

            case BodyTypes.Integrated:
            default:
                // 2 bytes (ushort, big-endian)
                Utils.WriteValue(bytes, 2, (ushort)Value);
                break;
        }

        return bytes;
    }
}