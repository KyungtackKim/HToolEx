using HTool.Core.Type.Process;
using HTool.Format.Process;
using Tester.TestHelpers;
using EventType = HTool.Core.Type.Process.Event;

namespace Tester.Format.Process;

/// <summary>
///     Analysis 블록(64B) 파싱 동작을 검증한다.
///     verifies the parsing behavior of the Analysis block (64B).
/// </summary>
public sealed class AnalysisTests {
    /// <summary>
    ///     주어진 enum 코드들로 64B 분석 블록을 구성한다.
    ///     builds a 64B analysis block with the given enum codes.
    /// </summary>
    private static byte[] BuildAnalysisBytes(ushort unit, ushort dir, ushort status, ushort syncId = 0) {
        // 64바이트 분석 블록 구성
        // construct the 64-byte analysis block
        return new ByteBuilder()
            .UInt16BigEndian(1500)         // FastenTime
            .UInt16BigEndian(5)            // Preset
            .UInt16BigEndian(unit)         // Unit
            .UInt16BigEndian(10)           // RemainScrew
            .UInt16BigEndian(dir)          // Direction
            .UInt16BigEndian(0)            // Error
            .UInt16BigEndian(status)       // EventStatus
            .SingleBigEndian(100.0f)       // TargetTorque
            .SingleBigEndian(95.5f)        // Torque
            .SingleBigEndian(80.0f)        // SeatingTorque
            .SingleBigEndian(90.0f)        // ClampTorque
            .SingleBigEndian(5.0f)         // PrevailingTorque
            .SingleBigEndian(50.0f)        // SnugTorque
            .UInt16BigEndian(300)          // Speed
            .UInt16BigEndian(100)          // Angle1
            .UInt16BigEndian(200)          // Angle2
            .UInt16BigEndian(300)          // Angle
            .UInt16BigEndian(50)           // SnugAngle
            .Zeros(14)                     // reserved (14B)
            .UInt16BigEndian(syncId)       // SyncId (2B, Rev.1-only)
            .Build();
    }

    /// <summary>
    ///     유효한 데이터에서 모든 필드가 올바르게 파싱되는지 검증한다.
    ///     verifies all fields are parsed correctly from valid data.
    /// </summary>
    [Fact]
    public void ParsesAllFields() {
        // 유효 데이터 생성 (Nm=2, Fastening=0, FastenOk=1)
        // build valid data (Nm=2, Fastening=0, FastenOk=1)
        var data = BuildAnalysisBytes((ushort)Unit.Nm, (ushort)Direction.Fastening, (ushort)EventType.FastenOk);
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;
        // 블록 파싱
        // parse the block
        var a = new Analysis(data, ref pos);

        // pos가 정확히 64로 전진했는지 확인
        // verify pos advanced exactly to 64
        Assert.Equal(64, pos);
        // FastenTime 값 확인
        // verify FastenTime value
        Assert.Equal(1500, a.FastenTime);
        // Preset 값 확인
        // verify Preset value
        Assert.Equal(5, a.Preset);
        // TorqueUnit 값 확인
        // verify TorqueUnit value
        Assert.Equal(Unit.Nm, a.TorqueUnit);
        // Direction 값 확인
        // verify Direction value
        Assert.Equal(Direction.Fastening, a.Direction);
        // EventStatus 값 확인
        // verify EventStatus value
        Assert.Equal(EventType.FastenOk, a.EventStatus);
        // TargetTorque 값 확인
        // verify TargetTorque value
        Assert.Equal(100.0f, a.TargetTorque);
        // Torque 값 확인
        // verify Torque value
        Assert.Equal(95.5f, a.Torque);
        // Speed 값 확인
        // verify Speed value
        Assert.Equal(300, a.Speed);
        // Angle 값 확인
        // verify Angle value
        Assert.Equal(300, a.Angle);
    }

    /// <summary>
    ///     정의 범위 밖의 enum 원시값은 기본값으로 클램프되는지 검증한다.
    ///     verifies that out-of-range enum raw values are clamped to default.
    /// </summary>
    [Fact]
    public void ClampsUndefinedEnumsToDefault() {
        // 모든 enum을 범위 밖 값으로 설정
        // set all enums to out-of-range values
        var data = BuildAnalysisBytes(999, 999, 999);
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;
        // 블록 파싱
        // parse the block
        var a = new Analysis(data, ref pos);

        // 정의되지 않은 unit은 기본값(KgfCm)으로 클램프
        // undefined unit clamps to default (KgfCm)
        Assert.Equal(default, a.TorqueUnit);
        // 정의되지 않은 direction은 기본값으로 클램프
        // undefined direction clamps to default
        Assert.Equal(default, a.Direction);
        // 정의되지 않은 status는 기본값으로 클램프
        // undefined status clamps to default
        Assert.Equal(default, a.EventStatus);
    }

    /// <summary>
    ///     예약 영역 마지막 2바이트가 SyncId로 파싱되는지 검증한다 (Rev.1 페이로드 시뮬레이션).
    ///     verifies the last 2 bytes of the reserved area parse into SyncId (simulating a Rev.1 payload).
    /// </summary>
    [Fact]
    public void ParsesSyncIdFromTailBytes() {
        // Rev.1 시뮬레이션을 위한 SyncId=0x1234 데이터 생성
        // build data with SyncId=0x1234 to simulate a Rev.1 payload
        var data = BuildAnalysisBytes((ushort)Unit.Nm, (ushort)Direction.Fastening, (ushort)EventType.FastenOk,
            syncId: 0x1234);
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;
        // 블록 파싱
        // parse the block
        var a = new Analysis(data, ref pos);

        // pos가 정확히 64로 전진했는지 확인
        // verify pos advanced exactly to 64
        Assert.Equal(64, pos);
        // SyncId가 페이로드와 일치하는지 확인
        // verify SyncId matches the payload
        Assert.Equal(0x1234, a.SyncId);
    }

    /// <summary>
    ///     예약 영역이 모두 0인 페이로드(Rev.0 시뮬레이션)에서 SyncId가 0인지 검증한다.
    ///     verifies SyncId is 0 when the reserved area is all zeros (simulating a Rev.0 payload).
    /// </summary>
    [Fact]
    public void Rev0Payload_SyncIdIsZero() {
        // Rev.0 시뮬레이션 (기본 syncId=0)
        // simulate Rev.0 (default syncId=0)
        var data = BuildAnalysisBytes((ushort)Unit.Nm, (ushort)Direction.Fastening, (ushort)EventType.FastenOk);
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;
        // 블록 파싱
        // parse the block
        var a = new Analysis(data, ref pos);

        // Rev.0이므로 SyncId는 0이어야 함
        // SyncId must be 0 on Rev.0
        Assert.Equal(0, a.SyncId);
    }

    /// <summary>
    ///     PRO X 전용 status 범위(100~109)도 허용되는지 검증한다.
    ///     verifies that the PRO X-only status range (100~109) is accepted.
    /// </summary>
    [Fact]
    public void AcceptsProXStatusRange() {
        // PRO X JobOk(102) 코드 설정
        // set PRO X JobOk (102) status code
        var data = BuildAnalysisBytes((ushort)Unit.Nm, (ushort)Direction.Fastening, 102);
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;
        // 블록 파싱
        // parse the block
        var a = new Analysis(data, ref pos);

        // PRO X JobOk이 그대로 저장되는지 확인
        // verify PRO X JobOk is preserved
        Assert.Equal(EventType.JobOk, a.EventStatus);
    }
}
