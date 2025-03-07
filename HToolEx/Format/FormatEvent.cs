using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HToolEx.Type;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.Format;

/// <summary>
///     Event data format class
/// </summary>
[PublicAPI]
public partial class FormatEvent {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatEvent() {
        // reset string properties
        Revision = "0.0";
        Barcode = string.Empty;
        // check graph step length
        for (var i = 0; i < GraphSteps.Length; i++)
            // add step value
            GraphSteps[i] = new GraphStep(GraphStepTypes.None, 0);
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatEvent(byte[] values) : this() {
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);
        // set information
        Revision = $"{bin.ReadByte()}.{bin.ReadByte()}";
        Id = bin.ReadUInt16();
        Date = new DateTime(bin.ReadUInt16(), bin.ReadByte(), bin.ReadByte(),
            bin.ReadByte(), bin.ReadByte(), bin.ReadByte(), bin.ReadByte());
        Time = Date;
        FastenTime = bin.ReadUInt16();
        Preset = bin.ReadUInt16();
        // get unit value
        var unit = bin.ReadUInt16();
        // check defined unit
        if (Enum.IsDefined(typeof(UnitTypes), (int)unit))
            // set unit
            Unit = (UnitTypes)unit;
        RemainScrew = bin.ReadUInt16();
        // get direction value
        var dir = bin.ReadUInt16();
        // check defined direction
        if (Enum.IsDefined(typeof(DirectionTypes), (int)dir))
            // set direction
            Direction = (DirectionTypes)dir;
        Error = bin.ReadUInt16();
        // get event status value
        var status = bin.ReadUInt16();
        // check defined event status
        if (Enum.IsDefined(typeof(EventTypes), (int)status))
            // set event status
            Event = (EventTypes)status;
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
        Barcode = Encoding.ASCII.GetString(bin.ReadBytes(64)).Trim('\0');
        // get type of channel 1
        var ch1 = bin.ReadUInt16();
        // check defined type
        if (Enum.IsDefined(typeof(GraphTypes), (int)ch1))
            // set type
            TypeOfChannel1 = (GraphTypes)ch1;
        // get type of channel 2
        var ch2 = bin.ReadUInt16();
        // check defined type
        if (Enum.IsDefined(typeof(GraphTypes), (int)ch2))
            // set type
            TypeOfChannel2 = (GraphTypes)ch2;
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

