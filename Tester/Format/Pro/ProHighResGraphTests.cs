using HTool.Core.Type.Process;
using HTool.Format.Pro;
using HTool.Format.Process;
using Tester.TestHelpers;
using EventType = HTool.Core.Type.Process.Event;

namespace Tester.Format.Pro;

/// <summary>
///     ProHighResGraph(PRO X 고해상도) 파싱 동작을 검증한다. Rev.0/Rev.1 분기와 PRO X 전용 필드(Tool/IdPairs/Names)가 핵심이다.
///     verifies ProHighResGraph (PRO X high-res) parsing. the key concerns are Rev.0/Rev.1 branching and PRO X-only fields
///     (Tool/IdPairs/Names).
/// </summary>
public sealed class ProHighResGraphTests {
    /// <summary>
    ///     주어진 리비전·툴 인덱스로 PRO X 본문(커브 제외)을 구성한다. 커브는 호출자가 별도로 이어붙인다.
    ///     builds the PRO X body (excluding curves) for the given revision and tool index. caller appends curves.
    /// </summary>
    private static ByteBuilder BuildProBody(int revision, short tool) {
        // 빌더 생성
        // create builder
        var b = new ByteBuilder()
            // Tool(2) + Length(2) + Date ASCII(20) + Id(4) = 28B
            // Tool(2) + Length(2) + Date ASCII(20) + Id(4) = 28B
            .Int16BigEndian(tool)             // Tool
            .UInt16BigEndian(0)               // Length (informational)
            .Ascii("2026-05-28 14:30:45", 20) // Date ASCII
            .UInt32BigEndian(123456789u)      // Id (uint32)
            // 분석 64B
            // analysis 64B
            .UInt16BigEndian(2000).UInt16BigEndian(1).UInt16BigEndian((ushort)Unit.Nm)
            .UInt16BigEndian(5).UInt16BigEndian((ushort)Direction.Fastening).UInt16BigEndian(0)
            .UInt16BigEndian((ushort)EventType.JobOk)
            .SingleBigEndian(150f).SingleBigEndian(148f).SingleBigEndian(140f)
            .SingleBigEndian(145f).SingleBigEndian(3f).SingleBigEndian(60f)
            .UInt16BigEndian(400).UInt16BigEndian(50).UInt16BigEndian(80)
            .UInt16BigEndian(130).UInt16BigEndian(25)
            .Zeros(14)           // reserved (14B)
            .UInt16BigEndian(0); // SyncId (2B)
        // ID 쌍 6세트 (Name + Value 각 128B = 12 × 128 = 1536B)
        // ID pairs 6 sets (name + value, 128B each = 12 × 128 = 1536B)
        for (var i = 1; i <= 6; i++) {
            // ID 이름 추가
            // append ID name
            b.Ascii($"NAME{i}", 128);
            // ID 값 추가
            // append ID value
            b.Ascii($"VALUE{i}", 128);
        }

        // Rev.1 전용 이름 4종 (512B)
        // Rev.1-only 4 names (512B)
        if (revision >= 1) {
            // 작업/스텝/툴/NG 이름 추가
            // append job/step/tool/NG cause names
            b.Ascii("JOB-A", 128);
            b.Ascii("STEP-1", 128);
            b.Ascii("TOOL-X", 128);
            b.Ascii("OK", 128);
        }

        // 그래프 메타 74B (커브 0/0으로 단순화)
        // graph meta 74B (curves 0/0 for simplicity)
        b.UInt16BigEndian((ushort)GraphChannel.Torque).UInt16BigEndian((ushort)GraphChannel.Angle)
            .UInt16BigEndian(0).UInt16BigEndian(0).UInt16BigEndian(2)
            .Zeros(16 * 4);
        // 빌더 반환
        // return the builder
        return b;
    }

