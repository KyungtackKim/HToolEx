using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     MODBUS communication types
/// </summary>
public enum ComTypes {
    [Description("MODBUS-RTU")]
    Rtu,
    [Description("MODBUS-TCP")]
    Tcp
}

/// <summary>
///     MODBUS communication error types
/// </summary>
public enum ComErrorTypes {
    InvalidFunction   = 0x01,
    InvalidAddress    = 0x02,
    InvalidValue      = 0x03,
    InvalidCrc        = 0x07,
    InvalidFrame      = 0x0C,
    InvalidValueRange = 0x0E,
    Timeout           = 0x0F
}