using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Param;

/// <summary>
///     툴 제어 파라미터 데이터 (200바이트). 함수 코드 0x03으로 레지스터 43651~43750 전체를 읽어 파싱한다.
///     tool control parameter data (200 bytes). parsed from the full register range 43651–43750 via function code 0x03.
/// </summary>
public readonly struct Control {
	/// <summary>
	///     제어 파라미터 데이터 크기 (바이트, 레지스터 43651~43750)
	///     control parameter data size (bytes, registers 43651–43750)
	/// </summary>
	public static int Size => 200;

	/// <summary>
	///     원시 패킷 데이터에서 제어 파라미터를 파싱합니다.
	///     parses control parameters from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Control(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data meets minimum size requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // ── Driver 그룹 (43651–43682) ──────────────────────────────────────────
        // ── Driver group (43651–43682) ─────────────────────────────────────────

        // 드라이버 ID 읽기 (1–15)
        // read driver ID (1–15)
        DriverId = BinarySpanReader.ReadUInt16(data, ref pos);
        // 드라이버 모델 읽기 (list)
        // read driver model (list)
        DriverModel = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (10바이트, 43653~43657)
        // skip reserved area (10 bytes, 43653–43657)
        pos += 10;
        // 토크 단위 읽기 (list)
        // read torque unit (list)
        TorqueUnit = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (4바이트, 43659~43660)
        // skip reserved area (4 bytes, 43659–43660)
        pos += 4;
        // 토크 캘리브레이션 읽기 [%]
        // read torque calibration [%]
        TorqueCalibration = BinarySpanReader.ReadUInt16(data, ref pos);
        // 모터 가속 읽기 [ms]
        // read motor acceleration [ms]
        MotorAcceleration = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크 홀딩 시간 읽기 [ms]
        // read torque holding time [ms]
        TorqueHoldingTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 풀림 속도 읽기 [rpm]
        // read loosening speed [rpm]
        LoosenSpeed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 체결 판정 최소 회전수 읽기 [turn]
        // read judged fastening minimum turns [turn]
        JudgedFasteningMinTurns = BinarySpanReader.ReadSingle(data, ref pos);
        // 자유 회전 최대 토크 읽기 [unit]
        // read free speed maximum torque [unit]
        FreeSpeedMaxTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 정방향 운전 시간 한계 읽기 [sec]
        // read forward run time limit [sec]
        ForwardRunTimeLimit = BinarySpanReader.ReadSingle(data, ref pos);
        // 역방향 운전 시간 한계 읽기 [sec]
        // read reverse run time limit [sec]
        ReverseRunTimeLimit = BinarySpanReader.ReadSingle(data, ref pos);
        // 모터 정지 시간 한계 읽기 [sec]
        // read motor stall time limit [sec]
        MotorStallTimeLimit = BinarySpanReader.ReadSingle(data, ref pos);
        // 홀딩 시간 각도 한계 읽기 [degree]
        // read holding time angle limit [degree]
        HoldingTimeAngleLimit = BinarySpanReader.ReadUInt16(data, ref pos);
        // 자동 속도 읽기 (0=사용 안 함, 1=사용)
        // read auto speed (0=disabled, 1=enabled)
        AutoSpeed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 풀림 시 최대 토크 사용 읽기 (0=사용 안 함, 1=사용)
        // read use max torque for loosening (0=disabled, 1=enabled)
        UseMaxTorqueForLoosen = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (10바이트, 43678~43682)
        // skip reserved area (10 bytes, 43678–43682)
        pos += 10;

        // ── Operation 그룹 (43683–43695) ───────────────────────────────────────
        // ── Operation group (43683–43695) ──────────────────────────────────────

        // 이벤트 데이터 선택 읽기 (list)
        // read event data select (list)
        EventDataSelect = BinarySpanReader.ReadUInt16(data, ref pos);
        // 체결 OK 신호 시간 읽기 [ms]
        // read fastening OK signal time [ms]
        FasteningOkSignalTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 체결 정지 오류 읽기 (0=사용 안 함, 1=사용)
        // read fastening stop error (0=disabled, 1=enabled)
        FasteningStopError = BinarySpanReader.ReadUInt16(data, ref pos);
        // 드라이버 자동 잠금 읽기 (0=사용 안 함, 1=사용)
        // read driver auto lock (0=disabled, 1=enabled)
        DriverAutoLock = BinarySpanReader.ReadUInt16(data, ref pos);
        // 트리거 시작 읽기 (핸드헬드 전용, 0=사용 안 함, 1=사용)
        // read trigger start (handheld only, 0=disabled, 1=enabled)
        TriggerStart = BinarySpanReader.ReadUInt16(data, ref pos);
        // 역방향 시작 읽기 (핸드헬드 전용, 0=사용 안 함, 1=사용)
        // read reverse start (handheld only, 0=disabled, 1=enabled)
        ReverseStart = BinarySpanReader.ReadUInt16(data, ref pos);
        // 역방향 제어 읽기 (0=사용 안 함, 1=사용)
        // read reverse control (0=disabled, 1=enabled)
        ReverseControl = BinarySpanReader.ReadUInt16(data, ref pos);
        // 드라이버 온도 한계 읽기 [°C]
        // read driver temperature limit [°C]
        DriverTemperatureLimit = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (10바이트, 43691~43695)
        // skip reserved area (10 bytes, 43691–43695)
        pos += 10;

        // ── Preset/Model 그룹 (43695–43705) ────────────────────────────────────
        // ── Preset/Model group (43695–43705) ───────────────────────────────────

        // 전원 투입 시 초기 프리셋 번호 읽기 (1–17)
        // read initial preset number when power on (1–17)
        InitialPresetOnPowerOn = BinarySpanReader.ReadUInt16(data, ref pos);
        // 패널에서 선택 읽기 (0=사용 안 함, 1=사용)
        // read selection on panel (0=disabled, 1=enabled)
        SelectionOnPanel = BinarySpanReader.ReadUInt16(data, ref pos);
        // 모델 선택 모드 읽기 (0=사용 안 함, 1=사용)
        // read model selection mode (0=disabled, 1=enabled)
        ModelSelectionMode = BinarySpanReader.ReadUInt16(data, ref pos);
        // LCD에서 프리셋/모델 변경 읽기 (0=사용 안 함, 1=사용)
        // read preset/model change by LCD (0=disabled, 1=enabled)
        PresetModelChangeByLcd = BinarySpanReader.ReadUInt16(data, ref pos);
        // 바코드로 프리셋/모델 시작 읽기 (0=사용 안 함, 1=사용)
        // read preset/model start by barcode (0=disabled, 1=enabled)
        PresetModelStartByBarcode = BinarySpanReader.ReadUInt16(data, ref pos);
        // 모델 자동 재시작 읽기 (0=사용 안 함, 1=사용)
        // read model auto restart (0=disabled, 1=enabled)
        ModelAutoRestart = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (10바이트, 43701~43705)
        // skip reserved area (10 bytes, 43701–43705)
        pos += 10;

        // ── Controller 그룹 (43706–43715) ──────────────────────────────────────
        // ── Controller group (43706–43715) ─────────────────────────────────────

        // 비밀번호 읽기 (0–9999)
        // read password (0–9999)
        Password = BinarySpanReader.ReadUInt16(data, ref pos);
        // 비프 알람 읽기 (0=사용 안 함, 1=사용)
        // read beep alarm (0=disabled, 1=enabled)
        BeepAlarm = BinarySpanReader.ReadUInt16(data, ref pos);
        // 오류 표시 초기화 시간 읽기 [sec]
        // read error display reset time [sec]
        ErrorDisplayResetTime = BinarySpanReader.ReadSingle(data, ref pos);
        // 라이트 켜짐 시간 읽기 [sec]
        // read light on time [sec]
        LightOnTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (10바이트, 43711~43715)
        // skip reserved area (10 bytes, 43711–43715)
        pos += 10;

        // ── Communication 그룹 (43716–43725) ───────────────────────────────────
        // ── Communication group (43716–43725) ──────────────────────────────────

        // 프로토콜 읽기 (0, 1)
        // read protocol (0, 1)
        Protocol = BinarySpanReader.ReadUInt16(data, ref pos);
        // RS232 포트 선택 읽기 (0, 1)
        // read RS232 port select (0, 1)
        Rs232PortSelect = BinarySpanReader.ReadUInt16(data, ref pos);
        // COM 포트 보드레이트 읽기 (list)
        // read COM port baudrate (list)
        ComPortBaudrate = BinarySpanReader.ReadUInt16(data, ref pos);
        // 자동 데이터 출력 읽기 (0=사용 안 함, 1=사용)
        // read auto data output (0=disabled, 1=enabled)
        AutoDataOutput = BinarySpanReader.ReadUInt16(data, ref pos);
        // 자동 데이터 출력 포트 읽기 (0, 1)
        // read auto data output port (0, 1)
        AutoDataOutputPort = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (10바이트, 43721~43725)
        // skip reserved area (10 bytes, 43721–43725)
        pos += 10;

        // ── Crowfoot 그룹 (43726–43737) ────────────────────────────────────────
        // ── Crowfoot group (43726–43737) ───────────────────────────────────────

        // 크로우풋 활성화 읽기 (0=사용 안 함, 1=사용)
        // read crowfoot enable (0=disabled, 1=enabled)
        CrowfootEnable = BinarySpanReader.ReadUInt16(data, ref pos);
        // 크로우풋 비율 읽기 [%]
        // read crowfoot ratio [%]
        CrowfootRatio = BinarySpanReader.ReadSingle(data, ref pos);
        // 크로우풋 효율 읽기 [%]
        // read crowfoot efficiency [%]
        CrowfootEfficiency = BinarySpanReader.ReadUInt16(data, ref pos);
        // 크로우풋 역방향 토크 읽기 [unit]
        // read crowfoot reverse torque [unit]
        CrowfootReverseTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 크로우풋 역방향 속도 읽기 [rpm]
        // read crowfoot reverse speed [rpm]
        CrowfootReverseSpeed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 크로우풋 최대 토크 읽기 [unit]
        // read crowfoot maximum torque [unit]
        CrowfootMaxTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 예약 영역 건너뛰기 (6바이트, 43735~43737)
        // skip reserved area (6 bytes, 43735–43737)
        pos += 6;

        // ── Wireless Tools 그룹 (43738–43750) ──────────────────────────────────
        // ── Wireless Tools group (43738–43750) ─────────────────────────────────

        // 슬립 시간 읽기 [min]
        // read sleep time [min]
        SleepTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // LCD 버튼 잠금 읽기 (0=사용 안 함, 1=사용)
        // read LCD button lock (0=disabled, 1=enabled)
        LcdButtonLock = BinarySpanReader.ReadUInt16(data, ref pos);
        // 트리거 시작 지연 시간 읽기 [ms]
        // read trigger start delay time [ms]
        TriggerStartDelayTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 표시할 프리셋 번호 선택 읽기 (BIT 플래그, BIT.0=프리셋1, BIT.1=프리셋2, ...)
        // read select display preset number (BIT flags, BIT.0=preset.1, BIT.1=preset.2, ...)
        SelectDisplayPresetNumber = BinarySpanReader.ReadUInt32(data, ref pos);
        // Wifi 연결 끊김 시 드라이버 잠금 읽기 (0=사용 안 함, 1=사용)
        // read driver lock after wifi disconnect (0=disabled, 1=enabled)
        DriverLockAfterWifiDisconnect = BinarySpanReader.ReadUInt16(data, ref pos);
        // 풀림/체결 전환 타입 읽기 (0, 1)
        // read loosening/fastening switch type (0, 1)
        LoosenFastenSwitchType = BinarySpanReader.ReadUInt16(data, ref pos);
        // 예약 영역 건너뛰기 (12바이트, 43745~43750)
        // skip reserved area (12 bytes, 43745–43750)
        pos += 12;

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..pos]);
    }

	/// <summary>
	///     원시 데이터에서 제어 파라미터를 파싱합니다.
	///     attempts to parse control parameters from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Control result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length < Size) {
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new Control(data);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

    // ── Driver 그룹 ────────────────────────────────────────────────────────────
    // ── Driver group ───────────────────────────────────────────────────────────

    /// <summary>
    ///     드라이버 ID (1–15)
    ///     driver ID (1–15)
    /// </summary>
    public int DriverId { get; init; }

    /// <summary>
    ///     드라이버 모델 (list)
    ///     driver model (list)
    /// </summary>
    public int DriverModel { get; init; }

    /// <summary>
    ///     토크 단위 (list)
    ///     torque unit (list)
    /// </summary>
    public int TorqueUnit { get; init; }

    /// <summary>
    ///     토크 캘리브레이션 [%]
    ///     torque calibration [%]
    /// </summary>
    public int TorqueCalibration { get; init; }

    /// <summary>
    ///     모터 가속 [ms]
    ///     motor acceleration [ms]
    /// </summary>
    public int MotorAcceleration { get; init; }

    /// <summary>
    ///     토크 홀딩 시간 [ms]
    ///     torque holding time [ms]
    /// </summary>
    public int TorqueHoldingTime { get; init; }

    /// <summary>
    ///     풀림 속도 [rpm]
    ///     loosening speed [rpm]
    /// </summary>
    public int LoosenSpeed { get; init; }

    /// <summary>
    ///     체결 판정 최소 회전수 [turn]
    ///     judged fastening minimum turns [turn]
    /// </summary>
    public float JudgedFasteningMinTurns { get; init; }

    /// <summary>
    ///     자유 회전 최대 토크 [unit]
    ///     free speed maximum torque [unit]
    /// </summary>
    public float FreeSpeedMaxTorque { get; init; }

    /// <summary>
    ///     정방향 운전 시간 한계 [sec]
    ///     forward run time limit [sec]
    /// </summary>
    public float ForwardRunTimeLimit { get; init; }

    /// <summary>
    ///     역방향 운전 시간 한계 [sec]
    ///     reverse run time limit [sec]
    /// </summary>
    public float ReverseRunTimeLimit { get; init; }

    /// <summary>
    ///     모터 정지 시간 한계 [sec]
    ///     motor stall time limit [sec]
    /// </summary>
    public float MotorStallTimeLimit { get; init; }

    /// <summary>
    ///     홀딩 시간 각도 한계 [degree]
    ///     holding time angle limit [degree]
    /// </summary>
    public int HoldingTimeAngleLimit { get; init; }

    /// <summary>
    ///     자동 속도 (0=사용 안 함, 1=사용)
    ///     auto speed (0=disabled, 1=enabled)
    /// </summary>
    public int AutoSpeed { get; init; }

    /// <summary>
    ///     풀림 시 최대 토크 사용 (0=사용 안 함, 1=사용)
    ///     use max torque for loosening (0=disabled, 1=enabled)
    /// </summary>
    public int UseMaxTorqueForLoosen { get; init; }

    // ── Operation 그룹 ─────────────────────────────────────────────────────────
    // ── Operation group ────────────────────────────────────────────────────────

    /// <summary>
    ///     이벤트 데이터 선택 (list)
    ///     event data select (list)
    /// </summary>
    public int EventDataSelect { get; init; }

    /// <summary>
    ///     체결 OK 신호 시간 [ms]
    ///     fastening OK signal time [ms]
    /// </summary>
    public int FasteningOkSignalTime { get; init; }

    /// <summary>
    ///     체결 정지 오류 (0=사용 안 함, 1=사용)
    ///     fastening stop error (0=disabled, 1=enabled)
    /// </summary>
    public int FasteningStopError { get; init; }

    /// <summary>
    ///     드라이버 자동 잠금 (0=사용 안 함, 1=사용)
    ///     driver auto lock (0=disabled, 1=enabled)
    /// </summary>
    public int DriverAutoLock { get; init; }

    /// <summary>
    ///     트리거 시작 — 핸드헬드 전용 (0=사용 안 함, 1=사용)
    ///     trigger start — handheld only (0=disabled, 1=enabled)
    /// </summary>
    public int TriggerStart { get; init; }

    /// <summary>
    ///     역방향 시작 — 핸드헬드 전용 (0=사용 안 함, 1=사용)
    ///     reverse start — handheld only (0=disabled, 1=enabled)
    /// </summary>
    public int ReverseStart { get; init; }

    /// <summary>
    ///     역방향 제어 (0=사용 안 함, 1=사용)
    ///     reverse control (0=disabled, 1=enabled)
    /// </summary>
    public int ReverseControl { get; init; }

    /// <summary>
    ///     드라이버 온도 한계 [°C]
    ///     driver temperature limit [°C]
    /// </summary>
    public int DriverTemperatureLimit { get; init; }

    // ── Preset/Model 그룹 ──────────────────────────────────────────────────────
    // ── Preset/Model group ─────────────────────────────────────────────────────

    /// <summary>
    ///     전원 투입 시 초기 프리셋 번호 (1–17)
    ///     initial preset number when power on (1–17)
    /// </summary>
    public int InitialPresetOnPowerOn { get; init; }

    /// <summary>
    ///     패널에서 선택 (0=사용 안 함, 1=사용)
    ///     selection on panel (0=disabled, 1=enabled)
    /// </summary>
    public int SelectionOnPanel { get; init; }

    /// <summary>
    ///     모델 선택 모드 (0=사용 안 함, 1=사용)
    ///     model selection mode (0=disabled, 1=enabled)
    /// </summary>
    public int ModelSelectionMode { get; init; }

    /// <summary>
    ///     LCD에서 프리셋/모델 변경 (0=사용 안 함, 1=사용)
    ///     preset/model change by LCD (0=disabled, 1=enabled)
    /// </summary>
    public int PresetModelChangeByLcd { get; init; }

    /// <summary>
    ///     바코드로 프리셋/모델 시작 (0=사용 안 함, 1=사용)
    ///     preset/model start by barcode (0=disabled, 1=enabled)
    /// </summary>
    public int PresetModelStartByBarcode { get; init; }

    /// <summary>
    ///     모델 자동 재시작 (0=사용 안 함, 1=사용)
    ///     model auto restart (0=disabled, 1=enabled)
    /// </summary>
    public int ModelAutoRestart { get; init; }

    // ── Controller 그룹 ────────────────────────────────────────────────────────
    // ── Controller group ───────────────────────────────────────────────────────

    /// <summary>
    ///     비밀번호 (0–9999)
    ///     password (0–9999)
    /// </summary>
    public int Password { get; init; }

    /// <summary>
    ///     비프 알람 (0=사용 안 함, 1=사용)
    ///     beep alarm (0=disabled, 1=enabled)
    /// </summary>
    public int BeepAlarm { get; init; }

    /// <summary>
    ///     오류 표시 초기화 시간 [sec]
    ///     error display reset time [sec]
    /// </summary>
    public float ErrorDisplayResetTime { get; init; }

    /// <summary>
    ///     라이트 켜짐 시간 [sec]
    ///     light on time [sec]
    /// </summary>
    public int LightOnTime { get; init; }

    // ── Communication 그룹 ─────────────────────────────────────────────────────
    // ── Communication group ────────────────────────────────────────────────────

    /// <summary>
    ///     프로토콜 (0, 1)
    ///     protocol (0, 1)
    /// </summary>
    public int Protocol { get; init; }

    /// <summary>
    ///     RS232 포트 선택 (0, 1)
    ///     RS232 port select (0, 1)
    /// </summary>
    public int Rs232PortSelect { get; init; }

    /// <summary>
    ///     COM 포트 보드레이트 (list)
    ///     COM port baudrate (list)
    /// </summary>
    public int ComPortBaudrate { get; init; }

    /// <summary>
    ///     자동 데이터 출력 (0=사용 안 함, 1=사용)
    ///     auto data output (0=disabled, 1=enabled)
    /// </summary>
    public int AutoDataOutput { get; init; }

    /// <summary>
    ///     자동 데이터 출력 포트 (0, 1)
    ///     auto data output port (0, 1)
    /// </summary>
    public int AutoDataOutputPort { get; init; }

    // ── Crowfoot 그룹 ──────────────────────────────────────────────────────────
    // ── Crowfoot group ─────────────────────────────────────────────────────────

    /// <summary>
    ///     크로우풋 활성화 (0=사용 안 함, 1=사용)
    ///     crowfoot enable (0=disabled, 1=enabled)
    /// </summary>
    public int CrowfootEnable { get; init; }

    /// <summary>
    ///     크로우풋 비율 [%]
    ///     crowfoot ratio [%]
    /// </summary>
    public float CrowfootRatio { get; init; }

    /// <summary>
    ///     크로우풋 효율 [%]
    ///     crowfoot efficiency [%]
    /// </summary>
    public int CrowfootEfficiency { get; init; }

    /// <summary>
    ///     크로우풋 역방향 토크 [unit]
    ///     crowfoot reverse torque [unit]
    /// </summary>
    public float CrowfootReverseTorque { get; init; }

    /// <summary>
    ///     크로우풋 역방향 속도 [rpm]
    ///     crowfoot reverse speed [rpm]
    /// </summary>
    public int CrowfootReverseSpeed { get; init; }

    /// <summary>
    ///     크로우풋 최대 토크 [unit]
    ///     crowfoot maximum torque [unit]
    /// </summary>
    public float CrowfootMaxTorque { get; init; }

    // ── Wireless Tools 그룹 ────────────────────────────────────────────────────
    // ── Wireless Tools group ───────────────────────────────────────────────────

    /// <summary>
    ///     슬립 시간 [min]
    ///     sleep time [min]
    /// </summary>
    public int SleepTime { get; init; }

    /// <summary>
    ///     LCD 버튼 잠금 (0=사용 안 함, 1=사용)
    ///     LCD button lock (0=disabled, 1=enabled)
    /// </summary>
    public int LcdButtonLock { get; init; }

    /// <summary>
    ///     트리거 시작 지연 시간 [ms]
    ///     trigger start delay time [ms]
    /// </summary>
    public int TriggerStartDelayTime { get; init; }

    /// <summary>
    ///     표시할 프리셋 번호 선택 (BIT 플래그 — BIT.0=프리셋1, BIT.1=프리셋2, ...)
    ///     select display preset number (BIT flags — BIT.0=preset.1, BIT.1=preset.2, ...)
    /// </summary>
    public uint SelectDisplayPresetNumber { get; init; }

    /// <summary>
    ///     Wifi 연결 끊김 시 드라이버 잠금 (0=사용 안 함, 1=사용)
    ///     driver lock after wifi disconnect (0=disabled, 1=enabled)
    /// </summary>
    public int DriverLockAfterWifiDisconnect { get; init; }

    /// <summary>
    ///     풀림/체결 전환 타입 (0, 1)
    ///     loosening/fastening switch type (0, 1)
    /// </summary>
    public int LoosenFastenSwitchType { get; init; }

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    [Browsable(false)]
    public ulong Hash { get; init; }
}