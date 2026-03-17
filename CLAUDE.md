# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

한타스(HANTAS) 제품군의 지원 라이브러리를 담은 솔루션. MODBUS RTU/TCP 프로토콜로 산업용 토크 공구 및 토크 미터와 통신한다.

**Company**: HANTAS
**Target Framework**: .NET 8.0 Windows 10.0.17763.0+
**Language**: C# 12
**Primary Protocol**: MODBUS RTU/TCP

## Build and Development Commands

```bash
# Build entire solution
dotnet build HTool.sln

# Build specific project in Release mode
dotnet build HTool.Core/HTool.Core.csproj -c Release
dotnet build HTool.Format/HTool.Format.csproj -c Release
dotnet build HTool/HTool.csproj -c Release
dotnet build HToolEx/HToolEx.csproj -c Release
dotnet build HToolEz/HToolEz.csproj -c Release

# Create NuGet packages (GeneratePackageOnBuild is enabled)
dotnet pack HTool.Core/HTool.Core.csproj -c Release
dotnet pack HTool.Format/HTool.Format.csproj -c Release
dotnet pack HTool/HTool.csproj -c Release

# Clean and restore
dotnet clean HTool.sln
dotnet restore HTool.sln
```

No unit test infrastructure exists. The `Tester/` directory is currently empty.

## High-Level Architecture

### Solution Structure

```
HTool.sln
├── HTool.Core/         # 공유 유틸리티 및 도메인 타입 (v1.0.0, .NET 8.0)
├── HTool.Format/       # 데이터 파싱 라이브러리 (v1.0.0, .NET 8.0)
├── HTool/              # 통합 MODBUS 통신 라이브러리 — 직접 RTU/TCP + PRO X (v2.0.0)
├── HToolEx/            # PRO X 장비 지원, ParaMon 4에서 사용 중 (v1.1.18, 레거시 전환 예정)
├── HToolEz/            # 한타스 토크미터 지원 (v0.0.20, x64 only)
├── HToolLegacy/        # HTool v1 아카이브 (v1.1.24, 신규 개발 금지)
├── HComm/              # 구버전 한타스 툴 지원 (.NET Standard 2.0, legacy)
└── HCommEz/            # 구버전 한타스 토크미터 지원 (.NET Standard 2.0, legacy)
```

**Platform**: HTool, HTool.Core, HTool.Format target AnyCPU (.NET 8.0). HToolEz targets **win-x64** only.

**주력 개발 대상**: HTool (v2), HTool.Core, HTool.Format, HToolEz
**유지보수 중**: HToolEx (ParaMon 4에서 사용 중이므로 지원 계속)
**레거시**: HToolLegacy, HComm, HCommEz (신규 기능 추가 금지)

**Migration 상태**: HTool v2가 HToolEx의 PRO X 기능(ProService, ToolService, FtpService)을 통합 완료. HToolEx는 ParaMon 4 마이그레이션 후 레거시로 전환 예정.

### Dependency Graph

```
HTool.Core (types, utilities — no dependencies)
    ↑
HTool.Format (data parsing — depends on HTool.Core)
    ↑
HTool (communication — depends on HTool.Core + HTool.Format + FluentFTP + System.IO.Ports)
```

### HTool.Core (Shared Foundation)

유틸리티와 도메인 타입을 제공하는 공유 기반 라이브러리. Windows 종속성 없음.

**Key namespaces**:
- `HTool.Core.Util`: KeyedQueue, RingBuffer, BinarySpanReader, DataHash, Utils, EnumUtil
- `HTool.Core.Type.Device`: Model, Manufacturer, ModelNames
- `HTool.Core.Type.Process`: Event, Direction, Unit, GraphStep, GraphChannel, etc.
- `HTool.Core.Type.Pro`: MessageId, JobStep, JobEvent, LogField, etc.
- `HTool.Core.Type.Ez`: DeviceCommand, CalPoint, Frequency, etc.

**Dependencies**: System.IO.Hashing (9.0.4)

### HTool.Format (Data Parsing)

MODBUS 응답 페이로드를 강타입 `readonly struct`로 파싱하는 라이브러리.

