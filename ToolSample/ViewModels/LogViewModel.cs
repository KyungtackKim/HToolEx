using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HTool.Type;
using Microsoft.Win32;
using ToolSample.Models;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     로그 뷰어 페이지 ViewModel. 패킷 hex 로그 표시 및 필터.
///     Log viewer page ViewModel. Packet hex log display and filtering.
/// </summary>
public sealed partial class LogViewModel(
    HToolService       htool,
    IDispatcherService dispatcher) : ObservableObject, IPageViewModel {
    // 최대 로그 수
    // maximum log count
    private const int MaxLogs = 5000;

    // 필터: Connection 카테고리
    // filter: Connection category
    [ObservableProperty]
    private bool _filterConnection = true;

    // 필터: Error 카테고리
    // filter: Error category
    [ObservableProperty]
    private bool _filterError = true;

    // 필터: KeepAlive 카테고리
    // filter: KeepAlive category
    [ObservableProperty]
    private bool _filterKeepAlive;

    // 필터: Packet 카테고리
    // filter: Packet category
    [ObservableProperty]
    private bool _filterPacket = true;

    // 필터: Pipeline 카테고리
    // filter: Pipeline category
    [ObservableProperty]
    private bool _filterPipeline = true;

    // 필터: Pro 카테고리
    // filter: Pro category
    [ObservableProperty]
    private bool _filterPro = true;

    // 일시정지 여부
    // paused flag
    [ObservableProperty]
    private bool _isPaused;

    /// <summary>
    ///     로그 레코드 목록.
    ///     Log record list.
    /// </summary>
    public ObservableCollection<LogRecord> Logs { get; } = [];

    /// <inheritdoc />
    public void Activate() {
        // 로그 수신 이벤트 구독
        // subscribe to log received event
        htool.LogReceived += OnLogReceived;
    }

    /// <inheritdoc />
    public void Deactivate() {
        // 로그 수신 이벤트 구독 해제
        // unsubscribe from log received event
        htool.LogReceived -= OnLogReceived;
    }

    /// <summary>
    ///     일시정지/재개를 토글한다.
    ///     Toggles pause/resume.
    /// </summary>
    [RelayCommand]
    private void TogglePause() {
        // 일시정지 상태 토글
        // toggle paused state
        IsPaused = !IsPaused;
    }

    /// <summary>
    ///     로그 목록을 초기화한다.
    ///     Clears log list.
    /// </summary>
    [RelayCommand]
    private void Clear() {
        // 로그 목록 초기화
        // clear log list
        Logs.Clear();
    }

    /// <summary>
    ///     로그를 파일로 내보낸다.
    ///     Exports logs to file.
    /// </summary>
    [RelayCommand]
    private void Export() {
        // 저장 대화 상자 표시
        // show save file dialog
        var dlg = new SaveFileDialog {
            // 파일 필터 설정
            // set file filter
            Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt",
            // 기본 파일명 설정
            // set default filename
            FileName = $"htool_log_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        // 사용자가 취소하면 반환
        // return if user cancelled
        if (dlg.ShowDialog() is not true)
            // 사용자 취소 — 반환
            // user cancelled — return
            return;

        // 로그 내용 빌드
        // build log content
        var sb = new StringBuilder();
        // 각 로그 레코드 추가
        // append each log record
        foreach (var log in Logs)
            // 타임스탬프/카테고리/레벨/메시지 형식으로 추가
            // append in timestamp/category/level/message format
            sb.AppendLine($"{log.Timestamp}\t{log.Category}\t{log.Level}\t{log.Message}");

        // 파일에 쓰기
        // write to file
        File.WriteAllText(dlg.FileName, sb.ToString());
    }

    /// <summary>
    ///     카테고리가 현재 필터에 포함되는지 확인한다.
    ///     Checks if category is included in current filter.
    /// </summary>
    private bool IsFiltered(LogCategories category) {
        return category switch {
            // Packet 카테고리 필터 확인
            // check Packet category filter
            LogCategories.Packet => FilterPacket,
            // Connection 카테고리 필터 확인
            // check Connection category filter
            LogCategories.Connection => FilterConnection,
            // Pipeline 카테고리 필터 확인
            // check Pipeline category filter
            LogCategories.Pipeline => FilterPipeline,
            // Pro 카테고리 필터 확인
            // check Pro category filter
            LogCategories.Pro => FilterPro,
            // Error 카테고리 필터 확인
            // check Error category filter
            LogCategories.Error => FilterError,
            // KeepAlive 카테고리 필터 확인
            // check KeepAlive category filter
            LogCategories.KeepAlive => FilterKeepAlive,
            // 기타 카테고리 — 표시
            // other categories — show
            _ => true
        };
    }

    /// <summary>
    ///     로그 수신 핸들러.
    ///     Log received handler.
    /// </summary>
    private void OnLogReceived(LogEntry entry) {
        // 일시정지 중이면 무시
        // ignore if paused
        if (IsPaused)
            // 일시정지 — 반환
            // paused — return
            return;

        // 카테고리 필터 확인
        // check category filter
        if (!IsFiltered(entry.Category))
            // 필터 불일치 — 반환
            // filter mismatch — return
            return;

        // 로그 레코드 생성
        // create log record
        var record = new LogRecord {
            // 타임스탬프 포맷
            // format timestamp
            Timestamp = entry.Timestamp.ToString("HH:mm:ss.fff"),
            // 카테고리명
            // category name
            Category = entry.Category.ToString(),
            // 레벨명
            // level name
            Level = entry.Level.ToString(),
            // 메시지
            // message
            Message = entry.Message
        };

        // UI 스레드에서 추가
        // add on UI thread
        dispatcher.Invoke(() => {
            // 상한 초과 시 오래된 항목 제거
            // remove old items when exceeding limit
            while (Logs.Count >= MaxLogs)
                // 가장 오래된 항목 제거
                // remove oldest item
                Logs.RemoveAt(0);
            // 로그 레코드 추가
            // add log record
            Logs.Add(record);
        });
    }
}