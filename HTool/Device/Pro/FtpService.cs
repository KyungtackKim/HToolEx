using System.Diagnostics;
using FluentFTP;
using FluentFTP.Rules;
using HTool.Type;

namespace HTool.Device.Pro;

/// <summary>
///     PRO X 게이트웨이의 FTP 파일 전송 서비스.
///     Lazy 연결을 사용하여 첫 FTP 작업 시 자동으로 연결된다.
///     FTP file transfer service for PRO X gateway.
///     Uses lazy connection — automatically connects on first FTP operation.
/// </summary>
public sealed class FtpService(HToolLogger logger, string ip, int port) : IDisposable {
    // FTP 사용자 이름
    // FTP username
    private const string UserName = "hantas";

    // FTP 비밀번호
    // FTP password
    private const string Password = "hantas0809";

    /// <summary>
    ///     기본 FTP 포트.
    ///     Default FTP port.
    /// </summary>
    public const int DefaultPort = 7762;

    // FTP 클라이언트 인스턴스
    // FTP client instance
    private AsyncFtpClient? _client;

    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;

    /// <summary>
    ///     FTP 연결 상태.
    ///     FTP connection state.
    /// </summary>
    public bool IsConnected => _client?.IsConnected ?? false;

    /// <summary>
    ///     동기화 상태.
    ///     Synchronization state.
    /// </summary>
    public FtpSyncState SyncState { get; private set; }

    /// <inheritdoc />
    public void Dispose() {
        // 이중 해제 방지
        // prevent double disposal
        if (_disposed)
            // 이미 해제됨 — 반환
            // already disposed — return
            return;
        // 해제됨으로 표시
        // mark as disposed
        _disposed = true;
        // 연결 해제
        // disconnect
        Disconnect();
    }

    /// <summary>
    ///     동기화 완료 이벤트.
    ///     Synchronization completed event.
    /// </summary>
    public event Action? SyncCompleted;

    /// <summary>
    ///     FTP 연결을 보장한다. 미연결 시 자동으로 연결한다.
    ///     Ensures FTP connection. Automatically connects if not connected.
    /// </summary>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    internal async Task EnsureConnectedAsync(CancellationToken ct = default) {
        // 이미 연결 상태이면 건너뜀
        // skip if already connected
        if (IsConnected)
            // 이미 연결됨 — 반환
            // already connected — return
            return;