**Key namespaces**:
- `HTool.Format.Device`: SimpleInfo (13B, FC 0x11), Info (200B, FC 0x04), Status
- `HTool.Format.Process`: Event, Graph, Barcode
- `HTool.Format.Ez`: CalibrationData, CalibrationSettings, DeviceSettings
- `HTool.Format.Pro`: ToolInfo, SystemInfo, JobEvent, NgCause, RecipeVersion
- `HTool.Format.Pro.Setting`: Operation, Network, Barcode, Log, Sound, Share, InOut, Encoder, IoToolName
- `HTool.Format.Pro.Job`: Job, Step, StepHeader, JobHeader, FastenBody, DelayBody, InputBody, OutputBody, MessageBody

**Dependencies**: HTool.Core

### HTool (Unified Communication — v2)

직접 RTU/TCP 연결과 PRO X 게이트웨이 연결을 동일한 API로 제공하는 통합 라이브러리.

**Key namespaces**:
- `HTool`: HTool (main entry point, unified API)
- `HTool.Device.Transport`: ITransport, RtuTransport, TcpTransport
- `HTool.Device.Codec`: IModbusCodec, ModbusRtuCodec, ModbusTcpCodec, ProCodec
- `HTool.Device.Protocol`: ModbusRequest, ModbusResponse, ProRequest, ProMessage
- `HTool.Device`: MessagePipeline (request queue, retry, timeout), HToolLogger
- `HTool.Device.Pro`: ProService (PRO X protocol), ToolService (multi-tool state), FtpService
- `HTool.Type`: ComType, Connection, FunctionCode, ComError, ComErrorCode, ModbusExceptionCode, LogLevel, LogCategories, LogEntry, FtpSyncState, FtpSyncResult, HToolSettings

**Dependencies**: HTool.Core, HTool.Format, FluentFTP (53.0.2), System.IO.Ports (10.0.3)

### HToolEx (Extended Library — Legacy Transition)

ParaMon-Pro X 게이트웨이를 통한 다중 툴 관리 라이브러리. ParaMon 4에서 사용 중이므로 유지보수 계속하되, HTool v2로 마이그레이션 후 레거시 전환 예정.

**Key namespaces**:
- `HToolEx.Device`: Extended device implementations
- `HToolEx.ProEx`: Professional extended features
  - `ProEx.Manager`: ToolManager, SessionManager, FtpManager
  - `ProEx.Format`: ProX-specific message formats (19+ format classes)
  - `ProEx.FormatJob`: Job/recipe definitions with step types
- `HToolEx.Localization`: Multi-language support (EN, DE, ES, FR)

**Dependencies**: FluentFTP (49.0.1), JetBrains.Annotations (2025.2.4)

### HToolEz (EZTorQ-III Library)

한타스 토크미터(EZTorQ-III) 통신 및 캘리브레이션 전용 라이브러리.

**Key namespaces**:
- `HToolEz.Device`: DeviceService, DeviceHelper, DeviceServiceFactory
- `HToolEz.Format`: FormatCalData, FormatCalSetData, FormatMessage, FormatSetData
- `HToolEz.Type`: 12 device-specific enumerations
- `HToolEz.Util`: Constants, KeyedQueue, RingBuffer, Utils (shared utility copies)

**Dependencies**: System.IO.Ports (9.0.10)

## Important Patterns and Conventions

### 1. Constructor and Connection (HTool v2)

```csharp
// Direct connection (RTU/TCP) — type fixed at construction
var tool = new HTool.HTool(ComType.Tcp);
await tool.ConnectAsync("192.168.1.1", 502);

// Parameterless constructor — type specified at connect time
var tool = new HTool.HTool();
await tool.ConnectAsync(ComType.Tcp, "192.168.1.1", 502);

// ComType switching on the same instance
tool.Close();
await tool.ConnectAsync(ComType.Rtu, "COM3", 115200);  // rebuilds internal components
tool.Close();
await tool.ConnectAsync(ComType.Pro, "192.168.1.200", 80);  // switches to PRO X

// Per-instance settings (configure before ConnectAsync)
tool.Settings.Pipeline.MessageTimeout = 2000;
tool.Settings.KeepAlive.Enabled = true;
tool.Settings.Connection.TcpConnectTimeout = 3000;
```

### 2. Event-Driven Architecture (HTool v2)

```csharp
// Connection state
tool.ChangedConnect += (bool connected) => { };

// MODBUS response
tool.ReceivedData += (ModbusResponse response) => { };

// Communication error
tool.ReceiveError += (ComError error) => { };

// PRO X events (when Type is ComType.Pro)
tool.Pro!.EventDataReceived += (Event ev) => { };
tool.Pro.JobEventReceived += (JobEvent job) => { };
tool.Pro.MemberToolsChanged += (IReadOnlyList<ToolInfo> tools) => { };
```

