# ParaMon Pro-X 코드 구조

> **작성일(Created)**: 2026-06-30
> **대상**: ParaMon (ParaMonEx, .NET 8 / WinForms / DevExpress) — Remote-Pro X 지원
> **참조**: HToolEx NuGet 1.1.23 (소스: `D:\Dev\Enterprise\Windows\HToolEx\Legacy\HToolEx`)
> **연관 문서**: [Remote-Pro X 프로토콜 매뉴얼](./ParaMon-Pro-X-Remote-Pro-X.md) (Confluence v10 / 프로토콜 v4.2.2)
> **용도**: v4.1.3 Pro-X 지원 기능 업데이트를 위한 코드 구조 파악 (로컬 보관용)

---

## 전체 구조 — 2계층

```
┌─ ParaMon (통합·UI) ──────────────────────────────────────────────┐
│  FormMain ─ Pro-X 모드 진입/이탈, IProObserver                    │
│  Manager/   ComManager(이중경로) · StateManager · NodeManager      │
│             ShareManager(릴리스 다운로드) · PageManager             │
│  Module/    PagePro* (11개)    Forms/ FormPro* (4) · FormSelectTool │
│  Interface/ IProObserver                                           │
└───────────────┬───────────────────────────────────────────────────┘
                │ HToolEx.ProEx.*  (NuGet 1.1.23)
┌───────────────▼─ HToolEx — Remote-Pro X 프로토콜 계층 ─────────────┐
│  HCommProEx (단일 진입점)                                          │
│  Format/    FormatMessage·Info·Request, FormatToolInfo,            │
│             FormatEventExtended, FormatJobEvent, FormatSet*         │
│  FormatJob/ FormatJob + FormatStep{Fasten,Input,Output,Delay,…}     │
│  Type/      MessageIdTypes(MID enum), JobStepTypes …                │
│  Manager/   SessionManager(TCP) · ToolManager · FtpManager          │
└─────────────────────────────────────────────────────────────────────┘
```

---

## A. HToolEx — 프로토콜 계층

소스 루트: `D:\Dev\Enterprise\Windows\HToolEx\Legacy\HToolEx\ProEx`
네임스페이스: `HToolEx.ProEx` (+ `.Format`, `.FormatJob`, `.Manager`, `.Type`, `.Util`)

### A.1 단일 진입점 — `HCommProEx` (`ProEx/HCommProEx.cs`)

ParaMon은 이 클래스 하나만 소비한다.

- **연결**: `Connect(ip, port)` (async, TCP + FTP 동시) / `Disconnect()`
  - 포트는 그룹 설정값 (장치 Share 화면 기본 **6474**)
- **송신**: `RequestMessage(FormatMessage)` (큐잉), `static CreateRequestPacket(id, revision, values)`
  - Modbus 헬퍼: `ReadHoldingReg`, `ReadInputReg`, `WriteSingleReg`, `WriteMultiReg`, `WriteStrReg`
- **이벤트**:
  | 이벤트 | 내용 |
  |--------|------|
  | `ReceivedMsg` | 일반 메시지 수신 (FormatMessage) |
  | `ReceivedEventData` | TOOL 이벤트 (FormatEventExtended) |
  | `ReceivedJobEventData` | JOB 이벤트 (FormatJobEvent) |
  | `ChangesMemberTools` / `ChangesScanTools` | 멤버/스캔 공구 목록 변경 |
  | `ReceivedData` | Modbus 응답 |
  | `ConnectionState` | 연결 상태 변경 |
- **내부 동작**: 100ms `ProcessTimer` 큐 디스패치 · **Keep-Alive 10초(MID 2)** · 재시도 3회 × 1000ms 타임아웃

### A.2 패킷 구조 (매뉴얼과 일치 확인)

- `FormatMessageInfo` (`Format/FormatMessageInfo.cs`) = **16바이트 헤더** (big-endian)
  ```
  Offset  Field      Size   Description
  0–1     Length     2      전체 길이 (Length 필드 제외)
  2–3     MID        2      Message ID (MessageIdTypes)
  4–5     Revision   2      프로토콜 Revision (기본 0)
  6–15    Reserved   10     예약/패딩
  ```
