# HTool

A comprehensive C# MODBUS communication library suite for HANTAS industrial torque tools and controllers.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Library Details](#library-details)
  - [HTool Core Library](#htool-core-library)
  - [HToolEx Extended Library](#htoolex-extended-library)
  - [HToolEz EZTorQ-III Library](#htoolez-eztorq-iii-library)
- [Usage Guide](#usage-guide)
- [API Reference](#api-reference)
- [Development Environment](#development-environment)
- [License](#license)

## Overview

**HTool** is a C# library suite for controlling and monitoring HANTAS industrial torque tools and controllers. It communicates with devices via MODBUS RTU/TCP protocols and provides features ranging from basic communication to multi-tool management, job programming, and FTP file transfers.

### Core Capabilities

- **MODBUS RTU/TCP Support**: Communication over serial ports and TCP/IP networks
- **Event-Driven Architecture**: Asynchronous data reception and state change handling
- **Automatic Message Queuing**: Thread-safe message queue with duplicate prevention and retry logic
- **Register Auto-Splitting**: Automatic chunking of large read/write operations
- **Keep-Alive Mechanism**: Automatic connection state monitoring
- **ProEx Gateway Support**: Multi-tool management via ParaMon-Pro X
- **Multi-Language Support**: English, German, Spanish, French

## Key Features

### HTool (Core Library)

- Direct MODBUS RTU/TCP communication
- Register read/write operations (Holding, Input)
- Device information retrieval
- Torque graph data collection
- Status and event monitoring
- Custom MODBUS function code support

### HToolEx (Extended Library)

- All HTool features included
- ParaMon-Pro X gateway support
- Multi-tool session management
- Job/Recipe programming (Fasten, Delay, I/O, Message steps)
- FTP file management (logs, configurations)
- Localized resources

### HToolEz (EZTorQ-III Library)

- EZTorQ-III torque meter specific features
- Calibration data processing
- Device-specific protocol handling

## Project Structure

```
HTool.sln
├── HTool/              # Core MODBUS communication library (v1.1.21)
│   ├── Device/         # Communication implementations (ITool, HcRtu, HcTcp)
│   ├── Data/           # Received data containers
│   ├── Format/         # Message parsing classes
│   ├── Type/           # Enumeration definitions
│   └── Util/           # Utilities (KeyedQueue, RingBuffer, Utils)
│
├── HToolEx/            # Extended features library (v1.1.18-beta.8)
│   ├── Device/         # Extended device implementations
│   ├── ProEx/          # Professional Extended features
│   │   ├── Manager/    # ToolManager, SessionManager, FtpManager
│   │   ├── Format/     # ProX message formats
│   │   └── FormatJob/  # Job/Recipe definitions
│   └── Localization/   # Multi-language resources
│
├── HToolEz/            # EZTorQ-III dedicated library (v0.0.14)
│   ├── Device/         # EZTorQ device helpers
│   ├── Format/         # Calibration data formats
│   └── Type/           # EZTorQ type definitions
│
├── HComm/              # Legacy communication library (.NET Standard 2.0)
└── HCommEz/            # Legacy EZTorQ library (.NET Standard 2.0)
```

## Requirements

- **.NET 8.0 SDK** or later
- **Windows 10.0.17763.0** or later
- **Visual Studio 2022 (v17.13+)** or **JetBrains Rider**

### Key Dependencies

**HTool**:
- SuperSimpleTcp (v3.0.17)
- System.IO.Ports (v9.0.9)

**HToolEx**:
- FluentFTP (v49.0.1)
- JetBrains.Annotations (v2025.2.2)
- SuperSimpleTcp, System.IO.Ports

**HToolEz**:
- System.IO.Ports (v9.0.10)

## Installation

### NuGet Packages (Recommended)

```bash
# HTool core library
dotnet add package HTool

# HToolEx extended library
dotnet add package HToolEx

# HToolEz (for EZTorQ-III)
dotnet add package HToolEz
```

### Build from Source

```bash
# Clone repository
git clone https://github.com/KyungtackKim/HToolEx.git
cd HToolEx

# Build solution
dotnet build HTool.sln -c Release

# Create NuGet packages
dotnet pack HTool/HTool.csproj -c Release
dotnet pack HToolEx/HToolEx.csproj -c Release
```

## Quick Start

### Basic MODBUS TCP Connection Example

```csharp
using HTool;
using HTool.Type;

// Create HTool instance (TCP mode)
var htool = new HTool(ComTypes.Tcp);

// Register event handlers
htool.ChangedConnect += (connected) =>
{
    Console.WriteLine(connected ? "Connected!" : "Disconnected!");
};

htool.ReceivedData += (code, addr, data) =>
{
    Console.WriteLine($"Received: Code={code}, Addr={addr}");
    // Process data
};

// Connect to device (IP: 192.168.1.100, Port: 502, Device ID: 1)
if (htool.Connect("192.168.1.100", 502, id: 0x01))
{
    Console.WriteLine("Connecting...");
}

// Wait for connection
await Task.Delay(1000);

// Read Holding Registers (address: 0, count: 10)
htool.ReadHoldingReg(addr: 0, count: 10);

// Write register (address: 100, value: 1234)
htool.WriteSingleReg(addr: 100, value: 1234);

// Close connection
htool.Close();
```

### MODBUS RTU (Serial) Connection Example

```csharp
using HTool;
using HTool.Type;

var htool = new HTool(ComTypes.Rtu);

// Connect via COM3, 115200 baud
if (htool.Connect("COM3", 115200, id: 0x01))
{
    Console.WriteLine("Connecting to RTU device...");
}

// Rest is identical to TCP
htool.ReadHoldingReg(0, 50);
```

### Enable Keep-Alive

```csharp
var htool = new HTool(ComTypes.Tcp);

// Enable Keep-Alive (auto Info request every 3s, 10s timeout)
htool.EnableKeepAlive = true;

htool.Connect("192.168.1.100", 502, 0x01);
```

## Library Details

### HTool (Core Library)

HTool is the core library for direct MODBUS communication with HANTAS devices.

#### Main Classes

##### `HTool`

The main library class providing all communication functionality.

**Properties**:
- `Type`: Communication type (RTU/TCP)
- `ConnectionState`: Connection state (Close/Connecting/Connected)
- `Info`: Device information (`FormatInfo`)
- `Gen`: Device generation (GenRev1/GenRev1Plus/GenRev2)
- `EnableKeepAlive`: Keep-Alive enabled flag

**Key Methods**:
- `Connect(target, option, id)`: Connect to device
- `Close()`: Close connection
- `ReadHoldingReg(addr, count, split, check)`: Read Holding Registers
- `ReadInputReg(addr, count, split, check)`: Read Input Registers
- `WriteSingleReg(addr, value, check)`: Write single register
- `WriteMultiReg(addr, values, check)`: Write multiple registers
- `WriteStrReg(addr, str, length, check)`: Write string register
- `ReadInfoReg(check)`: Read device information

**Events**:
- `ChangedConnect`: Connection state changed
- `ReceivedData`: Data received
- `ReceiveError`: Error occurred
- `ReceivedRawData`: Raw data received (debugging)
- `TransmitRawData`: Raw data transmitted (debugging)

#### ITool Interface

Low-level interface for MODBUS communication.

```csharp
public interface ITool
{
    byte DeviceId { get; set; }
    GenerationTypes Revision { get; set; }

    bool Connect(string target, int option, byte id);
    void Close();
    bool Write(byte[] packet, int length);

    byte[] GetReadHoldingRegPacket(ushort addr, ushort count);
    byte[] GetReadInputRegPacket(ushort addr, ushort count);
    byte[] SetSingleRegPacket(ushort addr, ushort value);
    byte[] SetMultiRegPacket(ushort addr, ReadOnlySpan<ushort> values);
    // ... other methods
}
```

#### Format Classes

Classes for parsing data received from devices.

- **FormatInfo**: Device information (ID, firmware version, serial number, model)
- **FormatStatus**: Status information (torque, angle, battery, etc.)
- **FormatEvent**: Event data
- **FormatGraph**: Torque/angle graph data
- **FormatMessage**: Internal message queue item

#### Type Enumerations

- **ComTypes**: RTU, TCP
- **CodeTypes**: MODBUS function codes (0x03, 0x04, 0x06, 0x10, 0x11, 0x64, 0x65, 0x66)
- **ConnectionTypes**: Close, Connecting, Connected
- **GenerationTypes**: GenRev1, GenRev1Plus, GenRev1Ad, GenRev2
- **ModelTypes**: Device models (Ad, At, Md, Mt, etc.)
- **ComErrorTypes**: Communication error types

#### Utility Classes

##### KeyedQueue<T, TKey>

Thread-safe queue with duplicate prevention.

```csharp
// Message key-based duplicate prevention
var queue = KeyedQueue<FormatMessage, FormatMessage.MessageKey>
    .Create(m => m.Key, capacity: 64);

// Enqueue with duplicate checking
queue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// Allow duplicates
queue.TryEnqueue(msg, EnqueueMode.AllowDuplicate);
```

**Features**:
- O(1) enqueue/dequeue performance
- Monitor-based thread safety
- Blocking/non-blocking operations
- Timeout support

##### BinaryReaderBigEndian

BinaryReader extension for handling MODBUS Big-Endian byte order.

```csharp
using var stream = new MemoryStream(data);
using var reader = new BinaryReaderBigEndian(stream);

ushort value = reader.ReadUInt16();  // Read as Big-Endian
uint value32 = reader.ReadUInt32();
```

##### Utils

Provides endianness conversion and utility methods.

```csharp
// Big-Endian conversion (default)
Utils.ConvertValue(bytes, out int value, isBigEndian: true);

// Little-Endian conversion
Utils.ConvertValue(bytes, out int value, isBigEndian: false);
```

### HToolEx (Extended Library)

HToolEx includes all HTool features and adds multi-tool management via ParaMon-Pro X gateway.

#### ProEx Architecture

**Session-Based Communication**:
- `SessionManager`: TCP session management with ParaMon-Pro X
- `ToolManager`: Tracks member tools (connected) and scan tools (discovered)
- `FtpManager`: Log and configuration file transfers

**Multi-Tool Control**:

```csharp
using HToolEx;
using HToolEx.ProEx.Manager;

var proEx = new HCommProEx();
var toolManager = new ToolManager();

// Connect to ProX gateway
proEx.Connect("192.168.1.50", 502);

// Select specific tool
toolManager.SelectedTool = toolId;

// Read registers from selected tool
proEx.ReadHoldingReg(addr: 0, count: 10);
```

#### Job/Recipe Programming

ProEx provides Job/Recipe features for programming complex fastening sequences.

**Job Step Types**:
- **Fasten**: Fastening operation (torque, angle control)
- **Delay**: Wait time
- **Input**: Wait for input signal
- **Output**: Control output signal
- **Message**: Display message

```csharp
// Job step definition example
var job = new JobDefinition
{
    Steps = new[]
    {
        new FastenStep { Torque = 10.5, Angle = 90 },
        new DelayStep { Duration = 500 },  // 500ms
        new OutputStep { Port = 1, State = true },
        new MessageStep { Text = "Operation Complete" }
    }
};
```

#### Multi-Language Support

HToolEx supports 4 languages:
- English (EN)
- German (DE)
- Spanish (ES)
- French (FR)

```csharp
using HToolEx.Localization;

// Get resource string
string msg = HToolExRes.ConnectionError;
```

### HToolEz (EZTorQ-III Library)

HToolEz provides EZTorQ-III torque meter specific features.

#### Key Features

- **Calibration Data Processing**: Calibration point and settings management
- **Device Helper**: EZTorQ-III specialized communication helper
- **Data Formats**: Calibration data formats (FormatCalData, FormatCalSetData)

```csharp
using HToolEz;
using HToolEz.Device;

var ezDevice = new DeviceService();
var helper = new DeviceHelper();

// EZTorQ-III connection and control
// (Specific API varies by project version)
```

## Usage Guide

### 1. Large Register Read/Write

HTool automatically handles MODBUS maximum register limits.

```csharp
// Read 500 registers (automatically split into 4 requests of 125 each)
htool.ReadHoldingReg(addr: 0, count: 500);

// Specify custom split size
htool.ReadHoldingReg(addr: 0, count: 500, split: 100);

// Write 200 registers (automatically split by 123)
ushort[] values = new ushort[200];
// ... populate values ...
htool.WriteMultiReg(addr: 1000, values);
```

**Limits**:
- Read: Maximum 125 registers/request
- Write: Maximum 123 registers/request

### 2. Message Duplicate Control

KeyedQueue prevents duplicate messages from being inserted into the queue.

```csharp
// Enable duplicate checking (default)
htool.ReadHoldingReg(addr: 100, count: 10, check: true);

// Allow duplicates (send same request multiple times)
htool.ReadHoldingReg(addr: 100, count: 10, check: false);
```

### 3. Connection State Management

```csharp
htool.ChangedConnect += (connected) =>
{
    if (connected)
    {
        // Connected: Info register successfully read
        Console.WriteLine($"Device: {htool.Info.Model}");
        Console.WriteLine($"Serial: {htool.Info.Serial}");
        Console.WriteLine($"Firmware: {htool.Info.Firmware}");
        Console.WriteLine($"Generation: {htool.Gen}");
    }
    else
    {
        // Disconnected
        Console.WriteLine("Device disconnected");
    }
};

// Check connection state
if (htool.ConnectionState == ConnectionTypes.Connected)
{
    // Request data
}
```

**Connection Flow**:
1. `Connect()` called → `Connecting` state
2. Automatically sends `ReadInfoReg()`
3. Info received successfully → `Connected` state → `ChangedConnect(true)` event
4. Timeout or failure → `Close` state

### 4. Error Handling

```csharp
htool.ReceiveError += (reason, param) =>
{
    switch (reason)
    {
        case ComErrorTypes.Timeout:
            Console.WriteLine("Request timeout");
            break;
        case ComErrorTypes.InvalidCrc:
            Console.WriteLine("CRC error");
            break;
        case ComErrorTypes.InvalidFunction:
            Console.WriteLine($"Invalid function code: {param}");
            break;
        // ... other error handling
    }
};
```

### 5. Raw Packet Monitoring (Debugging)

```csharp
htool.TransmitRawData += (data) =>
{
    Console.WriteLine($"TX: {BitConverter.ToString(data)}");
};

htool.ReceivedRawData += (data) =>
{
    Console.WriteLine($"RX: {BitConverter.ToString(data)}");
};

// Output example:
// TX: 01-03-00-00-00-0A-C5-CD
// RX: 01-03-14-00-01-00-02-00-03-00-04-00-05-00-06-00-07-00-08-00-09-00-0A-XX-XX
```

### 6. Data Parsing

```csharp
htool.ReceivedData += (code, addr, data) =>
{
    switch (code)
    {
        case CodeTypes.ReadHoldingReg:
        case CodeTypes.ReadInputReg:
            // data.Data is byte array of register values
            byte[] regData = data.Data;

            // Parse as Big-Endian
            using var stream = new MemoryStream(regData);
            using var reader = new BinaryReaderBigEndian(stream);

            ushort reg0 = reader.ReadUInt16();
            ushort reg1 = reader.ReadUInt16();
            // ...
            break;

        case CodeTypes.ReadInfoReg:
            // Automatically saved to htool.Info
            Console.WriteLine($"Device ID: {htool.Info.Id}");
            break;

        case CodeTypes.Graph:
            var graph = new FormatGraph(data.Data);
            // Process graph data
            break;
    }
};
```

### 7. String Register Read/Write

```csharp
// Write string (ASCII, 2 bytes/register)
htool.WriteStrReg(addr: 200, str: "HANTAS", length: 10);

// Read string
htool.ReceivedData += (code, addr, data) =>
{
    if (code == CodeTypes.ReadHoldingReg && addr == 200)
    {
        // Convert 2 bytes per ASCII character
        string text = Encoding.ASCII.GetString(data.Data).TrimEnd('\0');
        Console.WriteLine($"String: {text}");
    }
};

htool.ReadHoldingReg(addr: 200, count: 10);
```

## API Reference

### MODBUS Function Codes (CodeTypes)

| Code | Value | Description |
|------|-------|-------------|
| ReadHoldingReg | 0x03 | Read Holding Registers |
| ReadInputReg | 0x04 | Read Input Registers |
| WriteSingleReg | 0x06 | Write Single Register |
| WriteMultiReg | 0x10 | Write Multiple Registers |
| ReadInfoReg | 0x11 | Read Device Information (custom) |
| Graph | 0x64 | Graph Data (custom) |
| GraphRes | 0x65 | Graph Result (custom) |
| HighResGraph | 0x66 | High Resolution Graph (custom) |
| Error | 0x80 | Error Response |

### Error Codes (ComErrorTypes)

| Error | Value | Description |
|-------|-------|-------------|
| InvalidFunction | 0x01 | Invalid function code |
| InvalidAddress | 0x02 | Invalid address |
| InvalidValue | 0x03 | Invalid value |
| InvalidCrc | 0x07 | CRC checksum error |
| InvalidFrame | 0x0C | Frame error |
| InvalidValueRange | 0x0E | Value range error |
| Timeout | 0x0F | Timeout |

### Constants

```csharp
// HTool internal constants
ProcessPeriod = 10ms        // Message processing period
MessageTimeout = 1000ms     // Message response timeout
ConnectTimeout = 10s        // Connection timeout
KeepAlivePeriod = 3000ms    // Keep-Alive request period
KeepAliveTimeout = 10s      // Keep-Alive timeout

ReadRegMaxSize = 125        // Maximum read register count
WriteRegMaxSize = 123       // Maximum write register count
```

## Development Environment

### Required Tools

- .NET 8.0 SDK
- Visual Studio 2022 (17.13+) or JetBrains Rider
- Git

### Build Commands

```bash
# Build entire solution
dotnet build HTool.sln

# Build specific project in Release mode
dotnet build HTool/HTool.csproj -c Release
dotnet build HToolEx/HToolEx.csproj -c Release
dotnet build HToolEz/HToolEz.csproj -c Release

# Clean build artifacts
dotnet clean HTool.sln

# Restore NuGet packages
dotnet restore HTool.sln

# Create NuGet packages
dotnet pack HTool/HTool.csproj -c Release
dotnet pack HToolEx/HToolEx.csproj -c Release
```

### Project Settings

- **Target Framework**: net8.0-windows10.0.17763.0
- **Language**: C# 12
- **Nullable**: enabled
- **Implicit Usings**: enabled
- **Platform**: AnyCPU (HTool, HToolEx) / x64 (HToolEz)

### Coding Conventions

**Naming**:
- `Hc*`: HANTAS Communication classes (e.g., HcRtu, HcTcp)
- `Format*`: Data parsing/structure classes
- `*Types`: Enumeration classes
- `Perform*`: Delegate naming pattern

**Threading**:
- Timer-based processing (System.Timers.Timer)
- ConcurrentQueue and KeyedQueue usage
- Monitor-based locking

**Memory Management**:
- ArrayPool<byte> usage (receive buffers)
- ReadOnlyMemory<byte>, ReadOnlySpan<byte> (zero-copy)
- IDisposable pattern

## Advanced Topics

### Adding Custom Device Types

1. Implement `ITool` interface
2. Register in `Tool.Create()` factory method
3. Add new type to `ComTypes` enumeration

```csharp
public class HcCustom : ITool
{
    // Implement ITool interface
}

// In Tool.cs
public static ITool? Create(ComTypes type)
{
    return type switch
    {
        ComTypes.Rtu => new HcRtu(),
        ComTypes.Tcp => new HcTcp(),
        ComTypes.Custom => new HcCustom(),  // Add
        _ => null
    };
}
```

### Adding New Message Formats

```csharp
public sealed class FormatCustom
{
    public static int Size => 20;

    public FormatCustom(byte[] values)
    {
        if (values.Length < Size)
            return;

        using var stream = new MemoryStream(values);
        using var reader = new BinaryReaderBigEndian(stream);

        // Parse fields as Big-Endian
        Field1 = reader.ReadUInt16();
        Field2 = reader.ReadUInt32();
        // ...
    }

    public ushort Field1 { get; }
    public uint Field2 { get; }
}
```

### ProEx Extensions

Add new features to ProEx namespace:
- `ProEx/Format/`: New message formats
- `ProEx/FormatJob/`: New job step types
- `ProEx/Manager/`: New manager classes

## Troubleshooting

### Connection Failure

```csharp
// Check timeout
if (htool.ConnectionState == ConnectionTypes.Connecting)
{
    // Still Connecting after 10 seconds = timeout
}

// Network verification
// - Verify device IP address
// - Check firewall settings
// - Verify port number (default 502)

// RTU: Check COM port and baud rate
// - Verify baud rate from device manual (e.g., 115200)
// - Check COM port number in Windows Device Manager
```

### CRC Errors

```csharp
htool.ReceiveError += (reason, param) =>
{
    if (reason == ComErrorTypes.InvalidCrc)
    {
        // Can occur in RTU mode
        // - Baud rate mismatch
        // - Cable quality issues
        // - Electrical noise
    }
};
```

### No Data Received

```csharp
// Check connection state
if (htool.ConnectionState != ConnectionTypes.Connected)
{
    Console.WriteLine("Device not connected");
    return;
}

// Verify correct address range
// Refer to device manual for register map

// Monitor raw packets
htool.TransmitRawData += data => Console.WriteLine($"TX: {BitConverter.ToString(data)}");
htool.ReceivedRawData += data => Console.WriteLine($"RX: {BitConverter.ToString(data)}");
```

## Performance Considerations

### Message Queue Optimization

```csharp
// KeyedQueue has capacity limit of 64
// Consider disabling duplicate checking for many requests
htool.ReadHoldingReg(addr, count, check: false);
```

### Large Data Transfers

```csharp
// Optimize split size for large data reads
// Adjust based on network conditions
htool.ReadHoldingReg(addr: 0, count: 1000, split: 100);

// Too small split = increased overhead
// Too large split = timeout risk
```

### Keep-Alive Settings

```csharp
// Keep-Alive incurs network overhead
// Enable only when needed
htool.EnableKeepAlive = true;  // Only for long idle periods

// Disable for short operations
htool.EnableKeepAlive = false;
```

## Example Projects

### 1. Basic Torque Monitoring

```csharp
using HTool;
using HTool.Type;
using HTool.Format;

var htool = new HTool(ComTypes.Tcp);

htool.ChangedConnect += connected =>
{
    if (connected)
    {
        Console.WriteLine("Monitoring started...");
        // Periodically read status registers
        ReadStatusPeriodically();
    }
};

htool.ReceivedData += (code, addr, data) =>
{
    if (code == CodeTypes.ReadHoldingReg)
    {
        var status = new FormatStatus(data.Data);
        Console.WriteLine($"Torque: {status.Torque}, Angle: {status.Angle}");
    }
};

htool.Connect("192.168.1.100", 502, 0x01);

async void ReadStatusPeriodically()
{
    while (htool.ConnectionState == ConnectionTypes.Connected)
    {
        htool.ReadHoldingReg(addr: 0, count: 50);
        await Task.Delay(100);  // Read every 100ms
    }
}
```

### 2. Multi-Tool Management (ProEx)

```csharp
using HToolEx;
using HToolEx.ProEx.Manager;

var proEx = new HCommProEx();
var toolMgr = new ToolManager();

proEx.Connect("192.168.1.50", 502);

// Get tool list
var memberTools = toolMgr.MemberTools;
foreach (var tool in memberTools)
{
    Console.WriteLine($"Tool ID: {tool.Id}, Name: {tool.Name}");

    // Select and control each tool
    toolMgr.SelectedTool = tool.Id;
    proEx.ReadHoldingReg(0, 10);
}
```

### 3. Batch Register Configuration

```csharp
// Configure multiple registers at once
var settings = new ushort[]
{
    1000,  // Reg 100: Torque setpoint
    90,    // Reg 101: Angle setpoint
    5,     // Reg 102: Speed
    1      // Reg 103: Mode
};

htool.WriteMultiReg(addr: 100, settings);

// Verify
htool.ReceivedData += (code, addr, data) =>
{
    if (code == CodeTypes.WriteMultiReg && addr == 100)
    {
        Console.WriteLine("Settings applied!");
    }
};
```

## License

This project is distributed under the **MIT License**.

```
MIT License

Copyright (c) HANTAS

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Contributing

Contributions to improve the project are welcome!

### How to Contribute

1. Fork this repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Style

- Follow C# coding conventions
- Write XML documentation comments
- Add unit tests (future)

## Support and Contact

- **Company**: HANTAS
- **Author**: Eloiz
- **GitHub**: [https://github.com/KyungtackKim/HToolEx](https://github.com/KyungtackKim/HToolEx)
- **Issue Reports**: Use GitHub Issues

## Version History

### HTool
- **v1.1.21** (current): Latest stable version

### HToolEx
- **v1.1.18-beta.8** (current): Includes ProEx features, beta version

### HToolEz
- **v0.0.14** (current): Initial EZTorQ-III version

See release notes for each project for detailed change history.

---

**HTool** - Powerful and flexible C# communication library for HANTAS torque tools
