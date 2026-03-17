using HTool.Core.Util;
using HTool.Device.Codec;
using HTool.Type;

namespace Tester.Device.Codec;

/// <summary>
///     ModbusTcpCodec 단위 테스트.
///     Unit tests for ModbusTcpCodec.
/// </summary>
public sealed class ModbusTcpCodecTests {
    // 테스트 대상 코덱 인스턴스
    // codec instance under test
    private readonly ModbusTcpCodec _codec = new();

    #region BuildReadInputReg

    /// <summary>
    ///     BuildReadInputReg가 FC 0x04로 12바이트 프레임을 생성하는지 검증한다.
    ///     Verifies BuildReadInputReg produces a 12-byte frame with FC 0x04.
    /// </summary>
    [Fact]
    public void BuildReadInputReg_ValidParams_Returns12ByteFrameWithFC04() {
        // FC 0x04 패킷 생성
        // build FC 0x04 packet
        var frame = _codec.BuildReadInputReg(200, 5, 1);

        // 프레임 길이 12바이트 확인
        // verify frame length is 12 bytes
        Assert.Equal(12, frame.Length);
        // 함수 코드 0x04 확인
        // verify function code 0x04
        Assert.Equal(0x04, frame[7]);
    }

    #endregion

    #region BuildWriteSingleReg

    /// <summary>
    ///     BuildWriteSingleReg가 FC 0x06으로 12바이트 프레임을 생성하고 값을 올바르게 기록하는지 검증한다.
    ///     Verifies BuildWriteSingleReg produces a 12-byte frame with FC 0x06 and correct value.
    /// </summary>
    [Fact]
    public void BuildWriteSingleReg_ValidParams_Returns12ByteFrameWithValue() {
        // FC 0x06 패킷 생성 (주소 50, 값 0x04D2 = 1234)
        // build FC 0x06 packet (address 50, value 0x04D2 = 1234)
        var frame = _codec.BuildWriteSingleReg(50, 1234, 1);

        // 프레임 길이 12바이트 확인
        // verify frame length is 12 bytes
        Assert.Equal(12, frame.Length);
        // 함수 코드 0x06 확인
        // verify function code 0x06
        Assert.Equal(0x06, frame[7]);
        // 값의 상위 바이트 확인 (오프셋 10)
        // verify value high byte (offset 10)
        Assert.Equal(0x04, frame[10]);
        // 값의 하위 바이트 확인 (오프셋 11)
        // verify value low byte (offset 11)
        Assert.Equal(0xD2, frame[11]);
    }

    #endregion

    #region BuildWriteMultiReg

    /// <summary>
    ///     BuildWriteMultiReg가 올바른 길이의 프레임을 생성하는지 검증한다.
    ///     Verifies BuildWriteMultiReg produces a frame with correct length.
    /// </summary>
    [Fact]
    public void BuildWriteMultiReg_ThreeValues_ReturnsCorrectFrame() {
        // 3개 레지스터 값 배열
        // three register values array
        ushort[] values = [0x000A, 0x000B, 0x000C];

        // FC 0x10 패킷 생성
        // build FC 0x10 packet
        var frame = _codec.BuildWriteMultiReg(100, values, 1);

        // 프레임 길이: MBAP(6) + PDU(7 + 6바이트 데이터) = 6 + 13 = 19
        // frame length: MBAP(6) + PDU(7 + 6-byte data) = 6 + 13 = 19
        Assert.Equal(19, frame.Length);
        // 함수 코드 0x10 확인
        // verify function code 0x10
        Assert.Equal(0x10, frame[7]);
        // 바이트 수 필드 확인 (3 레지스터 * 2 바이트 = 6)
        // verify byte count field (3 registers * 2 bytes = 6)
        Assert.Equal(6, frame[12]);
    }

    #endregion

    #region BuildReadInfoReg

