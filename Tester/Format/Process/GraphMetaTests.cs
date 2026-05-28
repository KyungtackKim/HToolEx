using HTool.Core.Type.Process;
using HTool.Format.Process;
using Tester.TestHelpers;

namespace Tester.Format.Process;

/// <summary>
///     GraphMeta 블록(74B) 파싱 동작을 검증한다.
///     verifies the parsing behavior of the GraphMeta block (74B).
/// </summary>
public sealed class GraphMetaTests {
    /// <summary>
    ///     16개 스텝 슬롯에 첫 2개만 정의값으로 채우고 나머지는 0으로 둔 74B 메타를 구성한다.
    ///     builds a 74B meta with only the first two step slots populated; the remaining slots are zero-filled.
    /// </summary>
    private static byte[] BuildGraphMetaBytes() {
        // 74바이트 메타 블록 구성
        // construct the 74-byte meta block
        var b = new ByteBuilder()
            .UInt16BigEndian((ushort)GraphChannel.Torque)        // TypeOfChannel1
            .UInt16BigEndian((ushort)GraphChannel.Angle)         // TypeOfChannel2
            .UInt16BigEndian(500)                                // CountOfChannel1
            .UInt16BigEndian(400)                                // CountOfChannel2
            .UInt16BigEndian(2)                                  // SamplingRate
            .UInt16BigEndian((ushort)GraphStep.Seating).UInt16BigEndian(10)
            .UInt16BigEndian((ushort)GraphStep.Clamp).UInt16BigEndian(20);
        // 나머지 14개 스텝 슬롯은 0으로 채움
        // zero-fill the remaining 14 step slots
        b.Zeros(14 * 4);
        // 빌드 반환
        // return the built array
        return b.Build();
    }

    /// <summary>
    ///     유효한 데이터에서 채널 정보와 스텝 배열이 올바르게 파싱되는지 검증한다.
    ///     verifies that channel info and step array are parsed correctly from valid data.
    /// </summary>
    [Fact]
    public void ParsesChannelsAndSteps() {
        // 유효 데이터 생성
        // build valid data
        var data = BuildGraphMetaBytes();
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;
        // 블록 파싱
        // parse the block
        var m = new GraphMeta(data, ref pos);

        // pos가 정확히 74로 전진했는지 확인
        // verify pos advanced exactly to 74
        Assert.Equal(74, pos);
        // TypeOfChannel1 값 확인
        // verify TypeOfChannel1
        Assert.Equal(GraphChannel.Torque, m.TypeOfChannel1);
        // TypeOfChannel2 값 확인
        // verify TypeOfChannel2
        Assert.Equal(GraphChannel.Angle, m.TypeOfChannel2);
        // CountOfChannel1 값 확인
        // verify CountOfChannel1
        Assert.Equal(500, m.CountOfChannel1);
        // CountOfChannel2 값 확인
        // verify CountOfChannel2
        Assert.Equal(400, m.CountOfChannel2);
        // SamplingRate 값 확인
        // verify SamplingRate
        Assert.Equal(2, m.SamplingRate);
        // 스텝 배열 길이는 항상 16
        // step array length is always 16
        Assert.Equal(16, m.GraphSteps.Length);
        // 첫 번째 스텝 확인 (Seating, 10)
        // verify first step (Seating, 10)
        Assert.Equal(GraphStep.Seating, m.GraphSteps[0].Type);
        Assert.Equal(10, m.GraphSteps[0].Index);
        // 두 번째 스텝 확인 (Clamp, 20)
        // verify second step (Clamp, 20)
        Assert.Equal(GraphStep.Clamp, m.GraphSteps[1].Type);
        Assert.Equal(20, m.GraphSteps[1].Index);
        // 나머지 슬롯은 기본값 (None, 0)
        // remaining slots are default (None, 0)
        Assert.Equal(GraphStep.None, m.GraphSteps[2].Type);
        Assert.Equal(0, m.GraphSteps[2].Index);
    }
}
