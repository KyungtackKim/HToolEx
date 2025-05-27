using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Model command types
/// </summary>
public enum ModelCmdTypes {
    [Description("None")]
    None,
    [Description("Fastening")]
    Fastening,
    [Description("Delay")]
    Delay,
    [Description("Input")]
    Input,
    [Description("Output")]
    Output,
    [Description("Barcode")]
    Barcode
}