        // get check sum
        CheckSum = values.Sum(v => v);
        // throw an error if not all data has been read
        if (bin.BaseStream.Position != bin.BaseStream.Length)
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{bin.BaseStream.Length - bin.BaseStream.Position} byte(s) remain");
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="msg">error</param>
    public FormatEvent(string values, out string msg) : this() {
        // reset out
        msg = string.Empty;

        // try catch
        try {
            var pos = 0;
            // check values
            if (string.IsNullOrEmpty(values))
                throw new Exception("Invalid values");
            // split values
            var data = values.Split(',');
            // check count
            if (data.Length < Count)
                throw new Exception("Invalid values count");
            // try parse id
            if (!uint.TryParse(data[pos++], out var id))
                throw new Exception("Invalid value : Id");
            // try parse revision
            if (!MyRegex().IsMatch(data[pos++]))
                throw new Exception("Invalid value : Revision");
            // try parse date-time
            if (!DateTime.TryParseExact($"{data[pos++]} {data[pos++]}", "yyyy-MM-dd HH:mm:ss",
                    new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out var dateTime))
                throw new Exception("Invalid value : DateTime");
            // try parse fasten time
            if (!int.TryParse(data[pos++], out var fastenTime))
                throw new Exception("Invalid value : Fasten time");
            // try parse preset
            if (!int.TryParse(data[pos++], out var preset))
                throw new Exception("Invalid value : Preset");
            // try parse unit
            var unit = EnumUtil.GetValue<UnitTypes>(data[pos++]);
            // check unit
            if (!unit.res)
                throw new Exception("Invalid value : Unit");
            // try parse remain screw
            if (!int.TryParse(data[pos++], out var screw))
                throw new Exception("Invalid value : Remain screw");
            // try parse direction
            var dir = EnumUtil.GetValue<DirectionTypes>(data[pos++]);
            // check direction
            if (!dir.res)
                throw new Exception("Invalid value : Direction");
            // try parse error
            if (!int.TryParse(data[pos++], out var error))
                throw new Exception("Invalid value : Error");
            // try parse status
            var status = EnumUtil.GetValue<EventTypes>(data[pos++]);
            // check status
            if (!status.res)
                throw new Exception("Invalid value : Status");
            // try parse target torque
            if (!float.TryParse(data[pos++], out var target))
                throw new Exception("Invalid value : Target torque");
            // try parse torque
            if (!float.TryParse(data[pos++], out var torque))
                throw new Exception("Invalid value : Torque");
            // try parse seating torque
            if (!float.TryParse(data[pos++], out var seating))
                throw new Exception("Invalid value : Seating torque");
            // try parse clamp torque
            if (!float.TryParse(data[pos++], out var clamp))
                throw new Exception("Invalid value : Clamp torque");
            // try parse prevailing torque
            if (!float.TryParse(data[pos++], out var prevailing))
                throw new Exception("Invalid value : Prevailing torque");
            // try parse snug torque
            if (!float.TryParse(data[pos++], out var snugTorque))
                throw new Exception("Invalid value : Snug torque");
            // try parse speed
            if (!int.TryParse(data[pos++], out var speed))
                throw new Exception("Invalid value : Speed");
            // try parse angle 1
            if (!int.TryParse(data[pos++], out var angle1))
                throw new Exception("Invalid value : Angle 1");
            // try parse angle 2
            if (!int.TryParse(data[pos++], out var angle2))
                throw new Exception("Invalid value : Angle 2");
            // try parse angle
            if (!int.TryParse(data[pos++], out var angle))
                throw new Exception("Invalid value : Angle");
            // try parse snug angle
            if (!int.TryParse(data[pos++], out var snugAngle))
                throw new Exception("Invalid value : Snug angle");
            // try parse type of channel 1
            var ch1 = EnumUtil.GetValue<GraphTypes>(data[pos++]);
            // check status
            if (!ch1.res)
                throw new Exception("Invalid value : Type of Channel 1");
            // try parse type of channel 2
            var ch2 = EnumUtil.GetValue<GraphTypes>(data[pos++]);
            // check status
            if (!ch2.res)
                throw new Exception("Invalid value : Type of Channel 1");
            // try parse count of channel 1
            if (!int.TryParse(data[pos++], out var countCh1))
                throw new Exception("Invalid value : Count of Channel 1");
            // try parse count of channel 2
            if (!int.TryParse(data[pos++], out var countCh2))
                throw new Exception("Invalid value : Count of Channel 2");
            // try parse sampling rate
            if (!int.TryParse(data[pos++], out var sample))
                throw new Exception("Invalid value : Sampling rate");
            // get steps
            var steps = data[pos].Split('|');
            // check length
            if (steps.Length / 2 != GraphSteps.Length)
                throw new Exception("Invalid count : Graph steps");

            // check count
            for (var i = 0; i < GraphSteps.Length; i++) {
                // try parse step id
                var stepId = EnumUtil.GetValue<GraphStepTypes>(steps[i * 2]);
                // check status
                if (!stepId.res)
                    throw new Exception($"Invalid value : Graph step type : {i}");
                // try parse step index
                if (!int.TryParse(steps[i * 2 + 1], out var stepIndex))
                    throw new Exception($"Invalid value : Graph step index {i}");
                // set step value
                GraphSteps[i].Id = stepId.type;
                GraphSteps[i].Index = stepIndex;
            }

            // set values
            Id = id;
            Revision = data[1];
            Date = dateTime;
            Time = dateTime;
            FastenTime = fastenTime;
            Preset = preset;
            Unit = unit.type;
            RemainScrew = screw;
            Direction = dir.type;
            Error = error;
            Event = status.type;
            TargetTorque = target;
            Torque = torque;
            SeatingTorque = seating;
            ClampTorque = clamp;
            PrevailingTorque = prevailing;
            SnugTorque = snugTorque;
            Speed = speed;
            Angle1 = angle1;
            Angle2 = angle2;
            Angle = angle;
            SnugAngle = snugAngle;
            TypeOfChannel1 = ch1.type;
            TypeOfChannel2 = ch2.type;
            CountOfChannel1 = countCh1;
            CountOfChannel2 = countCh2;
            SamplingRate = sample;
            Barcode = data[Count - 1];
        }
        catch (Exception e) {
            // set error
            msg = e.Message;
        }
    }

#if NOT_USE
    /// <summary>
    ///     Constructor by ParaMon-Pro X format
    /// </summary>
    /// <param name="src">source</param>
    public FormatEvent(FormatData src) : this() {
        Id = src.Id;
        Revision = src.Revision;
        Date = src.Date;
        Time = src.Time;
        FastenTime = src.FastenTime;
        Preset = src.Preset;
        Unit = src.Unit;
        RemainScrew = src.RemainScrew;
        Direction = src.Direction;
        Error = src.Error;
        Event = src.Event;
        TargetTorque = src.TargetTorque;
        Torque = src.Torque;
        SeatingTorque = src.SeatingTorque;
        ClampTorque = src.ClampTorque;
        PrevailingTorque = src.PrevailingTorque;
        SnugTorque = src.SnugTorque;
        Speed = src.Speed;
        Angle1 = src.Angle1;
        Angle2 = src.Angle2;
        Angle = src.Angle;
        SnugAngle = src.SnugAngle;
        Barcode = src.Barcode;
        TypeOfChannel1 = ToGraphTypes(src.TypeOfChannel1);
        TypeOfChannel2 = ToGraphTypes(src.TypeOfChannel2);
        CountOfChannel1 = src.CountOfChannel1;
        CountOfChannel2 = src.CountOfChannel2;
        SamplingRate = src.SamplingRate;
        // check graph steps
        for (var i = 0; i < src.GraphSteps.Length; i++) {
            // set graph step
            GraphSteps[i].Id = src.GraphSteps[i].Id;
            GraphSteps[i].Index = src.GraphSteps[i].Index;
        }

        // set check sum
        CheckSum = src.CheckSum;
    }
#endif

