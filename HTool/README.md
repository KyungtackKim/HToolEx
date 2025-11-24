# HTool

A C# library for MODBUS communication with HANTAS industrial torque tools and controllers.

[![NuGet](https://img.shields.io/badge/nuget-v1.1.21-blue)](https://www.nuget.org/packages/HTool)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)

## Overview

**HTool** is a comprehensive C# library that provides MODBUS RTU/TCP communication capabilities for HANTAS torque tools and controllers. It features an event-driven architecture with automatic message queuing, register auto-splitting, and thread-safe operations, making it easy to integrate HANTAS devices into your .NET applications.

## Key Features

- **Dual Protocol Support**: MODBUS RTU (serial) and MODBUS TCP/IP
- **Event-Driven Architecture**: Asynchronous callbacks for connection state, data reception, and errors
- **Automatic Message Queuing**: Thread-safe queue with duplicate prevention and retry logic
- **Smart Register Operations**: Automatic chunking of large read/write operations (max 125 read, 123 write)
- **Keep-Alive Mechanism**: Automatic connection monitoring with configurable timeout
- **Device Information**: Automatic parsing of device info (model, serial, firmware, generation)
- **Graph Data Support**: Torque and angle graph data collection
- **Custom MODBUS Functions**: Support for standard and HANTAS-specific function codes
- **Raw Packet Monitoring**: Debug-friendly packet inspection capabilities

## Requirements

- **.NET 8.0** or later
- **Windows 10.0.17763.0** or later
- For RTU communication: Available COM port
- For TCP communication: Network access to device

## Installation

### Via NuGet (Recommended)

```bash
dotnet add package HTool
```

Or via Package Manager Console:

```powershell
Install-Package HTool
```

### From Source

```bash
git clone https://github.com/KyungtackKim/HToolEx.git
cd HToolEx/HTool
dotnet build -c Release
```

## Quick Start

### MODBUS TCP Example

```csharp
using HTool;
using HTool.Type;

// Create HTool instance for TCP communication
var htool = new HTool(ComTypes.Tcp);

// Subscribe to connection events
htool.ChangedConnect += (connected) =>
{
    if (connected)
    {
        Console.WriteLine($"Connected to {htool.Info.Model}");
        Console.WriteLine($"Serial: {htool.Info.Serial}");
        Console.WriteLine($"Firmware: {htool.Info.Firmware}");
    }
    else
    {
        Console.WriteLine("Disconnected");
    }
};

// Subscribe to data events
htool.ReceivedData += (code, addr, data) =>
{
    Console.WriteLine($"Received: Code={code}, Address={addr}");
    // Process data.Data byte array
};

// Connect to device (IP address, port, device ID)
htool.Connect("192.168.1.100", 502, id: 0x01);

// Read 10 holding registers starting at address 0
htool.ReadHoldingReg(addr: 0, count: 10);

// Write single register
htool.WriteSingleReg(addr: 100, value: 1234);

// Close connection when done
htool.Close();
```

### MODBUS RTU (Serial) Example

```csharp
using HTool;
using HTool.Type;

var htool = new HTool(ComTypes.Rtu);

// Connect via COM port (port name, baud rate, device ID)
htool.Connect("COM3", 115200, id: 0x01);

// Use same API as TCP
htool.ReadHoldingReg(addr: 0, count: 50);
```

## Core API

### HTool Class

Main class providing all communication functionality.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `ComTypes` | Communication type (RTU/TCP) |
| `ConnectionState` | `ConnectionTypes` | Current connection state |
| `Info` | `FormatInfo` | Device information (after connection) |
| `Gen` | `GenerationTypes` | Device generation/revision |
| `EnableKeepAlive` | `bool` | Enable automatic keep-alive (default: false) |

#### Methods

**Connection Management**

```csharp
bool Connect(string target, int option, byte id = 0x01)
void Close()
```

- **TCP**: `target` = IP address, `option` = port number
- **RTU**: `target` = COM port name, `option` = baud rate

**Register Operations**

```csharp
// Read operations
bool ReadHoldingReg(ushort addr, ushort count, int split = 0, bool check = true)
bool ReadInputReg(ushort addr, ushort count, int split = 0, bool check = true)
bool ReadInfoReg(bool check = true)

// Write operations
bool WriteSingleReg(ushort addr, ushort value, bool check = true)
bool WriteMultiReg(ushort addr, ushort[] values, bool check = true)
bool WriteStrReg(ushort addr, string str, int length = 0, bool check = true)
```

**Parameters**:
- `addr`: Starting register address
- `count`: Number of registers to read
- `split`: Custom split size (0 = automatic)
- `check`: Enable duplicate checking in queue
- `value/values`: Data to write
- `str`: ASCII string to write (2 bytes per register)

#### Events

```csharp
event PerformChangedConnect ChangedConnect;      // Connection state changed
event PerformReceivedData ReceivedData;          // Data received
event ITool.PerformReceiveError ReceiveError;    // Error occurred
event PerformRawData ReceivedRawData;            // Raw data received (debug)
event PerformRawData TransmitRawData;            // Raw data transmitted (debug)
```

## Usage Examples

### 1. Large Register Operations

HTool automatically splits large operations into multiple MODBUS requests:

```csharp
// Read 500 registers - automatically split into 4 requests of 125
htool.ReadHoldingReg(addr: 0, count: 500);

// Custom split size
htool.ReadHoldingReg(addr: 0, count: 500, split: 100);

// Write 200 registers - automatically split by 123
ushort[] data = new ushort[200];
// ... populate data ...
htool.WriteMultiReg(addr: 1000, data);
```

### 2. Connection State Handling

```csharp
htool.ChangedConnect += (connected) =>
{
    if (connected)
    {
        // Connection established and device info received
        Console.WriteLine($"Device Model: {htool.Info.Model}");
        Console.WriteLine($"Serial Number: {htool.Info.Serial}");
        Console.WriteLine($"Firmware Version: {htool.Info.Firmware}");
        Console.WriteLine($"Generation: {htool.Gen}");

        // Start your operations here
    }
    else
    {
        // Connection lost
        Console.WriteLine("Device disconnected");
    }
};
```

**Connection Flow**:
1. `Connect()` called → `ConnectionState = Connecting`
2. Library automatically sends `ReadInfoReg()` request
3. Device info received → `ConnectionState = Connected` → `ChangedConnect(true)` fired
4. Timeout (10s) or error → `ConnectionState = Close`

### 3. Data Parsing

```csharp
using HTool.Util;

htool.ReceivedData += (code, addr, data) =>
{
    switch (code)
    {
        case CodeTypes.ReadHoldingReg:
        case CodeTypes.ReadInputReg:
            // Parse register data (Big-Endian)
            using var stream = new MemoryStream(data.Data);
            using var reader = new BinaryReaderBigEndian(stream);

            ushort reg0 = reader.ReadUInt16();
            ushort reg1 = reader.ReadUInt16();
            int reg2_3 = reader.ReadInt32();  // 2 registers combined
            break;

        case CodeTypes.ReadInfoReg:
            // Device info already parsed into htool.Info
            break;

        case CodeTypes.Graph:
            var graph = new FormatGraph(data.Data);
            // Process torque/angle graph data
            break;
    }
};
```

### 4. Error Handling

```csharp
htool.ReceiveError += (reason, param) =>
{
    switch (reason)
    {
        case ComErrorTypes.Timeout:
            Console.WriteLine("Request timeout - device not responding");
            break;
        case ComErrorTypes.InvalidCrc:
            Console.WriteLine("CRC error - check cable quality");
            break;
        case ComErrorTypes.InvalidFunction:
            Console.WriteLine($"Invalid function code: {param}");
            break;
        case ComErrorTypes.InvalidAddress:
            Console.WriteLine("Invalid register address");
            break;
    }
};
```

### 5. Keep-Alive Configuration

```csharp
var htool = new HTool(ComTypes.Tcp);

// Enable keep-alive (sends Info request every 3s when idle)
htool.EnableKeepAlive = true;

htool.Connect("192.168.1.100", 502, 0x01);

// Connection will auto-close after 10s of no response
```

### 6. Raw Packet Monitoring

```csharp
// Monitor outgoing packets
htool.TransmitRawData += (packet) =>
{
    Console.WriteLine($"TX: {BitConverter.ToString(packet)}");
};

// Monitor incoming packets
htool.ReceivedRawData += (packet) =>
{
    Console.WriteLine($"RX: {BitConverter.ToString(packet)}");
};

// Example output:
// TX: 01-03-00-00-00-0A-C5-CD
// RX: 01-03-14-00-01-00-02-00-03-00-04-00-05-...
```

### 7. String Register Operations

```csharp
// Write ASCII string to registers
htool.WriteStrReg(addr: 200, str: "HANTAS", length: 10);

// Read string back
htool.ReceivedData += (code, addr, data) =>
{
    if (code == CodeTypes.ReadHoldingReg && addr == 200)
    {
        string text = Encoding.ASCII.GetString(data.Data).TrimEnd('\0');
        Console.WriteLine($"Device Name: {text}");
    }
};

htool.ReadHoldingReg(addr: 200, count: 10);
```

## Architecture

### Core Components

```
HTool/
├── HTool.cs              # Main library class
├── Device/
│   ├── ITool.cs          # Communication interface
│   ├── Tool.cs           # Factory for creating devices
│   ├── HcRtu.cs          # MODBUS RTU implementation
│   └── HcTcp.cs          # MODBUS TCP implementation
├── Data/
│   ├── HcRtuData.cs      # RTU data container
│   └── HcTcpData.cs      # TCP data container
├── Format/
│   ├── FormatInfo.cs     # Device information
│   ├── FormatStatus.cs   # Status data
│   ├── FormatEvent.cs    # Event data
│   ├── FormatGraph.cs    # Graph data
│   └── FormatMessage.cs  # Internal message queue item
├── Type/
│   ├── CodeTypes.cs      # MODBUS function codes
│   ├── ComTypes.cs       # Communication types
│   ├── ConnectionTypes.cs# Connection states
│   ├── ModelTypes.cs     # Device models
│   └── ...               # Other enumerations
└── Util/
    ├── KeyedQueue.cs     # Thread-safe duplicate-preventing queue
    ├── RingBuffer.cs     # Circular buffer for packet parsing
    ├── BinaryReaderBigEndian.cs  # Big-Endian reader
    └── Utils.cs          # Utility methods
```

### Key Design Patterns

**Factory Pattern**
```csharp
ITool tool = Tool.Create(ComTypes.Tcp);  // Creates HcTcp instance
ITool tool = Tool.Create(ComTypes.Rtu);  // Creates HcRtu instance
```

**Event-Driven Communication**
```csharp
// All communication is asynchronous via events
htool.ChangedConnect += OnConnectionChanged;
htool.ReceivedData += OnDataReceived;
htool.ReceiveError += OnError;
```

**Thread-Safe Message Queue**
```csharp
// KeyedQueue prevents duplicate messages
private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue =
    KeyedQueue<FormatMessage, FormatMessage.MessageKey>
        .Create(m => m.Key, capacity: 64);
```

## MODBUS Function Codes

HTool supports standard MODBUS functions and HANTAS-specific extensions:

| Code | Value | Description |
|------|-------|-------------|
| `ReadHoldingReg` | 0x03 | Read Holding Registers |
| `ReadInputReg` | 0x04 | Read Input Registers |
| `WriteSingleReg` | 0x06 | Write Single Register |
| `WriteMultiReg` | 0x10 | Write Multiple Registers |
| `ReadInfoReg` | 0x11 | Read Device Information (HANTAS) |
| `Graph` | 0x64 | Graph Data (HANTAS) |
| `GraphRes` | 0x65 | Graph Result (HANTAS) |
| `HighResGraph` | 0x66 | High Resolution Graph (HANTAS) |
| `Error` | 0x80 | Error Response |

## Error Codes

| Error | Value | Description |
|-------|-------|-------------|
| `InvalidFunction` | 0x01 | Unsupported function code |
| `InvalidAddress` | 0x02 | Invalid register address |
| `InvalidValue` | 0x03 | Invalid data value |
| `InvalidCrc` | 0x07 | CRC checksum mismatch (RTU) |
| `InvalidFrame` | 0x0C | Malformed packet |
| `InvalidValueRange` | 0x0E | Value out of range |
| `Timeout` | 0x0F | Request timeout |

## Configuration Constants

```csharp
ProcessPeriod = 10ms        // Message processing interval
MessageTimeout = 1000ms     // Request timeout
ConnectTimeout = 10s        // Connection establishment timeout
KeepAlivePeriod = 3000ms    // Keep-alive request interval
KeepAliveTimeout = 10s      // Keep-alive timeout

ReadRegMaxSize = 125        // Maximum registers per read
WriteRegMaxSize = 123       // Maximum registers per write
```

## Advanced Topics

### Custom Device Implementation

Implement `ITool` interface for custom communication:

```csharp
public class HcCustom : ITool
{
    public byte DeviceId { get; set; }
    public GenerationTypes Revision { get; set; }

    public bool Connect(string target, int option, byte id)
    {
        // Custom connection logic
    }

    // ... implement other ITool methods
}
```

### Custom Message Formats

Create custom format classes for parsing device-specific data:

```csharp
public sealed class FormatCustom
{
    public static int Size => 20;

    public FormatCustom(byte[] values)
    {
        using var stream = new MemoryStream(values);
        using var reader = new BinaryReaderBigEndian(stream);

        // Parse Big-Endian data
        Field1 = reader.ReadUInt16();
        Field2 = reader.ReadUInt32();
    }

    public ushort Field1 { get; }
    public uint Field2 { get; }
}
```

### Utility Classes

**KeyedQueue<T, TKey>** - Thread-safe queue with duplicate prevention:

```csharp
var queue = KeyedQueue<MyMessage, string>
    .Create(m => m.Id, capacity: 100);

// Enforce uniqueness by key
queue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// Allow duplicates
queue.TryEnqueue(msg, EnqueueMode.AllowDuplicate);
```

**BinaryReaderBigEndian** - Read MODBUS Big-Endian data:

```csharp
using var reader = new BinaryReaderBigEndian(stream);
ushort value = reader.ReadUInt16();  // Automatically handles endianness
```

**Utils** - Endianness conversion:

```csharp
// Convert bytes to value with endianness control
Utils.ConvertValue(bytes, out int value, isBigEndian: true);
```

## Troubleshooting

### Connection Issues

**TCP Connection Fails**
- Verify device IP address and port (default: 502)
- Check firewall settings
- Ensure device is on same network/VLAN
- Test with `ping` command

**RTU Connection Fails**
- Verify COM port name in Device Manager
- Check baud rate matches device (common: 115200, 9600)
- Ensure no other application is using the port
- Check cable connections

### CRC Errors (RTU)

```csharp
htool.ReceiveError += (reason, param) =>
{
    if (reason == ComErrorTypes.InvalidCrc)
    {
        // Common causes:
        // - Baud rate mismatch
        // - Poor cable quality
        // - Electromagnetic interference
        // - Incorrect parity/stop bits
    }
};
```

### Timeout Errors

```csharp
// Increase timeout is not supported directly
// Instead, use retry logic:

int retries = 3;
void SendWithRetry()
{
    htool.ReceivedData += OnDataReceived;
    htool.ReceiveError += (reason, param) =>
    {
        if (reason == ComErrorTypes.Timeout && retries-- > 0)
        {
            htool.ReadHoldingReg(addr, count);
        }
    };

    htool.ReadHoldingReg(addr, count);
}
```

### No Data Received

1. Check connection state: `htool.ConnectionState == ConnectionTypes.Connected`
2. Verify register addresses in device manual
3. Enable raw packet monitoring to inspect traffic
4. Check device-specific register map

## Performance Tips

### Message Queue Optimization

```csharp
// Queue capacity is 64 messages
// For high-frequency requests, disable duplicate checking
htool.ReadHoldingReg(addr, count, check: false);
```

### Optimal Split Sizes

```csharp
// Network: larger splits (less overhead)
htool.ReadHoldingReg(addr: 0, count: 1000, split: 125);

// Slow/unreliable links: smaller splits (less timeout risk)
htool.ReadHoldingReg(addr: 0, count: 1000, split: 50);
```

### Keep-Alive Usage

```csharp
// Only enable for long-running connections
// Adds 3-second periodic traffic
htool.EnableKeepAlive = true;  // Use for monitoring apps

htool.EnableKeepAlive = false; // Use for quick operations
```

## Dependencies

- **SuperSimpleTcp** (3.0.17) - TCP communication
- **System.IO.Ports** (9.0.9) - Serial port communication

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

## Support

- **Company**: HANTAS
- **Author**: Eloiz
- **GitHub**: [https://github.com/KyungtackKim/HToolEx](https://github.com/KyungtackKim/HToolEx)
- **Issues**: [GitHub Issues](https://github.com/KyungtackKim/HToolEx/issues)

## Version History

### 1.1.21 - Current
- Latest stable release

### 1.0.2 - 2025.01.27
- Fixed minor bugs

### 1.0.1 - 2024.12.09
- Fixed minor bugs

### 1.0.0 - 2024.09.13
- Initial release

---

**HTool** - Professional MODBUS communication library for HANTAS torque tools
