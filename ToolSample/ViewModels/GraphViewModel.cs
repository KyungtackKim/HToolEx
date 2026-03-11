using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HTool.Device.Protocol;
using HTool.Format.Process;
using HTool.Type;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     그래프 페이지 ViewModel. LiveCharts2 이중 채널 토크/각도 그래프.
///     Graph page ViewModel. LiveCharts2 dual-channel torque/angle chart.
/// </summary>
public sealed partial class GraphViewModel(
    HToolService       htool,
    IDispatcherService dispatcher) : ObservableObject, IPageViewModel {
    // 채널 1 데이터
    // channel 1 data
    private readonly ObservableCollection<ObservableValue> _channel1Values = [];

    // 채널 2 데이터
    // channel 2 data
    private readonly ObservableCollection<ObservableValue> _channel2Values = [];

    /// <summary>
    ///     차트 시리즈.
    ///     Chart series.
    /// </summary>
    public ISeries[] Series => [
        new LineSeries<ObservableValue> {
            // 채널 1 값 바인딩
            // bind channel 1 values
            Values = _channel1Values,
            // 채널 1 시리즈명
            // channel 1 series name
            Name = "Channel 1",
            // 영역 채우기 비활성화
            // disable area fill
            Fill = null,
            // 마커 크기 0 (선만 표시)
            // marker size 0 (line only)
            GeometrySize = 0,
            // 좌측 Y축 사용
            // use left Y axis
            ScalesYAt = 0,
            // 애니메이션 비활성화
            // disable animation
            AnimationsSpeed = TimeSpan.Zero
        },
        new LineSeries<ObservableValue> {
            // 채널 2 값 바인딩
            // bind channel 2 values
            Values = _channel2Values,
            // 채널 2 시리즈명
            // channel 2 series name
            Name = "Channel 2",
            // 영역 채우기 비활성화
            // disable area fill
            Fill = null,
            // 마커 크기 0 (선만 표시)
            // marker size 0 (line only)
            GeometrySize = 0,
            // 우측 Y축 사용
            // use right Y axis
            ScalesYAt = 1,
            // 애니메이션 비활성화
            // disable animation
            AnimationsSpeed = TimeSpan.Zero
        }
    ];

    /// <summary>
    ///     Y축 정의 (좌: 채널 1, 우: 채널 2).
    ///     Y axes definition (left: channel 1, right: channel 2).
    /// </summary>
    public Axis[] YAxes => [
        new() {
            // 좌측 Y축 이름
            // left Y axis name
            Name = "Channel 1",
            // 좌측 배치
            // left position
            Position = AxisPosition.Start
        },
        new() {
            // 우측 Y축 이름
            // right Y axis name
            Name = "Channel 2",
            // 우측 배치
            // right position
            Position = AxisPosition.End,
            // 분리 표시 비활성화
            // disable separator lines
            ShowSeparatorLines = false
        }
    ];

    /// <summary>
    ///     X축 정의 (샘플 인덱스).
    ///     X axes definition (sample index).
    /// </summary>
    public Axis[] XAxes => [
        new() {
            // X축 이름
            // X axis name
            Name = "Sample"
        }
    ];

    /// <summary>
    ///     LiveCharts2 동기화 객체.
    ///     LiveCharts2 synchronization object.
    /// </summary>
    public object Sync { get; } = new();

    /// <inheritdoc />
    public void Activate() {
        // 데이터 수신 이벤트 구독
        // subscribe to data received event
        htool.DataReceived += OnDataReceived;
        // PRO X 이벤트 구독
        // subscribe to PRO X event
        htool.ToolEventReceived += OnToolEventReceived;
    }

    /// <inheritdoc />
    public void Deactivate() {
        // 데이터 수신 이벤트 구독 해제
        // unsubscribe from data received event
        htool.DataReceived -= OnDataReceived;
        // PRO X 이벤트 구독 해제
        // unsubscribe from PRO X event
        htool.ToolEventReceived -= OnToolEventReceived;
    }

    /// <summary>
    ///     그래프 데이터를 초기화한다.
    ///     Clears graph data.
    /// </summary>
    [RelayCommand]
    private void ClearGraph() {
        // 동기화 블록 내에서 데이터 초기화
        // clear data within sync block
        lock (Sync) {
            // 채널 1 데이터 초기화
            // clear channel 1 data
            _channel1Values.Clear();
            // 채널 2 데이터 초기화
            // clear channel 2 data
            _channel2Values.Clear();
        }
    }

    /// <summary>
    ///     MODBUS 응답 수신 핸들러 (직접 모드 그래프 데이터).
    ///     MODBUS response received handler (direct mode graph data).
    /// </summary>
    private void OnDataReceived(ModbusResponse response) {
        // 그래프 FC 응답만 처리 (FC 0x64 / 0x66)
        // handle graph FC responses only (FC 0x64 / 0x66)
        if (response.Code is not (FunctionCode.GraphData or FunctionCode.HighResGraph))
            // 그래프 응답이 아님 — 반환
            // not a graph response — return
            return;

        // 그래프 파싱 시도
        // attempt graph parsing
        if (!Graph.TryParse(response.Payload.Span, out var graph))
            // 파싱 실패 — 반환
            // parse failed — return
            return;

        // 대상 채널 결정
        // determine target channel
        var target = graph.Channel is 1 ? _channel1Values : _channel2Values;

        // UI 스레드에서 데이터 갱신
        // update data on UI thread
        dispatcher.Invoke(() => {
            // 동기화 블록 내에서 갱신
            // update within sync block
            lock (Sync) {
                // 기존 데이터 초기화
                // clear existing data
                target.Clear();
                // 새 데이터 추가
                // add new data
                foreach (var v in graph.Values)
                    // 데이터 포인트 추가
                    // add data point
                    target.Add(new ObservableValue(v));
            }
        });
    }

    /// <summary>
    ///     PRO X 이벤트 수신 핸들러 (이벤트에 포함된 그래프 개수 표시).
    ///     PRO X event received handler (shows graph point count from event).
    /// </summary>
    private void OnToolEventReceived(Event ev) {
        // 그래프 데이터가 없으면 무시
        // ignore if no graph data
        if (ev.CountOfChannel1 is 0 && ev.CountOfChannel2 is 0)
            // 그래프 데이터 없음 — 반환
            // no graph data — return
            return;

        // PRO X 이벤트의 그래프 데이터는 Event 객체에 내장되어 있으나
        // 직접 접근 API가 다를 수 있음 — 향후 확장 포인트
        // graph data in PRO X events is embedded in Event object
        // but direct access API may differ — future extension point
    }
}