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
    /// <summary>Invalid function code</summary>
    [Description("Invalid function")]
    InvalidFunction = 0x01,

    /// <summary>Invalid register address</summary>
    [Description("Invalid address")]
    InvalidAddress = 0x02,

    /// <summary>Invalid data value</summary>
    [Description("Invalid value")]
    InvalidValue = 0x03,

    /// <summary>CRC validation failed</summary>
    [Description("Invalid CRC")]
    InvalidCrc = 0x07,

    /// <summary>Invalid frame format</summary>
    [Description("Invalid frame")]
    InvalidFrame = 0x0C,

    /// <summary>Value out of valid range</summary>
    [Description("Invalid value range")]
    InvalidValueRange = 0x0E,

    /// <summary>Communication timeout</summary>
    [Description("Timeout")]
    Timeout = 0x0F
}