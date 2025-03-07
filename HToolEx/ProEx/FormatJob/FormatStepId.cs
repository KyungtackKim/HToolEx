using System.Text;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step id data class
/// </summary>
[PublicAPI]
public sealed class FormatStepId : FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatStepId() {
        // set type
        Type = JobStepTypes.Id;
        // create array
        Position = new byte[100];
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatStepId(byte[] values) : this() {
        // set values
        Set(values);
    }

    /// <summary>
    ///     Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Operation type
    /// </summary>
    public int Operation { get; set; }

    /// <summary>
    ///     Name of ID
    /// </summary>
    public string NameOfId { get; set; } = string.Empty;

    /// <summary>
    ///     Length
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    ///     Length option
    /// </summary>
    public int LengthOption { get; set; }

    /// <summary>
    ///     Positions
    /// </summary>
    public byte[] Position { get; private set; }

    /// <summary>
    ///     Positions string
    /// </summary>
    public string PositionString {
        get {
            var writer = new StringBuilder();
            // check position length
            for (var i = 0; i < Position.Length; i++)
                // check enabled
                if (Position[i] > 0)
                    // append item
                    writer.AppendFormat($"{i + 1},");
            // check length
            if (writer.Length > 0)
                // remove last comma
                writer.Remove(writer.Length - 1, 1);
            // result
            return writer.ToString();
        }
        set {
            // get data
            var data = value.Split(',');
            // reset all values
            for (var i = 0; i < Position.Length; i++)
                // reset value
                Position[i] = 0x00;
            // check length
            foreach (var t in data)
                // convert value
                if (int.TryParse(t, out var val))
                    // set value
                    Position[val - 1] = 0x01;
        }
    }

    /// <summary>
    ///     Text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///     Source for USB
    /// </summary>
    public int SourceForUsb { get; set; }

    /// <summary>
    ///     Source for Tool
    /// </summary>
    public int SourceForTool { get; set; }

    /// <summary>
    ///     Source for virtual keyboard
    /// </summary>
    public int SourceForVirtualKey { get; set; }

    /// <summary>
    ///     Auto reset end of job
    /// </summary>
    public int AutoResetEndOfJob { get; set; }

    /// <summary>
    ///     Auto skip if empty ID
    /// </summary>
    public int AutoSkipIfEmptyId { get; set; }

    /// <summary>
    ///     Update values
    /// </summary>
    [PublicAPI]
    public override void Update() {
        // memory stream
        using var stream = new MemoryStream(Values);
        // binary reader
        using var bin = new BinaryWriter(stream);

        // get positions
        var pos = new string(PositionString);

        // update step values
        bin.Write((int)Type);
        bin.Write(Name.ToCharArray());
        bin.Write(new byte[128 - Name.Length]);

        // get text value
        var txtNameOfId = Encoding.ASCII.GetBytes(NameOfId);
        var txtText = Encoding.ASCII.GetBytes(Text);
        // update step data values
        bin.Write(Id);
        bin.Write(Operation);
        bin.Write(txtNameOfId);
        bin.Write(new byte[128 - txtNameOfId.Length]);
        bin.Write(Length);
        bin.Write(LengthOption);
        bin.Write(pos.ToCharArray());
        bin.Write(new byte[512 - pos.Length]);
        bin.Write(txtText);
        bin.Write(new byte[128 - txtText.Length]);
        bin.Write(SourceForUsb);
        bin.Write(SourceForTool);
        bin.Write(SourceForVirtualKey);
        bin.Write(AutoResetEndOfJob);
        bin.Write(AutoSkipIfEmptyId);
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

        // set values
        Id = bin.ReadInt32();
        Operation = bin.ReadInt32();
        NameOfId = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        Length = bin.ReadInt32();
        LengthOption = bin.ReadInt32();
        PositionString = Encoding.ASCII.GetString(bin.ReadBytes(512)).TrimEnd('\0');
        Text = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        SourceForUsb = bin.ReadInt32();
        SourceForTool = bin.ReadInt32();
        SourceForVirtualKey = bin.ReadInt32();
        AutoResetEndOfJob = bin.ReadInt32();
        AutoSkipIfEmptyId = bin.ReadInt32();
    }
}