using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;
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
        [Display(Description = @"SetInTypeDisable", ResourceType = typeof(HToolExRes))]
        Disable,
        [Display(Description = @"SetInTypeJob1", ResourceType = typeof(HToolExRes))]
        JobSelect1,
        [Display(Description = @"SetInTypeJob2", ResourceType = typeof(HToolExRes))]
        JobSelect2,
        [Display(Description = @"SetInTypeJob3", ResourceType = typeof(HToolExRes))]
        JobSelect3,
        [Display(Description = @"SetInTypeJob4", ResourceType = typeof(HToolExRes))]
        JobSelect4,
        [Display(Description = @"SetInTypeJob5", ResourceType = typeof(HToolExRes))]
        JobSelect5,
        [Display(Description = @"SetInTypeJob6", ResourceType = typeof(HToolExRes))]
        JobSelect6,
        [Display(Description = @"SetInTypeJob7", ResourceType = typeof(HToolExRes))]
        JobSelect7,
        [Display(Description = @"SetInTypeJob8", ResourceType = typeof(HToolExRes))]
        JobSelect8,
        [Display(Description = @"SetInTypeSkip", ResourceType = typeof(HToolExRes))]
        Skip,
        [Display(Description = @"SetInTypeBack", ResourceType = typeof(HToolExRes))]
        Back,
        [Display(Description = @"SetInTypeStepReset", ResourceType = typeof(HToolExRes))]
        StepReset,
        [Display(Description = @"SetInTypeJobReset", ResourceType = typeof(HToolExRes))]
        JobReset,
        [Display(Description = @"SetInTypeNextJob", ResourceType = typeof(HToolExRes))]
        NextJob,
        [Display(Description = @"SetInTypePrevJob", ResourceType = typeof(HToolExRes))]
        PreviousJob,
        [Display(Description = @"SetInTypeAlarmReset", ResourceType = typeof(HToolExRes))]
        AlarmReset,
        [Display(Description = @"SetInTypeEmergency", ResourceType = typeof(HToolExRes))]
        EmergencyLock
    }

    /// <summary>
    ///     Output function types
    /// </summary>
    public enum SetOutputTypes {
        [Display(Description = @"SetOutTypeDisable", ResourceType = typeof(HToolExRes))]
        Disable,
        [Display(Description = @"SetOutTypeFastenOk", ResourceType = typeof(HToolExRes))]
        FastenOk,
        [Display(Description = @"SetOutTypeFastenNg", ResourceType = typeof(HToolExRes))]
        FastenNg,
        [Display(Description = @"SetOutTypeStepOk", ResourceType = typeof(HToolExRes))]
        StepOk,
        [Display(Description = @"SetOutTypeStepNg", ResourceType = typeof(HToolExRes))]
        StepNg,
        [Display(Description = @"SetOutTypeJobOk", ResourceType = typeof(HToolExRes))]
        JobOk,
        [Display(Description = @"SetOutTypeJobNg", ResourceType = typeof(HToolExRes))]
        JobNg,
        [Display(Description = @"SetOutTypeReady", ResourceType = typeof(HToolExRes))]
        SystemReady,
        [Display(Description = @"SetOutTypeAlarm", ResourceType = typeof(HToolExRes))]
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