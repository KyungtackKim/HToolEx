namespace Tester.TestHelpers;

/// <summary>
///     테스트용 사전 구축 바이트 배열 팩토리.
///     Pre-built byte array factory for testing.
/// </summary>
internal static class TestData {
    /// <summary>
    ///     SimpleInfo 13바이트 테스트 데이터를 생성한다.
    ///     creates 13-byte SimpleInfo test data.
    /// </summary>
    /// <returns>13바이트 배열 / 13-byte array</returns>
    internal static byte[] SimpleInfo() {
        // Id=1, Controller=100, Driver=200, Firmware=10, Serial=5바이트
        // Id=1, Controller=100, Driver=200, Firmware=10, Serial=5 bytes
        return new ByteBuilder()
            .UInt16BigEndian(1)
            .UInt16BigEndian(100)
            .UInt16BigEndian(200)
            .UInt16BigEndian(10)
            .Raw(0x01, 0x02, 0x03, 0x04, 0x05)
            .Build();
    }

    /// <summary>
    ///     SimpleInfo 17바이트 (Used 포함) 테스트 데이터를 생성한다.
    ///     creates 17-byte SimpleInfo test data with Used field.
    /// </summary>
    /// <returns>17바이트 배열 / 17-byte array</returns>
    internal static byte[] SimpleInfoWithUsed() {
        // 기본 13바이트 + Used=500 (4바이트)
        // base 13 bytes + Used=500 (4 bytes)
        return new ByteBuilder()
            .UInt16BigEndian(1)
            .UInt16BigEndian(100)
            .UInt16BigEndian(200)
            .UInt16BigEndian(10)
            .Raw(0x01, 0x02, 0x03, 0x04, 0x05)
            .UInt16BigEndian(0)
            .UInt16BigEndian(500)
            .Build();
    }

    /// <summary>
    ///     Info 200바이트 테스트 데이터를 생성한다.
    ///     creates 200-byte Info test data.
    /// </summary>
    /// <returns>200바이트 배열 / 200-byte array</returns>
    internal static byte[] Info() {
        // 200바이트 구조: SystemInfo + DriverId + DriverModelNumber + DriverModelName(32) + ...
        // 200-byte structure: SystemInfo + DriverId + DriverModelNumber + DriverModelName(32) + ...
        return new ByteBuilder()
            .UInt16BigEndian(0)                                                                    // SystemInfo
            .UInt16BigEndian(1)                                                                    // DriverId
            .UInt16BigEndian(15)                                                                   // DriverModelNumber
            .Ascii("MDT-100", 32)                                                           // DriverModelName (32 bytes)
            .Ascii("SN12345678", 10)                                                        // DriverSerialNumber (10 bytes)
            .UInt16BigEndian(15)                                                                   // ControllerModelNumber
            .Ascii("MDT-CTRL", 32)                                                          // ControllerModelName (32 bytes)
            .Ascii("CTRL00001", 10)                                                         // ControllerSerialNumber (10 bytes)
            .UInt16BigEndian(2)                                                                    // FirmwareVersionMajor
            .UInt16BigEndian(1)                                                                    // FirmwareVersionMinor
            .UInt16BigEndian(5)                                                                    // FirmwareVersionPatch
            .UInt32BigEndian(20250101)                                                             // ProductionDate
            .UInt16BigEndian(0)                                                                    // AdvanceType
            .Raw(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF)                                        // MacAddress (6 bytes)
            .UInt16BigEndian(1)                                                                    // EventDataRevision
            .UInt16BigEndian(1)                                                                    // ManufacturerCode
            .Zeros(200 - 2 - 2 - 2 - 32 - 10 - 2 - 32 - 10 - 2 - 2 - 2 - 4 - 2 - 6 - 2 - 2) // 나머지 패딩 / remaining padding
            .Build();
    }

