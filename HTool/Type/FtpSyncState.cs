using HTool.Device.Pro;

namespace HTool.Type;

/// <summary>
///     FTP 동기화 상태.
///     FTP synchronization state.
/// </summary>
/// <remarks>
///     사용처: <see cref="FtpService"/>.
///     used by: <see cref="FtpService"/>.
/// </remarks>
public enum FtpSyncState {
	/// <summary>
	///     초기 상태 (동기화 실행 전).
	///     Initial state (before synchronization).
	/// </summary>
	None,

	/// <summary>
	///     동기화 진행 중.
	///     Synchronization in progress.
	/// </summary>
	Syncing,

	/// <summary>
	///     동기화 완료.
	///     Synchronization completed.
	/// </summary>
	Synced,

	/// <summary>
	///     동기화 실패.
	///     Synchronization failed.
	/// </summary>
	Failed
}