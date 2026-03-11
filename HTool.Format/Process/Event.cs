using System.ComponentModel;
using System.Globalization;
using HTool.Core.Type.Process;
using HTool.Core.Util;
using EventType = HTool.Core.Type.Process.Event;

namespace HTool.Format.Process;

/// <summary>
///     체결 작업 완료 시 발생하는 이벤트 데이터를 담는 클래스.
///     직접 연결(214B Gen.2)과 PRO X Last Event를 통합 지원한다.
///     sealed class containing event data generated upon fastening operation completion.
///     supports both direct connection (214B Gen.2) and PRO X Last Event formats.
/// </summary>
/// <remarks>
///     revision &lt; 0: 직접 연결 MODBUS 포맷 (ParseCore, 214바이트 고정)
///     revision &gt;= 0: PRO X Last Event 포맷 (ParsePro, Rev.0=1702B / Rev.1=2214B 고정 + 가변 그래프)
///     revision &lt; 0: direct connection MODBUS format (ParseCore, 214 bytes fixed)
///     revision &gt;= 0: PRO X Last Event format (ParsePro, Rev.0=1702B / Rev.1=2214B fixed + variable graph)
/// </remarks>
public sealed class Event {
    /// <summary>
    ///     바코드 필드 길이 (바이트)
    ///     barcode field length (bytes)
    /// </summary>
    private const int BarcodeLength = 64;

    /// <summary>
    ///     최대 그래프 스텝 수
    ///     maximum graph step count
    /// </summary>
    private const int MaxGraphSteps = 16;

    /// <summary>
    ///     PRO X ID 필드 길이 (바이트)
    ///     PRO X ID field length (bytes)
    /// </summary>
    private const int IdFieldLength = 128;

    /// <summary>
    ///     PRO X ID 쌍 수 (ID1~ID6)
    ///     PRO X ID pair count (ID1~ID6)
    /// </summary>
    private const int IdPairCount = 6;

    /// <summary>
    ///     PRO X 시간 필드 길이 (바이트, ASCII "yyyy-MM-dd HH:mm:ss")
    ///     PRO X time field length (bytes, ASCII "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    private const int ProTimeLength = 20;

    /// <summary>
    ///     통합 생성자. revision에 따라 직접 연결(코어) 또는 PRO X(확장) 포맷을 파싱한다.
    ///     unified constructor. parses direct connection (core) or PRO X (extended) format based on revision.
    /// </summary>
    /// <param name="data">원시 이벤트 데이터 / raw event data</param>
    /// <param name="revision">PRO X 리비전 (음수=직접 연결) / PRO X revision (negative=direct connection)</param>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    /// <exception cref="ArgumentOutOfRangeException">리비전이 범위 밖일 때 / when revision is out of range</exception>
    public Event(ReadOnlySpan<byte> data, int revision = -1) {
        // 직접 연결 모드인지 확인
        // check if direct connection mode
        if (revision < 0) {
            // 코어 데이터 크기 검증
            // validate core data size
            if (data.Length < Size)
                // 데이터 부족 예외 발생
                // throw insufficient data exception
                throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");
            // 직접 연결 포맷 파싱 (214B)
            // parse direct connection format (214B)
            ParseCore(data);
        } else {
            // PRO X 리비전 범위 검증
            // validate PRO X revision range
            if (revision >= ProData.Sizes.Length)
                // 리비전 범위 초과 예외 발생
                // throw revision out of range exception
                throw new ArgumentOutOfRangeException(nameof(revision),
                    $"Revision {revision} is out of range. Max: {ProData.Sizes.Length - 1}");
            // PRO X 데이터 크기 검증
            // validate PRO X data size
            if (data.Length < ProData.Sizes[revision])
                // 데이터 부족 예외 발생
                // throw insufficient data exception
                throw new FormatException(
                    $"Data length {data.Length} is less than required {ProData.Sizes[revision]} bytes for revision {revision}.");
            // PRO X Last Event 포맷 파싱
            // parse PRO X Last Event format
            ParsePro(data, revision);
        }
    }

    /// <summary>
    ///     직접 연결 이벤트 데이터의 고정 크기 (바이트, Gen.2 기준)
    ///     direct connection event data fixed size (bytes, Gen.2)
    /// </summary>
    public static int Size => 214;

