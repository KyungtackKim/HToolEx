namespace HTool.Format.Process;

/// <summary>
///     이벤트/그래프 데이터의 출처 구분. 리비전 축이 다르므로 소비자가 출처를 식별할 수 있게 한다.
///     source of event/graph data. lets consumers distinguish the origin, since the revision axis differs.
/// </summary>
public enum GraphSource {
    /// <summary>
    ///     직접 연결 (RTU/TCP) — direct-tool 펌웨어 리비전
    ///     direct connection (RTU/TCP) — direct-tool firmware revision
    /// </summary>
    Direct,

    /// <summary>
    ///     PRO X 게이트웨이 가공본 — PRO X 펌웨어 리비전
    ///     PRO X gateway-processed — PRO X firmware revision
    /// </summary>
    Pro
}
