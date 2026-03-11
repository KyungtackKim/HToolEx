using HTool.Core.Type.Pro;
using HTool.Core.Util;
using HTool.Device.Codec;

namespace Tester.Device.Codec;

/// <summary>
///     ProCodec 단위 테스트.
///     Unit tests for ProCodec.
/// </summary>
public sealed class ProCodecTests {
    #region BuildModbusPassthrough

    /// <summary>
    ///     BuildModbusPassthrough가 MID 110으로 MODBUS 패킷을 래핑하고 Unit ID를 설정하는지 검증한다.
    ///     Verifies BuildModbusPassthrough wraps MODBUS packet with MID 110 and sets Unit ID.
    /// </summary>
    [Fact]
    public void BuildModbusPassthrough_ValidPacket_MID110AndUnitIdSet() {
        // MODBUS TCP 프레임 (12바이트): MBAP(7) + FC(1) + Addr(2) + Count(2)
        // MODBUS TCP frame (12 bytes): MBAP(7) + FC(1) + Addr(2) + Count(2)
        byte[] modbusPacket = [
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x06,
            0x00, // MBAP (UID=0x00)
            0x03, // FC
            0x00,
            0x64,
            0x00,
            0x0A // Addr=100, Count=10
        ];
        // 대상 툴 ID
        // target tool ID
        const byte toolId = 42;

        // MODBUS 패스스루 프레임 생성
        // build MODBUS passthrough frame
        var frame = ProCodec.BuildModbusPassthrough(modbusPacket, toolId);

        // 프레임 길이 확인: 16(헤더) + 12(MODBUS 패킷) = 28
        // verify frame length: 16(header) + 12(MODBUS packet) = 28
        Assert.Equal(28, frame.Length);
        // MID 필드 확인 (Big-Endian: 110 = 0x006E)
        // verify MID field (Big-Endian: 110 = 0x006E)
        var mid = (frame[2] << 8) | frame[3];
        Assert.Equal(110, mid);
        // Unit ID가 toolId로 설정되었는지 확인 (페이로드 내 offset 6 → 전체 offset 22)
        // verify Unit ID is set to toolId (payload offset 6 → total offset 22)
        Assert.Equal(toolId, frame[16 + 6]);
    }

    #endregion

    #region BuildMessage

    /// <summary>
    ///     BuildMessage가 페이로드 없이 16바이트 헤더만 생성하는지 검증한다.
    ///     Verifies BuildMessage produces a 16-byte header-only frame with no payload.
    /// </summary>
    [Fact]
    public void BuildMessage_NoPayload_Returns16ByteHeader() {
        // KeepAlive 메시지 생성 (페이로드 없음)
        // build KeepAlive message (no payload)
        var frame = ProCodec.BuildMessage(MessageId.KeepAlive);

        // 프레임 길이 16바이트 (헤더만)
        // frame length 16 bytes (header only)
        Assert.Equal(16, frame.Length);
        // 전체 길이 필드 확인 (Big-Endian, offset 0-1)
        // verify total length field (Big-Endian, offset 0-1)
        Assert.Equal(0x00, frame[0]);
        Assert.Equal(0x10, frame[1]);
        // MID 필드 확인 (Big-Endian, offset 2-3, KeepAlive=2)
        // verify MID field (Big-Endian, offset 2-3, KeepAlive=2)
        Assert.Equal(0x00, frame[2]);
        Assert.Equal(0x02, frame[3]);
    }

    /// <summary>
    ///     BuildMessage가 페이로드를 포함하여 올바른 전체 길이를 설정하는지 검증한다.
    ///     Verifies BuildMessage sets correct total length including payload.
    /// </summary>
    [Fact]
    public void BuildMessage_WithPayload_IncludesPayloadInLength() {
        // 4바이트 페이로드 생성
        // create 4-byte payload
        byte[] payload = [0x01, 0x02, 0x03, 0x04];

        // MemberToolRequest 메시지 생성 (페이로드 포함)
        // build MemberToolRequest message (with payload)
        var frame = ProCodec.BuildMessage(MessageId.MemberToolRequest, payload: payload);

        // 프레임 길이 20바이트 (16 헤더 + 4 페이로드)
        // frame length 20 bytes (16 header + 4 payload)
        Assert.Equal(20, frame.Length);
        // 전체 길이 필드 확인 (Big-Endian: 0x0014 = 20)
        // verify total length field (Big-Endian: 0x0014 = 20)
        var totalLen = (frame[0] << 8) | frame[1];
        Assert.Equal(20, totalLen);
        // 페이로드 데이터가 오프셋 16부터 시작하는지 확인
        // verify payload data starts at offset 16
        Assert.Equal(0x01, frame[16]);
        Assert.Equal(0x04, frame[19]);
    }

