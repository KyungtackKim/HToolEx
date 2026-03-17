using HToolEx.Type;
using HToolEx.Util;
using JetBrains.Annotations;
using Convert = System.Convert;

namespace HToolEx.Format;

/// <summary>
///     Status format class
/// </summary>
[PublicAPI]
public class FormatStatus {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatStatus(byte[] values) {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);
        // set information
        Torque   = bin.ReadSingle();
        Speed    = bin.ReadUInt16();
        Current  = bin.ReadSingle();
        Preset   = bin.ReadUInt16();
        Model    = bin.ReadUInt16();
        TorqueUp = Convert.ToBoolean(bin.ReadUInt16());
        FastenOk = Convert.ToBoolean(bin.ReadUInt16());
        Ready    = Convert.ToBoolean(bin.ReadUInt16());
        Run      = Convert.ToBoolean(bin.ReadUInt16());
        Alarm    = bin.ReadUInt16();
        // get direction value
        var dir = bin.ReadUInt16();
        // check defined direction
        if (Enum.IsDefined(typeof(DirectionTypes), (int)dir))
            // set direction
            Direction = (DirectionTypes)dir;
        RemainScrew = bin.ReadUInt16();
        // get input/output value
        var input  = bin.ReadUInt16();
        var output = bin.ReadUInt16();
        // set input/output value
        Input  = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((input  >> i) & 0x1)).ToArray();
        Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((output >> i) & 0x1)).ToArray();

        // set temperature
        Temperature = bin.ReadSingle();
        // set lock state
        IsLock = Convert.ToBoolean(bin.ReadUInt16());
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
    public static int Size { get; private set; } = 38;

    /// <summary>
    ///     Torque
    /// </summary>
    public float Torque { get; private set; }

    /// <summary>
    ///     Speed
    /// </summary>
    public int Speed { get; private set; }

    /// <summary>
    ///     Current
    /// </summary>
    public float Current { get; private set; }

    /// <summary>
    ///     Selected preset ( 0 ~ 31, MA(32), MB(33) )
    /// </summary>
    public int Preset { get; private set; }

    /// <summary>
    ///     Selected model  ( 0 ~ 15 )
    /// </summary>
    public int Model { get; private set; }

    /// <summary>
    ///     Torque up state
    /// </summary>
    public bool TorqueUp { get; private set; }

    /// <summary>
    ///     Fasten ok state
    /// </summary>
    public bool FastenOk { get; private set; }

    /// <summary>
    ///     Ready state
    /// </summary>
    public bool Ready { get; private set; }

    /// <summary>
    ///     Running state
    /// </summary>
    public bool Run { get; private set; }

    /// <summary>
    ///     Alarm code
    /// </summary>
    public int Alarm { get; private set; }

    /// <summary>
    ///     Direction state
    /// </summary>
    public DirectionTypes Direction { get; private set; }

    /// <summary>
    ///     Remain screws
    /// </summary>
    public int RemainScrew { get; private set; }

    /// <summary>
    ///     Input signal
    /// </summary>
    public bool[] Input { get; }

    /// <summary>
    ///     Output signal
    /// </summary>
    public bool[] Output { get; }

    /// <summary>
    ///     Temperature
    /// </summary>
    public float Temperature { get; private set; }

    /// <summary>
    ///     Lock state
    /// </summary>
    public bool IsLock { get; private set; }

    /// <summary>
    ///     Check sum
    /// </summary>
    public int CheckSum { get; }
}