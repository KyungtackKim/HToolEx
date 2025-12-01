using System.ComponentModel;
using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     체결 작업 완료 시 발생하는 이벤트 데이터를 담는 클래스. Gen.2의 경우 214바이트, Gen.1은 더 작습니다.
///     Class containing event data generated upon fastening operation completion. 214 bytes for Gen.2, smaller for Gen.1.
/// </summary>
/// <remarks>
///     최종 토크, 각도, OK/NG 결과, 시트 토크, 클램프 토크, 프리베일 토크 등의 체결 결과 데이터를 포함합니다.
///     Includes fastening result data like final torque, angle, OK/NG result, seating torque, clamp torque, prevailing
///     torque, etc.
/// </remarks>
public sealed class FormatEvent {
    /// <summary>
    ///     기본 생성자
    ///     Default constructor
    /// </summary>
    public FormatEvent() { }

    /// <summary>
    ///     바이트 배열로부터 이벤트 데이터 생성
    ///     Constructor to create event data from byte array
    /// </summary>
    /// <param name="values">원시 이벤트 데이터 / raw event data</param>
    /// <param name="type">프로토콜 세대 타입 / protocol generation type</param>
    public FormatEvent(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        // set the event data
        Set(values, type);
    }

    /// <summary>
    ///     프로토콜 세대 타입
    ///     Protocol generation type
    /// </summary>
    public GenerationTypes Type { get; set; }

    /// <summary>
    ///     이벤트 고유 ID
    ///     Unique event ID
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    ///     이벤트 포맷 리비전 (예: "1.0")
    ///     Event format revision (e.g., "1.0")
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    ///     이벤트 발생 날짜
    ///     Event occurrence date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     이벤트 발생 시간
    ///     Event occurrence time
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     체결 소요 시간 (밀리초)
    ///     Fastening duration (milliseconds)
    /// </summary>
    public int FastenTime { get; set; }

    /// <summary>
    ///     선택된 프리셋 번호 (0-31, MA=32, MB=33)
    ///     Selected preset number (0-31, MA=32, MB=33)
    /// </summary>
    public int Preset { get; set; }

    /// <summary>
    ///     토크 단위
    ///     Torque unit
    /// </summary>
    public UnitTypes Unit { get; set; }

    /// <summary>
    ///     남은 스크류 카운트
    ///     Remaining screw count
    /// </summary>
    public int RemainScrew { get; set; }

    /// <summary>
    ///     모터 회전 방향 (체결/풀림)
    ///     Motor rotation direction (fastening/loosening)
    /// </summary>
    public DirectionTypes Direction { get; set; }

    /// <summary>
    ///     에러 코드 (0=정상)
    ///     Error code (0=normal)
    /// </summary>
    public int Error { get; set; }

    /// <summary>
    ///     이벤트 상태 (OK/NG/에러 등)
    ///     Event status (OK/NG/Error etc.)
    /// </summary>
    public EventTypes Event { get; set; }

    /// <summary>
    ///     목표 토크 값
    ///     Target torque value
    /// </summary>
    public float TargetTorque { get; set; }

    /// <summary>
    ///     실제 측정 토크 값
    ///     Actual measured torque value
    /// </summary>
    public float Torque { get; set; }

    /// <summary>
    ///     착좌 토크 (볼트 착좌 시점의 토크)
    ///     Seating torque (torque at bolt seating point)
    /// </summary>
    public float SeatingTorque { get; set; }

    /// <summary>
    ///     클램프 토크 (부품 밀착 시점의 토크)
    ///     Clamp torque (torque at part clamping point)
    /// </summary>
    public float ClampTorque { get; set; }

    /// <summary>
    ///     프리베일링 토크 (나사산 마찰 토크)
    ///     Prevailing torque (thread friction torque)
    /// </summary>
    public float PrevailingTorque { get; set; }

    /// <summary>
    ///     스너그 토크 (초기 접촉 토크)
    ///     Snug torque (initial contact torque)
    /// </summary>
    public float SnugTorque { get; set; }

    /// <summary>
    ///     모터 회전 속도 (RPM)
    ///     Motor rotation speed (RPM)
    /// </summary>
    public int Speed { get; set; }

