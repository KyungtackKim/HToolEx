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
dotnet build HTool/HTool.csproj -c Release
dotnet build HToolEx/HToolEx.csproj -c Release
dotnet build HToolEz/HToolEz.csproj -c Release

# Create NuGet packages (GeneratePackageOnBuild is enabled)
dotnet pack HTool/HTool.csproj -c Release
dotnet pack HToolEx/HToolEx.csproj -c Release

# Clean and restore
dotnet clean HTool.sln
dotnet restore HTool.sln
```

No unit test infrastructure exists. The `Tester/` directory is currently empty.

## High-Level Architecture

### Solution Structure

```
HTool.sln
├── HTool/              # 한타스 툴 지원 — 가장 범용적이고 대표적인 라이브러리 (v1.1.24)
├── HToolEx/            # PRO X 장비 지원, ParaMon 4에서 사용 중 (v1.1.18)
├── HToolEz/            # 한타스 토크미터 지원 (v0.0.20, x64 only)
├── HComm/              # 구버전 한타스 툴 지원 (.NET Standard 2.0, legacy)
└── HCommEz/            # 구버전 한타스 토크미터 지원 (.NET Standard 2.0, legacy)
```

**Platform**: HTool and HToolEx target AnyCPU; HToolEz targets **win-x64** only.

**주력 개발 대상**: HTool, HToolEz
**유지보수 중**: HToolEx (ParaMon 4에서 사용 중이므로 지원 계속)
**레거시**: HComm, HCommEz (신규 기능 추가 금지)

**Migration 방향**: HToolEx의 기능(ProEx)을 HTool로 점진적으로 통합하여, 최종적으로 HToolEx도 레거시로 전환하는 것이 목표. 신규 공통 기능은 HTool에 추가한다.

### HTool (Core Library)

한타스 툴과의 MODBUS 직접 통신을 위한 핵심 라이브러리. 가장 범용적이며, 신규 기능의 우선 추가 대상.

**Key namespaces**:
- `HTool.Device`: Communication implementations (ITool, HcRtu, HcTcp)
- `HTool.Data`: Data containers for received messages
- `HTool.Format`: Message parsing and data structures (FormatInfo, FormatMessage, FormatStatus, FormatEvent, FormatGraph)
- `HTool.Type`: Enumerations (CodeTypes, ComTypes, ConnectionTypes, ModelTypes)
- `HTool.Util`: Utility classes (KeyedQueue, RingBuffer, BinaryReaderBigEndian, Utils)

**Dependencies**: SuperSimpleTcp (3.0.20), System.IO.Ports (10.0.0)

### HToolEx (Extended Library)

ParaMon-Pro X 게이트웨이를 통한 다중 툴 관리 라이브러리. ParaMon 4에서 사용 중이므로 유지보수 계속하되, 기능은 점진적으로 HTool로 이관.

**Key namespaces**:
- `HToolEx.Device`: Extended device implementations
- `HToolEx.ProEx`: Professional extended features
  - `ProEx.Manager`: ToolManager (multi-tool state), SessionManager, FtpManager
  - `ProEx.Format`: ProX-specific message formats (19+ format classes)
  - `ProEx.FormatJob`: Job/recipe definitions with step types (Fasten, Delay, Input, Output, Message)
- `HToolEx.Localization`: Multi-language support (EN, DE, ES, FR)

**Additional dependencies**: FluentFTP (49.0.1), JetBrains.Annotations (2025.2.4)

### HToolEz (EZTorQ-III Library)

한타스 토크미터(EZTorQ-III) 통신 및 캘리브레이션 전용 라이브러리.

**Key namespaces**:
- `HToolEz.Device`: DeviceService, DeviceHelper, DeviceServiceFactory — handles both ASCII and binary torque data
- `HToolEz.Format`: FormatCalData, FormatCalSetData, FormatMessage, FormatSetData
- `HToolEz.Type`: 12 device-specific enumerations (CalPointModeTypes, DeviceCommandTypes, DirectionTypes, etc.)
- `HToolEz.Util`: Constants, KeyedQueue, RingBuffer, Utils (shared utility copies)

**Dependencies**: System.IO.Ports (9.0.10)

### ModelTypes Enum Divergence

`HTool.Type.ModelTypes` is a **unified** enum covering both HANTAS and Mountz models. `HToolEx.Type` splits this into **two separate enums**: `ModelTypes` (HANTAS only, 10 entries) and `ModelTypesMountz` (Mountz variants, 9 entries including a distinct `Ad = 1002`). Keep these in sync when adding new device models.

## Important Patterns and Conventions

### 1. Factory Pattern for Device Creation

```csharp
// HTool
ITool tool = Tool.Create(ComTypes.Tcp);

// HToolEx
IHComm comm = CommunicationFactory.Create(CommTypes.Tcp);
```

### 2. Event-Driven Architecture

All communication classes use delegate-based events:

```csharp
public event PerformChangedConnect? ChangedConnect;
public event PerformReceivedData? ReceivedData;
public event PerformRawData? ReceivedRawData;
public event PerformRawData? TransmitRawData;
public event ITool.PerformReceiveError? ReceiveError;
```

### 3. Thread-Safe KeyedQueue Pattern

Custom implementation prevents duplicate messages in queue:

```csharp
private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue =
    KeyedQueue<FormatMessage, FormatMessage.MessageKey>
        .Create(static m => m.Key, capacity: 64);