    /// <summary>
    ///     이벤트 고유 ID
    ///     unique event ID
    /// </summary>
    public uint Id { get; private set; }

    /// <summary>
    ///     이벤트 포맷 리비전 (직접 연결: "major.minor", PRO X: 빈 문자열)
    ///     event format revision (direct: "major.minor", PRO X: empty string)
    /// </summary>
    public string Revision { get; private set; } = string.Empty;

    /// <summary>
    ///     이벤트 발생 날짜
    ///     event occurrence date
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    ///     이벤트 발생 시간
    ///     event occurrence time
    /// </summary>
    public DateTime Time { get; private set; }

    /// <summary>
    ///     체결 소요 시간 (밀리초)
    ///     fastening duration (milliseconds)
    /// </summary>
    public int FastenTime { get; private set; }

    /// <summary>
    ///     선택된 프리셋 번호 (0-31, MA=32, MB=33)
    ///     selected preset number (0-31, MA=32, MB=33)
    /// </summary>
    public int Preset { get; private set; }

    /// <summary>
    ///     토크 단위
    ///     torque unit
    /// </summary>
    public Unit TorqueUnit { get; private set; }

    /// <summary>
    ///     남은 스크류 카운트
    ///     remaining screw count
    /// </summary>
    public int RemainScrew { get; private set; }

    /// <summary>
    ///     모터 회전 방향 (체결/풀림)
    ///     motor rotation direction (fastening/loosening)
    /// </summary>
    public Direction Direction { get; private set; }

    /// <summary>
    ///     에러 코드 (0=정상)
    ///     error code (0=normal)
    /// </summary>
    public int Error { get; private set; }

    /// <summary>
    ///     이벤트 상태 (OK/NG/에러 등)
    ///     event status (OK/NG/Error etc.)
    /// </summary>
    public EventType EventStatus { get; private set; }

    /// <summary>
    ///     목표 토크 값
    ///     target torque value
    /// </summary>
    public float TargetTorque { get; private set; }

    /// <summary>
    ///     실제 측정 토크 값
    ///     actual measured torque value
    /// </summary>
    public float Torque { get; private set; }

    /// <summary>
    ///     착좌 토크 (볼트 착좌 시점의 토크)
    ///     seating torque (torque at bolt seating point)
    /// </summary>
    public float SeatingTorque { get; private set; }

    /// <summary>
    ///     클램프 토크 (부품 밀착 시점의 토크)
    ///     clamp torque (torque at part clamping point)
    /// </summary>
    public float ClampTorque { get; private set; }

    /// <summary>
    ///     프리베일링 토크 (나사산 마찰 토크)
    ///     prevailing torque (thread friction torque)
    /// </summary>
    public float PrevailingTorque { get; private set; }

    /// <summary>
    ///     스너그 토크 (초기 접촉 토크)
    ///     snug torque (initial contact torque)
    /// </summary>
    public float SnugTorque { get; private set; }

    /// <summary>
    ///     모터 회전 속도 (RPM)
    ///     motor rotation speed (RPM)
    /// </summary>
    public int Speed { get; private set; }

    /// <summary>
    ///     각도 1 (스너그 전 각도)
    ///     angle 1 (angle before snug)
    /// </summary>
    public int Angle1 { get; private set; }

    /// <summary>
    ///     각도 2 (스너그 후 각도)
    ///     angle 2 (angle after snug)
    /// </summary>
    public int Angle2 { get; private set; }

    /// <summary>
    ///     총 각도 (Angle1 + Angle2)
    ///     total angle (Angle1 + Angle2)
    /// </summary>
    public int Angle { get; private set; }

    /// <summary>
    ///     스너그 각도 (스너그 시점까지의 각도)
    ///     snug angle (angle until snug point)
    /// </summary>
    public int SnugAngle { get; private set; }

    /// <summary>
    ///     바코드 데이터 (PRO X 모드에서는 Id1 값으로 설정)
    ///     barcode data (set to Id1 value in PRO X mode)
    /// </summary>
    public string Barcode { get; private set; } = string.Empty;

