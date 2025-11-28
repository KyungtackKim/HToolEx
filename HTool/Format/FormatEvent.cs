using System.ComponentModel;
using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     Event data format class
/// </summary>
public sealed class FormatEvent {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatEvent() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="type">type</param>
    public FormatEvent(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        // set the event data
        Set(values, type);
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
    ///     Set the event data
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="type">generation type</param>
    public void Set(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        int dir;
        int status;
        var pos = 0;
        // reset the information
        Reset();
        // check the values (year - month)
        if (values[4] == 0 && values[5] == 0 && values[6] == 0 && values[7] == 0 && values[8] == 0 && values[9] == 0)
            return;
        // set the type
        Type = type;
        // get the span
        var span = values.AsSpan();
        // check the type
        switch (type) {
            case GenerationTypes.GenRev1:
            case GenerationTypes.GenRev1Ad:
                // set values
                Id           = BinarySpanReader.ReadUInt16(span, ref pos);
                Date         = Time = DateTime.Now;
                FastenTime   = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset       = BinarySpanReader.ReadUInt16(span, ref pos);
                TargetTorque = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Torque       = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Speed        = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle1       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle2       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle        = BinarySpanReader.ReadUInt16(span, ref pos);
                RemainScrew  = BinarySpanReader.ReadUInt16(span, ref pos);
                Error        = BinarySpanReader.ReadUInt16(span, ref pos);
                // get the direction
                dir = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (dir <= (int)DirectionTypes.Loosening)
                    Direction = (DirectionTypes)dir;
                // get the status
                status = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    Event = (EventTypes)status;
                SnugAngle =  BinarySpanReader.ReadUInt16(span, ref pos);
                Barcode   =  Utils.ToAsciiTrimEnd(span.Slice(pos, Constants.BarcodeLength));
                pos       += Constants.BarcodeLength;
                break;
            case GenerationTypes.GenRev1Plus:
                // set values
                Id           = BinarySpanReader.ReadUInt16(span, ref pos);
                Date         = Time = DateTime.Now;
                FastenTime   = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset       = BinarySpanReader.ReadUInt16(span, ref pos);
                TargetTorque = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Torque       = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Speed        = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle1       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle2       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle        = BinarySpanReader.ReadUInt16(span, ref pos);
                RemainScrew  = BinarySpanReader.ReadUInt16(span, ref pos);
                Error        = BinarySpanReader.ReadUInt16(span, ref pos);
                // get the direction
                dir = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (dir <= (int)DirectionTypes.Loosening)
                    Direction = (DirectionTypes)dir;
                // get the status
                status = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    Event = (EventTypes)status;
                SnugAngle        =  BinarySpanReader.ReadUInt16(span, ref pos);
                SeatingTorque    =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                ClampTorque      =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                PrevailingTorque =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                SnugTorque       =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Barcode          =  Utils.ToAsciiTrimEnd(span.Slice(pos, Constants.BarcodeLength));
                pos              += Constants.BarcodeLength;
                break;
            case GenerationTypes.GenRev2:
                // header
                Revision = $"{BinarySpanReader.ReadByte(span, ref pos)}.{BinarySpanReader.ReadByte(span, ref pos)}";
                Id       = BinarySpanReader.ReadUInt16(span, ref pos);
                // date time
                var year        = BinarySpanReader.ReadUInt16(span, ref pos);
                var month       = BinarySpanReader.ReadByte(span, ref pos);
                var day         = BinarySpanReader.ReadByte(span, ref pos);
                var hour        = BinarySpanReader.ReadByte(span, ref pos);
                var minute      = BinarySpanReader.ReadByte(span, ref pos);
                var second      = BinarySpanReader.ReadByte(span, ref pos);
                var millisecond = BinarySpanReader.ReadByte(span, ref pos);
                // set date time
                Date = Time = new DateTime(year, month, day, hour, minute, second, millisecond);
                // common
                FastenTime = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset     = BinarySpanReader.ReadUInt16(span, ref pos);
                // get the unit
                var unit = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (unit <= (int)UnitTypes.LbfFt)
                    Unit = (UnitTypes)unit;
                RemainScrew = BinarySpanReader.ReadUInt16(span, ref pos);
                // get the direction
                dir = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (dir <= (int)DirectionTypes.Loosening)
                    Direction = (DirectionTypes)dir;
                Error = BinarySpanReader.ReadUInt16(span, ref pos);
                // get the status
                status = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    Event = (EventTypes)status;
                // torque
                TargetTorque     = BinarySpanReader.ReadSingle(span, ref pos);
                Torque           = BinarySpanReader.ReadSingle(span, ref pos);
                SeatingTorque    = BinarySpanReader.ReadSingle(span, ref pos);
                ClampTorque      = BinarySpanReader.ReadSingle(span, ref pos);
                PrevailingTorque = BinarySpanReader.ReadSingle(span, ref pos);
                SnugTorque       = BinarySpanReader.ReadSingle(span, ref pos);
                // speed angle
                Speed     = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle1    = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle2    = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle     = BinarySpanReader.ReadUInt16(span, ref pos);
                SnugAngle = BinarySpanReader.ReadUInt16(span, ref pos);
                // reserved
                pos += 16;
                // set the barcode
                Barcode =  Utils.ToAsciiTrimEnd(span.Slice(pos, Constants.BarcodeLength));
                pos     += Constants.BarcodeLength;
                // get the graph type
                var type1 = BinarySpanReader.ReadUInt16(span, ref pos);
                var type2 = BinarySpanReader.ReadUInt16(span, ref pos);
                // check defined
                if (type1 <= (int)GraphTypes.TorqueAngle)
                    TypeOfChannel1 = (GraphTypes)type1;
                if (type2 <= (int)GraphTypes.TorqueAngle)
                    TypeOfChannel2 = (GraphTypes)type2;
                CountOfChannel1 = BinarySpanReader.ReadUInt16(span, ref pos);
                CountOfChannel2 = BinarySpanReader.ReadUInt16(span, ref pos);
                SamplingRate    = BinarySpanReader.ReadUInt16(span, ref pos);
                // check the step count
                for (var i = 0; i < GraphSteps.Length; i++) {
                    // get the step information
                    var id    = BinarySpanReader.ReadUInt16(span, ref pos);
                    var index = BinarySpanReader.ReadUInt16(span, ref pos);
                    // check defined
                    if (id <= (int)GraphStepTypes.RotationAfterTorqueUp)
                        GraphSteps[i] = new GraphStep((GraphStepTypes)id, index);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // set the checksum
        CheckSum = Utils.CalculateCheckSum(span);
        // check the length
        if (pos != span.Length)
            throw new InvalidDataException($"Not all bytes have been consumed. {span.Length - pos} byte(s) remain");
    }

    private void Reset() {
        // reset the values
        Id              = 0;
        Revision        = string.Empty;
        Date            = default;
        Time            = default;
        FastenTime      = Preset = RemainScrew   = Error       = Speed            = Angle1     = Angle2 = Angle = SnugAngle = 0;
        TargetTorque    = Torque = SeatingTorque = ClampTorque = PrevailingTorque = SnugTorque = 0f;
        Unit            = default;
        Direction       = default;
        Event           = default;
        TypeOfChannel1  = default;
        TypeOfChannel2  = default;
        CountOfChannel1 = CountOfChannel2 = SamplingRate = 0;
        Barcode         = string.Empty;
        Array.Clear(GraphSteps, 0, GraphSteps.Length);
        CheckSum = 0;
    }

    /// <summary>
    ///     Graph step class
    /// </summary>
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