- `FormatMessage` = 헤더 + `Values[]` 페이로드, `GetValues()` = 전체 패킷
- `FormatMessageRequest` = 큐용 래퍼 (재시도·타임아웃·`IsNotAck`·Modbus용 `Code`/`Address`)
- `ProPacket` (`Util/ProPacket.cs`) = Pro-X 터널 안에 실리는 **Modbus 스타일 빌더**
  - `GetReadHoldingRegPacketFromPro`, `GetReadInputRegPacketFromPro`, `SetSingleRegPacketFromPro`, `SetMultiRegPacketFromPro`, `SetMultiRegStrPacketFromPro`
  - FC: 3=ReadHolding, 4=ReadInput, 6=WriteSingle, 16=WriteMulti
  - → FormVirtual의 5200번대 R/W가 이 경로

### A.3 MID enum — `MessageIdTypes` (`Type/MessageIdTypes.cs`)

ushort 기반. 그룹별 범위 (정확한 번호·Revision은 구현 시 소스 직접 확인 필요):

| 범위 | 그룹 | 예시 |
|------|------|------|
| 0–4 | General | CommandAccepted(0), CommandError(1), KeepAlive(2), SystemReboot(3), SystemTime(4) |
| 10–16 | Member tool | MemberToolRequest/Reply, ScanToolRequest/Reply, Add/Release/RenameMemberTool |
| 20–32 | Job | JobListRequest/Reply, JobListRefresh, JobCodeUpdateAndRefresh |
| 40–68 | Setting | Operation/InOut/Log/Barcode/Network/Share/Sound/StepNgCause/JobNgCause/IoToolName 의 Request·Reply·Set |
| 70–77 | System | Information, Xml, Multilingual 의 Request·Reply·Update |
| 80–… | Operation | SelectJob, Previous/Next/ResetJob, ResetStep, Back, Skip, JobEvent(Subscribe/Ack/…) |
| (Event) | Event | LastEvent(Subscribe/Ack), OldEventRequest/Reply, LastEventId Request/Reply |
| 110–111 | Modbus | ModbusRequest / ModbusReply → 레지스터 I/O |
| 120–124 | Encoder(Position) | EncoderRequest/Reply/Set, EncoderValueRequest/Reply |

- **Revision**: 헤더 2바이트. 기본 0. `FormatEventExtended`는 `"0.{revision}"` 문자열로 비교.

### A.4 주요 데이터 타입 (`ProEx/Format`)

| 타입 | 크기 | 용도 |
|------|------|------|
| `FormatToolInfo` | 멤버 80B / 스캔 47B | 공구 정보 — ToolType(255=I/O), Model, Serial, Version, IP, Port, MAC, Name, Status, CheckSum |
| `FormatEventExtended` | ≥1702B | 체결 이벤트 — 토크/각도 메트릭, 그래프 2채널, ID 6쌍, GraphSteps[10] |
| `FormatJobEvent` | 1960B | JOB 이벤트 — EventType(JobEventTypes), Job/Step 정보, NgCause |
| `FormatSetOperation/InOut/Log/Barcode/Network/Share/Sound/Encoder` | 가변 | 각 설정 화면 R/W 데이터 |
| `FormatSystem` | 88/90/91B | 시스템 정보 (버전/시리얼/저장공간/네트워크/엔코더 지원) |
| `FormatXml` | 20B | XML 버전/배포일 |
| `FormatNgCause`, `FormatSetIoToolName`, `FormatEncPos` | 가변 | NG 사유 / IO 공구명 / 엔코더 위치 |

### A.5 JOB 파일 포맷 (`ProEx/FormatJob`)

- `FormatJob` — 헤더 시그니처 `"bmc.job."`, 172B(rev0) / 176B(rev1+)
  - Index, Name(128B), CountOf{Step,Screw,Fasten,Input,Output,Delay,Message,Id}
- `FormatStep` (베이스) — Type(JobStepTypes) + Name(128B) + Data, 스텝당 4096B(rev0) / 12288B(rev1)
- 스텝 종류: `FormatStepFasten`(+ `FormatScrew[99]`, `FormatEncoder[99]`, `FormatVirtual`), `FormatStepInput`, `FormatStepOutput`, `FormatStepDelay`, `FormatStepMessage`, `FormatStepId`

