using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Member/Scan tool information class in ParaMon-Pro X
/// </summary>
public class FormatToolInfo {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatToolInfo(byte[] values) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // check values
        foreach (var b in values)
            // add check sum
            CheckSum += b;

        // check size
        if (values.Length >= ScanSize) {
            // set information
            ToolType  = bin.ReadByte();
            Model     = Encoding.ASCII.GetString(bin.ReadBytes(16)).Replace("\0", "");
            Serial    = Encoding.ASCII.GetString(bin.ReadBytes(16)).Replace("\0", "");
            Version   = bin.ReadUInt16();
            IpAddress = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
            Port      = bin.ReadUInt16();
            Mac =
                $"{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}";
        }

        // check size
        if (values.Length != MemberSize)
            return;
        Name   = Encoding.ASCII.GetString(bin.ReadBytes(32)).Replace("\0", "");
        Status = Convert.ToBoolean(bin.ReadByte());
    }

    /// <summary>
    ///     Constructor for I/O tool
    /// </summary>
    /// <param name="name">tool name</param>
    public FormatToolInfo(string name = "") {
        // set tool type
        ToolType = byte.MaxValue;
        // set tool name
        Name = name;
        // set status
        Status = true;
        // set serial number
        Serial = "-";
    }

    /// <summary>
    ///     Member tool information size
    /// </summary>
    [PublicAPI]
    public static int MemberSize => 80;

    /// <summary>
    ///     Scan tool information size
    /// </summary>
    [PublicAPI]
    public static int ScanSize => 47;

    /// <summary>
    ///     Tool type
    /// </summary>
    [PublicAPI]
    public int ToolType { get; }

    /// <summary>
    ///     Model number
    /// </summary>
    [PublicAPI]
    public string Model { get; } = "";

    /// <summary>
    ///     Serial number
    /// </summary>
    [PublicAPI]
    public string? Serial { get; } = "0000000000";

    /// <summary>
    ///     Version
    /// </summary>
    [PublicAPI]
    public int Version { get; }

    /// <summary>
    ///     IP address
    /// </summary>
    [PublicAPI]
    public string? IpAddress { get; } = "0.0.0.0";

    /// <summary>
    ///     Port number
    /// </summary>
    [PublicAPI]
    public int Port { get; }

    /// <summary>
    ///     Mac address
    /// </summary>
    [PublicAPI]
    public string? Mac { get; } = "00:00:00:00:00:00";

    /// <summary>
    ///     Tool name
    /// </summary>
    [PublicAPI]
    public string? Name { get; }

    /// <summary>
    ///     Status
    /// </summary>
    [PublicAPI]
    public bool Status { get; }

    /// <summary>
    ///     Compare check sum
    /// </summary>
    [PublicAPI]
    public int CheckSum { get; }
}

/// <summary>
///     Tool information message object comparer class
/// </summary>
public class FormatToolInfoComparer : IEqualityComparer<FormatToolInfo> {
    /// <summary>
    ///     Check equals
    /// </summary>
    /// <param name="x">source</param>
    /// <param name="y">destination</param>
    /// <returns>result</returns>
    public bool Equals(FormatToolInfo? x, FormatToolInfo? y) {
        // check message
        if (x == null || y == null)
            return false;
        // compare
        return x.CheckSum == y.CheckSum;
    }

    /// <summary>
    ///     Get hash code
    /// </summary>
    /// <param name="obj">object</param>
    /// <returns>result</returns>
    public int GetHashCode(FormatToolInfo obj) {
        return obj.CheckSum;
    }
}