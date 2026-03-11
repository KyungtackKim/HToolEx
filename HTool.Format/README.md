# HTool.Format

Data parsing and format classes for HANTAS tool communication libraries.

HANTAS 툴 통신 라이브러리용 데이터 파싱 및 포맷 클래스

[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue)](https://www.nuget.org/packages/Hantas.HTool.Format)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Overview / 개요

HTool.Format provides `readonly struct` format classes that parse MODBUS response payloads into strongly-typed data. All
parsing uses `ReadOnlySpan<byte>` for zero-allocation performance. Depends on HTool.Core only.

HTool.Format은 MODBUS 응답 페이로드를 강타입 데이터로 파싱하는 `readonly struct` 포맷 클래스를 제공합니다. 모든 파싱은 `ReadOnlySpan<byte>`를 사용하여 할당 없이
수행됩니다. HTool.Core에만 의존합니다.

---

## Installation / 설치

```bash
dotnet add package Hantas.HTool.Format
```

---

## Structure / 구조

```
HTool.Format/
├── Device/             # SimpleInfo, Info, Status / 기본 정보, 상세 정보, 상태
├── Process/            # Event, Graph, Barcode / 이벤트, 그래프, 바코드
├── Param/              # Preset, AdvPreset, Control / 프리셋, 고급 프리셋, 제어 파라미터
├── Ez/                 # CalibrationData, CalibrationSettings, DeviceSettings / 캘리브레이션 데이터, 설정, 장치 설정
└── Pro/
    ├── ProToolInfo, SystemInfo, EncoderPosition, JobEvent, NgCause, RecipeVersion
    ├── Setting/        # Operation, Network, Barcode, Log, Sound, Share, ... / 동작, 네트워크, 바코드...
    └── Job/            # Job, Step, StepHeader, JobHeader, FastenBody, ...
    │                   # JobError, JobException / 작업, 스텝, 헤더, 오류 열거형, 예외 계층
```

---

## Format Classes / 포맷 클래스

### Device

| Class        | Size | Description                                                       |
|--------------|------|-------------------------------------------------------------------|
| `SimpleInfo` | 13B  | Basic device info (FC 0x11) / 기본 장치 정보 (FC 0x11)                  |
| `Info`       | 200B | Detailed device info (FC 0x04, Gen.2) / 상세 장치 정보 (FC 0x04, Gen.2) |
| `Status`     | —    | Device status registers / 장치 상태 레지스터                              |

### Process

| Class     | Description                             |
|-----------|-----------------------------------------|
| `Event`   | Fastening result event / 체결 결과 이벤트      |
| `Graph`   | Torque/angle graph data / 토크/각도 그래프 데이터 |
| `Barcode` | Barcode scan result / 바코드 스캔 결과         |

### Param

| Class       | Size | Registers   | Description                                                                       |
|-------------|------|-------------|-----------------------------------------------------------------------------------|
| `Preset`    | 60B  | 40002–40031 | Fastening preset configuration / 체결 프리셋 설정                                        |
| `AdvPreset` | 90B  | 41001–41045 | Advanced function preset with mode-specific typed views / 고급 기능 프리셋 (모드별 타입 뷰 제공) |
| `Control`   | 200B | 43651–43750 | Tool control parameters (6 groups) / 툴 제어 파라미터 (6개 그룹)                            |

### Ez

| Class                 | Description                                      |
|-----------------------|--------------------------------------------------|
| `CalibrationData`     | Torque meter calibration data / 토크 미터 캘리브레이션 데이터 |
| `CalibrationSettings` | Calibration settings / 캘리브레이션 설정                 |
| `DeviceSettings`      | EZTorQ device settings / EZTorQ 장치 설정            |

### Pro

| Class             | Size      | Description                                               |
|-------------------|-----------|-----------------------------------------------------------|
| `ProToolInfo`     | 47B / 80B | Scan/member tool info (scan=47B, member=80B) / 스캔/멤버 툴 정보 |
| `SystemInfo`      | —         | PRO X system information / PRO X 시스템 정보                   |
| `EncoderPosition` | —         | Encoder position data / 엔코더 위치 데이터                        |
| `JobEvent`        | —         | PRO X job event data / PRO X 작업 이벤트 데이터                   |
| `NgCause`         | —         | NG cause detail / NG 원인 상세                                |
| `RecipeVersion`   | —         | PRO X recipe version / PRO X 레시피 버전                       |

### Pro.Setting

| Class        | Description                                     |
|--------------|-------------------------------------------------|
| `Operation`  | PRO X operation settings / PRO X 동작 설정          |
| `Network`    | PRO X network settings / PRO X 네트워크 설정          |
| `Barcode`    | PRO X barcode settings / PRO X 바코드 설정           |
| `Log`        | PRO X log settings / PRO X 로그 설정                |
| `Sound`      | PRO X sound settings / PRO X 사운드 설정             |
| `Share`      | PRO X share settings / PRO X 공유 설정              |
| `InOut`      | PRO X I/O signal settings / PRO X I/O 신호 설정     |
| `Encoder`    | PRO X encoder settings / PRO X 엔코더 설정           |
| `IoToolName` | PRO X I/O tool name mapping / PRO X I/O 툴 이름 매핑 |

### Pro.Job

| Class / Enum               | Description                                                  |
|----------------------------|--------------------------------------------------------------|
| `Job`                      | PRO X job/recipe definition / PRO X 작업/레시피 정의                |
| `Step`                     | Individual step with header and body / 헤더와 바디를 포함한 개별 스텝     |
| `StepHeader`               | Step metadata (number, type) / 스텝 메타데이터 (번호, 유형)             |
| `JobHeader`                | Job-level metadata / 작업 수준 메타데이터                             |
| `FastenBody`               | Fastening step body / 체결 스텝 바디                               |
| `DelayBody`                | Delay step body / 지연 스텝 바디                                   |
| `InputBody`                | Input signal step body / 입력 신호 스텝 바디                         |
| `OutputBody`               | Output signal step body / 출력 신호 스텝 바디                        |
| `MessageBody`              | Message step body / 메시지 스텝 바디                                |
| `JobError`                 | Error type enum for job parsing/validation / 작업 파싱/검증 오류 열거형 |
| `JobException`             | Base exception class for job processing / 작업 처리 기본 예외 클래스    |
| `JobParseException`        | Exception during job parsing / 작업 파싱 중 예외                    |
| `UnknownRevisionException` | Unknown job revision detected / 알 수 없는 작업 리비전 감지             |
| `UnknownStepTypeException` | Unknown step type detected / 알 수 없는 스텝 유형 감지                 |
| `JobSerializeException`    | Exception during job serialization / 작업 직렬화 중 예외             |

---

## Usage / 사용법

### Parsing Pattern / 파싱 패턴

All format classes follow a consistent pattern:
모든 포맷 클래스는 일관된 패턴을 따릅니다:

```csharp
using HTool.Format.Device;

// constructor parsing (throws FormatException on invalid data)
// 생성자 파싱 (잘못된 데이터 시 FormatException 발생)
var info = new Info(responsePayload);
Console.WriteLine($"Model: {info.Controller}, FW: {info.Firmware}");

// TryParse for safe parsing without exceptions (returns false on failure)
// 예외 없는 안전한 파싱을 위한 TryParse (실패 시 false 반환)
if (SimpleInfo.TryParse(data, out var simpleInfo)) {
    Console.WriteLine($"Serial: {simpleInfo.Serial}");
}
```

### Process Event / 프로세스 이벤트

```csharp
using HTool.Format.Process;

// parse fastening result event from MODBUS response payload
// MODBUS 응답 페이로드에서 체결 결과 이벤트 파싱
var ev = new Event(payload);
// ev.Result holds OK/NG verdict; ev.Torque holds the measured torque value
// ev.Result에는 OK/NG 판정, ev.Torque에는 측정된 토크값이 포함됨
Console.WriteLine($"Result: {ev.Result}, Torque: {ev.Torque}");
```

### Preset / 프리셋

```csharp
using HTool.Format.Param;

// parse fastening preset from registers 40002–40031 (FC 0x03, 60 bytes)
// 레지스터 40002–40031에서 체결 프리셋 파싱 (FC 0x03, 60바이트)
if (Preset.TryParse(payload, out var preset)) {
    // Mode 0 = torque control (TC/AM), Mode 1 = angle control (AC/TM)
    // Mode 0 = 토크 제어 (TC/AM), Mode 1 = 각도 제어 (AC/TM)
    Console.WriteLine($"Mode: {preset.Mode}");
    // Torque / TorqueLimit meaning changes based on Mode
    // Torque / TorqueLimit 의미는 Mode에 따라 달라짐
    Console.WriteLine($"Torque: {preset.Torque}, Limit: {preset.TorqueLimit}");
    // convenience aliases for AC/TM mode — same value as Torque / TorqueLimit
    // AC/TM 모드용 편의 별칭 — Torque / TorqueLimit과 동일한 값
    Console.WriteLine($"MaxTorque: {preset.MaxTorque}, MinTorque: {preset.MinTorque}");
    // Hash allows efficient change detection without full field comparison
    // Hash를 통해 전체 필드 비교 없이 효율적인 변경 감지 가능
    Console.WriteLine($"Hash: {preset.Hash}");
}
```

### AdvPreset / 고급 프리셋

```csharp
using HTool.Format.Param;

// parse advanced function preset from registers 41001–41045 (FC 0x03, 90 bytes)
// 레지스터 41001–41045에서 고급 기능 프리셋 파싱 (FC 0x03, 90바이트)
if (AdvPreset.TryParse(payload, out var adv)) {
    // Mode determines which typed view to use for Parameters array
    // Mode에 따라 Parameters 배열 해석에 사용할 타입 뷰가 결정됨
    Console.WriteLine($"Mode: {adv.Mode}");

    // use AsSeatingDetection() when Mode == SeatingDetection (PLUS only)
    // Mode == SeatingDetection일 때 AsSeatingDetection() 사용 (PLUS 전용)
    var sd = adv.AsSeatingDetection();
    Console.WriteLine($"SeatingPointTorqueRate: {sd.SeatingPointTorqueRate}");

    // use AsPrevailingControl() when Mode == PrevailingControl (PLUS only)
    // Mode == PrevailingControl일 때 AsPrevailingControl() 사용 (PLUS 전용)
    var pc = adv.AsPrevailingControl();
    Console.WriteLine($"PrevailingSnugTorque: {pc.PrevailingSnugTorque}");

    // use AsOpenHole(isPlus) — NORMAL adds seating/angle fields not in PLUS
    // AsOpenHole(isPlus) 사용 — NORMAL은 PLUS에 없는 착좌/각도 필드를 추가로 포함
    var oh = adv.AsOpenHole(isPlus: false);
    Console.WriteLine($"StartTorque: {oh.StartTorque}, EndTorque: {oh.EndTorque}");

    // use AsEngagingTorque(isPlus) — torque resolution differs between PLUS and NORMAL
    // AsEngagingTorque(isPlus) 사용 — PLUS와 NORMAL 간 토크 해상도가 다름
    var et = adv.AsEngagingTorque(isPlus: true);
    Console.WriteLine($"Torque: {et.Torque}, AngleLimit: {et.AngleLimit}");
}
```

### Control / 제어 파라미터

```csharp
using HTool.Format.Param;

// parse tool control parameters from registers 43651–43750 (FC 0x03, 200 bytes)
// 레지스터 43651–43750에서 툴 제어 파라미터 파싱 (FC 0x03, 200바이트)
if (Control.TryParse(payload, out var ctrl)) {
    // Driver group — motor and torque control settings
    // Driver 그룹 — 모터 및 토크 제어 설정
    Console.WriteLine($"DriverId: {ctrl.DriverId}, TorqueUnit: {ctrl.TorqueUnit}");
    Console.WriteLine($"TorqueHoldingTime: {ctrl.TorqueHoldingTime}ms, LoosenSpeed: {ctrl.LoosenSpeed}rpm");

    // Operation group — fastening behavior settings
    // Operation 그룹 — 체결 동작 설정
    Console.WriteLine($"FasteningOkSignalTime: {ctrl.FasteningOkSignalTime}ms");
    Console.WriteLine($"DriverAutoLock: {ctrl.DriverAutoLock}, TriggerStart: {ctrl.TriggerStart}");

    // Crowfoot group — crowfoot attachment compensation
    // Crowfoot 그룹 — 크로우풋 부속품 보정
    Console.WriteLine($"CrowfootEnable: {ctrl.CrowfootEnable}, CrowfootRatio: {ctrl.CrowfootRatio}%");

    // Wireless Tools group — wireless-specific behavior
    // Wireless Tools 그룹 — 무선 전용 동작 설정
    Console.WriteLine($"SleepTime: {ctrl.SleepTime}min, LcdButtonLock: {ctrl.LcdButtonLock}");
    // SelectDisplayPresetNumber is a BIT flag — BIT.n = preset n+1 visible on LCD
    // SelectDisplayPresetNumber는 BIT 플래그 — BIT.n = LCD에 프리셋 n+1 표시
    Console.WriteLine($"DisplayPresets: 0x{ctrl.SelectDisplayPresetNumber:X}");
}
```

### PRO X Job / PRO X JOB

Job 바이너리는 리틀 엔디언 형식입니다 (MODBUS 빅 엔디언과 다름).
Job binaries use little-endian format — different from MODBUS big-endian.

#### 파싱 / Parsing

`Job.Parse()` — 예외 기반, `Job.TryParse()` — `JobError` 열거형으로 오류를 반환하는 안전한 버전.
`Job.Parse()` throws on failure; `Job.TryParse()` returns a `JobError` code for safe parsing.

```csharp
using HTool.Format.Pro.Job;

// ── TryParse (recommended for user-provided files) ─────────────────────────
// ── TryParse (사용자 제공 파일에 권장) ────────────────────────────────────────

// attempt to parse a job binary and receive a structured error code on failure
// 작업 바이너리 파싱 시도, 실패 시 구조적 오류 코드 반환
if (!Job.TryParse(payload, out var job, out var error)) {
    // error identifies exactly what went wrong during parsing
    // error는 파싱 중 발생한 문제를 정확히 식별함
    Console.WriteLine(error switch {
        // binary is smaller than the minimum header size
        // 바이너리가 최소 헤더 크기보다 작음
        JobError.FileTooSmall      => "file is too small",
        // first 8 bytes do not match "bmc.job." signature
        // 앞 8바이트가 "bmc.job." 시그니처와 일치하지 않음
        JobError.InvalidSignature  => "not a valid job file",
        // revision (Major.Minor) is not supported by this library version
        // 리비전(Major.Minor)이 이 라이브러리 버전에서 지원되지 않음
        JobError.UnknownVersion    => "unsupported job revision",
        // step count in header is outside the valid range 0–255
        // 헤더의 스텝 수가 유효 범위(0–255)를 벗어남
        JobError.InvalidStepCount  => "invalid step count",
        // one or more steps could not be parsed (unknown type or malformed data)
        // 하나 이상의 스텝 파싱 실패 (알 수 없는 유형 또는 잘못된 데이터)
        JobError.InvalidStep       => "invalid step data",
        // header counts do not match actual step counts after full parse
        // 전체 파싱 후 헤더 카운트와 실제 스텝 수 불일치
        JobError.CountMismatch     => "step count mismatch",
        _                          => "unknown error"
    });
    return;
}

// ── Parse (use when the source is trusted) ────────────────────────────────
// ── Parse (신뢰할 수 있는 소스에 사용) ───────────────────────────────────────

// guard: catch job-specific parsing errors with typed exceptions
try {
    // parse job binary — throws JobParseException hierarchy on any failure
    // 작업 바이너리 파싱 — 실패 시 JobParseException 계층 예외 발생
    var parsed = Job.Parse(payload);
} catch (UnknownRevisionException ex) {
    // revision not supported — ex.Major / ex.Minor identify the unsupported version
    // 지원되지 않는 리비전 — ex.Major / ex.Minor로 버전 식별
    Console.WriteLine($"Unsupported revision: {ex.Major}.{ex.Minor}");
} catch (UnknownStepTypeException ex) {
    // step type value not recognized — ex.TypeValue holds the raw integer
    // 인식되지 않는 스텝 유형 값 — ex.TypeValue에 원시 정수값이 포함됨
    Console.WriteLine($"Unknown step type: {ex.TypeValue}");
} catch (JobParseException ex) {
    // general parse failure: invalid signature, insufficient size, etc.
    // 일반 파싱 실패: 잘못된 시그니처, 데이터 부족 등
    Console.WriteLine($"Parse failed: {ex.Message}");
}
```

#### 헤더 / Job Header

```csharp
// Header contains job metadata — all counts are kept in sync by Save()
// Header는 작업 메타데이터 포함 — Save() 호출 시 모든 카운트가 자동 동기화됨
var h = job!.Header;
// Version identifies the binary format revision (Major >= 1 = Rev.1 step format)
// Version은 바이너리 형식 리비전을 식별 (Major >= 1 = Rev.1 스텝 형식)
Console.WriteLine($"Version: {h.Version}, Rev: {h.Revision}");
// Index is 1-based; Name is up to 128 ASCII characters
// Index는 1 기반, Name은 최대 128자 ASCII
Console.WriteLine($"Index: {h.Index}, Name: {h.Name}");
// step type counts — automatically updated when calling Save()
// 스텝 유형별 카운트 — Save() 호출 시 자동 갱신됨
Console.WriteLine($"Steps: {h.CountOfStep}, Screws: {h.CountOfScrew}");
Console.WriteLine($"Fasten: {h.CountOfFasten}, Input: {h.CountOfInput}");
Console.WriteLine($"Output: {h.CountOfOutput}, Delay: {h.CountOfDelay}, Message: {h.CountOfMessage}");
// FastenCount and StepCount are convenience properties on Job itself
// FastenCount와 StepCount는 Job의 편의 프로퍼티
Console.WriteLine($"job.FastenCount={job.FastenCount}, job.StepCount={job.StepCount}");
```

#### 스텝 순회 및 타입 분기 / Iterating Steps and Dispatching by Type

```csharp
// iterate all steps and dispatch to typed handler by step type
// 모든 스텝을 순회하며 스텝 유형별 타입 핸들러로 분기
foreach (var step in job!.Steps) {
    // Header.Name is a user-defined label (up to 128 ASCII characters)
    // Header.Name은 사용자 정의 레이블 (최대 128자 ASCII)
    Console.WriteLine($"[{step.Header.Type}] {step.Header.Name}");

    // cast Body to the concrete type that matches Header.Type
    // Body를 Header.Type에 맞는 구체적 타입으로 캐스트
    switch (step.Body) {
        case FastenBody f:
            // handle fastening step
            // 체결 스텝 처리
            HandleFasten(f);
            break;
        case DelayBody d:
            // handle delay step
            // 지연 스텝 처리
            HandleDelay(d);
            break;
        case InputBody i:
            // handle input wait step
            // 입력 대기 스텝 처리
            HandleInput(i);
            break;
        case OutputBody o:
            // handle output step
            // 출력 스텝 처리
            HandleOutput(o);
            break;
        case MessageBody m:
            // handle message display step
            // 메시지 표시 스텝 처리
            HandleMessage(m);
            break;
    }
}
```

#### FastenBody — 체결 스텝

```csharp
void HandleFasten(FastenBody f) {
    // ToolName identifies the target tool by name (36 bytes ASCII)
    // ToolName은 툴을 이름으로 식별 (36바이트 ASCII)
    Console.WriteLine($"Tool: {f.ToolName}, Preset: {f.Preset}");
    // CountOfScrew is the number of screw positions defined in this step
    // CountOfScrew는 이 스텝에 정의된 나사 위치 수
    Console.WriteLine($"Screws: {f.CountOfScrew}");

    // IsImage / ImagePath — optional overlay image for the UI fastening guide
    // IsImage / ImagePath — UI 체결 가이드용 오버레이 이미지 (선택)
    if (f.IsImage)
        Console.WriteLine($"Image: {f.ImagePath}");

    // iterate only the active screws (up to MaxScrewCount=99 slots, CountOfScrew are active)
    // 활성화된 나사만 순회 (최대 MaxScrewCount=99 슬롯, CountOfScrew개가 활성)
    for (var i = 0; i < f.CountOfScrew; i++) {
        // each Screw has screen coordinates, display radii, and RGB colors per state
        // 각 Screw는 화면 좌표, 표시 반경, 상태별 RGB 색상을 가짐
        var s = f.Screws[i];
        Console.WriteLine($"  #{i + 1}: Enable={s.Enable} X={s.X} Y={s.Y} R={s.Radius}");
        Console.WriteLine($"    Default=({s.DefaultColor.R},{s.DefaultColor.G},{s.DefaultColor.B})");
        Console.WriteLine($"    OK=({s.OkColor.R},{s.OkColor.G},{s.OkColor.B})");
        Console.WriteLine($"    NG=({s.NgColor.R},{s.NgColor.G},{s.NgColor.B})");
    }

    // Virtual — overrides preset parameters per-step without modifying the stored preset
    // Virtual — 저장된 프리셋을 수정하지 않고 스텝별로 프리셋 파라미터를 덮어씀
    if (f.Virtual.Enable)
        Console.WriteLine("Virtual preset is active");

    // IsReTight — enables automatic re-fastening if the result is NG
    // IsReTight — 결과가 NG일 때 자동 재체결 활성화
    if (f.IsReTight)
        Console.WriteLine($"Re-tight: max={f.MaxReTight}, preset={f.ReTightPreset}");

    // SocketNumber — socket tray slot index for automatic socket selection
    // SocketNumber — 자동 소켓 선택을 위한 소켓 트레이 슬롯 인덱스
    Console.WriteLine($"Socket: {f.SocketNumber}");

    // Rev.1 encoder data — only populated when Header.Revision >= 1
    // Rev.1 인코더 데이터 — Header.Revision >= 1일 때만 값이 채워짐
    if (f.EnableEncoder) {
        // EnableNonSeq allows screws to be fastened in any order
        // EnableNonSeq는 나사를 임의 순서로 체결 허용
        Console.WriteLine($"Encoder: NonSeq={f.EnableNonSeq}");
        // each ScrewEncoder holds 4 save positions, 4 zone tolerances, 4 OK tolerances, 2 pick-up enables
        // 각 ScrewEncoder는 저장 위치 4개, 존 허용 오차 4개, OK 허용 오차 4개, 픽업 활성화 2개를 가짐
        var enc = f.Encoders[0];
        Console.WriteLine($"  Enc[0]: SavePos={string.Join(",", enc.SavePos)}");
        Console.WriteLine($"           ZoneTol={string.Join(",", enc.ZoneTol)}");
        Console.WriteLine($"           OkTol={string.Join(",", enc.OkTol)}");
        Console.WriteLine($"           PickUp={string.Join(",", enc.EnabledPickUp)}");
    }
}
```

#### DelayBody — 지연 스텝

```csharp
void HandleDelay(DelayBody d) {
    // DelayType determines which fields are valid
    // DelayType에 따라 유효한 필드가 결정됨
    switch (d.DelayType) {
        case Delay.Time:
            // time-based delay — waits DelayTime in the specified unit before proceeding
            // 시간 기반 지연 — 지정 단위의 DelayTime 동안 대기 후 진행
            Console.WriteLine($"Wait {d.DelayTime} ({d.DelayTimeUnit})");
            break;
        case Delay.PopUp:
            // pop-up delay — displays Message[0..2] and waits for operator acknowledgement
            // 팝업 지연 — Message[0..2]를 표시하고 작업자 확인을 기다림
            Console.WriteLine($"PopUp: {d.Message[0]} / {d.Message[1]} / {d.Message[2]}");
            break;
        case Delay.Barcode:
            // barcode delay — waits until a barcode matching Code is scanned
            // 바코드 지연 — Code와 일치하는 바코드가 스캔될 때까지 대기
            Console.WriteLine($"Barcode: {d.Code}");
            // IsMask enables bit-level masking of the scanned barcode before comparison
            // IsMask는 비교 전 스캔된 바코드에 비트 수준 마스킹 적용
            if (d.IsMask)
                // Mask is a combined 64-bit value from MaskHigh (upper) + MaskLow (lower)
                // Mask는 MaskHigh(상위)와 MaskLow(하위)를 합성한 64비트 값
                Console.WriteLine($"Mask: 0x{d.Mask:X16} (H=0x{d.MaskHigh:X8} L=0x{d.MaskLow:X8})");
            break;
    }
}
```

#### InputBody — 입력 대기 스텝

```csharp
void HandleInput(InputBody i) {
    // InputType defines whether the step waits for rising, falling, or level signal
    // InputType은 스텝이 상승, 하강, 레벨 신호 중 어떤 것을 기다릴지 정의
    Console.WriteLine($"InputType: {i.InputType}");
    // IsPort[0..15] — which of the 16 I/O ports must satisfy the input condition
    // IsPort[0..15] — 16개 I/O 포트 중 입력 조건을 충족해야 하는 포트
    for (var p = 0; p < InputBody.PortCount; p++) {
        // skip ports that are not active in this step
        // 이 스텝에서 활성화되지 않은 포트 건너뜀
        if (!i.IsPort[p]) continue;
        Console.WriteLine($"  Port[{p}] active");
    }
}
```

#### OutputBody — 출력 스텝

```csharp
void HandleOutput(OutputBody o) {
    // Duration is the signal hold time in milliseconds
    // Duration은 신호 유지 시간 (밀리초)
    Console.WriteLine($"Duration: {o.Duration}ms");
    // each port has its own enable flag and output signal type (Pulse, Hold, etc.)
    // 각 포트는 자체 활성화 플래그와 출력 신호 유형 (펄스, 유지 등)을 가짐
    for (var p = 0; p < OutputBody.PortCount; p++) {
        // skip ports that are not active in this step
        // 이 스텝에서 활성화되지 않은 포트 건너뜀
        if (!o.IsPort[p]) continue;
        // PortType[p] specifies the electrical behavior for this port
        // PortType[p]는 해당 포트의 전기적 동작을 지정
        Console.WriteLine($"  Port[{p}]: {o.PortType[p]}");
    }
}
```

#### MessageBody — 메시지 스텝

```csharp
void HandleMessage(MessageBody m) {
    // Message[0..2] — up to 3 lines of display text (128 bytes ASCII each)
    // Message[0..2] — 최대 3줄 표시 텍스트 (각 128바이트 ASCII)
    Console.WriteLine($"Line1: {m.Message[0]}");
    Console.WriteLine($"Line2: {m.Message[1]}");
    Console.WriteLine($"Line3: {m.Message[2]}");
    // ImagePath — optional image shown alongside the message
    // ImagePath — 메시지와 함께 표시되는 선택적 이미지 경로
    if (!string.IsNullOrEmpty(m.ImagePath))
        Console.WriteLine($"Image: {m.ImagePath}");
    // MessageType controls how the operator proceeds past this step
    // MessageType은 작업자가 이 스텝을 넘어가는 방식을 제어
    switch (m.MessageType) {
        case StepMessage.Validation:
            // operator must press OK/NG to proceed — most common confirmation mode
            // 작업자가 OK/NG를 눌러야 진행 — 가장 일반적인 확인 모드
            Console.WriteLine("Mode: operator confirmation required");
            break;
        case StepMessage.NextStep:
            // automatically advances to the next step without operator input
            // 작업자 입력 없이 자동으로 다음 스텝으로 진행
            Console.WriteLine("Mode: auto advance");
            break;
        case StepMessage.DelayTime:
            // waits DelayTime milliseconds then automatically advances
            // DelayTime 밀리초 대기 후 자동으로 다음 스텝으로 진행
            Console.WriteLine($"Mode: timed auto advance after {m.DelayTime}ms");
            break;
    }
}
```

#### CRUD / 편집

```csharp
// Add a step to the end of the step list (max 255 steps)
// 스텝 목록 끝에 스텝 추가 (최대 255개)
job!.Add(new Step(new StepHeader { Type = JobStep.Delay, Name = "Wait" }, new DelayBody {
    DelayType = Delay.Time, DelayTime = 500, DelayTimeUnit = DelayTimeUnit.Ms
}));

// insert a step at index 2 (shifts existing steps down)
// 인덱스 2에 스텝 삽입 (기존 스텝이 아래로 이동)
job.InsertAt(2, new Step(new StepHeader { Type = JobStep.Message, Name = "Confirm" }, new MessageBody {
    MessageType = StepMessage.Validation
}));

// swap two steps by index
// 인덱스로 두 스텝 교환
job.Swap(0, 1);

// move step from index 3 to index 1
// 인덱스 3의 스텝을 인덱스 1로 이동
job.Move(3, 1);

// deep-copy step at index 0 (serialize + re-parse internally)
// 인덱스 0의 스텝을 깊은 복사 (내부적으로 직렬화 후 재파싱)
var copied = job.Copy(0);

// replace step at index 0 with the copied step
// 인덱스 0의 스텝을 복사된 스텝으로 교체
job.ReplaceAt(0, copied);

// remove step at index 4
// 인덱스 4의 스텝 제거
job.RemoveAt(4);

// serialize to binary — header counts are synchronized automatically before writing
// 바이너리로 직렬화 — 쓰기 전 헤더 카운트가 자동으로 동기화됨
byte[] binary = job.Save();
File.WriteAllBytes("output.job", binary);
```

### PRO X Tool Info / PRO X 툴 정보

```csharp
using HTool.Format.Pro;

// ProToolInfo supports two sizes: ScanSize (47B) for discovered tools,
// MemberSize (80B) for registered tools — Name and Status fields are null/false in scan mode
// ProToolInfo는 두 가지 크기 지원: 발견된 툴용 ScanSize (47B),
// 등록된 툴용 MemberSize (80B) — 스캔 모드에서 Name과 Status는 null/false
if (ProToolInfo.TryParse(data, out var toolInfo)) {
    // IsMember is true when Name field is present (member-size payload)
    // Name 필드가 있을 때 IsMember가 true (멤버 크기 페이로드)
    Console.WriteLine($"IsMember: {toolInfo.IsMember}");
    Console.WriteLine($"Model: {toolInfo.Model}, Serial: {toolInfo.Serial}");
    Console.WriteLine($"IP: {toolInfo.IpAddress}:{toolInfo.Port}, MAC: {toolInfo.Mac}");
    // Name and Status are only populated for member tools (80-byte payload)
    // Name과 Status는 멤버 툴(80바이트 페이로드)에서만 채워짐
    if (toolInfo.IsMember)
        Console.WriteLine($"Name: {toolInfo.Name}, Status: {toolInfo.Status}");
}
```

---

## Design / 설계

- All format classes are `readonly struct` (or `readonly record struct`) for value semantics and stack allocation
  모든 포맷 클래스는 값 의미론 및 스택 할당을 위해 `readonly struct` (또는 `readonly record struct`) 사용
- Constructors parse from `ReadOnlySpan<byte>` using `BinarySpanReader` — zero allocation
  생성자는 `BinarySpanReader`를 이용해 `ReadOnlySpan<byte>`에서 파싱 — 할당 없음
- `TryParse` static methods available for safe parsing without exceptions
  예외 없이 안전하게 파싱하는 `TryParse` 정적 메서드 제공
- `DataHash` field on each class enables efficient change detection without full field comparison
  각 클래스의 `DataHash` 필드로 전체 필드 비교 없이 효율적인 변경 감지 가능
- `AdvPreset` exposes mode-specific typed views via `AsXxx()` methods over a shared `float[]` parameter array
  `AdvPreset`은 공유 `float[]` 파라미터 배열에 대해 `AsXxx()` 메서드로 모드별 타입 뷰 제공
- All numeric fields are Big-Endian (MODBUS network byte order)
  모든 숫자 필드는 빅 엔디언 (MODBUS 네트워크 바이트 순서)
- `JobError` enum and `JobException` hierarchy provide structured error handling for job parsing
  `JobError` 열거형과 `JobException` 계층은 작업 파싱의 구조적 오류 처리를 제공

---

## License / 라이선스

MIT License - Copyright (c) HANTAS
