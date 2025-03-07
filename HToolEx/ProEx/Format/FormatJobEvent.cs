using System.ComponentModel;
using System.Globalization;
using System.Text;
using HToolEx.ProEx.Type;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format job event class
/// </summary>
public class FormatJobEvent {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatJobEvent() {
        // reset string properties
        Date = DateTime.MinValue;
        Time = DateTime.MinValue;
        JobName = string.Empty;
        StepName = string.Empty;
#if NOT_USE
        Barcode = string.Empty;
#else
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
        NgCause = string.Empty;
#endif
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatJobEvent(byte[] values) : this() {
        // check size
        if (values.Length != Size)
            return;
        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // get date-time text
        var dateText = Encoding.ASCII.GetString(bin.ReadBytes(20)).TrimEnd('\0');
        // convert date-time
        if (DateTime.TryParseExact(dateText, "yyyy-MM-dd HH:mm:ss",
                new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out var dateTime)) {
            // set date time
            Date = dateTime;
            Time = dateTime;
        }

        // get event type value
        var eventType = bin.ReadByte();
        // check defined unit
        if (Enum.IsDefined(typeof(JobEventTypes), (int)eventType))
            // set event type
            EventType = (JobEventTypes)eventType;
        JobNo = bin.ReadUInt16();
        JobName = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        TotalJobScrews = bin.ReadUInt16();
        JobScrews = bin.ReadUInt16();
        TotalStepCount = bin.ReadUInt16();
        StepNo = bin.ReadUInt16();
        StepName = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        // get step type value
        var stepType = bin.ReadByte();
        // check defined unit
        if (Enum.IsDefined(typeof(JobStepTypes), (int)stepType))
            // set step type
            StepType = (JobStepTypes)stepType;
        TotalStepScrews = bin.ReadUInt16();
        StepScrews = bin.ReadUInt16();
#if NOT_USE
        Barcode = Encoding.ASCII.GetString(bin.ReadBytes(64)).TrimEnd('\0');
#else
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
        NgCause = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
#endif
        Id = bin.ReadUInt32();
    }

    /// <summary>
    ///     Size
    /// </summary>
    [PublicAPI]
    [Browsable(false)]
    public static int Size => 1960;

    /// <summary>
    ///     Id
    /// </summary>
    [PublicAPI]
    public uint Id { get; }

    /// <summary>
    ///     Date
    /// </summary>
    [PublicAPI]
    public DateTime Date { get; }

    /// <summary>
    ///     Time
    /// </summary>
    [PublicAPI]
    public DateTime Time { get; }

    /// <summary>
    ///     Event type
    /// </summary>
    [PublicAPI]
    public JobEventTypes EventType { get; }

    /// <summary>
    ///     Job number
    /// </summary>
    [PublicAPI]
    public int JobNo { get; }

    /// <summary>
    ///     Job name
    /// </summary>
    [PublicAPI]
    public string JobName { get; }

    /// <summary>
    ///     Total job screws
    /// </summary>
    [PublicAPI]
    public int TotalJobScrews { get; }

    /// <summary>
    ///     Job screws
    /// </summary>
    [PublicAPI]
    public int JobScrews { get; }

    /// <summary>
    ///     Total step count
    /// </summary>
    [PublicAPI]
    public int TotalStepCount { get; }

    /// <summary>
    ///     Step number
    /// </summary>
    [PublicAPI]
    public int StepNo { get; }

    /// <summary>
    ///     Step name
    /// </summary>
    [PublicAPI]
    public string StepName { get; }

    /// <summary>
    ///     Step type
    /// </summary>
    [PublicAPI]
    public JobStepTypes StepType { get; }

    /// <summary>
    ///     Total step screws
    /// </summary>
    [PublicAPI]
    public int TotalStepScrews { get; }

    /// <summary>
    ///     Step screws
    /// </summary>
    [PublicAPI]
    public int StepScrews { get; }

#if NOT_USE
    /// <summary>
    ///     Barcode
    /// </summary>
    [PublicAPI]
    public string Barcode { get; }
#else
    /// <summary>
    ///     ID 1
    /// </summary>
    [PublicAPI]
    public string Id1 { get; }

    /// <summary>
    ///     ID Name 1
    /// </summary>
    [PublicAPI]
    public string IdName1 { get; }

    /// <summary>
    ///     ID 2
    /// </summary>
    [PublicAPI]
    public string Id2 { get; }

    /// <summary>
    ///     ID Name 2
    /// </summary>
    [PublicAPI]
    public string IdName2 { get; }

    /// <summary>
    ///     ID 3
    /// </summary>
    [PublicAPI]
    public string Id3 { get; }

    /// <summary>
    ///     ID Name 3
    /// </summary>
    [PublicAPI]
    public string IdName3 { get; }

    /// <summary>
    ///     ID 4
    /// </summary>
    [PublicAPI]
    public string Id4 { get; }

    /// <summary>
    ///     ID Name 4
    /// </summary>
    [PublicAPI]
    public string IdName4 { get; }

    /// <summary>
    ///     ID 5
    /// </summary>
    [PublicAPI]
    public string Id5 { get; }

    /// <summary>
    ///     ID Name 5
    /// </summary>
    [PublicAPI]
    public string IdName5 { get; }

    /// <summary>
    ///     ID 6
    /// </summary>
    [PublicAPI]
    public string Id6 { get; }

    /// <summary>
    ///     ID Name 6
    /// </summary>
    [PublicAPI]
    public string IdName6 { get; }

    /// <summary>
    ///     NG cause
    /// </summary>
    [PublicAPI]
    public string NgCause { get; }
#endif
}