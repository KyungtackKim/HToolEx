namespace HTool.Type;

/// <summary>
///     통신 오류 정보를 담는 레코드 구조체.
///     Record struct containing communication error information.
/// </summary>
/// <param name="Reason">오류 원인 코드 / error reason code</param>
/// <param name="Detail">추가 상세 정보 (선택적) / additional detail (optional)</param>
public readonly record struct ComError(ComErrorCode Reason, string? Detail);