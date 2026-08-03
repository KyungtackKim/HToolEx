namespace ToolRunner.Models;

/// <summary>
///     motor rotation direction written to <see cref="ControlRegisters.Direction" />.
///     the underlying value is the register value itself, so the enum casts straight to ushort.
///     회전 방향 (레지스터 값과 동일)
/// </summary>
public enum RunDirection : ushort {
    /// <summary>
    ///     clockwise — fastening direction (register value 0).
    ///     정방향 (체결)
    /// </summary>
    Cw = 0,

    /// <summary>
    ///     counter-clockwise — loosening direction (register value 1).
    ///     역방향 (풀림)
    /// </summary>
    Ccw = 1
}
