namespace HTool.Format.Process;

/// <summary>
///     체결 이벤트 공통 계약. 직접 이벤트(0x65)와 고해상도 그래프(0x66, PRO X 포함)가 공유하는 분석 데이터를 노출한다.
///     common contract for fastening events. exposes the analysis data shared by the direct event (0x65)
///     and high-res graphs (0x66, incl. PRO X).
/// </summary>
/// <remarks>
///     직접/PRO X 이벤트를 모두 다루는 소비자가 출처(<see cref="Source" />) 무관하게 분석 필드에 접근할 수 있게 한다.
///     커브가 필요한 경우 <see cref="IHighResGraph" />로 확장한다.
///     lets consumers handling both direct and PRO X events access analysis fields regardless of <see cref="Source" />.
///     extend to <see cref="IHighResGraph" /> when curve data is needed.
/// </remarks>
public interface IFastenEvent {
    /// <summary>
    ///     이벤트 고유 ID
    ///     unique event ID
    /// </summary>
    uint Id { get; }

    /// <summary>
    ///     이벤트 발생 시각
    ///     event occurrence time
    /// </summary>
    DateTime Time { get; }

    /// <summary>
    ///     포맷 리비전 (출처별 축이 다름: 직접=펌웨어, PRO X=PRO X 펌웨어)
    ///     format revision (axis differs by source: direct=firmware, PRO X=PRO X firmware)
    /// </summary>
    int Revision { get; }

    /// <summary>
    ///     데이터 출처 (직접 / PRO X)
    ///     data source (direct / PRO X)
    /// </summary>
    GraphSource Source { get; }

    /// <summary>
    ///     체결 분석 공통 필드 블록
    ///     fastening analysis common-field block
    /// </summary>
    Analysis Analysis { get; }

    /// <summary>
    ///     그래프 메타데이터 블록
    ///     graph metadata block
    /// </summary>
    GraphMeta Meta { get; }

    /// <summary>
    ///     ID/바코드 값 목록 (직접=바코드 1개, PRO X=ID1~ID6)
    ///     ID/barcode values (direct=single barcode, PRO X=ID1~ID6)
    /// </summary>
    IReadOnlyList<string> Ids { get; }
}