    /// <summary>
    ///     Status 38바이트 테스트 데이터를 생성한다.
    ///     creates 38-byte Status test data.
    /// </summary>
    /// <returns>38바이트 배열 / 38-byte array</returns>
    internal static byte[] Status() {
        // 38바이트 구조: Torque(4) + Speed(2) + Current(4) + Preset(2) + Model(2)
        //   + TorqueUp(2) + FastenOk(2) + Ready(2) + Run(2) + Alarm(2) + Direction(2)
        //   + RemainScrew(2) + Input(2) + Output(2) + Temperature(4) + IsLock(2)
        // 38-byte structure: Torque(4) + Speed(2) + Current(4) + Preset(2) + Model(2)
        //   + TorqueUp(2) + FastenOk(2) + Ready(2) + Run(2) + Alarm(2) + Direction(2)
        //   + RemainScrew(2) + Input(2) + Output(2) + Temperature(4) + IsLock(2)
        return new ByteBuilder()
            .SingleBigEndian(10.5f)  // Torque
            .UInt16BigEndian(300)    // Speed
            .SingleBigEndian(1.2f)   // Current
            .UInt16BigEndian(5)      // Preset
            .UInt16BigEndian(1)      // Model
            .UInt16BigEndian(1)      // TorqueUp (nonzero = true)
            .UInt16BigEndian(0)      // FastenOk (zero = false)
            .UInt16BigEndian(1)      // Ready (nonzero = true)
            .UInt16BigEndian(1)      // Run (nonzero = true)
            .UInt16BigEndian(0)      // Alarm
            .UInt16BigEndian(0)      // Direction (Fastening = 0)
            .UInt16BigEndian(3)      // RemainScrew
            .UInt16BigEndian(0x0003) // Input signals (bit0=1, bit1=1)
            .UInt16BigEndian(0x0001) // Output signals (bit0=1)
            .SingleBigEndian(35.5f)  // Temperature
            .UInt16BigEndian(0)      // IsLock (zero = false)
            .Build();
    }

    /// <summary>
    ///     Barcode 64바이트 테스트 데이터를 생성한다.
    ///     creates 64-byte Barcode test data.
    /// </summary>
    /// <returns>64바이트 배열 / 64-byte array</returns>
    internal static byte[] Barcode() {
        // ASCII 문자열 + 제로 패딩
        // ASCII string + zero padding
        return new ByteBuilder()
            .Ascii("TEST-BARCODE-001", 64)
            .Build();
    }

    /// <summary>
    ///     Preset 60바이트 테스트 데이터를 생성한다.
    ///     creates 60-byte Preset test data.
    /// </summary>
    /// <returns>60바이트 배열 / 60-byte array</returns>
    internal static byte[] Preset() {
        // 60바이트 구조: Mode(2) + Torque(4) + TorqueLimit(4) + 나머지 + 예약(16)
        // 60-byte structure: Mode(2) + Torque(4) + TorqueLimit(4) + rest + reserved(16)
        return new ByteBuilder()
            .UInt16BigEndian(0)     // Mode (TC/AM)
            .SingleBigEndian(15.0f) // Torque
            .SingleBigEndian(20.0f) // TorqueLimit
            .UInt16BigEndian(360)   // TargetAngle
            .UInt16BigEndian(10)    // MinAngle
            .UInt16BigEndian(720)   // MaxAngle
            .SingleBigEndian(5.0f)  // SnugTorque
            .UInt16BigEndian(500)   // Speed
            .UInt16BigEndian(30)    // FreeAngle
            .UInt16BigEndian(200)   // FreeSpeed
            .UInt16BigEndian(50)    // SoftStart
            .UInt16BigEndian(30)    // SeatingPoint
            .UInt16BigEndian(100)   // TorqueRisingTime
            .UInt16BigEndian(300)   // RampUpSpeedLimit
            .UInt16BigEndian(0)     // TorqueCompensation
            .UInt16BigEndian(0)     // TargetTorqueOffset
            .UInt16BigEndian(0)     // MaxPulseCount
            .UInt16BigEndian(0)     // ScrewType (CW)
            .UInt16BigEndian(0)     // SoftStop (disabled)
            .Zeros(16)       // Reserved (registers 40024-40031)
            .Build();
    }
}