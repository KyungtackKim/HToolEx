# HTool

A comprehensive C# library for MODBUS communication with HANTAS industrial torque tools and controllers.

HANTAS 산업용 토크 툴 및 컨트롤러를 위한 MODBUS 통신 C# 라이브러리

[![NuGet](https://img.shields.io/badge/nuget-v1.1.22-blue)](https://www.nuget.org/packages/HTool)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://www.microsoft.com/windows)

---

## Documentation / 문서

Please select your preferred language / 원하시는 언어를 선택하세요:

### 📖 [English Documentation](README-ENG.md)

Complete English documentation with detailed API reference, examples, and usage guides.

### 📖 [한국어 문서](README-KOR.md)

상세한 API 레퍼런스, 예제 및 사용 가이드가 포함된 한국어 문서입니다.

---

## Quick Links

- **NuGet Package**: [HTool on NuGet](https://www.nuget.org/packages/HTool)
- **GitHub Repository**: [HToolEx/HTool](https://github.com/KyungtackKim/HToolEx/tree/master/HTool)
- **Issues**: [GitHub Issues](https://github.com/KyungtackKim/HToolEx/issues)

---

## Quick Start / 빠른 시작

### Installation / 설치

```bash
dotnet add package HTool
```

### Basic Usage / 기본 사용법

```csharp
using HTool;
using HTool.Type;

// Create HTool instance for TCP
var htool = new HTool.HTool(ComTypes.Tcp);

// Subscribe to events
htool.ChangedConnect += (connected) => {
    if (connected) {
        Console.WriteLine($"Connected: {htool.Info.Model}");
    }
};

// Connect to device
htool.Connect("192.168.1.100", 5000, id: 0x01);
```

For more details, please refer to the full documentation in your preferred language.

자세한 내용은 원하시는 언어의 전체 문서를 참조하세요.

---

## License / 라이선스

MIT License - Copyright (c) HANTAS

---

**HTool** - Professional MODBUS communication library for HANTAS torque tools
