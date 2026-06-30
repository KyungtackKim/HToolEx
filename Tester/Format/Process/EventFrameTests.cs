using HTool.Core.Type.Process;
using HTool.Format.Process;
using Tester.TestHelpers;
using EventType = HTool.Core.Type.Process.Event;

namespace Tester.Format.Process;

/// <summary>
///     EventFrame(0x65, 214B) 파싱 동작을 검증한다.
///     verifies EventFrame (0x65, 214B) parsing behavior.
/// </summary>
public sealed class EventFrameTests {
    /// <summary>
    ///     유효한 214B 이벤트 페이로드를 구성한다.
    ///     builds a valid 214B event payload.
    /// </summary>
    private static byte[] BuildEventBytes(string barcode = "BARCODE-001", ushort syncId = 0) {
        // 214바이트 이벤트 페이로드 구성
        // construct the 214-byte event payload
        return new ByteBuilder()
            // 헤더: 리비전(2) + Id(2) + Date(8) = 12B
            // header: revision(2) + Id(2) + Date(8) = 12B
            .Byte(2).Byte(1)                                                             // revision 2.1
            .UInt16BigEndian(12345)                                                      // Id
            .UInt16BigEndian(2026).Byte(5).Byte(28).Byte(14).Byte(30).Byte(45).Byte(123) // date
            // 분석 블록 64B
            // analysis block 64B
            .UInt16BigEndian(1500)                        // FastenTime
            .UInt16BigEndian(5)                           // Preset
            .UInt16BigEndian((ushort)Unit.Nm)             // TorqueUnit
            .UInt16BigEndian(10)                          // RemainScrew
            .UInt16BigEndian((ushort)Direction.Fastening) // Direction
            .UInt16BigEndian(0)                           // Error
            .UInt16BigEndian((ushort)EventType.FastenOk)  // Status
            .SingleBigEndian(100.0f).SingleBigEndian(95.5f).SingleBigEndian(80.0f)
            .SingleBigEndian(90.0f).SingleBigEndian(5.0f).SingleBigEndian(50.0f)
            .UInt16BigEndian(300).UInt16BigEndian(100).UInt16BigEndian(200)
            .UInt16BigEndian(300).UInt16BigEndian(50)
            .Zeros(14)               // reserved (14B)
            .UInt16BigEndian(syncId) // SyncId (2B, Rev.1-only)
            // 바코드 64B
            // barcode 64B
            .Ascii(barcode, 64)
            // 그래프 메타 74B
            // graph meta 74B
            .UInt16BigEndian((ushort)GraphChannel.Torque).UInt16BigEndian((ushort)GraphChannel.Angle)
            .UInt16BigEndian(500).UInt16BigEndian(400).UInt16BigEndian(2)
            .Zeros(16 * 4) // 16 steps (zeros)
            .Build();
    }

