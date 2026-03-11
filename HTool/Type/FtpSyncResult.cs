namespace HTool.Type;

/// <summary>
///     FTP 디렉토리 동기화 결과.
///     FTP directory synchronization result.
/// </summary>
/// <param name="Success">동기화 성공 여부 / whether synchronization succeeded</param>
/// <param name="FilesUpdated">업데이트된 파일 수 / number of files updated</param>
/// <param name="FilesSkipped">건너뛴 파일 수 / number of files skipped</param>
/// <param name="Elapsed">소요 시간 / elapsed time</param>
public readonly record struct FtpSyncResult(bool Success, int FilesUpdated, int FilesSkipped, TimeSpan Elapsed);