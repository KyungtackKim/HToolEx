using System.Diagnostics;
using HTool.Format.Pro;

namespace HTool.Device.Pro;

/// <summary>
///     PRO X 게이트웨이의 멀티 툴 상태 관리 서비스.
///     Multi-tool state management service for PRO X gateway.
/// </summary>
/// <remarks>
///     <para>
///         Immutable swap 패턴으로 lock-free 읽기를 제공한다.
///         SequenceEqual 기반 변경 감지로 불필요한 이벤트 발생을 방지한다.
///     </para>
///     <para>
///         Provides lock-free reads via immutable swap pattern.
///         Prevents unnecessary events using SequenceEqual-based change detection.
///     </para>
/// </remarks>
public sealed class ToolService {
    // 선택 관리 락 객체
    // lock object for selection management
    private readonly object _lock = new();
    // 전체 멤버 툴 목록 — 해시 비교용 (volatile: 크로스 스레드 가시성)
    // full member tool list — used for hash comparison (volatile: cross-thread visibility)
    private volatile IReadOnlyList<ProToolInfo> _allMemberTools = [];

    // I/O 가상 툴 목록 (volatile: 크로스 스레드 가시성)
    // I/O virtual tool list (volatile: cross-thread visibility)
    private volatile IReadOnlyList<ProToolInfo> _ioTools = [];

    // 물리적 툴만 담은 멤버 툴 목록 (volatile: 크로스 스레드 가시성)
    // physical-tools-only member list (volatile: cross-thread visibility)
    private volatile IReadOnlyList<ProToolInfo> _memberTools = [];

    // 작업 진행 중 플래그 (volatile: 크로스 스레드 가시성)
    // operation-in-progress flag (volatile: cross-thread visibility)
    private volatile bool _operationActive;

    // 스캔 툴 목록 (volatile: 크로스 스레드 가시성)
    // scan tool list (volatile: cross-thread visibility)
    private volatile IReadOnlyList<ProToolInfo> _scanTools = [];

    // 선택된 툴 인덱스 (-1이면 미선택)
    // selected tool index (-1 if none selected)
    private int _selectedToolId = -1;

    /// <summary>
    ///     선택된 툴 인덱스. -1이면 미선택.
    ///     Selected tool index. -1 if none selected.
    /// </summary>
    public int SelectedToolId => Volatile.Read(ref _selectedToolId);

    /// <summary>
    ///     등록된 멤버 툴 목록 (물리적 툴만, I/O 가상 툴 제외).
    ///     Registered member tool list (physical tools only, I/O virtual tools excluded).
    /// </summary>
    public IReadOnlyList<ProToolInfo> MemberTools => _memberTools;

    /// <summary>
    ///     I/O 가상 툴 목록 (ToolType == 0). MemberTools와 별개로 관리.
    ///     I/O virtual tool list (ToolType == 0). Managed separately from MemberTools.
    /// </summary>
    public IReadOnlyList<ProToolInfo> IoTools => _ioTools;

    /// <summary>
    ///     네트워크에서 발견된 스캔 툴 목록.
    ///     Scan tool list discovered on the network.
    /// </summary>
    public IReadOnlyList<ProToolInfo> ScanTools => _scanTools;

    /// <summary>
    ///     선택된 툴이 유효한지 확인한다. 멤버 목록 범위 내이면 true.
    ///     Checks whether the selected tool is valid. true if within member list range.
    /// </summary>
    public bool IsSelectedToolValid {
        get {
            // 현재 선택 인덱스 스냅샷 (이중 읽기 방지)
            // snapshot current selected index (prevents double-read)
            var id = SelectedToolId;
            // 유효한 멤버 목록 범위 내인지 확인
            // check if within valid member list range
            return id >= 0 && id < _memberTools.Count;
        }
    }

