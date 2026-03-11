using System.Globalization;
using HTool.Core.Type.Pro;
using HTool.Core.Util;
using JobEventType = HTool.Core.Type.Pro.JobEvent;

namespace HTool.Format.Pro;

/// <summary>
///     Pro X 작업 이벤트 데이터 (1960바이트 고정). 작업/스텝 완료 시 발생하는 이벤트 정보를 담습니다.
///     Pro X job event data (fixed 1960 bytes). contains event information generated upon job/step completion.
/// </summary>
/// <remarks>
///     날짜/시간, 이벤트 유형, 작업/스텝 정보, ID 필드 6개, NG 원인 등을 포함합니다.
///     includes date/time, event type, job/step info, 6 ID fields, NG cause, etc.
/// </remarks>
public sealed class JobEvent {
	/// <summary>
	///     원시 패킷 데이터에서 작업 이벤트를 파싱합니다.
	///     parses job event from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <exception cref="FormatException">데이터 길이가 맞지 않을 때 / when data length does not match</exception>
	public JobEvent(ReadOnlySpan<byte> data) {
        // 데이터 크기 확인
        // ensure data length matches required size
        if (data.Length != Size)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} does not match required {Size} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 날짜/시간 텍스트 읽기 (20바이트 ASCII)
        // read date/time text (20 bytes ASCII)
        var dateText = BinarySpanReader.ReadAsciiString(data, ref pos, 20);
        // 날짜/시간 파싱
        // parse date/time
        if (DateTime.TryParseExact(dateText, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateTime))
            // 파싱된 DateTime 설정
            // set parsed DateTime
            DateTime = dateTime;

        // 이벤트 타입 바이트 읽기
        // read event type byte
        var eventType = BinarySpanReader.ReadByte(data, ref pos);
        // enum에 정의된 값인지 확인
        // check if value is defined in enum
        if (EnumUtil.IsDefined<JobEventType>(eventType))
            // 이벤트 타입 설정
            // set event type
            EventType = (JobEventType)eventType;

        // 작업 번호 읽기
        // read job number
        JobNo = BinarySpanReader.ReadUInt16(data, ref pos);
        // 작업 이름 읽기 (128바이트 ASCII)
        // read job name (128 bytes ASCII)
        JobName = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // 전체 작업 스크류 수 읽기
        // read total job screw count
        TotalJobScrews = BinarySpanReader.ReadUInt16(data, ref pos);
        // 현재 작업 스크류 수 읽기
        // read current job screw count
        JobScrews = BinarySpanReader.ReadUInt16(data, ref pos);
        // 전체 스텝 수 읽기
        // read total step count
        TotalStepCount = BinarySpanReader.ReadUInt16(data, ref pos);
        // 스텝 번호 읽기
        // read step number
        StepNo = BinarySpanReader.ReadUInt16(data, ref pos);
        // 스텝 이름 읽기 (128바이트 ASCII)
        // read step name (128 bytes ASCII)
        StepName = BinarySpanReader.ReadAsciiString(data, ref pos, 128);

        // 스텝 타입 바이트 읽기
        // read step type byte
        var stepType = BinarySpanReader.ReadByte(data, ref pos);
        // enum에 정의된 값인지 확인
        // check if value is defined in enum
        if (EnumUtil.IsDefined<JobStep>(stepType))
            // 스텝 타입 설정
            // set step type
            StepType = (JobStep)stepType;

        // 전체 스텝 스크류 수 읽기
        // read total step screw count
        TotalStepScrews = BinarySpanReader.ReadUInt16(data, ref pos);
        // 현재 스텝 스크류 수 읽기
        // read current step screw count
        StepScrews = BinarySpanReader.ReadUInt16(data, ref pos);

        // ID 이름 1 읽기 (128바이트 ASCII)
        // read ID name 1 (128 bytes ASCII)
        IdName1 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 1 읽기 (128바이트 ASCII)
        // read ID 1 (128 bytes ASCII)
        Id1 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 이름 2 읽기 (128바이트 ASCII)
        // read ID name 2 (128 bytes ASCII)
        IdName2 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 2 읽기 (128바이트 ASCII)
        // read ID 2 (128 bytes ASCII)
        Id2 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 이름 3 읽기 (128바이트 ASCII)
        // read ID name 3 (128 bytes ASCII)
        IdName3 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 3 읽기 (128바이트 ASCII)
        // read ID 3 (128 bytes ASCII)
        Id3 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 이름 4 읽기 (128바이트 ASCII)
        // read ID name 4 (128 bytes ASCII)
        IdName4 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 4 읽기 (128바이트 ASCII)
        // read ID 4 (128 bytes ASCII)
        Id4 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 이름 5 읽기 (128바이트 ASCII)
        // read ID name 5 (128 bytes ASCII)
        IdName5 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 5 읽기 (128바이트 ASCII)
        // read ID 5 (128 bytes ASCII)
        Id5 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 이름 6 읽기 (128바이트 ASCII)
        // read ID name 6 (128 bytes ASCII)
        IdName6 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // ID 6 읽기 (128바이트 ASCII)
        // read ID 6 (128 bytes ASCII)
        Id6 = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // NG 원인 읽기 (128바이트 ASCII)
        // read NG cause (128 bytes ASCII)
        NgCause = BinarySpanReader.ReadAsciiString(data, ref pos, 128);

