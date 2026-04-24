using HTool.Device;
using HTool.Device.Pro;

namespace HTool.Type;

/// <summary>
///     로그 카테고리를 정의한다. 비트 플래그로 복수 선택 가능.
///     Defines log categories. Multiple categories can be selected via bit flags.
/// </summary>
/// <remarks>
///     사용처: <see cref="HToolLogger"/>, <see cref="MessagePipeline"/>, <see cref="ProService"/>, <see cref="FtpService"/>.
///     used by: <see cref="HToolLogger"/>, <see cref="MessagePipeline"/>, <see cref="ProService"/>, <see cref="FtpService"/>.
/// </remarks>
[Flags]
public enum LogCategories {
	/// <summary>
	///     로그 비활성화.
	///     Logging disabled.
	/// </summary>
	None = 0,

	/// <summary>
	///     원시 패킷 데이터.
	///     Raw packet data.
	/// </summary>
	Packet = 1 << 0,

	/// <summary>
	///     연결 상태 변경.
	///     Connection state changes.
	/// </summary>
	Connection = 1 << 1,

	/// <summary>
	///     메시지 파이프라인 처리.
	///     Message pipeline processing.
	/// </summary>
	Pipeline = 1 << 2,

	/// <summary>
	///     Keep-Alive 활동.
	///     Keep-Alive activity.
	/// </summary>
	KeepAlive = 1 << 3,

	/// <summary>
	///     PRO X 게이트웨이 통신.
	///     PRO X gateway communication.
	/// </summary>
	Pro = 1 << 4,

	/// <summary>
	///     FTP 파일 전송.
	///     FTP file transfer.
	/// </summary>
	Ftp = 1 << 5,

	/// <summary>
	///     오류 이벤트.
	///     Error events.
	/// </summary>
	Error = 1 << 6,

	/// <summary>
	///     모든 카테고리.
	///     All categories.
	/// </summary>
	All = ~0
}