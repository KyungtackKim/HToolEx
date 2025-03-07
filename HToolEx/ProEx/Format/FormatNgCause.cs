using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format Step/Job NG cause comment class
/// </summary>
[PublicAPI]
public class FormatNgCause {
    /// <summary>
    ///     NG cause comment count
    /// </summary>
    [PublicAPI] public static readonly int[] CommentCount = [10];

    /// <summary>
    ///     NG cause setting size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [1280];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatNgCause() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatNgCause(byte[] values, int revision = 0) {
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

        // check comment count
        for (var i = 0; i < CommentCount[revision]; i++)
            // add comment
            Comment.Add(Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0'));
    }

    /// <summary>
    ///     Comment
    /// </summary>
    public List<string> Comment { get; } = [];

    /// <summary>
    ///     Get values
    /// </summary>
    /// <param name="revision">revision</param>
    /// <returns>values</returns>
    public byte[] GetValues(int revision = 0) {
        var values = new List<byte>();
        // check comment count
        for (var i = 0; i < CommentCount[revision]; i++) {
            // get comment
            var comment = Encoding.ASCII.GetBytes(Comment[i]);
            // add values
            values.AddRange(comment);
            values.AddRange(new byte[128 - comment.Length]);
        }

        // values
        return values.ToArray();
    }
}