        // 이벤트 고유 ID 읽기
        // read unique event ID
        Id = BinarySpanReader.ReadUInt32(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     이벤트 데이터의 고정 크기 (바이트)
	///     fixed event data size (bytes)
	/// </summary>
	public static int Size => 1960;

	/// <summary>
	///     이벤트 발생 일시
	///     event date and time
	/// </summary>
	public DateTime DateTime { get; }

	/// <summary>
	///     이벤트 타입 (스텝 진입, OK, NG 등)
	///     event type (step into, OK, NG, etc.)
	/// </summary>
	public JobEventType EventType { get; }

	/// <summary>
	///     작업 번호
	///     job number
	/// </summary>
	public ushort JobNo { get; }

	/// <summary>
	///     작업 이름
	///     job name
	/// </summary>
	public string JobName { get; }

	/// <summary>
	///     전체 작업 스크류 수
	///     total job screw count
	/// </summary>
	public ushort TotalJobScrews { get; }

	/// <summary>
	///     현재 작업 스크류 수
	///     current job screw count
	/// </summary>
	public ushort JobScrews { get; }

	/// <summary>
	///     전체 스텝 수
	///     total step count
	/// </summary>
	public ushort TotalStepCount { get; }

	/// <summary>
	///     스텝 번호
	///     step number
	/// </summary>
	public ushort StepNo { get; }

	/// <summary>
	///     스텝 이름
	///     step name
	/// </summary>
	public string StepName { get; }

	/// <summary>
	///     스텝 타입 (체결, 입력, 출력, 지연, 메시지)
	///     step type (fastening, input, output, delay, message)
	/// </summary>
	public JobStep StepType { get; }

	/// <summary>
	///     전체 스텝 스크류 수
	///     total step screw count
	/// </summary>
	public ushort TotalStepScrews { get; }

	/// <summary>
	///     현재 스텝 스크류 수
	///     current step screw count
	/// </summary>
	public ushort StepScrews { get; }

	/// <summary>
	///     ID 이름 1
	///     ID name 1
	/// </summary>
	public string IdName1 { get; }

	/// <summary>
	///     ID 1
	///     ID 1
	/// </summary>
	public string Id1 { get; }

	/// <summary>
	///     ID 이름 2
	///     ID name 2
	/// </summary>
	public string IdName2 { get; }

	/// <summary>
	///     ID 2
	///     ID 2
	/// </summary>
	public string Id2 { get; }

	/// <summary>
	///     ID 이름 3
	///     ID name 3
	/// </summary>
	public string IdName3 { get; }

	/// <summary>
	///     ID 3
	///     ID 3
	/// </summary>
	public string Id3 { get; }

	/// <summary>
	///     ID 이름 4
	///     ID name 4
	/// </summary>
	public string IdName4 { get; }

	/// <summary>
	///     ID 4
	///     ID 4
	/// </summary>
	public string Id4 { get; }

	/// <summary>
	///     ID 이름 5
	///     ID name 5
	/// </summary>
	public string IdName5 { get; }

	/// <summary>
	///     ID 5
	///     ID 5
	/// </summary>
	public string Id5 { get; }

	/// <summary>
	///     ID 이름 6
	///     ID name 6
	/// </summary>
	public string IdName6 { get; }

	/// <summary>
	///     ID 6
	///     ID 6
	/// </summary>
	public string Id6 { get; }

	/// <summary>
	///     NG 원인
	///     NG cause
	/// </summary>
	public string NgCause { get; }

	/// <summary>
	///     이벤트 고유 ID
	///     unique event ID
	/// </summary>
	public uint Id { get; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	public ulong Hash { get; }

	/// <summary>
	///     원시 데이터에서 작업 이벤트를 파싱합니다.
	///     attempts to parse job event from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out JobEvent? result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length != Size) {
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new JobEvent(data);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }
}