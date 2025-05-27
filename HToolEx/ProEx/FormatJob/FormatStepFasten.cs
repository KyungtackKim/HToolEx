using System.Text;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step fasten data class
/// </summary>
[PublicAPI]
public sealed class FormatStepFasten : FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatStepFasten(int revision = 0) : base(new byte[Size[revision]], revision) {
        // set type
        Type = JobStepTypes.Fastening;
        // check screws count
        for (var i = 0; i < MaxScrewCount; i++) {
            // add screw
            Screws[i] = new FormatScrew();
            // add encoder
            Encoders[i] = new FormatEncoder();
        }
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatStepFasten(byte[] values, int revision = 0) : base(values, revision) {
        // set type
        Type = JobStepTypes.Fastening;
        // check screws count
        for (var i = 0; i < MaxScrewCount; i++) {
            // add screw
            Screws[i] = new FormatScrew();
            // add encoder
            Encoders[i] = new FormatEncoder();
        }
        // set values
        Set(values, revision);
    }

    /// <summary>
    ///     Max screw count
    /// </summary>
    public static int MaxScrewCount => 99;

    /// <summary>
    ///     Tool name
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    ///     Preset no.
    /// </summary>
    public int Preset { get; set; }

    /// <summary>
    ///     Count of screws
    /// </summary>
    public int CountOfScrew { get; set; }

    /// <summary>
    ///     Enabled image status
    /// </summary>
    public bool IsImage { get; set; }

    /// <summary>
    ///     Image path
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    ///     Screw position data : total 99 ea.
    /// </summary>
    public FormatScrew[] Screws { get; } = new FormatScrew[MaxScrewCount];

    /// <summary>
    ///     Virtual preset data
    /// </summary>
    public FormatVirtual Virtual { get; } = new();

    /// <summary>
    ///     ReTight status
    /// </summary>
    public bool IsReTight { get; set; }

    /// <summary>
    ///     Max ReTight count
    /// </summary>
    public int MaxReTight { get; set; }

    /// <summary>
    ///     ReTight preset no.
    /// </summary>
    public int ReTightPreset { get; set; }

    /// <summary>
    ///     ReTight virtual preset data
    /// </summary>
    public FormatVirtual ReTightVirtual { get; } = new();

    /// <summary>
    ///     Socket tray number
    /// </summary>
    public int SocketNumber { get; set; }

    /// <summary>
    ///     Update values
    ///     <param name="revision">revision</param>
    /// </summary>
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
        bin.Write(ToolName.ToCharArray());
        bin.Write(new byte[36 - ToolName.Length]);
        bin.Write(Preset);
        bin.Write(CountOfScrew);
        bin.Write(Convert.ToInt32(IsImage));
        bin.Write(ImagePath.ToCharArray());
        bin.Write(new byte[256 - ImagePath.Length]);
        foreach (var screw in Screws) {
            bin.Write(Convert.ToInt32(screw.Enable));
            bin.Write(screw.X);
            bin.Write(screw.Y);
            bin.Write(screw.Radius);
            bin.Write(screw.Thickness);
            bin.Write(screw.Default.R);
            bin.Write(screw.Default.G);
            bin.Write(screw.Default.B);
            bin.Write(screw.Ok.R);
            bin.Write(screw.Ok.G);
            bin.Write(screw.Ok.B);
            bin.Write(screw.Ng.R);
            bin.Write(screw.Ng.G);
            bin.Write(screw.Ng.B);
            bin.Write(screw.Spare);
        }

