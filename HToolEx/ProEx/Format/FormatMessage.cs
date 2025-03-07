using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Message format class
/// </summary>
public class FormatMessage {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatMessage(byte[] values) {
        Header = new FormatMessageInfo(values);
        Values = new List<byte>(values.Skip(FormatMessageInfo.Size)).ToArray();
    }

    /// <summary>
    ///     Message header
    /// </summary>
    [PublicAPI]
    public FormatMessageInfo? Header { get; set; }

    /// <summary>
    ///     Message data values
    /// </summary>
    [PublicAPI]
    public byte[] Values { get; set; }

    /// <summary>
    ///     Get message values
    /// </summary>
    /// <returns>values</returns>
    [PublicAPI]
    public byte[] GetValues() {
        // check object
        if (Header == null)
            return Array.Empty<byte>();

        // get header values
        var values = new List<byte> {
            (byte)((Header.Length >> 8) & 0xFF),
            (byte)(Header.Length & 0xFF),
            (byte)(((int)Header.Id >> 8) & 0xFF),
            (byte)((int)Header.Id & 0xFF),
            (byte)((Header.Revision >> 8) & 0xFF),
            (byte)(Header.Revision & 0xFF)
        };
        // get reserved
        values.AddRange(new byte[FormatMessageInfo.Size - 6]);
        // get values
        values.AddRange(Values);
        // result
        return values.ToArray();
    }
}