    /// <summary>
    ///     Rev.0 본문이 정확히 1702B이며 PRO X 전용 필드가 노출되는지 검증한다.
    ///     verifies the Rev.0 body is exactly 1702B and PRO X-only fields are exposed.
    /// </summary>
    [Fact]
    public void Rev0_ParsesFixedBodyAndProOnlyFields() {
        // Rev.0 본문 (커브 0)
        // Rev.0 body (no curves)
        var data = BuildProBody(0, 3).Build();
        // 본문 크기 검증
        // verify body size
        Assert.Equal(1702, data.Length);

        // 파싱
        // parse
        var pro = new ProHighResGraph(data, 0);

        // 인터페이스 위임
        // interface delegation
        Assert.Equal(123456789u, pro.Id);
        Assert.Equal(0, pro.Revision);
        Assert.Equal(GraphSource.Pro, pro.Source);
        Assert.Equal(new DateTime(2026, 5, 28, 14, 30, 45), pro.Time);
        // 분석 위임
        // analysis delegation
        Assert.Equal(2000, pro.Analysis.FastenTime);
        Assert.Equal(EventType.JobOk, pro.Analysis.EventStatus);
        // PRO X 전용 필드
        // PRO X-only fields
        Assert.Equal(3, pro.Tool);
        Assert.Equal(6, pro.Ids.Count);
        Assert.Equal("VALUE1", pro.Ids[0]);
        Assert.Equal("VALUE6", pro.Ids[5]);
        Assert.Equal("NAME1", pro.IdNames[0]);
        Assert.Equal("NAME6", pro.IdNames[5]);
        // Rev.0이므로 이름 필드는 빈 문자열
        // Rev.0 — name fields are empty
        Assert.Equal(string.Empty, pro.JobName);
        Assert.Equal(string.Empty, pro.StepName);
        Assert.Equal(string.Empty, pro.ToolName);
        Assert.Equal(string.Empty, pro.NgCause);
        // 커브 0
        // empty curves
        Assert.Empty(pro.Channel1);
        Assert.Empty(pro.Channel2);
    }

    /// <summary>
    ///     Rev.1 본문이 정확히 2214B이며 이름 4종이 채워지는지 검증한다.
    ///     verifies the Rev.1 body is exactly 2214B and the 4 name fields are populated.
    /// </summary>
    [Fact]
    public void Rev1_IncludesNames() {
        // Rev.1 본문 (커브 0)
        // Rev.1 body (no curves)
        var data = BuildProBody(1, 0).Build();
        // 본문 크기 검증 (Rev.0 + 512B)
        // verify body size (Rev.0 + 512B)
        Assert.Equal(2214, data.Length);

        // 파싱
        // parse
        var pro = new ProHighResGraph(data, 1);

        // 리비전 확인
        // verify revision
        Assert.Equal(1, pro.Revision);
        // Rev.1 전용 이름 4종 확인
        // verify the 4 Rev.1-only names
        Assert.Equal("JOB-A", pro.JobName);
        Assert.Equal("STEP-1", pro.StepName);
        Assert.Equal("TOOL-X", pro.ToolName);
        Assert.Equal("OK", pro.NgCause);
    }

    /// <summary>
    ///     Tool 인덱스가 음수(-1, JOB 이벤트)도 허용되는지 검증한다.
    ///     verifies that a negative Tool index (-1, JOB event) is accepted.
    /// </summary>
    [Fact]
    public void NegativeTool_AcceptedAsJobEvent() {
        // Tool=-1 (JOB 이벤트)
        // Tool=-1 (JOB event)
        var data = BuildProBody(0, -1).Build();
        // 파싱
        // parse
        var pro = new ProHighResGraph(data, 0);

        // Tool=-1 확인
        // verify Tool=-1
        Assert.Equal(-1, pro.Tool);
    }

    /// <summary>
    ///     범위 밖 리비전은 ArgumentOutOfRangeException을 던지는지 검증한다 (호출자 실수).
    ///     verifies an out-of-range revision throws ArgumentOutOfRangeException (caller error).
    /// </summary>
    [Fact]
    public void TryParse_InvalidRevision_Throws() {
        // 더미 데이터
        // dummy data
        var data = new byte[2214];
        // 범위 밖 리비전 → 예외
        // out-of-range revision → exception
        Assert.Throws<ArgumentOutOfRangeException>(() => ProHighResGraph.TryParse(data, out _, 5));
    }

    /// <summary>
    ///     데이터 크기가 부족하면 TryParse가 false를 반환하는지 검증한다.
    ///     verifies TryParse returns false on insufficient data.
    /// </summary>
    [Fact]
    public void TryParse_ShortData_ReturnsFalse() {
        // 부족한 크기 (Rev.0 필요=1702)
        // insufficient size (Rev.0 needs 1702)
        var data = new byte[500];
        // TryParse 호출
        // call TryParse
        var ok = ProHighResGraph.TryParse(data, out var pro, 0);

        // 실패해야 함
        // must fail
        Assert.False(ok);
        // 결과는 기본값
        // result is default
        Assert.Equal(default, pro);
    }
}