    /// <summary>
    ///     BuildMessage가 리비전을 올바르게 기록하는지 검증한다.
    ///     Verifies BuildMessage writes revision correctly.
    /// </summary>
    [Fact]
    public void BuildMessage_WithRevision_RevisionFieldSet() {
        // 리비전 3으로 메시지 생성
        // build message with revision 3
        var frame = ProCodec.BuildMessage(MessageId.KeepAlive, 3);

        // 리비전 필드 확인 (Big-Endian, offset 4-5)
        // verify revision field (Big-Endian, offset 4-5)
        var revision = (frame[4] << 8) | frame[5];
        Assert.Equal(3, revision);
    }

    /// <summary>
    ///     BuildMessage가 예약 영역을 0으로 채우는지 검증한다.
    ///     Verifies BuildMessage fills reserved area with zeros.
    /// </summary>
    [Fact]
    public void BuildMessage_Always_ReservedAreaZeroed() {
        // 메시지 생성
        // build message
        var frame = ProCodec.BuildMessage(MessageId.KeepAlive);

        // 예약 영역(offset 6~15) 모두 0인지 확인
        // verify reserved area (offset 6~15) is all zeros
        for (var i = 6; i < 16; i++)
            // 각 바이트가 0인지 확인
            // verify each byte is 0
            Assert.Equal(0x00, frame[i]);
    }

    #endregion

    #region CalcFrameLength

    /// <summary>
    ///     CalcFrameLength가 데이터 부족 시 -1을 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns -1 when buffer has insufficient data.
    /// </summary>
    [Fact]
    public void CalcFrameLength_InsufficientData_ReturnsNegative() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // 10바이트만 기록 (16바이트 헤더 미만)
        // write only 10 bytes (less than 16-byte header)
        buffer.WriteBytes(new byte[] { 0x00, 0x10, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        // 프레임 길이 계산
        // calculate frame length
        var length = ProCodec.CalcFrameLength(buffer);

        // 헤더 미만 데이터 시 -1 반환 확인
        // verify returns -1 for data less than header
        Assert.Equal(-1, length);
    }

    /// <summary>
    ///     CalcFrameLength가 유효한 프레임에서 전체 길이를 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns total length for a valid frame.
    /// </summary>
    [Fact]
    public void CalcFrameLength_ValidHeader_ReturnsTotalLength() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // BuildMessage로 정상 프레임 생성
        // build normal frame via BuildMessage
        var frame = ProCodec.BuildMessage(MessageId.KeepAlive);
        // 프레임을 링 버퍼에 기록
        // write frame to ring buffer
        buffer.WriteBytes(frame);

        // 프레임 길이 계산
        // calculate frame length
        var length = ProCodec.CalcFrameLength(buffer);

        // 헤더만 있는 프레임이므로 16 반환
        // returns 16 for header-only frame
        Assert.Equal(16, length);
    }

    /// <summary>
    ///     CalcFrameLength가 길이 값이 16 미만이면 -1을 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns -1 when length value is less than 16.
    /// </summary>
    [Fact]
    public void CalcFrameLength_LengthBelowMinimum_ReturnsNegative() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // 잘못된 길이 값(10 < 16)을 가진 데이터 기록
        // write data with invalid length value (10 < 16)
        buffer.WriteBytes(new byte[] {
            0x00,
            0x0A, // Length = 10 (invalid, < 16)
            0x00,
            0x02,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00
        });

        // 프레임 길이 계산
        // calculate frame length
        var length = ProCodec.CalcFrameLength(buffer);