    /// <summary>
    ///     시리얼 번호로 멤버 툴을 검색한다.
    ///     Finds a member tool by serial number.
    /// </summary>
    /// <param name="serial">시리얼 번호 / serial number</param>
    /// <returns>일치하는 툴 정보 또는 null / matching tool info or null</returns>
    public ProToolInfo? FindBySerial(string serial) {
        // 현재 멤버 목록 스냅샷 참조
        // reference current member list snapshot
        var tools = _memberTools;
        // 모든 멤버 툴 순회
        // iterate through all member tools
        foreach (var tool in tools)
            // 시리얼 번호 일치 확인 (대소문자 무시)
            // check serial number match (case-insensitive)
            if (string.Equals(tool.Serial, serial, StringComparison.OrdinalIgnoreCase))
                // 일치하는 툴 반환
                // return matching tool
                return tool;
        // 일치하는 툴 없음
        // no matching tool found
        return null;
    }

    /// <summary>
    ///     멤버 툴 인덱스로 선택 툴을 안전하게 설정한다.
    ///     Safely sets the selected tool by member tool index.
    /// </summary>
    /// <param name="index">
    ///     멤버 툴 인덱스 (0 이상 Count 미만), 또는 선택 해제 시 -1.
    ///     member tool index (0 or more and less than Count), or -1 to deselect.
    /// </param>
    /// <param name="timeoutMs">
    ///     작업 완료 대기 최대 시간 (밀리초). 기본값 500ms.
    ///     maximum wait time for any active operation to complete (milliseconds). Default 500ms.
    /// </param>
    /// <returns>설정 성공 시 true, 타임아웃이거나 범위 초과이면 false / true on success, false on timeout or out of range</returns>
    public bool TrySelectTool(int index, int timeoutMs = 500) {
        // 스레드 안전 선택 갱신 (작업 완료 대기 포함)
        // thread-safe selection update (with wait for active operation)
        lock (_lock) {
            // 작업 진행 중이면 완료 신호를 기다림
            // if operation is active, wait for completion signal
            if (_operationActive) {
                // 경과 시간 측정 시작
                // start elapsed timer
                var sw = Stopwatch.StartNew();
                // 작업이 완료되거나 타임아웃될 때까지 반복 대기
                // loop until operation completes or timeout expires
                do {
                    // 남은 대기 시간 계산
                    // calculate remaining wait time
                    var remain = timeoutMs - (int)sw.ElapsedMilliseconds;
                    // 타임아웃 만료 확인
                    // check if timeout expired
                    if (remain <= 0)
                        // timed out waiting for operation to complete
                        // 작업 완료 대기 타임아웃 — 거부
                        return false;
                    // 락 해제 후 EndOperation 신호 대기
                    // release lock and wait for EndOperation signal
                    Monitor.Wait(_lock, remain);
                } while (_operationActive);
            }

            // -1이 아닌 경우 유효한 인덱스 범위 확인
            // validate index range when not deselecting
            if (index is not -1 && (index < 0 || index >= _memberTools.Count))
                // index out of range
                // 인덱스 범위 초과
                return false;
            // 새 선택 적용
            // apply the new selection
            _selectedToolId = index;
            // 선택 성공
            // selection applied successfully
            return true;
        }
    }

    /// <summary>
    ///     멤버 툴 목록을 갱신한다. 해시 비교로 변경 여부를 판단한다.
    ///     Updates the member tool list. Determines change by hash comparison.
    /// </summary>
    /// <param name="tools">새 멤버 툴 목록 / new member tool list</param>
    /// <returns>변경이 있으면 true / true if changed</returns>
    internal bool UpdateMemberTools(IReadOnlyList<ProToolInfo> tools) {
        // 전체 목록과 해시 비교하여 변경 감지
        // detect changes by hash comparison with full list
        if (HashEquals(_allMemberTools, tools))
            // 변경 없음
            // no changes
            return false;

        // 전체 목록 원자적 교체 (다음 비교용)
        // atomically swap full list (for next comparison)
        _allMemberTools = tools;
        // 물리적 툴만 필터하여 교체 (ToolType != 0)
        // atomically swap physical-tools-only list (ToolType != 0)
        _memberTools = tools.Where(static t => t.ToolType is not 0).ToList();
        // I/O 툴 필터 갱신 (ToolType == 0)
        // update I/O tool list (ToolType == 0)
        _ioTools = tools.Where(static t => t.ToolType is 0).ToList();
        // 변경 있음
        // changes detected
        return true;
    }