    /// <summary>
    ///     유효 데이터에서 헤더·블록이 모두 올바르게 파싱되는지 검증한다.
    ///     verifies all header and block fields are parsed correctly from valid data.
    /// </summary>
    [Fact]
    public void ParsesHeaderAndBlocks() {
        // 유효 데이터 생성
        // build valid data
        var data = BuildEventBytes();
        // EventFrame 파싱
        // parse EventFrame
        var ev = new EventFrame(data);

        // 크기가 정확히 214인지 확인
        // verify size is exactly 214
        Assert.Equal(214, EventFrame.Size);
        // Id 값 확인
        // verify Id
        Assert.Equal(12345u, ev.Id);
        // 리비전 원시값 확인 (2.1 = 0x0201 = 513)
        // verify raw revision (2.1 = 0x0201 = 513)
        Assert.Equal((2 << 8) | 1, ev.Revision);
        // 리비전 표시 문자열 확인
        // verify revision display string
        Assert.Equal("2.1", ev.RevisionText);
        // 발생 시각 확인
        // verify occurrence time
        Assert.Equal(new DateTime(2026, 5, 28, 14, 30, 45, 123), ev.Time);
        // Date 별칭이 Time과 동일한지 확인
        // verify Date alias equals Time
        Assert.Equal(ev.Time, ev.Date);
        // 단일 바코드가 Ids[0]로 노출되는지 확인
        // verify the single barcode is exposed at Ids[0]
        Assert.Equal("BARCODE-001", ev.Barcode);
        Assert.Equal("BARCODE-001", ev.Ids[0]);
        Assert.Single(ev.Ids);
        // 출처가 Direct인지 확인
        // verify source is Direct
        Assert.Equal(GraphSource.Direct, ev.Source);
        // 분석 블록 위임 확인
        // verify Analysis block delegation
        Assert.Equal(1500, ev.Analysis.FastenTime);
        Assert.Equal(95.5f, ev.Analysis.Torque);
        Assert.Equal(EventType.FastenOk, ev.Analysis.EventStatus);
        // 메타 블록 위임 확인
        // verify Meta block delegation
        Assert.Equal(500, ev.Meta.CountOfChannel1);
        Assert.Equal(400, ev.Meta.CountOfChannel2);
        Assert.Equal(GraphChannel.Torque, ev.Meta.TypeOfChannel1);
    }

    /// <summary>
    ///     Rev.1 페이로드에서 Analysis.SyncId가 정상 노출되는지 검증한다.
    ///     verifies Analysis.SyncId is exposed correctly from a Rev.1 payload.
    /// </summary>
    [Fact]
    public void Rev1Payload_ExposesSyncId() {
        // Rev.1 시뮬레이션 페이로드 (SyncId=0xABCD)
        // Rev.1-simulated payload (SyncId=0xABCD)
        var data = BuildEventBytes(syncId: 0xABCD);
        // EventFrame 파싱
        // parse EventFrame
        var ev = new EventFrame(data);

        // 분석 블록에서 SyncId 노출 확인
        // verify SyncId is exposed via the analysis block
        Assert.Equal(0xABCD, ev.Analysis.SyncId);
    }

    /// <summary>
    ///     Rev.0 시뮬레이션(예약 영역 전부 0)에서 SyncId가 0인지 검증한다.
    ///     verifies SyncId is 0 when the reserved area is all zeros (Rev.0 simulation).
    /// </summary>
    [Fact]
    public void Rev0Payload_SyncIdIsZero() {
        // Rev.0 시뮬레이션 (기본 syncId=0)
        // Rev.0 simulation (default syncId=0)
        var data = BuildEventBytes();
        // EventFrame 파싱
        // parse EventFrame
        var ev = new EventFrame(data);

        // Rev.0 페이로드의 SyncId는 0
        // SyncId on a Rev.0 payload must be 0
        Assert.Equal(0, ev.Analysis.SyncId);
    }

    /// <summary>
    ///     크기가 부족한 데이터에서 TryParse가 false를 반환하는지 검증한다.
    ///     verifies TryParse returns false on insufficient data.
    /// </summary>
    [Fact]
    public void TryParse_ShortData_ReturnsFalse() {
        // 부족한 크기의 데이터 (100B < 214B)
        // insufficient data (100B < 214B)
        var data = new byte[100];
        // TryParse 호출
        // call TryParse
        var ok = EventFrame.TryParse(data, out var ev);

        // 실패해야 함
        // must fail
        Assert.False(ok);
        // 결과는 기본값
        // result is default
        Assert.Equal(default, ev);
    }

    /// <summary>
    ///     데이터 크기가 부족할 때 생성자가 FormatException을 던지는지 검증한다.
    ///     verifies the constructor throws FormatException on insufficient data.
    /// </summary>
    [Fact]
    public void Constructor_ShortData_Throws() {
        // 부족한 크기의 데이터
        // insufficient data
        var data = new byte[100];
        // FormatException 예상
        // expect FormatException
        Assert.Throws<FormatException>(() => new EventFrame(data));
    }
}