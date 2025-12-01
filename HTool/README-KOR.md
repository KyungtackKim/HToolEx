# HTool

HANTAS 산업용 토크 툴 및 컨트롤러를 위한 MODBUS 통신 C# 라이브러리

[![NuGet](https://img.shields.io/badge/nuget-v1.1.22-blue)](https://www.nuget.org/packages/HTool)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue)](https://www.microsoft.com/windows)

> **한국어** | [English](README-ENG.md)

---

## 목차

- [개요](#개요)
- [주요 기능](#주요-기능)
- [시스템 요구사항](#시스템-요구사항)
- [설치](#설치)
- [빠른 시작](#빠른-시작)
- [API 레퍼런스](#api-레퍼런스)
- [사용 예제](#사용-예제)
- [아키텍처](#아키텍처)
- [데이터 포맷](#데이터-포맷)
- [유틸리티 클래스](#유틸리티-클래스)
- [MODBUS 프로토콜](#modbus-프로토콜)
- [에러 처리](#에러-처리)
- [설정 상수](#설정-상수)
- [문제 해결](#문제-해결)
- [성능 최적화](#성능-최적화)
- [버전 히스토리](#버전-히스토리)
- [라이선스](#라이선스)
- [지원](#지원)

---

## 개요

**HTool**은 HANTAS 토크 툴 및 컨트롤러와 MODBUS RTU/TCP 통신을 위한 종합적인 C# 라이브러리입니다. 이벤트 기반 아키텍처와 자동 메시지 큐잉, 레지스터 자동 분할, 스레드 안전 연산 기능을
제공하여 .NET 애플리케이션에 HANTAS 장비를 쉽게 통합할 수 있습니다.

---

## 주요 기능

### 통신

- **듀얼 프로토콜 지원**: MODBUS RTU (시리얼) 및 MODBUS TCP/IP
- **자동 연결 관리**: 연결 상태 추적 및 자동 재연결 지원
- **Keep-Alive 메커니즘**: 연결 상태 자동 모니터링 (3초 주기)

### 메시지 처리

- **스레드 안전 메시지 큐**: `KeyedQueue`를 통한 중복 방지 및 재시도 로직
- **스마트 레지스터 연산**: 대용량 읽기/쓰기 시 자동 청크 분할
    - 최대 읽기: 125 레지스터/요청
    - 최대 쓰기: 123 레지스터/요청

### 데이터 파싱

- **장치 정보 자동 파싱**: 모델, 시리얼, 펌웨어, 세대 정보
- **상태 데이터**: 실시간 토크, 속도, 전류, 알람 등
- **이벤트 데이터**: 체결 결과, 바코드, 그래프 스텝
- **그래프 데이터**: 토크/각도 그래프 수집

### 디버깅

- **Raw 패킷 모니터링**: TX/RX 패킷 실시간 확인
- **에러 이벤트**: 상세한 에러 정보 제공

---

## 시스템 요구사항

| 항목         | 요구사항                    |
|------------|-------------------------|
| **.NET**   | 8.0 이상                  |
| **OS**     | Windows 10.0.17763.0 이상 |
| **RTU 통신** | 사용 가능한 COM 포트           |
| **TCP 통신** | 장치 네트워크 접근              |

### 의존성

| 패키지                                                               | 버전     | 용도                        |
|-------------------------------------------------------------------|--------|---------------------------|
| [SuperSimpleTcp](https://www.nuget.org/packages/SuperSimpleTcp)   | 3.0.20 | TCP 소켓 통신                 |
| [System.IO.Ports](https://www.nuget.org/packages/System.IO.Ports) | 10.0.0 | 시리얼 포트 통신 (RS-232/RS-485) |

---

## 설치

### NuGet (권장)

```bash
dotnet add package HTool
```

또는 Package Manager Console:

```powershell
Install-Package HTool
```

### 소스에서 빌드

```bash
git clone https://github.com/KyungtackKim/HToolEx.git
cd HToolEx/HTool
dotnet build -c Release
```

### NuGet 패키지 생성

```bash
dotnet pack HTool.csproj -c Release
# 출력: bin/Release/HTool.1.1.22.nupkg
```

---

## 빠른 시작

### MODBUS TCP 예제

```csharp
using HTool;
using HTool.Type;
using HTool.Format;

// HTool 인스턴스 생성
var htool = new HTool.HTool(ComTypes.Tcp);

// 연결 이벤트 구독
htool.ChangedConnect += (connected) => {
    if (connected) {
        Console.WriteLine($"연결됨: {htool.Info.Model}");
        Console.WriteLine($"시리얼: {htool.Info.Serial}");
        Console.WriteLine($"펌웨어: {htool.Info.Firmware}");
        Console.WriteLine($"세대: {htool.Gen}");

        // 연결 후 레지스터 읽기
        htool.ReadHoldingReg(addr: 0, count: 10);
    } else {
        Console.WriteLine("연결 해제됨");
    }
};

// 데이터 수신 이벤트 구독
htool.ReceivedData += (code, addr, data) => {
    Console.WriteLine($"수신: Code={code}, Address={addr}, Length={data.Length}");
};

// 에러 이벤트 구독
htool.ReceiveError += (reason, param) => {
    Console.WriteLine($"에러: {reason}");
};

// 장치 연결 (IP주소, 포트, 장치ID)
if (htool.Connect("192.168.1.100", 5000, id: 0x01)) {
    Console.WriteLine("연결 시도 중...");
}

// 프로그램 종료 시
htool.Close();
```

### MODBUS RTU (시리얼) 예제

```csharp
using HTool;
using HTool.Type;

var htool = new HTool.HTool(ComTypes.Rtu);

htool.ChangedConnect += (connected) => {
    if (connected) {
        Console.WriteLine($"연결됨: {htool.Info.Serial}");
    }
};

// COM 포트 연결 (포트명, 보드레이트, 장치ID)
htool.Connect("COM3", 115200, id: 0x01);
```

### 지원 보드레이트

```csharp
// HcRtu.GetBaudRates()로 확인 가능
int[] baudRates = [9600, 19200, 38400, 57600, 115200, 230400];
```

---

## API 레퍼런스

### HTool 클래스

메인 라이브러리 클래스로 모든 통신 기능을 제공합니다.

#### 생성자

```csharp
// 기본 생성자
public HTool()

// 통신 타입 지정 생성자
public HTool(ComTypes type)
```

#### 속성

| 속성                | 타입                | 설명              | 기본값       |
|-------------------|-------------------|-----------------|-----------|
| `Type`            | `ComTypes`        | 통신 타입 (RTU/TCP) | -         |
| `ConnectionState` | `ConnectionTypes` | 현재 연결 상태        | `Close`   |
| `Info`            | `FormatInfo`      | 장치 정보 (연결 후)    | 빈 인스턴스    |
| `Gen`             | `GenerationTypes` | 장치 세대/리비전       | `GenRev2` |
| `EnableKeepAlive` | `bool`            | Keep-Alive 활성화  | `false`   |

#### 정적 속성

| 속성                | 타입    | 값   | 설명           |
|-------------------|-------|-----|--------------|
| `ReadRegMaxSize`  | `int` | 125 | 최대 읽기 레지스터 수 |
| `WriteRegMaxSize` | `int` | 123 | 최대 쓰기 레지스터 수 |

#### 메서드

**통신 타입 설정**

```csharp
/// <summary>
/// 통신 타입 설정 (연결 전에만 가능)
/// </summary>
/// <param name="type">RTU 또는 TCP</param>
public void SetType(ComTypes type)
```

**연결 관리**

```csharp
/// <summary>
/// 장치 연결
/// </summary>
/// <param name="target">TCP: IP주소, RTU: COM포트명</param>
/// <param name="option">TCP: 포트번호, RTU: 보드레이트</param>
/// <param name="id">MODBUS 장치 ID (1-15)</param>
/// <returns>연결 시작 성공 여부</returns>
public bool Connect(string target, int option, byte id = 0x01)

/// <summary>
/// 연결 종료
/// </summary>
public void Close()
```

**레지스터 읽기**

```csharp
/// <summary>
/// Holding 레지스터 읽기 (Function Code 0x03)
/// </summary>
/// <param name="addr">시작 주소</param>
/// <param name="count">레지스터 수</param>
/// <param name="split">분할 크기 (0=자동, 최대 125)</param>
/// <param name="check">중복 체크 여부</param>
public bool ReadHoldingReg(ushort addr, ushort count, int split = 0, bool check = true)

/// <summary>
/// Input 레지스터 읽기 (Function Code 0x04)
/// </summary>
public bool ReadInputReg(ushort addr, ushort count, int split = 0, bool check = true)

/// <summary>
/// 장치 정보 읽기 (Function Code 0x11, HANTAS 전용)
/// </summary>
public bool ReadInfoReg(bool check = true)
```

**레지스터 쓰기**

```csharp
/// <summary>
/// 단일 레지스터 쓰기 (Function Code 0x06)
/// </summary>
public bool WriteSingleReg(ushort addr, ushort value, bool check = true)

/// <summary>
/// 다중 레지스터 쓰기 (Function Code 0x10)
/// </summary>
public bool WriteMultiReg(ushort addr, ushort[] values, bool check = true)
public bool WriteMultiReg(ushort addr, ReadOnlySpan<ushort> values, bool check = true)

/// <summary>
/// 문자열 레지스터 쓰기 (ASCII, 레지스터당 2바이트)
/// </summary>
/// <param name="length">패딩 포함 총 길이 (0=문자열 길이 사용)</param>
public bool WriteStrReg(ushort addr, string str, int length = 0, bool check = true)
```

#### 이벤트

```csharp
/// <summary>
/// 연결 상태 변경 이벤트
/// </summary>
/// <param name="state">true: 연결됨, false: 연결 해제됨</param>
public event PerformChangedConnect? ChangedConnect;

/// <summary>
/// 데이터 수신 이벤트
/// </summary>
/// <param name="codeTypes">MODBUS Function Code</param>
/// <param name="addr">레지스터 주소</param>
/// <param name="data">수신 데이터</param>
public event PerformReceivedData? ReceivedData;

/// <summary>
/// 에러 발생 이벤트
/// </summary>
/// <param name="reason">에러 유형</param>
/// <param name="param">추가 에러 정보</param>
public event ITool.PerformReceiveError? ReceiveError;

/// <summary>
/// Raw 데이터 수신 이벤트 (디버깅용)
/// </summary>
public event PerformRawData? ReceivedRawData;

/// <summary>
/// Raw 데이터 송신 이벤트 (디버깅용)
/// </summary>
public event PerformRawData? TransmitRawData;
```

---

## 사용 예제

### 1. 대용량 레지스터 연산

HTool은 대용량 연산을 자동으로 분할합니다:

```csharp
// 500개 레지스터 읽기 - 자동으로 4개 요청(125개씩)으로 분할
htool.ReadHoldingReg(addr: 0, count: 500);

// 커스텀 분할 크기 지정
htool.ReadHoldingReg(addr: 0, count: 500, split: 100);

// 200개 레지스터 쓰기 - 자동으로 123개씩 분할
ushort[] data = new ushort[200];
// ... 데이터 채우기 ...
htool.WriteMultiReg(addr: 1000, data);
```

### 2. 연결 흐름 및 상태 처리

```csharp
htool.ChangedConnect += (connected) => {
    if (connected) {
        // 연결 완료 및 장치 정보 수신됨
        var info = htool.Info;

        Console.WriteLine($"장치 모델: {info.Model}");
        Console.WriteLine($"시리얼 번호: {info.Serial}");
        Console.WriteLine($"펌웨어 버전: {info.Firmware}");
        Console.WriteLine($"컨트롤러: {info.Controller}");
        Console.WriteLine($"드라이버: {info.Driver}");
        Console.WriteLine($"사용 횟수: {info.Used}");
        Console.WriteLine($"세대: {htool.Gen}");

        // 여기서 작업 시작
    }
};
```

**연결 흐름:**

1. `Connect()` 호출 → `ConnectionState = Connecting`
2. 라이브러리가 자동으로 `ReadInfoReg()` 요청 전송
3. 장치 정보 수신 → `ConnectionState = Connected` → `ChangedConnect(true)` 발생
4. 타임아웃(5초) 또는 에러 → `ConnectionState = Close`

### 3. 데이터 파싱

```csharp
using HTool.Format;
using HTool.Util;

htool.ReceivedData += (code, addr, data) => {
    switch (code) {
        case CodeTypes.ReadHoldingReg:
        case CodeTypes.ReadInputReg:
            // Big-Endian 데이터 파싱
            var span = data.Data.AsSpan();
            var pos = 0;

            ushort reg0 = BinarySpanReader.ReadUInt16(span, ref pos);
            ushort reg1 = BinarySpanReader.ReadUInt16(span, ref pos);
            int reg2_3 = BinarySpanReader.ReadInt32(span, ref pos);  // 2 레지스터 조합
            float reg4_5 = BinarySpanReader.ReadSingle(span, ref pos);  // float 값
            break;

        case CodeTypes.ReadInfoReg:
            // 장치 정보는 htool.Info에 자동 파싱됨
            Console.WriteLine($"Serial: {htool.Info.Serial}");
            break;

        case CodeTypes.Graph:
        case CodeTypes.GraphRes:
            // 그래프 데이터 파싱
            var graph = new FormatGraph(data.Data, htool.Gen);
            Console.WriteLine($"Channel: {graph.Channel}, Points: {graph.Count}");
            foreach (var value in graph.Values) {
                Console.WriteLine($"  {value:F3}");
            }
            break;
    }
};
```

### 4. 상태 데이터 모니터링

```csharp
htool.ReceivedData += (code, addr, data) => {
    if (code == CodeTypes.ReadInputReg && addr == /* 상태 레지스터 주소 */) {
        var status = new FormatStatus(data.Data, htool.Gen);

        Console.WriteLine($"토크: {status.Torque:F2}");
        Console.WriteLine($"속도: {status.Speed} RPM");
        Console.WriteLine($"전류: {status.Current:F2} A");
        Console.WriteLine($"프리셋: {status.Preset}");
        Console.WriteLine($"Torque Up: {status.TorqueUp}");
        Console.WriteLine($"Fasten OK: {status.FastenOk}");
        Console.WriteLine($"Ready: {status.Ready}");
        Console.WriteLine($"Running: {status.Run}");
        Console.WriteLine($"알람: {status.Alarm}");
        Console.WriteLine($"방향: {status.Direction}");
        Console.WriteLine($"남은 스크류: {status.RemainScrew}");
        Console.WriteLine($"온도: {status.Temperature:F1}°C");

        // 입출력 신호 (16비트)
        for (int i = 0; i < 16; i++) {
            if (status.Input[i]) Console.WriteLine($"Input {i}: ON");
            if (status.Output[i]) Console.WriteLine($"Output {i}: ON");
        }
    }
};
```

### 5. 이벤트(체결 결과) 데이터 처리

```csharp
htool.ReceivedData += (code, addr, data) => {
    if (code == CodeTypes.ReadHoldingReg && addr == /* 이벤트 레지스터 주소 */) {
        var evt = new FormatEvent(data.Data, htool.Gen);

        Console.WriteLine($"=== 체결 이벤트 #{evt.Id} ===");
        Console.WriteLine($"일시: {evt.Date:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"결과: {evt.Event}");
        Console.WriteLine($"방향: {evt.Direction}");
        Console.WriteLine($"프리셋: {evt.Preset}");
        Console.WriteLine($"체결 시간: {evt.FastenTime}ms");
        Console.WriteLine($"목표 토크: {evt.TargetTorque:F2}");
        Console.WriteLine($"측정 토크: {evt.Torque:F2}");
        Console.WriteLine($"Seating 토크: {evt.SeatingTorque:F2}");
        Console.WriteLine($"Clamp 토크: {evt.ClampTorque:F2}");
        Console.WriteLine($"Prevailing 토크: {evt.PrevailingTorque:F2}");
        Console.WriteLine($"Snug 토크: {evt.SnugTorque:F2}");
        Console.WriteLine($"속도: {evt.Speed} RPM");
        Console.WriteLine($"각도1: {evt.Angle1}°");
        Console.WriteLine($"각도2: {evt.Angle2}°");
        Console.WriteLine($"총 각도: {evt.Angle}°");
        Console.WriteLine($"Snug 각도: {evt.SnugAngle}°");
        Console.WriteLine($"바코드: {evt.Barcode}");
        Console.WriteLine($"에러 코드: {evt.Error}");

        // 그래프 스텝 정보 (Gen.2)
        if (htool.Gen == GenerationTypes.GenRev2) {
            Console.WriteLine($"채널1 타입: {evt.TypeOfChannel1}");
            Console.WriteLine($"채널1 포인트: {evt.CountOfChannel1}");
            Console.WriteLine($"채널2 타입: {evt.TypeOfChannel2}");
            Console.WriteLine($"채널2 포인트: {evt.CountOfChannel2}");
            Console.WriteLine($"샘플링 레이트: {evt.SamplingRate}");

            foreach (var step in evt.GraphSteps) {
                if (step != null && step.Id != GraphStepTypes.None) {
                    Console.WriteLine($"  Step: {step.Id} at index {step.Index}");
                }
            }
        }
    }
};
```

### 6. Keep-Alive 설정

```csharp
var htool = new HTool.HTool(ComTypes.Tcp);

// Keep-Alive 활성화 (유휴 시 3초마다 Info 요청)
htool.EnableKeepAlive = true;

htool.Connect("192.168.1.100", 5000, 0x01);

// 10초간 응답 없으면 자동 연결 해제
```

### 7. Raw 패킷 모니터링

```csharp
// 송신 패킷 모니터링
htool.TransmitRawData += (packet) => {
    Console.WriteLine($"TX [{packet.Length}]: {BitConverter.ToString(packet)}");
};

// 수신 패킷 모니터링
htool.ReceivedRawData += (packet) => {
    Console.WriteLine($"RX [{packet.Length}]: {BitConverter.ToString(packet)}");
};

// 출력 예시:
// TX [12]: 00-01-00-00-00-06-00-03-00-00-00-0A
// RX [29]: 00-01-00-00-00-17-00-03-14-00-01-00-02-...
```

### 8. 문자열 레지스터 연산

```csharp
// 문자열 쓰기 (레지스터당 2바이트)
htool.WriteStrReg(addr: 200, str: "HANTAS", length: 10);

// 문자열 읽기
htool.ReceivedData += (code, addr, data) => {
    if (code == CodeTypes.ReadHoldingReg && addr == 200) {
        string text = Utils.ToAsciiTrimEnd(data.Data);
        Console.WriteLine($"Device Name: {text}");
    }
};

htool.ReadHoldingReg(addr: 200, count: 5);  // 10바이트 = 5 레지스터
```

### 9. 토크 단위 변환

```csharp
using HTool.Util;
using HTool.Type;

// N.m → kgf.cm 변환
float torqueNm = 10.0f;
float torqueKgfCm = Utils.ConvertTorqueUnit(torqueNm, UnitTypes.Nm, UnitTypes.KgfCm);
Console.WriteLine($"{torqueNm} N.m = {torqueKgfCm:F2} kgf.cm");

// 단위 문자열 파싱
UnitTypes unit = Utils.ParseToUnit("N.m");  // UnitTypes.Nm
string unitStr = Utils.ParseToUnit(2);       // "N.m"
```

---

## 아키텍처

### 프로젝트 구조

```
HTool/
├── HTool.cs                    # 메인 라이브러리 클래스
├── Device/
│   ├── ITool.cs                # 통신 인터페이스 정의
│   ├── Tool.cs                 # 팩토리 클래스
│   ├── HcRtu.cs                # MODBUS RTU 구현
│   └── HcTcp.cs                # MODBUS TCP 구현
├── Data/
│   ├── IReceivedData.cs        # 수신 데이터 인터페이스
│   ├── HcRtuData.cs            # RTU 데이터 컨테이너
│   └── HcTcpData.cs            # TCP 데이터 컨테이너
├── Format/
│   ├── FormatInfo.cs           # 장치 정보 (13+ 바이트)
│   ├── FormatStatus.cs         # 상태 데이터 (세대별 가변)
│   ├── FormatEvent.cs          # 이벤트 데이터 (세대별 가변)
│   ├── FormatGraph.cs          # 그래프 데이터
│   └── FormatMessage.cs        # 내부 메시지 큐 아이템
├── Type/
│   ├── CodeTypes.cs            # MODBUS Function Code
│   ├── ComTypes.cs             # 통신 타입/에러 타입
│   ├── ConnectionTypes.cs      # 연결 상태
│   ├── ModelTypes.cs           # 장치 모델
│   ├── GenerationTypes.cs      # 펌웨어 세대
│   ├── UnitTypes.cs            # 토크 단위
│   ├── EventTypes.cs           # 이벤트 상태
│   ├── DirectionTypes.cs       # 체결 방향
│   ├── GraphTypes.cs           # 그래프 채널 타입
│   ├── GraphStepTypes.cs       # 그래프 스텝 타입
│   ├── GraphDirectionTypes.cs  # 그래프 방향 옵션
│   ├── OptionTypes.cs          # 그래프 옵션
│   ├── SampleTypes.cs          # 샘플링 타입
│   └── WordOrderTypes.cs       # 32비트 워드 순서
└── Util/
    ├── Constants.cs            # 상수 정의
    ├── KeyedQueue.cs           # 스레드 안전 중복 방지 큐
    ├── RingBuffer.cs           # 순환 버퍼 (패킷 파싱용)
    ├── BinarySpanReader.cs     # Big-Endian 읽기 유틸리티
    └── Utils.cs                # 일반 유틸리티 메서드
```

### 핵심 디자인 패턴

#### 1. 팩토리 패턴

```csharp
// Tool.Create()로 통신 객체 생성
ITool tool = Tool.Create(ComTypes.Tcp);  // HcTcp 인스턴스 반환
ITool tool = Tool.Create(ComTypes.Rtu);  // HcRtu 인스턴스 반환
```

#### 2. 이벤트 기반 통신

```csharp
// 모든 통신은 이벤트를 통해 비동기 처리
htool.ChangedConnect += OnConnectionChanged;
htool.ReceivedData += OnDataReceived;
htool.ReceiveError += OnError;
htool.ReceivedRawData += OnRawDataReceived;
htool.TransmitRawData += OnRawDataTransmitted;
```

#### 3. 스레드 안전 메시지 큐

```csharp
// KeyedQueue로 중복 메시지 방지
private KeyedQueue<FormatMessage, FormatMessage.MessageKey> MessageQue =
    KeyedQueue<FormatMessage, FormatMessage.MessageKey>
        .Create(m => m.Key, capacity: 64);

// 중복 방지 모드
MessageQue.TryEnqueue(msg, EnqueueMode.EnforceUnique);

// 중복 허용 모드
MessageQue.TryEnqueue(msg, EnqueueMode.AllowDuplicate);
```

#### 4. 타이머 기반 메시지 처리

```csharp
// 20ms 주기로 메시지 큐 처리
// - 메시지 송신
// - 타임아웃 체크
// - 재시도 로직
// - Keep-Alive 처리
```

---

## 데이터 포맷

### FormatSimpleInfo - 장치 정보 (레거시 프로토콜)

> **참고**: 이 클래스는 Gen.1/1+ 레거시 프로토콜용입니다.

| 필드         | 타입         | 크기 | 설명                |
|------------|------------|----|-------------------|
| Id         | int        | 2  | 장치 ID             |
| Controller | int        | 2  | 컨트롤러 모델 번호        |
| Driver     | int        | 2  | 드라이버 모델 번호        |
| Firmware   | int        | 2  | 펌웨어 버전            |
| Serial     | string     | 5  | 시리얼 번호 (10자리 문자열) |
| Used       | uint       | 4  | 사용 횟수             |
| Model      | ModelTypes | -  | 모델 타입 (시리얼에서 추출)  |

### FormatInfo - 장치 정보 (Gen.2 Modbus 표준 프로토콜)

> **참고**: 이 클래스는 Gen.2 장치 전용입니다. 다른 세대 장치에서는 사용하지 마세요.

| 필드                     | 타입     | 오프셋 | 크기 | 설명                         |
|------------------------|--------|-----|----|----------------------------|
| SystemInfo             | int    | 0   | 2  | 시스템 정보 (예약)                |
| DriverId               | int    | 2   | 2  | 드라이버 ID (1-15)             |
| DriverModelNumber      | int    | 4   | 2  | 드라이버 모델 번호                 |
| DriverModelName        | string | 6   | 32 | 드라이버 모델명 (ASCII)           |
| DriverSerialNumber     | string | 38  | 10 | 드라이버 시리얼 번호                |
| ControllerModelNumber  | int    | 48  | 2  | 컨트롤러 모델 번호                 |
| ControllerModelName    | string | 50  | 32 | 컨트롤러 모델명 (ASCII)           |
| ControllerSerialNumber | string | 82  | 10 | 컨트롤러 시리얼 번호                |
| FirmwareVersionMajor   | int    | 92  | 2  | 펌웨어 버전 Major               |
| FirmwareVersionMinor   | int    | 94  | 2  | 펌웨어 버전 Minor               |
| FirmwareVersionPatch   | int    | 96  | 2  | 펌웨어 버전 Patch               |
| FirmwareVersion        | string | -   | -  | 펌웨어 버전 문자열 (계산됨)           |
| ProductionDate         | uint   | 98  | 4  | 생산일 (YYYYMMDD)             |
| AdvanceType            | int    | 102 | 2  | 어드밴스 타입 (0=Normal, 1=Plus) |
| MacAddress             | byte[] | 104 | 6  | MAC 주소                     |
| MacAddressString       | string | -   | -  | MAC 주소 문자열 (계산됨)           |
| EventDataRevision      | int    | 110 | 2  | 이벤트 데이터 리비전                |
| Manufacturer           | int    | 112 | 2  | 제조사 (1=Hantas, 2=Mountz)   |
| Reserved               | -      | 114 | 86 | 예약 영역                      |

**총 크기**: 200 bytes (100 레지스터)

### FormatStatus - 상태 데이터

| 필드          | Gen.1/1+       | Gen.2          | 설명          |
|-------------|----------------|----------------|-------------|
| Torque      | ushort         | float          | 현재 토크       |
| Speed       | ushort         | ushort         | 현재 속도 (RPM) |
| Current     | ushort         | float          | 현재 전류       |
| Preset      | ushort         | ushort         | 선택된 프리셋     |
| Model       | -              | ushort         | 선택된 모델      |
| TorqueUp    | bool           | bool           | 토크업 상태      |
| FastenOk    | bool           | bool           | 체결 OK 상태    |
| Ready       | bool           | bool           | 준비 상태       |
| Run         | bool           | bool           | 동작 상태       |
| Alarm       | ushort         | ushort         | 알람 코드       |
| Direction   | DirectionTypes | DirectionTypes | 체결 방향       |
| RemainScrew | ushort         | ushort         | 남은 스크류 수    |
| Input       | bool[16]       | bool[16]       | 입력 신호       |
| Output      | bool[16]       | bool[16]       | 출력 신호       |
| Temperature | ushort         | float          | 온도          |
| IsLock      | -              | bool           | 잠금 상태       |

### FormatEvent - 이벤트 데이터

**Gen.1/1+ 공통 필드:**

| 필드                    | 설명          |
|-----------------------|-------------|
| Id                    | 이벤트 ID      |
| FastenTime            | 체결 시간 (ms)  |
| Preset                | 프리셋 번호      |
| TargetTorque          | 목표 토크       |
| Torque                | 측정 토크       |
| Speed                 | 속도          |
| Angle1, Angle2, Angle | 각도 값        |
| RemainScrew           | 남은 스크류      |
| Error                 | 에러 코드       |
| Direction             | 체결 방향       |
| Event                 | 이벤트 상태      |
| SnugAngle             | 스너그 각도      |
| Barcode               | 바코드 (64바이트) |

**Gen.1+ 추가 필드:**

| 필드               | 설명       |
|------------------|----------|
| SeatingTorque    | 시팅 토크    |
| ClampTorque      | 클램프 토크   |
| PrevailingTorque | 프리베일링 토크 |
| SnugTorque       | 스너그 토크   |

**Gen.2 추가 필드:**

| 필드                | 설명            |
|-------------------|---------------|
| Revision          | 이벤트 포맷 리비전    |
| Date/Time         | 체결 일시 (ms 포함) |
| Unit              | 토크 단위         |
| TypeOfChannel1/2  | 그래프 채널 타입     |
| CountOfChannel1/2 | 그래프 포인트 수     |
| SamplingRate      | 샘플링 레이트       |
| GraphSteps[16]    | 그래프 스텝 정보     |

### FormatGraph - 그래프 데이터

| 필드       | 타입              | 설명        |
|----------|-----------------|-----------|
| Type     | GenerationTypes | 세대 타입     |
| Channel  | int             | 채널 번호     |
| Count    | int             | 데이터 포인트 수 |
| Values   | float[]         | 그래프 값 배열  |
| CheckSum | int             | 체크섬       |

---

## 유틸리티 클래스

### BinarySpanReader

Big-Endian 바이너리 데이터 읽기를 위한 정적 유틸리티:

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
string str = BinarySpanReader.ReadAsciiString(span, ref pos, 32);  // ASCII 문자열 (null/공백 trim)

// 위치 없이 읽기 (첫 바이트부터)
ushort value = BinarySpanReader.ReadUInt16(span);
```

### KeyedQueue<T, TKey>

스레드 안전하고 중복을 방지하는 고성능 큐:

```csharp
// 생성
var queue = KeyedQueue<MyMessage, string>
    .Create(m => m.Id, capacity: 100);

// 단일 항목 추가
queue.TryEnqueue(msg, EnqueueMode.EnforceUnique);  // 중복 방지
queue.TryEnqueue(msg, EnqueueMode.AllowDuplicate); // 중복 허용

// 배치 추가
var result = queue.TryEnqueueRange(messages, EnqueueMode.EnforceUnique);
Console.WriteLine($"추가: {result.Accepted}, 스킵: {result.Skipped}");

// 제거
if (queue.TryDequeue(out var item)) { /* 처리 */ }

// 블로킹 제거 (타임아웃)
if (queue.TryDequeue(out var item, timeoutMs: 1000)) { /* 처리 */ }

// Peek
if (queue.TryPeek(out var item)) { /* 확인만 */ }

// 키 기반 연산
bool exists = queue.ContainsKey(key);
int pending = queue.PendingCountByKey(key);
queue.TryRemoveByKey(key);      // 첫 번째 제거
queue.RemoveAllByKey(key);      // 모두 제거

// 상태 확인
int count = queue.Count;
bool empty = queue.IsEmpty;
int uniqueKeys = queue.UniqueKeyCount;

// 스냅샷
List<MyMessage> snapshot = queue.Snapshot();
IReadOnlyDictionary<string, int> keySnapshot = queue.GetKeySnapshot();

// 정리
queue.Clear();
queue.TrimExcess();
queue.Dispose();
```

### RingBuffer

패킷 파싱을 위한 순환 버퍼:

```csharp
// 생성 (용량은 2의 제곱으로 조정됨)
var buffer = new RingBuffer(16 * 1024);

// 쓰기
buffer.Write(singleByte);
buffer.WriteBytes(byteArray);
buffer.WriteBytes(readOnlySpan);

// Peek (제거 없이 읽기)
byte b = buffer.Peek(offset);
ReadOnlySpan<byte> data = buffer.PeekBytes();

// 읽기 (제거하며 읽기)
byte[] packet = buffer.ReadBytes(length);

// 제거 (읽지 않고 제거)
buffer.RemoveBytes(length);

// 상태
int capacity = buffer.Capacity;
int available = buffer.Available;

// 초기화
buffer.Clear();
```

### Utils

다양한 유틸리티 메서드:

```csharp
// CRC 계산 (MODBUS RTU)
var (low, high) = Utils.CalculateCrc(packet);
Utils.CalculateCrcTo(packet, crcBuffer);
bool valid = Utils.ValidateCrc(packetWithCrc);

// 체크섬
int sum = Utils.CalculateCheckSum(packet);

// 토크 단위 변환
float converted = Utils.ConvertTorqueUnit(10.0f, UnitTypes.Nm, UnitTypes.KgfCm);
UnitTypes unit = Utils.ParseToUnit("N.m");
string unitStr = Utils.ParseToUnit(2);

// 바이트 → 값 변환
Utils.ConvertValue(bytes, out float floatValue, WordOrderTypes.HighLow);
Utils.ConvertValue(bytes, out ushort ushortValue);
Utils.ConvertValue(bytes, out int intValue, WordOrderTypes.LowHigh);

// 값 → ushort[] 변환
ushort[] words = Utils.GetValuesFromValue(intValue, WordOrderTypes.HighLow);
ushort[] words = Utils.GetValuesFromValue(floatValue);

// 문자열 변환
ushort[] words = Utils.GetWordValuesFromText("HANTAS");
string text = Utils.ToAsciiTrimEnd(byteSpan);

// 시간 유틸리티
long ticks = Utils.GetCurrentTicks();
long ms = Utils.TimeLapsMs(startTime);
long sec = Utils.TimeLapsSec(startTime);

// 리스트 스왑
Utils.Swap(list, sourceIndex, destIndex);
```

---

## MODBUS 프로토콜

### Function Code

| 코드               | 값    | 설명              | 타입     |
|------------------|------|-----------------|--------|
| `ReadHoldingReg` | 0x03 | Holding 레지스터 읽기 | 표준     |
| `ReadInputReg`   | 0x04 | Input 레지스터 읽기   | 표준     |
| `WriteSingleReg` | 0x06 | 단일 레지스터 쓰기      | 표준     |
| `WriteMultiReg`  | 0x10 | 다중 레지스터 쓰기      | 표준     |
| `ReadInfoReg`    | 0x11 | 장치 정보 읽기        | HANTAS |
| `Graph`          | 0x64 | 그래프 데이터         | HANTAS |
| `GraphRes`       | 0x65 | 그래프 결과          | HANTAS |
| `HighResGraph`   | 0x66 | 고해상도 그래프        | HANTAS |
| `Error`          | 0x80 | 에러 응답           | 표준     |

### RTU 프레임 구조

```
[Device ID (1)] [Function Code (1)] [Data (N)] [CRC Low (1)] [CRC High (1)]
```

### TCP 프레임 구조 (MBAP Header)

```
[Transaction ID (2)] [Protocol ID (2)] [Length (2)] [Unit ID (1)] [Function Code (1)] [Data (N)]
```

---

## 에러 처리

### 에러 코드

| 에러                  | 값    | 설명                    |
|---------------------|------|-----------------------|
| `InvalidFunction`   | 0x01 | 지원하지 않는 Function Code |
| `InvalidAddress`    | 0x02 | 유효하지 않은 레지스터 주소       |
| `InvalidValue`      | 0x03 | 유효하지 않은 데이터 값         |
| `InvalidCrc`        | 0x07 | CRC 체크섬 오류 (RTU)      |
| `InvalidFrame`      | 0x0C | 잘못된 프레임 형식            |
| `InvalidValueRange` | 0x0E | 값 범위 초과               |
| `Timeout`           | 0x0F | 요청 타임아웃               |

### 에러 처리 예제

```csharp
htool.ReceiveError += (reason, param) => {
    switch (reason) {
        case ComErrorTypes.Timeout:
            Console.WriteLine($"타임아웃 - 버퍼 크기: {param}");
            // 재시도 로직 또는 연결 재설정
            break;

        case ComErrorTypes.InvalidCrc:
            Console.WriteLine("CRC 에러 - 케이블/보드레이트 확인 필요");
            break;

        case ComErrorTypes.InvalidFunction:
            Console.WriteLine("지원하지 않는 Function Code");
            break;

        case ComErrorTypes.InvalidAddress:
            Console.WriteLine("유효하지 않은 레지스터 주소");
            break;

        case ComErrorTypes.InvalidValue:
            Console.WriteLine("유효하지 않은 데이터 값");
            break;

        case ComErrorTypes.InvalidFrame:
            Console.WriteLine("프레임 형식 오류");
            break;

        case ComErrorTypes.InvalidValueRange:
            Console.WriteLine("값 범위 초과");
            break;
    }
};
```

---

## 설정 상수

`HTool.Util.Constants` 클래스에 정의된 상수들:

| 상수                 | 값                                           | 설명               |
|--------------------|---------------------------------------------|------------------|
| `BarcodeLength`    | 64                                          | 바코드 필드 길이        |
| `BaudRates`        | [9600, 19200, 38400, 57600, 115200, 230400] | 지원 보드레이트         |
| `ProcessPeriod`    | 20ms                                        | 메시지 처리 주기        |
| `ProcessLockTime`  | 2ms                                         | 처리 락 타임아웃        |
| `ProcessTimeout`   | 500ms                                       | 처리 타임아웃          |
| `ConnectTimeout`   | 5000ms                                      | 연결 타임아웃          |
| `MessageTimeout`   | 1000ms                                      | 메시지 타임아웃         |
| `KeepAlivePeriod`  | 3000ms                                      | Keep-Alive 요청 주기 |
| `KeepAliveTimeout` | 10s                                         | Keep-Alive 타임아웃  |

**HTool 클래스 상수:**

| 상수                | 값   | 설명             |
|-------------------|-----|----------------|
| `ReadRegMaxSize`  | 125 | 요청당 최대 읽기 레지스터 |
| `WriteRegMaxSize` | 123 | 요청당 최대 쓰기 레지스터 |

---

## 문제 해결

### TCP 연결 실패

```csharp
// 체크리스트:
// 1. IP 주소 및 포트 확인 (기본 HANTAS: 5000)
// 2. 방화벽 설정 확인
// 3. 같은 네트워크/VLAN인지 확인
// 4. ping 테스트

if (!htool.Connect("192.168.1.100", 5000, 0x01)) {
    Console.WriteLine("연결 시작 실패");
    // IP, 포트, 네트워크 설정 확인
}
```

### RTU 연결 실패

```csharp
// 사용 가능한 COM 포트 확인
foreach (var port in HcRtu.GetPortNames()) {
    Console.WriteLine($"Available: {port}");
}

// 체크리스트:
// 1. COM 포트명 확인 (장치 관리자)
// 2. 보드레이트 확인 (일반적: 115200, 9600)
// 3. 다른 프로그램이 포트 사용 중인지 확인
// 4. 케이블 연결 상태 확인
```

### CRC 에러 (RTU)

```csharp
htool.ReceiveError += (reason, param) => {
    if (reason == ComErrorTypes.InvalidCrc) {
        // 일반적인 원인:
        // - 보드레이트 불일치
        // - 케이블 품질 문제
        // - 전자기 간섭
        // - Parity/Stop bits 설정 불일치
        Console.WriteLine("CRC 에러 발생");
    }
};
```

### 타임아웃 에러

```csharp
htool.ReceiveError += (reason, param) => {
    if (reason == ComErrorTypes.Timeout) {
        Console.WriteLine($"타임아웃 - 클리어된 버퍼: {param} bytes");
        // 원인:
        // - 장치 응답 지연
        // - 잘못된 레지스터 주소
        // - 네트워크 지연
    }
};

// 재시도 로직 구현 예제
int retryCount = 3;
bool SendWithRetry(ushort addr, ushort count) {
    for (int i = 0; i < retryCount; i++) {
        if (htool.ReadHoldingReg(addr, count, check: false)) {
            // 응답 대기...
            return true;
        }
        Thread.Sleep(100);
    }
    return false;
}
```

### 데이터 미수신

1. 연결 상태 확인:

```csharp
if (htool.ConnectionState != ConnectionTypes.Connected) {
    Console.WriteLine("연결되지 않음");
    return;
}
```

2. Raw 패킷 모니터링으로 트래픽 확인:

```csharp
htool.TransmitRawData += (p) => Console.WriteLine($"TX: {BitConverter.ToString(p)}");
htool.ReceivedRawData += (p) => Console.WriteLine($"RX: {BitConverter.ToString(p)}");
```

3. 레지스터 주소 확인 (장치 매뉴얼 참조)

---

## 성능 최적화

### 메시지 큐 최적화

```csharp
// 큐 용량: 64 메시지
// 고빈도 요청 시 중복 체크 비활성화
htool.ReadHoldingReg(addr, count, check: false);
```

### 최적 분할 크기

```csharp
// 안정적인 네트워크: 큰 분할 (오버헤드 감소)
htool.ReadHoldingReg(addr: 0, count: 1000, split: 125);

// 불안정한 연결: 작은 분할 (타임아웃 위험 감소)
htool.ReadHoldingReg(addr: 0, count: 1000, split: 50);
```

### Keep-Alive 사용

```csharp
// 장시간 연결에만 활성화 (3초 주기 트래픽 발생)
htool.EnableKeepAlive = true;   // 모니터링 앱용

htool.EnableKeepAlive = false;  // 빠른 작업용
```

### Span 기반 데이터 처리

```csharp
// 메모리 할당 최소화
htool.ReceivedData += (code, addr, data) => {
    var span = data.Data.AsSpan();
    var pos = 0;

    // BinarySpanReader 사용으로 스트림 객체 생성 없이 파싱
    while (pos < span.Length - 2) {
        ushort value = BinarySpanReader.ReadUInt16(span, ref pos);
        // 처리...
    }
};
```

---

## 버전 히스토리

### 1.1.22 - Current

- FormatInfo 클래스 리팩토링
    - 기존 FormatInfo → FormatSimpleInfo (레거시 프로토콜)
    - 새 FormatInfo (Gen.2 Modbus 표준 프로토콜, 200 bytes)
- BinarySpanReader.ReadAsciiString 메서드 추가

### 1.1.21

- BinarySpanReader 추가 (BinaryReaderBigEndian 대체)
- XML 주석 개선 및 오류 수정
- 성능 최적화

### 1.1.20

- KeyedQueue 개선
- RingBuffer 최적화
- Constants 정리

### 1.0.2 - 2025.01.27

- 마이너 버그 수정

### 1.0.1 - 2024.12.09

- 마이너 버그 수정

### 1.0.0 - 2024.09.13

- 최초 릴리즈

---

## 라이선스

이 프로젝트는 MIT 라이선스로 배포됩니다.

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

## 지원

- **회사**: HANTAS
- **작성자**: Eloiz
- **GitHub
  **: [https://github.com/KyungtackKim/HToolEx/tree/master/HTool](https://github.com/KyungtackKim/HToolEx/tree/master/HTool)
- **이슈**: [GitHub Issues](https://github.com/KyungtackKim/HToolEx/issues)

---

**HTool** - HANTAS 토크 툴을 위한 전문 MODBUS 통신 라이브러리
