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
        // reset id and name
        Id1 = string.Empty;
        IdName1 = string.Empty;
        Id2 = string.Empty;
        IdName2 = string.Empty;
        Id3 = string.Empty;
        IdName3 = string.Empty;
        Id4 = string.Empty;
        IdName4 = string.Empty;
        Id5 = string.Empty;
        IdName5 = string.Empty;
        Id6 = string.Empty;
        IdName6 = string.Empty;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatEventExtended(byte[] values, string revision = "0.0") : this() {
        // check min size
        if (values.Length < ExtendSize)
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

        Id = bin.ReadUInt32();
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
        // set ids
        IdName1 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id1 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName2 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id2 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName3 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id3 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName4 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id4 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName5 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id5 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        IdName6 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Id6 = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
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
    ///     Tool number
    /// </summary>
    [PublicAPI]
    public int Tool { get; set; }

    /// <summary>
    ///     ID 1
    /// </summary>
    [PublicAPI]
    public string Id1 { get; set; }

    /// <summary>
    ///     ID Name 1
    /// </summary>
    [PublicAPI]
    public string IdName1 { get; set; }

    /// <summary>
    ///     ID 2
    /// </summary>
    [PublicAPI]
    public string Id2 { get; set; }

    /// <summary>
    ///     ID Name 2
    /// </summary>
    [PublicAPI]
    public string IdName2 { get; set; }

    /// <summary>
    ///     ID 3
    /// </summary>
    [PublicAPI]
    public string Id3 { get; set; }

    /// <summary>
    ///     ID Name 3
    /// </summary>
    [PublicAPI]
    public string IdName3 { get; set; }

    /// <summary>
    ///     ID 4
    /// </summary>
    [PublicAPI]
    public string Id4 { get; set; }

    /// <summary>
    ///     ID Name 4
    /// </summary>
    [PublicAPI]
    public string IdName4 { get; set; }

    /// <summary>
    ///     ID 5
    /// </summary>
    [PublicAPI]
    public string Id5 { get; set; }

    /// <summary>
    ///     ID Name 5
    /// </summary>
    [PublicAPI]
    public string IdName5 { get; set; }

    /// <summary>
    ///     ID 6
    /// </summary>
    [PublicAPI]
    public string Id6 { get; set; }

    /// <summary>
    ///     ID Name 6
    /// </summary>
    [PublicAPI]
    public string IdName6 { get; set; }

    /// <summary>
    ///     Graph values
    /// </summary>
    [PublicAPI]
    public Dictionary<int, List<float>> Graph { get; set; } = [];
}