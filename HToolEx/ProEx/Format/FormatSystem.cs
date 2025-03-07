using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format system information class
/// </summary>
public class FormatSystem {
    /// <summary>
    ///     Share information size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [88, 90];

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSystem(byte[] values, int revision = 0) {
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
        // get revision.0
        Version = Encoding.ASCII.GetString(bin.ReadBytes(16)).Replace("\0", "");
        Serial = Encoding.ASCII.GetString(bin.ReadBytes(16)).Replace("\0", "");
        InternalCapacity = bin.ReadUInt32();
        InternalFree = bin.ReadUInt32();
        InternalUsed = bin.ReadUInt32();
        SdCapacity = bin.ReadUInt32();
        SdFree = bin.ReadUInt32();
        SdUsed = bin.ReadUInt32();
        IpAddress = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        NetMask = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        Gateway = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        Mac =
            $"{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}";
        WiFiIpAddress = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        WifiNetMask = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        WifiMac =
            $"{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}:{bin.ReadByte():X2}";
        // check revision.1
        if (revision < 1)
            return;
        // get revision.1
        LatestEventRevision = bin.ReadUInt16();
    }

    /// <summary>
    ///     Software version
    /// </summary>
    [PublicAPI]
    public string Version { get; } = default!;

    /// <summary>
    ///     Serial number
    /// </summary>
    [PublicAPI]
    public string Serial { get; } = default!;

    /// <summary>
    ///     Internal capacity
    /// </summary>
    [PublicAPI]
    public double InternalCapacity { get; }

    /// <summary>
    ///     Internal free
    /// </summary>
    [PublicAPI]
    public double InternalFree { get; }

    /// <summary>
    ///     Internal used
    /// </summary>
    [PublicAPI]
    public double InternalUsed { get; }

    /// <summary>
    ///     SD card capacity
    /// </summary>
    [PublicAPI]
    public double SdCapacity { get; }

    /// <summary>
    ///     SD card free
    /// </summary>
    [PublicAPI]
    public double SdFree { get; }

    /// <summary>
    ///     SD card used
    /// </summary>
    [PublicAPI]
    public double SdUsed { get; }

    /// <summary>
    ///     IP address
    /// </summary>
    [PublicAPI]
    public string IpAddress { get; } = default!;

    /// <summary>
    ///     Net mask
    /// </summary>
    [PublicAPI]
    public string NetMask { get; } = default!;

    /// <summary>
    ///     Gateway
    /// </summary>
    [PublicAPI]
    public string Gateway { get; } = default!;

    /// <summary>
    ///     MAC address
    /// </summary>
    [PublicAPI]
    public string Mac { get; } = default!;

    /// <summary>
    ///     Wi-Fi ip address
    /// </summary>
    [PublicAPI]
    public string WiFiIpAddress { get; } = default!;

    /// <summary>
    ///     Wi-Fi netmask
    /// </summary>
    [PublicAPI]
    public string WifiNetMask { get; } = default!;

    /// <summary>
    ///     Wi-Fi mac address
    /// </summary>
    [PublicAPI]
    public string WifiMac { get; } = default!;

    /// <summary>
    ///     Latest event revision number
    /// </summary>
    [PublicAPI]
    public int LatestEventRevision { get; }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }
}