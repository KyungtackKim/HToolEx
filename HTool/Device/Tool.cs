using System.Runtime.CompilerServices;
using HTool.Type;

namespace HTool.Device;

/// <summary>
///     Hantas tool class
/// </summary>
public abstract class Tool {
    /// <summary>
    ///     Create the communication tool
    /// </summary>
    /// <param name="type">Communication type (RTU or TCP)</param>
    /// <returns>Created tool instance</returns>
    public static ITool Create(ComTypes type) {
        return type switch {
            ComTypes.Rtu => new HcRtu(),
            ComTypes.Tcp => new HcTcp(),
            _            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    ///     Check the known code
    /// </summary>
    /// <param name="v">MODBUS function code value</param>
    /// <returns>True if the code is known, otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKnownCode(byte v) {
        // check the known code
        return v is (byte)CodeTypes.ReadHoldingReg or (byte)CodeTypes.ReadInputReg
            or (byte)CodeTypes.ReadInfoReg or (byte)CodeTypes.WriteSingleReg
            or (byte)CodeTypes.WriteMultiReg or (byte)CodeTypes.Graph
            or (byte)CodeTypes.GraphRes;
    }
}