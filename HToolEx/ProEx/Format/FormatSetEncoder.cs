using System.ComponentModel;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format encoder setting class
/// </summary>
[PublicAPI]
public class FormatSetEncoder {
    /// <summary>
    ///     Encoder setting size each version
    /// </summary>
    public static readonly int[] Size = [128];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetEncoder() {
        // check channel count
        for (var i = 0; i < 4; i++) {
            // add use and default tolerance
            UseAndTolerance.Add(i, (0, 200, 50));
            // add default zero position
            ZeroPos.Add(i, 0);
            // add default rest position
            RestPos.Add(i, 0);
        }

        // check feeder count
        for (var i = 0; i < 2; i++) {
            // add default feeder
            Feeder.Add(i, new Dictionary<int, (int use, int ch1, int ch2)>());
            // check corner count
            for (var j = 0; j < 2; j++)
                // add default corner
                Feeder[i].Add(j, (0, 0, 0));
        }
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetEncoder(byte[] values, int revision = 0) : this() {
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

        // check channel count
        for (var i = 0; i < 4; i++)
            // set values
            UseAndTolerance[i] = (bin.ReadInt32(), bin.ReadInt32(), bin.ReadInt32());
        // check feeder count
        for (var i = 0; i < 2; i++)
            // check corner count
        for (var j = 0; j < 2; j++)
            // set values
            Feeder[i][j] = (bin.ReadInt32(), bin.ReadInt32(), bin.ReadInt32());

        // check channel count
        for (var i = 0; i < 4; i++)
            // set values
            ZeroPos[i] = bin.ReadUInt32();
        // check channel count
        for (var i = 0; i < 4; i++)
            // set values
            RestPos[i] = bin.ReadInt32();
    }

    /// <summary>
    ///     Use and tolerance by channel
    /// </summary>
    public Dictionary<int, (int use, int zone, int ok)> UseAndTolerance { get; } = new();

    /// <summary>
    ///     Feeder pick-up position
    /// </summary>
    public Dictionary<int, Dictionary<int, (int use, int ch1, int ch2)>> Feeder { get; } = new();

    /// <summary>
    ///     Zero position
    /// </summary>
    public Dictionary<int, uint> ZeroPos { get; } = new();

    /// <summary>
    ///     Rest position
    /// </summary>
    public Dictionary<int, int> RestPos { get; } = new();

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; set; }

    /// <summary>
    ///     Get values
    /// </summary>
    /// <param name="revision">revision</param>
    /// <returns>values</returns>
    public byte[] GetValues(int revision = 0) {
        var values = new List<byte>();
        // check channel count
        for (var i = 0; i < 4; i++) {
            // add values for use and tolerance
            values.Add(0x00);
            values.Add(0x00);
            values.Add(0x00);
            values.Add(Convert.ToByte(UseAndTolerance[i].use));
            values.Add(Convert.ToByte((UseAndTolerance[i].zone >> 24) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].zone >> 16) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].zone >> 8) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].zone >> 0) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].ok >> 24) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].ok >> 16) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].ok >> 8) & 0xFF));
            values.Add(Convert.ToByte((UseAndTolerance[i].ok >> 0) & 0xFF));
        }

        // check feeder count
        for (var i = 0; i < 2; i++) {
            // get feeder item
            var feeder = Feeder[i];
            // check corner count
            for (var j = 0; j < 2; j++) {
                // add values for feeder's corner
                values.Add(0x00);
                values.Add(0x00);
                values.Add(0x00);
                values.Add(Convert.ToByte(feeder[j].use));
                values.Add(Convert.ToByte((feeder[j].ch1 >> 24) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch1 >> 16) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch1 >> 8) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch1 >> 0) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch2 >> 24) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch2 >> 16) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch2 >> 8) & 0xFF));
                values.Add(Convert.ToByte((feeder[j].ch2 >> 0) & 0xFF));
            }
        }

        // check channel count
        for (var i = 0; i < 4; i++) {
            // add values for zero position
            values.Add(Convert.ToByte((ZeroPos[i] >> 24) & 0xFF));
            values.Add(Convert.ToByte((ZeroPos[i] >> 16) & 0xFF));
            values.Add(Convert.ToByte((ZeroPos[i] >> 8) & 0xFF));
            values.Add(Convert.ToByte((ZeroPos[i] >> 0) & 0xFF));
        }

        // check channel count
        for (var i = 0; i < 4; i++) {
            // add values for rest position
            values.Add(Convert.ToByte((RestPos[i] >> 24) & 0xFF));
            values.Add(Convert.ToByte((RestPos[i] >> 16) & 0xFF));
            values.Add(Convert.ToByte((RestPos[i] >> 8) & 0xFF));
            values.Add(Convert.ToByte((RestPos[i] >> 0) & 0xFF));
        }

        return values.ToArray();
    }
}