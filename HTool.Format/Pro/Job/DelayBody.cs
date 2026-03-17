using System.Buffers.Binary;
using System.Text;
using HTool.Core.Type.Pro;
using HTool.Core.Util;

namespace HTool.Format.Pro.Job;

/// <summary>
///     지연 스텝 본문. 시간/팝업/바코드 모드에 따라 데이터 구조가 달라집니다.
///     delay step body. data structure varies by mode: time, pop-up, or barcode.
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. DelayType에 따라 파싱/직렬화 분기.
///     little-endian binary format. parsing/serialization branches by DelayType.
/// </remarks>
public sealed class DelayBody : IStepBody {
    /// <summary>
    ///     지연 모드 유형
    ///     delay mode type
    /// </summary>
    public Delay DelayType { get; set; }

    /// <summary>
    ///     지연 시간 (시간 모드)
    ///     delay time (time mode)
    /// </summary>
    public int DelayTime { get; set; }

    /// <summary>
    ///     지연 시간 단위 (시간 모드)
    ///     delay time unit (time mode)
    /// </summary>
    public DelayTimeUnit DelayTimeUnit { get; set; }

    /// <summary>
    ///     팝업 메시지 (팝업 모드, 3줄)
    ///     pop-up messages (pop-up mode, 3 lines)
    /// </summary>
    public string[] Message { get; } = [string.Empty, string.Empty, string.Empty];

    /// <summary>
    ///     바코드 코드 (바코드 모드)
    ///     barcode code (barcode mode)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     마스크 사용 여부 (바코드 모드)
    ///     mask enabled (barcode mode)
    /// </summary>
    public bool IsMask { get; set; }

    /// <summary>
    ///     마스크 상위 32비트 (바코드 모드)
    ///     mask upper 32 bits (barcode mode)
    /// </summary>
    public uint MaskHigh { get; set; }

    /// <summary>
    ///     마스크 하위 32비트 (바코드 모드)
    ///     mask lower 32 bits (barcode mode)
    /// </summary>
    public uint MaskLow { get; set; }

    /// <summary>
    ///     64비트 마스크 합성 값 (바코드 모드)
    ///     combined 64-bit mask value (barcode mode)
    /// </summary>
    public ulong Mask => ((ulong)MaskHigh << 32) | MaskLow;

    /// <summary>
    ///     스텝 유형 (지연)
    ///     step type (delay)
    /// </summary>
    public JobStep StepType => JobStep.Delay;

