using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     MODBUS function code types
/// </summary>
public enum CodeTypes {
    [Description("Read holding register")] ReadHoldingReg = 0x03,
    [Description("Read input register")] ReadInputReg = 0x04,
    [Description("Write single register")] WriteSingleReg = 0x06,

    [Description("Write multiple register")]
    WriteMultiReg = 0x10,
    [Description("Graph register")] Graph = 0x64,
    [Description("Graph result register")] GraphRes = 0x65,
    [Description("Error")] Error = 0x80
}