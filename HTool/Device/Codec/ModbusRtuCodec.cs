using System.Runtime.CompilerServices;
using System.Text;
using HTool.Core.Util;
using HTool.Type;

namespace HTool.Device.Codec;

/// <summary>
///     MODBUS RTU 프레임 인코더/디코더.
///     MODBUS RTU frame encoder/decoder.
/// </summary>
/// <remarks>
///     <para>
///         RTU 프레임: [ID(1)][FC(1)][Data][CRC(2)].
///         RTU frame: [ID(1)][FC(1)][Data][CRC(2)].
///     </para>
/// </remarks>
public sealed class ModbusRtuCodec : IModbusCodec {
    /// <summary>
    ///     RTU 프레임 헤더 크기 (ID + FC = 2바이트).
    ///     RTU frame header size (ID + FC = 2 bytes).
    /// </summary>
    public int HeaderSize => 2;

    /// <summary>
    ///     프레임 내 함수 코드 위치 (오프셋 1).
    ///     Function code position within the frame (offset 1).
    /// </summary>
    public int FunctionPos => 1;

    /// <inheritdoc />
    public byte[] BuildReadHoldingReg(ushort addr, ushort count, byte dId, ushort tId = 0) {
        // 8바이트 패킷 할당: ID(1) + FC(1) + Addr(2) + Count(2) + CRC(2)
        // allocate 8-byte packet: ID(1) + FC(1) + Addr(2) + Count(2) + CRC(2)
        var p = GC.AllocateUninitializedArray<byte>(8);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // 슬레이브 장치 ID 설정
        // set slave device ID
        p[0] = dId;
        // 함수 코드 설정
        // set function code
        p[1] = (byte)FunctionCode.ReadHoldingReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[2..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[4..], count);
        // CRC-16 계산 및 추가
        // calculate and append CRC-16
        Checksum.CalculateTo(s[..6], s[6..]);
        // 완성된 보유 레지스터 읽기 패킷 반환
        // return the constructed read holding register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildReadInputReg(ushort addr, ushort count, byte dId, ushort tId = 0) {
        // 8바이트 패킷 할당: ID(1) + FC(1) + Addr(2) + Count(2) + CRC(2)
        // allocate 8-byte packet: ID(1) + FC(1) + Addr(2) + Count(2) + CRC(2)
        var p = GC.AllocateUninitializedArray<byte>(8);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // 슬레이브 장치 ID 설정
        // set slave device ID
        p[0] = dId;
        // 함수 코드 설정
        // set function code
        p[1] = (byte)FunctionCode.ReadInputReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[2..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[4..], count);
        // CRC-16 계산 및 추가
        // calculate and append CRC-16
        Checksum.CalculateTo(s[..6], s[6..]);
        // 완성된 입력 레지스터 읽기 패킷 반환
        // return the constructed read input register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildWriteSingleReg(ushort addr, ushort value, byte dId, ushort tId = 0) {
        // 8바이트 패킷 할당: ID(1) + FC(1) + Addr(2) + Value(2) + CRC(2)
        // allocate 8-byte packet: ID(1) + FC(1) + Addr(2) + Value(2) + CRC(2)
        var p = GC.AllocateUninitializedArray<byte>(8);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // 슬레이브 장치 ID 설정
        // set slave device ID
        p[0] = dId;
        // 함수 코드 설정
        // set function code
        p[1] = (byte)FunctionCode.WriteSingleReg;
        // 레지스터 주소 기록 (Big-Endian)
        // write register address (Big-Endian)
        ByteOrder.WriteUInt16(s[2..], addr);
        // 레지스터 값 기록 (Big-Endian)
        // write register value (Big-Endian)
        ByteOrder.WriteUInt16(s[4..], value);
        // CRC-16 계산 및 추가
        // calculate and append CRC-16
        Checksum.CalculateTo(s[..6], s[6..]);
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
        // 패킷 할당: ID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N) + CRC(2)
        // allocate packet: ID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N) + CRC(2)
        var p = GC.AllocateUninitializedArray<byte>(7 + byteCount + 2);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // 슬레이브 장치 ID 설정
        // set slave device ID
        p[0] = dId;
        // 함수 코드 설정
        // set function code
        p[1] = (byte)FunctionCode.WriteMultiReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[2..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[4..], (ushort)count);
        // 바이트 수 기록
        // write byte count
        p[6] = (byte)byteCount;
        // 레지스터 값들을 Big-Endian으로 기록
        // write register values (Big-Endian)
        for (var i = 0; i < count; i++)
            // 각 레지스터 값을 Big-Endian으로 기록
            // write each register value (Big-Endian)
            ByteOrder.WriteUInt16(s[(7 + i * 2)..], values[i]);

        // CRC-16 계산 및 추가
        // calculate and append CRC-16
        Checksum.CalculateTo(s[..^2], s[^2..]);
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
        // 패킷 할당: ID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N) + CRC(2)
        // allocate packet: ID(1) + FC(1) + Addr(2) + Count(2) + ByteCount(1) + Data(N) + CRC(2)
        var p = GC.AllocateUninitializedArray<byte>(7 + byteLen + 2);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // 데이터 영역을 0으로 초기화 (byteLen보다 짧은 문자열용 패딩)
        // initialize data area to zero (padding for strings shorter than byteLen)
        Array.Clear(p, 7, byteLen);
        // 슬레이브 장치 ID 설정
        // set slave device ID
        p[0] = dId;
        // 함수 코드 설정
        // set function code
        p[1] = (byte)FunctionCode.WriteMultiReg;
        // 시작 주소 기록 (Big-Endian)
        // write start address (Big-Endian)
        ByteOrder.WriteUInt16(s[2..], addr);
        // 레지스터 개수 기록 (Big-Endian)
        // write register count (Big-Endian)
        ByteOrder.WriteUInt16(s[4..], (ushort)regCount);
        // 바이트 수 기록
        // write byte count
        p[6] = (byte)byteLen;
        // 문자열 바이트를 데이터 영역에 복사
        // copy string bytes into data area
        Array.Copy(strBytes, 0, p, 7, Math.Min(strBytes.Length, byteLen));
        // CRC-16 계산 및 추가
        // calculate and append CRC-16
        Checksum.CalculateTo(s[..^2], s[^2..]);
        // 완성된 문자열 레지스터 쓰기 패킷 반환
        // return the constructed write string register packet
        return p;
    }

