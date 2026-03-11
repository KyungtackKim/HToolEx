using System.ComponentModel;
using HTool.Core.Util;

namespace HTool.Format.Param;

/// <summary>
///     추가 기능(Advanced Function) 체결 프리셋 설정 데이터 (90바이트).
///     함수 코드 0x03으로 레지스터 41001~41045 전체를 읽어 파싱한다.
///     Mode에 따라 Parameter 1~18의 역할이 달라지며, AsXxx() 메서드로 typed 뷰를 얻어 사용한다.
///     advanced function fastening preset configuration data (90 bytes).
///     parsed from the full register range 41001–41045 via function code 0x03.
///     parameter 1–18 meaning varies by Mode; use AsXxx() methods to obtain typed views.
/// </summary>
public readonly struct AdvPreset {
	/// <summary>
	///     파라미터 수 (Parameter 1~18)
	///     parameter count (Parameter 1–18)
	/// </summary>
	private const int ParameterCount = 18;

	/// <summary>
	///     프리셋 데이터 크기 (바이트, 레지스터 41001~41045)
	///     preset data size (bytes, registers 41001–41045)
	/// </summary>
	private static int Size => 90;

	/// <summary>
	///     원시 패킷 데이터에서 Advanced Function 프리셋 설정을 파싱합니다.
	///     parses Advanced Function preset configuration from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public AdvPreset(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data meets minimum size requirement
        if (data.Length < Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // Advanced Function 모드 읽기
        // read Advanced Function mode
        Mode = BinarySpanReader.ReadUInt16(data, ref pos);

        // Parameter 1~18 읽기 (각 4바이트 float)
        // read Parameter 1–18 (4 bytes each as float)
        var parameters = new float[ParameterCount];
        // 파라미터 순회
        // iterate parameters
        for (var i = 0; i < ParameterCount; i++)
            // 각 파라미터 float 읽기
            // read each parameter as float
            parameters[i] = BinarySpanReader.ReadSingle(data, ref pos);
        // 파라미터 배열 저장
        // store parameter array
        Parameters = parameters;

        // 예약 영역 건너뛰기 (4바이트, 41038~41039)
        // skip reserved area (4 bytes, 41038–41039)
        pos += 4;

        // 역방향 자유 회전 속도 읽기 [rpm]
        // read free reverse rotation speed [rpm]
        FreeReverseSpeed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 역방향 자유 회전 각도 읽기 [turn]
        // read free reverse rotation angle [turn]
        FreeReverseAngle = BinarySpanReader.ReadSingle(data, ref pos);
        // 토크업 후 각도 이동 속도 읽기 [rpm]
        // read angle-after-torque-up speed [rpm]
        AngleAfterTorqueUpSpeed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크업 후 각도 이동 각도 읽기 [degree]
        // read angle-after-torque-up angle [degree]
        AngleAfterTorqueUpAngle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 토크업 후 각도 이동 방향 읽기 (0=CW, 1=CCW)
        // read angle-after-torque-up direction (0=CW, 1=CCW)
        AngleAfterTorqueUpDirection = BinarySpanReader.ReadUInt16(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..pos]);
    }

	/// <summary>
	///     원시 데이터에서 Advanced Function 프리셋 설정을 파싱합니다.
	///     attempts to parse Advanced Function preset configuration from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out AdvPreset result) {
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
            result = new AdvPreset(data);
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

	/// <summary>
	///     Advanced Function 모드
	///     Advanced Function mode
	/// </summary>
	public int Mode { get; init; }

	/// <summary>
	///     파라미터 원시 배열 (Parameter 1~18). AsXxx() 메서드를 통해 typed 뷰로 접근합니다.
	///     raw parameter array (Parameter 1–18). access via AsXxx() methods for typed views.
	/// </summary>
	[Browsable(false)]
    public float[] Parameters { get; init; }

	/// <summary>
	///     역방향 자유 회전 속도 [rpm]
	///     free reverse rotation speed [rpm]
	/// </summary>
	public int FreeReverseSpeed { get; init; }

	/// <summary>
	///     역방향 자유 회전 각도 [turn]
	///     free reverse rotation angle [turn]
	/// </summary>
	public float FreeReverseAngle { get; init; }

	/// <summary>
	///     토크업 후 각도 이동 속도 [rpm]
	///     angle-after-torque-up speed [rpm]
	/// </summary>
	public int AngleAfterTorqueUpSpeed { get; init; }

	/// <summary>
	///     토크업 후 각도 이동 각도 [degree]
	///     angle-after-torque-up angle [degree]
	/// </summary>
	public int AngleAfterTorqueUpAngle { get; init; }

	/// <summary>
	///     토크업 후 각도 이동 방향 (0=CW, 1=CCW)
	///     angle-after-torque-up direction (0=CW, 1=CCW)
	/// </summary>
	public int AngleAfterTorqueUpDirection { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }

	/// <summary>
	///     파라미터를 Seating Detection 뷰로 해석합니다 (PLUS 전용).
	///     interprets parameters as a Seating Detection view (PLUS only).
	/// </summary>
	/// <returns>Seating Detection 파라미터 뷰 / Seating Detection parameter view</returns>
	public SeatingDetectionView AsSeatingDetection() {
        // Seating Detection 뷰 반환
        // return Seating Detection view
        return new SeatingDetectionView(Parameters);
    }

	/// <summary>
	///     파라미터를 Prevailing Control 뷰로 해석합니다 (PLUS 전용).
	///     interprets parameters as a Prevailing Control view (PLUS only).
	/// </summary>
	/// <returns>Prevailing Control 파라미터 뷰 / Prevailing Control parameter view</returns>
	public PrevailingControlView AsPrevailingControl() {
        // Prevailing Control 뷰 반환
        // return Prevailing Control view
        return new PrevailingControlView(Parameters);
    }

	/// <summary>
	///     파라미터를 Open Hole 뷰로 해석합니다.
	///     interprets parameters as an Open Hole view.
	/// </summary>
	/// <param name="isPlus">PLUS 모드 여부 / whether the tool is in PLUS mode</param>
	/// <returns>Open Hole 파라미터 뷰 / Open Hole parameter view</returns>
	public OpenHoleView AsOpenHole(bool isPlus) {
        // Open Hole 뷰 반환
        // return Open Hole view
        return new OpenHoleView(Parameters, isPlus);
    }

	/// <summary>
	///     파라미터를 Engaging Torque 뷰로 해석합니다.
	///     interprets parameters as an Engaging Torque view.
	/// </summary>
	/// <param name="isPlus">PLUS 모드 여부 / whether the tool is in PLUS mode</param>
	/// <returns>Engaging Torque 파라미터 뷰 / Engaging Torque parameter view</returns>
	public EngagingTorqueView AsEngagingTorque(bool isPlus) {
        // Engaging Torque 뷰 반환
        // return Engaging Torque view
        return new EngagingTorqueView(Parameters, isPlus);
    }

    // -------------------------------------------------------------------------
    // Mode-specific typed views
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Seating Detection 모드 파라미터 뷰 (PLUS 전용).
    ///     Seating Detection mode parameter view (PLUS only).
    /// </summary>
    public readonly struct SeatingDetectionView(float[] p) {
	    /// <summary>
	    ///     착좌 포인트 토크 비율 [unit]
	    ///     seating point torque rate [unit]
	    /// </summary>
	    public float SeatingPointTorqueRate => p[0];

	    /// <summary>
	    ///     착좌 최소 토크 [unit]
	    ///     seating minimum torque [unit]
	    /// </summary>
	    public float SeatingMinTorque => p[1];

	    /// <summary>
	    ///     착좌 최대 토크 [unit]
	    ///     seating maximum torque [unit]
	    /// </summary>
	    public float SeatingMaxTorque => p[2];

	    /// <summary>
	    ///     클램프 토크 목표값 대체 여부 (0=사용 안 함, 1=대체)
	    ///     clamp torque replaces target torque (0=no, 1=yes)
	    /// </summary>
	    public int ClampTorqueReplaceTarget => (int)p[3];

	    /// <summary>
	    ///     착좌 후 클램프 토크 [unit]
	    ///     clamp torque after seating [unit]
	    /// </summary>
	    public float ClampTorqueAfterSeating => p[4];

	    /// <summary>
	    ///     클램프 토크 최소 한계 [unit]
	    ///     clamp torque minimum limit [unit]
	    /// </summary>
	    public float ClampTorqueMinLimit => p[5];

	    /// <summary>
	    ///     클램프 토크 최대 한계 [unit]
	    ///     clamp torque maximum limit [unit]
	    /// </summary>
	    public float ClampTorqueMaxLimit => p[6];

	    /// <summary>
	    ///     A1 최소 각도 [degree]
	    ///     A1 minimum angle [degree]
	    /// </summary>
	    public int A1MinAngle => (int)p[7];

	    /// <summary>
	    ///     A1 최대 각도 [degree]
	    ///     A1 maximum angle [degree]
	    /// </summary>
	    public int A1MaxAngle => (int)p[8];

	    /// <summary>
	    ///     A2 최소 각도 [degree]
	    ///     A2 minimum angle [degree]
	    /// </summary>
	    public int A2MinAngle => (int)p[9];

	    /// <summary>
	    ///     A2 최대 각도 [degree]
	    ///     A2 maximum angle [degree]
	    /// </summary>
	    public int A2MaxAngle => (int)p[10];

	    /// <summary>
	    ///     런다운 토크 최소 한계 [unit]
	    ///     rundown torque minimum limit [unit]
	    /// </summary>
	    public float RundownTorqueMinLimit => p[11];

	    /// <summary>
	    ///     런다운 토크 최대 한계 [unit]
	    ///     rundown torque maximum limit [unit]
	    /// </summary>
	    public float RundownTorqueMaxLimit => p[12];

	    /// <summary>
	    ///     런다운 검사 시작 각도 [degree]
	    ///     rundown inspection start angle [degree]
	    /// </summary>
	    public int RundownInspectionStartAngle => (int)p[13];

	    /// <summary>
	    ///     런다운 검사 종료 각도 [degree]
	    ///     rundown inspection end angle [degree]
	    /// </summary>
	    public int RundownInspectionEndAngle => (int)p[14];
    }

    /// <summary>
    ///     Prevailing Control 모드 파라미터 뷰 (PLUS 전용).
    ///     Prevailing Control mode parameter view (PLUS only).
    /// </summary>
    public readonly struct PrevailingControlView(float[] p) {
	    /// <summary>
	    ///     프리베일링 스너그 토크 [unit]
	    ///     prevailing snug torque [unit]
	    /// </summary>
	    public float PrevailingSnugTorque => p[0];

	    /// <summary>
	    ///     스너그 토크 후 프리베일링 시작 각도 [degree]
	    ///     prevailing start angle after snug torque [degree]
	    /// </summary>
	    public int PrevailingStartAngleAfterSnug => (int)p[1];

	    /// <summary>
	    ///     프리베일링 각도 범위 [degree]
	    ///     prevailing angle range [degree]
	    /// </summary>
	    public int PrevailingAngleRange => (int)p[2];

	    /// <summary>
	    ///     프리베일링 중 최소 토크 [unit]
	    ///     minimum torque during prevailing [unit]
	    /// </summary>
	    public float MinTorqueDuringPrevailing => p[3];

	    /// <summary>
	    ///     프리베일링 중 최대 토크 [unit]
	    ///     maximum torque during prevailing [unit]
	    /// </summary>
	    public float MaxTorqueDuringPrevailing => p[4];

	    /// <summary>
	    ///     프리베일링 최소 토크 [unit]
	    ///     minimum prevailing torque [unit]
	    /// </summary>
	    public float MinPrevailingTorque => p[5];

	    /// <summary>
	    ///     프리베일링 최대 토크 [unit]
	    ///     maximum prevailing torque [unit]
	    /// </summary>
	    public float MaxPrevailingTorque => p[6];

	    /// <summary>
	    ///     프리베일링 보상 (list)
	    ///     prevailing compensation (list)
	    /// </summary>
	    public int PrevailingCompensation => (int)p[7];

	    /// <summary>
	    ///     프리베일링 후 클램프 토크 [unit]
	    ///     clamp torque after prevailing [unit]
	    /// </summary>
	    public float ClampTorqueAfterPrevailing => p[8];

	    /// <summary>
	    ///     클램프 토크 최소 한계 [unit]
	    ///     clamp torque minimum limit [unit]
	    /// </summary>
	    public float ClampTorqueMinLimit => p[9];

	    /// <summary>
	    ///     클램프 토크 최대 한계 [unit]
	    ///     clamp torque maximum limit [unit]
	    /// </summary>
	    public float ClampTorqueMaxLimit => p[10];

	    /// <summary>
	    ///     착좌 포인트 토크 비율 [unit]
	    ///     seating point torque rate [unit]
	    /// </summary>
	    public float SeatingPointTorqueRate => p[11];

	    /// <summary>
	    ///     착좌 최소 토크 [unit]
	    ///     seating minimum torque [unit]
	    /// </summary>
	    public float SeatingMinTorque => p[12];

	    /// <summary>
	    ///     착좌 최대 토크 [unit]
	    ///     seating maximum torque [unit]
	    /// </summary>
	    public float SeatingMaxTorque => p[13];

	    /// <summary>
	    ///     A1 최소 각도 [degree]
	    ///     A1 minimum angle [degree]
	    /// </summary>
	    public int A1MinAngle => (int)p[14];

	    /// <summary>
	    ///     A1 최대 각도 [degree]
	    ///     A1 maximum angle [degree]
	    /// </summary>
	    public int A1MaxAngle => (int)p[15];

	    /// <summary>
	    ///     A2 최소 각도 [degree]
	    ///     A2 minimum angle [degree]
	    /// </summary>
	    public int A2MinAngle => (int)p[16];

	    /// <summary>
	    ///     A2 최대 각도 [degree]
	    ///     A2 maximum angle [degree]
	    /// </summary>
	    public int A2MaxAngle => (int)p[17];
    }

    /// <summary>
    ///     Open Hole 모드 파라미터 뷰. PLUS와 NORMAL에서 공통 필드 외에 NORMAL 전용 착좌/각도 필드가 추가된다.
    ///     Open Hole mode parameter view. NORMAL mode adds seating and angle fields beyond the common fields.
    /// </summary>
    public readonly struct OpenHoleView(float[] p, bool isPlus) {
	    /// <summary>
	    ///     시작 토크 [unit]
	    ///     start torque [unit]
	    /// </summary>
	    public float StartTorque => p[0];

	    /// <summary>
	    ///     최대 토크 [unit]
	    ///     maximum torque [unit]
	    /// </summary>
	    public float MaxTorque => p[1];

	    /// <summary>
	    ///     종료 토크 [unit]
	    ///     end torque [unit]
	    /// </summary>
	    public float EndTorque => p[2];

	    /// <summary>
	    ///     속도 [rpm]
	    ///     speed [rpm]
	    /// </summary>
	    public int Speed => (int)p[3];

	    /// <summary>
	    ///     시작 토크 기준 각도 한계 [degree]
	    ///     angle limit from start torque [degree]
	    /// </summary>
	    public int AngleLimitFromStartTorque => (int)p[4];

	    /// <summary>
	    ///     태핑 시작 각도 사용 여부 (0=사용 안 함, 1=사용)
	    ///     use angle start from tapping (0=no, 1=yes)
	    /// </summary>
	    public int AngleStartFromTapping => (int)p[5];

	    /// <summary>
	    ///     착좌 최소 토크 — NORMAL 전용, PLUS에서는 0 [unit]
	    ///     seating minimum torque — NORMAL only, 0 in PLUS [unit]
	    /// </summary>
	    public float SeatingMinTorque => isPlus ? 0f : p[8];

	    /// <summary>
	    ///     착좌 최대 토크 — NORMAL 전용, PLUS에서는 0 [unit]
	    ///     seating maximum torque — NORMAL only, 0 in PLUS [unit]
	    /// </summary>
	    public float SeatingMaxTorque => isPlus ? 0f : p[9];

	    /// <summary>
	    ///     A1 최소 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A1 minimum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A1MinAngle => isPlus ? 0 : (int)p[10];

	    /// <summary>
	    ///     A1 최대 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A1 maximum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A1MaxAngle => isPlus ? 0 : (int)p[11];

	    /// <summary>
	    ///     A2 최소 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A2 minimum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A2MinAngle => isPlus ? 0 : (int)p[12];

	    /// <summary>
	    ///     A2 최대 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A2 maximum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A2MaxAngle => isPlus ? 0 : (int)p[13];
    }

    /// <summary>
    ///     Engaging Torque 모드 파라미터 뷰. PLUS와 NORMAL에서 토크 해상도가 다르며, NORMAL에 착좌/각도 필드가 추가된다.
    ///     Engaging Torque mode parameter view. torque resolution differs between PLUS and NORMAL;
    ///     NORMAL adds seating and angle fields.
    /// </summary>
    public readonly struct EngagingTorqueView(float[] p, bool isPlus) {
	    /// <summary>
	    ///     맞물림 토크 [%] (PLUS=0.01% 해상도, NORMAL=0.1% 해상도)
	    ///     engaging torque [%] (PLUS=0.01% resolution, NORMAL=0.1% resolution)
	    /// </summary>
	    public float Torque => p[0];

	    /// <summary>
	    ///     속도 [rpm]
	    ///     speed [rpm]
	    /// </summary>
	    public int Speed => (int)p[1];

	    /// <summary>
	    ///     각도 한계 [degree]
	    ///     angle limit [degree]
	    /// </summary>
	    public int AngleLimit => (int)p[2];

	    /// <summary>
	    ///     시간 한계 [sec]
	    ///     time limit [sec]
	    /// </summary>
	    public float TimeLimit => p[3];

	    /// <summary>
	    ///     맞물림 후 각도 클리어 여부 (0=사용 안 함, 1=사용)
	    ///     clear angle after engaging (0=no, 1=yes)
	    /// </summary>
	    public int AngleClearAfterEngaging => (int)p[4];

	    /// <summary>
	    ///     착좌 최소 토크 — NORMAL 전용, PLUS에서는 0 [unit]
	    ///     seating minimum torque — NORMAL only, 0 in PLUS [unit]
	    /// </summary>
	    public float SeatingMinTorque => isPlus ? 0f : p[8];

	    /// <summary>
	    ///     착좌 최대 토크 — NORMAL 전용, PLUS에서는 0 [unit]
	    ///     seating maximum torque — NORMAL only, 0 in PLUS [unit]
	    /// </summary>
	    public float SeatingMaxTorque => isPlus ? 0f : p[9];

	    /// <summary>
	    ///     A1 최소 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A1 minimum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A1MinAngle => isPlus ? 0 : (int)p[10];

	    /// <summary>
	    ///     A1 최대 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A1 maximum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A1MaxAngle => isPlus ? 0 : (int)p[11];

	    /// <summary>
	    ///     A2 최소 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A2 minimum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A2MinAngle => isPlus ? 0 : (int)p[12];

	    /// <summary>
	    ///     A2 최대 각도 — NORMAL 전용, PLUS에서는 0 [degree]
	    ///     A2 maximum angle — NORMAL only, 0 in PLUS [degree]
	    /// </summary>
	    public int A2MaxAngle => isPlus ? 0 : (int)p[13];
    }
}