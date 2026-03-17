using HTool.Core.Type.Pro;

namespace HTool.Format.Pro.Job;

/// <summary>
///     Job 관리 클래스. 헤더와 스텝 목록을 포함하며, 파싱/직렬화/CRUD를 제공합니다.
///     job manager class. contains header and step list, provides parsing/serialization/CRUD.
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. 시그니처 "bmc.job."로 시작합니다.
///     little-endian binary format. starts with signature "bmc.job.".
/// </remarks>
public sealed class Job {
	/// <summary>
	///     최대 스텝 수
	///     maximum step count
	/// </summary>
	public const int MaxStep = 255;

	/// <summary>
	///     헤더와 스텝으로 Job을 생성합니다.
	///     creates a job with header and steps.
	/// </summary>
	/// <param name="header">Job 헤더 / job header</param>
	/// <param name="steps">스텝 목록 / step list</param>
	private Job(JobHeader header, List<Step> steps) {
        // 헤더 설정
        // set header
        Header = header;
        // 스텝 목록 설정
        // set step list
        Steps = steps;
    }

	/// <summary>
	///     Job 헤더
	///     job header
	/// </summary>
	public JobHeader Header { get; }

	/// <summary>
	///     스텝 목록
	///     step list
	/// </summary>
	public List<Step> Steps { get; }

	/// <summary>
	///     현재 스텝 수
	///     current step count
	/// </summary>
	public int StepCount => Steps.Count;

	/// <summary>
	///     체결 스텝 수
	///     fastening step count
	/// </summary>
	public int FastenCount => Steps.Count(s => s.Header.Type is JobStep.Fastening);

	/// <summary>
	///     원시 데이터에서 Job을 파싱합니다.
	///     parses a job from raw data.
	/// </summary>
	/// <param name="data">원시 데이터 스팬 / raw data span</param>
	/// <returns>파싱된 Job / parsed job</returns>
	/// <exception cref="JobParseException">파싱 실패 / parsing failed</exception>
	/// <exception cref="UnknownRevisionException">알 수 없는 리비전 / unknown revision</exception>
	public static Job Parse(ReadOnlySpan<byte> data) {
        // 최소 크기 확인
        // ensure minimum size
        if (data.Length < JobHeader.BaseSize)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new JobParseException($"Data too small: {data.Length} bytes, minimum {JobHeader.BaseSize} required.");

        // 헤더 파싱
        // parse header
        var header = JobHeader.Parse(data);

        // 리비전 결정
        // determine revision
        var revision = header.Revision;
        // 리비전 범위 확인
        // ensure revision range is valid
        if (revision < 0 || revision >= Step.Sizes.Length)
            // 인자 예외 발생
            // throw argument exception
            throw new UnknownRevisionException(header.Major, header.Minor);

        // 스텝 수 유효성 확인
        // ensure step count is valid
        if (header.CountOfStep is < 0 or > MaxStep)
            // 잘못된 스텝 수 예외 발생
            // throw format exception for invalid step count
            throw new JobParseException($"Invalid step count: {header.CountOfStep}, max {MaxStep}.");

        // 헤더 크기 결정
        // determine header size
        var headerLen = JobHeader.GetLength(header.Major, header.Minor);
        // 스텝 크기 결정
        // determine step size
        var stepSize = Step.Sizes[revision];

        // 전체 데이터 크기 확인
        // verify total data size
        var totalRequired = headerLen + stepSize * header.CountOfStep;
        // 크기 부족 확인
        // ensure size is sufficient
        if (data.Length < totalRequired)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new JobParseException($"Data too small for {header.CountOfStep} steps: {data.Length} bytes, need {totalRequired}.");

        // 스텝 목록 생성
        // create step list
        var steps = new List<Step>(header.CountOfStep);

        // 스텝 읽기 시작 위치
        // step reading start position
        var pos = headerLen;

        // 각 스텝 파싱
        // parse each step
        for (var i = 0; i < header.CountOfStep; i++) {
            // 스텝 데이터 슬라이스
            // slice step data
            var stepData = data.Slice(pos, stepSize);
            // 스텝 파싱
            // parse step
            var step = Step.Parse(stepData, revision);
            // 스텝 추가
            // add step
            steps.Add(step);
            // 위치 전진
            // advance position
            pos += stepSize;
        }

        // 파싱된 Job 반환
        // return parsed job
        return new Job(header, steps);
    }

