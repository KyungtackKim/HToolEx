using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Event status types
/// </summary>
[PublicAPI]
public enum EventTypes {
    [Description("Etc.")]
    Etc,
    [Description("Fastening OK")]
    FastenOk,
    [Description("Fastening NG")]
    FastenNg,
    [Description("F / L")]
    Fl,
    [Description("Preset change")]
    PresetChange,
    [Description("Alarm reset")]
    AlarmReset,
    [Description("Error")]
    Error,
    [Description("Barcode")]
    Barcode,
    [Description("Screw cancel")]
    ScrewCancel,
    [Description("Screw count reset")]
    ScrewCountReset
}

/// <summary>
///     Event status flag types
/// </summary>
[Flags]
[PublicAPI]
public enum EventFlagTypes {
    [Description("None")]
    None = 0,
    [Description("Etc.")]
    Etc = 1 << 0,
    [Description("Fastening OK")]
    FastenOk = 1 << 1,
    [Description("Fastening NG")]
    FastenNg = 1 << 2,
    [Description("F / L")]
    Fl = 1 << 3,
    [Description("Preset change")]
    PresetChange = 1 << 4,
    [Description("Alarm reset")]
    AlarmReset = 1 << 5,
    [Description("Error")]
    Error = 1 << 6,
    [Description("Barcode")]
    Barcode = 1 << 7,
    [Description("Screw cancel")]
    ScrewCancel = 1 << 8,
    [Description("Screw count reset")]
    ScrewCountReset = 1 << 9
}