    /// <summary>
    ///     본문을 바이트 배열로 직렬화합니다 (리틀엔디안).
    ///     serializes body to a byte array (little-endian).
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
    public byte[] ToBytes(int revision = 0) {
        // 스트림 생성
        // create stream
        using var stream = new MemoryStream();
        // 바이너리 라이터 생성
        // create binary writer
        using var bin = new BinaryWriter(stream);

        // 지연 유형 쓰기
        // write delay type
        bin.Write((int)DelayType);

        // 지연 유형에 따라 분기 직렬화
        // branch serialization by delay type
        switch (DelayType) {
            case Delay.Time:
                // 지연 시간 쓰기
                // write delay time
                bin.Write(DelayTime);
                // 시간 단위 쓰기
                // write time unit
                bin.Write((int)DelayTimeUnit);
                // switch 종료
                // exit switch
                break;
#pragma warning disable CS0618 // Type or member is obsolete
            case Delay.PopUp:
                // 팝업 메시지 3줄 쓰기 (각 128바이트 ASCII, 널 패딩)
                // write 3 pop-up message lines (128 bytes ASCII each, null-padded)
                foreach (var msg in Message) {
                    // 메시지 바이트 가져오기
                    // get message bytes
                    var msgBytes = Encoding.ASCII.GetBytes(msg);
                    // 메시지 바이트 쓰기
                    // write message bytes
                    bin.Write(msgBytes);
                    // 패딩 쓰기
                    // write padding
                    bin.Write(new byte[128 - msgBytes.Length]);
                }

                // switch 종료
                // exit switch
                break;
            case Delay.Barcode:
                // 바코드 코드 쓰기 (68바이트 ASCII, 널 패딩)
                // write barcode code (68 bytes ASCII, null-padded)
                var codeBytes = Encoding.ASCII.GetBytes(Code);
                // 바코드 바이트 쓰기
                // write barcode bytes
                bin.Write(codeBytes);
                // 패딩 쓰기
                // write padding
                bin.Write(new byte[68 - codeBytes.Length]);
                // 마스크 상태 쓰기
                // write mask status
                bin.Write(Convert.ToInt32(IsMask));
                // 마스크 상위 쓰기
                // write mask high
                bin.Write(MaskHigh);
                // 마스크 하위 쓰기
                // write mask low
                bin.Write(MaskLow);
                // switch 종료
                // exit switch
                break;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        // 직렬화된 배열 반환
        // return serialized array
        return stream.ToArray();
    }

    /// <summary>
    ///     원시 스팬에서 지연 본문을 파싱합니다 (리틀엔디안).
    ///     parses delay body from raw span (little-endian).
    /// </summary>
    /// <param name="data">헤더 이후의 데이터 스팬 / data span after header</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>파싱된 지연 본문 / parsed delay body</returns>
    public static DelayBody Parse(ReadOnlySpan<byte> data, int revision = 0) {
        // 본문 인스턴스 생성
        // create body instance
        var body = new DelayBody();
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 지연 유형 읽기 (int32 리틀엔디안)
        // read delay type (int32 little-endian)
        var delay = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 정의된 유형인지 확인
        // check if type is defined
        if (EnumUtil.IsDefined<Delay>(delay))
            // 지연 유형 설정
            // set delay type
            body.DelayType = (Delay)delay;

        // 지연 유형에 따라 분기 파싱
        // branch parsing by delay type
        switch (body.DelayType) {
            case Delay.Time:
                // 지연 시간 읽기
                // read delay time
                body.DelayTime = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
                // 위치 4바이트 이동
                // advance position by 4 bytes
                pos += 4;
                // 시간 단위 읽기
                // read time unit
                var unit = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
                // 정의된 단위인지 확인
                // check if unit is defined
                if (EnumUtil.IsDefined<DelayTimeUnit>(unit))
                    // 시간 단위 설정
                    // set time unit
                    body.DelayTimeUnit = (DelayTimeUnit)unit;
                // switch 종료
                // exit switch
                break;
#pragma warning disable CS0618 // Type or member is obsolete
            case Delay.PopUp:
                // 팝업 메시지 3줄 읽기 (각 128바이트 ASCII)
                // read 3 pop-up message lines (128 bytes ASCII each)
                for (var i = 0; i < 3; i++) {
                    // 메시지 읽기
                    // read message
                    body.Message[i] = Encoding.ASCII.GetString(data.Slice(pos, 128)).TrimEnd('\0');
                    // 위치 128바이트 이동
                    // advance position by 128 bytes
                    pos += 128;
                }

                // switch 종료
                // exit switch
                break;
            case Delay.Barcode:
                // 바코드 코드 읽기 (68바이트 ASCII)
                // read barcode code (68 bytes ASCII)
                body.Code = Encoding.ASCII.GetString(data.Slice(pos, 68)).TrimEnd('\0');
                // 위치 68바이트 이동
                // advance position by 68 bytes
                pos += 68;
                // 마스크 상태 읽기 (int32 → bool)
                // read mask status (int32 -> bool)
                body.IsMask = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
                // 위치 4바이트 이동
                // advance position by 4 bytes
                pos += 4;
                // 마스크 상위 읽기
                // read mask high
                body.MaskHigh = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
                // 위치 4바이트 이동
                // advance position by 4 bytes
                pos += 4;
                // 마스크 하위 읽기
                // read mask low
                body.MaskLow = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
                // switch 종료
                // exit switch
                break;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        // 파싱된 본문 반환
        // return parsed body
        return body;
    }
}