    /// <summary>
    ///     그래프 채널 1 데이터 타입
    ///     graph channel 1 data type
    /// </summary>
    public GraphChannel TypeOfChannel1 { get; private set; }

    /// <summary>
    ///     그래프 채널 2 데이터 타입
    ///     graph channel 2 data type
    /// </summary>
    public GraphChannel TypeOfChannel2 { get; private set; }

    /// <summary>
    ///     그래프 채널 1 데이터 포인트 수
    ///     graph channel 1 data point count
    /// </summary>
    public int CountOfChannel1 { get; private set; }

    /// <summary>
    ///     그래프 채널 2 데이터 포인트 수
    ///     graph channel 2 data point count
    /// </summary>
    public int CountOfChannel2 { get; private set; }

    /// <summary>
    ///     그래프 샘플링 주기 (밀리초)
    ///     graph sampling rate (milliseconds)
    /// </summary>
    public int SamplingRate { get; private set; }

    /// <summary>
    ///     그래프 스텝 정보 배열 (최대 16개)
    ///     graph step information array (max 16 steps)
    /// </summary>
    [Browsable(false)]
    public GraphStepInfo[] GraphSteps { get; private set; } = [];

    /// <summary>
    ///     PRO X 확장 데이터 (직접 연결 시 null)
    ///     PRO X extension data (null for direct connection)
    /// </summary>
    [Browsable(false)]
    public ProData? Pro { get; private set; }

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    [Browsable(false)]
    public ulong Hash { get; private set; }

    /// <summary>
    ///     코어 이벤트 데이터 파싱 (직접 연결 214바이트 Gen.2 포맷)
    ///     parses core event data (direct connection 214-byte Gen.2 format)
    /// </summary>
    /// <param name="data">원시 이벤트 데이터 / raw event data</param>
    private void ParseCore(ReadOnlySpan<byte> data) {
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 리비전 파싱
        // parse revision
        Revision = $"{BinarySpanReader.ReadByte(data, ref pos)}.{BinarySpanReader.ReadByte(data, ref pos)}";
        // 이벤트 ID 파싱
        // parse event ID
        Id = BinarySpanReader.ReadUInt16(data, ref pos);

        // 연도 파싱
        // parse year
        var year = BinarySpanReader.ReadUInt16(data, ref pos);
        // 월 파싱
        // parse month
        var month = BinarySpanReader.ReadByte(data, ref pos);
        // 일 파싱
        // parse day
        var day = BinarySpanReader.ReadByte(data, ref pos);
        // 시 파싱
        // parse hour
        var hour = BinarySpanReader.ReadByte(data, ref pos);
        // 분 파싱
        // parse minute
        var minute = BinarySpanReader.ReadByte(data, ref pos);
        // 초 파싱
        // parse second
        var second = BinarySpanReader.ReadByte(data, ref pos);
        // 밀리초 파싱
        // parse millisecond
        var millisecond = BinarySpanReader.ReadByte(data, ref pos);
        // DateTime 설정
        // set DateTime
        Date = Time = new DateTime(year, month, day, hour, minute, second, millisecond);

        // 공통 필드 파싱
        // parse common fields
        ParseCommonFields(data, ref pos);

        // 바코드 파싱 (64바이트)
        // parse barcode (64 bytes)
        Barcode = BinarySpanReader.ReadAsciiString(data, ref pos, BarcodeLength);

        // 그래프 메타데이터 파싱
        // parse graph metadata
        ParseGraphMetadata(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);

        // 데이터 길이 검증
        // validate all data consumed
        if (pos != data.Length)
            // 잘못된 데이터 예외 발생
            // throw unconsumed data exception
            throw new FormatException($"Not all bytes have been consumed. {data.Length - pos} byte(s) remain");
    }

