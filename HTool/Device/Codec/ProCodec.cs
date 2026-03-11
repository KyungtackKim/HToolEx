using HTool.Core.Type.Pro;
using HTool.Core.Util;

namespace HTool.Device.Codec;

/// <summary>
///     PRO X 게이트웨이 메시지 프레임 인코더/디코더.
///     PRO X gateway message frame encoder/decoder.
/// </summary>
/// <remarks>
///     <para>
///         PRO X 프레임: [Length(2)][MID(2)][Revision(2)][Reserved(10)][Payload(N)].
///         Length = 전체 메시지 길이(헤더 포함). Big-Endian.
///     </para>
///     <para>
///         PRO X frame: [Length(2)][MID(2)][Revision(2)][Reserved(10)][Payload(N)].
///         Length = total message length (including header). Big-Endian.
///     </para>
/// </remarks>
internal static class ProCodec {
    // PRO X 헤더 크기 (바이트)
    // PRO X header size (bytes)
    private const int HeaderSize = 16;

    /// <summary>
    ///     PRO X 메시지 프레임을 생성한다.
    ///     Builds a PRO X message frame.
    /// </summary>
    /// <param name="mid">메시지 ID / message ID</param>
    /// <param name="revision">프로토콜 리비전 / protocol revision</param>
    /// <param name="payload">페이로드 데이터 (없으면 null) / payload data (null if none)</param>
    /// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
    internal static byte[] BuildMessage(MessageId mid, int revision = 0, ReadOnlySpan<byte> payload = default) {
        // 전체 프레임 크기 계산 (헤더 + 페이로드)
        // calculate total frame size (header + payload)
        var totalLength = HeaderSize + payload.Length;
        // 프레임 바이트 배열 할당
        // allocate frame byte array
        var frame = new byte[totalLength];
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = frame.AsSpan();

        // 전체 길이 기록 (Big-Endian)
        // write total length (Big-Endian)
        Utils.WriteUInt16(s, (ushort)totalLength);
        // MID 기록 (Big-Endian)
        // write MID (Big-Endian)
        Utils.WriteUInt16(s[2..], (ushort)mid);
        // 리비전 기록 (Big-Endian)
        // write revision (Big-Endian)
        Utils.WriteUInt16(s[4..], (ushort)revision);
        // 예약 영역 0으로 초기화
        // initialize reserved area to zero
        s[6..HeaderSize].Clear();

        // 페이로드가 있으면 복사
        // copy payload if present
        if (!payload.IsEmpty)
            // 페이로드를 헤더 뒤에 복사
            // copy payload after header
            payload.CopyTo(s[HeaderSize..]);

        // 완성된 프레임 반환
        // return the constructed frame
        return frame;
    }

    /// <summary>
    ///     MODBUS 패스스루 요청 프레임을 생성한다 (MID 110).
    ///     Builds a MODBUS passthrough request frame (MID 110).
    /// </summary>
    /// <param name="modbusPacket">MODBUS TCP 프레임 (MBAP+PDU) / MODBUS TCP frame (MBAP+PDU)</param>
    /// <param name="toolId">대상 툴 ID (Unit ID에 설정) / target tool ID (set in Unit ID)</param>
    /// <param name="revision">프로토콜 리비전 / protocol revision</param>
    /// <returns>MID 110 프레임 바이트 배열 / MID 110 frame byte array</returns>
    internal static byte[] BuildModbusPassthrough(ReadOnlySpan<byte> modbusPacket, byte toolId, int revision = 0) {
        // MODBUS 패킷 복사본 생성 (Unit ID 수정용)
        // create a copy of the MODBUS packet (for Unit ID modification)
        var payload = modbusPacket.ToArray();

        // MODBUS TCP 프레임의 Unit ID 위치 (오프셋 6)에 툴 ID 설정
        // set tool ID at Unit ID position (offset 6) of MODBUS TCP frame
        if (payload.Length > 6)
            // Unit ID를 대상 툴 ID로 설정
            // set Unit ID to target tool ID
            payload[6] = toolId;

        // MID 110 메시지 생성
        // build MID 110 message
        return BuildMessage(MessageId.ModbusRequest, revision, payload);
    }

