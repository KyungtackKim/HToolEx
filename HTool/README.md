# HTool

Unified MODBUS communication library for HANTAS industrial torque tools.

HANTAS 산업용 토크 툴을 위한 통합 MODBUS 통신 라이브러리

[![NuGet](https://img.shields.io/badge/nuget-v2.0.0-blue)](https://www.nuget.org/packages/Hantas.HTool)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://www.microsoft.com/windows)

---

## Overview / 개요

HTool v2 provides a single API for both direct MODBUS RTU/TCP connections and PRO X gateway connections. The library
internally handles protocol differences transparently.

HTool v2는 직접 MODBUS RTU/TCP 연결과 PRO X 게이트웨이 연결을 동일한 API로 제공합니다. 라이브러리가 프로토콜 차이를 내부적으로 투명하게 처리합니다.

### Key Features / 주요 기능

- **Unified API**: Same `HTool` class for RTU, TCP, and PRO X
  **통합 API**: RTU, TCP, PRO X에 동일한 `HTool` 클래스 사용
- **Auto-split**: Large register reads automatically split into 125-register chunks
  **자동 분할**: 대용량 레지스터 읽기를 125개 단위로 자동 분할
- **Keep-Alive**: Automatic connection health monitoring
  **연결 유지**: 자동 연결 상태 모니터링
- **PRO X Gateway**: Multi-tool management, event subscription, FTP operations
  **PRO X 게이트웨이**: 다중 툴 관리, 이벤트 구독, FTP 작업
- **Structured Logging**: Category-based logger with file/console/event output
  **구조적 로깅**: 파일/콘솔/이벤트 출력이 가능한 카테고리 기반 로거

---

## Installation / 설치

```bash
dotnet add package Hantas.HTool
```

---

## Quick Start / 빠른 시작

### Direct TCP Connection / 직접 TCP 연결

```csharp
using HTool;
using HTool.Type;

// create HTool instance fixed to TCP mode (ComType cannot change after construction)
// TCP 모드로 고정된 HTool 인스턴스 생성 (생성 후 ComType 변경 불가)
var tool = new HTool.HTool(ComType.Tcp);

// register connection state change handler — fires on connect and disconnect
// 연결 상태 변경 핸들러 등록 — 연결 및 해제 시 호출됨
tool.ChangedConnect += connected => {
    // print device serial number when connection is successfully established
    // 연결 성공 시 장치 시리얼 번호 출력
    if (connected)
        Console.WriteLine($"Connected: {tool.Info.Serial}");
};

// register MODBUS response handler — called for every successful register read/write
// MODBUS 응답 핸들러 등록 — 레지스터 읽기/쓰기 성공 시마다 호출됨
tool.ReceivedData += response => {
    // log the function code and starting register address of the response
    // 응답의 기능 코드와 시작 레지스터 주소 출력
    Console.WriteLine($"FC=0x{(byte)response.Code:X2} Addr={response.Address}");
};

// register communication error handler — called on timeout, CRC failure, or protocol error
// 통신 오류 핸들러 등록 — 타임아웃, CRC 오류, 프로토콜 오류 발생 시 호출됨
tool.ReceiveError += error => {
    Console.WriteLine($"Error: {error.Reason}");
};

// enable keep-alive before connecting — polls device periodically to detect silent disconnection
// 연결 전 연결 유지 활성화 — 주기적으로 장치를 폴링하여 조용한 연결 끊김 감지
tool.EnableKeepAlive = true;
// connect to device at the given IP and MODBUS TCP standard port 502
// 지정된 IP와 MODBUS TCP 표준 포트 502로 장치에 연결
tool.Connect("192.168.1.100", 502);

// read 10 holding registers starting at address 0x0000 (FC 0x03)
// 0x0000 주소부터 홀딩 레지스터 10개 읽기 (FC 0x03)
tool.ReadHoldingReg(0x0000, 10);
// write value 1 to the single holding register at address 0x0100 (FC 0x06)
// 0x0100 주소의 단일 홀딩 레지스터에 값 1 쓰기 (FC 0x06)
tool.WriteSingleReg(0x0100, 1);
```

### Direct RTU Connection / 직접 RTU 연결

```csharp
// create HTool instance fixed to RTU mode (MODBUS RTU over serial port)
// RTU 모드로 고정된 HTool 인스턴스 생성 (시리얼 포트 MODBUS RTU 통신)
var tool = new HTool.HTool(ComType.Rtu);
// open COM3 at 115200 baud — other serial parameters use MODBUS defaults (8-N-1)
// 115200 보드레이트로 COM3 포트 열기 — 나머지 시리얼 파라미터는 MODBUS 기본값 사용 (8-N-1)
tool.Connect("COM3", 115200);
```

### PRO X Gateway Connection / PRO X 게이트웨이 연결

```csharp
// create HTool instance in PRO X gateway mode
// PRO X 게이트웨이 모드로 HTool 인스턴스 생성
var tool = new HTool.HTool(ComType.Pro);

// register connection state change handler
// 연결 상태 변경 핸들러 등록
tool.ChangedConnect += connected => {
    // skip disconnection events — member tool list is only valid while connected
    // 연결 해제 이벤트 무시 — 멤버 툴 목록은 연결 중에만 유효
    if (!connected) return;

    // retrieve the list of member tools registered on the PRO X gateway
    // PRO X 게이트웨이에 등록된 멤버 툴 목록 조회
    var members = tool.Pro!.Tools.MemberTools;
    // print the number of tools currently connected to the gateway
    // 게이트웨이에 현재 연결된 툴 수 출력
    Console.WriteLine($"Tools: {members.Count}");

    // select the first tool (index 0) as the target for subsequent MODBUS operations
    // 이후 MODBUS 작업의 대상으로 첫 번째 툴(인덱스 0) 선택
    // returns false if the gateway is busy or the index is out of range
    // 게이트웨이가 사용 중이거나 인덱스 범위 초과 시 false 반환
    tool.Pro.Tools.TrySelectTool(0);
};

// register tool event handler — receives fastening events as IFastenEvent (direct or PRO X)
// 툴 이벤트 핸들러 등록 — 직접/PRO X 공용 IFastenEvent로 체결 이벤트 수신
tool.Pro!.EventDataReceived += ev => {
    // ev.Analysis.EventStatus carries the OK/NG verdict; ev.Source distinguishes Direct vs Pro
    // ev.Analysis.EventStatus가 OK/NG 판정, ev.Source는 출처(Direct/Pro) 구분
    Console.WriteLine($"Tool Event: {ev.Analysis.EventStatus} (Source={ev.Source}, Id={ev.Id})");
};

// register job event handler — receives step-level completion notifications during a job
// 작업 이벤트 핸들러 등록 — 작업 진행 중 스텝 단위 완료 알림 수신
tool.Pro.JobEventReceived += job => {
    // job.StepNumber identifies which step in the job recipe just completed
    // job.StepNumber는 작업 레시피에서 방금 완료된 스텝 번호를 나타냄
    Console.WriteLine($"Job Event: {job.StepNumber}");
};

// connect to the PRO X gateway at the given IP and port
// 지정된 IP와 포트로 PRO X 게이트웨이에 연결
tool.Connect("192.168.1.200", 4545);

// subscribe to tool events — gateway pushes fastening results to EventDataReceived
// 툴 이벤트 구독 — 게이트웨이가 체결 결과를 EventDataReceived로 푸시
tool.Pro.SubscribeToolEvent();
// subscribe to job events — gateway pushes step results to JobEventReceived
// 작업 이벤트 구독 — 게이트웨이가 스텝 결과를 JobEventReceived로 푸시
tool.Pro.SubscribeJobEvent();

// MODBUS read is transparently routed through the currently selected tool on the gateway
// MODBUS 읽기가 게이트웨이의 현재 선택된 툴을 통해 투명하게 전달됨
tool.ReadHoldingReg(0x0000, 10);
```

---

## Architecture / 아키텍처

```
HTool (entry point / 진입점)
├── Transport/          # ITransport → RtuTransport, TcpTransport
├── Codec/              # IModbusCodec → ModbusRtuCodec, ModbusTcpCodec, ProCodec
├── Protocol/           # ModbusRequest/Response, ProRequest/Message
├── MessagePipeline     # Request queue, retry, timeout (direct mode) / 요청 큐, 재시도, 타임아웃 (직접 모드)
├── Pro/
│   ├── ProService      # PRO X protocol handler (MID routing, Keep-Alive) / PRO X 프로토콜 처리기
│   ├── ToolService     # Multi-tool state management / 다중 툴 상태 관리
│   └── FtpService      # File operations via FluentFTP / FluentFTP 파일 작업
├── HToolLogger         # Category-based structured logging / 카테고리 기반 구조적 로깅
└── Type/               # ComType, Connection, FunctionCode, ComError, ...
```

---

## Logger / 로거

```csharp
// enable only Connection and Error categories to reduce log noise
// 불필요한 로그를 줄이기 위해 Connection과 Error 카테고리만 활성화
tool.Logger.EnabledCategories = LogCategories.Connection | LogCategories.Error;
// set minimum log level — entries below Info will be silently suppressed
// 최소 로그 레벨 설정 — Info 미만 항목은 무시됨
tool.Logger.MinLevel = LogLevel.Info;

// enable file output — log entries are appended to the specified file path
// 파일 출력 활성화 — 로그 항목이 지정 파일 경로에 추가됨
tool.Logger.EnableFile("htool.log");

// register event handler to receive log entries directly in application code
// 애플리케이션 코드에서 로그 항목을 직접 수신하기 위한 이벤트 핸들러 등록
tool.Logger.LogReceived += entry => {
    // entry.Category identifies which subsystem produced the log entry
    // entry.Category는 로그 항목을 생성한 서브시스템을 식별함
    Console.WriteLine($"[{entry.Category}] {entry.Message}");
};

// enable console output — log entries are written to standard output in real time
// 콘솔 출력 활성화 — 로그 항목이 실시간으로 표준 출력에 기록됨
tool.Logger.ConsoleEnabled = true;
```

### Log Categories / 로그 카테고리

| Category     | Description                               |
|--------------|-------------------------------------------|
| `Packet`     | Raw MODBUS packet data / 원시 MODBUS 패킷 데이터 |
| `Connection` | Connect/disconnect events / 연결/해제 이벤트     |
| `Pipeline`   | Message queue and retry / 메시지 큐 및 재시도     |
| `KeepAlive`  | Keep-Alive ping/timeout / 연결 유지 핑/타임아웃    |
| `Pro`        | PRO X protocol messages / PRO X 프로토콜 메시지  |
| `Ftp`        | FTP file operations / FTP 파일 작업           |
| `Error`      | Communication errors / 통신 오류              |

---

## FTP (PRO X) / FTP 기능

```csharp
// access the FTP service — non-null only when connected in Pro mode
// FTP 서비스 접근 — Pro 모드로 연결된 경우에만 non-null
var ftp = tool.Pro!.Ftp!;

// sync remote log directory to local path — only downloads files that changed since last sync
// 원격 로그 디렉토리를 로컬 경로로 동기화 — 마지막 동기화 이후 변경된 파일만 다운로드
var result = await ftp.SyncDirectoryAsync(
    "/remote/logs", @"C:\local\logs",
    // report download progress as a percentage via IProgress<FtpProgress>
    // IProgress<FtpProgress>를 통해 다운로드 진행률을 백분율로 보고
    progress: new Progress<FtpProgress>(p => Console.WriteLine($"{p.Progress:F1}%")),
    ct: cancellationToken);

// print how many files were downloaded vs. skipped (already up-to-date)
// 다운로드된 파일 수와 건너뛴 파일 수(이미 최신) 출력
Console.WriteLine($"Updated: {result.FilesUpdated}, Skipped: {result.FilesSkipped}");

// deploy firmware atomically: upload to temp name → chmod → delete old binary → rename to target
// 펌웨어 원자적 배포: 임시 이름으로 업로드 → chmod → 기존 바이너리 삭제 → 대상 이름으로 변경
await ftp.DeployFirmwareAsync(@"C:\firmware\update.bin", "/firmware/app.bin", ct: cancellationToken);
```

---

## Settings / 설정

```csharp
// configure global (static) settings — changes apply to all HTool instances
// 전역(정적) 설정 구성 — 변경 사항이 모든 HTool 인스턴스에 적용됨
// MODBUS response wait timeout (ms) — request is retried or fails if no reply arrives in time
// MODBUS 응답 대기 타임아웃(ms) — 시간 내 응답 없으면 재시도 또는 실패 처리
HTool.HTool.Settings.MessageTimeout = 1000;
// number of retries after timeout before ReceiveError event is raised
// ReceiveError 이벤트 발생 전 타임아웃 후 재시도 횟수
HTool.HTool.Settings.MessageRetry = 1;
// incomplete frame discard timeout (ms) — partial bytes in receive buffer are dropped after this
// 불완전 프레임 폐기 타임아웃(ms) — 이 시간 후 수신 버퍼의 불완전 바이트 폐기
HTool.HTool.Settings.FrameTimeout = 500;
// TCP connect timeout (ms) — ConnectAsync fails if the TCP handshake is not completed in time
// TCP 연결 타임아웃(ms) — TCP 핸드셰이크가 시간 내 완료되지 않으면 ConnectAsync 실패
HTool.HTool.Settings.ConnectTimeout = 5000;
```

---

## Dependencies / 의존성

| Package         | Version | Description                               |
|-----------------|---------|-------------------------------------------|
| HTool.Core      | 1.0.0   | Shared types and utilities / 공유 타입 및 유틸리티 |
| HTool.Format    | 1.0.0   | Data parsing classes / 데이터 파싱 클래스         |
| FluentFTP       | 53.0.2  | FTP operations (PRO X) / FTP 작업 (PRO X)   |
| System.IO.Ports | 10.0.3  | Serial port (RTU) / 시리얼 포트 (RTU)          |

---

## License / 라이선스

MIT License - Copyright (c) HANTAS
