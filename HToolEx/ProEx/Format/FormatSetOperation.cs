using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format operation setting class
/// </summary>
public class FormatSetOperation {
    /// <summary>
    ///     Operation setting size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [178, 220, 221, 504];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetOperation() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetOperation(byte[] values, int revision = 0) {
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
        // get revision.0 information
        WithoutJobMode = Convert.ToInt32(bin.ReadByte());
        DownCountForScrew = Convert.ToInt32(bin.ReadByte());
        JobCountForScrew = Convert.ToInt32(bin.ReadByte());
        DirectJobSelectionMode = Convert.ToInt32(bin.ReadByte());
        RemoteBarcodeMode = Convert.ToInt32(bin.ReadByte());
        ScanBarcodeWhileRun = Convert.ToInt32(bin.ReadByte());
        SkipScrewForReTightFail = Convert.ToInt32(bin.ReadByte());
        OperationMode = Convert.ToInt32(bin.ReadByte());
        JobNameOnBoot = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        SkipWithoutPassword = Convert.ToInt32(bin.ReadByte());
        BackWithoutPassword = Convert.ToInt32(bin.ReadByte());
        ResetWithoutPassword = Convert.ToInt32(bin.ReadByte());
        JobResetButton = Convert.ToInt32(bin.ReadByte());
        JobSelectionWithoutPassword = Convert.ToInt32(bin.ReadByte());
        AutoRestartWhenFinished = Convert.ToInt32(bin.ReadByte());
        AutoDataBackup = Convert.ToInt32(bin.ReadByte());
        BitSocketTray = Convert.ToInt32(bin.ReadByte());
        BitSocketToolName = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        LoadHistoryOnBoot = Convert.ToInt32(bin.ReadByte());
        JobRecovery = Convert.ToInt32(bin.ReadByte());
        // check revision.1
        if (revision < 1)
            return;
        // get revision.1 information
        EnableInOutTool = Convert.ToInt32(bin.ReadByte());
        InOutToolName = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        FastenOkPort = Convert.ToInt32(bin.ReadByte());
        FastenNgPort = Convert.ToInt32(bin.ReadByte());
        Preset1 = Convert.ToInt32(bin.ReadByte());
        Preset2 = Convert.ToInt32(bin.ReadByte());
        Preset3 = Convert.ToInt32(bin.ReadByte());
        Lock = Convert.ToInt32(bin.ReadByte());
        AutoStepForward = Convert.ToInt32(bin.ReadByte());
        SkipByStep = Convert.ToInt32(bin.ReadByte());
        ReTightWithoutPassword = Convert.ToInt32(bin.ReadByte());
        // check revision.2
        if (revision < 2)
            return;
        // get revision.2 information
        Preset4 = Convert.ToInt32(bin.ReadByte());
        // check revision.3
        if (revision < 3)
            return;
        // get revision.3 information
        InOutTool2.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool2.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool2.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool2.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool2.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool2.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool2.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool2.Lock = Convert.ToInt32(bin.ReadByte());
        InOutTool3.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool3.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool3.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool3.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool3.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool3.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool3.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool3.Lock = Convert.ToInt32(bin.ReadByte());
        InOutTool4.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool4.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool4.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool4.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool4.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool4.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool4.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool4.Lock = Convert.ToInt32(bin.ReadByte());
        InOutTool5.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool5.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool5.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool5.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool5.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool5.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool5.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool5.Lock = Convert.ToInt32(bin.ReadByte());
        InOutTool6.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool6.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool6.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool6.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool6.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool6.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool6.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool6.Lock = Convert.ToInt32(bin.ReadByte());
        InOutTool7.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool7.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool7.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool7.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool7.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool7.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool7.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool7.Lock = Convert.ToInt32(bin.ReadByte());
        InOutTool8.Name = Encoding.ASCII.GetString(bin.ReadBytes(32)).TrimEnd('\0');
        InOutTool8.FastenOk = Convert.ToInt32(bin.ReadByte());
        InOutTool8.FastenNg = Convert.ToInt32(bin.ReadByte());
        InOutTool8.Preset1 = Convert.ToInt32(bin.ReadByte());
        InOutTool8.Preset2 = Convert.ToInt32(bin.ReadByte());
        InOutTool8.Preset3 = Convert.ToInt32(bin.ReadByte());
        InOutTool8.Preset4 = Convert.ToInt32(bin.ReadByte());
        InOutTool8.Lock = Convert.ToInt32(bin.ReadByte());
        AbortJobWithoutPass = Convert.ToInt32(bin.ReadByte());
        CommentTheCauseOfStepNg = Convert.ToInt32(bin.ReadByte());
        CommentTheCauseOfJobNg = Convert.ToInt32(bin.ReadByte());
        EditId1WithoutPass = Convert.ToInt32(bin.ReadByte());
        EditId2WithoutPass = Convert.ToInt32(bin.ReadByte());
        EditId3WithoutPass = Convert.ToInt32(bin.ReadByte());
        EditId4WithoutPass = Convert.ToInt32(bin.ReadByte());
        EditId5WithoutPass = Convert.ToInt32(bin.ReadByte());
        EditId6WithoutPass = Convert.ToInt32(bin.ReadByte());
        AllScrewPosAppearAtOnce = Convert.ToInt32(bin.ReadByte());
    }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }

    /// <summary>
    ///     Get values
    /// </summary>
    /// <param name="revision">revision</param>
    /// <returns>values</returns>
    [PublicAPI]
    public byte[] GetValues(int revision = 0) {
        var values = new List<byte>();
        // get string values
        var jobName = Encoding.ASCII.GetBytes(JobNameOnBoot).ToList();
        var bstName = Encoding.ASCII.GetBytes(BitSocketToolName).ToList();
        // string length offset
        jobName.AddRange(new byte[128 - JobNameOnBoot.Length]);
        bstName.AddRange(new byte[32 - BitSocketToolName.Length]);
        // get revision.0 values
        values.Add(Convert.ToByte(WithoutJobMode));
        values.Add(Convert.ToByte(DownCountForScrew));
        values.Add(Convert.ToByte(JobCountForScrew));
        values.Add(Convert.ToByte(DirectJobSelectionMode));
        values.Add(Convert.ToByte(RemoteBarcodeMode));
        values.Add(Convert.ToByte(ScanBarcodeWhileRun));
        values.Add(Convert.ToByte(SkipScrewForReTightFail));
        values.Add(Convert.ToByte(OperationMode));
        values.AddRange(jobName);
        values.Add(Convert.ToByte(SkipWithoutPassword));
        values.Add(Convert.ToByte(BackWithoutPassword));
        values.Add(Convert.ToByte(ResetWithoutPassword));
        values.Add(Convert.ToByte(JobResetButton));
        values.Add(Convert.ToByte(JobSelectionWithoutPassword));
        values.Add(Convert.ToByte(AutoRestartWhenFinished));
        values.Add(Convert.ToByte(AutoDataBackup));
        values.Add(Convert.ToByte(BitSocketTray));
        values.AddRange(bstName);
        values.Add(Convert.ToByte(LoadHistoryOnBoot));
        values.Add(Convert.ToByte(JobRecovery));
        // check revision.1
        if (revision < 1)
            // values
            return values.ToArray();
        // get string values
        var ioName = Encoding.ASCII.GetBytes(InOutToolName).ToList();
        // string length offset
        ioName.AddRange(new byte[32 - InOutToolName.Length]);
        // get revision.1 values
        values.Add(Convert.ToByte(EnableInOutTool));
        values.AddRange(ioName.ToArray());
        values.Add(Convert.ToByte(FastenOkPort));
        values.Add(Convert.ToByte(FastenNgPort));
        values.Add(Convert.ToByte(Preset1));
        values.Add(Convert.ToByte(Preset2));
        values.Add(Convert.ToByte(Preset3));
        values.Add(Convert.ToByte(Lock));
        values.Add(Convert.ToByte(AutoStepForward));
        values.Add(Convert.ToByte(SkipByStep));
        values.Add(Convert.ToByte(ReTightWithoutPassword));
        // check revision.2
        if (revision < 2)
            // values
            return values.ToArray();
        // get revision.2 values
        values.Add(Convert.ToByte(Preset4));
        // check revision.3
        if (revision < 3)
            return values.ToArray();
        // get string values
        var ioName2 = Encoding.ASCII.GetBytes(InOutTool2.Name).ToList();
        var ioName3 = Encoding.ASCII.GetBytes(InOutTool3.Name).ToList();
        var ioName4 = Encoding.ASCII.GetBytes(InOutTool4.Name).ToList();
        var ioName5 = Encoding.ASCII.GetBytes(InOutTool5.Name).ToList();
        var ioName6 = Encoding.ASCII.GetBytes(InOutTool6.Name).ToList();
        var ioName7 = Encoding.ASCII.GetBytes(InOutTool7.Name).ToList();
        var ioName8 = Encoding.ASCII.GetBytes(InOutTool8.Name).ToList();
        // string length offset
        ioName2.AddRange(new byte[32 - InOutTool2.Name.Length]);
        ioName3.AddRange(new byte[32 - InOutTool3.Name.Length]);
        ioName4.AddRange(new byte[32 - InOutTool4.Name.Length]);
        ioName5.AddRange(new byte[32 - InOutTool5.Name.Length]);
        ioName6.AddRange(new byte[32 - InOutTool6.Name.Length]);
        ioName7.AddRange(new byte[32 - InOutTool7.Name.Length]);
        ioName8.AddRange(new byte[32 - InOutTool8.Name.Length]);
        // get revision.3 values
        values.AddRange(ioName2.ToArray());
        values.Add(Convert.ToByte(InOutTool2.FastenOk));
        values.Add(Convert.ToByte(InOutTool2.FastenNg));
        values.Add(Convert.ToByte(InOutTool2.Preset1));
        values.Add(Convert.ToByte(InOutTool2.Preset2));
        values.Add(Convert.ToByte(InOutTool2.Preset3));
        values.Add(Convert.ToByte(InOutTool2.Preset4));
        values.Add(Convert.ToByte(InOutTool2.Lock));
        values.AddRange(ioName3.ToArray());
        values.Add(Convert.ToByte(InOutTool3.FastenOk));
        values.Add(Convert.ToByte(InOutTool3.FastenNg));
        values.Add(Convert.ToByte(InOutTool3.Preset1));
        values.Add(Convert.ToByte(InOutTool3.Preset2));
        values.Add(Convert.ToByte(InOutTool3.Preset3));
        values.Add(Convert.ToByte(InOutTool3.Preset4));
        values.Add(Convert.ToByte(InOutTool3.Lock));
        values.AddRange(ioName4.ToArray());
        values.Add(Convert.ToByte(InOutTool4.FastenOk));
        values.Add(Convert.ToByte(InOutTool4.FastenNg));
        values.Add(Convert.ToByte(InOutTool4.Preset1));
        values.Add(Convert.ToByte(InOutTool4.Preset2));
        values.Add(Convert.ToByte(InOutTool4.Preset3));
        values.Add(Convert.ToByte(InOutTool4.Preset4));
        values.Add(Convert.ToByte(InOutTool4.Lock));
        values.AddRange(ioName5.ToArray());
        values.Add(Convert.ToByte(InOutTool5.FastenOk));
        values.Add(Convert.ToByte(InOutTool5.FastenNg));
        values.Add(Convert.ToByte(InOutTool5.Preset1));
        values.Add(Convert.ToByte(InOutTool5.Preset2));
        values.Add(Convert.ToByte(InOutTool5.Preset3));
        values.Add(Convert.ToByte(InOutTool5.Preset4));
        values.Add(Convert.ToByte(InOutTool5.Lock));
        values.AddRange(ioName6.ToArray());
        values.Add(Convert.ToByte(InOutTool6.FastenOk));
        values.Add(Convert.ToByte(InOutTool6.FastenNg));
        values.Add(Convert.ToByte(InOutTool6.Preset1));
        values.Add(Convert.ToByte(InOutTool6.Preset2));
        values.Add(Convert.ToByte(InOutTool6.Preset3));
        values.Add(Convert.ToByte(InOutTool6.Preset4));
        values.Add(Convert.ToByte(InOutTool6.Lock));
        values.AddRange(ioName7.ToArray());
        values.Add(Convert.ToByte(InOutTool7.FastenOk));
        values.Add(Convert.ToByte(InOutTool7.FastenNg));
        values.Add(Convert.ToByte(InOutTool7.Preset1));
        values.Add(Convert.ToByte(InOutTool7.Preset2));
        values.Add(Convert.ToByte(InOutTool7.Preset3));
        values.Add(Convert.ToByte(InOutTool7.Preset4));
        values.Add(Convert.ToByte(InOutTool7.Lock));
        values.AddRange(ioName8.ToArray());
        values.Add(Convert.ToByte(InOutTool8.FastenOk));
        values.Add(Convert.ToByte(InOutTool8.FastenNg));
        values.Add(Convert.ToByte(InOutTool8.Preset1));
        values.Add(Convert.ToByte(InOutTool8.Preset2));
        values.Add(Convert.ToByte(InOutTool8.Preset3));
        values.Add(Convert.ToByte(InOutTool8.Preset4));
        values.Add(Convert.ToByte(InOutTool8.Lock));
        values.Add(Convert.ToByte(AbortJobWithoutPass));
        values.Add(Convert.ToByte(CommentTheCauseOfStepNg));
        values.Add(Convert.ToByte(CommentTheCauseOfJobNg));
        values.Add(Convert.ToByte(EditId1WithoutPass));
        values.Add(Convert.ToByte(EditId2WithoutPass));
        values.Add(Convert.ToByte(EditId3WithoutPass));
        values.Add(Convert.ToByte(EditId4WithoutPass));
        values.Add(Convert.ToByte(EditId5WithoutPass));
        values.Add(Convert.ToByte(EditId6WithoutPass));
        values.Add(Convert.ToByte(AllScrewPosAppearAtOnce));
        // values
        return values.ToArray();
    }

    #region REVISION

    #region REV.0

    /// <summary>
    ///     Operation mode
    /// </summary>
    [PublicAPI]
    public int WithoutJobMode { get; set; }

    /// <summary>
    ///     Screw count direction
    /// </summary>
    [PublicAPI]
    public int DownCountForScrew { get; set; }

    /// <summary>
    ///     Screw count unit
    /// </summary>
    [PublicAPI]
    public int JobCountForScrew { get; set; }

    /// <summary>
    ///     Job selection type via input
    /// </summary>
    [PublicAPI]
    public int DirectJobSelectionMode { get; set; }

    /// <summary>
    ///     Barcode interface (only without job)
    /// </summary>
    [PublicAPI]
    public int RemoteBarcodeMode { get; set; }

    /// <summary>
    ///     Barcode scan while job running
    /// </summary>
    [PublicAPI]
    [Obsolete("Supported by Rev.0 only.")]
    public int ScanBarcodeWhileRun { get; set; }

    /// <summary>
    ///     ReTightening Failed
    /// </summary>
    [PublicAPI]
    public int SkipScrewForReTightFail { get; set; }

    /// <summary>
    ///     Operation mode on boot
    /// </summary>
    [PublicAPI]
    public int OperationMode { get; set; }

    /// <summary>
    ///     Job name on boot (only with job)
    /// </summary>
    [PublicAPI]
    public string JobNameOnBoot { get; set; } = default!;

    /// <summary>
    ///     Skip button access without password
    /// </summary>
    [PublicAPI]
    public int SkipWithoutPassword { get; set; }

    /// <summary>
    ///     Back button access without password
    /// </summary>
    [PublicAPI]
    public int BackWithoutPassword { get; set; }

    /// <summary>
    ///     Job/Step reset button without password
    /// </summary>
    [PublicAPI]
    public int ResetWithoutPassword { get; set; }

    /// <summary>
    ///     Display job reset button
    /// </summary>
    [PublicAPI]
    public int JobResetButton { get; set; }

    /// <summary>
    ///     Job selection access without password
    /// </summary>
    [PublicAPI]
    public int JobSelectionWithoutPassword { get; set; }

    /// <summary>
    ///     Automatically restart job when finished
    /// </summary>
    [PublicAPI]
    public int AutoRestartWhenFinished { get; set; }

    /// <summary>
    ///     Automatic data backup
    /// </summary>
    [PublicAPI]
    public int AutoDataBackup { get; set; }

    /// <summary>
    ///     Enable Bit Socket Tray
    /// </summary>
    [PublicAPI]
    public int BitSocketTray { get; set; }

    /// <summary>
    ///     Tool name for Bit socket tray
    /// </summary>
    [PublicAPI]
    public string BitSocketToolName { get; set; } = default!;

    /// <summary>
    ///     Load the day's operation history on boot
    /// </summary>
    [PublicAPI]
    public int LoadHistoryOnBoot { get; set; }

    /// <summary>
    ///     Enable job status backup / recovery
    /// </summary>
    [PublicAPI]
    public int JobRecovery { get; set; }

    #endregion

    #region REV.2

    /// <summary>
    ///     Output : Preset 4 port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int Preset4 { get; set; }

    #endregion

    #region REV.1

    /// <summary>
    ///     Enable I/O tool
    /// </summary>
    [PublicAPI]
    public int EnableInOutTool { get; set; }

    /// <summary>
    ///     I/O tool name
    /// </summary>
    [PublicAPI]
    public string InOutToolName { get; set; } = default!;

    /// <summary>
    ///     Input : Fastening OK port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int FastenOkPort { get; set; }

    /// <summary>
    ///     Input : Fastening NG port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int FastenNgPort { get; set; }

    /// <summary>
    ///     Output : Preset 1 port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int Preset1 { get; set; }

    /// <summary>
    ///     Output : Preset 2 port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int Preset2 { get; set; }

    /// <summary>
    ///     Output : Preset 3 port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int Preset3 { get; set; }

    /// <summary>
    ///     Output : Lock port number for I/O tool
    /// </summary>
    [PublicAPI]
    public int Lock { get; set; }

    /// <summary>
    ///     Enable auto step forward
    /// </summary>
    [PublicAPI]
    public int AutoStepForward { get; set; }

    /// <summary>
    ///     Skip by step
    /// </summary>
    [PublicAPI]
    public int SkipByStep { get; set; }

    /// <summary>
    ///     Allow re-tight without password
    /// </summary>
    [PublicAPI]
    public int ReTightWithoutPassword { get; set; }

    #endregion

    #region REV.3

    /// <summary>
    ///     I/O Tool 2
    /// </summary>
    public InOutTool InOutTool2 { get; } = new();

    /// <summary>
    ///     I/O Tool 3
    /// </summary>
    public InOutTool InOutTool3 { get; } = new();

    /// <summary>
    ///     I/O Tool 4
    /// </summary>
    public InOutTool InOutTool4 { get; } = new();

    /// <summary>
    ///     I/O Tool 5
    /// </summary>
    public InOutTool InOutTool5 { get; } = new();

    /// <summary>
    ///     I/O Tool 6
    /// </summary>
    public InOutTool InOutTool6 { get; } = new();

    /// <summary>
    ///     I/O Tool 7
    /// </summary>
    public InOutTool InOutTool7 { get; } = new();

    /// <summary>
    ///     I/O Tool 8
    /// </summary>
    public InOutTool InOutTool8 { get; } = new();

    /// <summary>
    ///     Abort job without password
    /// </summary>
    public int AbortJobWithoutPass { get; set; }

    /// <summary>
    ///     Comment the cause of step ng
    /// </summary>
    public int CommentTheCauseOfStepNg { get; set; }

    /// <summary>
    ///     Comment the cause of job ng
    /// </summary>
    public int CommentTheCauseOfJobNg { get; set; }

    /// <summary>
    ///     Edit id 1 without password
    /// </summary>
    public int EditId1WithoutPass { get; set; }

    /// <summary>
    ///     Edit id 2 without password
    /// </summary>
    public int EditId2WithoutPass { get; set; }

    /// <summary>
    ///     Edit id 3 without password
    /// </summary>
    public int EditId3WithoutPass { get; set; }

    /// <summary>
    ///     Edit id 4 without password
    /// </summary>
    public int EditId4WithoutPass { get; set; }

    /// <summary>
    ///     Edit id 5 without password
    /// </summary>
    public int EditId5WithoutPass { get; set; }

    /// <summary>
    ///     Edit id 6 without password
    /// </summary>
    public int EditId6WithoutPass { get; set; }

    /// <summary>
    ///     All screw position appear at once
    /// </summary>
    public int AllScrewPosAppearAtOnce { get; set; }

    /// <summary>
    /// </summary>
    [PublicAPI]
    public class InOutTool {
        /// <summary>
        ///     Tool name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Input : Fasten OK
        /// </summary>
        public int FastenOk { get; set; }

        /// <summary>
        ///     Input : Fasten NG
        /// </summary>
        public int FastenNg { get; set; }

        /// <summary>
        ///     Output : Preset 1
        /// </summary>
        public int Preset1 { get; set; }

        /// <summary>
        ///     Output : Preset 2
        /// </summary>
        public int Preset2 { get; set; }

        /// <summary>
        ///     Output : Preset 3
        /// </summary>
        public int Preset3 { get; set; }

        /// <summary>
        ///     Output : Preset 4
        /// </summary>
        public int Preset4 { get; set; }

        /// <summary>
        ///     Output : Lock
        /// </summary>
        public int Lock { get; set; }
    }

    #endregion

    #endregion
}