    /// <summary>
    ///     각도 1 (스너그 전 각도)
    ///     Angle 1 (angle before snug)
    /// </summary>
    public int Angle1 { get; set; }

    /// <summary>
    ///     각도 2 (스너그 후 각도)
    ///     Angle 2 (angle after snug)
    /// </summary>
    public int Angle2 { get; set; }

    /// <summary>
    ///     총 각도 (Angle1 + Angle2)
    ///     Total angle (Angle1 + Angle2)
    /// </summary>
    public int Angle { get; set; }

    /// <summary>
    ///     스너그 각도 (스너그 시점까지의 각도)
    ///     Snug angle (angle until snug point)
    /// </summary>
    public int SnugAngle { get; set; }

    /// <summary>
    ///     바코드 데이터
    ///     Barcode data
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    ///     그래프 채널 1 데이터 타입
    ///     Graph channel 1 data type
    /// </summary>
    public GraphTypes TypeOfChannel1 { get; set; }

    /// <summary>
    ///     그래프 채널 2 데이터 타입
    ///     Graph channel 2 data type
    /// </summary>
    public GraphTypes TypeOfChannel2 { get; set; }

    /// <summary>
    ///     그래프 채널 1 데이터 포인트 수
    ///     Graph channel 1 data point count
    /// </summary>
    public int CountOfChannel1 { get; set; }

    /// <summary>
    ///     그래프 채널 2 데이터 포인트 수
    ///     Graph channel 2 data point count
    /// </summary>
    public int CountOfChannel2 { get; set; }

    /// <summary>
    ///     그래프 샘플링 주기 (밀리초)
    ///     Graph sampling rate (milliseconds)
    /// </summary>
    public int SamplingRate { get; set; }

    /// <summary>
    ///     그래프 스텝 정보 배열 (최대 16개)
    ///     Graph step information array (max 16 steps)
    /// </summary>
    [Browsable(false)]
    public GraphStep[] GraphSteps { get; set; } = new GraphStep[16];

