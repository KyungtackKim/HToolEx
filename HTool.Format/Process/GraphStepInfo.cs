using HTool.Core.Type.Process;

namespace HTool.Format.Process;

/// <summary>
///     그래프 스텝 정보. 체결 과정의 각 단계와 그래프 데이터 내 시작 인덱스를 담는다.
///     graph step information. holds a fastening phase and its start index within the graph data.
/// </summary>
/// <param name="Type">스텝 타입 (체결 단계 식별자) / step type (fastening phase identifier)</param>
/// <param name="Index">그래프 데이터의 시작 인덱스 / start index in graph data</param>
public readonly record struct GraphStepInfo(GraphStep Type, int Index);