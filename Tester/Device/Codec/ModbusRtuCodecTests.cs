using HTool.Core.Util;
using HTool.Device.Codec;
using HTool.Type;

namespace Tester.Device.Codec;

/// <summary>
///     ModbusRtuCodec 단위 테스트.
///     Unit tests for ModbusRtuCodec.
/// </summary>
public sealed class ModbusRtuCodecTests {
    // 테스트 대상 코덱 인스턴스
    // codec instance under test
    private readonly ModbusRtuCodec _codec = new();

    #region BuildReadInputReg

    /// <summary>
    ///     BuildReadInputReg가 FC 0x04로 8바이트 프레임을 생성하는지 검증한다.
    ///     Verifies BuildReadInputReg produces an 8-byte frame with FC 0x04.
    /// </summary>
    [Fact]
    public void BuildReadInputReg_ValidParams_Returns8ByteFrameWithFC04() {
        // FC 0x04 패킷 생성
        // build FC 0x04 packet
        var frame = _codec.BuildReadInputReg(200, 5, 1);

        // 프레임 길이 8바이트 확인
        // verify frame length is 8 bytes
        Assert.Equal(8, frame.Length);
        // 함수 코드 0x04 확인
        // verify function code 0x04
        Assert.Equal(0x04, frame[1]);
        // CRC 유효성 확인
        // verify CRC validity
        Assert.True(Checksum.Validate(frame));
    }

    #endregion

    #region BuildWriteSingleReg

    /// <summary>
    ///     BuildWriteSingleReg가 FC 0x06으로 8바이트 프레임을 생성하는지 검증한다.
    ///     Verifies BuildWriteSingleReg produces an 8-byte frame with FC 0x06.
    /// </summary>
    [Fact]
    public void BuildWriteSingleReg_ValidParams_Returns8ByteFrameWithFC06() {
        // FC 0x06 패킷 생성 (주소 50, 값 1234)
        // build FC 0x06 packet (address 50, value 1234)
        var frame = _codec.BuildWriteSingleReg(50, 1234, 1);

        // 프레임 길이 8바이트 확인
        // verify frame length is 8 bytes
        Assert.Equal(8, frame.Length);
        // 함수 코드 0x06 확인
        // verify function code 0x06
        Assert.Equal(0x06, frame[1]);
        // 값의 상위 바이트 확인 (1234 = 0x04D2)
        // verify value high byte (1234 = 0x04D2)
        Assert.Equal(0x04, frame[4]);
        // 값의 하위 바이트 확인
        // verify value low byte
        Assert.Equal(0xD2, frame[5]);
        // CRC 유효성 확인
        // verify CRC validity
        Assert.True(Checksum.Validate(frame));
    }

    #endregion

    #region BuildWriteMultiReg

    /// <summary>
    ///     BuildWriteMultiReg가 올바른 길이와 CRC를 가진 프레임을 생성하는지 검증한다.
    ///     Verifies BuildWriteMultiReg produces a frame with correct length and CRC.
    /// </summary>
    [Fact]
    public void BuildWriteMultiReg_TwoValues_ReturnsCorrectFrame() {
        // 2개 레지스터 값 배열
        // two register values array
        ushort[] values = [0x000A, 0x000B];

        // FC 0x10 패킷 생성
        // build FC 0x10 packet
        var frame = _codec.BuildWriteMultiReg(100, values, 1);

        // 프레임 길이: 7 + 4(데이터) + 2(CRC) = 13바이트
        // frame length: 7 + 4(data) + 2(CRC) = 13 bytes
        Assert.Equal(13, frame.Length);
        // 함수 코드 0x10 확인
        // verify function code 0x10
        Assert.Equal(0x10, frame[1]);
        // 바이트 수 필드 확인 (2 레지스터 * 2 바이트 = 4)
        // verify byte count field (2 registers * 2 bytes = 4)
        Assert.Equal(4, frame[6]);
        // CRC 유효성 확인
        // verify CRC validity
        Assert.True(Checksum.Validate(frame));
    }

    #endregion

    #region BuildWriteStrReg

