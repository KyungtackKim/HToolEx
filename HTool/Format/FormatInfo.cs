using System.ComponentModel;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     Device information format class (Modbus standard protocol)
/// </summary>
/// <remarks>
///     This class is only for Gen.2 devices. Do not use for other device types.
/// </remarks>
public sealed class FormatInfo {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatInfo() {
        // reset values
        SystemInfo             = 0;
        DriverId               = 0;
        DriverModelNumber      = 0;
        DriverModelName        = string.Empty;
        DriverSerialNumber     = string.Empty;
        ControllerModelNumber  = 0;
        ControllerModelName    = string.Empty;
        ControllerSerialNumber = string.Empty;
        FirmwareVersionMajor   = 0;
        FirmwareVersionMinor   = 0;
        FirmwareVersionPatch   = 0;
        ProductionDate         = 0;
        AdvanceType            = 0;
        MacAddress             = new byte[6];
        EventDataRevision      = 0;
        Manufacturer           = 0;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatInfo(byte[] values) {
        // get span
        var span = values.AsSpan();
        var pos  = 0;

        // skip system info (2 bytes)
        SystemInfo = BinarySpanReader.ReadUInt16(span, ref pos);

        // get driver info
        DriverId           = BinarySpanReader.ReadUInt16(span, ref pos);
        DriverModelNumber  = BinarySpanReader.ReadUInt16(span, ref pos);
        DriverModelName    = BinarySpanReader.ReadAsciiString(span, ref pos, 32);
        DriverSerialNumber = BinarySpanReader.ReadAsciiString(span, ref pos, 10);

        // get controller info
        ControllerModelNumber  = BinarySpanReader.ReadUInt16(span, ref pos);
        ControllerModelName    = BinarySpanReader.ReadAsciiString(span, ref pos, 32);
        ControllerSerialNumber = BinarySpanReader.ReadAsciiString(span, ref pos, 10);

        // get firmware version
        FirmwareVersionMajor = BinarySpanReader.ReadUInt16(span, ref pos);
        FirmwareVersionMinor = BinarySpanReader.ReadUInt16(span, ref pos);
        FirmwareVersionPatch = BinarySpanReader.ReadUInt16(span, ref pos);

        // get production info
        ProductionDate = BinarySpanReader.ReadUInt32(span, ref pos);
        AdvanceType    = BinarySpanReader.ReadUInt16(span, ref pos);

        // get mac address
        MacAddress =  span.Slice(pos, 6).ToArray();
        pos        += 6;

        // get metadata
        EventDataRevision = BinarySpanReader.ReadUInt16(span, ref pos);
        Manufacturer      = BinarySpanReader.ReadUInt16(span, ref pos);

        // skip reserved (86 bytes)
        pos += 86;

        // get check sum
        CheckSum = Utils.CalculateCheckSum(span);
        // throw an error if not all data has been read
        if (pos != span.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{span.Length - pos} byte(s) remain");
    }

    /// <summary>
    ///     Information data size
    /// </summary>
    public static int Size => 200;

    /// <summary>
    ///     System info (reserved)
    /// </summary>
    [Browsable(false)]
    public int SystemInfo { get; }

    /// <summary>
    ///     Driver id
    /// </summary>
    public int DriverId { get; }

    /// <summary>
    ///     Driver model number
    /// </summary>
    public int DriverModelNumber { get; }

    /// <summary>
    ///     Driver model name
    /// </summary>
    public string DriverModelName { get; } = string.Empty;

    /// <summary>
    ///     Driver serial number
    /// </summary>
    public string DriverSerialNumber { get; } = string.Empty;

    /// <summary>
    ///     Controller model number
    /// </summary>
    public int ControllerModelNumber { get; }

    /// <summary>
    ///     Controller model name
    /// </summary>
    public string ControllerModelName { get; } = string.Empty;

    /// <summary>
    ///     Controller serial number
    /// </summary>
    public string ControllerSerialNumber { get; } = string.Empty;

    /// <summary>
    ///     Firmware version major
    /// </summary>
    public int FirmwareVersionMajor { get; }

    /// <summary>
    ///     Firmware version minor
    /// </summary>
    public int FirmwareVersionMinor { get; }

    /// <summary>
    ///     Firmware version patch
    /// </summary>
    public int FirmwareVersionPatch { get; }

    /// <summary>
    ///     Firmware version string
    /// </summary>
    public string FirmwareVersion => $"{FirmwareVersionMajor}.{FirmwareVersionMinor}.{FirmwareVersionPatch}";

    /// <summary>
    ///     Production date (YYYYMMDD)
    /// </summary>
    public uint ProductionDate { get; }

    /// <summary>
    ///     Advance type (0=Normal, 1=Plus)
    /// </summary>
    public int AdvanceType { get; }

    /// <summary>
    ///     Mac address bytes
    /// </summary>
    public byte[] MacAddress { get; } = new byte[6];

    /// <summary>
    ///     Mac address string (xx:xx:xx:xx:xx:xx)
    /// </summary>
    public string MacAddressString => string.Join(":", MacAddress.Select(b => b.ToString("X2")));

    /// <summary>
    ///     Event data revision
    /// </summary>
    public int EventDataRevision { get; }

    /// <summary>
    ///     Manufacturer (1=Hantas, 2=Mountz)
    /// </summary>
    public int Manufacturer { get; }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; }
}