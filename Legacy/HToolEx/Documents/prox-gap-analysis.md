# Pro-X 갭 분석 & v4.3.0 적용 계획

> **작성일(Created)**: 2026-06-30
> **최종 목표**: **ParaMonEx에 Remote-Pro X v4.3.0 기능 추가**
> **프로토콜 기준**: 매뉴얼 **v4.3.0 (2026-06-16, Confluence page 202735678)**
> **소스 기준**: HToolEx Legacy 소스(`D:\Dev\Enterprise\Windows\HToolEx\Legacy\HToolEx\ProEx`, NuGet 1.1.23 추정) · ParaMon `ParaMonEx\ParaMon`
> **연관 문서**: [프로토콜 매뉴얼](./ParaMon-Pro-X-Remote-Pro-X.md) · [코드 구조](./paramon-prox-code-structure.md)

---

## 0. 목표 & 작업 구조

- **할 일**: ParaMonEx(ParaMon/MountzCom)가 Remote-Pro X **v4.3.0** 프로토콜을 지원하도록 기능 추가.
- **2-repo 의존**: 프로토콜 파싱은 HToolEx에 있으므로 **HToolEx(선행) → ParaMon(후행)** 순서.
  - HToolEx 현재 수준 ≈ **프로토콜 v4.2.0** (아래 §2). 즉 v4.2.1·v4.3.0 델타가 미반영.
  - ParaMon Set 페이지는 revision-적응형이라 **HToolEx만 올리면 상당 부분 자동 추종**(§3), 단 새 필드 노출용 UI는 추가 필요.

---

## 1. v4.3.0 개정 내역 (2026-06-16)

원문(개정 이력) 9개 항목을 메시지별로 그룹화.

| # | 분류 | MID / 위치 | 변경 내용 | 종류 |
|---|------|-----------|-----------|------|
| 1 | JOB **Message 스텝** | JOB file (`FormatStepMessage`) | `Operation type`에 **`3 = Minimum display time (sec)`** 추가 | 신규 옵션 |
| 2 | **Operation upload reply** | 41 | **Rev.5** 추가 | 신규 Rev |
| 3 | **Operation set request** | 42 | **Rev.5** 추가 | 신규 Rev |
| 4 | **Log upload reply** | 47 | **Rev.3** 추가 | 신규 Rev |
| 5 | **Log set request** | 48 | **Rev.3** 추가 | 신규 Rev |
| 6 | Log upload reply | 47 | Rev.1의 "NG cause" → **"NG comment"** 명칭 변경 | 명칭만 |
| 7 | Log set request | 48 | Rev.1의 "NG cause" → **"NG comment"** 명칭 변경 | 명칭만 |
| 8 | **Last event** | 102 | **Rev.2** 추가 | 신규 Rev |
| 9 | **Old event reply** | 106 | **Rev.2** 추가 | 신규 Rev |

> 참고: Event Rev.1(Status Code, Job/Step/Tool name, NG cause)과 Multilingual(MID 75–77)은 **v4.2.1**에서 추가된 항목으로, HToolEx에 아직 미반영(§2). Event Rev.2(v4.3.0)는 Rev.1 위에 쌓이므로 이벤트 작업 시 **Rev.1을 선행 포함**해야 함.

---

## 2. 현재 구현 수준 (Baseline)

### HToolEx — 프로토콜 ≈ v4.2.0 수준

| Format 클래스 | 현재 최대 Rev | Size 배열 근거 | v4.3.0 목표 | 비고 |
|---------------|---------------|----------------|-------------|------|
| `FormatSetOperation` | Rev.4 | [178,220,221,504,506] | **Rev.5** | 🟠 +1 Rev |
| `FormatSetLog` | Rev.2 | [27,56,63] | **Rev.3** | 🟠 +1 Rev (+ NG comment 명칭) |
| `FormatEventExtended` | **Rev.0 고정**(분기 없음, 1702B) | 단일 | **Rev.2** (Rev.1 경유) | 🔴 Rev.1+Rev.2 신설 |
| `FormatSetNetwork` | Rev.1 | [69,70] | Rev.1 | ✅ |
| `FormatSetShare` | Rev.1 | [280,283] | Rev.1 | ✅ |
| `FormatSystem` | Rev.2 | [88,90,91] | Rev.2 | ✅ |
| `FormatStepMessage` | MessageType {Validation,DelayTime,NextStep} | — | **+Minimum display time** | 🆕 옵션 추가 |
| Multilingual (75–77) | **없음** | — | (v4.2.1, 별도) | 🟡 범위 외 후보 |
| InOut/Barcode/Sound/NgCause/IoToolName/XML/Encoder | Rev.0 | — | Rev.0 | ✅ |

- `HCommProEx.Revision => 0` 은 keep-alive·Modbus 등 **내부 메시지 전용**(설정 요청 revision과 무관).

### ParaMon — revision-적응형 (§3)

- Set 페이지: `RequestAction(XxxRequest, FormatSetXxx.Size.Length - 1)` + `new FormatSetXxx(msg.Values, msg.Header.Revision)`
- 이벤트: `LastEventSubscribe`(revision 인자 없음=0) + `FormatEventExtended` Rev.0 고정 → **이벤트만 양끝 Rev.0 고정**

---

## 3. 핵심 메커니즘 (갭 해석의 전제)

```csharp
// 요청: HToolEx 파서가 지원하는 "최고 Revision"으로 요청
ComManager.Manager.RequestAction(MessageIdTypes.XxxRequest, FormatSetXxx.Size.Length - 1);
// 응답: 장치가 돌려준 헤더 Revision으로 파싱
Item = new FormatSetXxx(msg.Values, msg.Header.Revision);
```

