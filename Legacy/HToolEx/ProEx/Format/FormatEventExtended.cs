using System.Globalization;
using System.Text;
using HToolEx.Format;
using HToolEx.Type;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format event data class using Pro-X
/// </summary>
public class FormatEventExtended : FormatEvent {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatEventExtended() {
        // every string member is initialized by its property initializer
    }

    /// <summary>
    ///     Constructor by csv row text
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="msg">error</param>
    /// <param name="hasGraphData">true when graph values are appended after the event columns</param>
    public FormatEventExtended(string values, out string msg, bool hasGraphData = false)
        : base(values, out msg) {
        // check the base event format result
        if (!string.IsNullOrEmpty(msg))
            // stop restoring when the base format failed
            return;

        // split values
        var data = values.Split(',');
        // count of columns that belong to the event row, excluding the appended graph values
        var columns = hasGraphData ? data.Length - (CountOfChannel1 + CountOfChannel2) : data.Length;
        // count of extended columns actually written, which differs by the version that saved the file
        var extended = Math.Min(ExtendCount, columns - Count);
        // check written extended columns
        if (extended < 1)
            // stop restoring when the file holds no extended column
            return;

        // local helper that reads the extended column at the written position
        string Value(int index) => index < extended ? data[Count + index] : string.Empty;

        // restore code.2
        Id2 = Value(0);
        // restore code.3
        Id3 = Value(1);
        // restore code.4
        Id4 = Value(2);
        // restore code.5
        Id5 = Value(3);
        // restore code.6
        Id6 = Value(4);
        // restore job name (rev.1)
        JobName = Value(5);
        // restore step name (rev.1)
        StepName = Value(6);
        // restore tool name (rev.1)
        ToolName = Value(7);
        // restore ng comment (rev.1)
        NgComment = Value(8);
        // restore job id (rev.2)
        JobId = Value(9);
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatEventExtended(byte[] values, string revision = "0.0") : this() {
        // split revision text by dot ("0.{minor}")
        var parts = revision.Split('.');
        // parse minor revision value (0 when missing or non-numeric)
        var minor = parts.Length > 1 && int.TryParse(parts[1], out var n) ? n : 0;
        // per-revision minimum size (Rev.0 1702 / Rev.1 +512 / Rev.2 +128)
        var minSize = ExtendSize + (minor >= 1 ? 512 : 0) + (minor >= 2 ? 128 : 0);
        // check min size
        if (values.Length < minSize)
            return;
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set revision
        Revision = revision;
        // set tool index
        Tool = bin.ReadUInt16();
        // get frame length
        var frame = bin.ReadUInt16();
        // check total frame size
        if (values.Length != frame + 4)
            return;

        // get date-time text
        var dateText = Encoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
        // convert date-time
        if (DateTime.TryParseExact(dateText, "yyyy-MM-dd HH:mm:ss",
                new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out var dateTime)) {
            // set date time
            Date = dateTime;
            Time = dateTime;
        }

        Id         = bin.ReadUInt32();
        FastenTime = bin.ReadUInt16();
        Preset     = bin.ReadUInt16();
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
        TargetTorque     = bin.ReadSingle();
        Torque           = bin.ReadSingle();
        SeatingTorque    = bin.ReadSingle();
        ClampTorque      = bin.ReadSingle();
        PrevailingTorque = bin.ReadSingle();
        SnugTorque       = bin.ReadSingle();
        Speed            = bin.ReadUInt16();
        Angle1           = bin.ReadUInt16();
        Angle2           = bin.ReadUInt16();
        Angle            = bin.ReadUInt16();
        SnugAngle        = bin.ReadUInt16();
        // reserved
        bin.ReadBytes(16);
        // set ids
        IdName1 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id1     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName2 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id2     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName3 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id3     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName4 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id4     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName5 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id5     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName6 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id6     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        // check revision.1
        if (minor >= 1) {
            // set job name
            JobName = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
            // set step name
            StepName = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
            // set tool name
            ToolName = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
            // set ng comment
            NgComment = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        }

        // check revision.2
        if (minor >= 2)
            // set job id
            JobId = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        // get type of channel 1
        var ch1 = bin.ReadUInt16();
        // check type of channel 1
        TypeOfChannel1 = ch1 switch {
            1 => GraphTypes.Torque,
            2 => GraphTypes.Speed,
            3 => GraphTypes.Angle,
            4 => GraphTypes.TorqueAngle,
            _ => GraphTypes.None
        };
        // get type of channel 2
        var ch2 = bin.ReadUInt16();
        // check type of channel 2
        TypeOfChannel2 = ch2 switch {
            1 => GraphTypes.Torque,
            2 => GraphTypes.Speed,
            3 => GraphTypes.Angle,
            _ => GraphTypes.None
        };
        CountOfChannel1 = bin.ReadUInt16();
        CountOfChannel2 = bin.ReadUInt16();
        SamplingRate    = bin.ReadUInt16();

        // check count
        for (var i = 0; i < GraphSteps.Length; i++) {
            // get id/index
            var id    = (GraphStepTypes)bin.ReadUInt16();
            var index = bin.ReadUInt16();
            // check defined id
            if (Enum.IsDefined(typeof(GraphStepTypes), id))
                // set step values
                GraphSteps[i] = new GraphStep(id, index);
        }

        // create value array
        Graph = new Dictionary<int, List<float>> {
            { 0, Enumerable.Range(0, CountOfChannel1).Select(_ => bin.ReadSingle()).ToList() },
            { 1, Enumerable.Range(0, CountOfChannel2).Select(_ => bin.ReadSingle()).ToList() }
        };
        // get check sum
        CheckSum = values.Sum(v => v);
    }

    /// <summary>
    ///     Extended event data min size
    /// </summary>
    [PublicAPI]
    public static int ExtendSize => 1702;

    /// <summary>
    ///     Extended event column count that is
    ///     written after the barcode column in a csv row
    /// </summary>
    [PublicAPI]
    public static int ExtendCount => 10;

    /// <summary>
    ///     Tool number
    /// </summary>
    [PublicAPI]
    public int Tool { get; set; }

    /// <summary>
    ///     ID 1
    /// </summary>
    [PublicAPI]
    public string Id1 { get; set; } = string.Empty;

    /// <summary>
    ///     ID Name 1
    /// </summary>
    [PublicAPI]
    public string IdName1 { get; set; } = string.Empty;

    /// <summary>
    ///     ID 2
    /// </summary>
    [PublicAPI]
    public string Id2 { get; set; } = string.Empty;

    /// <summary>
    ///     ID Name 2
    /// </summary>
    [PublicAPI]
    public string IdName2 { get; set; } = string.Empty;

    /// <summary>
    ///     ID 3
    /// </summary>
    [PublicAPI]
    public string Id3 { get; set; } = string.Empty;

    /// <summary>
    ///     ID Name 3
    /// </summary>
    [PublicAPI]
    public string IdName3 { get; set; } = string.Empty;

    /// <summary>
    ///     ID 4
    /// </summary>
    [PublicAPI]
    public string Id4 { get; set; } = string.Empty;

    /// <summary>
    ///     ID Name 4
    /// </summary>
    [PublicAPI]
    public string IdName4 { get; set; } = string.Empty;

    /// <summary>
    ///     ID 5
    /// </summary>
    [PublicAPI]
    public string Id5 { get; set; } = string.Empty;

    /// <summary>
    ///     ID Name 5
    /// </summary>
    [PublicAPI]
    public string IdName5 { get; set; } = string.Empty;

    /// <summary>
    ///     ID 6
    /// </summary>
    [PublicAPI]
    public string Id6 { get; set; } = string.Empty;

    /// <summary>
    ///     ID Name 6
    /// </summary>
    [PublicAPI]
    public string IdName6 { get; set; } = string.Empty;

    /// <summary>
    ///     Job name (Rev.1)
    /// </summary>
    [PublicAPI]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    ///     Step name (Rev.1)
    /// </summary>
    [PublicAPI]
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    ///     Tool name (Rev.1)
    /// </summary>
    [PublicAPI]
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    ///     NG comment (Rev.1)
    /// </summary>
    [PublicAPI]
    public string NgComment { get; set; } = string.Empty;

    /// <summary>
    ///     Job ID (Rev.2)
    /// </summary>
    [PublicAPI]
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    ///     Graph values
    /// </summary>
    [PublicAPI]
    public Dictionary<int, List<float>> Graph { get; set; } = [];
}