using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    public void Set(byte[] values, GenerationTypes type = GenerationTypes.GenRev1) {
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
                Id           = ReadUInt16(span, ref pos);
                Date         = Time = DateTime.Now;
                FastenTime   = ReadUInt16(span, ref pos);
                Preset       = ReadUInt16(span, ref pos);
                TargetTorque = ReadUInt16(span, ref pos) / 100.0f;
                Torque       = ReadUInt16(span, ref pos) / 100.0f;
                Speed        = ReadUInt16(span, ref pos);
                Angle1       = ReadUInt16(span, ref pos);
                Angle2       = ReadUInt16(span, ref pos);
                Angle        = ReadUInt16(span, ref pos);
                RemainScrew  = ReadUInt16(span, ref pos);
                Error        = ReadUInt16(span, ref pos);
                // get the direction
                dir = ReadUInt16(span, ref pos);
                // check defined
                if (dir <= (int)DirectionTypes.Loosening)
                    // set the value
                    Direction = (DirectionTypes)dir;
                // get the status
                status = ReadUInt16(span, ref pos);
                // check defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    // set the value
                    Event = (EventTypes)status;
                SnugAngle = ReadUInt16(span, ref pos);
                Barcode   = AsciiTrimNullRight(span.Slice(pos, Constants.BarcodeLength));
                // update the position
                pos += Constants.BarcodeLength;
                break;
            case GenerationTypes.GenRev1Plus:
                // set values
                Id           = ReadUInt16(span, ref pos);
                Date         = Time = DateTime.Now;
                FastenTime   = ReadUInt16(span, ref pos);
                Preset       = ReadUInt16(span, ref pos);
                TargetTorque = ReadUInt16(span, ref pos) / 100.0f;
                Torque       = ReadUInt16(span, ref pos) / 100.0f;
                Speed        = ReadUInt16(span, ref pos);
                Angle1       = ReadUInt16(span, ref pos);
                Angle2       = ReadUInt16(span, ref pos);
                Angle        = ReadUInt16(span, ref pos);
                RemainScrew  = ReadUInt16(span, ref pos);
                Error        = ReadUInt16(span, ref pos);
                // get the direction
                dir = ReadUInt16(span, ref pos);
                // check defined
                if (dir <= (int)DirectionTypes.Loosening)
                    // set the value
                    Direction = (DirectionTypes)dir;
                // get the status
                status = ReadUInt16(span, ref pos);
                // check defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    // set the value
                    Event = (EventTypes)status;
                SnugAngle        = ReadUInt16(span, ref pos);
                SeatingTorque    = ReadUInt16(span, ref pos) / 100.0f;
                ClampTorque      = ReadUInt16(span, ref pos) / 100.0f;
                PrevailingTorque = ReadUInt16(span, ref pos) / 100.0f;
                SnugTorque       = ReadUInt16(span, ref pos) / 100.0f;
                Barcode          = AsciiTrimNullRight(span.Slice(pos, Constants.BarcodeLength));
                // update the position
                pos += Constants.BarcodeLength;
                break;
            case GenerationTypes.GenRev2:
                // header
                Revision = $"{ReadByte(span, ref pos)}.{ReadByte(span, ref pos)}";
                Id       = ReadUInt16(span, ref pos);
                // date time
                var year        = ReadUInt16(span, ref pos);
                var month       = ReadByte(span, ref pos);
                var day         = ReadByte(span, ref pos);
                var hour        = ReadByte(span, ref pos);
                var minute      = ReadByte(span, ref pos);
                var second      = ReadByte(span, ref pos);
                var millisecond = ReadByte(span, ref pos);
                // set date time
                Date = Time = new DateTime(year, month, day, hour, minute, second, millisecond);
                // common
                FastenTime = ReadUInt16(span, ref pos);
                Preset     = ReadUInt16(span, ref pos);
                // get the unit
                var unit = ReadUInt16(span, ref pos);
                // check defined
                if (unit <= (int)UnitTypes.LbfFt)
                    // set the value
                    Unit = (UnitTypes)unit;
                RemainScrew = ReadUInt16(span, ref pos);
                // get the direction
                dir = ReadUInt16(span, ref pos);
                // check defined
                if (dir <= (int)DirectionTypes.Loosening)
                    // set the value
                    Direction = (DirectionTypes)dir;
                Error = ReadUInt16(span, ref pos);
                // get the status
                status = ReadUInt16(span, ref pos);
                // check defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    // set the value
                    Event = (EventTypes)status;
                // torque
                TargetTorque     = ReadSingle(span, ref pos);
                Torque           = ReadSingle(span, ref pos);
                SeatingTorque    = ReadSingle(span, ref pos);
                ClampTorque      = ReadSingle(span, ref pos);
                PrevailingTorque = ReadSingle(span, ref pos);
                SnugTorque       = ReadSingle(span, ref pos);
                // speed angle
                Speed     = ReadUInt16(span, ref pos);
                Angle1    = ReadUInt16(span, ref pos);
                Angle2    = ReadUInt16(span, ref pos);
                Angle     = ReadUInt16(span, ref pos);
                SnugAngle = ReadUInt16(span, ref pos);
                // reserved
                pos += 16;
                // set the barcode
                Barcode = AsciiTrimNullRight(span.Slice(pos, Constants.BarcodeLength));
                // update the position
                pos += Constants.BarcodeLength;
                // get the graph type
                var type1 = ReadUInt16(span, ref pos);
                var type2 = ReadUInt16(span, ref pos);
                // check defined
                if (type1 <= (int)GraphTypes.TorqueAngle)
                    // set the value
                    TypeOfChannel1 = (GraphTypes)type1;
                if (type2 <= (int)GraphTypes.TorqueAngle)
                    // set the value
                    TypeOfChannel2 = (GraphTypes)type2;
                CountOfChannel1 = ReadUInt16(span, ref pos);
                CountOfChannel2 = ReadUInt16(span, ref pos);
                SamplingRate    = ReadUInt16(span, ref pos);
                // check the step count
                for (var i = 0; i < GraphSteps.Length; i++) {
                    // get the step information
                    var id    = ReadUInt16(span, ref pos);
                    var index = ReadUInt16(span, ref pos);
                    // check defined
                    if (id <= (int)GraphStepTypes.RotationAfterTorqueUp)
                        // set the graph step
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    ///     Check the length
    /// </summary>
    /// <param name="remaining">remaining</param>
    /// <param name="need">need</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Ensure(int remaining, int need) {
        // check the length
        if (remaining < need)
            // throw the exception
            throw new InvalidDataException($"Not enough bytes: need {need}, have {remaining}");
    }

    /// <summary>
    ///     Trim the null character
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AsciiTrimNullRight(ReadOnlySpan<byte> s) {
        // get the length
        var end = s.Length;
        // check the null
        while (end > 0 && s[end - 1] == 0)
            // remove the character
            end--;
        // return the valid string
        return end == 0 ? string.Empty : Encoding.ASCII.GetString(s[..end]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ReadSingle(ReadOnlySpan<byte> values, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadUInt32BigEndian(values.Slice(pos, 4));
        // update the position
        pos += 4;
        // return the value
        return BitConverter.Int32BitsToSingle((int)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReadUInt16(ReadOnlySpan<byte> values, ref int pos) {
        // get the value
        var value = BinaryPrimitives.ReadUInt16BigEndian(values.Slice(pos, 2));
        // update the position
        pos += 2;
        // return the value
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReadByte(ReadOnlySpan<byte> values, ref int pos) {
        // return the value
        return values[pos++];
    }

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