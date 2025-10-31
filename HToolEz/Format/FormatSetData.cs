using HToolEz.Type;
using HToolEz.Util;

namespace HToolEz.Format;

/// <summary>
///     Format device setting data class (0x84 RES_SETTING_DATA)
/// </summary>
public sealed class FormatSetData {
    /// <summary>
    ///     Format setting data size
    /// </summary>
    private const int Size = 14;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="data">data packet</param>
    /// <exception cref="ArgumentException">Thrown when data size is not 14 bytes</exception>
    public FormatSetData(ReadOnlyMemory<byte> data) {
        // get the data size (only valid data)
        var size = data.Length;
        // check the data size
        if (size != Size)
            throw new ArgumentException($"Invalid setting data size: {size} bytes. Expected size: {Size} bytes.",
                nameof(data));

        // get the data
        var d = data.Span;

        /*      target information      */
        // check the target enable item
        if (Utils.IsKnownItem<TargetEnableTypes>(d[0]))
            // set the target enable
            TargetEnable = (TargetEnableTypes)d[0];

        // convert target torque (4 bytes, float, big-endian)
        Utils.ConvertValue(d[1..5], out float targetTorque);
        TargetTorque = targetTorque;

        /*      operation settings      */
        // check the auto clear time item
        if (Utils.IsKnownItem<AutoClearTimeTypes>(d[5]))
            // set the auto clear time
            AutoClearTime = (AutoClearTimeTypes)d[5];

        // set the tolerance
        Tolerance = d[6];

        // check the unit item
        if (Utils.IsKnownItem<UnitTypes>(d[7]))
            // set the unit
            Unit = (UnitTypes)d[7];

        // check the mode item
        if (Utils.IsKnownItem<OperationModeTypes>(d[8]))
            // set the mode
            Mode = (OperationModeTypes)d[8];

        // check the frequency item
        if (Utils.IsKnownItem<FrequencyTypes>(d[9]))
            // set the frequency
            Frequency = (FrequencyTypes)d[9];

        // check the direction item
        if (Utils.IsKnownItem<DirectionTypes>(d[10]))
            // set the direction
            Direction = (DirectionTypes)d[10];

        /*      firmware version        */
        FwMajor = d[11];
        FwMinor = d[12];
        FwMicro = d[13];
    }

    /// <summary>
    ///     Target enable/disable
    /// </summary>
    public TargetEnableTypes TargetEnable { get; } = TargetEnableTypes.Disable;

    /// <summary>
    ///     Target torque value
    /// </summary>
    public float TargetTorque { get; }

    /// <summary>
    ///     Auto clear time
    /// </summary>
    public AutoClearTimeTypes AutoClearTime { get; } = AutoClearTimeTypes.Disable;

    /// <summary>
    ///     Tolerance percentage
    /// </summary>
    public byte Tolerance { get; }

    /// <summary>
    ///     Torque unit
    /// </summary>
    public UnitTypes Unit { get; } = UnitTypes.KgfCm;

    /// <summary>
    ///     Operation mode
    /// </summary>
    public OperationModeTypes Mode { get; } = OperationModeTypes.Peak;

    /// <summary>
    ///     Sampling frequency
    /// </summary>
    public FrequencyTypes Frequency { get; } = FrequencyTypes.Hz100;

    /// <summary>
    ///     Torque direction
    /// </summary>
    public DirectionTypes Direction { get; } = DirectionTypes.CW;

    /// <summary>
    ///     Firmware major version
    /// </summary>
    public byte FwMajor { get; }

    /// <summary>
    ///     Firmware minor version
    /// </summary>
    public byte FwMinor { get; }

    /// <summary>
    ///     Firmware micro version
    /// </summary>
    public byte FwMicro { get; }

    /// <summary>
    ///     Firmware version string (Major.Minor.Micro)
    /// </summary>
    public string Firmware => $"{FwMajor}.{FwMinor}.{FwMicro}";

    /// <summary>
    ///     Convert setting data to byte array
    /// </summary>
    /// <returns>byte array (14 bytes)</returns>
    public byte[] ToBytes() {
        var bytes = new byte[Size];

        // target information (0-4)
        bytes[0] = (byte)TargetEnable;
        Utils.WriteValue(bytes, 1, TargetTorque);

        // operation settings (5-10)
        bytes[5]  = (byte)AutoClearTime;
        bytes[6]  = Tolerance;
        bytes[7]  = (byte)Unit;
        bytes[8]  = (byte)Mode;
        bytes[9]  = (byte)Frequency;
        bytes[10] = (byte)Direction;

        // firmware version (11-13)
        bytes[11] = FwMajor;
        bytes[12] = FwMinor;
        bytes[13] = FwMicro;

        return bytes;
    }
}