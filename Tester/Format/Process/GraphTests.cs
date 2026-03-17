using HTool.Format.Process;
using Tester.TestHelpers;

namespace Tester.Format.Process;

/// <summary>
///     Graph 구조체의 파싱 동작을 검증한다.
///     verifies parsing behavior of the Graph struct.
/// </summary>
public sealed class GraphTests {
    /// <summary>
    ///     유효한 그래프 데이터로 채널과 카운트를 올바르게 파싱하는지 검증한다.
    ///     verifies that Channel and Count are correctly parsed from valid graph data.
    /// </summary>
    [Fact]
    public void Constructor_ValidData_ParsesChannelAndCountCorrectly() {
        // 채널=1, 카운트=2, 값=1.5f, 2.5f 의 그래프 데이터 생성
        // create graph data with channel=1, count=2, values=1.5f, 2.5f
        var data = new ByteBuilder()
            .UInt16BigEndian(1)
            .UInt16BigEndian(2)
            .SingleBigEndian(1.5f)
            .SingleBigEndian(2.5f)
            .Build();

        // Graph 구조체 파싱
        // parse Graph struct
        var graph = new Graph(data);

        // Channel 값이 1인지 확인
        // verify Channel value is 1
        Assert.Equal(1, graph.Channel);
        // Count 값이 2인지 확인
        // verify Count value is 2
        Assert.Equal(2, graph.Count);
    }

    /// <summary>
    ///     복수 값을 가진 그래프에서 float 배열이 올바르게 파싱되는지 검증한다.
    ///     verifies that float values array is correctly parsed from a multi-value graph.
    /// </summary>
    [Fact]
    public void Constructor_MultipleValues_ParsesValuesArrayCorrectly() {
        // 채널=0, 카운트=3, 3개의 float 값을 포함하는 그래프 데이터 생성
        // create graph data with channel=0, count=3, containing 3 float values
        var data = new ByteBuilder()
            .UInt16BigEndian(0)
            .UInt16BigEndian(3)
            .SingleBigEndian(10.0f)
            .SingleBigEndian(20.5f)
            .SingleBigEndian(30.25f)
            .Build();

        // Graph 구조체 파싱
        // parse Graph struct
        var graph = new Graph(data);

        // Values 배열 길이가 3인지 확인
        // verify Values array length is 3
        Assert.Equal(3, graph.Values.Length);
        // 첫 번째 값이 10.0인지 확인
        // verify first value is 10.0
        Assert.Equal(10.0f, graph.Values[0]);
        // 두 번째 값이 20.5인지 확인
        // verify second value is 20.5
        Assert.Equal(20.5f, graph.Values[1]);
        // 세 번째 값이 30.25인지 확인
        // verify third value is 30.25
        Assert.Equal(30.25f, graph.Values[2]);
    }

    /// <summary>
    ///     카운트가 0인 빈 그래프가 정상적으로 파싱되는지 검증한다.
    ///     verifies that an empty graph with count=0 is parsed successfully.
    /// </summary>
    [Fact]
    public void Constructor_EmptyGraph_ParsesWithZeroValues() {
        // 채널=0, 카운트=0의 빈 그래프 데이터 생성 (헤더 4바이트만)
        // create empty graph data with channel=0, count=0 (header only, 4 bytes)
        var data = new ByteBuilder()
            .UInt16BigEndian(0)
            .UInt16BigEndian(0)
            .Build();

        // Graph 구조체 파싱
        // parse Graph struct
        var graph = new Graph(data);

        // Count가 0인지 확인
        // verify Count is 0
        Assert.Equal(0, graph.Count);
        // Values 배열이 비어있는지 확인
        // verify Values array is empty
        Assert.Empty(graph.Values);
    }

    /// <summary>
    ///     헤더 크기 미만의 데이터에서 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when data is smaller than header size.
    /// </summary>
    [Fact]
    public void Constructor_UndersizedData_ThrowsFormatException() {
        // 3바이트 배열 생성 (최소 4바이트 미만)
        // create 3-byte array (below minimum 4 bytes)
        var data = new byte[3];

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new Graph(data));
    }

    /// <summary>
    ///     페이로드 길이가 일치하지 않을 때 FormatException이 발생하는지 검증한다.
    ///     verifies that FormatException is thrown when payload length does not match count.
    /// </summary>
    [Fact]
    public void Constructor_MismatchedPayloadLength_ThrowsFormatException() {
        // 카운트=2이지만 float 1개만 포함하는 데이터 생성 (8바이트 부족)
        // create data with count=2 but only 1 float value (8 bytes instead of 12)
        var data = new ByteBuilder()
            .UInt16BigEndian(0)
            .UInt16BigEndian(2)
            .SingleBigEndian(1.0f)
            .Build();

        // FormatException 발생 확인
        // verify FormatException is thrown
        Assert.Throws<FormatException>(() => new Graph(data));
    }

    /// <summary>
    ///     TryParse가 부족한 데이터에서 false를 반환하는지 검증한다.
    ///     verifies that TryParse returns false for undersized data.
    /// </summary>
    [Fact]
    public void TryParse_UndersizedData_ReturnsFalse() {
        // 2바이트 배열 생성 (최소 4바이트 미만)
        // create 2-byte array (below minimum 4 bytes)
        var data = new byte[2];

        // TryParse가 false를 반환하는지 확인
        // verify TryParse returns false
        var success = Graph.TryParse(data, out _);

        // 파싱 실패 확인
        // verify parsing failed
        Assert.False(success);
    }
}