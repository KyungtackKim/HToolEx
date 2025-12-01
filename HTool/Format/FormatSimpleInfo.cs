using System.ComponentModel;
using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     Device information format function class (Legacy protocol)
/// </summary>
public sealed class FormatSimpleInfo {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSimpleInfo() {
        // reset values
        Id         = 0;
        Controller = 0;
        Driver     = 0;
        Firmware   = 0;
        Serial     = "0000000000";
        Model      = 0;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatSimpleInfo(byte[] values) {
        // check size
        if (values.Length < Size)
            return;

        // get span
        var span = values.AsSpan();
        var pos  = 0;

        // get values
        Id         = BinarySpanReader.ReadUInt16(span, ref pos);
        Controller = BinarySpanReader.ReadUInt16(span, ref pos);
        Driver     = BinarySpanReader.ReadUInt16(span, ref pos);
        Firmware   = BinarySpanReader.ReadUInt16(span, ref pos);
        // get serial raw values
        var s = span.Slice(pos, 5);
        pos += 5;
        // set serial number
        Serial = $"{s[^1]:D2}{s[^2]:D2}{s[^3]:D2}{s[^4]:D2}{s[^5]:D2}";
        // check length (255255xx255255)
        if (Serial.Length == 14)
            // set default model code
            Serial = $"0000{s[^3]:D2}0000";
        // check position
        if (pos <= span.Length - sizeof(uint))
            // set used count
            Used = BinarySpanReader.ReadUInt32(span, ref pos);
        // check defined model
        if (Enum.IsDefined(typeof(ModelTypes), Convert.ToInt32(Serial[4..6])))
            // set model types
            Model = (ModelTypes)Convert.ToInt32(Serial[4..6]);
        else
            // set default model type
            Model = ModelTypes.Ad;
        // check ad model type
        if (Model == ModelTypes.Ad)
            // skip the dummy data
            pos += 1;

        // get check sum
        CheckSum = values.Sum(v => v);
        // throw an error if not all data has been read
        if (pos != span.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{span.Length - pos} byte(s) remain");
    }

    /// <summary>
    ///     Information data size
    /// </summary>
    public static int Size => 13;

    /// <summary>
    ///     Device id
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Controller model number
    /// </summary>
    public int Controller { get; }

    /// <summary>
    ///     Driver model number
    /// </summary>
    public int Driver { get; }

    /// <summary>
    ///     Firmware version
    /// </summary>
    public int Firmware { get; }

    /// <summary>
    ///     Serial number
    /// </summary>
    public string Serial { get; } = string.Empty;

    /// <summary>
    ///     Used count
    /// </summary>
    public uint Used { get; }

    /// <summary>
    ///     Model types
    /// </summary>
    public ModelTypes Model { get; }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; }
}