    /// <summary>
    ///     데이터 무결성 검증용 체크섬
    ///     Checksum for data integrity verification
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; set; }

    /// <summary>
    ///     바이트 배열에서 이벤트 데이터 파싱
    ///     Parse event data from byte array
    /// </summary>
    /// <param name="values">원시 이벤트 데이터 / raw event data</param>
    /// <param name="type">프로토콜 세대 타입 / protocol generation type</param>
    public void Set(byte[] values, GenerationTypes type = GenerationTypes.GenRev2) {
        int dir;
        int status;
        var pos = 0;
        // 정보 초기화
        // reset information
        Reset();
        // 날짜/시간 값 유효성 확인
        // check date/time values validity
        if (values[4] == 0 && values[5] == 0 && values[6] == 0 && values[7] == 0 && values[8] == 0 && values[9] == 0)
            return;
        // 타입 설정
        // set type
        Type = type;
        // Span 초기화
        // initialize span
        var span = values.AsSpan();
        // 타입별 처리
        // process by type
        switch (type) {
            case GenerationTypes.GenRev1:
            case GenerationTypes.GenRev1Ad:
                // Gen1/Gen1Ad 이벤트 값 파싱
                // parse Gen1/Gen1Ad event values
                Id           = BinarySpanReader.ReadUInt16(span, ref pos);
                Date         = Time = DateTime.Now;
                FastenTime   = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset       = BinarySpanReader.ReadUInt16(span, ref pos);
                TargetTorque = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Torque       = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Speed        = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle1       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle2       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle        = BinarySpanReader.ReadUInt16(span, ref pos);
                RemainScrew  = BinarySpanReader.ReadUInt16(span, ref pos);
                Error        = BinarySpanReader.ReadUInt16(span, ref pos);
                // 회전 방향 읽기
                // read direction
                dir = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (dir <= (int)DirectionTypes.Loosening)
                    Direction = (DirectionTypes)dir;
                // 이벤트 상태 읽기
                // read event status
                status = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    Event = (EventTypes)status;
                SnugAngle =  BinarySpanReader.ReadUInt16(span, ref pos);
                Barcode   =  Utils.ToAsciiTrimEnd(span.Slice(pos, Constants.BarcodeLength));
                pos       += Constants.BarcodeLength;
                break;
            case GenerationTypes.GenRev1Plus:
                // Gen1Plus 이벤트 값 파싱
                // parse Gen1Plus event values
                Id           = BinarySpanReader.ReadUInt16(span, ref pos);
                Date         = Time = DateTime.Now;
                FastenTime   = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset       = BinarySpanReader.ReadUInt16(span, ref pos);
                TargetTorque = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Torque       = BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Speed        = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle1       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle2       = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle        = BinarySpanReader.ReadUInt16(span, ref pos);
                RemainScrew  = BinarySpanReader.ReadUInt16(span, ref pos);
                Error        = BinarySpanReader.ReadUInt16(span, ref pos);
                // 회전 방향 읽기
                // read direction
                dir = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (dir <= (int)DirectionTypes.Loosening)
                    Direction = (DirectionTypes)dir;
                // 이벤트 상태 읽기
                // read event status
                status = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    Event = (EventTypes)status;
                SnugAngle        =  BinarySpanReader.ReadUInt16(span, ref pos);
                SeatingTorque    =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                ClampTorque      =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                PrevailingTorque =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                SnugTorque       =  BinarySpanReader.ReadUInt16(span, ref pos) / 100.0f;
                Barcode          =  Utils.ToAsciiTrimEnd(span.Slice(pos, Constants.BarcodeLength));
                pos              += Constants.BarcodeLength;
                break;
            case GenerationTypes.GenRev2:
                // Gen2 헤더 파싱
                // parse Gen2 header
                Revision = $"{BinarySpanReader.ReadByte(span, ref pos)}.{BinarySpanReader.ReadByte(span, ref pos)}";
                Id       = BinarySpanReader.ReadUInt16(span, ref pos);
                // 날짜/시간 파싱
                // parse date/time
                var year        = BinarySpanReader.ReadUInt16(span, ref pos);
                var month       = BinarySpanReader.ReadByte(span, ref pos);
                var day         = BinarySpanReader.ReadByte(span, ref pos);
                var hour        = BinarySpanReader.ReadByte(span, ref pos);
                var minute      = BinarySpanReader.ReadByte(span, ref pos);
                var second      = BinarySpanReader.ReadByte(span, ref pos);
                var millisecond = BinarySpanReader.ReadByte(span, ref pos);
                // 날짜/시간 설정
                // set date/time
                Date = Time = new DateTime(year, month, day, hour, minute, second, millisecond);
                // 공통 값 파싱
                // parse common values
                FastenTime = BinarySpanReader.ReadUInt16(span, ref pos);
                Preset     = BinarySpanReader.ReadUInt16(span, ref pos);
                // 단위 읽기
                // read unit
                var unit = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (unit <= (int)UnitTypes.LbfFt)
                    Unit = (UnitTypes)unit;
                RemainScrew = BinarySpanReader.ReadUInt16(span, ref pos);
                // 회전 방향 읽기
                // read direction
                dir = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (dir <= (int)DirectionTypes.Loosening)
                    Direction = (DirectionTypes)dir;
                Error = BinarySpanReader.ReadUInt16(span, ref pos);
                // 이벤트 상태 읽기
                // read event status
                status = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (status <= (int)EventTypes.ScrewCountReset)
                    Event = (EventTypes)status;
                // 토크 값 파싱
                // parse torque values
                TargetTorque     = BinarySpanReader.ReadSingle(span, ref pos);
                Torque           = BinarySpanReader.ReadSingle(span, ref pos);
                SeatingTorque    = BinarySpanReader.ReadSingle(span, ref pos);
                ClampTorque      = BinarySpanReader.ReadSingle(span, ref pos);
                PrevailingTorque = BinarySpanReader.ReadSingle(span, ref pos);
                SnugTorque       = BinarySpanReader.ReadSingle(span, ref pos);
                // 속도/각도 파싱
                // parse speed/angle
                Speed     = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle1    = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle2    = BinarySpanReader.ReadUInt16(span, ref pos);
                Angle     = BinarySpanReader.ReadUInt16(span, ref pos);
                SnugAngle = BinarySpanReader.ReadUInt16(span, ref pos);
                // 예약 영역 건너뛰기
                // skip reserved area
                pos += 16;
                // 바코드 설정
                // set barcode
                Barcode =  Utils.ToAsciiTrimEnd(span.Slice(pos, Constants.BarcodeLength));
                pos     += Constants.BarcodeLength;
                // 그래프 타입 읽기
                // read graph types
                var type1 = BinarySpanReader.ReadUInt16(span, ref pos);
                var type2 = BinarySpanReader.ReadUInt16(span, ref pos);
                // 정의된 값인지 확인
                // check if defined
                if (type1 <= (int)GraphTypes.TorqueAngle)
                    TypeOfChannel1 = (GraphTypes)type1;
                if (type2 <= (int)GraphTypes.TorqueAngle)
                    TypeOfChannel2 = (GraphTypes)type2;
                CountOfChannel1 = BinarySpanReader.ReadUInt16(span, ref pos);
                CountOfChannel2 = BinarySpanReader.ReadUInt16(span, ref pos);
                SamplingRate    = BinarySpanReader.ReadUInt16(span, ref pos);
                // 그래프 스텝 파싱
                // parse graph steps
                for (var i = 0; i < GraphSteps.Length; i++) {
                    // 스텝 정보 읽기
                    // read step information
                    var id    = BinarySpanReader.ReadUInt16(span, ref pos);
                    var index = BinarySpanReader.ReadUInt16(span, ref pos);
                    // 정의된 값인지 확인
                    // check if defined
                    if (id <= (int)GraphStepTypes.RotationAfterTorqueUp)
                        GraphSteps[i] = new GraphStep((GraphStepTypes)id, index);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        // 체크섬 계산
        // calculate checksum
        CheckSum = Utils.CalculateCheckSumFast(span);
        // 데이터 길이 검증
        // validate data length
        if (pos != span.Length)
            throw new InvalidDataException($"Not all bytes have been consumed. {span.Length - pos} byte(s) remain");
    }

    /// <summary>
    ///     모든 속성을 기본값으로 초기화
    ///     Reset all properties to default values
    /// </summary>
    private void Reset() {
        // 기본 정보 초기화
        // reset basic information
        Id              = 0;
        Revision        = string.Empty;
        Date            = default;
        Time            = default;
        FastenTime      = Preset = RemainScrew   = Error       = Speed            = Angle1     = Angle2 = Angle = SnugAngle = 0;
        TargetTorque    = Torque = SeatingTorque = ClampTorque = PrevailingTorque = SnugTorque = 0f;
        Unit            = default;
        Direction       = default;
        Event           = default;
        TypeOfChannel1  = default;
        TypeOfChannel2  = default;
        CountOfChannel1 = CountOfChannel2 = SamplingRate = 0;
        Barcode         = string.Empty;
        Array.Clear(GraphSteps, 0, GraphSteps.Length);
        CheckSum = 0;
    }

    /// <summary>
    ///     그래프 스텝 정보 클래스 (체결 과정의 단계별 정보)
    ///     Graph step information class (step-by-step fastening process info)
    /// </summary>
    public class GraphStep {
        /// <summary>
        ///     생성자
        ///     Constructor
        /// </summary>
        /// <param name="id">스텝 타입 / step type</param>
        /// <param name="index">그래프 데이터 인덱스 / graph data index</param>
        public GraphStep(GraphStepTypes id, int index) {
            // 스텝 타입 설정
            // set step type
            Id = id;
            // 그래프 인덱스 설정
            // set graph index
            Index = index;
        }

        /// <summary>
        ///     스텝 타입 (체결 단계 식별자)
        ///     Step type (fastening phase identifier)
        /// </summary>
        public GraphStepTypes Id { get; set; }

        /// <summary>
        ///     그래프 데이터의 시작 인덱스
        ///     Start index in graph data
        /// </summary>
        public int Index { get; set; }
    }
}