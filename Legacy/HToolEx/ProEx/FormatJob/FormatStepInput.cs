using System.Text;
using HToolEx.ProEx.Format;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step input data class
/// </summary>
[PublicAPI]
public sealed class FormatStepInput : FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="revision">revision</param>
    public FormatStepInput(int revision = 0) : base(new byte[Size[revision]], revision) {
        // set type
        Type = JobStepTypes.Input;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatStepInput(byte[] values, int revision = 0) : base(values, revision) {
        // set type
        Type = JobStepTypes.Input;
        // set values
        Set(values, revision);
    }

    /// <summary>
    ///     Input port enable status
    /// </summary>
    public bool[] IsPort { get; } = new bool[FormatSetInOut.Port];

    /// <summary>
    ///     Input port signal type
    /// </summary>
    public InputSignalTypes InputType { get; set; }

    /// <summary>
    ///     Update values
    /// </summary>
    /// <param name="revision">revision</param>
    public override void Update(int revision = 0) {
        // memory stream
        using var stream = new MemoryStream(Values);
        // binary reader
        using var bin = new BinaryWriter(stream);

        // update step values
        bin.Write((int)Type);
        bin.Write(Name.ToCharArray());
        bin.Write(new byte[128 - Name.Length]);

        // update step data values
        for (var i = 0; i < FormatSetInOut.Port; i++)
            // set value
            bin.Write(Convert.ToInt32(IsPort[i]));
        bin.Write((int)InputType);
    }

    /// <summary>
    ///     Refresh values
    /// </summary>
    /// <param name="revision">revision</param>
    public override void Refresh(int revision = 0) {
        // memory stream
        using var stream = new MemoryStream(Values);
        // binary reader
        using var bin = new BinaryReader(stream);

        // get type value
        var type = bin.ReadInt32();
        // check defined
        if (Enum.IsDefined(typeof(JobStepTypes), type))
            // set type
            Type = (JobStepTypes)type;
        // set step name
        Name = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');

        // check port count
        for (var i = 0; i < FormatSetInOut.Port; i++)
            // set port status
            IsPort[i] = Convert.ToBoolean(bin.ReadInt32());
        // get input type
        var input = bin.ReadInt32();
        // check defined
        if (Enum.IsDefined(typeof(InputSignalTypes), input))
            // set type
            InputType = (InputSignalTypes)input;
    }
}