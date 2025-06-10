using HToolEz.Defines.Enums;

namespace HToolEz.Defines.Entities;

/// <summary>
///     Setting data class
/// </summary>
public sealed class SettingData {
    /// <summary>
    ///     Size
    /// </summary>
    public static readonly int[] Size = [19];

    private int _major, _minor, _patch;

    /// <summary>
    ///     Using for the target torque
    /// </summary>
    public bool UsingTargetTorque { get; set; }

    /// <summary>
    ///     Target torque
    /// </summary>
    public float TargetTorque { get; set; }

    /// <summary>
    ///     Auto clear time
    /// </summary>
    public AutoClearTimeTypes ClearTime { get; set; }

    /// <summary>
    ///     Tolerance
    /// </summary>
    public int Tolerance { get; set; }

    /// <summary>
    ///     Unit
    /// </summary>
    public UnitTypes Unit { get; set; }

    /// <summary>
    ///     Operation mode
    /// </summary>
    public OperationModeTypes Mode { get; set; }

    /// <summary>
    ///     Frequency
    /// </summary>
    public FreqTypes Frequency { get; set; }

    /// <summary>
    ///     Direction
    /// </summary>
    public DirectionTypes Direction { get; set; }

    /// <summary>
    ///     Version
    /// </summary>
    public string Version => $"{_major}.{_minor}.{_patch}";

    /// <summary>
    ///     Write to buffer
    /// </summary>
    /// <param name="buffer"></param>
    public void WriteTo(Span<byte> buffer) {
        // get data buffer
        var buf = buffer[5..];
        // write values
        buf[0] = (byte)(UsingTargetTorque ? 1 : 0);
        BitConverter.TryWriteBytes(buf[1..5], TargetTorque);
        buf[5] = (byte)ClearTime;
        buf[6] = (byte)Tolerance;
        buf[7] = (byte)Unit;
        buf[8] = (byte)Mode;
        buf[9] = (byte)Frequency;
        buf[10] = (byte)Direction;
        buf[11] = (byte)_major;
        buf[12] = (byte)_minor;
        buf[13] = (byte)_patch;
    }

    /// <summary>
    ///     Read from the packet
    /// </summary>
    /// <param name="buffer">buffer</param>
    public void ReadFrom(ReadOnlySpan<byte> buffer) {
        // get data buffer
        var buf = buffer[5..];
        // read values
        UsingTargetTorque = buf[0] > 0;
        TargetTorque = BitConverter.ToSingle(buf[1..5]);
        ClearTime = (AutoClearTimeTypes)buf[5];
        Tolerance = buf[6];
        Unit = (UnitTypes)buf[7];
        Mode = (OperationModeTypes)buf[8];
        Frequency = (FreqTypes)buf[9];
        Direction = (DirectionTypes)buf[10];
        _major = buf[11];
        _minor = buf[12];
        _patch = buf[13];
    }
}