    /// <summary>
    ///     BuildReadInfoReg가 8바이트 프레임(FC 0x11)을 생성하는지 검증한다.
    ///     Verifies BuildReadInfoReg produces an 8-byte frame with FC 0x11.
    /// </summary>
    [Fact]
    public void BuildReadInfoReg_ValidDeviceId_Returns8ByteFrame() {
        // FC 0x11 패킷 생성
        // build FC 0x11 packet
        var frame = _codec.BuildReadInfoReg(1);

        // 프레임 길이 8바이트: MBAP(7) + FC(1)
        // frame length 8 bytes: MBAP(7) + FC(1)
        Assert.Equal(8, frame.Length);
        // 함수 코드 0x11 확인
        // verify function code 0x11
        Assert.Equal(0x11, frame[7]);
        // PDU 길이 필드 확인 (UID(1) + FC(1) = 2)
        // verify PDU length field (UID(1) + FC(1) = 2)
        Assert.Equal(0x00, frame[4]);
        Assert.Equal(0x02, frame[5]);
    }

    #endregion

    #region Properties

    /// <summary>
    ///     HeaderSize가 8(MBAP(7) + FC(1))을 반환하는지 검증한다.
    ///     Verifies HeaderSize returns 8 (MBAP(7) + FC(1)).
    /// </summary>
    [Fact]
    public void HeaderSize_Default_Returns8() {
        // 헤더 크기 조회
        // get header size
        var size = _codec.HeaderSize;

        // TCP 헤더는 MBAP(7) + FC(1) = 8
        // TCP header is MBAP(7) + FC(1) = 8
        Assert.Equal(8, size);
    }

    /// <summary>
    ///     FunctionPos가 7을 반환하는지 검증한다.
    ///     Verifies FunctionPos returns 7.
    /// </summary>
    [Fact]
    public void FunctionPos_Default_Returns7() {
        // 함수 코드 위치 조회
        // get function code position
        var pos = _codec.FunctionPos;

        // TCP 프레임에서 FC는 오프셋 7
        // FC is at offset 7 in TCP frame
        Assert.Equal(7, pos);
    }

    #endregion

    #region BuildReadHoldingReg

    /// <summary>
    ///     BuildReadHoldingReg가 12바이트 프레임을 생성하는지 검증한다.
    ///     Verifies BuildReadHoldingReg produces a 12-byte frame.
    /// </summary>
    [Fact]
    public void BuildReadHoldingReg_ValidParams_Returns12ByteFrame() {
        // FC 0x03 패킷 생성 (주소 100, 10 레지스터, 장치 ID 1)
        // build FC 0x03 packet (address 100, 10 registers, device ID 1)
        var frame = _codec.BuildReadHoldingReg(100, 10, 1);

        // 프레임 길이 12바이트: MBAP(7) + FC(1) + Addr(2) + Count(2)
        // frame length 12 bytes: MBAP(7) + FC(1) + Addr(2) + Count(2)
        Assert.Equal(12, frame.Length);
        // 함수 코드 0x03 확인 (위치 7)
        // verify function code 0x03 (position 7)
        Assert.Equal(0x03, frame[7]);
    }

    /// <summary>
    ///     BuildReadHoldingReg가 MBAP 헤더를 올바르게 구성하는지 검증한다.
    ///     Verifies BuildReadHoldingReg constructs MBAP header correctly.
    /// </summary>
    [Fact]
    public void BuildReadHoldingReg_TransactionId_MbapHeaderCorrect() {
        // 트랜잭션 ID 0x1234로 패킷 생성
        // build packet with transaction ID 0x1234
        var frame = _codec.BuildReadHoldingReg(0, 1, 5, 0x1234);

        // 트랜잭션 ID 상위 바이트 확인
        // verify transaction ID high byte
        Assert.Equal(0x12, frame[0]);
        // 트랜잭션 ID 하위 바이트 확인
        // verify transaction ID low byte
        Assert.Equal(0x34, frame[1]);
        // 프로토콜 ID 상위 바이트 확인 (항상 0x00)
        // verify protocol ID high byte (always 0x00)
        Assert.Equal(0x00, frame[2]);
        // 프로토콜 ID 하위 바이트 확인 (항상 0x00)
        // verify protocol ID low byte (always 0x00)
        Assert.Equal(0x00, frame[3]);
        // 길이 필드 상위 바이트 확인 (PDU = 6)
        // verify length field high byte (PDU = 6)
        Assert.Equal(0x00, frame[4]);
        // 길이 필드 하위 바이트 확인
        // verify length field low byte
        Assert.Equal(0x06, frame[5]);
        // Unit ID 확인
        // verify Unit ID
        Assert.Equal(5, frame[6]);
    }

