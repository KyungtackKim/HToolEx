using System.ComponentModel;
using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     HANTAS 장치의 현재 상태를 담는 클래스. 레지스터 주소 100부터 읽어오며, 토크/각도/속도 등의 실시간 값을 포함합니다.
///     Class containing current HANTAS device status. Read from register address 100, includes real-time values like
///     torque/angle/speed.
/// </summary>
/// <remarks>
///     현재 측정값, 목표값, 상한/하한, 단위, 방향 등의 상태 정보를 포함합니다. 세대별로 레지스터 구성이 다릅니다.
///     Includes status info like current value, target value, upper/lower limits, unit, direction. Register layout varies
///     by generation.
/// </remarks>
public sealed class FormatStatus {
    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    /// <param name="type">도구 세대 타입 / tool generation type</param>
    public FormatStatus(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        int val1, val2;
        // Span 및 위치 초기화
        // initialize span and position
        var span = values.AsSpan();
        var pos  = 0;

        // 세대 타입 설정
        // set generation type
        Type = type;
        // 타입별 처리
        // process by type
        switch (type) {
            case GenerationTypes.GenRev1:
            case GenerationTypes.GenRev1Ad:
            case GenerationTypes.GenRev1Plus:
                // Gen1 상태값 파싱
                // parse Gen1 status values
                Torque   = BinarySpanReader.ReadUInt16(span, ref pos);
                Speed    = BinarySpanReader.ReadUInt16(span, ref pos);
                Current  = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset   = BinarySpanReader.ReadUInt16(span, ref pos);
                TorqueUp = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                FastenOk = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Ready    = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Run      = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Alarm    = BinarySpanReader.ReadUInt16(span, ref pos);
                // 회전 방향 값 읽기
                // read direction value
                val1 = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 방향인지 확인
                // check if direction is defined
                if (Enum.IsDefined(typeof(DirectionTypes), val1))
                    Direction = (DirectionTypes)val1;
                RemainScrew = BinarySpanReader.ReadUInt16(span, ref pos);
                // 입출력 값 읽기
                // read input/output values
                val1 = BinarySpanReader.ReadUInt16(span, ref pos);
                val2 = BinarySpanReader.ReadUInt16(span, ref pos);
                // 입출력 비트 배열로 변환
                // convert to input/output bit arrays
                Input  = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((val1 >> i) & 0x1)).ToArray();
                Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((val2 >> i) & 0x1)).ToArray();
                // 온도 설정
                // set temperature
                Temperature = BinarySpanReader.ReadUInt16(span, ref pos);
                break;
            case GenerationTypes.GenRev2:
                // Gen2 상태값 파싱
                // parse Gen2 status values
                Torque   = BinarySpanReader.ReadSingle(span, ref pos);
                Speed    = BinarySpanReader.ReadUInt16(span, ref pos);
                Current  = BinarySpanReader.ReadSingle(span, ref pos);
                Preset   = BinarySpanReader.ReadUInt16(span, ref pos);
                Model    = BinarySpanReader.ReadUInt16(span, ref pos);
                TorqueUp = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                FastenOk = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Ready    = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Run      = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                Alarm    = BinarySpanReader.ReadUInt16(span, ref pos);
                // 회전 방향 값 읽기
                // read direction value
                val1 = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 방향인지 확인
                // check if direction is defined
                if (Enum.IsDefined(typeof(DirectionTypes), val1))
                    Direction = (DirectionTypes)val1;
                RemainScrew = BinarySpanReader.ReadUInt16(span, ref pos);
                // 입출력 값 읽기
                // read input/output values
                var input  = BinarySpanReader.ReadUInt16(span, ref pos);
                var output = BinarySpanReader.ReadUInt16(span, ref pos);
                // 입출력 비트 배열로 변환
                // convert to input/output bit arrays
                Input  = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((input  >> i) & 0x1)).ToArray();
                Output = Enumerable.Range(0, 16).Select(i => Convert.ToBoolean((output >> i) & 0x1)).ToArray();
                // 온도 설정
                // set temperature
                Temperature = BinarySpanReader.ReadSingle(span, ref pos);
                // 잠금 상태 설정
                // set lock state
                IsLock = Convert.ToBoolean(BinarySpanReader.ReadUInt16(span, ref pos));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // 체크섬 계산
        // calculate checksum
        CheckSum = Utils.CalculateCheckSumFast(values);
        // 모든 데이터를 읽었는지 확인
        // verify all data has been read
        if (pos != span.Length)
            // 예외 발생
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{span.Length - pos} byte(s) remain");
    }

    /// <summary>
    ///     도구 세대 타입
    ///     Tool generation type
    /// </summary>
    public GenerationTypes Type { get; }

    /// <summary>
    ///     토크 값 (N.m 또는 설정 단위)
    ///     Torque value (N.m or configured unit)
    /// </summary>
    public float Torque { get; }

    /// <summary>
    ///     속도 값 (RPM)
    ///     Speed value (RPM)
    /// </summary>
    public int Speed { get; }

    /// <summary>
    ///     전류 값 (A)
    ///     Current value (A)
    /// </summary>
    public float Current { get; }

    /// <summary>
    ///     선택된 프리셋 번호 (0~31, MA=32, MB=33)
    ///     Selected preset number (0~31, MA=32, MB=33)
    /// </summary>
    public int Preset { get; }

    /// <summary>
    ///     선택된 모델 번호 (0~15)
    ///     Selected model number (0~15)
    /// </summary>
    public int Model { get; }

    /// <summary>
    ///     토크업 상태 (true: 토크 도달)
    ///     Torque-up state (true: torque reached)
    /// </summary>
    public bool TorqueUp { get; }

    /// <summary>
    ///     체결 OK 상태 (true: 체결 성공)
    ///     Fastening OK state (true: fastening successful)
    /// </summary>
    public bool FastenOk { get; }

    /// <summary>
    ///     준비 상태 (true: 동작 가능)
    ///     Ready state (true: ready to operate)
    /// </summary>
    public bool Ready { get; }

    /// <summary>
    ///     동작 상태 (true: 모터 동작 중)
    ///     Running state (true: motor running)
    /// </summary>
    public bool Run { get; }

    /// <summary>
    ///     알람 코드 (0: 정상)
    ///     Alarm code (0: normal)
    /// </summary>
    public int Alarm { get; }

    /// <summary>
    ///     회전 방향 상태
    ///     Rotation direction state
    /// </summary>
    public DirectionTypes Direction { get; }

    /// <summary>
    ///     남은 나사 개수
    ///     Remaining screw count
    /// </summary>
    public int RemainScrew { get; }

    /// <summary>
    ///     입력 신호 상태 배열 (16비트)
    ///     Input signal state array (16 bits)
    /// </summary>
    public bool[] Input { get; }

    /// <summary>
    ///     출력 신호 상태 배열 (16비트)
    ///     Output signal state array (16 bits)
    /// </summary>
    public bool[] Output { get; }

    /// <summary>
    ///     온도 값 (°C)
    ///     Temperature value (°C)
    /// </summary>
    public float Temperature { get; }

    /// <summary>
    ///     잠금 상태 (true: 잠금됨)
    ///     Lock state (true: locked)
    /// </summary>
    public bool IsLock { get; }

    /// <summary>
    ///     체크섬 값
    ///     Checksum value
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; }
}