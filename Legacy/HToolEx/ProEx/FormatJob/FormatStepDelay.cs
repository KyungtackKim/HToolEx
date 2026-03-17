using System.Text;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step delay data class
/// </summary>
[PublicAPI]
public sealed class FormatStepDelay : FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="revision">revision</param>
    public FormatStepDelay(int revision = 0) : base(new byte[Size[revision]], revision) {
        // set type
        Type = JobStepTypes.Delay;
        // check message length
        for (var i = 0; i < 3; i++)
            // reset message
            Message[i] = string.Empty;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatStepDelay(byte[] values, int revision = 0) : base(values, revision) {
        // set type
        Type = JobStepTypes.Delay;
        // check message length
        for (var i = 0; i < 3; i++)
            // reset message
            Message[i] = string.Empty;
        // set values
        Set(values, revision);
    }

    /// <summary>
    ///     Delay type
    /// </summary>
    public DelayTypes DelayType { get; set; }

    /// <summary>
    ///     Delay time for time mode
    /// </summary>
    public int DelayTime { get; set; }

    /// <summary>
    ///     Delay time for time mode
    /// </summary>
    public DelayTimeUnitTypes DelayTimeUnit { get; set; }

    /// <summary>
    ///     Messages for popup mode
    /// </summary>
    public string[] Message { get; } = new string[3];

    /// <summary>
    ///     Barcode for barcode mode
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     Mask status for barcode mode
    /// </summary>
    public bool IsMask { get; set; }

    /// <summary>
    ///     Mask high value for barcode mode
    /// </summary>
    public uint MaskHigh { get; set; }

    /// <summary>
    ///     Mask low value for barcode mode
    /// </summary>
    public uint MaskLow { get; set; }

    /// <summary>
    ///     Mask value for barcode mode
    /// </summary>
    public ulong Mask => Convert.ToUInt64(((ulong)MaskHigh << 32) | MaskLow);

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
        bin.Write((int)DelayType);
        // check delay type
        switch (DelayType) {
            case DelayTypes.Time:
                // update step data values
                bin.Write(DelayTime);
                bin.Write((int)DelayTimeUnit);
                break;
            case DelayTypes.PopUp:
                // update step data values
                foreach (var msg in Message) {
                    bin.Write(msg.ToCharArray());
                    bin.Write(new byte[128 - msg.Length]);
                }

                break;
            case DelayTypes.Barcode:
                // update step data values
                bin.Write(Code.ToCharArray());
                bin.Write(new byte[68 - Code.Length]);
                bin.Write(Convert.ToInt32(IsMask));
                bin.Write(MaskHigh);
                bin.Write(MaskLow);
                break;
            default:
                return;
        }
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

        // get delay type
        var delay = bin.ReadInt32();
        // check defined
        if (Enum.IsDefined(typeof(DelayTypes), delay))
            // set delay type
            DelayType = (DelayTypes)delay;

        // check delay type
        switch (DelayType) {
            case DelayTypes.Time:
                // set time value
                DelayTime = bin.ReadInt32();
                // get delay time unit
                var unit = bin.ReadInt32();
                // check defined
                if (Enum.IsDefined(typeof(DelayTimeUnitTypes), unit))
                    // set delay type
                    DelayTimeUnit = (DelayTimeUnitTypes)unit;
                break;
            case DelayTypes.PopUp:
                // check message length
                for (var i = 0; i < Message.Length; i++)
                    // set message
                    Message[i] = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
                break;
            case DelayTypes.Barcode:
                // set code
                Code = Encoding.ASCII.GetString(bin.ReadBytes(68)).TrimEnd('\0');
                // set mask status
                IsMask = Convert.ToBoolean(bin.ReadInt32());
                // set mask high
                MaskHigh = bin.ReadUInt32();
                // set mask low
                MaskLow = bin.ReadUInt32();
                break;
            default:
                return;
        }
    }
}