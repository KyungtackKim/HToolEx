namespace HToolEx.Type;

/// <summary>
///     Event status types
/// </summary>
public enum EventTypes {
    Etc,
    FastenOk,
    FastenNg,
    Fl,
    PresetChange,
    AlarmReset,
    Error,
    Barcode,
    ScrewCancel,
    ScrewCountReset
}

/// <summary>
///     Event status flag types
/// </summary>
[Flags]
public enum EventFlagTypes {
    None            = 0,
    Etc             = 1 << 0,
    FastenOk        = 1 << 1,
    FastenNg        = 1 << 2,
    Fl              = 1 << 3,
    PresetChange    = 1 << 4,
    AlarmReset      = 1 << 5,
    Error           = 1 << 6,
    Barcode         = 1 << 7,
    ScrewCancel     = 1 << 8,
    ScrewCountReset = 1 << 9
}