        bin.Write(Convert.ToInt32(Virtual.Enable));
        bin.Write(Virtual.Fasten);
        bin.Write(Virtual.Advance);
        bin.Write(Convert.ToInt32(IsReTight));
        bin.Write(MaxReTight);
        bin.Write(ReTightPreset);
        bin.Write(Convert.ToInt32(ReTightVirtual.Enable));
        bin.Write(ReTightVirtual.Fasten);
        bin.Write(ReTightVirtual.Advance);
        bin.Write(SocketNumber);
        // check revision
        switch (revision) {
            case 1:
                bin.Write(Convert.ToInt32(EnableEncoder));
                bin.Write(Convert.ToInt32(EnableNonSeq));
                foreach (var enc in Encoders) {
                    bin.Write(Convert.ToInt32(enc.SavePos[0]));
                    bin.Write(Convert.ToInt32(enc.SavePos[1]));
                    bin.Write(Convert.ToInt32(enc.SavePos[2]));
                    bin.Write(Convert.ToInt32(enc.SavePos[3]));
                    bin.Write(Convert.ToInt32(enc.ZoneTol[0]));
                    bin.Write(Convert.ToInt32(enc.ZoneTol[1]));
                    bin.Write(Convert.ToInt32(enc.ZoneTol[2]));
                    bin.Write(Convert.ToInt32(enc.ZoneTol[3]));
                    bin.Write(Convert.ToInt32(enc.OkTol[0]));
                    bin.Write(Convert.ToInt32(enc.OkTol[1]));
                    bin.Write(Convert.ToInt32(enc.OkTol[2]));
                    bin.Write(Convert.ToInt32(enc.OkTol[3]));
                    bin.Write(Convert.ToInt32(enc.EnabledPickUp[0]));
                    bin.Write(Convert.ToInt32(enc.EnabledPickUp[1]));
                }

                break;
        }
    }

    /// <summary>
    ///     Refresh values
    ///     <param name="revision">revision</param>
    /// </summary>
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

        // set tool name
        ToolName = Encoding.ASCII.GetString(bin.ReadBytes(36)).TrimEnd('\0');
        // set preset
        Preset = bin.ReadInt32();
        // set screw count
        CountOfScrew = bin.ReadInt32();
        // set image status
        IsImage = Convert.ToBoolean(bin.ReadInt32());
        // set image path
        ImagePath = Encoding.ASCII.GetString(bin.ReadBytes(256)).TrimEnd('\0');

        // check max screw count
        for (var i = 0; i < MaxScrewCount; i++) {
            // get screw
            var screw = Screws[i];
            // set screw status
            screw.Enable = Convert.ToBoolean(bin.ReadInt32());
            // set x position
            screw.X = bin.ReadUInt32();
            // set y position
            screw.Y = bin.ReadUInt32();
            // set radius
            screw.Radius = bin.ReadUInt32();
            // set thickness
            screw.Thickness = bin.ReadUInt32();
            // set default color
            screw.Default = Color.FromArgb(0xFF, bin.ReadByte(), bin.ReadByte(), bin.ReadByte());
            // set ok color
            screw.Ok = Color.FromArgb(0xFF, bin.ReadByte(), bin.ReadByte(), bin.ReadByte());
            // set ng color
            screw.Ng = Color.FromArgb(0xFF, bin.ReadByte(), bin.ReadByte(), bin.ReadByte());
            // set spare
            bin.ReadBytes(screw.Spare.Length);
        }

        // set virtual status
        Virtual.Enable = Convert.ToBoolean(bin.ReadInt32());
        // get fasten data
        var fasten = bin.ReadBytes(Virtual.Fasten.Length);
        // get advance data
        var advance = bin.ReadBytes(Virtual.Advance.Length);
        // check data length
        for (var i = 0; i < Virtual.Fasten.Length; i++)
            // set data
            Virtual.Fasten[i] = fasten[i];
        // check data length
        for (var i = 0; i < Virtual.Advance.Length; i++)
            // set data
            Virtual.Advance[i] = advance[i];

        // set re-tight status
        IsReTight = Convert.ToBoolean(bin.ReadInt32());
        // set re-tight max count
        MaxReTight = bin.ReadInt32();
        // set re-tight preset
        ReTightPreset = bin.ReadInt32();

        // set re-tight virtual status
        ReTightVirtual.Enable = Convert.ToBoolean(bin.ReadInt32());
        // get fasten data
        var reFasten = bin.ReadBytes(ReTightVirtual.Fasten.Length);
        // get advance data
        var reAdvance = bin.ReadBytes(ReTightVirtual.Advance.Length);
        // check data length
        for (var i = 0; i < ReTightVirtual.Fasten.Length; i++)
            // set data
            ReTightVirtual.Fasten[i] = reFasten[i];
        // check data length
        for (var i = 0; i < ReTightVirtual.Advance.Length; i++)
            // set data
            ReTightVirtual.Advance[i] = reAdvance[i];

        // set socket tray number
        SocketNumber = bin.ReadInt32();

        // check revision
        switch (revision) {
            case 1:
                // set encoder option
                EnableEncoder = Convert.ToBoolean(bin.ReadInt32());
                EnableNonSeq = Convert.ToBoolean(bin.ReadInt32());
                // check screw count
                for (var i = 0; i < MaxScrewCount; i++) {
                    // get encoder
                    var enc = Encoders[i];
                    // set encoder values
                    enc.SavePos[0] = bin.ReadInt32();
                    enc.SavePos[1] = bin.ReadInt32();
                    enc.SavePos[2] = bin.ReadInt32();
                    enc.SavePos[3] = bin.ReadInt32();
                    enc.ZoneTol[0] = bin.ReadInt32();
                    enc.ZoneTol[1] = bin.ReadInt32();
                    enc.ZoneTol[2] = bin.ReadInt32();
                    enc.ZoneTol[3] = bin.ReadInt32();
                    enc.OkTol[0] = bin.ReadInt32();
                    enc.OkTol[1] = bin.ReadInt32();
                    enc.OkTol[2] = bin.ReadInt32();
                    enc.OkTol[3] = bin.ReadInt32();
                    enc.EnabledPickUp[0] = Convert.ToBoolean(bin.ReadInt32());
                    enc.EnabledPickUp[1] = Convert.ToBoolean(bin.ReadInt32());
                }

                break;
        }
    }

    #region REVISION

    #region REV.1.0

    /// <summary>
    ///     Enabled encoder
    /// </summary>

    public bool EnableEncoder { get; set; }

    /// <summary>
    ///     Enabled non-sequence
    /// </summary>

    public bool EnableNonSeq { get; set; }

    /// <summary>
    ///     Encoder data
    /// </summary>

    public FormatEncoder[] Encoders { get; } = new FormatEncoder[MaxScrewCount];

    #endregion

    #endregion
}