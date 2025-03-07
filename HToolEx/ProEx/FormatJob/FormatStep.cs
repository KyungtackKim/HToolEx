using System.Text;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step data class
/// </summary>
public class FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatStep() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatStep(byte[] values) : this() {
        // memory stream
        using var stream = new MemoryStream(values);
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

        // check data size
        for (var i = 0; i < DataSize; i++)
            // set value
            Values[Size - DataSize + i] = bin.ReadByte();
    }

    /// <summary>
    ///     Step size
    /// </summary>
    [PublicAPI]
    public static int Size => 4096;

    /// <summary>
    ///     Step data size
    /// </summary>
    [PublicAPI]
    public static int DataSize => 3964;

    /// <summary>
    ///     Values
    /// </summary>
    [PublicAPI]
    public byte[] Values { get; } = new byte[Size];

    /// <summary>
    ///     Step type
    /// </summary>
    [PublicAPI]
    public JobStepTypes Type { get; set; }

    /// <summary>
    ///     Step name
    /// </summary>
    [PublicAPI]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Step data
    /// </summary>
    [PublicAPI]
    public byte[] Data { get; } = new byte[DataSize];

    /// <summary>
    ///     Get values
    /// </summary>
    /// <returns>values</returns>
    [PublicAPI]
    public virtual byte[] Get() {
        // update
        Update();
        // return values
        return Values;
    }

    /// <summary>
    ///     Set values
    /// </summary>
    /// <param name="values">values</param>
    /// <returns>result</returns>
    [PublicAPI]
    public virtual bool Set(byte[] values) {
        // check size
        if (values.Length != Size)
            return false;

        // check size
        for (var i = 0; i < Size; i++)
            // set value
            Values[i] = values[i];
        // refresh
        Refresh();

        // result
        return true;
    }

    /// <summary>
    ///     Update values
    /// </summary>
    [PublicAPI]
    public virtual void Update() {
        // memory stream
        using var stream = new MemoryStream(Values);
        // binary reader
        using var bin = new BinaryWriter(stream);

        // update step values
        bin.Write((int)Type);
        bin.Write(Name.ToCharArray());
        bin.Write(new byte[128 - Name.Length]);
        bin.Write(Data);
    }

    /// <summary>
    ///     Refresh values
    /// </summary>
    [PublicAPI]
    public virtual void Refresh() {
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
    }
}