using System.ComponentModel;
using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace HTool.Format.Process;

/// <summary>
///     그래프 메타데이터 블록 (74바이트). 채널 타입·포인트 수·샘플링 주기·스텝 정보를 담는다.
///     graph metadata block (74 bytes). holds channel types, point counts, sampling rate, and step info.
/// </summary>
/// <remarks>
///     이벤트(0x65)·고해상도 그래프(0x66)·PRO X 고해상도가 공유한다. 커브 데이터 자체는 포함하지 않으며,
///     커브 샘플 수는 <see cref="CountOfChannel1" />/<see cref="CountOfChannel2" />로 제공한다.
///     shared by event (0x65), high-res graph (0x66), and PRO X high-res. does not contain the curve data itself;
///     curve sample counts are provided via <see cref="CountOfChannel1" />/<see cref="CountOfChannel2" />.
/// </remarks>
public readonly struct GraphMeta {
    /// <summary>
    ///     최대 그래프 스텝 수
    ///     maximum graph step count
    /// </summary>
    private const int MaxGraphSteps = 16;

    /// <summary>
    ///     그래프 메타데이터 블록의 고정 크기 (바이트)
    ///     fixed size of the graph metadata block (bytes)
    /// </summary>
    public static int Size => 74;

    /// <summary>
    ///     주어진 위치에서 그래프 메타데이터를 파싱하고 위치를 74바이트 전진시킨다.
    ///     parses the graph metadata at the given position and advances it by 74 bytes.
    /// </summary>
    /// <param name="data">원시 데이터 / raw data</param>
    /// <param name="pos">현재 읽기 위치 (ref) / current read position (ref)</param>
    public GraphMeta(ReadOnlySpan<byte> data, ref int pos) {
        // 채널 1 타입 원시값 읽기
        // read raw channel 1 type value
        var type1 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 채널 2 타입 원시값 읽기
        // read raw channel 2 type value
        var type2 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 정의된 범위면 채널 1 타입 설정, 아니면 기본값
        // set channel 1 type when within defined range, else default
        TypeOfChannel1 = type1 <= (int)GraphChannel.TorqueAngle ? (GraphChannel)type1 : default;
        // 정의된 범위면 채널 2 타입 설정, 아니면 기본값
        // set channel 2 type when within defined range, else default
        TypeOfChannel2 = type2 <= (int)GraphChannel.TorqueAngle ? (GraphChannel)type2 : default;

        // 채널 1 데이터 포인트 수 파싱
        // parse channel 1 data point count
        CountOfChannel1 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 채널 2 데이터 포인트 수 파싱
        // parse channel 2 data point count
        CountOfChannel2 = BinarySpanReader.ReadUInt16(data, ref pos);
        // 샘플링 주기 파싱
        // parse sampling rate
        SamplingRate = BinarySpanReader.ReadUInt16(data, ref pos);

        // 스텝 배열 할당 (16개)
        // allocate step array (16 entries)
        var steps = new GraphStepInfo[MaxGraphSteps];
        // 스텝 정보 순회
        // iterate step information
        for (var i = 0; i < MaxGraphSteps; i++) {
            // 스텝 타입 원시값 읽기
            // read raw step type value
            var id = BinarySpanReader.ReadUInt16(data, ref pos);
            // 스텝 시작 인덱스 읽기
            // read step start index
            var index = BinarySpanReader.ReadUInt16(data, ref pos);
            // 정의된 범위가 아니면 건너뛰기 (기본값 유지)
            // skip when out of defined range (keep default entry)
            if (id > (int)GraphStep.RotationAfterTorqueUp)
                continue;
            // 스텝 정보 저장
            // store step info
            steps[i] = new GraphStepInfo((GraphStep)id, index);
        }

        // 스텝 배열 저장
        // store step array
        GraphSteps = steps;
    }

    /// <summary>
    ///     그래프 채널 1 데이터 타입
    ///     graph channel 1 data type
    /// </summary>
    public GraphChannel TypeOfChannel1 { get; }

    /// <summary>
    ///     그래프 채널 2 데이터 타입
    ///     graph channel 2 data type
    /// </summary>
    public GraphChannel TypeOfChannel2 { get; }

    /// <summary>
    ///     그래프 채널 1 데이터 포인트 수
    ///     graph channel 1 data point count
    /// </summary>
    public int CountOfChannel1 { get; }

    /// <summary>
    ///     그래프 채널 2 데이터 포인트 수
    ///     graph channel 2 data point count
    /// </summary>
    public int CountOfChannel2 { get; }

    /// <summary>
    ///     그래프 샘플링 주기 (밀리초)
    ///     graph sampling rate (milliseconds)
    /// </summary>
    public int SamplingRate { get; }

    /// <summary>
    ///     그래프 스텝 정보 배열 (최대 16개)
    ///     graph step information array (max 16 steps)
    /// </summary>
    [Browsable(false)]
    public GraphStepInfo[] GraphSteps { get; }
}
