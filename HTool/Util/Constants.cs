namespace HTool.Util;

/// <summary>
///     HTool 라이브러리의 통신 타이밍 및 설정 상수 클래스. 모든 타임아웃과 주기 값을 중앙에서 관리합니다.
///     HTool library communication timing and configuration constants class. Centrally manages all timeout and period
///     values.
/// </summary>
/// <remarks>
///     이 값들은 MODBUS 통신의 안정성과 성능에 직접적인 영향을 미칩니다. 변경 시 주의가 필요합니다.
///     These values directly affect MODBUS communication stability and performance. Modify with caution.
/// </remarks>
public static class Constants {
    /// <summary>
    ///     바코드 최대 길이 (바이트)
    ///     Maximum barcode length (bytes)
    /// </summary>
    public const int BarcodeLength = 64;

    /// <summary>
    ///     시리얼 통신 지원 보드레이트 목록
    ///     Supported baud rates for serial communication
    /// </summary>
    public static readonly int[] BaudRates = [9600, 19200, 38400, 57600, 115200, 230400];

    /// <summary>
    ///     메시지 처리 주기 (밀리초)
    ///     Message processing period (milliseconds)
    /// </summary>
    public static int ProcessPeriod => 20;

    /// <summary>
    ///     Keep-Alive 요청 전송 주기 (밀리초)
    ///     Keep-alive request transmission period (milliseconds)
    /// </summary>
    public static int KeepAlivePeriod => 3000;

    /// <summary>
    ///     처리 잠금 대기 시간 (밀리초)
    ///     Process lock wait timeout (milliseconds)
    /// </summary>
    public static int ProcessLockTime => 2;

    /// <summary>
    ///     메시지 처리 타임아웃 (밀리초)
    ///     Message processing timeout (milliseconds)
    /// </summary>
    public static int ProcessTimeout => 500;

    /// <summary>
    ///     연결 타임아웃 (밀리초)
    ///     Connection timeout (milliseconds)
    /// </summary>
    public static int ConnectTimeout => 5000;

    /// <summary>
    ///     메시지 응답 대기 타임아웃 (밀리초)
    ///     Message response wait timeout (milliseconds)
    /// </summary>
    public static int MessageTimeout => 1000;

    /// <summary>
    ///     Keep-Alive 응답 대기 타임아웃 (초)
    ///     Keep-alive response wait timeout (seconds)
    /// </summary>
    public static int KeepAliveTimeout => 10;
}