using System.Runtime.CompilerServices;
using System.Text;
using HTool.Core.Util;
using HTool.Type;

namespace HTool.Device.Codec;

/// <summary>
///     MODBUS TCP 프레임 인코더/디코더.
///     MODBUS TCP frame encoder/decoder.
/// </summary>
/// <remarks>
///     <para>
///         TCP 프레임: [MBAP(7)][FC(1)][Data]. MBAP = [TID(2)][PID(2)][LEN(2)][UID(1)].
///         TCP frame: [MBAP(7)][FC(1)][Data]. MBAP = [TID(2)][PID(2)][LEN(2)][UID(1)].
///     </para>
/// </remarks>
public sealed class ModbusTcpCodec : IModbusCodec {
    /// <summary>
    ///     TCP 프레임 헤더 크기 (MBAP(7) + FC(1) = 8바이트).
    ///     TCP frame header size (MBAP(7) + FC(1) = 8 bytes).
    /// </summary>
    public int HeaderSize => 8;

    /// <summary>
    ///     프레임 내 함수 코드 위치 (오프셋 7).
    ///     Function code position within the frame (offset 7).
    /// </summary>
    public int FunctionPos => 7;

    /// <inheritdoc />
    public byte[] BuildReadHoldingReg(ushort addr, ushort count, byte dId, ushort tId = 0) {
        // 12바이트 패킷 할당: MBAP(7) + FC(1) + Addr(2) + Count(2)
        // allocate 12-byte packet: MBAP(7) + FC(1) + Addr(2) + Count(2)
        var p = GC.AllocateUninitializedArray<byte>(12);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // MBAP 헤더 기록
        // write MBAP header
        WriteMbapHeader(s, tId, 6, dId);
        // 함수 코드 설정
        // set function code
        p[7] = (byte)FunctionCode.ReadHoldingReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[8..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[10..], count);
        // 완성된 보유 레지스터 읽기 패킷 반환
        // return the constructed read holding register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildReadInputReg(ushort addr, ushort count, byte dId, ushort tId = 0) {
        // 12바이트 패킷 할당: MBAP(7) + FC(1) + Addr(2) + Count(2)
        // allocate 12-byte packet: MBAP(7) + FC(1) + Addr(2) + Count(2)
        var p = GC.AllocateUninitializedArray<byte>(12);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // MBAP 헤더 기록
        // write MBAP header
        WriteMbapHeader(s, tId, 6, dId);
        // 함수 코드 설정
        // set function code
        p[7] = (byte)FunctionCode.ReadInputReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[8..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[10..], count);
        // 완성된 입력 레지스터 읽기 패킷 반환
        // return the constructed read input register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildWriteSingleReg(ushort addr, ushort value, byte dId, ushort tId = 0) {
        // 12바이트 패킷 할당: MBAP(7) + FC(1) + Addr(2) + Value(2)
        // allocate 12-byte packet: MBAP(7) + FC(1) + Addr(2) + Value(2)
        var p = GC.AllocateUninitializedArray<byte>(12);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // MBAP 헤더 기록
        // write MBAP header
        WriteMbapHeader(s, tId, 6, dId);
        // 함수 코드 설정
        // set function code
        p[7] = (byte)FunctionCode.WriteSingleReg;
        // 레지스터 주소 기록 (Big-Endian)
        // write register address (Big-Endian)
        ByteOrder.WriteUInt16(s[8..], addr);
        // 레지스터 값 기록 (Big-Endian)
        // write register value (Big-Endian)
        ByteOrder.WriteUInt16(s[10..], value);
        // 완성된 단일 레지스터 쓰기 패킷 반환
        // return the constructed write single register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildWriteMultiReg(ushort addr, ReadOnlySpan<ushort> values, byte dId, ushort tId = 0) {
        // 레지스터 개수 계산
        // calculate register count
        var count = values.Length;
        // 레지스터 데이터의 바이트 수 계산
        // calculate byte count for register data
        var byteCount = count * 2;
        // PDU 길이 계산: UID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N)
        // calculate PDU length: UID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N)
        var pduLen = 7 + byteCount;
        // 패킷 할당: MBAP(6) + UID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N)
        // allocate packet: MBAP(6) + UID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N)
        var p = GC.AllocateUninitializedArray<byte>(6 + pduLen);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // MBAP 헤더 기록
        // write MBAP header
        WriteMbapHeader(s, tId, pduLen, dId);
        // 함수 코드 설정
        // set function code
        p[7] = (byte)FunctionCode.WriteMultiReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[8..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[10..], (ushort)count);
        // 바이트 수 기록
        // write byte count
        p[12] = (byte)byteCount;
        // 레지스터 값들을 Big-Endian으로 기록
        // write register values (Big-Endian)
        for (var i = 0; i < count; i++)
            // 각 레지스터 값을 Big-Endian으로 기록
            // write each register value (Big-Endian)
            ByteOrder.WriteUInt16(s[(13 + i * 2)..], values[i]);
        // 완성된 다중 레지스터 쓰기 패킷 반환
        // return the constructed write multiple register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildWriteStrReg(ushort addr, string str, int length, byte dId, ushort tId = 0) {
        // 문자열을 바이트로 인코딩
        // encode string to bytes
        var strBytes = Encoding.ASCII.GetBytes(str);
        // 실제 바이트 길이 결정 (지정 길이 또는 문자열 길이, 2바이트 경계 정렬)
        // determine actual byte length (use specified length or string length, aligned to 2-byte boundary)
        var byteLen = length > 0 ? length : strBytes.Length;
        // 레지스터 정렬을 위해 짝수 바이트 보장
        // ensure even byte count for register alignment
        if (byteLen % 2 is not 0)
            // 다음 짝수로 올림
            // round up to next even number
            byteLen++;
        // 레지스터 개수 계산
        // calculate register count
        var regCount = byteLen / 2;
        // PDU 길이 계산: UID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N)
        // calculate PDU length: UID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N)
        var pduLen = 7 + byteLen;
        // 패킷 할당: MBAP(6) + PDU
        // allocate packet: MBAP(6) + PDU
        var p = GC.AllocateUninitializedArray<byte>(6 + pduLen);
        // 데이터 영역을 0으로 초기화 (byteLen보다 짧은 문자열용 패딩)
        // initialize data area to zero (padding for strings shorter than byteLen)
        Array.Clear(p, 13, byteLen);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // MBAP 헤더 기록
        // write MBAP header
        WriteMbapHeader(s, tId, pduLen, dId);
        // 함수 코드 설정
        // set function code
        p[7] = (byte)FunctionCode.WriteMultiReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[8..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[10..], (ushort)regCount);
        // 바이트 수 기록
        // write byte count
        p[12] = (byte)byteLen;
        // 문자열 바이트를 데이터 영역에 복사
        // copy string bytes into data area
        Array.Copy(strBytes, 0, p, 13, Math.Min(strBytes.Length, byteLen));
        // 완성된 문자열 레지스터 쓰기 패킷 반환
        // return the constructed write string register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildReadInfoReg(byte dId, ushort tId = 0) {
        // 8바이트 패킷 할당: MBAP(7) + FC(1)
        // allocate 8-byte packet: MBAP(7) + FC(1)
        var p = GC.AllocateUninitializedArray<byte>(8);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // PDU 길이 2(UID + FC)로 MBAP 헤더 기록
        // write MBAP header with PDU length 2 (UID + FC)
        WriteMbapHeader(s, tId, 2, dId);
        // 함수 코드 설정
        // set function code
        p[7] = (byte)FunctionCode.ReadInfoReg;
        // 완성된 장치 정보 읽기 패킷 반환
        // return the constructed read info register packet
        return p;
    }

