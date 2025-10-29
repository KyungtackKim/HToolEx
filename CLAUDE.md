# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**HToolEx** is a C# library suite for MODBUS communication with Hantas industrial torque tools and controllers. It provides both basic device communication (HTool) and extended features for enterprise tool management systems (HToolEx with ProEx support).

**Company**: HANTAS
**Target Framework**: .NET 8.0 Windows 10.0.17763.0+
**Primary Protocol**: MODBUS RTU/TCP

## Build and Development Commands

### Build Commands

```bash
# Build entire solution
dotnet build HTool.sln

# Build specific project in Release mode
dotnet build HTool/HTool.csproj -c Release
dotnet build HToolEx/HToolEx.csproj -c Release

# Create NuGet packages (GeneratePackageOnBuild is enabled)
dotnet pack HTool/HTool.csproj -c Release
dotnet pack HToolEx/HToolEx.csproj -c Release
```

### Clean and Restore

```bash
# Clean build artifacts
dotnet clean HTool.sln

# Restore NuGet packages
dotnet restore HTool.sln
```

**Development Environment**: Visual Studio 2022 (v17.13+) or JetBrains Rider with .NET 8.0 SDK

## High-Level Architecture

### Solution Structure

```
HTool.sln
├── HTool/              # Core MODBUS communication library (v1.1.17)
├── HToolEx/            # Extended features with ProEx support (v1.1.18-beta.8)
├── HComm/              # Legacy communication library (.NET Standard 2.0)
├── HCommEz/            # EZTorQ torque meter library (.NET Standard 2.0)
└── HToolEz/            # New EZTorQ-III functionality (v0.0.1)
```

### HTool (Core Library)

Foundation library for direct MODBUS communication with Hantas torque tools.

**Key namespaces**:
- `HTool.Device`: Communication implementations (ITool, HcRtu, HcTcp)
- `HTool.Data`: Data containers for received messages
- `HTool.Format`: Message parsing and data structures (FormatInfo, FormatMessage, FormatStatus, FormatEvent, FormatGraph)
- `HTool.Type`: Enumerations (CodeTypes, ComTypes, ConnectionTypes, ModelTypes)
- `HTool.Util`: Utility classes (KeyedQueue, RingBuffer, BinaryReaderBigEndian, Utils)

**Dependencies**: SuperSimpleTcp (v3.0.17), System.IO.Ports (v9.0.9)

### HToolEx (Extended Library)

Enhanced library with multi-tool management via ParaMon-Pro X gateway.

**Key namespaces**:
- `HToolEx.Device`: Extended device implementations
- `HToolEx.ProEx`: Professional extended features
  - `ProEx.Manager`: ToolManager (multi-tool state), SessionManager, FtpManager
  - `ProEx.Format`: ProX-specific message formats
  - `ProEx.FormatJob`: Job/recipe definitions with step types (Fasten, Delay, Input, Output, Message)
- `HToolEx.Localization`: Multi-language support (EN, DE, ES, FR)

**Additional dependencies**: FluentFTP (v49.0.1), JetBrains.Annotations

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
// Create queue with key selector (prevents duplicates by key)
private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue =
    KeyedQueue<FormatMessage, FormatMessage.MessageKey>
        .Create(static m => m.Key, capacity: 64);

// Enqueue with uniqueness enforcement
MessageQue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// Or allow duplicates
MessageQue.TryEnqueue(msg, EnqueueMode.AllowDuplicate);
```

**Features**: O(1) enqueue/dequeue, thread-safe Monitor-based locking, blocking/non-blocking operations, timeout support.

### 4. MODBUS Protocol Implementation

**Function Codes** (CodeTypes enum):
- `0x03`: Read Holding Registers
- `0x04`: Read Input Registers
- `0x06`: Write Single Register
- `0x10`: Write Multiple Registers
- `0x11`: Read Device Information (custom)
- `0x64`: Graph Data (custom)
- `0x65`: Graph Result (custom)

**Message Lifecycle**:
1. Create FormatMessage with code, address, and packet
2. Enqueue with duplicate checking
3. Timer-based processing transmits when activated
4. Response matched to request by code/address
5. Retry logic with timeout (default: 1000ms)

### 5. Connection State Management

**States**: Close/Closed → Connecting → Connected

**Connection Flow**: `Connect()` → Connecting → `ReadInfoReg()` → Parse Device Info → Connected

**Keep-Alive**: When `EnableKeepAlive = true`, automatic ReadInfoReg() every 3 seconds when idle, timeout after 10 seconds.

### 6. Register Read/Write Patterns

Automatic splitting for large operations:

```csharp
// Reading 500 registers automatically splits into chunks of 125
htool.ReadHoldingReg(addr: 0, count: 500, split: 125);
```

**Max Sizes**: Read 125 registers, Write 123 registers per request.

### 7. Format Classes Pattern

All Format* classes follow a consistent pattern:

```csharp
public class FormatInfo {
    public static int Size => 13;