    /// <summary>
    ///     스캔 툴 목록을 갱신한다. 해시 비교로 변경 여부를 판단한다.
    ///     Updates the scan tool list. Determines change by hash comparison.
    /// </summary>
    /// <param name="tools">새 스캔 툴 목록 / new scan tool list</param>
    /// <returns>변경이 있으면 true / true if changed</returns>
    internal bool UpdateScanTools(IReadOnlyList<ProToolInfo> tools) {
        // 기존 목록과 해시 비교하여 변경 감지
        // detect changes by hash comparison with existing list
        if (HashEquals(_scanTools, tools))
            // 변경 없음
            // no changes
            return false;

        // 새 목록으로 원자적 교체
        // atomically swap to new list
        _scanTools = tools;
        // 변경 있음
        // changes detected
        return true;
    }

    /// <summary>
    ///     MODBUS 작업 시작을 시도한다. 유효한 툴이 선택되어 있고 작업 중이 아닐 때만 성공한다.
    ///     Attempts to begin a MODBUS operation. Succeeds only when a valid tool is selected and no operation is active.
    /// </summary>
    /// <returns>
    ///     작업 시작 성공 시 선택된 툴 인덱스, 실패 시 -1.
    ///     selected tool index on success; -1 on failure.
    /// </returns>
    internal int TryBeginOperation() {
        // 선택 락 획득 후 원자적으로 확인 및 설정
        // acquire selection lock to atomically check and set operation state
        lock (_lock) {
            // 이미 작업 중이면 거부
            // reject if already in operation
            if (_operationActive)
                // operation already active
                // 이미 작업 진행 중
                return -1;
            // 현재 선택된 인덱스 읽기
            // read the current selected index
            var id = _selectedToolId;
            // 선택된 툴이 유효한지 확인
            // check if the selected tool is valid
            if (id < 0 || id >= _memberTools.Count)
                // no valid tool selected
                // 유효한 툴이 선택되어 있지 않음
                return -1;
            // 작업 활성화 플래그 설정
            // mark operation as active
            _operationActive = true;
            // 선택된 툴 인덱스 반환
            // return selected tool index
            return id;
        }
    }

    /// <summary>
    ///     진행 중인 MODBUS 작업을 완료 처리한다.
    ///     Marks the current MODBUS operation as complete.
    /// </summary>
    internal void EndOperation() {
        // 락 획득 후 플래그 해제 및 대기자 신호 전송을 원자적으로 수행
        // acquire lock to atomically clear flag and signal waiters
        lock (_lock) {
            // 작업 완료 — 플래그 해제
            // operation complete — clear active flag
            _operationActive = false;
            // TrySelectTool에서 대기 중인 스레드 깨우기
            // wake any threads waiting in TrySelectTool
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    ///     모든 툴 목록과 선택 상태를 초기화한다.
    ///     Clears all tool lists and selection state.
    /// </summary>
    internal void Clear() {
        // 전체 멤버 목록 초기화
        // clear full member list
        _allMemberTools = [];
        // 물리적 툴 목록 초기화
        // clear physical tool list
        _memberTools = [];
        // I/O 툴 목록 초기화
        // clear I/O tool list
        _ioTools = [];
        // 스캔 목록 초기화
        // clear scan list
        _scanTools = [];
        // 락 획득 후 선택 해제 및 대기자 신호 전송
        // acquire lock to reset selection and signal any waiters
        lock (_lock) {
            // 선택 해제
            // deselect tool
            _selectedToolId = -1;
            // 작업 플래그 초기화
            // clear operation flag
            _operationActive = false;
            // TrySelectTool 대기 중인 스레드 즉시 해제
            // immediately release any threads waiting in TrySelectTool
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    ///     두 툴 목록의 해시가 동일한지 비교한다.
    ///     Compares whether two tool lists have identical hashes.
    /// </summary>
    /// <param name="a">기존 목록 / existing list</param>
    /// <param name="b">새 목록 / new list</param>
    /// <returns>모든 요소의 해시가 동일하면 true / true if all element hashes are identical</returns>
    private static bool HashEquals(IReadOnlyList<ProToolInfo> a, IReadOnlyList<ProToolInfo> b) {
        // 개수 일치 및 모든 해시 쌍이 동일하면 true 반환
        // return true if counts match and all hash pairs are equal
        return a.Count == b.Count && a.Zip(b).All(p => p.First.Hash == p.Second.Hash);
    }
}