    /// <summary>
    ///     수신 버퍼에서 PRO X 프레임의 전체 길이를 계산한다.
    ///     Calculates the expected PRO X frame length from the receive buffer.
    /// </summary>
    /// <param name="buffer">수신 데이터가 담긴 링 버퍼 / ring buffer containing received data</param>
    /// <returns>프레임 전체 길이. 데이터 부족 시 -1 / total frame length, or -1 if insufficient data</returns>
    internal static int CalcFrameLength(RingBuffer buffer) {
        // 최소 헤더 크기 확인
        // check minimum header size
        if (buffer.Available < HeaderSize)
            // 헤더조차 읽을 수 없음 — 데이터 부족
            // cannot read even the header — insufficient data
            return -1;

        // 바이트 0~1에서 전체 길이 읽기 (Big-Endian)
        // read total length from bytes 0-1 (Big-Endian)
        var length = (buffer.Peek(0) << 8) | buffer.Peek(1);

        // 가드: 길이가 헤더 크기 미만이면 무효
        // guard: invalid if length is less than header size
        if (length < HeaderSize)
            // 잘못된 프레임 길이
            // invalid frame length
            return -1;

        // 전체 프레임 길이 반환
        // return total frame length
        return length;
    }

    /// <summary>
    ///     프레임에서 MID를 추출한다.
    ///     Extracts the MID from a frame.
    /// </summary>
    /// <param name="frame">완전한 프레임 데이터 / complete frame data</param>
    /// <returns>추출된 MID / extracted MID</returns>
    internal static MessageId ExtractMid(ReadOnlySpan<byte> frame) {
        // 바이트 2~3에서 MID 읽기 (Big-Endian)
        // read MID from bytes 2-3 (Big-Endian)
        return (MessageId)Utils.ReadUInt16(frame[2..]);
    }

    /// <summary>
    ///     프레임에서 리비전을 추출한다.
    ///     Extracts the revision from a frame.
    /// </summary>
    /// <param name="frame">완전한 프레임 데이터 / complete frame data</param>
    /// <returns>추출된 리비전 / extracted revision</returns>
    internal static int ExtractRevision(ReadOnlySpan<byte> frame) {
        // 바이트 4~5에서 리비전 읽기 (Big-Endian)
        // read revision from bytes 4-5 (Big-Endian)
        return Utils.ReadUInt16(frame[4..]);
    }

    /// <summary>
    ///     프레임에서 페이로드를 추출한다 (헤더 16바이트 제외).
    ///     Extracts the payload from a frame (excluding 16-byte header).
    /// </summary>
    /// <param name="frame">완전한 프레임 데이터 / complete frame data</param>
    /// <returns>페이로드 바이트 배열 / payload byte array</returns>
    internal static byte[] ExtractPayload(ReadOnlySpan<byte> frame) {
        // 페이로드가 있으면 헤더 이후 데이터 반환, 없으면 빈 배열
        // return data after header if payload exists, otherwise empty array
        return frame.Length > HeaderSize ? frame[HeaderSize..].ToArray() : [];
    }

    /// <summary>
    ///     MID 111 응답에서 MODBUS 페이로드를 추출한다.
    ///     Extracts the MODBUS payload from a MID 111 response.
    /// </summary>
    /// <param name="proPayload">PRO X 페이로드 (MID 111의 헤더 이후) / PRO X payload (after MID 111 header)</param>
    /// <returns>MODBUS TCP 페이로드 (MBAP 제외, FC+Data) / MODBUS TCP payload (excluding MBAP, FC+Data)</returns>
    internal static ReadOnlyMemory<byte> ExtractModbusPayload(ReadOnlySpan<byte> proPayload) {
        // 최소 크기 충족 시 MBAP(7바이트) 이후 FC+Data 반환, 미달 시 빈 메모리 반환
        // return FC+Data after MBAP (7 bytes) if minimum size met, otherwise empty
        return proPayload.Length >= 8 ? proPayload[7..].ToArray() : ReadOnlyMemory<byte>.Empty;
    }
}