	/// <summary>
	///     원시 데이터에서 Job 파싱을 시도합니다.
	///     attempts to parse a job from raw data.
	/// </summary>
	/// <param name="data">원시 데이터 스팬 / raw data span</param>
	/// <param name="job">파싱 결과 / parsed result</param>
	/// <param name="error">오류 유형 / error type</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out Job? job, out JobError error) {
        // 최소 크기 확인
        // verify minimum size
        if (data.Length < JobHeader.BaseSize) {
            // 크기 부족 오류
            // file too small error
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.FileTooSmall;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // 시그니처 검증을 위한 헤더 파싱 시도
        // try header parse for signature verification
        JobHeader header;
        // guard: catch signature or parsing errors
        try {
            // 헤더 파싱
            // parse header
            header = JobHeader.Parse(data);
        } catch (JobParseException ex) when (ex.Message.Contains("signature", StringComparison.OrdinalIgnoreCase)) {
            // invalid file signature
            // 결과 초기화
            // set null result
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.InvalidSignature;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        } catch (Exception) {
            // malformed header data
            // 결과 초기화
            // set null result
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.FileTooSmall;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // 리비전 범위 확인
        // verify revision range
        if (header.Revision < 0 || header.Revision >= Step.Sizes.Length) {
            // 알 수 없는 버전 오류
            // unknown version error
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.UnknownVersion;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // 스텝 수 유효성 확인
        // validate step count
        if (header.CountOfStep is < 0 or > MaxStep) {
            // 잘못된 스텝 수 오류
            // invalid step count error
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.InvalidStepCount;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any remaining parsing errors
        try {
            // 전체 Job 파싱
            // parse full job
            job = Parse(data);
            // 성공 코드 설정
            // set success error code
            error = JobError.None;
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (UnknownStepTypeException) {
            // unknown step type in binary data
            // 결과 초기화
            // set null result
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.InvalidStep;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        } catch (UnknownRevisionException) {
            // unknown revision in binary data
            // 결과 초기화
            // set null result
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.UnknownVersion;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        } catch (Exception) {
            // malformed data that passed initial checks but failed parsing
            // 결과 초기화
            // set null result
            job = null;
            // 오류 코드 설정
            // set error code
            error = JobError.InvalidStep;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

	/// <summary>
	///     Job을 바이트 배열로 직렬화합니다.
	///     serializes job to a byte array.
	/// </summary>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] Save() {
        // 리비전 결정
        // determine revision
        var revision = Header.Revision;

        // 헤더 카운트 동기화
        // sync header counts
        SyncHeaderCounts();

        // 결과 목록 초기화
        // initialize result list
        var result = new List<byte>();

        // 헤더 직렬화
        // serialize header
        result.AddRange(Header.ToBytes());

        // 각 스텝 직렬화
        // serialize each step
        foreach (var step in Steps)
            // 스텝 직렬화
            // serialize step
            result.AddRange(step.ToBytes(revision));

        // 직렬화된 배열 반환
        // return serialized array
        return result.ToArray();
    }

	/// <summary>
	///     스텝을 목록 끝에 추가합니다.
	///     adds a step to the end of the list.
	/// </summary>
	/// <param name="step">추가할 스텝 / step to add</param>
	/// <exception cref="JobSerializeException">최대 스텝 수 초과 / maximum step count exceeded</exception>
	public void Add(Step step) {
        // 최대 스텝 수 확인
        // ensure maximum step count is not exceeded
        if (Steps.Count >= MaxStep)
            // 인자 예외 발생
            // throw argument exception
            throw new JobSerializeException($"Cannot add step: maximum {MaxStep} steps reached.");
        // 스텝 추가
        // add step
        Steps.Add(step);
    }

	/// <summary>
	///     지정 위치에 스텝을 삽입합니다.
	///     inserts a step at the specified index.
	/// </summary>
	/// <param name="index">삽입 위치 / insert position</param>
	/// <param name="step">삽입할 스텝 / step to insert</param>
	/// <exception cref="JobSerializeException">최대 스텝 수 초과 / maximum step count exceeded</exception>
	public void InsertAt(int index, Step step) {
        // 최대 스텝 수 확인
        // ensure maximum step count is not exceeded
        if (Steps.Count >= MaxStep)
            // 인자 예외 발생
            // throw argument exception
            throw new JobSerializeException($"Cannot insert step: maximum {MaxStep} steps reached.");
        // 스텝 삽입
        // insert step
        Steps.Insert(index, step);
    }

	/// <summary>
	///     지정 위치의 스텝을 제거합니다.
	///     removes the step at the specified index.
	/// </summary>
	/// <param name="index">제거 위치 / remove position</param>
	public void RemoveAt(int index) {
        // 스텝 제거
        // remove step
        Steps.RemoveAt(index);
    }

	/// <summary>
	///     스텝을 다른 위치로 이동합니다.
	///     moves a step to a different position.
	/// </summary>
	/// <param name="from">원래 위치 / source position</param>
	/// <param name="to">대상 위치 / target position</param>
	public void Move(int from, int to) {
        // 이동할 스텝 가져오기
        // get step to move
        var step = Steps[from];
        // 원래 위치에서 제거
        // remove from source position
        Steps.RemoveAt(from);
        // 대상 위치에 삽입
        // insert at target position
        Steps.Insert(to, step);
    }

	/// <summary>
	///     두 스텝의 위치를 교환합니다.
	///     swaps two steps.
	/// </summary>
	/// <param name="a">첫 번째 위치 / first position</param>
	/// <param name="b">두 번째 위치 / second position</param>
	public void Swap(int a, int b) {
        // 임시 변수에 저장
        // store in temporary variable
        (Steps[a], Steps[b]) = (Steps[b], Steps[a]);
    }

	/// <summary>
	///     지정 위치의 스텝을 깊은 복사합니다.
	///     creates a deep copy of the step at the specified index.
	/// </summary>
	/// <param name="index">복사할 위치 / position to copy</param>
	/// <returns>복사된 스텝 / copied step</returns>
	public Step Copy(int index) {
        // 리비전 결정
        // determine revision
        var revision = Header.Revision;
        // 원본 직렬화 후 재파싱 (깊은 복사)
        // serialize and re-parse original (deep copy)
        var bytes = Steps[index].ToBytes(revision);
        // 재파싱하여 반환
        // re-parse and return
        return Step.Parse(bytes, revision);
    }

	/// <summary>
	///     지정 위치의 스텝을 교체합니다.
	///     replaces the step at the specified index.
	/// </summary>
	/// <param name="index">교체 위치 / replace position</param>
	/// <param name="step">새 스텝 / new step</param>
	public void ReplaceAt(int index, Step step) {
        // 스텝 교체
        // replace step
        Steps[index] = step;
    }

	/// <summary>
	///     헤더의 카운트 필드를 현재 스텝 목록과 동기화합니다.
	///     synchronizes header count fields with the current step list.
	/// </summary>
	private void SyncHeaderCounts() {
        // 총 스텝 수 동기화
        // sync total step count
        Header.CountOfStep = Steps.Count;
        // 체결 스텝 수 동기화
        // sync fastening step count
        Header.CountOfFasten = Steps.Count(s => s.Header.Type is JobStep.Fastening);
        // 입력 스텝 수 동기화
        // sync input step count
        Header.CountOfInput = Steps.Count(s => s.Header.Type is JobStep.Input);
        // 출력 스텝 수 동기화
        // sync output step count
        Header.CountOfOutput = Steps.Count(s => s.Header.Type is JobStep.Output);
        // 지연 스텝 수 동기화
        // sync delay step count
        Header.CountOfDelay = Steps.Count(s => s.Header.Type is JobStep.Delay);
        // 메시지 스텝 수 동기화
        // sync message step count
        Header.CountOfMessage = Steps.Count(s => s.Header.Type is JobStep.Message);
        // 총 나사 수 동기화 (체결 스텝의 나사 수 합산)
        // sync total screw count (sum of screw counts from fastening steps)
        Header.CountOfScrew = Steps.Where(s => s.Header.Type is JobStep.Fastening).Sum(s => ((FastenBody)s.Body).CountOfScrew);
    }
}