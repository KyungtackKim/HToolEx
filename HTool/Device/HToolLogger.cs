using HTool.Type;

namespace HTool.Device;

/// <summary>
///     카테고리 기반 통합 로거. 패킷·연결·파이프라인 등 카테고리별 로그를 콘솔/파일/이벤트로 출력한다.
///     Category-based integrated logger. Outputs logs per category (Packet, Connection, Pipeline, etc.)
///     to console, file, and/or event subscribers.
/// </summary>
/// <remarks>
///     <para>
///         조기 반환 최적화: 비활성 카테고리 로그 호출 시 문자열 생성/변환 없이 즉시 반환.
///     </para>
///     <para>
///         Early return optimization: when category is disabled, returns immediately without
///         string construction or conversion.
///     </para>
/// </remarks>
public sealed class HToolLogger : IDisposable {
    // 파일 쓰기용 락 객체
    // lock object for file write synchronization
    private readonly object _fileLock = new();
    // 해제 상태 플래그
    // disposed state flag
    private bool _disposed;

    // 파일 쓰기 스트림
    // file write stream
    private StreamWriter? _fileWriter;

    /// <summary>
    ///     활성화된 로그 카테고리. 비트 플래그로 복수 선택 가능.
    ///     Enabled log categories. Multiple categories via bit flags.
    /// </summary>
    public LogCategories EnabledCategories { get; set; } = LogCategories.None;

    /// <summary>
    ///     최소 로그 수준. 이 수준 미만의 로그는 무시된다.
    ///     Minimum log level. Logs below this level are ignored.
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    ///     콘솔 출력 활성화 여부.
    ///     Whether console output is enabled.
    /// </summary>
    public bool ConsoleEnabled { get; set; }

    /// <inheritdoc />
    public void Dispose() {
        // 이중 해제 방지
        // prevent double disposal
        if (_disposed)
            // 이미 해제됨 — 건너뜀
            // already disposed — skip
            return;
        // 해제됨으로 표시
        // mark as disposed
        _disposed = true;
        // 파일 스트림 정리
        // dispose file stream
        _fileWriter?.Dispose();
        // 참조 제거
        // clear reference
        _fileWriter = null;
    }

    /// <summary>
    ///     로그 항목 수신 이벤트. 활성화된 카테고리의 로그만 전달된다.
    ///     Log entry received event. Only delivers logs from enabled categories.
    /// </summary>
    public event Action<LogEntry>? LogReceived;

    /// <summary>
    ///     파일 로그 출력을 활성화한다.
    ///     Enables file log output.
    /// </summary>
    /// <param name="filePath">로그 파일 경로 / log file path</param>
    public void EnableFile(string filePath) {
        // 파일 쓰기 동기화
        // synchronize file write access
        lock (_fileLock) {
            // 기존 파일 스트림 정리
            // clean up existing file stream
            _fileWriter?.Dispose();
            // 새 파일 스트림 생성 (추가 모드, UTF-8)
            // create new file stream (append mode, UTF-8)
            _fileWriter = new StreamWriter(filePath, true) {
                // 자동 플러시 활성화
                // enable auto-flush
                AutoFlush = true
            };
        }
    }

    /// <summary>
    ///     파일 로그 출력을 비활성화한다.
    ///     Disables file log output.
    /// </summary>
    public void DisableFile() {
        // 파일 쓰기 동기화
        // synchronize file write access
        lock (_fileLock) {
            // 파일 스트림 정리
            // dispose file stream
            _fileWriter?.Dispose();
            // 참조 제거
            // clear reference
            _fileWriter = null;
        }
    }

    /// <summary>
    ///     로그를 기록한다. 비활성 카테고리이면 즉시 반환.
    ///     Logs a message. Returns immediately if category is disabled.
    /// </summary>
    /// <param name="category">로그 카테고리 / log category</param>
    /// <param name="level">로그 수준 / log severity level</param>
    /// <param name="message">로그 메시지 / log message</param>
    internal void Log(LogCategories category, LogLevel level, string message) {
        // 카테고리 활성화 여부 확인
        // check if category is enabled
        if ((EnabledCategories & category) is 0)
            // 비활성 카테고리 — 즉시 반환
            // disabled category — return immediately
            return;

        // 최소 로그 수준 확인
        // check minimum log level
        if (level < MinLevel)
            // 수준 미만 — 즉시 반환
            // below minimum level — return immediately
            return;

        // 로그 항목 생성
        // create log entry
        var entry = new LogEntry(DateTime.Now, category, level, message);

        // 구독자에게 로그 전달
        // deliver log to subscribers
        LogReceived?.Invoke(entry);

        // 로그 출력 문자열 포맷
        // format log output string
        var line = $"[{entry.Timestamp:HH:mm:ss.fff}] [{category}] [{level}] {message}";

        // 콘솔 출력 확인
        // check if console output is enabled
        if (ConsoleEnabled)
            // 콘솔에 출력
            // write to console
            Console.WriteLine(line);

        // 파일 쓰기 동기화 (null 확인 포함)
        // synchronize file write access (includes null check inside lock)
        lock (_fileLock) {
            // 파일 출력 활성화 여부 확인
            // check if file output is enabled
            if (_fileWriter is null)
                // 파일 쓰기 객체 없음 — 건너뜀
                // no file writer — skip
                return;
            // 가드: 파일 쓰기 실패 무시
            // guard: ignore file write failures
            try {
                // 파일에 기록
                // write to file
                _fileWriter.WriteLine(line);
            } catch {
                // 파일 쓰기 실패 — 로그 손실 허용
                // file write failed — log loss acceptable
            }
        }
    }

    /// <summary>
    ///     원시 패킷 데이터를 16진수 문자열로 기록한다.
    ///     Logs raw packet data as a hex string.
    /// </summary>
    /// <param name="direction">방향 ("TX" 또는 "RX") / direction ("TX" or "RX")</param>
    /// <param name="data">패킷 데이터 / packet data</param>
    internal void LogPacket(string direction, ReadOnlySpan<byte> data) {
        // Packet 카테고리 활성화 여부 확인
        // check if Packet category is enabled
        if ((EnabledCategories & LogCategories.Packet) is 0)
            // 비활성 카테고리 — 즉시 반환
            // disabled category — return immediately
            return;

        // 16진수 문자열 변환
        // convert to hex string
        var hex = Convert.ToHexString(data);
        // 방향과 데이터를 포함한 메시지 기록
        // log message with direction and data
        Log(LogCategories.Packet, LogLevel.Debug, $"{direction} [{data.Length}] {hex}");
    }
}