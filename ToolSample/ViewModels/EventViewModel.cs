using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HTool.Device.Protocol;
using HTool.Format.Process;
using HTool.Type;
using Microsoft.Win32;
using ToolSample.Models;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     이벤트 모니터링 페이지 ViewModel. 체결 이벤트 수신 및 표시.
///     Event monitoring page ViewModel. Receives and displays fastening events.
/// </summary>
public sealed partial class EventViewModel(
    HToolService       htool,
    IDispatcherService dispatcher) : ObservableObject, IPageViewModel {
    // 최대 이벤트 수
    // maximum event count
    private const int MaxEvents = 10000;

    // 모니터링 중 여부
    // monitoring in progress flag
    [ObservableProperty]
    private bool _isMonitoring;

    // 직접 모드 이벤트 폴링 타이머
    // direct mode event polling timer
    private DispatcherTimer? _pollingTimer;

    /// <summary>
    ///     이벤트 레코드 목록.
    ///     Event record list.
    /// </summary>
    public ObservableCollection<EventRecord> Events { get; } = [];

    /// <inheritdoc />
    public void Activate() {
        // 데이터 수신 이벤트 구독
        // subscribe to data received event
        htool.DataReceived += OnDataReceived;
        // PRO X 툴 이벤트 구독
        // subscribe to PRO X tool event
        htool.ToolEventReceived += OnToolEventReceived;
    }

    /// <inheritdoc />
    public void Deactivate() {
        // 모니터링 중이면 정지
        // stop if monitoring
        if (IsMonitoring)
            // 모니터링 정지
            // stop monitoring
            StopMonitoring();

        // 데이터 수신 이벤트 구독 해제
        // unsubscribe from data received event
        htool.DataReceived -= OnDataReceived;
        // PRO X 툴 이벤트 구독 해제
        // unsubscribe from PRO X tool event
        htool.ToolEventReceived -= OnToolEventReceived;
    }

    /// <summary>
    ///     이벤트 모니터링을 시작한다.
    ///     Starts event monitoring.
    /// </summary>
    [RelayCommand]
    private void StartMonitoring() {
        // 모니터링 시작 상태 설정
        // set monitoring started state
        IsMonitoring = true;

        // PRO X 모드: 이벤트 구독
        // PRO X mode: subscribe to events
        if (htool.Tool.Type is ComType.Pro) {
            // PRO X 이벤트 구독
            // subscribe PRO X events
            htool.Tool.Pro?.SubscribeToolEvent();
            // PRO X 모드 — 반환
            // PRO X mode — return
            return;
        }

        // 직접 모드: 폴링 타이머 시작
        // direct mode: start polling timer
        _pollingTimer = new DispatcherTimer {
            // 3초 간격 설정
            // set 3 second interval
            Interval = TimeSpan.FromSeconds(3)
        };
        // 타이머 Tick 이벤트 구독
        // subscribe to timer Tick event
        _pollingTimer.Tick += OnPollingTick;
        // 타이머 시작
        // start timer
        _pollingTimer.Start();
        // 첫 이벤트 즉시 요청
        // request first event immediately
        RequestEvent();
    }

    /// <summary>
    ///     이벤트 모니터링을 정지한다.
    ///     Stops event monitoring.
    /// </summary>
    [RelayCommand]
    private void StopMonitoring() {
        // 모니터링 정지 상태 설정
        // set monitoring stopped state
        IsMonitoring = false;

        // PRO X 모드: 이벤트 구독 해제
        // PRO X mode: unsubscribe from events
        if (htool.Tool.Type is ComType.Pro) {
            // PRO X 이벤트 구독 해제
            // unsubscribe PRO X events
            htool.Tool.Pro?.UnsubscribeToolEvent();
            // PRO X 모드 — 반환
            // PRO X mode — return
            return;
        }

        // 직접 모드: 폴링 타이머 정지
        // direct mode: stop polling timer
        if (_pollingTimer is not null) {
            // 타이머 정지
            // stop timer
            _pollingTimer.Stop();
            // Tick 이벤트 구독 해제
            // unsubscribe from Tick event
            _pollingTimer.Tick -= OnPollingTick;
            // 타이머 참조 제거
            // clear timer reference
            _pollingTimer = null;
        }
    }

    /// <summary>
    ///     이벤트 목록을 초기화한다.
    ///     Clears event list.
    /// </summary>
    [RelayCommand]
    private void ClearEvents() {
        // 이벤트 목록 초기화
        // clear event list
        Events.Clear();
    }

    /// <summary>
    ///     이벤트를 CSV로 내보낸다.
    ///     Exports events to CSV.
    /// </summary>
    [RelayCommand]
    private void ExportCsv() {
        // 저장 대화 상자 표시
        // show save file dialog
        var dlg = new SaveFileDialog {
            // CSV 파일 필터 설정
            // set CSV file filter
            Filter = "CSV files (*.csv)|*.csv",
            // 기본 파일명 설정
            // set default filename
            FileName = $"events_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        // 사용자가 취소하면 반환
        // return if user cancelled
        if (dlg.ShowDialog() is not true)
            // 사용자 취소 — 반환
            // user cancelled — return
            return;

        // CSV 내용 빌드
        // build CSV content
        var sb = new StringBuilder();
        // 헤더 행 추가
        // add header row
        sb.AppendLine("Id,DateTime,Preset,Status,Direction,TargetTorque,Torque,Unit,Angle,Speed,Barcode,Error");
        // 각 이벤트 레코드 추가
        // append each event record
        foreach (var ev in Events)
            // CSV 행 추가
            // add CSV row
            sb.AppendLine(
                $"{ev.Id},{ev.DateTime},{ev.Preset},{ev.Status},{ev.Direction},{ev.TargetTorque},{ev.Torque},{ev.Unit},{ev.Angle},{ev.Speed},{ev.Barcode},{ev.Error}");

        // 파일에 쓰기
        // write to file
        File.WriteAllText(dlg.FileName, sb.ToString());
    }

    /// <summary>
    ///     직접 모드에서 이벤트를 요청한다.
    ///     Requests event in direct mode.
    /// </summary>
    private void RequestEvent() {
        // 이벤트 트리거 레지스터 쓰기 (addr 5350, value 1)
        // write event trigger register (addr 5350, value 1)
        htool.Tool.WriteSingleReg(5350, 1);
        // 이벤트 데이터 읽기 (addr 5000, count 107)
        // read event data (addr 5000, count 107)
        htool.Tool.ReadInputReg(5000, 107);
    }

    /// <summary>
    ///     직접 모드 폴링 타이머 핸들러.
    ///     Direct mode polling timer handler.
    /// </summary>
    private void OnPollingTick(object? sender, EventArgs e) {
        // 연결 상태 확인
        // check connection state
        if (htool.Tool.ConnectionState is not Connection.Connected)
            // 미연결 — 반환
            // not connected — return
            return;

        // 이벤트 요청
        // request event
        RequestEvent();
    }

    /// <summary>
    ///     MODBUS 응답 수신 핸들러 (직접 모드 이벤트 파싱).
    ///     MODBUS response received handler (direct mode event parsing).
    /// </summary>
    private void OnDataReceived(ModbusResponse response) {
        // FC 0x04, 주소 5000 응답만 처리
        // handle only FC 0x04 at address 5000
        if (response.Code is not FunctionCode.ReadInputReg || response.Address is not 5000)
            // 이벤트 응답이 아님 — 반환
            // not an event response — return
            return;

        // 이벤트 파싱 시도 (Event는 struct이므로 null 체크 불필요)
        // attempt event parsing (Event is a struct, no null check needed)
        if (!EventFrame.TryParse(response.Payload.Span, out var ev))
            // 파싱 실패 — 반환
            // parse failed — return
            return;

        // 이벤트 레코드로 변환 및 추가
        // convert to event record and add
        AddEventRecord(ev);
    }

    /// <summary>
    ///     PRO X 툴 이벤트 핸들러. 직접/PRO X 공통 인터페이스로 수신한다.
    ///     PRO X tool event handler. receives both direct and PRO X events via the common interface.
    /// </summary>
    private void OnToolEventReceived(IFastenEvent ev) {
        // 이벤트 레코드로 변환 및 추가
        // convert to event record and add
        AddEventRecord(ev);
    }

    /// <summary>
    ///     IFastenEvent를 EventRecord로 변환하여 목록에 추가한다. 분석 필드는 <see cref="IFastenEvent.Analysis" />에서 가져온다.
    ///     Converts an IFastenEvent to an EventRecord and adds to the list. Analysis fields come from <see cref="IFastenEvent.Analysis" />.
    /// </summary>
    private void AddEventRecord(IFastenEvent ev) {
        // 분석 블록 참조
        // analysis block reference
        var a = ev.Analysis;
        // 기본 바코드/ID 값 (목록 첫 항목 또는 빈 문자열)
        // primary barcode/ID value (first list item or empty)
        var primaryId = ev.Ids.Count > 0 ? ev.Ids[0] : string.Empty;
        // EventRecord 생성
        // create EventRecord
        var record = new EventRecord {
            // 이벤트 ID 설정
            // set event ID
            Id = ev.Id,
            // 날짜 시간 포맷 (Time 단일 소스)
            // format date time (single source: Time)
            DateTime = $"{ev.Time:yyyy-MM-dd} {ev.Time:HH:mm:ss}",
            // 프리셋 번호 설정
            // set preset number
            Preset = a.Preset,
            // 이벤트 상태 설정
            // set event status
            Status = a.EventStatus.ToString(),
            // 회전 방향 설정
            // set rotation direction
            Direction = a.Direction.ToString(),
            // 목표 토크 설정
            // set target torque
            TargetTorque = a.TargetTorque,
            // 실측 토크 설정
            // set actual torque
            Torque = a.Torque,
            // 토크 단위 설정
            // set torque unit
            Unit = a.TorqueUnit.ToString(),
            // 총 각도 설정
            // set total angle
            Angle = a.Angle,
            // 모터 속도 설정
            // set motor speed
            Speed = a.Speed,
            // 바코드 설정 (Ids[0])
            // set barcode (Ids[0])
            Barcode = primaryId,
            // 오류 코드 설정
            // set error code
            Error = a.Error
        };

        // UI 스레드에서 추가
        // add on UI thread
        dispatcher.Invoke(() => {
            // 상한 초과 시 오래된 항목 제거
            // remove old items when exceeding limit
            while (Events.Count >= MaxEvents)
                // 가장 오래된 항목 제거
                // remove oldest item
                Events.RemoveAt(0);
            // 이벤트 레코드 추가
            // add event record
            Events.Add(record);
        });
    }
}