    /// <summary>
    ///     PRO X Last Event 포맷 파싱. 공통 필드는 Event 속성에, PRO X 전용 필드는 ProData에 저장한다.
    ///     parses PRO X Last Event format. common fields go to Event properties, PRO X-exclusive fields go to ProData.
    /// </summary>
    /// <param name="data">PRO X 페이로드 / PRO X payload</param>
    /// <param name="revision">PRO X 리비전 / PRO X revision</param>
    private void ParsePro(ReadOnlySpan<byte> data, int revision) {
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 툴 인덱스 파싱
        // parse tool index
        var tool = BinarySpanReader.ReadUInt16(data, ref pos);
        // 프레임 길이 파싱 (검증용)
        // parse frame length (for validation)
        _ = BinarySpanReader.ReadUInt16(data, ref pos);

        // ASCII 시간 파싱 (20바이트, "yyyy-MM-dd HH:mm:ss")
        // parse ASCII time (20 bytes, "yyyy-MM-dd HH:mm:ss")
        var timeText = BinarySpanReader.ReadAsciiString(data, ref pos, ProTimeLength);
        // 시간 문자열을 DateTime으로 변환
        // convert time string to DateTime
        if (DateTime.TryParseExact(timeText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateTime))
            // 파싱 성공 시 설정
            // set on parse success
            Date = Time = dateTime;

        // 이벤트 ID 파싱 (uint32)
        // parse event ID (uint32)
        Id = BinarySpanReader.ReadUInt32(data, ref pos);

        // 공통 필드 파싱
        // parse common fields
        ParseCommonFields(data, ref pos);

        // ID1~6 이름+값 파싱 (12 × 128바이트)
        // parse ID1~6 name+value (12 × 128 bytes)
        var idNames = new string[IdPairCount];
        var ids     = new string[IdPairCount];
        // ID 쌍 순회
        // iterate ID pairs
        for (var i = 0; i < IdPairCount; i++) {
            // ID 이름 파싱
            // parse ID name
            idNames[i] = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
            // ID 값 파싱
            // parse ID value
            ids[i] = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
        }

        // 바코드를 Id1으로 설정 (직접 연결 호환)
        // set barcode to Id1 (direct connection compatibility)
        Barcode = ids[0];

        // Rev.1 전용 필드 파싱
        // parse Rev.1-exclusive fields
        var jobName  = string.Empty;
        var stepName = string.Empty;
        var toolName = string.Empty;
        var ngCause  = string.Empty;
        // Rev.1 이상인지 확인
        // check if Rev.1 or higher
        if (revision >= 1) {
            // 작업 이름 파싱
            // parse job name
            jobName = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
            // 스텝 이름 파싱
            // parse step name
            stepName = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
            // 툴 이름 파싱
            // parse tool name
            toolName = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
            // NG 사유 파싱
            // parse NG cause
            ngCause = BinarySpanReader.ReadAsciiString(data, ref pos, IdFieldLength);
        }

        // 그래프 메타데이터 파싱
        // parse graph metadata
        ParseGraphMetadata(data, ref pos);

        // 그래프 채널 1 레코드 파싱
        // parse graph channel 1 records
        var ch1 = new float[CountOfChannel1];
        // 채널 1 데이터 순회
        // iterate channel 1 data
        for (var i = 0; i < ch1.Length; i++)
            // 채널 1 샘플 값 읽기
            // read channel 1 sample value
            ch1[i] = BinarySpanReader.ReadSingle(data, ref pos);

        // 그래프 채널 2 레코드 파싱
        // parse graph channel 2 records
        var ch2 = new float[CountOfChannel2];
        // 채널 2 데이터 순회
        // iterate channel 2 data
        for (var i = 0; i < ch2.Length; i++)
            // 채널 2 샘플 값 읽기
            // read channel 2 sample value
            ch2[i] = BinarySpanReader.ReadSingle(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..pos]);