    /// <summary>
    ///     BuildWriteStrReg가 문자열을 제로 패딩하여 인코딩하는지 검증한다.
    ///     Verifies BuildWriteStrReg encodes string with zero padding.
    /// </summary>
    [Fact]
    public void BuildWriteStrReg_ShortString_ZeroPadded() {
        // 고정 길이 10바이트로 "AB" 문자열 쓰기
        // write string "AB" with fixed length 10 bytes
        var frame = _codec.BuildWriteStrReg(0, "AB", 10, 1);

        // 프레임 길이: 7 + 10(데이터) + 2(CRC) = 19바이트
        // frame length: 7 + 10(data) + 2(CRC) = 19 bytes
        Assert.Equal(19, frame.Length);
        // 함수 코드 0x10 확인 (문자열은 WriteMultiReg 사용)
        // verify function code 0x10 (string uses WriteMultiReg)
        Assert.Equal(0x10, frame[1]);
        // 첫 번째 문자 'A' 확인
        // verify first character 'A'
        Assert.Equal((byte)'A', frame[7]);
        // 두 번째 문자 'B' 확인
        // verify second character 'B'
        Assert.Equal((byte)'B', frame[8]);
        // 세 번째 바이트가 0 (패딩) 확인
        // verify third byte is 0 (padding)
        Assert.Equal(0, frame[9]);
        // CRC 유효성 확인
        // verify CRC validity
        Assert.True(Checksum.Validate(frame));
    }

    #endregion

    #region BuildReadInfoReg

    /// <summary>
    ///     BuildReadInfoReg가 4바이트 프레임(FC 0x11)을 생성하는지 검증한다.
    ///     Verifies BuildReadInfoReg produces a 4-byte frame with FC 0x11.
    /// </summary>
    [Fact]
    public void BuildReadInfoReg_ValidDeviceId_Returns4ByteFrame() {
        // FC 0x11 패킷 생성
        // build FC 0x11 packet
        var frame = _codec.BuildReadInfoReg(1);

        // 프레임 길이 4바이트: ID(1) + FC(1) + CRC(2)
        // frame length 4 bytes: ID(1) + FC(1) + CRC(2)
        Assert.Equal(4, frame.Length);
        // 장치 ID 확인
        // verify device ID
        Assert.Equal(1, frame[0]);
        // 함수 코드 0x11 확인
        // verify function code 0x11
        Assert.Equal(0x11, frame[1]);
        // CRC 유효성 확인
        // verify CRC validity
        Assert.True(Checksum.Validate(frame));
    }

    #endregion

    #region Properties

    /// <summary>
    ///     HeaderSize가 2(ID + FC)를 반환하는지 검증한다.
    ///     Verifies HeaderSize returns 2 (ID + FC).
    /// </summary>
    [Fact]
    public void HeaderSize_Default_Returns2() {
        // 헤더 크기 조회
        // get header size
        var size = _codec.HeaderSize;

        // RTU 헤더는 ID(1) + FC(1) = 2
        // RTU header is ID(1) + FC(1) = 2
        Assert.Equal(2, size);
    }

    /// <summary>
    ///     FunctionPos가 1을 반환하는지 검증한다.
    ///     Verifies FunctionPos returns 1.
    /// </summary>
    [Fact]
    public void FunctionPos_Default_Returns1() {
        // 함수 코드 위치 조회
        // get function code position
        var pos = _codec.FunctionPos;

        // RTU 프레임에서 FC는 오프셋 1
        // FC is at offset 1 in RTU frame
        Assert.Equal(1, pos);
    }

    #endregion

    #region BuildReadHoldingReg