### A.6 내부 매니저 (`ProEx/Manager`)

| 클래스 | 책임 |
|--------|------|
| `SessionManager` | 저수준 TCP(SimpleTcp) + 16바이트 프레임 파서, 100ms 타이머 |
| `ToolManager` | 정적 멤버/스캔 공구 레지스트리, `SelectedTool` |
| `FtpManager` | JOB/파일 동기화 (AsyncFtpClient, 포트 7762, 계정 hantas/hantas0809) |

---

## B. ParaMon — 통합 계층 (`Manager/`)

### B.1 `ComManager` — 통신 허브 (RTU + Pro-X)

두 통신 객체 보유: `HCommEx`(RTU/serial), `HCommProEx`(Pro-X/TCP).

- **연결**: `Connect(GroupItem)` → `HCommPro.Connect(ip, port)` → FTP 다운로드 → 초기 요청(Member/Information/Operation/InOut/Encoder)
- **요청 이중 경로**:
  - `RequestAction(CodeTypes code, ushort addr, ushort[] values, …)` — RTU 연결이면 `HComm`, 아니면 `HCommPro`로 **Modbus** (쓰기 성공 시 이력 기록)
  - `RequestAction(MessageIdTypes id, int revision=0, byte[] values=null)` → `HCommPro.RequestMessage(CreateRequestPacket(...))` — **Pro-X 메시지**
- **옵서버 fan-out**: `RegisterProObserver(observer)` + `ProObservers`(ConcurrentDictionary)
  - `OnReceivedMsg` → `OnChangesScanTools` → `OnChangesMemberTools` → `OnReceivedEventData` 를 등록된 모든 IProObserver에 전파

### B.2 `IProObserver` (`Interface/IProObserver.cs`)

```csharp
void OnReceivedMsg(FormatMessage msg);                          // 메시지 수신
void OnChangesScanTools(IReadOnlyList<FormatToolInfo> tools);   // 스캔 공구 변경
void OnChangesMemberTools(IReadOnlyList<FormatToolInfo> tools); // 멤버 공구 변경
void OnReceivedEventData(FormatEventExtended data);             // 이벤트 데이터 수신
```

**구현체**: `FormMain`, 모든 `PagePro*`, `PageGraph`, `PageEvent`, `FormProXml`, `FormProNgCause`, `FormSelectTool`. `PageManager`가 페이지 열림/닫힘에 맞춰 옵서버 등록/해제.

### B.3 기타 매니저

| 매니저 | Pro-X 관련 책임 |
|--------|----------------|
| `StateManager` | 스냅샷: `StatusSystemInfo`(FormatSystem), `IsOldSystem`(저장공간<10MB), `ProSetOperation/InOut/Encoder`, `SelectedNode`(그룹 내 선택 공구) |
| `NodeManager` | DB `table_group`(`use_pro` 플래그) 로드. 노드 타입 `ProGroup`/`ProTool` vs `Group`/`Tool` |
| `ShareManager` | ⚠️ **Pro-X 공유 API 아님** — hantas-share.co.kr 릴리스(펌웨어/SW/XML) 다운로드 REST 래퍼(HMAC-SHA256) |
| `PageManager` | 페이지 전환 + Pro-X 옵서버 등록/해제 |

### B.4 `FormMain` — Pro-X 모드 조정

- `bbConnect_ItemClick`: ProGroup이면 `ComManager.Connect(group)` 후 `rbpcPro.Visible=true`(Pro 리본 노출); 이탈 시 `Close()` + 상태 초기화
- `bbSelectTool_ItemClick`: 그룹 내 공구 선택 → `RequestAction(ModbusRequest, GetReadInputRegPacketFromPro(...))` → `StateManager.SelectedNode`
- IProObserver 구현: `OnReceivedMsg`에서 Operation/InOut/Information/EncoderReply 처리 → StateManager 갱신 → UI BeginInvoke. 나머지 3개는 빈 스텁(페이지가 직접 처리)

---

