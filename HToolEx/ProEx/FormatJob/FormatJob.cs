using System.ComponentModel;
using System.Text;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Format job header class
/// </summary>
public class FormatJob {
    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatJob() {
        // reset default version
        Major = 1;
        Minor = 0;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    public FormatJob(byte[] values) : this() {
        // set values
        Set(values);
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="headerOnly">header only</param>
    public FormatJob(byte[] values, bool headerOnly) : this() {
        // set values
        Set(values, headerOnly);
    }

    /// <summary>
    ///     Constructor from source
    /// </summary>
    /// <param name="source">source</param>
    public FormatJob(FormatJob source) : this() {
        Major          = source.Major;
        Minor          = source.Minor;
        Index          = source.Index;
        Name           = source.Name;
        CountOfStep    = source.CountOfStep;
        CountOfScrew   = source.CountOfScrew;
        CountOfFasten  = source.CountOfFasten;
        CountOfInput   = source.CountOfInput;
        CountOfOutput  = source.CountOfOutput;
        CountOfDelay   = source.CountOfDelay;
        CountOfMessage = source.CountOfMessage;
        CountOfId      = source.CountOfId;
        FileName       = source.FileName;
        // set revision
        Revision = Major >= 1 ? 1 : 0;
    }

    /// <summary>
    ///     Job header size
    ///     Size가 176 이 아닌 이유는 Job의 헤더를 읽기 전까지는 version 을 알수 없기 때문에 rev.0 기준으로 Size를 체크
    /// </summary>
    [PublicAPI]
    public static int Size => 172;

    /// <summary>
    ///     Signature
    /// </summary>
    [PublicAPI]
    public string Signature { get; init; } = "bmc.job.";

    /// <summary>
    ///     Version.Major
    /// </summary>
    [PublicAPI]
    public int Major { get; set; }

    /// <summary>
    ///     Version.Minor
    /// </summary>
    [PublicAPI]
    public int Minor { get; set; }

    /// <summary>
    ///     Revision number
    /// </summary>
    [PublicAPI]
    public int Revision { get; private set; }

    /// <summary>
    ///     Version
    /// </summary>
    [PublicAPI]
    public string Version => $"{Major}.{Minor}";

    /// <summary>
    ///     Job index
    /// </summary>
    [PublicAPI]
    public int Index { get; set; }

    /// <summary>
    ///     Job name
    /// </summary>
    [PublicAPI]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Total step count
    /// </summary>
    [PublicAPI]
    public int CountOfStep { get; set; }

    /// <summary>
    ///     Total screw count
    /// </summary>
    [PublicAPI]
    public int CountOfScrew { get; set; }

    /// <summary>
    ///     Fasten step count
    /// </summary>
    [PublicAPI]
    public int CountOfFasten { get; set; }

    /// <summary>
    ///     Input step count
    /// </summary>
    [PublicAPI]
    public int CountOfInput { get; set; }

    /// <summary>
    ///     Output step count
    /// </summary>
    [PublicAPI]
    public int CountOfOutput { get; set; }

    /// <summary>
    ///     Delay step count
    /// </summary>
    [PublicAPI]
    public int CountOfDelay { get; set; }

    /// <summary>
    ///     Message step count
    /// </summary>
    [PublicAPI]
    public int CountOfMessage { get; set; }

    /// <summary>
    ///     ID step count
    /// </summary>
    [PublicAPI]
    public int CountOfId { get; set; }

    /// <summary>
    ///     File path and name
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     Get values
    /// </summary>
    /// <returns>values</returns>
    [PublicAPI]
    public byte[] Get() {
        var values = new List<byte>();

        // set signature
        values.AddRange(Encoding.ASCII.GetBytes(Signature));
        // set version
        values.Add(Convert.ToByte(Version.Split('.')[0]));
        values.Add(Convert.ToByte(Version.Split('.')[1]));
        // set reserved
        values.AddRange(new byte[2]);
        // set job index
        values.AddRange(BitConverter.GetBytes(Index - 1));
        // set job name
        values.AddRange(Encoding.ASCII.GetBytes(Name));
        values.AddRange(new byte[128 - Name.Length]);
        // set step count
        values.AddRange(BitConverter.GetBytes(CountOfStep));
        // set screw count
        values.AddRange(BitConverter.GetBytes(CountOfScrew));
        // set fasten step count
        values.AddRange(BitConverter.GetBytes(CountOfFasten));
        // set input step count
        values.AddRange(BitConverter.GetBytes(CountOfInput));
        // set output step count
        values.AddRange(BitConverter.GetBytes(CountOfOutput));
        // set delay step count
        values.AddRange(BitConverter.GetBytes(CountOfDelay));
        // set message step count
        values.AddRange(BitConverter.GetBytes(CountOfMessage));

        // check version
        if (Major >= 1 || (Major == 0 && Minor >= 3))
            // set id step count
            values.AddRange(BitConverter.GetBytes(CountOfId));
        // return values
        return values.ToArray();
    }

    /// <summary>
    ///     Set values
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="headerOnly">header only</param>
    /// <returns>result</returns>
    [PublicAPI]
    public bool Set(byte[] values, bool headerOnly = false) {
        // check length
        if (values.Length < Size)
            return false;

        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReader(stream);

        // get signature
        var sign = Encoding.ASCII.GetString(bin.ReadBytes(8)).TrimEnd('\0');
        // check signature
        if (sign != Signature)
            return false;

        // version
        Major = bin.ReadByte();
        Minor = bin.ReadByte();
        // reserved
        bin.ReadBytes(2);
        // job index
        Index = bin.ReadInt32() + 1;
        // job name
        Name = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        // total step
        CountOfStep = bin.ReadInt32();
        // total screw
        CountOfScrew = bin.ReadInt32();
        // fasten step
        CountOfFasten = bin.ReadInt32();
        // input step
        CountOfInput = bin.ReadInt32();
        // output step
        CountOfOutput = bin.ReadInt32();
        // delay step
        CountOfDelay = bin.ReadInt32();
        // message step
        CountOfMessage = bin.ReadInt32();
        // check version
        if (!headerOnly && (Major >= 1 || (Major == 0 && Minor >= 3)))
            // id step
            CountOfId = bin.ReadInt32();
        // set revision
        Revision = Major >= 1 ? 1 : 0;

        // result
        return true;
    }

    /// <summary>
    ///     Get Job header length
    /// </summary>
    /// <returns></returns>
    public static int GetLength(int major = 0, int minor = 0) {
        // get length
        return major >= 1 || (major == 0 && minor >= 3) ? Size + 4 : Size;
    }
}