using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format I/O Tool names class
/// </summary>
[PublicAPI]
public class FormatSetIoToolName {
    /// <summary>
    ///     I/O tool count
    /// </summary>
    [PublicAPI] public static readonly int[] ToolCount = [8];

    /// <summary>
    ///     I/O Tool name size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [256];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetIoToolName() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetIoToolName(byte[] values, int revision = 0) {
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

        // check tool count
        for (var i = 0; i < ToolCount[revision]; i++)
            // add tool
            IoTools.Add(new FormatToolInfo(Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0')));
    }

    /// <summary>
    ///     I/O tool list
    /// </summary>
    public List<FormatToolInfo> IoTools { get; } = [];
}