### 3. Thread-Safe KeyedQueue Pattern

```csharp
var queue = KeyedQueue<ModbusRequest, ModbusRequest.RequestKey>
    .Create(static r => r.Key, capacity: 64);

queue.TryEnqueue(request, EnqueueMode.EnforceUnique);
```

Features: O(1) enqueue/dequeue, thread-safe Lock-based synchronization, blocking/non-blocking operations, timeout support.

### 4. MODBUS Protocol Implementation

**Function Codes** (FunctionCode enum):
- `0x03`: Read Holding Registers
- `0x04`: Read Input Registers
- `0x06`: Write Single Register
- `0x10`: Write Multiple Registers
- `0x11`: Read Device Information (custom)
- `0x64`: Graph Data (custom)
- `0x65`: Graph Result (custom)
- `0x66`: High Resolution Graph (custom)

**Message Lifecycle (Direct Mode)**:
1. Call `ReadHoldingReg()` / `WriteSingleReg()` / etc.
2. Codec builds MODBUS frame (RTU with CRC / TCP with MBAP header)
3. ModbusRequest enqueued to MessagePipeline (KeyedQueue, 50ms timer)
4. Response matched to request by FunctionCode + Address
5. Retry logic with configurable timeout (default: 1000ms)

**Message Lifecycle (PRO X Mode)**:
1. Same API calls → internally wrapped as MODBUS passthrough (MID 110/111)
2. ProCodec encapsulates MODBUS frame in PRO X 16-byte header
3. ProService routes request to selected tool
4. Response unwrapped and delivered via same ReceivedData event

**Max register sizes**: Read 125, Write 123 per request. Auto-split for larger reads.

### 5. Connection State Management

**States**: Close/Closed → Connecting → Connected

**Direct Mode Flow**: `Connect()` → Connecting → Transport connected → `ReadInfoReg()` → Parse SimpleInfo → Connected

**PRO X Mode Flow**: `Connect()` → Connecting → Transport connected → ProService.Start() → Member tools received → Connected

**ComType Switching**: `Close()` → `Connect(ComType, ...)` → `BuildComponents()` (disposes old, creates new) → `ConnectInternal()`

**Same-Type Reconnect**: `Close()` → `Connect(...)` → reuses existing Transport/Pipeline (no rebuild)

**Keep-Alive** (defaults, configurable via `Settings.KeepAlive` / `Settings.Pro`):
- Direct: 3s period, 10s timeout (ReadInfoReg polling)
- PRO X: 5s period, 15s timeout, 3 missed limit (ProService internal)

### 6. Format Classes Pattern (v2)

All format classes are `readonly struct` with `ReadOnlySpan<byte>` constructors:

```csharp
public readonly struct SimpleInfo {
    public static int Size => 13;

    public SimpleInfo(ReadOnlySpan<byte> data) {
        var pos = 0;
        Id = BinarySpanReader.ReadUInt16(data, ref pos);
        // ... zero-allocation Big-Endian parsing
    }

    public static bool TryParse(ReadOnlySpan<byte> data, out SimpleInfo result) { ... }
}
```

### 7. PRO X Multi-Tool Architecture (HTool v2)

**Integrated Services**:
- `ProService`: MID protocol handler (Keep-Alive, event routing, MODBUS passthrough)
- `ToolService`: Member/scan tool list management (volatile immutable swap)
- `FtpService`: FluentFTP-based file operations (lazy connection, atomic firmware deploy)

**Tool Selection**:
```csharp
tool.Pro!.Tools.SelectedToolId = serialNumber;
tool.ReadHoldingReg(addr, count); // Transparently targets selected tool
```

**Event Subscription**:
```csharp
tool.Pro.SubscribeToolEvent();     // Subscribe to tool events
tool.Pro.SubscribeJobEvent();      // Subscribe to job events
tool.Pro.UnsubscribeToolEvent();   // Unsubscribe
```

### 8. Structured Logging

```csharp
tool.Logger.EnabledCategories = LogCategories.Connection | LogCategories.Error;
tool.Logger.MinLevel = LogLevel.Info;
tool.Logger.EnableFile("htool.log");
tool.Logger.LogReceived += (LogEntry entry) => { };
```

