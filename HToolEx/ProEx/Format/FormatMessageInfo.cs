using HToolEx.ProEx.Type;
using HToolEx.Util;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Header format class in Message
/// </summary>
public class FormatMessageInfo {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="length">frame length</param>
    /// <param name="id">id</param>
    /// <param name="revision">revision</param>
    public FormatMessageInfo(int length, MessageIdTypes id, int revision = 0) {
        // set information
        Length   = length;
        Id       = id;
        Revision = revision;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatMessageInfo(byte[] values) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set frame length
        Length = bin.ReadUInt16();

        // get message id
        var id = bin.ReadUInt16();
        // check defined id
        if (Enum.IsDefined(typeof(MessageIdTypes), (int)id))
            // set id
            Id = (MessageIdTypes)id;

        // set revision
        Revision = bin.ReadUInt16();
    }

    /// <summary>
    ///     Header size
    /// </summary>
    public static int Size => 16;

    /// <summary>
    ///     Frame length
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    ///     Message id
    /// </summary>
    public MessageIdTypes Id { get; private set; } = MessageIdTypes.None;

    /// <summary>
    ///     Message revision
    /// </summary>
    public int Revision { get; private set; }
}