using System.ComponentModel;
using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     Status format class
/// </summary>
public sealed class FormatStatus {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="type">type</param>
    public FormatStatus(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        int val1, val2;
        // get span
        var span = values.AsSpan();
        var pos  = 0;

        // set generation type
        Type = type;
        // check type
        switch (type) {
            case GenerationTypes.GenRev1:
            case GenerationTypes.GenRev1Ad:
            case GenerationTypes.GenRev1Plus:
                // set values
                Torque   = BinarySpanReader.ReadUInt16(span, ref pos);
                Speed    = BinarySpanReader.ReadUInt16(span, ref pos);
                Current  = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset   = BinarySpanReader.ReadUInt16(span, ref pos);
                TorqueUp = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                FastenOk = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Ready    = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Run      = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Alarm    = BinarySpanReader.ReadUInt16(span, ref pos);
                // get direction value
                val1 = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), val1))
                    Direction = (DirectionTypes)val1;
                RemainScrew = BinarySpanReader.ReadUInt16(span, ref pos);
                // get input/output value
                val1 = BinarySpanReader.ReadUInt16(span, ref pos);
                val2 = BinarySpanReader.ReadUInt16(span, ref pos);
                // set input/output value
                Input  = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((val1 >> i) & 0x1)).ToArray();
                Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((val2 >> i) & 0x1)).ToArray();
                // set temperature
                Temperature = BinarySpanReader.ReadUInt16(span, ref pos);
                break;
            case GenerationTypes.GenRev2:
                // set values
                Torque   = BinarySpanReader.ReadSingle(span, ref pos);
                Speed    = BinarySpanReader.ReadUInt16(span, ref pos);
                Current  = BinarySpanReader.ReadSingle(span, ref pos);
                Preset   = BinarySpanReader.ReadUInt16(span, ref pos);
                Model    = BinarySpanReader.ReadUInt16(span, ref pos);
                TorqueUp = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                FastenOk = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Ready    = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Run      = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Alarm    = BinarySpanReader.ReadUInt16(span, ref pos);
                // get direction value
                val1 = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), val1))
                    Direction = (DirectionTypes)val1;
                RemainScrew = BinarySpanReader.ReadUInt16(span, ref pos);
                // get input/output value
                var input  = BinarySpanReader.ReadUInt16(span, ref pos);
                var output = BinarySpanReader.ReadUInt16(span, ref pos);
                // set input/output value
                Input  = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((input  >> i) & 0x1)).ToArray();
                Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((output >> i) & 0x1)).ToArray();
                // set temperature
                Temperature = BinarySpanReader.ReadSingle(span, ref pos);
                // set lock state
                IsLock = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // get check sum
        CheckSum = Utils.CalculateCheckSum(values);
        // throw an error if not all data has been read
        if (pos != span.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{span.Length - pos} byte(s) remain");
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