    public FormatInfo(byte[] values) {
        using var stream = new MemoryStream(values);
        using var bin = new BinaryReaderBigEndian(stream);
        // Parse fields...
    }

    // Read-only properties for parsed data
}
```

### 8. ProEx Multi-Tool Architecture

**Session-Based Communication**:
- `SessionManager`: TCP session to ParaMon-Pro X gateway
- `ToolManager`: Tracks member tools (connected) and scan tools (discovered)
- `FtpManager`: File operations for logs/configurations

**Tool Selection**:
```csharp
ToolManager.SelectedTool = toolId;
proEx.ReadHoldingReg(addr, count); // Targets selected tool
```

### 9. Binary Protocol Utilities

**Big-Endian Reading** (MODBUS network byte order):
```csharp
using var bin = new BinaryReaderBigEndian(stream);
ushort value = bin.ReadUInt16();
```

**Endianness Conversion** (Utils.cs):
```csharp
// Convert with endianness control
Utils.ConvertValue(bytes, out int value, isBigEndian: true);  // Default big-endian
Utils.ConvertValue(bytes, out int value, isBigEndian: false); // Little-endian
```

**Ring Buffer** for streaming data parsing:
```csharp
private RingBuffer AnalyzeBuf = new(16 * 1024);
```

### 10. Coding Conventions

**Naming**:
- `Hc*` prefix: Hantas Communication classes (HcRtu, HcTcp)
- `Format*`: Data parsing/structure classes
- `*Types`: Enumeration classes
- `Perform*`: Delegate naming pattern

**Threading**:
- Timer-based processing (System.Timers.Timer)
- ConcurrentQueue and custom KeyedQueue for thread safety
- Monitor-based locking in KeyedQueue

**Memory**:
- ArrayPool<byte> for receive buffers
- ReadOnlyMemory<byte> and ReadOnlySpan<byte> for zero-copy operations
- IDisposable pattern for cleanup

## Development Workflow

### Adding New Features

1. **New Device Type**: Implement `ITool` or `IHComm` interface
2. **New Message Format**: Create `Format*` class with BinaryReaderBigEndian parsing
3. **New Code Type**: Add to `CodeTypes` enum and update `Tool.IsKnownCode()`
4. **ProEx Extension**: Add to `ProEx/Format/` or `ProEx/FormatJob/`

### Debugging

**Raw Packet Monitoring**:
```csharp
htool.ReceivedRawData += (data) => Console.WriteLine($"RX: {BitConverter.ToString(data)}");
htool.TransmitRawData += (data) => Console.WriteLine($"TX: {BitConverter.ToString(data)}");
```

**Connection Diagnostics**:
```csharp
htool.ReceiveError += (reason, param) => Console.WriteLine($"Error: {reason}");
```

## Key Files Reference

- `HTool/HTool.cs` - Main library entry point
- `HTool/Device/ITool.cs` - Core device interface
- `HTool/Util/KeyedQueue.cs` - Custom thread-safe queue implementation
- `HTool/Util/Utils.cs` - Utility methods including endianness conversion
- `HToolEx/ProEx/HCommProEx.cs` - ProX controller interface
- `HToolEx/CommunicationFactory.cs` - Factory pattern implementation

## Notes

- Projects use implicit usings (C# 10+)
- Windows Forms dependency for some GUI-related functionality
- Serial port and TCP implementations are Windows-specific
- No current unit test infrastructure
- Localization via .resx files (EN, DE, ES, FR)