    /// <summary>
    ///     BuildReadHoldingReg가 8바이트 프레임을 생성하는지 검증한다.
    ///     Verifies BuildReadHoldingReg produces an 8-byte frame.
    /// </summary>
    [Fact]
    public void BuildReadHoldingReg_ValidParams_Returns8ByteFrame() {
        // FC 0x03 패킷 생성 (주소 100, 10 레지스터, 장치 ID 1)
        // build FC 0x03 packet (address 100, 10 registers, device ID 1)
        var frame = _codec.BuildReadHoldingReg(100, 10, 1);

        // 프레임 길이 8바이트: ID(1) + FC(1) + Addr(2) + Count(2) + CRC(2)
        // frame length 8 bytes: ID(1) + FC(1) + Addr(2) + Count(2) + CRC(2)
        Assert.Equal(8, frame.Length);
        // 장치 ID 확인
        // verify device ID
        Assert.Equal(1, frame[0]);
        // 함수 코드 0x03 확인
        // verify function code 0x03
        Assert.Equal(0x03, frame[1]);
        // CRC 유효성 확인
        // verify CRC validity
        Assert.True(Checksum.Validate(frame));
    }

    /// <summary>
    ///     BuildReadHoldingReg가 주소와 개수를 Big-Endian으로 기록하는지 검증한다.
    ///     Verifies BuildReadHoldingReg writes address and count in Big-Endian.
    /// </summary>
    [Fact]
    public void BuildReadHoldingReg_AddressAndCount_BigEndianEncoded() {
        // FC 0x03 패킷 생성 (주소 0x0100, 개수 0x000A)
        // build FC 0x03 packet (address 0x0100, count 0x000A)
        var frame = _codec.BuildReadHoldingReg(0x0100, 0x000A, 5);

        // 주소 상위 바이트 확인
        // verify address high byte
        Assert.Equal(0x01, frame[2]);
        // 주소 하위 바이트 확인
        // verify address low byte
        Assert.Equal(0x00, frame[3]);
        // 개수 상위 바이트 확인
        // verify count high byte
        Assert.Equal(0x00, frame[4]);
        // 개수 하위 바이트 확인
        // verify count low byte
        Assert.Equal(0x0A, frame[5]);
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
        // 3바이트만 기록 (최소 4바이트 필요)
        // write only 3 bytes (minimum 4 needed)
        buffer.WriteBytes(new byte[] { 0x01, 0x03, 0x04 });

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
        // 읽기 응답 기록: ID=1, FC=0x03, ByteCount=4 (→ 길이 = 4 + 5 = 9)
        // write read response: ID=1, FC=0x03, ByteCount=4 (→ length = 4 + 5 = 9)
        buffer.WriteBytes(new byte[] { 0x01, 0x03, 0x04, 0x00, 0x0A, 0x00, 0x0B, 0x00, 0x00 });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 3 + ByteCount(4) + 2(CRC) = 9
        // 3 + ByteCount(4) + 2(CRC) = 9
        Assert.Equal(9, length);
    }

