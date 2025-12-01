# HTool

A comprehensive C# library for MODBUS communication with HANTAS industrial torque tools and controllers.

[![NuGet](https://img.shields.io/badge/nuget-v1.1.21-blue)](https://www.nuget.org/packages/HTool)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://www.microsoft.com/windows)

> [한국어 문서](README-KOR.md) | **English**

---

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [Architecture](#architecture)
- [Data Formats](#data-formats)
- [Utility Classes](#utility-classes)
- [MODBUS Protocol](#modbus-protocol)
- [Error Handling](#error-handling)
- [Configuration Constants](#configuration-constants)
- [Troubleshooting](#troubleshooting)
- [Performance Optimization](#performance-optimization)
- [Version History](#version-history)
- [License](#license)
- [Support](#support)

---

## Overview

**HTool** is a comprehensive C# library providing MODBUS RTU/TCP communication capabilities for HANTAS torque tools and
controllers. It features an event-driven architecture with automatic message queuing, register auto-splitting, and
thread-safe operations, making it easy to integrate HANTAS devices into your .NET applications.

---

## Key Features

### Communication

- **Dual Protocol Support**: MODBUS RTU (serial) and MODBUS TCP/IP
- **Automatic Connection Management**: Connection state tracking and auto-reconnect support
- **Keep-Alive Mechanism**: Automatic connection monitoring (3-second interval)

### Message Processing

- **Thread-Safe Message Queue**: Duplicate prevention and retry logic via `KeyedQueue`
- **Smart Register Operations**: Automatic chunking for large read/write operations
    - Max read: 125 registers/request
    - Max write: 123 registers/request

### Data Parsing

- **Automatic Device Info Parsing**: Model, serial, firmware, generation info
- **Status Data**: Real-time torque, speed, current, alarm, etc.
- **Event Data**: Fastening results, barcode, graph steps
- **Graph Data**: Torque/angle graph collection

### Debugging

- **Raw Packet Monitoring**: Real-time TX/RX packet inspection
- **Error Events**: Detailed error information

---

## System Requirements

| Item                  | Requirement                   |
|-----------------------|-------------------------------|
| **.NET**              | 8.0 or later                  |
| **OS**                | Windows 10.0.17763.0 or later |
| **RTU Communication** | Available COM port            |
| **TCP Communication** | Network access to device      |

### Dependencies

| Package                                                           | Version | Purpose                                   |
|-------------------------------------------------------------------|---------|-------------------------------------------|
| [SuperSimpleTcp](https://www.nuget.org/packages/SuperSimpleTcp)   | 3.0.20  | TCP socket communication                  |
| [System.IO.Ports](https://www.nuget.org/packages/System.IO.Ports) | 10.0.0  | Serial port communication (RS-232/RS-485) |

---

## Installation

### NuGet (Recommended)

```bash
dotnet add package HTool
```

Or via Package Manager Console:

```powershell
Install-Package HTool
```

### Build from Source

```bash
git clone https://github.com/KyungtackKim/HToolEx.git
cd HToolEx/HTool
dotnet build -c Release
```

### Create NuGet Package

```bash
dotnet pack HTool.csproj -c Release
# Output: bin/Release/HTool.1.1.21.nupkg
```

---

## Quick Start

### MODBUS TCP Example

```csharp
using HTool;
using HTool.Type;
using HTool.Format;

// Create HTool instance
var htool = new HTool.HTool(ComTypes.Tcp);

// Subscribe to connection events
htool.ChangedConnect += (connected) => {
    if (connected) {
        Console.WriteLine($"Connected: {htool.Info.Model}");
        Console.WriteLine($"Serial: {htool.Info.Serial}");
        Console.WriteLine($"Firmware: {htool.Info.Firmware}");
        Console.WriteLine($"Generation: {htool.Gen}");

        // Read registers after connection
        htool.ReadHoldingReg(addr: 0, count: 10);
    } else {
        Console.WriteLine("Disconnected");
    }
};

// Subscribe to data events
htool.ReceivedData += (code, addr, data) => {
    Console.WriteLine($"Received: Code={code}, Address={addr}, Length={data.Length}");
};

// Subscribe to error events
htool.ReceiveError += (reason, param) => {
    Console.WriteLine($"Error: {reason}");
};

// Connect to device (IP address, port, device ID)
if (htool.Connect("192.168.1.100", 502, id: 0x01)) {
    Console.WriteLine("Connecting...");
}

// Close when done
htool.Close();
```

### MODBUS RTU (Serial) Example

```csharp
using HTool;
using HTool.Type;

var htool = new HTool.HTool(ComTypes.Rtu);

htool.ChangedConnect += (connected) => {
    if (connected) {
        Console.WriteLine($"Connected: {htool.Info.Serial}");
    }
};

// Connect via COM port (port name, baud rate, device ID)
htool.Connect("COM3", 115200, id: 0x01);
```

### Supported Baud Rates

```csharp
// Available via HcRtu.GetBaudRates()
int[] baudRates = [9600, 19200, 38400, 57600, 115200, 230400];
```

---

## API Reference

### HTool Class

Main library class providing all communication functionality.

#### Constructors

```csharp
// Default constructor
public HTool()

// Constructor with communication type
public HTool(ComTypes type)
```

#### Properties

| Property          | Type              | Description                           | Default        |
|-------------------|-------------------|---------------------------------------|----------------|
| `Type`            | `ComTypes`        | Communication type (RTU/TCP)          | -              |
| `ConnectionState` | `ConnectionTypes` | Current connection state              | `Close`        |
| `Info`            | `FormatInfo`      | Device information (after connection) | Empty instance |
| `Gen`             | `GenerationTypes` | Device generation/revision            | `GenRev2`      |
| `EnableKeepAlive` | `bool`            | Enable keep-alive                     | `false`        |

#### Static Properties

| Property          | Type  | Value | Description             |
|-------------------|-------|-------|-------------------------|
| `ReadRegMaxSize`  | `int` | 125   | Max registers per read  |
| `WriteRegMaxSize` | `int` | 123   | Max registers per write |

#### Methods

**Communication Type Setup**

```csharp
/// <summary>
/// Set communication type (must be called before connection)
/// </summary>
/// <param name="type">RTU or TCP</param>
public void SetType(ComTypes type)
```

**Connection Management**

```csharp
/// <summary>
/// Connect to device
/// </summary>
/// <param name="target">TCP: IP address, RTU: COM port name</param>
/// <param name="option">TCP: port number, RTU: baud rate</param>
/// <param name="id">MODBUS device ID (1-15)</param>
/// <returns>True if connection initiated successfully</returns>
public bool Connect(string target, int option, byte id = 0x01)

/// <summary>
/// Close connection
/// </summary>
public void Close()
```

**Register Read**

```csharp
/// <summary>
/// Read Holding registers (Function Code 0x03)
/// </summary>
/// <param name="addr">Starting address</param>
/// <param name="count">Number of registers</param>
/// <param name="split">Split size (0=auto, max 125)</param>
/// <param name="check">Enable duplicate checking</param>
public bool ReadHoldingReg(ushort addr, ushort count, int split = 0, bool check = true)

/// <summary>
/// Read Input registers (Function Code 0x04)
/// </summary>
public bool ReadInputReg(ushort addr, ushort count, int split = 0, bool check = true)

/// <summary>
/// Read device information (Function Code 0x11, HANTAS specific)
/// </summary>
public bool ReadInfoReg(bool check = true)
```

**Register Write**

```csharp
/// <summary>
/// Write single register (Function Code 0x06)
/// </summary>
public bool WriteSingleReg(ushort addr, ushort value, bool check = true)

/// <summary>
/// Write multiple registers (Function Code 0x10)
/// </summary>
public bool WriteMultiReg(ushort addr, ushort[] values, bool check = true)
public bool WriteMultiReg(ushort addr, ReadOnlySpan<ushort> values, bool check = true)

/// <summary>
/// Write string to registers (ASCII, 2 bytes per register)
/// </summary>
/// <param name="length">Total length including padding (0=use string length)</param>
public bool WriteStrReg(ushort addr, string str, int length = 0, bool check = true)
```

#### Events

```csharp
/// <summary>
/// Connection state changed event
/// </summary>
/// <param name="state">true: connected, false: disconnected</param>
public event PerformChangedConnect? ChangedConnect;

/// <summary>
/// Data received event
/// </summary>
/// <param name="codeTypes">MODBUS Function Code</param>
/// <param name="addr">Register address</param>
/// <param name="data">Received data</param>
public event PerformReceivedData? ReceivedData;

/// <summary>
/// Error occurred event
/// </summary>
/// <param name="reason">Error type</param>
/// <param name="param">Additional error info</param>
public event ITool.PerformReceiveError? ReceiveError;

/// <summary>
/// Raw data received event (for debugging)
/// </summary>
public event PerformRawData? ReceivedRawData;

/// <summary>
/// Raw data transmitted event (for debugging)
/// </summary>
public event PerformRawData? TransmitRawData;
```

---

## Usage Examples

### 1. Large Register Operations

HTool automatically splits large operations:

```csharp
// Read 500 registers - auto-split into 4 requests (125 each)
htool.ReadHoldingReg(addr: 0, count: 500);

// Custom split size
htool.ReadHoldingReg(addr: 0, count: 500, split: 100);

// Write 200 registers - auto-split by 123
ushort[] data = new ushort[200];
// ... populate data ...
htool.WriteMultiReg(addr: 1000, data);
```

### 2. Connection Flow and State Handling

```csharp
htool.ChangedConnect += (connected) => {
    if (connected) {
        // Connection established and device info received
        var info = htool.Info;

        Console.WriteLine($"Device Model: {info.Model}");
        Console.WriteLine($"Serial Number: {info.Serial}");
        Console.WriteLine($"Firmware Version: {info.Firmware}");
        Console.WriteLine($"Controller: {info.Controller}");
        Console.WriteLine($"Driver: {info.Driver}");
        Console.WriteLine($"Used Count: {info.Used}");
        Console.WriteLine($"Generation: {htool.Gen}");

        // Start operations here
    }
};
```

**Connection Flow:**

1. `Connect()` called → `ConnectionState = Connecting`
2. Library automatically sends `ReadInfoReg()` request
3. Device info received → `ConnectionState = Connected` → `ChangedConnect(true)` fired
4. Timeout (5s) or error → `ConnectionState = Close`

### 3. Data Parsing

```csharp
using HTool.Format;
using HTool.Util;

htool.ReceivedData += (code, addr, data) => {
    switch (code) {
        case CodeTypes.ReadHoldingReg:
        case CodeTypes.ReadInputReg:
            // Parse Big-Endian data
            var span = data.Data.AsSpan();
            var pos = 0;

            ushort reg0 = BinarySpanReader.ReadUInt16(span, ref pos);
            ushort reg1 = BinarySpanReader.ReadUInt16(span, ref pos);
            int reg2_3 = BinarySpanReader.ReadInt32(span, ref pos);  // 2 registers combined
            float reg4_5 = BinarySpanReader.ReadSingle(span, ref pos);  // float value
            break;

        case CodeTypes.ReadInfoReg:
            // Device info auto-parsed into htool.Info
            Console.WriteLine($"Serial: {htool.Info.Serial}");
            break;

        case CodeTypes.Graph:
        case CodeTypes.GraphRes:
            // Parse graph data
            var graph = new FormatGraph(data.Data, htool.Gen);
            Console.WriteLine($"Channel: {graph.Channel}, Points: {graph.Count}");
            foreach (var value in graph.Values) {
                Console.WriteLine($"  {value:F3}");
            }
            break;
    }
};
```

### 4. Status Data Monitoring

```csharp
htool.ReceivedData += (code, addr, data) => {
    if (code == CodeTypes.ReadInputReg && addr == /* status register address */) {
        var status = new FormatStatus(data.Data, htool.Gen);

        Console.WriteLine($"Torque: {status.Torque:F2}");
        Console.WriteLine($"Speed: {status.Speed} RPM");
        Console.WriteLine($"Current: {status.Current:F2} A");
        Console.WriteLine($"Preset: {status.Preset}");
        Console.WriteLine($"Torque Up: {status.TorqueUp}");
        Console.WriteLine($"Fasten OK: {status.FastenOk}");
        Console.WriteLine($"Ready: {status.Ready}");
        Console.WriteLine($"Running: {status.Run}");
        Console.WriteLine($"Alarm: {status.Alarm}");
        Console.WriteLine($"Direction: {status.Direction}");
        Console.WriteLine($"Remain Screw: {status.RemainScrew}");
        Console.WriteLine($"Temperature: {status.Temperature:F1}°C");

        // I/O signals (16-bit)
        for (int i = 0; i < 16; i++) {
            if (status.Input[i]) Console.WriteLine($"Input {i}: ON");
            if (status.Output[i]) Console.WriteLine($"Output {i}: ON");
        }
    }
};
```

### 5. Event (Fastening Result) Data Processing

```csharp
htool.ReceivedData += (code, addr, data) => {
    if (code == CodeTypes.ReadHoldingReg && addr == /* event register address */) {
        var evt = new FormatEvent(data.Data, htool.Gen);

        Console.WriteLine($"=== Fastening Event #{evt.Id} ===");
        Console.WriteLine($"DateTime: {evt.Date:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"Result: {evt.Event}");
        Console.WriteLine($"Direction: {evt.Direction}");
        Console.WriteLine($"Preset: {evt.Preset}");
        Console.WriteLine($"Fasten Time: {evt.FastenTime}ms");
        Console.WriteLine($"Target Torque: {evt.TargetTorque:F2}");
        Console.WriteLine($"Measured Torque: {evt.Torque:F2}");
        Console.WriteLine($"Seating Torque: {evt.SeatingTorque:F2}");
        Console.WriteLine($"Clamp Torque: {evt.ClampTorque:F2}");
        Console.WriteLine($"Prevailing Torque: {evt.PrevailingTorque:F2}");
        Console.WriteLine($"Snug Torque: {evt.SnugTorque:F2}");
        Console.WriteLine($"Speed: {evt.Speed} RPM");
        Console.WriteLine($"Angle1: {evt.Angle1}°");
        Console.WriteLine($"Angle2: {evt.Angle2}°");
        Console.WriteLine($"Total Angle: {evt.Angle}°");
        Console.WriteLine($"Snug Angle: {evt.SnugAngle}°");
        Console.WriteLine($"Barcode: {evt.Barcode}");
        Console.WriteLine($"Error Code: {evt.Error}");

        // Graph step info (Gen.2)
        if (htool.Gen == GenerationTypes.GenRev2) {
            Console.WriteLine($"Channel1 Type: {evt.TypeOfChannel1}");
            Console.WriteLine($"Channel1 Points: {evt.CountOfChannel1}");
            Console.WriteLine($"Channel2 Type: {evt.TypeOfChannel2}");
            Console.WriteLine($"Channel2 Points: {evt.CountOfChannel2}");
            Console.WriteLine($"Sampling Rate: {evt.SamplingRate}");

            foreach (var step in evt.GraphSteps) {
                if (step != null && step.Id != GraphStepTypes.None) {
                    Console.WriteLine($"  Step: {step.Id} at index {step.Index}");
                }
            }
        }
    }
};
```

### 6. Keep-Alive Configuration

```csharp
var htool = new HTool.HTool(ComTypes.Tcp);

// Enable keep-alive (sends Info request every 3s when idle)
htool.EnableKeepAlive = true;

htool.Connect("192.168.1.100", 502, 0x01);

// Auto-disconnect after 10s of no response
```

### 7. Raw Packet Monitoring

```csharp
// Monitor outgoing packets
htool.TransmitRawData += (packet) => {
    Console.WriteLine($"TX [{packet.Length}]: {BitConverter.ToString(packet)}");
};

// Monitor incoming packets
htool.ReceivedRawData += (packet) => {
    Console.WriteLine($"RX [{packet.Length}]: {BitConverter.ToString(packet)}");
};

// Example output:
// TX [12]: 00-01-00-00-00-06-00-03-00-00-00-0A
// RX [29]: 00-01-00-00-00-17-00-03-14-00-01-00-02-...
```

### 8. String Register Operations

```csharp
// Write string (2 bytes per register)
htool.WriteStrReg(addr: 200, str: "HANTAS", length: 10);

// Read string
htool.ReceivedData += (code, addr, data) => {
    if (code == CodeTypes.ReadHoldingReg && addr == 200) {
        string text = Utils.ToAsciiTrimEnd(data.Data);
        Console.WriteLine($"Device Name: {text}");
    }
};

htool.ReadHoldingReg(addr: 200, count: 5);  // 10 bytes = 5 registers
```

### 9. Torque Unit Conversion

```csharp
using HTool.Util;
using HTool.Type;

// N.m → kgf.cm conversion
float torqueNm = 10.0f;
float torqueKgfCm = Utils.ConvertTorqueUnit(torqueNm, UnitTypes.Nm, UnitTypes.KgfCm);
Console.WriteLine($"{torqueNm} N.m = {torqueKgfCm:F2} kgf.cm");

// Parse unit string
UnitTypes unit = Utils.ParseToUnit("N.m");  // UnitTypes.Nm
string unitStr = Utils.ParseToUnit(2);       // "N.m"
```

---

## Architecture

### Project Structure

```
HTool/
├── HTool.cs                    # Main library class
├── Device/
│   ├── ITool.cs                # Communication interface
│   ├── Tool.cs                 # Factory class
│   ├── HcRtu.cs                # MODBUS RTU implementation
│   └── HcTcp.cs                # MODBUS TCP implementation
├── Data/
│   ├── IReceivedData.cs        # Received data interface
│   ├── HcRtuData.cs            # RTU data container
│   └── HcTcpData.cs            # TCP data container
├── Format/
│   ├── FormatInfo.cs           # Device information (13+ bytes)
│   ├── FormatStatus.cs         # Status data (generation-dependent)
│   ├── FormatEvent.cs          # Event data (generation-dependent)
│   ├── FormatGraph.cs          # Graph data
│   └── FormatMessage.cs        # Internal message queue item
├── Type/
│   ├── CodeTypes.cs            # MODBUS Function Codes
│   ├── ComTypes.cs             # Communication types/error types
│   ├── ConnectionTypes.cs      # Connection states
│   ├── ModelTypes.cs           # Device models
│   ├── GenerationTypes.cs      # Firmware generations
│   ├── UnitTypes.cs            # Torque units
│   ├── EventTypes.cs           # Event states
│   ├── DirectionTypes.cs       # Fastening directions
│   ├── GraphTypes.cs           # Graph channel types
│   ├── GraphStepTypes.cs       # Graph step types
│   ├── GraphDirectionTypes.cs  # Graph direction options
│   ├── OptionTypes.cs          # Graph options
│   ├── SampleTypes.cs          # Sampling types
│   └── WordOrderTypes.cs       # 32-bit word order
└── Util/
    ├── Constants.cs            # Constant definitions
    ├── KeyedQueue.cs           # Thread-safe duplicate-preventing queue
    ├── RingBuffer.cs           # Ring buffer for packet parsing
    ├── BinarySpanReader.cs     # Big-Endian read utility
    └── Utils.cs                # General utility methods
```

### Core Design Patterns

#### 1. Factory Pattern

```csharp
// Create communication objects via Tool.Create()
ITool tool = Tool.Create(ComTypes.Tcp);  // Returns HcTcp instance
ITool tool = Tool.Create(ComTypes.Rtu);  // Returns HcRtu instance
```

#### 2. Event-Driven Communication

```csharp
// All communication handled asynchronously via events
htool.ChangedConnect += OnConnectionChanged;
htool.ReceivedData += OnDataReceived;
htool.ReceiveError += OnError;
htool.ReceivedRawData += OnRawDataReceived;
htool.TransmitRawData += OnRawDataTransmitted;
```

#### 3. Thread-Safe Message Queue

```csharp
// KeyedQueue prevents duplicate messages
private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue =
    KeyedQueue<FormatMessage, FormatMessage.MessageKey>
        .Create(m => m.Key, capacity: 64);

// Enforce uniqueness mode
MessageQue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// Allow duplicates mode
MessageQue.TryEnqueue(msg, EnqueueMode.AllowDuplicate);
```

#### 4. Timer-Based Message Processing

```csharp
// Process message queue every 20ms
// - Message transmission
// - Timeout checking
// - Retry logic
// - Keep-alive handling
```

---

## Data Formats

### FormatSimpleInfo - Device Information (Legacy Protocol)

> **Note**: This class is for Gen.1/1+ legacy protocol.

| Field      | Type       | Size | Description                        |
|------------|------------|------|------------------------------------|
| Id         | int        | 2    | Device ID                          |
| Controller | int        | 2    | Controller model number            |
| Driver     | int        | 2    | Driver model number                |
| Firmware   | int        | 2    | Firmware version                   |
| Serial     | string     | 5    | Serial number (10-char string)     |
| Used       | uint       | 4    | Usage count                        |
| Model      | ModelTypes | -    | Model type (extracted from serial) |

### FormatInfo - Device Information (Gen.2 Modbus Standard Protocol)

> **Note**: This class is only for Gen.2 devices. Do not use for other device types.

| Field                  | Type   | Offset | Size | Description                         |
|------------------------|--------|--------|------|-------------------------------------|
| SystemInfo             | int    | 0      | 2    | System info (reserved)              |
| DriverId               | int    | 2      | 2    | Driver ID (1-15)                    |
| DriverModelNumber      | int    | 4      | 2    | Driver model number                 |
| DriverModelName        | string | 6      | 32   | Driver model name (ASCII)           |
| DriverSerialNumber     | string | 38     | 10   | Driver serial number                |
| ControllerModelNumber  | int    | 48     | 2    | Controller model number             |
| ControllerModelName    | string | 50     | 32   | Controller model name (ASCII)       |
| ControllerSerialNumber | string | 82     | 10   | Controller serial number            |
| FirmwareVersionMajor   | int    | 92     | 2    | Firmware version Major              |
| FirmwareVersionMinor   | int    | 94     | 2    | Firmware version Minor              |
| FirmwareVersionPatch   | int    | 96     | 2    | Firmware version Patch              |
| FirmwareVersion        | string | -      | -    | Firmware version string (computed)  |
| ProductionDate         | uint   | 98     | 4    | Production date (YYYYMMDD)          |
| AdvanceType            | int    | 102    | 2    | Advance type (0=Normal, 1=Plus)     |
| MacAddress             | byte[] | 104    | 6    | MAC address                         |
| MacAddressString       | string | -      | -    | MAC address string (computed)       |
| EventDataRevision      | int    | 110    | 2    | Event data revision                 |
| Manufacturer           | int    | 112    | 2    | Manufacturer (1=Hantas, 2=Mountz)   |
| Reserved               | -      | 114    | 86   | Reserved area                       |

**Total Size**: 200 bytes (100 registers)

### FormatStatus - Status Data

| Field       | Gen.1/1+       | Gen.2          | Description         |
|-------------|----------------|----------------|---------------------|
| Torque      | ushort         | float          | Current torque      |
| Speed       | ushort         | ushort         | Current speed (RPM) |
| Current     | ushort         | float          | Current (A)         |
| Preset      | ushort         | ushort         | Selected preset     |
| Model       | -              | ushort         | Selected model      |
| TorqueUp    | bool           | bool           | Torque up state     |
| FastenOk    | bool           | bool           | Fasten OK state     |
| Ready       | bool           | bool           | Ready state         |
| Run         | bool           | bool           | Running state       |
| Alarm       | ushort         | ushort         | Alarm code          |
| Direction   | DirectionTypes | DirectionTypes | Fastening direction |
| RemainScrew | ushort         | ushort         | Remaining screws    |
| Input       | bool[16]       | bool[16]       | Input signals       |
| Output      | bool[16]       | bool[16]       | Output signals      |
| Temperature | ushort         | float          | Temperature         |
| IsLock      | -              | bool           | Lock state          |

### FormatEvent - Event Data

**Gen.1/1+ Common Fields:**

| Field                 | Description         |
|-----------------------|---------------------|
| Id                    | Event ID            |
| FastenTime            | Fastening time (ms) |
| Preset                | Preset number       |
| TargetTorque          | Target torque       |
| Torque                | Measured torque     |
| Speed                 | Speed               |
| Angle1, Angle2, Angle | Angle values        |
| RemainScrew           | Remaining screws    |
| Error                 | Error code          |
| Direction             | Fastening direction |
| Event                 | Event status        |
| SnugAngle             | Snug angle          |
| Barcode               | Barcode (64 bytes)  |

**Gen.1+ Additional Fields:**

| Field            | Description       |
|------------------|-------------------|
| SeatingTorque    | Seating torque    |
| ClampTorque      | Clamp torque      |
| PrevailingTorque | Prevailing torque |
| SnugTorque       | Snug torque       |

**Gen.2 Additional Fields:**

| Field             | Description                   |
|-------------------|-------------------------------|
| Revision          | Event format revision         |
| Date/Time         | Fastening date/time (with ms) |
| Unit              | Torque unit                   |
| TypeOfChannel1/2  | Graph channel type            |
| CountOfChannel1/2 | Graph point count             |
| SamplingRate      | Sampling rate                 |
| GraphSteps[16]    | Graph step information        |

### FormatGraph - Graph Data

| Field    | Type            | Description       |
|----------|-----------------|-------------------|
| Type     | GenerationTypes | Generation type   |
| Channel  | int             | Channel number    |
| Count    | int             | Data point count  |
| Values   | float[]         | Graph value array |
| CheckSum | int             | Checksum          |

---

## Utility Classes

### BinarySpanReader

Static utility for reading Big-Endian binary data:

```csharp
var span = data.AsSpan();
var pos = 0;

byte b = BinarySpanReader.ReadByte(span, ref pos);
short s = BinarySpanReader.ReadInt16(span, ref pos);
ushort us = BinarySpanReader.ReadUInt16(span, ref pos);
int i = BinarySpanReader.ReadInt32(span, ref pos);
uint ui = BinarySpanReader.ReadUInt32(span, ref pos);
long l = BinarySpanReader.ReadInt64(span, ref pos);
ulong ul = BinarySpanReader.ReadUInt64(span, ref pos);
float f = BinarySpanReader.ReadSingle(span, ref pos);
double d = BinarySpanReader.ReadDouble(span, ref pos);
string str = BinarySpanReader.ReadAsciiString(span, ref pos, 32);  // ASCII string (null/space trimmed)

// Read without position (from first byte)
ushort value = BinarySpanReader.ReadUInt16(span);
```

### KeyedQueue<T, TKey>

Thread-safe, high-performance queue with duplicate prevention:

```csharp
// Create
var queue = KeyedQueue<MyMessage, string>
    .Create(m => m.Id, capacity: 100);

// Add single item
queue.TryEnqueue(msg, EnqueueMode.EnforceUnique);  // Prevent duplicates
queue.TryEnqueue(msg, EnqueueMode.AllowDuplicate); // Allow duplicates

// Batch add
var result = queue.TryEnqueueRange(messages, EnqueueMode.EnforceUnique);
Console.WriteLine($"Added: {result.Accepted}, Skipped: {result.Skipped}");

// Remove
if (queue.TryDequeue(out var item)) { /* process */ }

// Blocking remove (with timeout)
if (queue.TryDequeue(out var item, timeoutMs: 1000)) { /* process */ }

// Peek
if (queue.TryPeek(out var item)) { /* check only */ }

// Key-based operations
bool exists = queue.ContainsKey(key);
int pending = queue.PendingCountByKey(key);
queue.TryRemoveByKey(key);      // Remove first
queue.RemoveAllByKey(key);      // Remove all

// State check
int count = queue.Count;
bool empty = queue.IsEmpty;
int uniqueKeys = queue.UniqueKeyCount;

// Snapshot
List<MyMessage> snapshot = queue.Snapshot();
IReadOnlyDictionary<string, int> keySnapshot = queue.GetKeySnapshot();

// Cleanup
queue.Clear();
queue.TrimExcess();
queue.Dispose();
```

### RingBuffer

Circular buffer for packet parsing:

```csharp
// Create (capacity rounded to power of 2)
var buffer = new RingBuffer(16 * 1024);

// Write
buffer.Write(singleByte);
buffer.WriteBytes(byteArray);
buffer.WriteBytes(readOnlySpan);

// Peek (read without removal)
byte b = buffer.Peek(offset);
ReadOnlySpan<byte> data = buffer.PeekBytes();

// Read (with removal)
byte[] packet = buffer.ReadBytes(length);

// Remove (without reading)
buffer.RemoveBytes(length);

// State
int capacity = buffer.Capacity;
int available = buffer.Available;

// Clear
buffer.Clear();
```

### Utils

Various utility methods:

```csharp
// CRC calculation (MODBUS RTU)
var (low, high) = Utils.CalculateCrc(packet);
Utils.CalculateCrcTo(packet, crcBuffer);
bool valid = Utils.ValidateCrc(packetWithCrc);

// Checksum
int sum = Utils.CalculateCheckSum(packet);

// Torque unit conversion
float converted = Utils.ConvertTorqueUnit(10.0f, UnitTypes.Nm, UnitTypes.KgfCm);
UnitTypes unit = Utils.ParseToUnit("N.m");
string unitStr = Utils.ParseToUnit(2);

// Byte → value conversion
Utils.ConvertValue(bytes, out float floatValue, WordOrderTypes.HighLow);
Utils.ConvertValue(bytes, out ushort ushortValue);
Utils.ConvertValue(bytes, out int intValue, WordOrderTypes.LowHigh);

// Value → ushort[] conversion
ushort[] words = Utils.GetValuesFromValue(intValue, WordOrderTypes.HighLow);
ushort[] words = Utils.GetValuesFromValue(floatValue);

// String conversion
ushort[] words = Utils.GetWordValuesFromText("HANTAS");
string text = Utils.ToAsciiTrimEnd(byteSpan);

// Time utilities
long ticks = Utils.GetCurrentTicks();
long ms = Utils.TimeLapsMs(startTime);
long sec = Utils.TimeLapsSec(startTime);

// List swap
Utils.Swap(list, sourceIndex, destIndex);
```

---

## MODBUS Protocol

### Function Codes

| Code             | Value | Description              | Type     |
|------------------|-------|--------------------------|----------|
| `ReadHoldingReg` | 0x03  | Read Holding Registers   | Standard |
| `ReadInputReg`   | 0x04  | Read Input Registers     | Standard |
| `WriteSingleReg` | 0x06  | Write Single Register    | Standard |
| `WriteMultiReg`  | 0x10  | Write Multiple Registers | Standard |
| `ReadInfoReg`    | 0x11  | Read Device Information  | HANTAS   |
| `Graph`          | 0x64  | Graph Data               | HANTAS   |
| `GraphRes`       | 0x65  | Graph Result             | HANTAS   |
| `HighResGraph`   | 0x66  | High Resolution Graph    | HANTAS   |
| `Error`          | 0x80  | Error Response           | Standard |

### RTU Frame Structure

```
[Device ID (1)] [Function Code (1)] [Data (N)] [CRC Low (1)] [CRC High (1)]
```

### TCP Frame Structure (MBAP Header)

```
[Transaction ID (2)] [Protocol ID (2)] [Length (2)] [Unit ID (1)] [Function Code (1)] [Data (N)]
```

---

## Error Handling

### Error Codes

| Error               | Value | Description               |
|---------------------|-------|---------------------------|
| `InvalidFunction`   | 0x01  | Unsupported Function Code |
| `InvalidAddress`    | 0x02  | Invalid register address  |
| `InvalidValue`      | 0x03  | Invalid data value        |
| `InvalidCrc`        | 0x07  | CRC checksum error (RTU)  |
| `InvalidFrame`      | 0x0C  | Malformed frame           |
| `InvalidValueRange` | 0x0E  | Value out of range        |
| `Timeout`           | 0x0F  | Request timeout           |

### Error Handling Example

```csharp
htool.ReceiveError += (reason, param) => {
    switch (reason) {
        case ComErrorTypes.Timeout:
            Console.WriteLine($"Timeout - Buffer size: {param}");
            // Retry logic or reconnect
            break;

        case ComErrorTypes.InvalidCrc:
            Console.WriteLine("CRC error - Check cable/baud rate");
            break;

        case ComErrorTypes.InvalidFunction:
            Console.WriteLine("Unsupported Function Code");
            break;

        case ComErrorTypes.InvalidAddress:
            Console.WriteLine("Invalid register address");
            break;

        case ComErrorTypes.InvalidValue:
            Console.WriteLine("Invalid data value");
            break;

        case ComErrorTypes.InvalidFrame:
            Console.WriteLine("Frame format error");
            break;

        case ComErrorTypes.InvalidValueRange:
            Console.WriteLine("Value out of range");
            break;
    }
};
```

---

## Configuration Constants

Constants defined in `HTool.Util.Constants` class:

| Constant           | Value                                       | Description                 |
|--------------------|---------------------------------------------|-----------------------------|
| `BarcodeLength`    | 64                                          | Barcode field length        |
| `BaudRates`        | [9600, 19200, 38400, 57600, 115200, 230400] | Supported baud rates        |
| `ProcessPeriod`    | 20ms                                        | Message processing interval |
| `ProcessLockTime`  | 2ms                                         | Processing lock timeout     |
| `ProcessTimeout`   | 500ms                                       | Processing timeout          |
| `ConnectTimeout`   | 5000ms                                      | Connection timeout          |
| `MessageTimeout`   | 1000ms                                      | Message timeout             |
| `KeepAlivePeriod`  | 3000ms                                      | Keep-alive request interval |
| `KeepAliveTimeout` | 10s                                         | Keep-alive timeout          |

**HTool Class Constants:**

| Constant          | Value | Description                     |
|-------------------|-------|---------------------------------|
| `ReadRegMaxSize`  | 125   | Max registers per read request  |
| `WriteRegMaxSize` | 123   | Max registers per write request |

---

## Troubleshooting

### TCP Connection Failure

```csharp
// Checklist:
// 1. Verify IP address and port (default MODBUS TCP: 502)
// 2. Check firewall settings
// 3. Ensure same network/VLAN
// 4. Test with ping

if (!htool.Connect("192.168.1.100", 502, 0x01)) {
    Console.WriteLine("Connection start failed");
    // Check IP, port, network settings
}
```

### RTU Connection Failure

```csharp
// Check available COM ports
foreach (var port in HcRtu.GetPortNames()) {
    Console.WriteLine($"Available: {port}");
}

// Checklist:
// 1. Verify COM port name (Device Manager)
// 2. Check baud rate (common: 115200, 9600)
// 3. Ensure port not in use by another program
// 4. Check cable connections
```

### CRC Errors (RTU)

```csharp
htool.ReceiveError += (reason, param) => {
    if (reason == ComErrorTypes.InvalidCrc) {
        // Common causes:
        // - Baud rate mismatch
        // - Poor cable quality
        // - Electromagnetic interference
        // - Incorrect parity/stop bits
        Console.WriteLine("CRC error occurred");
    }
};
```

### Timeout Errors

```csharp
htool.ReceiveError += (reason, param) => {
    if (reason == ComErrorTypes.Timeout) {
        Console.WriteLine($"Timeout - Cleared buffer: {param} bytes");
        // Causes:
        // - Device response delay
        // - Incorrect register address
        // - Network latency
    }
};

// Retry logic example
int retryCount = 3;
bool SendWithRetry(ushort addr, ushort count) {
    for (int i = 0; i < retryCount; i++) {
        if (htool.ReadHoldingReg(addr, count, check: false)) {
            // Wait for response...
            return true;
        }
        Thread.Sleep(100);
    }
    return false;
}
```

### No Data Received

1. Check connection state:

```csharp
if (htool.ConnectionState != ConnectionTypes.Connected) {
    Console.WriteLine("Not connected");
    return;
}
```

2. Monitor traffic with raw packet monitoring:

```csharp
htool.TransmitRawData += (p) => Console.WriteLine($"TX: {BitConverter.ToString(p)}");
htool.ReceivedRawData += (p) => Console.WriteLine($"RX: {BitConverter.ToString(p)}");
```

3. Verify register addresses (check device manual)

---

## Performance Optimization

### Message Queue Optimization

```csharp
// Queue capacity: 64 messages
// For high-frequency requests, disable duplicate checking
htool.ReadHoldingReg(addr, count, check: false);
```

### Optimal Split Sizes

```csharp
// Stable network: larger splits (less overhead)
htool.ReadHoldingReg(addr: 0, count: 1000, split: 125);

// Unreliable connection: smaller splits (less timeout risk)
htool.ReadHoldingReg(addr: 0, count: 1000, split: 50);
```

### Keep-Alive Usage

```csharp
// Enable only for long-running connections (3s periodic traffic)
htool.EnableKeepAlive = true;   // For monitoring apps

htool.EnableKeepAlive = false;  // For quick operations
```

### Span-Based Data Processing

```csharp
// Minimize memory allocation
htool.ReceivedData += (code, addr, data) => {
    var span = data.Data.AsSpan();
    var pos = 0;

    // Use BinarySpanReader to parse without stream object creation
    while (pos < span.Length - 2) {
        ushort value = BinarySpanReader.ReadUInt16(span, ref pos);
        // Process...
    }
};
```

---

## Version History

### 1.1.22 - Current

- FormatInfo class refactoring
  - Existing FormatInfo → FormatSimpleInfo (legacy protocol)
  - New FormatInfo (Gen.2 Modbus standard protocol, 200 bytes)
- Added BinarySpanReader.ReadAsciiString method

### 1.1.21

- Added BinarySpanReader (replaces BinaryReaderBigEndian)
- Improved XML comments and fixed errors
- Performance optimizations

### 1.1.20

- KeyedQueue improvements
- RingBuffer optimization
- Constants cleanup

### 1.0.2 - 2025.01.27

- Minor bug fixes

### 1.0.1 - 2024.12.09

- Minor bug fixes

### 1.0.0 - 2024.09.13

- Initial release

---

## License

This project is licensed under the MIT License.

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

---

## Support

- **Company**: HANTAS
- **Author**: Eloiz
- **GitHub**: [https://github.com/KyungtackKim/HToolEx](https://github.com/KyungtackKim/HToolEx)
- **Issues**: [GitHub Issues](https://github.com/KyungtackKim/HToolEx/issues)

---

**HTool** - Professional MODBUS communication library for HANTAS torque tools
