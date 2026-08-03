namespace ToolRunner.Models;

/// <summary>
///     the operator input that survives a restart — connection target, cycle timings and the
///     collapsed/expanded state of the info panel. serialized to JSON, so every member needs a
///     setter and a sensible default for the first run.
///     재시작 후에도 유지되는 사용자 설정 (JSON 직렬화 대상)
/// </summary>
public sealed class AppSettings {
    /// <summary>
    ///     target IP address of the tool controller.
    ///     컨트롤러 IP 주소
    /// </summary>
    public string IpAddress { get; set; } = "192.168.1.100";

    /// <summary>
    ///     target MODBUS-TCP port.
    ///     MODBUS-TCP 포트
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    ///     MODBUS slave id.
    ///     MODBUS 슬레이브 ID
    /// </summary>
    public byte SlaveId { get; set; } = 1;

    /// <summary>
    ///     whether keep-alive polling is enabled.
    ///     Keep-Alive 사용 여부
    /// </summary>
    public bool KeepAliveEnabled { get; set; } = true;

    /// <summary>
    ///     wait inserted before every run command — after Start and after each direction switch (ms).
    ///     Run 전 대기 시간 (ms)
    /// </summary>
    public int WaitBeforeRunMs { get; set; } = SequenceSettings.Default.WaitBeforeRunMs;

    /// <summary>
    ///     run duration of each clockwise (fastening) cycle (ms).
    ///     정방향 Run 유지 시간 (ms)
    /// </summary>
    public int CwRunDurationMs { get; set; } = SequenceSettings.Default.CwRunDurationMs;

    /// <summary>
    ///     run duration of each counter-clockwise (loosening) cycle (ms).
    ///     역방향 Run 유지 시간 (ms)
    /// </summary>
    public int CcwRunDurationMs { get; set; } = SequenceSettings.Default.CcwRunDurationMs;

    /// <summary>
    ///     direction written once before the first run command.
    ///     시작 방향
    /// </summary>
    public RunDirection InitialDirection { get; set; } = SequenceSettings.Default.InitialDirection;

    /// <summary>
    ///     whether the tool information and log panel was left expanded.
    ///     정보·로그 패널 펼침 상태
    /// </summary>
    public bool IsDetailsVisible { get; set; }
}