    /// <summary>
    ///     Format size
    /// </summary>
    [Browsable(false)]
    public static int Size => 214;

    /// <summary>
    ///     Column count
    /// </summary>
    [Browsable(false)]
    public static int Count => 29;

    /// <summary>
    ///     Event id
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    ///     Event revision
    /// </summary>
    public string Revision { get; set; }

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
    public string Barcode { get; set; }

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
    public GraphStep[] GraphSteps { get; } = new GraphStep[16];

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; set; }

    /// <summary>
    ///     Get string data
    /// </summary>
    /// <returns>data</returns>
    public string GetText() {
        // culture information
        var info = new CultureInfo("en-US");
        // string builder
        var sb = new StringBuilder();
        // append values
        sb.Append(Id).Append(',');
        sb.Append(Revision).Append(',');
        sb.Append($"{Date:yyyy-MM-dd}").Append(',');
        sb.Append($"{Time:HH:mm:ss}").Append(',');
        sb.Append(FastenTime).Append(',');
        sb.Append(Preset).Append(',');
        sb.Append(Unit.GetDesc()).Append(',');
        sb.Append(RemainScrew).Append(',');
        sb.Append(Direction.GetDesc()).Append(',');
        sb.Append(Error).Append(',');
        sb.Append(Event.GetDesc()).Append(',');
        sb.Append(TargetTorque.ToString(info)).Append(',');
        sb.Append(Torque.ToString(info)).Append(',');
        sb.Append(SeatingTorque.ToString(info)).Append(',');
        sb.Append(ClampTorque.ToString(info)).Append(',');
        sb.Append(PrevailingTorque.ToString(info)).Append(',');
        sb.Append(SnugTorque.ToString(info)).Append(',');
        sb.Append(Speed).Append(',');
        sb.Append(Angle1).Append(',');
        sb.Append(Angle2).Append(',');
        sb.Append(Angle).Append(',');
        sb.Append(SnugAngle).Append(',');
        sb.Append(TypeOfChannel1.GetDesc()).Append(',');
        sb.Append(TypeOfChannel2.GetDesc()).Append(',');
        sb.Append(CountOfChannel1).Append(',');
        sb.Append(CountOfChannel2).Append(',');
        sb.Append(SamplingRate).Append(',');
        // check steps
        for (var i = 0; i < GraphSteps.Length; i++) {
            sb.Append(GraphSteps[i].Id.GetDesc()).Append('|').Append(GraphSteps[i].Index);
            sb.Append(i < GraphSteps.Length - 1 ? '|' : ',');
        }

        sb.Append(Barcode);

        return $"{sb}";
    }

    [GeneratedRegex("[0-9]?[0-9]?[0-9].[0-9]?[0-9]?[0-9]")]
    private static partial Regex MyRegex();

    /// <summary>
    ///     Convert to graph types from ParaMon-Pro X graph type
    /// </summary>
    /// <param name="type">source</param>
    /// <returns>type</returns>
    public static GraphTypes ToGraphTypes(ProEx.Type.GraphTypes type) {
        return type switch {
            ProEx.Type.GraphTypes.None => GraphTypes.None,
            ProEx.Type.GraphTypes.Torque => GraphTypes.Torque,
            ProEx.Type.GraphTypes.Speed => GraphTypes.Speed,
            ProEx.Type.GraphTypes.Angle => GraphTypes.Angle,
            ProEx.Type.GraphTypes.TorqueAngle => GraphTypes.TorqueAngle,
            _ => GraphTypes.None
        };
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