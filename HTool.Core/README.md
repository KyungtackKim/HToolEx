# HTool.Core

Core utilities and domain types for HANTAS tool communication libraries.

HANTAS 툴 통신 라이브러리용 핵심 유틸리티 및 도메인 타입

[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue)](https://www.nuget.org/packages/Hantas.HTool.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Overview / 개요

HTool.Core is a shared foundation library providing domain types (enums, constants) and utility classes used by HTool
and HTool.Format. It has no Windows-specific dependencies and targets .NET 8.0.

HTool.Core는 HTool 및 HTool.Format에서 사용하는 도메인 타입(열거형, 상수)과 유틸리티 클래스를 제공하는 공유 기반 라이브러리입니다. Windows 종속성이 없으며 .NET 8.0을 대상으로
합니다.

---

## Installation / 설치

```bash
dotnet add package Hantas.HTool.Core
```

---

## Structure / 구조

```
HTool.Core/
├── Type/
│   ├── Device/         # Model, Manufacturer, ModelNames / 모델, 제조사, 모델명 조회
│   ├── Process/        # Event, Direction, Unit, GraphStep, GraphChannel, ... / 이벤트, 방향, 단위, 그래프 스텝...
│   ├── Pro/            # MessageId, JobStep, JobEvent, LogField, ... / 메시지 ID, 작업 스텝, 작업 이벤트...
│   └── Ez/             # DeviceCommand, CalPoint, Frequency, ... / 장치 명령, 캘리브레이션 포인트, 주파수...
└── Util/
    ├── KeyedQueue.cs       # Thread-safe keyed queue (O(1) enqueue/dequeue) / 스레드 안전 키 기반 큐
    ├── RingBuffer.cs       # Circular buffer for stream parsing / 스트림 파싱용 순환 버퍼
    ├── BinarySpanReader.cs # Big-Endian binary reader (ReadOnlySpan-based) / 빅 엔디언 이진 리더
    ├── DataHash.cs         # XxHash3-based change detection / XxHash3 기반 변경 감지
    ├── EnumUtil.cs         # Enum parsing utilities / 열거형 파싱 유틸리티
    └── Utils.cs            # CRC, endianness, packing helpers / CRC, 엔디언 변환, 값 패킹 도우미
```

### Type Namespaces / 타입 네임스페이스

| Namespace                 | Description                                                                                |
|---------------------------|--------------------------------------------------------------------------------------------|
| `HTool.Core.Type.Device`  | Device model codes and manufacturer identifiers / 장치 모델 코드 및 제조사 식별자                       |
| `HTool.Core.Type.Process` | Fastening process: events, directions, units, graph types / 체결 프로세스: 이벤트, 방향, 단위, 그래프 타입   |
| `HTool.Core.Type.Pro`     | PRO X gateway: MID protocol, job steps, I/O signals / PRO X 게이트웨이: MID 프로토콜, 작업 스텝, I/O 신호 |
| `HTool.Core.Type.Ez`      | EZTorQ torque meter: calibration, device modes / EZTorQ 토크 미터: 캘리브레이션, 장치 모드               |

### Utility Classes / 유틸리티 클래스

| Class                 | Description                                                                    |
|-----------------------|--------------------------------------------------------------------------------|
| `KeyedQueue<T, TKey>` | Thread-safe queue with key-based duplicate control / 키 기반 중복 제어를 갖춘 스레드 안전 큐   |
| `RingBuffer`          | Fixed-size circular buffer for streaming data / 스트리밍 데이터용 고정 크기 순환 버퍼          |
| `BinarySpanReader`    | Zero-allocation Big-Endian reader using `BinaryPrimitives` / 할당 없는 빅 엔디언 이진 리더 |
| `DataHash`            | XxHash3-based hash for efficient change detection / 효율적인 변경 감지를 위한 XxHash3 해시  |
| `Utils`               | CRC-16 (MODBUS), endianness conversion, value packing / CRC-16, 엔디언 변환, 값 패킹   |

---

## Usage / 사용법

### KeyedQueue

```csharp
using HTool.Core.Util;

// create a keyed queue with key selector and capacity
// 키 선택기와 용량을 지정하여 키 기반 큐 생성
var queue = KeyedQueue<Message, int>
    .Create(static m => m.Id, capacity: 64);

// enqueue with uniqueness enforcement
// 유일성을 적용하여 큐에 추가
queue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// blocking dequeue
// 블로킹 방식으로 큐에서 꺼내기
var item = queue.Dequeue();
```

### BinarySpanReader

```csharp
using HTool.Core.Util;

// wrap packet bytes as a read-only span
// 패킷 바이트를 읽기 전용 스팬으로 래핑
ReadOnlySpan<byte> data = packet.AsSpan();
// initialize read position
// 읽기 위치 초기화
var pos = 0;

// read UInt16 field (Big-Endian) and advance position
// UInt16 필드 읽기 (빅 엔디언), 위치 자동 전진
var id = BinarySpanReader.ReadUInt16(data, ref pos);
// read Int32 field (Big-Endian) and advance position
// Int32 필드 읽기 (빅 엔디언), 위치 자동 전진
var value = BinarySpanReader.ReadInt32(data, ref pos);
// read fixed-length string (20 bytes) and advance position
// 고정 길이 문자열 읽기 (20바이트), 위치 자동 전진
var name = BinarySpanReader.ReadString(data, ref pos, 20);
```

### ModelNames

```csharp
using HTool.Core.Type.Device;

// code to display name
// 코드로 표시 이름 조회
var name = ModelNames.ToName(42);    // "ECA-D40"

// display name to code
// 표시 이름으로 코드 조회
var code = ModelNames.FromName("ECA-D40"); // 42
```

---

## License / 라이선스

MIT License - Copyright (c) HANTAS
