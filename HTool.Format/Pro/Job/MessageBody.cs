using System.Buffers.Binary;
using System.Text;
using HTool.Core.Type.Pro;
using HTool.Core.Util;

namespace HTool.Format.Pro.Job;

/// <summary>
///     메시지 스텝 본문. 표시 메시지, 이미지 경로, 동작 유형을 포함합니다.
///     message step body. contains display messages, image path, and action type.
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. MessageType에 따라 DelayTime 포함 여부가 결정됩니다.
///     little-endian binary format. whether DelayTime is included depends on MessageType.
/// </remarks>
public sealed class MessageBody : IStepBody {
	/// <summary>
	///     메시지 필드 크기 (바이트)
	///     message field size (bytes)
	/// </summary>
	private const int MessageFieldSize = 128;

	/// <summary>
	///     이미지 경로 필드 크기 (바이트)
	///     image path field size (bytes)
	/// </summary>
	private const int ImagePathFieldSize = 256;

	/// <summary>
	///     표시 메시지 (3줄, 각 128바이트)
	///     display messages (3 lines, 128 bytes each)
	/// </summary>
	public string[] Message { get; } = [string.Empty, string.Empty, string.Empty];

	/// <summary>
	///     이미지 파일 경로 (256바이트)
	///     image file path (256 bytes)
	/// </summary>
	public string ImagePath { get; set; } = string.Empty;

	/// <summary>
	///     메시지 동작 유형
	///     message action type
	/// </summary>
	public StepMessage MessageType { get; set; } = StepMessage.Validation;

	/// <summary>
	///     지연 시간 (DelayTime 모드 전용)
	///     delay time (DelayTime mode only)
	/// </summary>
	public int DelayTime { get; set; }

	/// <summary>
	///     스텝 유형 (메시지)
	///     step type (message)
	/// </summary>
	public JobStep StepType => JobStep.Message;

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

        // 메시지 3줄 쓰기 (각 128바이트 ASCII, 널 패딩)
        // write 3 message lines (128 bytes ASCII each, null-padded)
        foreach (var msg in Message) {
            // 메시지 바이트 가져오기
            // get message bytes
            var msgBytes = Encoding.ASCII.GetBytes(msg);
            // 메시지 바이트 쓰기
            // write message bytes
            bin.Write(msgBytes);
            // 패딩 쓰기
            // write padding
            bin.Write(new byte[MessageFieldSize - msgBytes.Length]);
        }

        // 이미지 경로 쓰기 (256바이트 ASCII, 널 패딩)
        // write image path (256 bytes ASCII, null-padded)
        var pathBytes = Encoding.ASCII.GetBytes(ImagePath);
        // 경로 바이트 쓰기
        // write path bytes
        bin.Write(pathBytes);
        // 패딩 쓰기
        // write padding
        bin.Write(new byte[ImagePathFieldSize - pathBytes.Length]);

        // 메시지 유형 쓰기
        // write message type
        bin.Write((int)MessageType);

        // 메시지 유형에 따라 분기 직렬화
        // branch serialization by message type
        switch (MessageType) {
            case StepMessage.DelayTime:
                // 지연 시간 쓰기
                // write delay time
                bin.Write(DelayTime);
                // switch 종료
                // exit switch
                break;
            case StepMessage.Validation:
            case StepMessage.NextStep:
            default:
                // 추가 데이터 없음
                // no additional data
                break;
        }

        // 직렬화된 배열 반환
        // return serialized array
        return stream.ToArray();
    }

	/// <summary>
	///     원시 스팬에서 메시지 본문을 파싱합니다 (리틀엔디안).
	///     parses message body from raw span (little-endian).
	/// </summary>
	/// <param name="data">헤더 이후의 데이터 스팬 / data span after header</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>파싱된 메시지 본문 / parsed message body</returns>
	public static MessageBody Parse(ReadOnlySpan<byte> data, int revision = 0) {
        // 본문 인스턴스 생성
        // create body instance
        var body = new MessageBody();
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 메시지 3줄 읽기 (각 128바이트 ASCII)
        // read 3 message lines (128 bytes ASCII each)
        for (var i = 0; i < 3; i++) {
            // 메시지 읽기
            // read message
            body.Message[i] = Encoding.ASCII.GetString(data.Slice(pos, MessageFieldSize)).TrimEnd('\0');
            // 메시지 필드 크기만큼 위치 이동
            // advance position by message field size
            pos += MessageFieldSize;
        }

        // 이미지 경로 읽기 (256바이트 ASCII)
        // read image path (256 bytes ASCII)
        body.ImagePath = Encoding.ASCII.GetString(data.Slice(pos, ImagePathFieldSize)).TrimEnd('\0');
        // 이미지 경로 필드 크기만큼 위치 이동
        // advance position by image path field size
        pos += ImagePathFieldSize;

        // 메시지 유형 읽기 (int32 리틀엔디안)
        // read message type (int32 little-endian)
        var msgType = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 정의된 유형인지 확인
        // check if type is defined
        if (EnumUtil.IsDefined<StepMessage>(msgType))
            // 메시지 유형 설정
            // set message type
            body.MessageType = (StepMessage)msgType;

        // 메시지 유형에 따라 분기 파싱
        // branch parsing by message type
        switch (body.MessageType) {
            case StepMessage.DelayTime:
                // 지연 시간 읽기
                // read delay time
                body.DelayTime = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
                // switch 종료
                // exit switch
                break;
            case StepMessage.Validation:
            case StepMessage.NextStep:
            default:
                // 추가 데이터 없음
                // no additional data
                break;
        }

        // 파싱된 본문 반환
        // return parsed body
        return body;
    }
}