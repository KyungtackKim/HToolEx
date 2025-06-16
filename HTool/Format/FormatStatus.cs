using System.ComponentModel;
using HTool.Type;
using HTool.Util;
using JetBrains.Annotations;

namespace HTool.Format;

/// <summary>
///     Status format class
/// </summary>
[PublicAPI]
public class FormatStatus {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="type">type</param>
    public FormatStatus(byte[] values, GenerationTypes type = GenerationTypes.GenRev1) {
        int val1, val2;
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set generation type
        Type = type;
        // check type
        switch (type) {
            case GenerationTypes.GenRev1:
            case GenerationTypes.GenRev1Ad:
            case GenerationTypes.GenRev1Plus:
                // set values
                Torque   = bin.ReadUInt16();
                Speed    = bin.ReadUInt16();
                Current  = bin.ReadUInt16();
                Preset   = bin.ReadUInt16();
                TorqueUp = Convert.ToBoolean(bin.ReadUInt16());
                FastenOk = Convert.ToBoolean(bin.ReadUInt16());
                Ready    = Convert.ToBoolean(bin.ReadUInt16());
                Run      = Convert.ToBoolean(bin.ReadUInt16());
                Alarm    = bin.ReadUInt16();
                // get direction value
                val1 = bin.ReadUInt16();
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), val1))
                    // set direction
                    Direction = (DirectionTypes)val1;
                RemainScrew = bin.ReadUInt16();
                // get input/output value
                val1 = bin.ReadUInt16();
                val2 = bin.ReadUInt16();
                // set input/output value
                Input  = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((val1 >> i) & 0x1)).ToArray();
                Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((val2 >> i) & 0x1)).ToArray();

                // set temperature
                Temperature = bin.ReadUInt16();
                break;
            case GenerationTypes.GenRev2:
                // set values
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
                val1 = bin.ReadUInt16();
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), val1))
                    // set direction
                    Direction = (DirectionTypes)val1;
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
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // get check sum
        CheckSum = Utils.CalculateCheckSum(values);
        // throw an error if not all data has been read
        if (bin.BaseStream.Position != bin.BaseStream.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{bin.BaseStream.Length - bin.BaseStream.Position} byte(s) remain");
    }

    /// <summary>
    ///     Generation type
    /// </summary>
    public GenerationTypes Type { get; }

    /// <summary>
    ///     Torque
    /// </summary>
    public float Torque { get; }

    /// <summary>
    ///     Speed
    /// </summary>
    public int Speed { get; }

    /// <summary>
    ///     Current
    /// </summary>
    public float Current { get; }

    /// <summary>
    ///     Selected preset ( 0 ~ 31, MA(32), MB(33) )
    /// </summary>
    public int Preset { get; }

    /// <summary>
    ///     Selected model  ( 0 ~ 15 )
    /// </summary>
    public int Model { get; }

    /// <summary>
    ///     Torque up state
    /// </summary>
    public bool TorqueUp { get; }

    /// <summary>
    ///     Fasten ok state
    /// </summary>
    public bool FastenOk { get; }

    /// <summary>
    ///     Ready state
    /// </summary>
    public bool Ready { get; }

    /// <summary>
    ///     Running state
    /// </summary>
    public bool Run { get; }

    /// <summary>
    ///     Alarm code
    /// </summary>
    public int Alarm { get; }

    /// <summary>
    ///     Direction state
    /// </summary>
    public DirectionTypes Direction { get; }

    /// <summary>
    ///     Remain screws
    /// </summary>
    public int RemainScrew { get; }

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
    public float Temperature { get; }

    /// <summary>
    ///     Lock state
    /// </summary>
    public bool IsLock { get; }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; }
}