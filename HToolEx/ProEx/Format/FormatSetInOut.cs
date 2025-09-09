using System.ComponentModel;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format input/output setting class
/// </summary>
public class FormatSetInOut {
    /// <summary>
    ///     Input function types
    /// </summary>
    public enum SetInputTypes {
        [Description("Disable")]
        Disable,
        [Description("Job selection 1")]
        JobSelect1,
        [Description("Job selection 2")]
        JobSelect2,
        [Description("Job selection 3")]
        JobSelect3,
        [Description("Job selection 4")]
        JobSelect4,
        [Description("Job selection 5")]
        JobSelect5,
        [Description("Job selection 6")]
        JobSelect6,
        [Description("Job selection 7")]
        JobSelect7,
        [Description("Job selection 8")]
        JobSelect8,
        [Description("Skip")]
        Skip,
        [Description("Back")]
        Back,
        [Description("Step reset")]
        StepReset,
        [Description("Job reset")]
        JobReset,
        [Description("Next job")]
        NextJob,
        [Description("Previous job")]
        PreviousJob,
        [Description("Tool alarm reset")]
        AlarmReset,
        [Description("Emergency lock")]
        EmergencyLock
    }

    /// <summary>
    ///     Output function types
    /// </summary>
    public enum SetOutputTypes {
        [Description("Disable")]
        Disable,
        [Description("Fastening OK")]
        FastenOk,
        [Description("Fastening NG")]
        FastenNg,
        [Description("Step OK")]
        StepOk,
        [Description("Step NG")]
        StepNg,
        [Description("Job OK")]
        JobOk,
        [Description("Job NG")]
        JobNg,
        [Description("Ready (Continuous)")]
        SystemReady,
        [Description("Alarm (Continuous)")]
        Alarm
    }

    /// <summary>
    ///     Input/Output port count
    /// </summary>
    [PublicAPI]
    public static readonly int Port = 16;

    /// <summary>
    ///     Input/Output setting size each version
    /// </summary>
    [PublicAPI]
    public static readonly int[] Size = [96];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetInOut() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetInOut(byte[] values, int revision = 0) {
        // check revision
        if (revision >= Size.Length)
            // reset revision
            revision = 0;
        // check size
        if (values.Length < Size[revision])
            return;

        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set check sum
        CheckSum = values.Sum(x => x);
        // check input port count
        for (var i = 0; i < Port; i++)
            // get input function type
            FunctionForInput[i] = Convert.ToInt32(bin.ReadUInt16());
        // check output port count
        for (var i = 0; i < Port; i++)
            // get output function type
            FunctionForOutput[i] = Convert.ToInt32(bin.ReadUInt16());
        // check output port count
        for (var i = 0; i < Port; i++)
            // get output duration time
            DurationForOutput[i] = Convert.ToInt32(bin.ReadUInt16());
    }

    /// <summary>
    ///     Function type for input
    /// </summary>
    [PublicAPI]
    public int[] FunctionForInput { get; private set; } = new int[Port];

    /// <summary>
    ///     Function type for output
    /// </summary>
    [PublicAPI]
    public int[] FunctionForOutput { get; private set; } = new int[Port];

    /// <summary>
    ///     Duration for output
    /// </summary>
    [PublicAPI]
    public int[] DurationForOutput { get; private set; } = new int[Port];

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }

    /// <summary>
    ///     Get values
    /// </summary>
    /// <returns>values</returns>
    [PublicAPI]
    public byte[] GetValues(int revision = 0) {
        var values = new List<byte>();
        // check input port count
        for (var i = 0; i < Port; i++) {
            // get input function values
            values.Add(Convert.ToByte((FunctionForInput[i] >> 8) & 0xFF));
            values.Add(Convert.ToByte(FunctionForInput[i]        & 0xFF));
        }

        // check output port count
        for (var i = 0; i < Port; i++) {
            // get output function values
            values.Add(Convert.ToByte((FunctionForOutput[i] >> 8) & 0xFF));
            values.Add(Convert.ToByte(FunctionForOutput[i]        & 0xFF));
        }

        // check output port count
        for (var i = 0; i < Port; i++) {
            // get output duration values
            values.Add(Convert.ToByte((DurationForOutput[i] >> 8) & 0xFF));
            values.Add(Convert.ToByte(DurationForOutput[i]        & 0xFF));
        }

        // values
        return values.ToArray();
    }
}