        // ProData 생성
        // create ProData
        Pro = new ProData(revision, tool, ids, idNames, jobName, stepName, toolName, ngCause, ch1, ch2);
    }

    /// <summary>
    ///     직접 연결과 PRO X에서 공유하는 공통 필드를 파싱한다.
    ///     parses common fields shared between direct connection and PRO X.
    /// </summary>
    /// <param name="data">원시 데이터 / raw data</param>
    /// <param name="pos">현재 읽기 위치 (ref) / current read position (ref)</param>
    private void ParseCommonFields(ReadOnlySpan<byte> data, ref int pos) {
        // 체결 소요 시간 파싱
        // parse fastening duration
        FastenTime = BinarySpanReader.ReadUInt16(data, ref pos);
        // 프리셋 번호 파싱
        // parse preset number
        Preset = BinarySpanReader.ReadUInt16(data, ref pos);

        // 토크 단위 읽기
        // read torque unit
        var unit = BinarySpanReader.ReadUInt16(data, ref pos);
        // 정의된 값인지 확인
        // check if defined
        if (unit <= (int)Unit.LbfFt)
            // 토크 단위 설정
            // set torque unit
            TorqueUnit = (Unit)unit;

        // 남은 스크류 카운트 파싱
        // parse remaining screw count
        RemainScrew = BinarySpanReader.ReadUInt16(data, ref pos);

        // 회전 방향 읽기
        // read rotation direction
        var dir = BinarySpanReader.ReadUInt16(data, ref pos);
        // 정의된 값인지 확인
        // check if defined
        if (dir <= (int)Direction.Loosening)
            // 회전 방향 설정
            // set rotation direction
            Direction = (Direction)dir;

        // 에러 코드 파싱
        // parse error code
        Error = BinarySpanReader.ReadUInt16(data, ref pos);

        // 이벤트 상태 읽기
        // read event status
        var status = BinarySpanReader.ReadUInt16(data, ref pos);
        // 직접 연결 범위 (0~9) 또는 PRO X 범위 (100~109)인지 확인
        // check if in direct range (0~9) or PRO X range (100~109)
        if (status <= (int)EventType.ScrewCountReset || status is >= 100 and <= 109)
            // 이벤트 상태 설정
            // set event status
            EventStatus = (EventType)status;

        // 목표 토크 파싱
        // parse target torque
        TargetTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 실측 토크 파싱
        // parse actual torque
        Torque = BinarySpanReader.ReadSingle(data, ref pos);
        // 착좌 토크 파싱
        // parse seating torque
        SeatingTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 클램프 토크 파싱
        // parse clamp torque
        ClampTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 프리베일링 토크 파싱
        // parse prevailing torque
        PrevailingTorque = BinarySpanReader.ReadSingle(data, ref pos);
        // 스너그 토크 파싱
        // parse snug torque
        SnugTorque = BinarySpanReader.ReadSingle(data, ref pos);

        // 모터 속도 파싱
        // parse motor speed
        Speed = BinarySpanReader.ReadUInt16(data, ref pos);
        // 각도 1 파싱 (스너그 전)
        // parse angle 1 (before snug)
        Angle1 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 각도 2 파싱 (스너그 후)
        // parse angle 2 (after snug)
        Angle2 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 총 각도 파싱
        // parse total angle
        Angle = BinarySpanReader.ReadUInt16(data, ref pos);
        // 스너그 각도 파싱
        // parse snug angle
        SnugAngle = BinarySpanReader.ReadUInt16(data, ref pos);

        // 예약 영역 건너뛰기 (16바이트)
        // skip reserved area (16 bytes)
        pos += 16;
    }

    /// <summary>
    ///     그래프 메타데이터를 파싱한다 (채널 타입, 포인트 수, 샘플링 주기, 스텝 정보).
    ///     parses graph metadata (channel types, point counts, sampling rate, step info).
    /// </summary>
    /// <param name="data">원시 데이터 / raw data</param>
    /// <param name="pos">현재 읽기 위치 (ref) / current read position (ref)</param>
    private void ParseGraphMetadata(ReadOnlySpan<byte> data, ref int pos) {
        // 그래프 채널 1 타입 읽기
        // read graph channel 1 type
        var type1 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 그래프 채널 2 타입 읽기
        // read graph channel 2 type
        var type2 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 채널 1 타입이 정의된 값인지 확인
        // check if channel 1 type is defined
        if (type1 <= (int)GraphChannel.TorqueAngle)
            // 채널 1 타입 설정
            // set channel 1 type
            TypeOfChannel1 = (GraphChannel)type1;
        // 채널 2 타입이 정의된 값인지 확인
        // check if channel 2 type is defined
        if (type2 <= (int)GraphChannel.TorqueAngle)
            // 채널 2 타입 설정
            // set channel 2 type
            TypeOfChannel2 = (GraphChannel)type2;

        // 채널 1 데이터 포인트 수 파싱
        // parse channel 1 data point count
        CountOfChannel1 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 채널 2 데이터 포인트 수 파싱
        // parse channel 2 data point count
        CountOfChannel2 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 샘플링 주기 파싱
        // parse sampling rate
        SamplingRate = BinarySpanReader.ReadUInt16(data, ref pos);

        // 그래프 스텝 배열 파싱 (16개)
        // parse graph step array (16 entries)
        var steps = new GraphStepInfo[MaxGraphSteps];
        // 스텝 정보 순회
        // iterate step information
        for (var i = 0; i < MaxGraphSteps; i++) {
            // 스텝 타입 읽기
            // read step type
            var id = BinarySpanReader.ReadUInt16(data, ref pos);
            // 스텝 인덱스 읽기
            // read step index
            var index = BinarySpanReader.ReadUInt16(data, ref pos);
            // 정의된 값인지 확인하여 설정
            // set if defined value
            if (id <= (int)GraphStep.RotationAfterTorqueUp)
                // 스텝 정보 저장
                // store step info
                steps[i] = new GraphStepInfo((GraphStep)id, index);
        }

        // 스텝 배열 저장
        // store step array
        GraphSteps = steps;
    }

    /// <summary>
    ///     원시 데이터에서 이벤트를 파싱합니다. revision이 음수이면 직접 연결, 0 이상이면 PRO X 포맷.
    ///     attempts to parse an event from raw data. negative revision = direct, >= 0 = PRO X format.
    /// </summary>
    /// <param name="data">원시 이벤트 데이터 / raw event data</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <param name="revision">PRO X 리비전 (음수=직접 연결) / PRO X revision (negative=direct connection)</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, out Event? result, int revision = -1) {
        // 최소 크기 결정
        // determine minimum size
        var minSize = revision < 0 ? Size : ProData.Sizes[revision];
        // 데이터 크기 확인
        // check data size
        if (data.Length < minSize) {
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: 비정상 데이터의 파싱 예외 방지
        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new Event(data, revision);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // 크기 검증을 통과했으나 파싱에 실패한 비정상 데이터
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

    /// <summary>
    ///     그래프 스텝 정보 (체결 과정의 단계별 정보)
    ///     graph step information (step-by-step fastening process info)
    /// </summary>
    /// <param name="Type">스텝 타입 (체결 단계 식별자) / step type (fastening phase identifier)</param>
    /// <param name="Index">그래프 데이터의 시작 인덱스 / start index in graph data</param>
    public readonly record struct GraphStepInfo(GraphStep Type, int Index);

    /// <summary>
    ///     PRO X Last Event 확장 데이터. 직접 연결에 없는 PRO X 전용 필드를 보관한다.
    ///     PRO X Last Event extension data. holds PRO X-exclusive fields absent in direct connection.
    /// </summary>
    public sealed class ProData {
        /// <summary>
        ///     ProData 인스턴스를 생성합니다. ParsePro에서만 호출됩니다.
        ///     creates a ProData instance. called only from ParsePro.
        /// </summary>
        internal ProData(
            int      revision,
            int      tool,
            string[] ids,
            string[] idNames,
            string   jobName,
            string   stepName,
            string   toolName,
            string   ngCause,
            float[]  graphChannel1,
            float[]  graphChannel2) {
            // 리비전 저장
            // store revision
            Revision = revision;
            // 툴 인덱스 저장
            // store tool index
            Tool = tool;
            // ID 값 저장
            // store ID values
            Id1 = ids[0];
            Id2 = ids[1];
            Id3 = ids[2];
            Id4 = ids[3];
            Id5 = ids[4];
            Id6 = ids[5];
            // ID 이름 저장
            // store ID names
            IdName1 = idNames[0];
            IdName2 = idNames[1];
            IdName3 = idNames[2];
            IdName4 = idNames[3];
            IdName5 = idNames[4];
            IdName6 = idNames[5];
            // Rev.1 전용 필드 저장
            // store Rev.1-exclusive fields
            JobName  = jobName;
            StepName = stepName;
            ToolName = toolName;
            NgCause  = ngCause;
            // 그래프 레코드 저장
            // store graph records
            GraphChannel1 = graphChannel1;
            GraphChannel2 = graphChannel2;
        }

        /// <summary>
        ///     리비전별 최소 페이로드 크기 (바이트, 그래프 레코드 제외).
        ///     Rev.0=1702, Rev.1=2214.
        ///     minimum payload size per revision (bytes, excluding graph records).
        ///     Rev.0=1702, Rev.1=2214.
        /// </summary>
        public static ReadOnlySpan<int> Sizes => [1702, 2214];

        /// <summary>
        ///     확장 데이터 리비전
        ///     extension data revision
        /// </summary>
        public int Revision { get; }

        /// <summary>
        ///     툴 인덱스 (0~)
        ///     tool index (0~)
        /// </summary>
        public int Tool { get; }

        /// <summary>
        ///     ID1 값
        ///     ID1 value
        /// </summary>
        public string Id1 { get; }

        /// <summary>
        ///     ID1 이름
        ///     ID1 name
        /// </summary>
        public string IdName1 { get; }

        /// <summary>
        ///     ID2 값
        ///     ID2 value
        /// </summary>
        public string Id2 { get; }

        /// <summary>
        ///     ID2 이름
        ///     ID2 name
        /// </summary>
        public string IdName2 { get; }

        /// <summary>
        ///     ID3 값
        ///     ID3 value
        /// </summary>
        public string Id3 { get; }

        /// <summary>
        ///     ID3 이름
        ///     ID3 name
        /// </summary>
        public string IdName3 { get; }

        /// <summary>
        ///     ID4 값
        ///     ID4 value
        /// </summary>
        public string Id4 { get; }

        /// <summary>
        ///     ID4 이름
        ///     ID4 name
        /// </summary>
        public string IdName4 { get; }

        /// <summary>
        ///     ID5 값
        ///     ID5 value
        /// </summary>
        public string Id5 { get; }

        /// <summary>
        ///     ID5 이름
        ///     ID5 name
        /// </summary>
        public string IdName5 { get; }

        /// <summary>
        ///     ID6 값
        ///     ID6 value
        /// </summary>
        public string Id6 { get; }

        /// <summary>
        ///     ID6 이름
        ///     ID6 name
        /// </summary>
        public string IdName6 { get; }

        /// <summary>
        ///     작업 이름 (Rev.1 전용, Rev.0에서는 빈 문자열)
        ///     job name (Rev.1 only, empty string for Rev.0)
        /// </summary>
        public string JobName { get; }

        /// <summary>
        ///     스텝 이름 (Rev.1 전용, Rev.0에서는 빈 문자열)
        ///     step name (Rev.1 only, empty string for Rev.0)
        /// </summary>
        public string StepName { get; }

        /// <summary>
        ///     툴 이름 (Rev.1 전용, Rev.0에서는 빈 문자열)
        ///     tool name (Rev.1 only, empty string for Rev.0)
        /// </summary>
        public string ToolName { get; }

        /// <summary>
        ///     NG 사유 (Rev.1 전용, Rev.0에서는 빈 문자열)
        ///     NG cause (Rev.1 only, empty string for Rev.0)
        /// </summary>
        public string NgCause { get; }

        /// <summary>
        ///     그래프 채널 1 레코드 (최대 1,000 포인트)
        ///     graph channel 1 records (up to 1,000 points)
        /// </summary>
        [Browsable(false)]
        public float[] GraphChannel1 { get; }

        /// <summary>
        ///     그래프 채널 2 레코드 (최대 1,000 포인트)
        ///     graph channel 2 records (up to 1,000 points)
        /// </summary>
        [Browsable(false)]
        public float[] GraphChannel2 { get; }

        /// <summary>
        ///     지정한 리비전의 최소 페이로드 크기를 반환합니다.
        ///     returns minimum payload size for the specified revision.
        /// </summary>
        /// <param name="revision">리비전 번호 / revision number</param>
        /// <returns>최소 데이터 크기 (바이트) / minimum data size (bytes)</returns>
        public static int SizeOf(int revision) {
            // 지정 리비전의 크기 반환
            // return size for given revision
            return Sizes[revision];
        }
    }
}