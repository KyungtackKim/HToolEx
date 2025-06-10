using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format network setting class
/// </summary>
public class FormatSetNetwork {
    /// <summary>
    ///     Operation setting size each version
    /// </summary>
    [PublicAPI]
    public static readonly int[] Size = [69, 70];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetNetwork() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetNetwork(byte[] values, int revision = 0) {
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
        Ssid = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        Password = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        BandBy5G = Convert.ToInt32(bin.ReadByte());
        CountryType = Convert.ToInt32(bin.ReadByte());
        ManualChannel = Convert.ToInt32(bin.ReadByte());
        Channel = Convert.ToInt32(bin.ReadUInt16());
        // check revision.1 information
        if (revision < 1)
            return;
        // get revision.1 information
        UsedDhcp = Convert.ToInt32(bin.ReadByte());
    }

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
        var ssid = Encoding.ASCII.GetBytes(Ssid).ToList();
        var password = Encoding.ASCII.GetBytes(Password).ToList();
        // string length offset
        ssid.AddRange(new byte[32 - Ssid.Length]);
        password.AddRange(new byte[32 - Password.Length]);
        // get revision.0 values
        values.AddRange(ssid);
        values.AddRange(password);
        values.Add(Convert.ToByte(BandBy5G));
        values.Add(Convert.ToByte(CountryType));
        values.Add(Convert.ToByte(ManualChannel));
        values.Add(Convert.ToByte((Channel >> 8) & 0xFF));
        values.Add(Convert.ToByte(Channel & 0xFF));
        // check revision.1
        if (revision < 1)
            // values
            return values.ToArray();
        // get revision.1 values
        values.Add(Convert.ToByte(UsedDhcp));
        // values
        return values.ToArray();
    }

    #region REVISION

    #region REV.0

    /// <summary>
    ///     AP SSID
    /// </summary>
    [PublicAPI]
    public string Ssid { get; set; } = default!;

    /// <summary>
    ///     AP Password
    /// </summary>
    [PublicAPI]
    public string Password { get; set; } = default!;

    /// <summary>
    ///     Band
    /// </summary>
    [PublicAPI]
    public int BandBy5G { get; set; }

    /// <summary>
    ///     Country type
    /// </summary>
    [PublicAPI]
    public int CountryType { get; set; }

    /// <summary>
    ///     Channel selection
    /// </summary>
    [PublicAPI]
    public int ManualChannel { get; set; }

    /// <summary>
    ///     Manual channel number
    /// </summary>
    [PublicAPI]
    public int Channel { get; set; }

    #endregion

    #region REV.1

    public int UsedDhcp { get; set; }

    #endregion

    #endregion
}