    /// <inheritdoc />
    public int CalcFrameLength(RingBuffer buffer) {
        // 최소 프레임 크기 확인: MBAP(7) + FC(1) + 최소 1 데이터 바이트 = 9
        // check minimum frame size: MBAP(7) + FC(1) + at least 1 data byte = 9
        if (buffer.Available < 9)
            // 프레임 길이를 판단하기에 데이터 부족
            // insufficient data to determine frame length
            return -1;

        // 위치 7에서 함수 코드 바이트 읽기
        // read function code byte at position 7
        var fc = buffer.Peek(FunctionPos);
        // 오류 응답 여부 확인 (비트 7 설정)
        // check if error response (bit 7 set)
        if ((fc & 0x80) is not 0)
            // 오류 프레임: MBAP(7) + FC(1) + Error(1) = 9
            // error frame: MBAP(7) + FC(1) + Error(1) = 9
            return 9;

        // 함수 코드별 프레임 길이 결정
        // determine frame length by function code
        return fc switch {
            // 읽기 응답: MBAP(7) + FC(1) + LEN(1) + DATA(LEN)
            // read responses: MBAP(7) + FC(1) + LEN(1) + DATA(LEN)
            (byte)FunctionCode.ReadHoldingReg or (byte)FunctionCode.ReadInputReg or (byte)FunctionCode.ReadInfoReg => buffer.Peek(HeaderSize) + 9,

            // 쓰기 응답: MBAP(7) + FC(1) + ADDR(2) + VALUE(2) = 12
            // write responses: MBAP(7) + FC(1) + ADDR(2) + VALUE(2) = 12
            (byte)FunctionCode.WriteSingleReg or (byte)FunctionCode.WriteMultiReg => 12,

            // 그래프 데이터: MBAP(7) + FC(1) + LEN(2) + DATA(LEN)
            // graph data: MBAP(7) + FC(1) + LEN(2) + DATA(LEN)
            (byte)FunctionCode.GraphData or (byte)FunctionCode.GraphRes => buffer.Available >= 10
                ? ((buffer.Peek(HeaderSize) << 8) | buffer.Peek(HeaderSize + 1)) + 10
                : -1,

            // 고해상도 그래프: 그래프와 동일 헤더 구조
            // high-resolution graph: same header structure as graph
            (byte)FunctionCode.HighResGraph => buffer.Available >= 10
                ? ((buffer.Peek(HeaderSize) << 8) | buffer.Peek(HeaderSize + 1)) + 10
                : -1,

            // 알 수 없는 함수 코드 — 1바이트 재동기화 필요
            // unknown function code — requires 1-byte resync
            _ => 0
        };
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FunctionCode ExtractCode(ReadOnlySpan<byte> frame) {
        // 위치 7에서 함수 코드 읽기
        // read function code at position 7
        var raw = frame[FunctionPos];
        // 오류 응답 여부 확인 (비트 7 설정)
        // check for error response (bit 7 set)
        return (raw & 0x80) is not 0
            ? FunctionCode.Error
            : (FunctionCode)raw;
    }

    /// <inheritdoc />
    public byte[] ExtractPayload(ReadOnlySpan<byte> frame) {
        // MBAP 헤더와 FC 제거 (총 8바이트)
        // strip MBAP header and FC (8 bytes total)
        return frame[HeaderSize..].ToArray();
    }

    /// <inheritdoc />
    public bool ValidateFrame(ReadOnlySpan<byte> frame) {
        // TCP는 CRC 없음 — 항상 유효
        // TCP has no CRC — always valid
        return true;
    }

    /// <summary>
    ///     MBAP 헤더를 스팬에 기록한다.
    ///     Writes the MBAP header into the span.
    /// </summary>
    /// <param name="s">대상 스팬 (최소 7바이트) / destination span (minimum 7 bytes)</param>
    /// <param name="transactionId">트랜잭션 ID (요청/응답 매칭용) / transaction ID (for request/response matching)</param>
    /// <param name="pduLen">PDU 길이 (UID + FC + Data) / PDU length (UID + FC + Data)</param>
    /// <param name="deviceId">슬레이브 장치 ID (Unit ID 필드에만 사용) / slave device ID (used only in Unit ID field)</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteMbapHeader(Span<byte> s, ushort transactionId, int pduLen, byte deviceId) {
        // 트랜잭션 ID (Big-Endian)
        // transaction ID (Big-Endian)
        ByteOrder.WriteUInt16(s, transactionId);
        // 프로토콜 ID (MODBUS는 항상 0x0000)
        // protocol ID (always 0x0000 for MODBUS)
        ByteOrder.WriteUInt16(s[2..], 0x0000);
        // PDU 길이 (Big-Endian)
        // PDU length (Big-Endian)
        ByteOrder.WriteUInt16(s[4..], (ushort)pduLen);
        // 유닛 ID (슬레이브 장치 ID)
        // unit ID (slave device ID)
        s[6] = deviceId;
    }
}