        // 최소 길이 미만이면 -1 반환
        // returns -1 for below minimum length
        Assert.Equal(-1, length);
    }

    #endregion

    #region ExtractMid / ExtractRevision / ExtractPayload / ExtractModbusPayload

    /// <summary>
    ///     ExtractMid가 프레임에서 올바른 MessageId를 추출하는지 검증한다.
    ///     Verifies ExtractMid extracts the correct MessageId from a frame.
    /// </summary>
    [Fact]
    public void ExtractMid_ValidFrame_ReturnsCorrectMessageId() {
        // MemberToolRequest(10) 메시지 생성
        // build MemberToolRequest(10) message
        var frame = ProCodec.BuildMessage(MessageId.MemberToolRequest);

        // MID 추출
        // extract MID
        var mid = ProCodec.ExtractMid(frame);

        // MemberToolRequest 확인
        // verify MemberToolRequest
        Assert.Equal(MessageId.MemberToolRequest, mid);
    }

    /// <summary>
    ///     ExtractRevision이 프레임에서 올바른 리비전을 추출하는지 검증한다.
    ///     Verifies ExtractRevision extracts the correct revision from a frame.
    /// </summary>
    [Fact]
    public void ExtractRevision_WithRevision_ReturnsCorrectValue() {
        // 리비전 5로 메시지 생성
        // build message with revision 5
        var frame = ProCodec.BuildMessage(MessageId.KeepAlive, 5);

        // 리비전 추출
        // extract revision
        var revision = ProCodec.ExtractRevision(frame);

        // 리비전 5 확인
        // verify revision 5
        Assert.Equal(5, revision);
    }

    /// <summary>
    ///     ExtractPayload가 헤더(16바이트) 이후 페이로드를 반환하는지 검증한다.
    ///     Verifies ExtractPayload returns data after 16-byte header.
    /// </summary>
    [Fact]
    public void ExtractPayload_WithPayload_ReturnsDataAfterHeader() {
        // 4바이트 페이로드 포함 메시지 생성
        // build message with 4-byte payload
        byte[] payload = [0xAA, 0xBB, 0xCC, 0xDD];
        var    frame   = ProCodec.BuildMessage(MessageId.KeepAlive, payload: payload);

        // 페이로드 추출
        // extract payload
        var extracted = ProCodec.ExtractPayload(frame);

        // 페이로드 길이 4 확인
        // verify payload length is 4
        Assert.Equal(4, extracted.Length);
        // 페이로드 데이터 일치 확인
        // verify payload data matches
        Assert.Equal(payload, extracted);
    }

    /// <summary>
    ///     ExtractPayload가 헤더만 있는 프레임에서 빈 배열을 반환하는지 검증한다.
    ///     Verifies ExtractPayload returns empty array for header-only frame.
    /// </summary>
    [Fact]
    public void ExtractPayload_HeaderOnly_ReturnsEmpty() {
        // 페이로드 없는 메시지 생성
        // build message without payload
        var frame = ProCodec.BuildMessage(MessageId.KeepAlive);

        // 페이로드 추출
        // extract payload
        var extracted = ProCodec.ExtractPayload(frame);

        // 빈 배열 반환 확인
        // verify empty array returned
        Assert.Empty(extracted);
    }

    /// <summary>
    ///     ExtractModbusPayload가 PRO 페이로드에서 MBAP(7바이트)를 건너뛰고 FC+Data를 반환하는지 검증한다.
    ///     Verifies ExtractModbusPayload skips MBAP (7 bytes) and returns FC+Data from PRO payload.
    /// </summary>
    [Fact]
    public void ExtractModbusPayload_ValidProPayload_ReturnsFcAndData() {
        // MID 111 응답의 PRO 페이로드 시뮬레이션: MBAP(7) + FC(1) + Data(4)
        // simulate PRO payload of MID 111 response: MBAP(7) + FC(1) + Data(4)
        byte[] proPayload = [
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x07,
            0x01, // MBAP
            0x03, // FC
            0x04,
            0x00,
            0x0A,
            0x00 // Data
        ];

        // MODBUS 페이로드 추출
        // extract MODBUS payload
        var modbusPayload = ProCodec.ExtractModbusPayload(proPayload);

        // FC+Data 길이 확인: 12 - 7 = 5
        // verify FC+Data length: 12 - 7 = 5
        Assert.Equal(5, modbusPayload.Length);
        // 첫 번째 바이트가 FC 0x03인지 확인
        // verify first byte is FC 0x03
        Assert.Equal(0x03, modbusPayload.Span[0]);
    }

    /// <summary>
    ///     ExtractModbusPayload가 최소 크기 미달 시 빈 메모리를 반환하는지 검증한다.
    ///     Verifies ExtractModbusPayload returns empty memory when payload is too short.
    /// </summary>
    [Fact]
    public void ExtractModbusPayload_TooShort_ReturnsEmpty() {
        // 7바이트 미만의 페이로드
        // payload shorter than 7 bytes
        byte[] shortPayload = [0x00, 0x01, 0x02];

        // MODBUS 페이로드 추출
        // extract MODBUS payload
        var result = ProCodec.ExtractModbusPayload(shortPayload);

        // 빈 메모리 반환 확인
        // verify empty memory returned
        Assert.Equal(0, result.Length);
    }

    #endregion
}