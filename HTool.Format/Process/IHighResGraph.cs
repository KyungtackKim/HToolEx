namespace HTool.Format.Process;

/// <summary>
///     고해상도 그래프 계약. 분석 공통 필드(<see cref="IFastenEvent" />)에 채널 커브 데이터를 더한다.
///     high-res graph contract. adds channel curve data on top of the analysis common fields (<see cref="IFastenEvent" />).
/// </summary>
/// <remarks>
///     직접 고해상도(<c>HTool.Format.Process.HighResGraph</c>)와 PRO X 고해상도(<c>HTool.Format.Pro.ProHighResGraph</c>)가
///     구현하므로, 두 출처를 모두 다루는 소비자는 이 인터페이스로 통일해 소비할 수 있다.
///     implemented by direct high-res (<c>HTool.Format.Process.HighResGraph</c>) and PRO X high-res
///     (<c>HTool.Format.Pro.ProHighResGraph</c>), so consumers handling both can consume uniformly via this interface.
/// </remarks>
public interface IHighResGraph : IFastenEvent {
    /// <summary>
    ///     채널 1 커브 데이터 (샘플 수 = <see cref="IFastenEvent.Meta" />.CountOfChannel1)
    ///     channel 1 curve data (sample count = <see cref="IFastenEvent.Meta" />.CountOfChannel1)
    /// </summary>
    float[] Channel1 { get; }

    /// <summary>
    ///     채널 2 커브 데이터 (샘플 수 = <see cref="IFastenEvent.Meta" />.CountOfChannel2)
    ///     channel 2 curve data (sample count = <see cref="IFastenEvent.Meta" />.CountOfChannel2)
    /// </summary>
    float[] Channel2 { get; }
}
