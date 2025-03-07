using System.Text;
using HToolEx.ProEx.Format;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step output data class
/// </summary>
public sealed class FormatStepOutput : FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatStepOutput() {
        // set type
        Type = JobStepTypes.Output;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatStepOutput(byte[] values) : this() {
        // set values
        Set(values);
    }

    /// <summary>
    ///     summary port enable status
    /// </summary>
    [PublicAPI]
    public bool[] IsPort { get; } = new bool[16];

    /// <summary>
    ///     Output port signal type
    /// </summary>
    [PublicAPI]
    public OutputSignalTypes[] PortType { get; } = new OutputSignalTypes[FormatSetInOut.Port];

    /// <summary>
    ///     Output port signal duration
    /// </summary>
    [PublicAPI]
    public int Duration { get; set; }

    /// <summary>
    ///     Update values
    /// </summary>
    [PublicAPI]
    public override void Update() {
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
        for (var i = 0; i < FormatSetInOut.Port; i++)
            // set value
            bin.Write(Convert.ToInt32(PortType[i]));
        // set duration value
        bin.Write(Duration);
    }

    /// <summary>
    ///     Refresh values
    /// </summary>
    [PublicAPI]
    public override void Refresh() {
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
        // check port count
        for (var i = 0; i < FormatSetInOut.Port; i++) {
            // get output type
            var output = bin.ReadInt32();
            // check defined
            if (Enum.IsDefined(typeof(OutputSignalTypes), output))
                // set type
                PortType[i] = (OutputSignalTypes)output;
        }

        // set duration time
        Duration = bin.ReadInt32();
    }
}