using HTool.Core.Type.Process;
using HTool.Format.Process;
using Tester.TestHelpers;
using EventType = HTool.Core.Type.Process.Event;

namespace Tester.Format.Process;

/// <summary>
///     HighResGraph(0x66 직접) 파싱 동작을 검증한다. 임베드된 EventFrame과 헤더 없는 커브의 조합이 핵심이다.
///     verifies HighResGraph (0x66 direct) parsing. the key is the combination of an embedded EventFrame and headerless curves.
/// </summary>
public sealed class HighResGraphTests {
    /// <summary>
    ///     주어진 채널 카운트로 214B 이벤트 본문 + 커브를 구성한다.
    ///     builds a 214B event body + curves with the given channel counts.
    /// </summary>
    private static byte[] BuildHighResBytes(ushort count1, ushort count2) {
        // 빌더 준비
        // prepare builder
        var b = new ByteBuilder()
            // 헤더 12B + 분석 64B + 바코드 64B
            // header 12B + analysis 64B + barcode 64B
            .Byte(2).Byte(0)
            .UInt16BigEndian(7000)
            .UInt16BigEndian(2026).Byte(5).Byte(28).Byte(10).Byte(0).Byte(0).Byte(0)
            .UInt16BigEndian(1200).UInt16BigEndian(3).UInt16BigEndian((ushort)Unit.Nm)
            .UInt16BigEndian(7).UInt16BigEndian((ushort)Direction.Fastening).UInt16BigEndian(0)
            .UInt16BigEndian((ushort)EventType.FastenOk)
            .SingleBigEndian(50f).SingleBigEndian(48f).SingleBigEndian(40f)
            .SingleBigEndian(45f).SingleBigEndian(2f).SingleBigEndian(20f)
            .UInt16BigEndian(250).UInt16BigEndian(80).UInt16BigEndian(120)
            .UInt16BigEndian(200).UInt16BigEndian(40)
            .Zeros(14)                                              // reserved (14B)
            .UInt16BigEndian(0)                                     // SyncId (2B, Rev.0=0)
            .Ascii("HIRES-BC", 64)
            // 그래프 메타 74B (count1/count2 변동)
            // graph meta 74B (count1/count2 variable)
            .UInt16BigEndian((ushort)GraphChannel.Torque).UInt16BigEndian((ushort)GraphChannel.Angle)
            .UInt16BigEndian(count1).UInt16BigEndian(count2).UInt16BigEndian(2)
            .Zeros(16 * 4);
        // 채널 1 커브 (헤더 없는 float 배열)
        // channel 1 curve (headerless float array)
        for (var i = 0; i < count1; i++)
            // 채널 1 샘플 추가
            // add channel 1 sample
            b.SingleBigEndian(i + 0.5f);
        // 채널 2 커브 (헤더 없는 float 배열)
        // channel 2 curve (headerless float array)
        for (var i = 0; i < count2; i++)
            // 채널 2 샘플 추가
            // add channel 2 sample
            b.SingleBigEndian(i + 100.5f);
        // 빌드 반환
        // return the built array
        return b.Build();
    }

    /// <summary>
    ///     이벤트 본문이 임베드되고 커브가 메타 카운트만큼 헤더 없이 읽히는지 검증한다.
    ///     verifies the event body is embedded and curves are read headerlessly per the meta counts.
    /// </summary>
    [Fact]
    public void EmbedsEventAndReadsHeaderlessCurves() {
        // ch1=3, ch2=2 커브 데이터
        // ch1=3, ch2=2 curve data
        var data = BuildHighResBytes(3, 2);
        // HighResGraph 파싱
        // parse HighResGraph
        var hi = new HighResGraph(data);

        // 임베드된 EventFrame 노출 확인
        // verify the embedded EventFrame is exposed
        Assert.Equal(7000u, hi.Event.Id);
        Assert.Equal("HIRES-BC", hi.Event.Barcode);
        // 인터페이스 위임 확인 (Id/Time/Source)
        // verify interface delegation (Id/Time/Source)
        Assert.Equal(7000u, hi.Id);
        Assert.Equal(GraphSource.Direct, hi.Source);
        Assert.Equal(new DateTime(2026, 5, 28, 10, 0, 0), hi.Time);
        // 메타 위임 확인
        // verify Meta delegation
        Assert.Equal(3, hi.Meta.CountOfChannel1);
        Assert.Equal(2, hi.Meta.CountOfChannel2);
        // 커브 길이 — 메타 카운트와 일치해야 함
        // curve lengths — must match the meta counts
        Assert.Equal(3, hi.Channel1.Length);
        Assert.Equal(2, hi.Channel2.Length);
        // 커브 값 확인 (헤더 없이 순차 float)
        // verify curve values (sequential floats without a header)
        Assert.Equal(0.5f, hi.Channel1[0]);
        Assert.Equal(1.5f, hi.Channel1[1]);
        Assert.Equal(2.5f, hi.Channel1[2]);
        Assert.Equal(100.5f, hi.Channel2[0]);
        Assert.Equal(101.5f, hi.Channel2[1]);
    }

    /// <summary>
    ///     커브 데이터가 부족하면 FormatException을 던지는지 검증한다.
    ///     verifies that insufficient curve data throws FormatException.
    /// </summary>
    [Fact]
    public void Constructor_MissingCurveBytes_Throws() {
        // 메타는 ch1=10이라고 선언하지만 실제 커브는 부족
        // meta declares ch1=10 but actual curves are short
        var full = BuildHighResBytes(10, 0);
        // 커브 일부만 포함 (이벤트 본문 214 + 5 float = 234, 필요 = 214 + 10*4 = 254)
        // include only part of the curves (event body 214 + 5 floats = 234, required = 214 + 10*4 = 254)
        var truncated = full.AsSpan(0, 234).ToArray();

        // 부족한 데이터로 파싱 시도
        // attempt parsing with insufficient data
        Assert.Throws<FormatException>(() => new HighResGraph(truncated));
    }

    /// <summary>
    ///     커브가 0개일 때도 정상 파싱되는지 검증한다.
    ///     verifies parsing succeeds even when both channels have zero samples.
    /// </summary>
    [Fact]
    public void ParsesWithZeroCurves() {
        // 양쪽 채널 모두 0개 샘플
        // zero samples on both channels
        var data = BuildHighResBytes(0, 0);
        // 파싱
        // parse
        var hi = new HighResGraph(data);

        // 빈 커브 배열 확인
        // verify empty curve arrays
        Assert.Empty(hi.Channel1);
        Assert.Empty(hi.Channel2);
    }
}