// Enqueue with uniqueness enforcement
MessageQue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// Or allow duplicates
MessageQue.TryEnqueue(msg, EnqueueMode.AllowDuplicate);
```

Features: O(1) enqueue/dequeue, thread-safe Monitor-based locking, blocking/non-blocking operations, timeout support.

### 4. MODBUS Protocol Implementation

**Function Codes** (CodeTypes enum):
- `0x03`: Read Holding Registers
- `0x04`: Read Input Registers
- `0x06`: Write Single Register
- `0x10`: Write Multiple Registers
- `0x11`: Read Device Information (custom)
- `0x64`: Graph Data (custom)
- `0x65`: Graph Result (custom)
- `0x66`: High Resolution Graph (custom)

**Message Lifecycle**:
1. Create FormatMessage with code, address, and packet
2. Enqueue with duplicate checking
3. Timer-based processing transmits when activated (10ms period)
4. Response matched to request by code/address
5. Retry logic with timeout (default: 1000ms)

**Max register sizes**: Read 125, Write 123 per request. Large operations are automatically split.

### 5. Connection State Management

**States**: Close/Closed → Connecting → Connected

**Connection Flow**: `Connect()` → Connecting → `ReadInfoReg()` → Parse Device Info → Connected

**Keep-Alive**: When `EnableKeepAlive = true`, automatic ReadInfoReg() every 3 seconds when idle, timeout after 10 seconds.

### 6. Format Classes Pattern

All Format* classes follow a consistent pattern:

```csharp
public class FormatInfo {
    public static int Size => 13;

    public FormatInfo(byte[] values) {
        using var stream = new MemoryStream(values);
        using var bin = new BinaryReaderBigEndian(stream);
        // Parse fields as Big-Endian (MODBUS network byte order)
    }

    // Read-only properties for parsed data
}
```

### 7. ProEx Multi-Tool Architecture

**Session-Based Communication**:
- `SessionManager`: TCP session to ParaMon-Pro X gateway
- `ToolManager`: Tracks member tools (connected) and scan tools (discovered)
- `FtpManager`: File operations for logs/configurations

**Tool Selection**:
```csharp
ToolManager.SelectedTool = toolId;
proEx.ReadHoldingReg(addr, count); // Targets selected tool
```

### 8. Binary Protocol Utilities

- `BinaryReaderBigEndian`: Wraps BinaryReader for MODBUS Big-Endian byte order
- `Utils.ConvertValue()`: Endianness conversion with `isBigEndian` parameter (default: true)
- `RingBuffer`: Streaming data parsing buffer (typically 16KB)

### 9. Coding Conventions

**Naming**:
- `Hc*` prefix: Hantas Communication classes (HcRtu, HcTcp)
- `Format*`: Data parsing/structure classes
- `*Types`: Enumeration classes
- `Perform*`: Delegate naming pattern

**Threading**: Timer-based processing (System.Timers.Timer), ConcurrentQueue and custom KeyedQueue for thread safety, Monitor-based locking in KeyedQueue.

**Memory**: ArrayPool<byte> for receive buffers, ReadOnlyMemory<byte> and ReadOnlySpan<byte> for zero-copy operations, IDisposable pattern for cleanup.

## Development Workflow

### Adding New Features

1. **New Device Type**: Implement `ITool` or `IHComm` interface, register in `Tool.Create()` factory
2. **New Message Format**: Create `Format*` class with BinaryReaderBigEndian parsing
3. **New Code Type**: Add to `CodeTypes` enum and update `Tool.IsKnownCode()`
4. **New Device Model**: Update ModelTypes in **both** HTool and HToolEx (see divergence note above)
5. **ProEx Extension**: Add to `ProEx/Format/` or `ProEx/FormatJob/`

### Debugging

```csharp
// Raw packet monitoring
htool.ReceivedRawData += (data) => Console.WriteLine($"RX: {BitConverter.ToString(data)}");
htool.TransmitRawData += (data) => Console.WriteLine($"TX: {BitConverter.ToString(data)}");

// Connection diagnostics
htool.ReceiveError += (reason, param) => Console.WriteLine($"Error: {reason}");
```

## Key Files Reference

- `HTool/HTool.cs` - Main library entry point
- `HTool/Device/Tool.cs` - Abstract factory (`Tool.Create()`) and `IsKnownCode()` validator
- `HTool/Device/ITool.cs` - Core device interface
- `HTool/Util/KeyedQueue.cs` - Custom thread-safe queue implementation
- `HTool/Util/Utils.cs` - Utility methods including endianness conversion
- `HToolEx/ProEx/HCommProEx.cs` - ProX controller interface
- `HToolEx/CommunicationFactory.cs` - Factory pattern implementation
- `HToolEz/HToolEz.cs` - Main EZTorQ-III entry point
- `HToolEz/Device/DeviceService.cs` - ASCII+binary torque data handling

## Notes

- Nullable enabled, implicit usings enabled across all .NET 8 projects
- Windows Forms dependency (`UseWindowsForms=true`) in all three main projects
- Serial port and TCP implementations are Windows-specific
- HToolEx suppresses warning CS0618 (obsolete member usage) via `<NoWarn>`
- Localization via .resx files with `PublicResXFileCodeGenerator`
- HComm, HCommEz: .NET Standard 2.0 레거시 — 신규 기능 추가 금지
- HToolEx: ParaMon 4 지원을 위해 유지보수 중이나, 신규 공통 기능은 HTool에 추가
