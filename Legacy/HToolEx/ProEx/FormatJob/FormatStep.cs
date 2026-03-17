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
    /// <param name="revision">revision</param>
    public FormatStep(byte[] values, int revision = 0) : this() {
        // check revision
        if (revision >= Size.Length)
            return;
        // check data size
        if (values.Length < Size[revision])
            return;
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

        // create values and data
        Values = new byte[Size[revision]];
        Data   = new byte[DataSize[revision]];
        // check data size
        for (var i = 0; i < DataSize[revision]; i++)
            // set value
            Values[HeaderSize + i] = bin.ReadByte();
    }

    /// <summary>
    ///     Step size
    /// </summary>
    [PublicAPI]
    public static int[] Size => [4096, 12288];

    /// <summary>
    ///     Step header size
    /// </summary>
    [PublicAPI]
    public static int HeaderSize => 132;

    /// <summary>
    ///     Step data size
    /// </summary>
    [PublicAPI]
    public static int[] DataSize => [Size[0] - HeaderSize, Size[1] - HeaderSize];

    /// <summary>
    ///     Values
    /// </summary>
    [PublicAPI]
    public byte[] Values { get; private set; } = [];

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
    public byte[] Data { get; private set; } = [];

    /// <summary>
    ///     Get values
    /// </summary>
    /// <param name="revision">revision</param>
    /// <returns>values</returns>
    [PublicAPI]
    public virtual byte[] Get(int revision = 0) {
        // update
        Update(revision);
        // return values
        return Values;
    }

    /// <summary>
    ///     Set values
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    /// <returns>result</returns>
    [PublicAPI]
    public virtual bool Set(byte[] values, int revision = 0) {
        // check size
        if (values.Length != Size[revision])
            return false;
        // create values and data
        Values = new byte[Size[revision]];
        Data   = new byte[DataSize[revision]];
        // check size
        for (var i = 0; i < Size[revision]; i++)
            // set value
            Values[i] = values[i];
        // refresh
        Refresh(revision);
        // result
        return true;
    }

    /// <summary>
    ///     Update values
    ///     <param name="revision">revision</param>
    /// </summary>
    [PublicAPI]
    public virtual void Update(int revision = 0) {
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
    ///     <param name="revision">revision</param>
    /// </summary>
    [PublicAPI]
    public virtual void Refresh(int revision = 0) {
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