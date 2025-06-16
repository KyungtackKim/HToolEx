using System.Net;
using FluentFTP;
using FluentFTP.Rules;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Manager;

/// <summary>
///     FTP management class for ParaMon-Pro X
/// </summary>
public class FtpManager {
    /// <summary>
    ///     FTP transfer delegate
    /// </summary>
    public delegate void FileTransferStatus(FtpProgress progress);

    private const string HostName = "hantas";
    private const string HostPass = "hantas0809";

    /// <summary>
    ///     Constructor
    /// </summary>
    public FtpManager() { }

    private AsyncFtpClient? Client { get; set; }
    private CancellationToken Token { get; } = new();

    /// <summary>
    ///     FTP port number in ParaMon-Pro X
    /// </summary>
    public static int FtpPort => 7762;

    /// <summary>
    ///     Connection state
    /// </summary>
    [PublicAPI]
    public bool IsConnected => Client?.IsConnected ?? false;

    /// <summary>
    ///     Transfer status event
    /// </summary>
    [PublicAPI]
    public FileTransferStatus TransferStatus { get; set; } = default!;

    /// <summary>
    ///     Connect to FTP host
    /// </summary>
    /// <param name="ip">ip address</param>
    /// <param name="port">port</param>
    /// <returns>result</returns>
    [PublicAPI]
    public async Task<(bool result, string error)> Connect(string ip, int port) {
        var res = false;
        // set error
        var error = "Invalid ip address format";
        // check target
        if (!IPAddress.TryParse(ip, out _))
            return (false, error);

        // try catch
        try {
            // reset error
            error = string.Empty;
            // close client
            Disconnect();
            // create client
            Client = new AsyncFtpClient(ip, HostName, HostPass, port);
            // auto connect
            await Client.AutoConnect(Token);

            // set result
            res = true;
        } catch (Exception e) {
            // set error
            error = e.Message;
            // dispose
            Client?.Dispose();
            // clear
            Client = null;
        }

        return (res, error);
    }

    /// <summary>
    ///     Disconnect for FTP client
    /// </summary>
    /// <returns>result</returns>
    [PublicAPI]
    public void Disconnect() {
        // try catch
        try {
            // check client
            if (Client is { IsConnected: true })
                // close
                Client.Disconnect(Token);

            // dispose
            Client?.Dispose();
            // clear
            Client = null;
        } catch (Exception ex) {
            // console
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    ///     Download files in remote directory
    /// </summary>
    /// <param name="local">local path</param>
    /// <param name="remote">remote path</param>
    /// <param name="mode">mode</param>
    /// <param name="block">block list</param>
    /// <returns>result</returns>
    public async Task<bool> DownloadFilesAsync(
        string local,
        string remote,
        FtpFolderSyncMode mode = FtpFolderSyncMode.Mirror,
        List<string>? block = null) {
        // check client
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;

        // try catch
        try {
            // Callback method that accepts a FtpProgress object
            var progress = new Progress<FtpProgress>(x => { TransferStatus(x); });
            // define rules
            var rules = new List<FtpRule>();
            // check block rules
            if (block is { Count: > 0 })
                // create folder name rule
                rules.Add(new FtpFolderNameRule(false, block));
            // download files
            var res = await Client.DownloadDirectory(local, remote, mode, FtpLocalExists.Overwrite,
                FtpVerify.OnlyChecksum, rules, progress, Token);
            // result
            return !res.Any(x => x.IsFailed);
        } catch (Exception e) {
            // debug
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     Upload files in remote directory
    /// </summary>
    /// <param name="local">local path</param>
    /// <param name="remote">remote path</param>
    /// <param name="mode">mode</param>
    /// <param name="exists">exists</param>
    /// <returns>result</returns>
    public async Task<bool> UploadFilesAsync(
        string local,
        string remote,
        FtpFolderSyncMode mode = FtpFolderSyncMode.Update,
        FtpRemoteExists exists = FtpRemoteExists.Overwrite) {
        // check client
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;

        // try catch
        try {
            // Callback method that accepts a FtpProgress object
            var progress = new Progress<FtpProgress>(x => { TransferStatus(x); });
            // upload files
            var res = await Client.UploadDirectory(local, remote, mode, exists, FtpVerify.OnlyChecksum, null, progress,
                Token);
            // result
            return !res.Any(x => x.IsFailed);
        } catch (Exception e) {
            // debug
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     Upload file in remote path
    /// </summary>
    /// <param name="local">local path</param>
    /// <param name="remote">remote path</param>
    /// <param name="exists">exists</param>
    /// <returns>result</returns>
    public async Task<bool> UploadFileAsync(
        string local,
        string remote,
        FtpRemoteExists exists = FtpRemoteExists.Overwrite) {
        // check client
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;

        // try catch
        try {
            // Callback method that accepts a FtpProgress object
            var progress = new Progress<FtpProgress>(x => { TransferStatus(x); });
            // upload file
            var res = await Client.UploadFile(local, remote, exists, true, FtpVerify.OnlyChecksum, progress, Token);
            // result
            return res != FtpStatus.Failed;
        } catch (Exception e) {
            // debug
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     Delete the file
    /// </summary>
    /// <param name="file">file path</param>
    /// <returns>result</returns>
    public async Task<bool> DeleteFileAsync(string file) {
        // check client
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;
        // try catch
        try {
            // delete file
            await Client.DeleteFile(file, Token);
            // result
            return true;
        } catch (Exception e) {
            // debug
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     Set chmod the file
    /// </summary>
    /// <param name="file">file path</param>
    /// <param name="permission">permission</param>
    /// <returns>result</returns>
    public async Task<bool> SetChmodAsync(string file, int permission = 755) {
        // check client
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;

        // try catch
        try {
            // check file exists
            if (!await Client.FileExists(file, Token))
                return false;
            // set chmod
            await Client.Chmod(file, permission, Token);
            // result
            return true;
        } catch (Exception e) {
            // debug
            Console.WriteLine(e.Message);
        }

        return false;
    }

    /// <summary>
    ///     Change file name
    /// </summary>
    /// <param name="file">file path</param>
    /// <param name="newFile">new file path</param>
    /// <returns>result</returns>
    public async Task<bool> ChangeFileName(string file, string newFile) {
        // check client
        if (Client == null)
            return false;
        // check connection state
        if (!IsConnected)
            return false;
        // try catch
        try {
            // check file exists
            if (!await Client.FileExists(file, Token))
                return false;
            // change file name
            await Client.Rename(file, newFile, Token);
            // result
            return true;
        } catch (Exception e) {
            // debug
            Console.WriteLine(e.Message);
        }

        return false;
    }
}