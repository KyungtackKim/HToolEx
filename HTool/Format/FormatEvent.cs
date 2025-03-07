using System.ComponentModel;
using System.Text;
using HTool.Type;
using HTool.Util;
using JetBrains.Annotations;

namespace HTool.Format;

/// <summary>
///     Event data format class
/// </summary>
[PublicAPI]
public class FormatEvent {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatEvent() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="type">type</param>
    public FormatEvent(byte[] values, GenerationTypes type = GenerationTypes.GenRev1) {
        ushort val;
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
                // set values
                Id = bin.ReadUInt16();
                Date = DateTime.Now;
                Time = Date;
                FastenTime = bin.ReadUInt16();
                Preset = bin.ReadUInt16();
                TargetTorque = bin.ReadUInt16() / 100.0f;
                Torque = bin.ReadUInt16() / 100.0f;
                Speed = bin.ReadUInt16();
                Angle1 = bin.ReadUInt16();
                Angle2 = bin.ReadUInt16();
                Angle = bin.ReadUInt16();
                RemainScrew = bin.ReadUInt16();
                Error = bin.ReadUInt16();
                // get direction value
                val = bin.ReadUInt16();
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), (int)val))
                    // set direction
                    Direction = (DirectionTypes)val;
                // get event status value
                val = bin.ReadUInt16();
                // check defined event status
                if (Enum.IsDefined(typeof(EventTypes), (int)val))
                    // set event status
                    Event = (EventTypes)val;
                SnugAngle = bin.ReadUInt16();
                Barcode = Encoding.ASCII.GetString(bin.ReadBytes(Constants.BarcodeLength)).Replace("\0", string.Empty);
                break;
            case GenerationTypes.GenRev1Plus:
                // set values
                Id = bin.ReadUInt16();
                Date = DateTime.Now;
                Time = Date;
                FastenTime = bin.ReadUInt16();
                Preset = bin.ReadUInt16();
                TargetTorque = bin.ReadUInt16() / 100.0f;
                Torque = bin.ReadUInt16() / 100.0f;
                Speed = bin.ReadUInt16();
                Angle1 = bin.ReadUInt16();
                Angle2 = bin.ReadUInt16();
                Angle = bin.ReadUInt16();
                RemainScrew = bin.ReadUInt16();
                Error = bin.ReadUInt16();
                // get direction value
                val = bin.ReadUInt16();
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), (int)val))
                    // set direction
                    Direction = (DirectionTypes)val;
                // get event status value
                val = bin.ReadUInt16();
                // check defined event status
                if (Enum.IsDefined(typeof(EventTypes), (int)val))
                    // set event status
                    Event = (EventTypes)val;
                SnugAngle = bin.ReadUInt16();
                SeatingTorque = bin.ReadUInt16() / 100.0f;
                ClampTorque = bin.ReadUInt16() / 100.0f;
                PrevailingTorque = bin.ReadUInt16() / 100.0f;
                SnugTorque = bin.ReadUInt16() / 100.0f;
                Barcode = Encoding.ASCII.GetString(bin.ReadBytes(Constants.BarcodeLength)).Replace("\0", string.Empty);
                break;
            case GenerationTypes.GenRev2:
                // set values
                Revision = $"{bin.ReadByte()}.{bin.ReadByte()}";
                Id = bin.ReadUInt16();
                Date = new DateTime(bin.ReadUInt16(), bin.ReadByte(), bin.ReadByte(),
                    bin.ReadByte(), bin.ReadByte(), bin.ReadByte(), bin.ReadByte());
                Time = Date;
                FastenTime = bin.ReadUInt16();
                Preset = bin.ReadUInt16();
                // get unit value
                val = bin.ReadUInt16();
                // check defined unit
                if (Enum.IsDefined(typeof(UnitTypes), (int)val))
                    // set unit
                    Unit = (UnitTypes)val;
                RemainScrew = bin.ReadUInt16();
                // get direction value
                val = bin.ReadUInt16();
                // check defined direction
                if (Enum.IsDefined(typeof(DirectionTypes), (int)val))
                    // set direction
                    Direction = (DirectionTypes)val;
                Error = bin.ReadUInt16();
                // get event status value
                val = bin.ReadUInt16();
                // check defined event status
                if (Enum.IsDefined(typeof(EventTypes), (int)val))
                    // set event status
                    Event = (EventTypes)val;
                TargetTorque = bin.ReadSingle();
                Torque = bin.ReadSingle();
                SeatingTorque = bin.ReadSingle();
                ClampTorque = bin.ReadSingle();
                PrevailingTorque = bin.ReadSingle();
                SnugTorque = bin.ReadSingle();
                Speed = bin.ReadUInt16();
                Angle1 = bin.ReadUInt16();
                Angle2 = bin.ReadUInt16();
                Angle = bin.ReadUInt16();
                SnugAngle = bin.ReadUInt16();
                // reserved
                bin.ReadBytes(16);
                // barcode
                Barcode = Encoding.ASCII.GetString(bin.ReadBytes(Constants.BarcodeLength)).Replace("\0", string.Empty);
                // get type of channel 1
                val = bin.ReadUInt16();
                // check defined type
                if (Enum.IsDefined(typeof(GraphTypes), (int)val))
                    // set type
                    TypeOfChannel1 = (GraphTypes)val;
                // get type of channel 2
                val = bin.ReadUInt16();
                // check defined type
                if (Enum.IsDefined(typeof(GraphTypes), (int)val))
                    // set type
                    TypeOfChannel2 = (GraphTypes)val;
                CountOfChannel1 = bin.ReadUInt16();
                CountOfChannel2 = bin.ReadUInt16();
                SamplingRate = bin.ReadUInt16();
                // check count
                for (var i = 0; i < GraphSteps.Length; i++) {
                    // get id/index
                    var id = (GraphStepTypes)bin.ReadUInt16();
                    var index = bin.ReadUInt16();
                    // check defined id
                    if (Enum.IsDefined(typeof(GraphStepTypes), id))
                        // set step values
                        GraphSteps[i] = new GraphStep(id, index);
                }

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
    public GenerationTypes Type { get; set; }


    /// <summary>
    ///     Event id
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    ///     Event revision
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    ///     Event date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Event time
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     Event fastening time
    /// </summary>
    public int FastenTime { get; set; }

    /// <summary>
    ///     Event preset number
    /// </summary>
    public int Preset { get; set; }

    /// <summary>
    ///     Event torque unit
    /// </summary>
    public UnitTypes Unit { get; set; }

    /// <summary>
    ///     Event remain screws
    /// </summary>
    public int RemainScrew { get; set; }

    /// <summary>
    ///     Event tool direction
    /// </summary>
    public DirectionTypes Direction { get; set; }

    /// <summary>
    ///     Event error code
    /// </summary>
    public int Error { get; set; }

    /// <summary>
    ///     Event status
    /// </summary>
    public EventTypes Event { get; set; }

    /// <summary>
    ///     Event target torque
    /// </summary>
    public float TargetTorque { get; set; }

    /// <summary>
    ///     Event torque
    /// </summary>
    public float Torque { get; set; }

    /// <summary>
    ///     Event seating torque
    /// </summary>
    public float SeatingTorque { get; set; }

    /// <summary>
    ///     Event clamp torque
    /// </summary>
    public float ClampTorque { get; set; }

    /// <summary>
    ///     Event prevailing torque
    /// </summary>
    public float PrevailingTorque { get; set; }

    /// <summary>
    ///     Event snug torque
    /// </summary>
    public float SnugTorque { get; set; }

    /// <summary>
    ///     Event speed
    /// </summary>
    public int Speed { get; set; }

    /// <summary>
    ///     Event angle 1
    /// </summary>
    public int Angle1 { get; set; }

    /// <summary>
    ///     Event angle 2
    /// </summary>
    public int Angle2 { get; set; }

    /// <summary>
    ///     Event angle = angle 1 + angle 2
    /// </summary>
    public int Angle { get; set; }

    /// <summary>
    ///     Event snug angle
    /// </summary>
    public int SnugAngle { get; set; }

    /// <summary>
    ///     Event barcode
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    ///     Event type of channel 1
    /// </summary>
    public GraphTypes TypeOfChannel1 { get; set; }

    /// <summary>
    ///     Event type of channel 2
    /// </summary>
    public GraphTypes TypeOfChannel2 { get; set; }

    /// <summary>
    ///     Event count of channel 1
    /// </summary>
    public int CountOfChannel1 { get; set; }

    /// <summary>
    ///     Event count of channel 2
    /// </summary>
    public int CountOfChannel2 { get; set; }

    /// <summary>
    ///     Event sampling rate
    /// </summary>
    public int SamplingRate { get; set; }

    /// <summary>
    ///     Event graph steps
    /// </summary>
    [Browsable(false)]
    public GraphStep[] GraphSteps { get; set; } = new GraphStep[16];

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; set; }


    /// <summary>
    ///     Graph step class
    /// </summary>
    [PublicAPI]
    public class GraphStep {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="index">index</param>
        public GraphStep(GraphStepTypes id, int index) {
            // set id
            Id = id;
            // set index
            Index = index;
        }

        /// <summary>
        ///     Step id
        /// </summary>
        public GraphStepTypes Id { get; set; }

        /// <summary>
        ///     Step index
        /// </summary>
        public int Index { get; set; }
    }
}