    /// <summary>
    ///     CalcFrameLength가 쓰기 응답의 프레임 길이를 8로 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns 8 for write response frames.
    /// </summary>
    [Fact]
    public void CalcFrameLength_WriteResponse_Returns8() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // 쓰기 응답 기록: ID=1, FC=0x06, ...
        // write response data: ID=1, FC=0x06, ...
        buffer.WriteBytes(new byte[] { 0x01, 0x06, 0x00, 0x32, 0x04, 0xD2, 0x00, 0x00 });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 쓰기 응답 고정 길이 8
        // write response fixed length 8
        Assert.Equal(8, length);
    }

    /// <summary>
    ///     CalcFrameLength가 오류 응답의 프레임 길이를 5로 반환하는지 검증한다.
    ///     Verifies CalcFrameLength returns 5 for error response frames.
    /// </summary>
    [Fact]
    public void CalcFrameLength_ErrorResponse_Returns5() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // 오류 응답 기록: ID=1, FC=0x83(오류), ErrorCode=0x02, CRC(2)
        // write error response: ID=1, FC=0x83(error), ErrorCode=0x02, CRC(2)
        buffer.WriteBytes(new byte[] { 0x01, 0x83, 0x02, 0x00, 0x00 });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 오류 프레임 고정 길이 5: ID(1) + FC(1) + Error(1) + CRC(2)
        // error frame fixed length 5: ID(1) + FC(1) + Error(1) + CRC(2)
        Assert.Equal(5, length);
    }

    /// <summary>
    ///     CalcFrameLength가 그래프 데이터 프레임의 길이를 올바르게 계산하는지 검증한다.
    ///     Verifies CalcFrameLength correctly calculates graph data frame length.
    /// </summary>
    [Fact]
    public void CalcFrameLength_GraphResponse_ReturnsLengthFromTwoByteField() {
        // 1024바이트 링 버퍼 생성
        // create 1024-byte ring buffer
        var buffer = new RingBuffer(1024);
        // 그래프 응답 기록: ID=1, FC=0x64, LenH=0x00, LenL=0x0A
        // write graph response: ID=1, FC=0x64, LenH=0x00, LenL=0x0A
        buffer.WriteBytes(new byte[] { 0x01, 0x64, 0x00, 0x0A, 0x00, 0x00 });

        // 프레임 길이 계산
        // calculate frame length
        var length = _codec.CalcFrameLength(buffer);

        // 그래프: (0x00 << 8 | 0x0A) + 6 = 10 + 6 = 16
        // graph: (0x00 << 8 | 0x0A) + 6 = 10 + 6 = 16
        Assert.Equal(16, length);
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
        // 오류 응답 프레임 생성: ID=1, FC=0x83
        // create error response frame: ID=1, FC=0x83
        byte[] frame = [0x01, 0x83, 0x02, 0x00, 0x00];

        // 함수 코드 추출
        // extract function code
        var code = _codec.ExtractCode(frame);

        // Error 코드 확인
        // verify Error code
        Assert.Equal(FunctionCode.Error, code);
    }

    /// <summary>
    ///     ExtractPayload가 헤더(2바이트)와 CRC(2바이트)를 제거한 페이로드를 반환하는지 검증한다.
    ///     Verifies ExtractPayload strips header (2 bytes) and CRC (2 bytes).
    /// </summary>
    [Fact]
    public void ExtractPayload_ValidFrame_ReturnsDataWithoutHeaderAndCrc() {
        // FC 0x03 패킷 빌드 (8바이트 프레임)
        // build FC 0x03 packet (8-byte frame)
        var frame = _codec.BuildReadHoldingReg(0x0100, 10, 1);

        // 페이로드 추출
        // extract payload
        var payload = _codec.ExtractPayload(frame);

        // 페이로드 길이 확인: 8 - 2(헤더) - 2(CRC) = 4
        // verify payload length: 8 - 2(header) - 2(CRC) = 4
        Assert.Equal(4, payload.Length);
    }

    /// <summary>
    ///     ValidateFrame이 유효한 CRC를 가진 프레임에 true를 반환하는지 검증한다.
    ///     Verifies ValidateFrame returns true for frames with valid CRC.
    /// </summary>
    [Fact]
    public void ValidateFrame_ValidCrc_ReturnsTrue() {
        // 유효한 CRC를 가진 패킷 생성
        // build packet with valid CRC
        var frame = _codec.BuildReadHoldingReg(0, 1, 1);

        // 프레임 검증
        // validate frame
        var result = _codec.ValidateFrame(frame);

        // 유효한 CRC이면 true
        // true for valid CRC
        Assert.True(result);
    }

    /// <summary>
    ///     ValidateFrame이 손상된 CRC를 가진 프레임에 false를 반환하는지 검증한다.
    ///     Verifies ValidateFrame returns false for frames with corrupted CRC.
    /// </summary>
    [Fact]
    public void ValidateFrame_CorruptedCrc_ReturnsFalse() {
        // 유효한 패킷 생성 후 CRC 손상
        // build valid packet then corrupt CRC
        var frame = _codec.BuildReadHoldingReg(0, 1, 1);
        // 마지막 바이트 XOR로 CRC 손상
        // corrupt CRC by XOR on last byte
        frame[^1] ^= 0xFF;

        // 프레임 검증
        // validate frame
        var result = _codec.ValidateFrame(frame);

        // 손상된 CRC이면 false
        // false for corrupted CRC
        Assert.False(result);
    }

    #endregion
}