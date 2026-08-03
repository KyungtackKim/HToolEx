namespace ToolRunner.Models;

/// <summary>
///     MODBUS holding-register addresses and values the tool controller exposes for external
///     run control. written with function code 0x06 (write single register).
///     외부 제어용 홀딩 레지스터 주소 및 값
/// </summary>
public static class ControlRegisters {
    /// <summary>
    ///     run/stop command register — 1 starts the motor, 0 stops it.
    ///     운전/정지 명령 레지스터
    /// </summary>
    public const ushort RunStop = 5000;

    /// <summary>
    ///     rotation direction register — see <see cref="RunDirection" /> for the accepted values.
    ///     회전 방향 레지스터
    /// </summary>
    public const ushort Direction = 5001;

    /// <summary>
    ///     value written to <see cref="RunStop" /> to start the motor.
    ///     운전 시작 값
    /// </summary>
    public const ushort RunValue = 1;

    /// <summary>
    ///     value written to <see cref="RunStop" /> to stop the motor.
    ///     정지 값
    /// </summary>
    public const ushort StopValue = 0;
}