    /// <inheritdoc />
    public byte[] BuildReadInfoReg(byte dId, ushort tId = 0) {
        // 4바이트 패킷 할당: ID(1) + FC(1) + CRC(2)
        // allocate 4-byte packet: ID(1) + FC(1) + CRC(2)
        var p = GC.AllocateUninitializedArray<byte>(4);
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = p.AsSpan();
        // 슬레이브 장치 ID 설정
        // set slave device ID
        p[0] = dId;
        // 함수 코드 설정
        // set function code
        p[1] = (byte)FunctionCode.ReadInfoReg;
        // CRC-16 계산 및 추가
        // calculate and append CRC-16
        Checksum.CalculateTo(s[..2], s[2..]);
        // 완성된 장치 정보 읽기 패킷 반환
        // return the constructed read info register packet
        return p;
    }

    /// <inheritdoc />
    public int CalcFrameLength(RingBuffer buffer) {
        // 최소 프레임 크기 확인: ID(1) + FC(1) + 최소 1 데이터 바이트 + CRC(2) = 4
        // check minimum frame size: ID(1) + FC(1) + at least 1 data byte + CRC(2) = 4
        if (buffer.Available < 4)
            // 프레임 길이를 판단하기에 데이터 부족
            // insufficient data to determine frame length
            return -1;

        // 위치 1에서 함수 코드 바이트 읽기
        // read function code byte at position 1
        var fc = buffer.Peek(FunctionPos);
        // 오류 응답 여부 확인 (비트 7 설정)
        // check if error response (bit 7 set)
        if ((fc & 0x80) is not 0)
            // 오류 프레임: ID(1) + FC(1) + Error(1) + CRC(2) = 5
            // error frame: ID(1) + FC(1) + Error(1) + CRC(2) = 5
            return 5;

        // 함수 코드별 프레임 길이 결정
        // determine frame length by function code
        return fc switch {
            // 읽기 응답: ID(1) + FC(1) + LEN(1) + DATA(LEN) + CRC(2)
            // read responses: ID(1) + FC(1) + LEN(1) + DATA(LEN) `+ CRC(2)
            (byte)FunctionCode.ReadHoldingReg or (byte)FunctionCode.ReadInputReg or (byte)FunctionCode.ReadInfoReg => buffer.Peek(HeaderSize) + 5,

            // 쓰기 응답: ID(1) + FC(1) + ADDR(2) + VALUE(2) + CRC(2) = 8
            // write responses: ID(1) + FC(1) + ADDR(2) + VALUE(2) + CRC(2) = 8
            (byte)FunctionCode.WriteSingleReg or (byte)FunctionCode.WriteMultiReg => 8,

            // 그래프 데이터: ID(1) + FC(1) + LEN(2) + DATA(LEN) + CRC(2)
            // graph data: ID(1) + FC(1) + LEN(2) + DATA(LEN) + CRC(2)
            (byte)FunctionCode.GraphData or (byte)FunctionCode.GraphRes => buffer.Available >= 4
                ? ((buffer.Peek(HeaderSize) << 8) | buffer.Peek(HeaderSize + 1)) + 6
                : -1,

            // 고해상도 그래프: 그래프와 동일 구조
            // high-resolution graph: same structure as graph
            (byte)FunctionCode.HighResGraph => buffer.Available >= 4
                ? ((buffer.Peek(HeaderSize) << 8) | buffer.Peek(HeaderSize + 1)) + 6
                : -1,

            // 알 수 없는 함수 코드 — 1바이트 재동기화 필요
            // unknown function code — requires 1-byte resync
            _ => 0
        };
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FunctionCode ExtractCode(ReadOnlySpan<byte> frame) {
        // 위치 1에서 함수 코드 읽기
        // read function code at position 1
        var raw = frame[FunctionPos];
        // 정상 응답이면 함수 코드로 변환, 오류 응답(비트 7 설정)이면 Error 반환
        // return parsed function code for normal response, Error if bit 7 is set
        return (raw & 0x80) is 0 ? (FunctionCode)raw : FunctionCode.Error;
    }

    /// <inheritdoc />
    public byte[] ExtractPayload(ReadOnlySpan<byte> frame) {
        // 헤더(ID + FC)와 CRC 트레일러 제거
        // strip header (ID + FC) and CRC trailer
        return frame[HeaderSize..^2].ToArray();
    }

    /// <inheritdoc />
    public bool ValidateFrame(ReadOnlySpan<byte> frame) {
        // 기존 유틸리티로 CRC-16 검증 위임
        // delegate CRC-16 validation to existing utility
        return Checksum.Validate(frame);
    }

    /// <summary>
    ///     알려진 함수 코드인지 확인한다.
    ///     Checks if the given byte is a known function code.
    /// </summary>
    /// <param name="fc">함수 코드 바이트 / function code byte</param>
    /// <returns>알려진 코드이면 true / true if known code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKnownCode(byte fc) {
        // 지원하는 함수 코드 패턴 매칭 결과 반환
        // return pattern match result for supported function codes
        return fc is (byte)FunctionCode.ReadHoldingReg
            or (byte)FunctionCode.ReadInputReg
            or (byte)FunctionCode.WriteSingleReg
            or (byte)FunctionCode.WriteMultiReg
            or (byte)FunctionCode.ReadInfoReg
            or (byte)FunctionCode.GraphData
            or (byte)FunctionCode.GraphRes
            or (byte)FunctionCode.HighResGraph;
    }
}