using System.IO.Ports;

namespace HTool.Type;

/// <summary>
///     HTool 인스턴스별 설정. 인스턴스 생성 후, Connect() 호출 전에 설정한다.
///     Per-instance settings for HTool. Configure after construction, before Connect().
/// </summary>
public sealed class HToolSettings {
	/// <summary>
	///     연결 파라미터 설정.
	///     Connection parameter settings.
	/// </summary>
	public ConnectionSettings Connection { get; } = new();

	/// <summary>
	///     MODBUS 요청/응답 파이프라인 설정.
	///     MODBUS request/response pipeline settings.
	/// </summary>
	public PipelineSettings Pipeline { get; } = new();

	/// <summary>
	///     직접 연결 Keep-Alive 설정.
	///     Direct connection keep-alive settings.
	/// </summary>
	public KeepAliveSettings KeepAlive { get; } = new();

	/// <summary>
	///     PRO X 프로토콜 설정.
	///     PRO X protocol settings.
	/// </summary>
	public ProSettings Pro { get; } = new();

	/// <summary>
	///     연결 파라미터 설정 그룹.
	///     Connection parameters settings group.
	/// </summary>
	public sealed class ConnectionSettings {
		/// <summary>
		///     TCP 연결 타임아웃 (밀리초). 기본값 5000.
		///     TCP connect timeout (ms). Default 5000.
		/// </summary>
		public int TcpConnectTimeout { get; set; } = 5000;

		/// <summary>
		///     RTU 패리티. 기본값 None.
		///     RTU parity. Default None.
		/// </summary>
		public Parity RtuParity { get; set; } = Parity.None;

		/// <summary>
		///     RTU 스톱 비트. 기본값 One.
		///     RTU stop bits. Default One.
		/// </summary>
		public StopBits RtuStopBits { get; set; } = StopBits.One;
    }

	/// <summary>
	///     MODBUS 파이프라인 설정 그룹.
	///     MODBUS pipeline settings group.
	/// </summary>
	public sealed class PipelineSettings {
		/// <summary>
		///     메시지 응답 타임아웃 (밀리초). 기본값 1000.
		///     Message response timeout (ms). Default 1000.
		/// </summary>
		public int MessageTimeout { get; set; } = 1000;

		/// <summary>
		///     메시지 재시도 횟수. 기본값 1 (총 2회 시도).
		///     Message retry count. Default 1 (total 2 attempts).
		/// </summary>
		public int MessageRetry { get; set; } = 1;

		/// <summary>
		///     프레임 수신 타임아웃 (밀리초). 불완전 프레임 정리용. 기본값 500.
		///     Frame receive timeout (ms). For clearing incomplete frames. Default 500.
		/// </summary>
		public int FrameTimeout { get; set; } = 500;
    }

	/// <summary>
	///     직접 연결 Keep-Alive 설정 그룹.
	///     Direct connection keep-alive settings group.
	/// </summary>
	public sealed class KeepAliveSettings {
		/// <summary>
		///     Keep-Alive 활성화 여부. 기본값 false.
		///     Whether keep-alive is enabled. Default false.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		///     Keep-Alive 폴링 주기 (밀리초). 기본값 3000.
		///     Keep-alive polling period (ms). Default 3000.
		/// </summary>
		public int Period { get; set; } = 3000;

		/// <summary>
		///     Keep-Alive 타임아웃 (밀리초). 기본값 10000.
		///     Keep-alive timeout (ms). Default 10000.
		/// </summary>
		public int Timeout { get; set; } = 10000;
    }

	/// <summary>
	///     PRO X 프로토콜 설정 그룹.
	///     PRO X protocol settings group.
	/// </summary>
	public sealed class ProSettings {
		/// <summary>
		///     PRO X Keep-Alive 송신 주기 (밀리초). 기본값 5000.
		///     PRO X keep-alive send period (ms). Default 5000.
		/// </summary>
		public int KeepAlivePeriod { get; set; } = 5000;

		/// <summary>
		///     PRO X Keep-Alive 타임아웃 (밀리초). 기본값 15000.
		///     PRO X keep-alive timeout (ms). Default 15000.
		/// </summary>
		public int KeepAliveTimeout { get; set; } = 15000;

		/// <summary>
		///     PRO X Keep-Alive 최대 미응답 횟수. 기본값 3.
		///     PRO X keep-alive maximum missed count. Default 3.
		/// </summary>
		public int KeepAliveMissLimit { get; set; } = 3;

		/// <summary>
		///     PRO X 프레임 수신 타임아웃 (밀리초). 기본값 3000.
		///     PRO X frame receive timeout (ms). Default 3000.
		/// </summary>
		public int FrameTimeout { get; set; } = 3000;
    }
}