Categories: Packet, Connection, Pipeline, KeepAlive, Pro, Ftp, Error

### 9. Binary Protocol Utilities

- `BinarySpanReader`: Zero-allocation Big-Endian reader using `BinaryPrimitives`
- `Utils.Crc16()`: MODBUS CRC-16 calculation
- `DataHash`: XxHash3-based change detection
- `RingBuffer`: Streaming data parsing buffer (typically 16KB)

### 10. Coding Conventions

**Naming (HTool v2)**:
- Direct class names: `HTool`, `ProService`, `ToolService`, `FtpService`
- Format classes: `SimpleInfo`, `Info`, `Status`, `Event`, `Graph` (no prefix)
- Type enums: `ComType`, `Connection`, `FunctionCode` (no suffix)

**Threading**: Timer-based processing (System.Timers.Timer, 50ms for direct / 100ms for PRO X), KeyedQueue with Lock-based synchronization, volatile immutable swap for tool lists.

**Memory**: ArrayPool<byte> for receive buffers, ReadOnlySpan<byte> for zero-copy parsing, IDisposable pattern for cleanup.

## Development Workflow

### Adding New Features

1. **New Domain Type**: Add to `HTool.Core/Type/` in appropriate subdirectory (Device/Process/Pro/Ez)
2. **New Format Class**: Add `readonly struct` to `HTool.Format/` with `BinarySpanReader` parsing
3. **New Function Code**: Add to `HTool.Core/Type/Process/` FunctionCode enum
4. **New Device Model**: Update `HTool.Core/Type/Device/Model.cs` and `ModelNames.cs`
5. **New PRO X Feature**: Extend `ProService` or add to `HTool/Device/Pro/`

### Debugging

```csharp
// Structured logging
tool.Logger.EnabledCategories = LogCategories.All;
tool.Logger.ConsoleEnabled = true;

// Connection diagnostics
tool.ReceiveError += error => Console.WriteLine($"Error: {error.Reason} — {error.Detail}");

// PRO X diagnostics
tool.Pro!.MessageReceived += msg => Console.WriteLine($"MID={msg.Mid} Rev={msg.Revision}");
tool.Pro.ErrorReceived += (mid, code) => Console.WriteLine($"PRO X Error: MID={mid} Code={code}");
```

## Key Files Reference

**HTool.Core**:
- `HTool.Core/Util/KeyedQueue.cs` — Thread-safe keyed queue
- `HTool.Core/Util/BinarySpanReader.cs` — Big-Endian binary reader
- `HTool.Core/Util/DataHash.cs` — XxHash3 change detection
- `HTool.Core/Type/Device/ModelNames.cs` — Model code ↔ name lookup

**HTool.Format**:
- `HTool.Format/Device/SimpleInfo.cs` — Basic device info (FC 0x11)
- `HTool.Format/Device/Info.cs` — Detailed device info (FC 0x04)
- `HTool.Format/Pro/Job/Job.cs` — PRO X job/recipe definition

**HTool**:
- `HTool/HTool.cs` — Main entry point (unified API)
- `HTool/Device/MessagePipeline.cs` — Request queue, retry, timeout
- `HTool/Device/HToolLogger.cs` — Category-based logger
- `HTool/Device/Pro/ProService.cs` — PRO X protocol handler
- `HTool/Device/Pro/ToolService.cs` — Multi-tool state management
- `HTool/Device/Pro/FtpService.cs` — FTP operations

**Legacy**:
- `HToolEx/ProEx/HCommProEx.cs` — ProX controller interface (HToolEx)
- `HToolEx/CommunicationFactory.cs` — Factory pattern (HToolEx)
- `HToolEz/HToolEz.cs` — Main EZTorQ-III entry point
- `HToolEz/Device/DeviceService.cs` — ASCII+binary torque data handling

## Notes

- Nullable enabled, implicit usings enabled across all .NET 8 projects
- HTool.Core, HTool.Format: pure .NET 8.0 (no Windows dependency)
- HTool: Windows Forms dependency (`UseWindowsForms=true`) for serial port
- HToolEx suppresses warning CS0618 (obsolete member usage) via `<NoWarn>`
- Localization via .resx files with `PublicResXFileCodeGenerator` (HToolEx only)
- HToolLegacy, HComm, HCommEz: 신규 기능 추가 금지
- HToolEx: ParaMon 4 지원을 위해 유지보수 중, HTool v2 마이그레이션 후 레거시 전환