    #endregion

    #region BuildWriteStrReg

    /// <summary>
    ///     BuildWriteStrReg가 문자열을 제로 패딩하여 인코딩하는지 검증한다.
    ///     Verifies BuildWriteStrReg encodes string with zero padding.
    /// </summary>
    [Fact]
    public void BuildWriteStrReg_ShortString_ZeroPaddedAndCorrectLength() {
        // 고정 길이 8바이트로 "Hi" 문자열 쓰기
        // write string "Hi" with fixed length 8 bytes
        var frame = _codec.BuildWriteStrReg(0, "Hi", 8, 1);

        // 프레임 길이: MBAP(6) + PDU(7 + 8바이트 데이터) = 6 + 15 = 21
        // frame length: MBAP(6) + PDU(7 + 8-byte data) = 6 + 15 = 21
        Assert.Equal(21, frame.Length);
        // 함수 코드 0x10 확인
        // verify function code 0x10
        Assert.Equal(0x10, frame[7]);
        // 첫 번째 문자 'H' 확인 (오프셋 13)
        // verify first character 'H' (offset 13)
        Assert.Equal((byte)'H', frame[13]);
        // 두 번째 문자 'i' 확인 (오프셋 14)
        // verify second character 'i' (offset 14)
        Assert.Equal((byte)'i', frame[14]);
        // 세 번째 바이트가 0 (패딩) 확인
        // verify third byte is 0 (padding)
        Assert.Equal(0, frame[15]);
    }

