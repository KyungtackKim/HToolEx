using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format share setting class
/// </summary>
public class FormatSetShare {
    /// <summary>
    ///     Share setting size each version
    /// </summary>
    [PublicAPI]
    public static readonly int[] Size = [280, 283];

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetShare() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetShare(byte[] values, int revision = 0) {
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
        FtpServer       = Convert.ToInt32(bin.ReadByte());
        FtpUserName     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        FtpPassword     = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        BackupData      = Convert.ToInt32(bin.ReadByte());
        BackupDataIp    = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        BackupDataPort  = Convert.ToInt32(bin.ReadUInt16());
        ModbusProxy     = Convert.ToInt32(bin.ReadByte());
        ModbusProxyPort = Convert.ToInt32(bin.ReadUInt16());
        TcpClient       = Convert.ToInt32(bin.ReadByte());
        TcpClientIp     = $"{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}.{bin.ReadByte()}";
        TcpClientPort   = Convert.ToInt32(bin.ReadUInt16());
        TcpServer       = Convert.ToInt32(bin.ReadByte());
        TcpServerPort   = Convert.ToInt32(bin.ReadUInt16());
        RemoteJob       = Convert.ToInt32(bin.ReadByte());
        RemoteJobPort   = Convert.ToInt32(bin.ReadUInt16());
        // check revision.1 information
        if (revision < 1)
            return;
        // get revision.1 information
        OpenProtocol     = Convert.ToInt32(bin.ReadByte());
        OpenProtocolPort = Convert.ToInt32(bin.ReadUInt16());
    }

    /// <summary>
    ///     FTP server enable
    /// </summary>
    [PublicAPI]
    public int FtpServer { get; set; }

    /// <summary>
    ///     FTP server user name
    /// </summary>
    [PublicAPI]
    public string FtpUserName { get; set; } = default!;

    /// <summary>
    ///     FTP server password
    /// </summary>
    [PublicAPI]
    public string FtpPassword { get; set; } = default!;

    /// <summary>
    ///     Backup data forward
    /// </summary>
    [PublicAPI]
    public int BackupData { get; set; }

    /// <summary>
    ///     Backup data forward ip address
    /// </summary>
    [PublicAPI]
    public string BackupDataIp { get; set; } = "0.0.0.0";

    /// <summary>
    ///     Backup data forward port number
    /// </summary>
    [PublicAPI]
    public int BackupDataPort { get; set; }

    /// <summary>
    ///     MODBUS proxy server
    /// </summary>
    [PublicAPI]
    public int ModbusProxy { get; set; }

    /// <summary>
    ///     MODBUS proxy server port number
    /// </summary>
    [PublicAPI]
    public int ModbusProxyPort { get; set; }

    /// <summary>
    ///     Remote-Pro X tcp client
    /// </summary>
    [PublicAPI]
    public int TcpClient { get; set; }

    /// <summary>
    ///     Remote-Pro X tcp client ip address
    /// </summary>
    [PublicAPI]
    public string TcpClientIp { get; set; } = "0.0.0.0";

    /// <summary>
    ///     Remote-Pro X tcp client port number
    /// </summary>
    [PublicAPI]
    public int TcpClientPort { get; set; }

    /// <summary>
    ///     Remote-Pro X tcp server
    /// </summary>
    [PublicAPI]
    public int TcpServer { get; set; }

    /// <summary>
    ///     Remote-Pro X tcp server port number
    /// </summary>
    [PublicAPI]
    public int TcpServerPort { get; set; }

    /// <summary>
    ///     Remote job control
    /// </summary>
    [PublicAPI]
    public int RemoteJob { get; set; }

    /// <summary>
    ///     Remote job control port number
    /// </summary>
    [PublicAPI]
    public int RemoteJobPort { get; set; }

    /// <summary>
    ///     Open protocol
    /// </summary>
    [PublicAPI]
    public int OpenProtocol { get; set; }

    /// <summary>
    ///     Open protocol port
    /// </summary>
    [PublicAPI]
    public int OpenProtocolPort { get; set; }

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
        var user     = Encoding.ASCII.GetBytes(FtpUserName).ToList();
        var password = Encoding.ASCII.GetBytes(FtpPassword).ToList();
        // string length offset
        user.AddRange(new byte[128     - FtpUserName.Length]);
        password.AddRange(new byte[128 - FtpPassword.Length]);
        // get ip values
        var backupIp = BackupDataIp.Split('.').Select(byte.Parse);
        var clientIp = TcpClientIp.Split('.').Select(byte.Parse);
        // get revision.0 values
        values.Add(Convert.ToByte(FtpServer));
        values.AddRange(user);
        values.AddRange(password);
        values.Add(Convert.ToByte(BackupData));
        values.AddRange(backupIp);
        values.Add(Convert.ToByte((BackupDataPort >> 8) & 0xFF));
        values.Add(Convert.ToByte(BackupDataPort        & 0xFF));
        values.Add(Convert.ToByte(ModbusProxy));
        values.Add(Convert.ToByte((ModbusProxyPort >> 8) & 0xFF));
        values.Add(Convert.ToByte(ModbusProxyPort        & 0xFF));
        values.Add(Convert.ToByte(TcpClient));
        values.AddRange(clientIp);
        values.Add(Convert.ToByte((TcpClientPort >> 8) & 0xFF));
        values.Add(Convert.ToByte(TcpClientPort        & 0xFF));
        values.Add(Convert.ToByte(TcpServer));
        values.Add(Convert.ToByte((TcpServerPort >> 8) & 0xFF));
        values.Add(Convert.ToByte(TcpServerPort        & 0xFF));
        values.Add(Convert.ToByte(RemoteJob));
        values.Add(Convert.ToByte((RemoteJobPort >> 8) & 0xFF));
        values.Add(Convert.ToByte(RemoteJobPort        & 0xFF));
        // check revision.1 information
        if (revision < 1)
            return values.ToArray();
        // get revision.1 values
        values.Add(Convert.ToByte(OpenProtocol));
        values.Add(Convert.ToByte((OpenProtocolPort >> 8) & 0xFF));
        values.Add(Convert.ToByte(OpenProtocolPort        & 0xFF));
        // values
        return values.ToArray();
    }
}