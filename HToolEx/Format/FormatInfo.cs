using System.Globalization;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.Format;

/// <summary>
///     Information format class
/// </summary>
[PublicAPI]
public class FormatInfo {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatInfo(byte[] values) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);
        // set information
        DriverId = bin.ReadInt16();
        DriverNo = bin.ReadInt16();
        DriverName = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        DriverSerial = Encoding.ASCII.GetString(bin.ReadBytes(10)).TrimEnd('\0');
        ControllerNo = bin.ReadInt16();
        ControllerName = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        ControllerSerial = Encoding.ASCII.GetString(bin.ReadBytes(10)).TrimEnd('\0');
        Firmware = $"{bin.ReadInt16()}.{bin.ReadInt16()}.{bin.ReadInt16()}";
        if (DateTime.TryParseExact($"{bin.ReadInt16():D4}-{bin.ReadByte():D2}-{bin.ReadByte():D2}",
                "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            // set production date
            ProductionDate = date;
        IsAdvanceMode = bin.ReadInt16() > 0;
        Mac = string.Join(":", bin.ReadBytes(6).Select(v => $"{v:X2}"));
        // get check sum
        CheckSum = values.Sum(v => v);

        // throw an error if not all data has been read
        if (bin.BaseStream.Position != bin.BaseStream.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{bin.BaseStream.Length - bin.BaseStream.Position} byte(s) remain");
    }

    /// <summary>
    ///     Format size
    /// </summary>
    public static int Size { get; } = 108;

    /// <summary>
    ///     Format address count
    /// </summary>
    public static ushort Count { get; } = 54;

    /// <summary>
    ///     Driver id
    /// </summary>
    public int DriverId { get; }

    /// <summary>
    ///     Driver model number
    /// </summary>
    public int DriverNo { get; }

    /// <summary>
    ///     Driver model name
    /// </summary>
    public string DriverName { get; }

    /// <summary>
    ///     Driver serial number
    /// </summary>
    public string DriverSerial { get; }

    /// <summary>
    ///     Controller model number
    /// </summary>
    public int ControllerNo { get; }

    /// <summary>
    ///     Controller model name
    /// </summary>
    public string ControllerName { get; }

    /// <summary>
    ///     Controller serial number
    /// </summary>
    public string ControllerSerial { get; }

    /// <summary>
    ///     Controller firmware version
    /// </summary>
    public string Firmware { get; }

    /// <summary>
    ///     Controller production date
    /// </summary>
    public DateTime ProductionDate { get; }

    /// <summary>
    ///     Controller advance mode
    /// </summary>
    public bool IsAdvanceMode { get; }

    /// <summary>
    ///     MAC address
    /// </summary>
    public string Mac { get; }

    /// <summary>
    ///     Check sum
    /// </summary>
    public int CheckSum { get; }
}