    /// <summary>
    ///     BuildWriteStrReg가 홀수 길이를 짝수로 올림 처리하는지 검증한다.
    ///     Verifies BuildWriteStrReg rounds odd length up to even.
    /// </summary>
    [Fact]
    public void BuildWriteStrReg_OddLength_RoundedUpToEven() {
        // 홀수 길이 7을 지정하여 문자열 쓰기 (→ 8바이트로 올림)
        // write string with odd length 7 (→ rounded to 8 bytes)
        var frame = _codec.BuildWriteStrReg(0, "ABC", 7, 1);

        // 바이트 수 필드가 8 (7 → 짝수 올림)
        // byte count field is 8 (7 → rounded to even)
        Assert.Equal(8, frame[12]);
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
        // 8바이트만 기록 (최소 9바이트 필요)
        // write only 8 bytes (minimum 9 needed)
        buffer.WriteBytes(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03 });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 데이터 부족 시 -1 반환 확인
        // verify returns -1 for insufficient data
        Assert.Equal(-1, length);
    }

    /// <summary>
    ///     CalcFrameLength가 읽기 응답의 프레임 길이를 올바르게 계산하는지 검증한다.
    ///     Verifies CalcFrameLength correctly calculates read response frame length.
    /// </summary>
    [Fact]
    public void CalcFrameLength_ReadResponse_ReturnsCorrectLength() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // TCP 읽기 응답 기록: MBAP(7) + FC=0x03 + ByteCount=4 + Data(4)
        // write TCP read response: MBAP(7) + FC=0x03 + ByteCount=4 + Data(4)
        buffer.WriteBytes(new byte[] {
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x07,
            0x01, // MBAP
            0x03, // FC
            0x04, // ByteCount
            0x00,
            0x0A,
            0x00,
            0x0B // Data
        });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 9 + ByteCount(4) = 13
        // 9 + ByteCount(4) = 13
        Assert.Equal(13, length);
    }

    /// <summary>
    ///     CalcFrameLength가 쓰기 응답의 프레임 길이를 12로 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns 12 for write response frames.
    /// </summary>
    [Fact]
    public void CalcFrameLength_WriteResponse_Returns12() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // TCP 쓰기 응답 기록: MBAP(7) + FC=0x06 + Addr(2) + Value(2)
        // write TCP write response: MBAP(7) + FC=0x06 + Addr(2) + Value(2)
        buffer.WriteBytes(new byte[] {
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x06,
            0x01, // MBAP
            0x06, // FC
            0x00,
            0x32,
            0x04,
            0xD2 // Addr + Value
        });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 쓰기 응답 고정 길이 12
        // write response fixed length 12
        Assert.Equal(12, length);
    }

    /// <summary>
    ///     CalcFrameLength가 오류 응답의 프레임 길이를 9로 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns 9 for error response frames.
    /// </summary>
    [Fact]
    public void CalcFrameLength_ErrorResponse_Returns9() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // TCP 오류 응답 기록: MBAP(7) + FC=0x83(오류) + ErrorCode
        // write TCP error response: MBAP(7) + FC=0x83(error) + ErrorCode
        buffer.WriteBytes(new byte[] {
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x03,
            0x01, // MBAP
            0x83, // FC (error)
            0x02  // Exception code
        });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 오류 프레임 고정 길이 9: MBAP(7) + FC(1) + Error(1)
        // error frame fixed length 9: MBAP(7) + FC(1) + Error(1)
        Assert.Equal(9, length);
    }

    #endregion

    #region ExtractCode / ExtractPayload / ValidateFrame

    /// <summary>
    ///     ExtractCode가 정상 응답에서 올바른 함수 코드를 추출하는지 검증한다.
    ///     Verifies ExtractCode extracts the correct function code from a normal response.
    /// </summary>
    [Fact]
    public void ExtractCode_NormalResponse_ReturnsFunctionCode() {
        // FC 0x03 패킷 빌드
        // build FC 0x03 packet
        var frame = _codec.BuildReadHoldingReg(0, 1, 1);

        // 함수 코드 추출
        // extract function code
        var code = _codec.ExtractCode(frame);

        // ReadHoldingReg 코드 확인
        // verify ReadHoldingReg code
        Assert.Equal(FunctionCode.ReadHoldingReg, code);
    }

    /// <summary>
    ///     ExtractCode가 오류 응답(비트 7 설정)에서 Error를 반환하는지 검증한다.
    ///     Verifies ExtractCode returns Error for error response (bit 7 set).
    /// </summary>
    [Fact]
    public void ExtractCode_ErrorResponse_ReturnsError() {
        // 오류 응답 프레임 생성: MBAP(7) + FC=0x83
        // create error response frame: MBAP(7) + FC=0x83
        byte[] frame = [0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x01, 0x83, 0x02];

        // 함수 코드 추출
        // extract function code
        var code = _codec.ExtractCode(frame);

        // Error 코드 확인
        // verify Error code
        Assert.Equal(FunctionCode.Error, code);
    }

    /// <summary>
    ///     ExtractPayload가 MBAP 헤더(7바이트)와 FC(1바이트)를 제거한 페이로드를 반환하는지 검증한다.
    ///     Verifies ExtractPayload strips MBAP header (7 bytes) and FC (1 byte).
    /// </summary>
    [Fact]
    public void ExtractPayload_ValidFrame_ReturnsDataAfterHeader() {
        // FC 0x03 패킷 빌드 (12바이트 프레임)
        // build FC 0x03 packet (12-byte frame)
        var frame = _codec.BuildReadHoldingReg(0x0100, 10, 1);

        // 페이로드 추출
        // extract payload
        var payload = _codec.ExtractPayload(frame);

        // 페이로드 길이 확인: 12 - 8(MBAP + FC) = 4바이트 (Addr + Count)
        // verify payload length: 12 - 8(MBAP + FC) = 4 bytes (Addr + Count)
        Assert.Equal(4, payload.Length);
    }

    /// <summary>
    ///     ValidateFrame이 TCP 프레임에 항상 true를 반환하는지 검증한다.
    ///     Verifies ValidateFrame always returns true for TCP frames (no CRC).
    /// </summary>
    [Fact]
    public void ValidateFrame_AnyFrame_ReturnsTrue() {
        // 임의 바이트 배열 생성
        // create arbitrary byte array
        byte[] frame = [0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA, 0xF9, 0xF8];

        // 프레임 검증
        // validate frame
        var result = _codec.ValidateFrame(frame);

        // TCP는 CRC 없음 — 항상 true
        // TCP has no CRC — always true
        Assert.True(result);
    }

    #endregion
}