## C. UI 페이지/폼 → 메시지 대응

| 화면 | 책임 | 주 메시지/호출 |
|------|------|----------------|
| `PageProTools` | 멤버/스캔 공구 관리 | Scan/MemberToolRequest, Add/Release/RenameMemberTool |
| `PageProJob` | JOB/스텝 CRUD | 파일 기반 JOB 직렬화 + FTP |
| `PageProLog` | 이벤트 로그 뷰 | 다운로드된 CSV 로그 |
| `PageProSetOperation` | 운영 설정 | OperationRequest/Reply/Set |
| `PageProSetInOut` | 입출력 신호 | InOutRequest/Reply/Set |
| `PageProSetLog` | 로그/그래프 채널 | LogRequest/Reply/Set |
| `PageProSetBarcode` | 바코드 할당 | BarcodeRequest/Reply/Refresh |
| `PageProSetNetwork` | WiFi/네트워크 | NetworkRequest/Reply/Set |
| `PageProSetShare` | FTP/Modbus proxy/Remote Pro X(Client·Server)/OpenProtocol | ShareRequest/Reply/Set |
| `PageProSetSound` | 음원 업로드(OK/NG/ETC/TAP) | SoundRequest/Reply/Set + UploadFile |
| `PageProSetEncoder` | 엔코더 보정 | Encoder(Value)Request/Reply/Set |
| `PageGraph` / `PageEvent` | 실시간 그래프/이벤트 | `OnReceivedEventData` |
| `FormProXml` | XML 업로드/버전 | XmlRequest/Reply |
| `FormProNgCause` | NG 사유 편집 | Step/JobNgCause Request/Reply/Set |
| `FormProRename` | 이름 변경 | RenameMemberTool (32B ASCII) |
| `FormProRestore` | .p2p 백업 복원 | FTP 업로드 |
| `FormSelectTool` | 공구/IO공구 선택 | IoToolName Request/Reply |

---

## D. 핵심 흐름

- **연결**: FormMain → `ComManager.Connect(group)` → `HCommProEx.Connect`(TCP+FTP) → 초기 요청 묶음(Member/Info/Operation/InOut/Encoder) → 리본 `rbpcPro` 노출
- **요청-응답**: Page → `ComManager.RequestAction(MID)` → `HCommProEx` 큐 → 응답 수신 → `OnReceivedMsg` → FormMain이 StateManager 갱신 → 해당 Page UI 반영
- **이벤트 푸시**: 장치 → `ReceivedEventData/JobEventData`(자동 ACK) → `OnReceivedEventData` → PageGraph/PageEvent 실시간 갱신
- **공구 변경**: MID 11/13 수신 → `ChangesMember/ScanTools` → PageProTools 그리드 갱신

---

## E. 매뉴얼 ↔ 코드 & 구현 시 유의점

1. **MID 정합성 검증 필수** — 매뉴얼은 **v4.2.2(Confluence v10)** 기준, HToolEx NuGet은 **1.1.23**. 구현 시 정확한 MID↔이름·Revision은 `MessageIdTypes.cs`와 매뉴얼 MID list를 **직접 대조**할 것 (버전 스큐로 일부 번호/Revision 차이 가능). 매뉴얼이 더 최신일 수 있으므로 **HToolEx에 미구현 MID/Revision이 있는지**가 이번 업데이트의 핵심 포인트.
2. **CLAUDE.md 표기 오류 2건** — 통신 클래스는 `HCommProX`가 아니라 **`HCommProEx`**; `ShareManager`는 Pro-X 공유가 아니라 **릴리스 다운로드**.
3. **확장 지점 (3단 동시 수정)** — 신규 메시지 추가 시:
   1. HToolEx: `MessageIdTypes` enum + 파서/`FormatSet*`
   2. ParaMon: `ComManager.RequestAction`/옵서버 분기
   3. ParaMon: 해당 `PagePro*` UI

---

## 다음 단계 후보

- **갭 분석**: 매뉴얼의 MID/메시지 중 HToolEx·ParaMon에 미구현인 항목 식별
- **v4.1.3 Pro-X 기능 범위 확정** 후 단계별 구현 플랜 작성
