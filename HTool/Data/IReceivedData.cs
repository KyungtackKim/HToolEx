using HTool.Type;
using JetBrains.Annotations;

namespace HTool.Data;

/// <summary>
///     Received data format interface
/// </summary>
[PublicAPI]
public interface IReceivedData {
    /// <summary>
    ///     MODBUS function code
    /// </summary>
    CodeTypes Code { get; }

    /// <summary>
    ///     Data length
    /// </summary>
    int Length { get; }

    /// <summary>
    ///     Data
    /// </summary>
    byte[] Data { get; }
}