        // 기존 클라이언트 정리
        // clean up existing client
        _client?.Dispose();
        // 새 FTP 클라이언트 생성
        // create new FTP client
        _client = new AsyncFtpClient(ip, UserName, Password, port);
        // 연결 시도 로그
        // log connection attempt
        logger.Log(LogCategories.Ftp, LogLevel.Info, $"FTP connecting: {ip}:{port}");
        // FTP 서버에 연결
        // connect to FTP server
        await _client.Connect(ct).ConfigureAwait(false);
        // 연결 성공 로그
        // log connection success
        logger.Log(LogCategories.Ftp, LogLevel.Info, "FTP connected");
    }

    /// <summary>
    ///     FTP 연결을 해제한다.
    ///     Disconnects from the FTP server.
    /// </summary>
    public void Disconnect() {
        // 가드: FTP 해제 오류 처리
        // guard: handle FTP disconnect errors
        try {
            // 연결 상태 확인
            // check connection state
            if (_client is { IsConnected: true })
                // FTP 연결 해제
                // disconnect FTP
                _client.Disconnect();
            // 클라이언트 해제
            // dispose client
            _client?.Dispose();
            // 참조 제거
            // clear reference
            _client = null;
            // 해제 로그
            // log disconnect
            logger.Log(LogCategories.Ftp, LogLevel.Info, "FTP disconnected");
        } catch (Exception ex) {
            // FTP 해제 중 오류 발생
            // error during FTP disconnect
            logger.Log(LogCategories.Ftp, LogLevel.Warning, $"FTP disconnect error: {ex.Message}");
        }
    }

    /// <summary>
    ///     원격 디렉토리를 로컬로 동기화한다 (Mirror 모드, 캐시 유지).
    ///     Synchronizes a remote directory to local (Mirror mode, cache retained).
    /// </summary>
    /// <param name="localPath">로컬 경로 / local path</param>
    /// <param name="remotePath">원격 경로 / remote path</param>
    /// <param name="excludeFolders">제외할 폴더 목록 (없으면 null) / folders to exclude (null if none)</param>
    /// <param name="progress">진행 보고 (없으면 null) / progress reporter (null if none)</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>동기화 결과 / synchronization result</returns>
    public async Task<FtpSyncResult> SyncDirectoryAsync(
        string                  localPath,
        string                  remotePath,
        List<string>?           excludeFolders = null,
        IProgress<FtpProgress>? progress       = null,
        CancellationToken       ct             = default) {
        // 시작 시각 기록
        // record start time
        var sw = Stopwatch.StartNew();
        // 동기화 상태를 진행 중으로 변경
        // set sync state to syncing
        SyncState = FtpSyncState.Syncing;

        // 가드: FTP 동기화 오류 처리
        // guard: handle FTP sync errors
        try {
            // FTP 연결 보장
            // ensure FTP connection
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            // 동기화 시작 로그
            // log sync start
            logger.Log(LogCategories.Ftp, LogLevel.Info, $"Sync start: {remotePath} → {localPath}");

            // 제외 규칙 생성
            // build exclusion rules
            var rules = new List<FtpRule>();
            // 제외 폴더가 있으면 규칙 추가
            // add exclusion rule if folders specified
            if (excludeFolders is { Count: > 0 })
                // 폴더 이름 제외 규칙 추가
                // add folder name exclusion rule
                rules.Add(new FtpFolderNameRule(false, excludeFolders));

            // Mirror 모드로 디렉토리 동기화
            // sync directory with Mirror mode
            var results = await _client!.DownloadDirectory(
                localPath, remotePath,
                FtpFolderSyncMode.Mirror, FtpLocalExists.Overwrite,
                FtpVerify.OnlyVerify, rules,
                progress, ct
            ).ConfigureAwait(false);

            // 타이머 정지
            // stop timer
            sw.Stop();
            // 업데이트된 파일 수 집계
            // count updated files
            var updated = results.Count(static r => r.IsSuccess);
            // 건너뛴 파일 수 집계
            // count skipped files
            var skipped = results.Count(static r => r.IsSkipped);
            // 실패 여부 확인
            // check for failures
            var failed = results.Any(static r => r.IsFailed);

            // 동기화 상태 갱신
            // update sync state
            SyncState = failed ? FtpSyncState.Failed : FtpSyncState.Synced;
            // 동기화 결과 로그
            // log sync result
            logger.Log(LogCategories.Ftp, failed ? LogLevel.Error : LogLevel.Info,
                $"Sync done: updated={updated} skipped={skipped} failed={failed} elapsed={sw.Elapsed.TotalSeconds:F1}s");
            // 동기화 완료 이벤트 발생
            // raise sync completed event
            SyncCompleted?.Invoke();
            // 결과 반환
            // return result
            return new FtpSyncResult(!failed, updated, skipped, sw.Elapsed);
        } catch (Exception ex) {
            // FTP 동기화 실패
            // FTP sync failed
            sw.Stop();
            // 동기화 상태를 실패로 변경
            // set sync state to failed
            SyncState = FtpSyncState.Failed;
            // 동기화 실패 로그
            // log sync failure
            logger.Log(LogCategories.Ftp, LogLevel.Error, $"Sync failed: {ex.Message}");
            // 동기화 완료 이벤트 발생
            // raise sync completed event
            SyncCompleted?.Invoke();
            // 실패 결과 반환
            // return failure result
            return new FtpSyncResult(false, 0, 0, sw.Elapsed);
        }
    }

    /// <summary>
    ///     로컬 디렉토리를 원격에 업로드한다.
    ///     Uploads a local directory to remote.
    /// </summary>
    /// <param name="localPath">로컬 경로 / local path</param>
    /// <param name="remotePath">원격 경로 / remote path</param>
    /// <param name="progress">진행 보고 (없으면 null) / progress reporter (null if none)</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>업로드 성공 여부 / whether the upload succeeded</returns>
    public async Task<bool> UploadDirectoryAsync(
        string                  localPath,
        string                  remotePath,
        IProgress<FtpProgress>? progress = null,
        CancellationToken       ct       = default) {
        // 가드: FTP 디렉토리 업로드 오류 처리
        // guard: handle FTP directory upload errors
        try {
            // FTP 연결 보장
            // ensure FTP connection
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            // 디렉토리 업로드 로그
            // log directory upload
            logger.Log(LogCategories.Ftp, LogLevel.Info, $"Upload directory: {localPath} → {remotePath}");
            // 디렉토리 업로드
            // upload directory
            var results = await _client!.UploadDirectory(
                localPath, remotePath,
                FtpFolderSyncMode.Update, FtpRemoteExists.Overwrite,
                FtpVerify.OnlyVerify, null, progress, ct
            ).ConfigureAwait(false);
            // 실패 여부 확인
            // check for failures
            var success = !results.Any(static r => r.IsFailed);
            // 업로드 결과 로그
            // log upload result
            logger.Log(LogCategories.Ftp, success ? LogLevel.Info : LogLevel.Error, $"Upload directory {(success ? "done" : "failed")}");
            // 결과 반환
            // return result
            return success;
        } catch (Exception ex) {
            // FTP 디렉토리 업로드 실패
            // FTP directory upload failed
            logger.Log(LogCategories.Ftp, LogLevel.Error, $"Upload directory failed: {ex.Message}");
            // 실패 반환
            // return failure
            return false;
        }
    }

    /// <summary>
    ///     단일 파일을 원격에 업로드한다.
    ///     Uploads a single file to remote.
    /// </summary>
    /// <param name="localPath">로컬 파일 경로 / local file path</param>
    /// <param name="remotePath">원격 파일 경로 / remote file path</param>
    /// <param name="progress">진행 보고 (없으면 null) / progress reporter (null if none)</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>업로드 성공 여부 / whether the upload succeeded</returns>
    public async Task<bool> UploadFileAsync(
        string                  localPath,
        string                  remotePath,
        IProgress<FtpProgress>? progress = null,
        CancellationToken       ct       = default) {
        // 가드: FTP 파일 업로드 오류 처리
        // guard: handle FTP file upload errors
        try {
            // FTP 연결 보장
            // ensure FTP connection
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            // 파일 업로드 로그
            // log file upload
            logger.Log(LogCategories.Ftp, LogLevel.Info, $"Upload file: {localPath} → {remotePath}");
            // 파일 업로드
            // upload file
            var status = await _client!.UploadFile(
                localPath, remotePath,
                FtpRemoteExists.Overwrite, true,
                FtpVerify.OnlyVerify, progress, ct
            ).ConfigureAwait(false);
            // 결과 확인
            // check result
            var success = status is not FtpStatus.Failed;
            // 업로드 결과 로그
            // log upload result
            logger.Log(LogCategories.Ftp, success ? LogLevel.Info : LogLevel.Error, $"Upload file {(success ? "done" : "failed")}");
            // 결과 반환
            // return result
            return success;
        } catch (Exception ex) {
            // FTP 파일 업로드 실패
            // FTP file upload failed
            logger.Log(LogCategories.Ftp, LogLevel.Error, $"Upload file failed: {ex.Message}");
            // 실패 반환
            // return failure
            return false;
        }
    }

    /// <summary>
    ///     원격 파일을 삭제한다.
    ///     Deletes a remote file.
    /// </summary>
    /// <param name="remotePath">원격 파일 경로 / remote file path</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>삭제 성공 여부 / whether the deletion succeeded</returns>
    public async Task<bool> DeleteFileAsync(string remotePath, CancellationToken ct = default) {
        // 가드: FTP 파일 삭제 오류 처리
        // guard: handle FTP file deletion errors
        try {
            // FTP 연결 보장
            // ensure FTP connection
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            // 파일 삭제 로그
            // log file deletion
            logger.Log(LogCategories.Ftp, LogLevel.Info, $"Delete file: {remotePath}");
            // 파일 삭제
            // delete file
            await _client!.DeleteFile(remotePath, ct).ConfigureAwait(false);
            // 삭제 성공 반환
            // return deletion success
            return true;
        } catch (Exception ex) {
            // FTP 파일 삭제 실패
            // FTP file deletion failed
            logger.Log(LogCategories.Ftp, LogLevel.Error, $"Delete file failed: {ex.Message}");
            // 실패 반환
            // return failure
            return false;
        }
    }

    /// <summary>
    ///     원격 파일 이름을 변경한다.
    ///     Renames a remote file.
    /// </summary>
    /// <param name="remotePath">현재 원격 경로 / current remote path</param>
    /// <param name="newRemotePath">새 원격 경로 / new remote path</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>이름 변경 성공 여부 / whether the rename succeeded</returns>
    public async Task<bool> RenameFileAsync(
        string            remotePath,
        string            newRemotePath,
        CancellationToken ct = default) {
        // 가드: FTP 파일 이름 변경 오류 처리
        // guard: handle FTP file rename errors
        try {
            // FTP 연결 보장
            // ensure FTP connection
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            // 파일 존재 여부 확인
            // check if file exists
            if (!await _client!.FileExists(remotePath, ct).ConfigureAwait(false)) {
                // 파일이 존재하지 않음 — 건너뜀
                // file does not exist — skip
                logger.Log(LogCategories.Ftp, LogLevel.Warning, $"Rename skipped: {remotePath} not found");
                // 실패 반환
                // return failure
                return false;
            }

            // 파일 이름 변경 로그
            // log file rename
            logger.Log(LogCategories.Ftp, LogLevel.Info, $"Rename: {remotePath} → {newRemotePath}");
            // 파일 이름 변경
            // rename file
            await _client.Rename(remotePath, newRemotePath, ct).ConfigureAwait(false);
            // 이름 변경 성공 반환
            // return rename success
            return true;
        } catch (Exception ex) {
            // FTP 파일 이름 변경 실패
            // FTP file rename failed
            logger.Log(LogCategories.Ftp, LogLevel.Error, $"Rename failed: {ex.Message}");
            // 실패 반환
            // return failure
            return false;
        }
    }

    /// <summary>
    ///     원격 파일의 권한을 설정한다.
    ///     Sets permissions on a remote file.
    /// </summary>
    /// <param name="remotePath">원격 파일 경로 / remote file path</param>
    /// <param name="permission">권한 값 (기본 755) / permission value (default 755)</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>권한 설정 성공 여부 / whether the chmod succeeded</returns>
    public async Task<bool> SetChmodAsync(
        string            remotePath,
        int               permission = 755,
        CancellationToken ct         = default) {
        // 가드: FTP 권한 설정 오류 처리
        // guard: handle FTP chmod errors
        try {
            // FTP 연결 보장
            // ensure FTP connection
            await EnsureConnectedAsync(ct).ConfigureAwait(false);
            // 파일 존재 여부 확인
            // check if file exists
            if (!await _client!.FileExists(remotePath, ct).ConfigureAwait(false)) {
                // 파일이 존재하지 않음 — 건너뜀
                // file does not exist — skip
                logger.Log(LogCategories.Ftp, LogLevel.Warning, $"Chmod skipped: {remotePath} not found");
                // 실패 반환
                // return failure
                return false;
            }

            // 권한 설정 로그
            // log chmod
            logger.Log(LogCategories.Ftp, LogLevel.Info, $"Chmod: {remotePath} → {permission}");
            // 권한 설정
            // set chmod
            await _client.Chmod(remotePath, permission, ct).ConfigureAwait(false);
            // 권한 설정 성공 반환
            // return chmod success
            return true;
        } catch (Exception ex) {
            // FTP 권한 설정 실패
            // FTP chmod failed
            logger.Log(LogCategories.Ftp, LogLevel.Error, $"Chmod failed: {ex.Message}");
            // 실패 반환
            // return failure
            return false;
        }
    }

    /// <summary>
    ///     펌웨어를 원자적으로 배포한다 (Upload → Chmod → Delete → Rename).
    ///     4단계를 단일 메서드로 원자 실행하여 중간 실패 시 조기 종료한다.
    ///     Deploys firmware atomically (Upload → Chmod → Delete → Rename).
    ///     Executes 4 steps as a single atomic operation, aborting on intermediate failure.
    /// </summary>
    /// <param name="localPath">로컬 펌웨어 파일 경로 / local firmware file path</param>
    /// <param name="remoteTempPath">원격 임시 경로 (업로드 대상) / remote temp path (upload target)</param>
    /// <param name="remoteTargetPath">원격 최종 경로 (Rename 대상) / remote final path (rename target)</param>
    /// <param name="permission">실행 권한 (기본 755) / execution permission (default 755)</param>
    /// <param name="progress">진행 보고 (없으면 null) / progress reporter (null if none)</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>배포 성공 여부 / whether the deployment succeeded</returns>
    public async Task<bool> DeployFirmwareAsync(
        string                  localPath,
        string                  remoteTempPath,
        string                  remoteTargetPath,
        int                     permission = 755,
        IProgress<FtpProgress>? progress   = null,
        CancellationToken       ct         = default) {
        // 배포 시작 로그
        // log deployment start
        logger.Log(LogCategories.Ftp, LogLevel.Info, $"Deploy firmware: {localPath} → {remoteTargetPath}");

        // 1단계: 임시 경로에 업로드
        // step 1: upload to temp path
        if (!await UploadFileAsync(localPath, remoteTempPath, progress, ct).ConfigureAwait(false)) {
            // 업로드 실패 — 중단
            // upload failed — abort
            logger.Log(LogCategories.Ftp, LogLevel.Error, "Deploy aborted: upload failed");
            // 실패 반환
            // return failure
            return false;
        }

        // 2단계: 실행 권한 설정
        // step 2: set execution permission
        if (!await SetChmodAsync(remoteTempPath, permission, ct).ConfigureAwait(false)) {
            // 권한 설정 실패 — 중단
            // chmod failed — abort
            logger.Log(LogCategories.Ftp, LogLevel.Error, "Deploy aborted: chmod failed");
            // 실패 반환
            // return failure
            return false;
        }

        // 3단계: 기존 파일 삭제
        // step 3: delete existing file
        if (!await DeleteFileAsync(remoteTargetPath, ct).ConfigureAwait(false)) {
            // 삭제 실패 — 중단
            // delete failed — abort
            logger.Log(LogCategories.Ftp, LogLevel.Error, "Deploy aborted: delete failed");
            // 실패 반환
            // return failure
            return false;
        }

        // 4단계: 임시 파일을 최종 경로로 이동
        // step 4: rename temp file to final path
        if (!await RenameFileAsync(remoteTempPath, remoteTargetPath, ct).ConfigureAwait(false)) {
            // 이름 변경 실패 — 중단
            // rename failed — abort
            logger.Log(LogCategories.Ftp, LogLevel.Error, "Deploy aborted: rename failed");
            // 실패 반환
            // return failure
            return false;
        }

        // 배포 완료 로그
        // log deployment complete
        logger.Log(LogCategories.Ftp, LogLevel.Info, "Deploy firmware completed");
        // 성공 반환
        // return success
        return true;
    }
}