→ 설정 메시지는 **HToolEx 파서를 올리면 ParaMon이 `Size.Length-1` 덕에 자동으로 새 Rev를 요청·파싱**. ParaMon에서 남는 일은 **새 필드를 보여줄 그리드 컬럼/바인딩 추가**뿐.
→ **이벤트는 예외**: subscribe revision과 `FormatEventExtended` 파서를 모두 직접 올려야 함.

> 패턴 위치: `PageProSet{Operation,InOut,Log,Barcode,Network,Share,Sound,Encoder}.cs`. 이벤트: `PageEvent.cs:500`, `PageGraph.cs:647`(`LastEventSubscribe`).

---

## 4. 갭 매트릭스 (v4.3.0 기준)

| 메시지 | MID | 매뉴얼 v4.3.0 | HToolEx | ParaMon 자동추종 | 갭 |
|--------|-----|---------------|---------|------------------|-----|
| Operation reply/set | 41/42 | Rev.5 | Rev.4 | ✔(컬럼만) | 🟠 GAP-3 |
| Log reply/set | 47/48 | Rev.3 | Rev.2 | ✔(컬럼만) | 🟠 GAP-4 |
| Last/Old event | 102/106 | Rev.2 | Rev.0 | ✘(수동) | 🔴 GAP-2 |
| JOB Message 스텝 | JOB file | +Min display time | 없음 | ✘(수동) | 🆕 GAP-5 |
| Log NG cause→comment | 47/48 | 명칭 | 명칭 미반영 | — | 🟡 라벨 |
| Multilingual | 75–77 | (v4.2.1) | 없음 | ✘ | 🟡 GAP-1(범위 외) |

---

## 5. 할 일 (v4.3.0 적용 작업 계획)

### A. HToolEx (프로토콜 계층) — **선행**

1. **Operation Rev.5** — `FormatSetOperation`에 Rev.5 항목/필드 추가 (`Size` 배열 6번째 원소 + Rev.5 파싱 분기)
2. **Log Rev.3 + 명칭** — `FormatSetLog`에 Rev.3 추가 + Rev.1 "NG cause"→"NG comment" 명칭 반영
3. **Event Rev.1 + Rev.2** — `FormatEventExtended`를 Rev 분기형으로 개편: Rev.1(Status Code, Job/Step/Tool name, NG cause) + Rev.2 필드 추가, Tool index -1 범위 처리
4. **Message 스텝** — `FormatStepMessage`의 `MessageType`(또는 Operation type) enum에 `Minimum display time (sec)` 값 추가 + 관련 필드/직렬화
5. (선택, v4.2.1 catch-up) **Multilingual** — `MessageIdTypes` 75–77 + `FormatMultilingual*` 신설
6. NuGet 빌드/버전업 후 ParaMonEx 참조 갱신

### B. ParaMonEx (UI/사용) — **후행**

1. **Operation** (`PageProSetOperation`) — Rev.5 신규 필드 그리드 컬럼/바인딩, 필요한 `Lang` 키
2. **Log** (`PageProSetLog`) — Rev.3 신규 필드 컬럼 + "NG cause"→"NG comment" 라벨/`Lang` 키 변경
3. **Event** (`PageEvent`, `PageGraph`, 결과 표시) — `LastEventSubscribe`를 상위 rev로 요청, 신규 필드(Status Code/NG cause/이름) 표시, Tool index -1 처리
4. **JOB Message 스텝** (`PageProJob`) — Minimum display time 옵션 UI
5. (선택) **Multilingual** — 신규 페이지/폼(`FormProXml` 유사)
6. **MountzCom 동기화** — 동일 변경을 OEM 측에 미러링

---

## 6. 범위 구분 (제안)

| 범위 | 항목 |
|------|------|
| **v4.3.0 필수** | GAP-3(Operation Rev.5), GAP-4(Log Rev.3+rename), GAP-2(Event Rev.2, **Rev.1 선행 포함**), GAP-5(Message Min display time) |
| **별도/선택 (v4.2.1 catch-up)** | GAP-1(Multilingual 75–77) — v4.3.0과 무관한 독립 기능 |

> Event는 Rev.2가 Rev.1 위에 쌓이므로, v4.3.0 필수 범위라도 **Rev.1 필드를 함께 구현**해야 함.

---

## 7. 확인 필요 사항

1. **HToolEx Legacy 소스 = NuGet 1.1.23 동일성** — 본 분석은 Legacy 소스 기준. 실제 참조 패키지와 동기화/드리프트 확인.
2. **각 신규 Rev의 실제 필드 델타 추출** — 매뉴얼 본문 §5.5.2 Rev.5 / §5.5.7~8 Log Rev.3 / §5.8.2 Last event Rev.1·Rev.2 / Message 스텝에서 추가 필드 목록·바이트 레이아웃을 추출해야 구현 가능 (**다음 단계 권장**).
3. **Operation Rev 번호 정합** — 매뉴얼 reply는 Rev.2를 건너뜀(0,1,3,4,5), HToolEx `Size`는 인덱스 0–4. 번호↔인덱스 1:1 대응 정밀 대조.
4. **HToolEx 수정 권한/빌드 파이프라인** — HToolEx 선행 수정이 전제이므로 라이브러리 빌드·배포 흐름 확인.

---

## 다음 단계

1. (권장) **§7-2 필드 델타 추출** — v4.3.0 각 신규 Rev의 추가 필드를 매뉴얼 본문에서 표로 정리 → 구현 스펙 확정
2. 범위 확정 후 `~/.claude/plans/` 규약에 따른 구현 플랜 작성 (HToolEx → ParaMon → MountzCom)
