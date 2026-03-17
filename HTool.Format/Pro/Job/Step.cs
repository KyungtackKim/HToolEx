using HTool.Core.Type.Pro;

namespace HTool.Format.Pro.Job;

/// <summary>
///     조합(composition) 기반 스텝 래퍼. 헤더와 본문으로 구성됩니다.
///     composition-based step wrapper. consists of header and body.
/// </summary>
/// <remarks>
///     v2 설계: Step = StepHeader + IStepBody. 상속 없음.
///     v2 design: Step = StepHeader + IStepBody. no inheritance.
/// </remarks>
public sealed class Step {
    /// <summary>
    ///     헤더와 본문으로 스텝을 생성합니다.
    ///     creates a step with header and body.
    /// </summary>
    /// <param name="header">스텝 헤더 / step header</param>
    /// <param name="body">스텝 본문 / step body</param>
    public Step(StepHeader header, IStepBody body) {
        // 헤더 설정
        // set header
        Header = header;
        // 본문 설정
        // set body
        Body = body;
    }

    /// <summary>
    ///     리비전별 스텝 전체 크기 (바이트)
    ///     step total size per revision (bytes)
    /// </summary>
    public static ReadOnlySpan<int> Sizes => [4096, 12288];

    /// <summary>
    ///     스텝 헤더 크기 (바이트)
    ///     step header size (bytes)
    /// </summary>
    public static int HeaderSize => StepHeader.Size;

    /// <summary>
    ///     스텝 헤더
    ///     step header
    /// </summary>
    public StepHeader Header { get; }

    /// <summary>
    ///     스텝 본문
    ///     step body
    /// </summary>
    public IStepBody Body { get; }

    /// <summary>
    ///     원시 데이터에서 스텝을 파싱합니다.
    ///     parses a step from raw data.
    /// </summary>
    /// <param name="data">원시 데이터 스팬 / raw data span</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>파싱된 스텝 / parsed step</returns>
    /// <exception cref="UnknownStepTypeException">알 수 없는 스텝 유형 / unknown step type</exception>
    public static Step Parse(ReadOnlySpan<byte> data, int revision = 0) {
        // 헤더 파싱
        // parse header
        var header = StepHeader.Parse(data);

        // 본문 데이터 슬라이스 (헤더 이후)
        // slice body data (after header)
        var bodyData = data[StepHeader.Size..];

        // 스텝 유형에 따라 본문 파싱
        // parse body by step type
        IStepBody body = header.Type switch {
            // 체결 스텝 파싱
            // parse fastening step
            JobStep.Fastening => FastenBody.Parse(bodyData, revision),
            // 입력 스텝 파싱
            // parse input step
            JobStep.Input => InputBody.Parse(bodyData, revision),
            // 출력 스텝 파싱
            // parse output step
            JobStep.Output => OutputBody.Parse(bodyData, revision),
            // 지연 스텝 파싱
            // parse delay step
            JobStep.Delay => DelayBody.Parse(bodyData, revision),
            // 메시지 스텝 파싱
            // parse message step
            JobStep.Message => MessageBody.Parse(bodyData, revision),
            // 알 수 없는 스텝 유형
            // unknown step type
            _ => throw new UnknownStepTypeException((int)header.Type)
        };

        // 파싱된 스텝 반환
        // return parsed step
        return new Step(header, body);
    }

    /// <summary>
    ///     원시 데이터에서 스텝 파싱을 시도합니다.
    ///     attempts to parse a step from raw data.
    /// </summary>
    /// <param name="data">원시 데이터 스팬 / raw data span</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <param name="error">오류 유형 / error type</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Step? result, out JobError error) {
        // 리비전 범위 확인
        // verify revision range
        if (revision < 0 || revision >= Sizes.Length) {
            // 기본값 설정
            // set null result
            result = null;
            // 오류 유형 설정
            // set error type
            error = JobError.InvalidStep;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // 데이터 크기 확인
        // verify data size
        if (data.Length < Sizes[revision]) {
            // 기본값 설정
            // set null result
            result = null;
            // 오류 유형 설정
            // set error type
            error = JobError.InvalidStep;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed step data
        try {
            // 스텝 파싱
            // parse step
            result = Parse(data, revision);
            // 오류 초기화
            // clear error
            error = JobError.None;
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (UnknownStepTypeException) {
            // unknown step type in binary data
            // 기본값 설정
            // set null result
            result = null;
            // 오류 유형 설정
            // set error type
            error = JobError.InvalidStep;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set null result
            result = null;
            // 오류 유형 설정
            // set error type
            error = JobError.InvalidStep;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

    /// <summary>
    ///     스텝을 바이트 배열로 직렬화합니다.
    ///     serializes step to a byte array.
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>직렬화된 바이트 배열 (고정 크기) / serialized byte array (fixed size)</returns>
    public byte[] ToBytes(int revision = 0) {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= Sizes.Length)
            // 리비전 기본 값 설정
            // reset the revision
            revision = 0;

        // 고정 크기 결과 배열 할당
        // allocate fixed-size result array
        var result = new byte[Sizes[revision]];

        // 헤더 직렬화
        // serialize header
        var headerBytes = Header.ToBytes();
        // 헤더 바이트 복사
        // copy header bytes
        Array.Copy(headerBytes, 0, result, 0, headerBytes.Length);

        // 본문 직렬화
        // serialize body
        var bodyBytes = Body.ToBytes(revision);
        // 본문 바이트 복사
        // copy body bytes
        Array.Copy(bodyBytes, 0, result, StepHeader.Size, Math.Min(bodyBytes.Length, result.Length - StepHeader.Size));

        // 직렬화된 배열 반환
        // return serialized array
        return result;
    }
}