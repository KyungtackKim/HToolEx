using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     MODBUS communication types
/// </summary>
[PublicAPI]
public enum ComTypes {
    [Description("MODBUS-RTU")] Rtu,
    [Description("MODBUS-TCP")] Tcp
}