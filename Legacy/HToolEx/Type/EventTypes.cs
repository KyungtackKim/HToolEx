using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Event status types
/// </summary>
public enum EventTypes {
    [Display(Description = @"EventEtc", ResourceType = typeof(HToolExRes))]
    Etc,
    [Display(Description = @"EventFastenOk", ResourceType = typeof(HToolExRes))]
    FastenOk,
    [Display(Description = @"EventFastenNg", ResourceType = typeof(HToolExRes))]
    FastenNg,
    [Display(Description = @"EventDir", ResourceType = typeof(HToolExRes))]
    Fl,
    [Display(Description = @"EventPreset", ResourceType = typeof(HToolExRes))]
    PresetChange,
    [Display(Description = @"EventAlarmReset", ResourceType = typeof(HToolExRes))]
    AlarmReset,
    [Display(Description = @"EventError", ResourceType = typeof(HToolExRes))]
    Error,
    [Display(Description = @"EventBarcode", ResourceType = typeof(HToolExRes))]
    Barcode,
    [Display(Description = @"EventScrewCancel", ResourceType = typeof(HToolExRes))]
    ScrewCancel,
    [Display(Description = @"EventScrewReset", ResourceType = typeof(HToolExRes))]
    ScrewCountReset
}

/// <summary>
///     Event status flag types
/// </summary>
[Flags]
public enum EventFlagTypes {
    [Display(Description = @"EventNone", ResourceType = typeof(HToolExRes))]
    None = 0,
    [Display(Description = @"EventEtc", ResourceType = typeof(HToolExRes))]
    Etc = 1 << 0,
    [Display(Description = @"EventFastenOk", ResourceType = typeof(HToolExRes))]
    FastenOk = 1 << 1,
    [Display(Description = @"EventFastenNg", ResourceType = typeof(HToolExRes))]
    FastenNg = 1 << 2,
    [Display(Description = @"EventDir", ResourceType = typeof(HToolExRes))]
    Fl = 1 << 3,
    [Display(Description = @"EventPreset", ResourceType = typeof(HToolExRes))]
    PresetChange = 1 << 4,
    [Display(Description = @"EventAlarmReset", ResourceType = typeof(HToolExRes))]
    AlarmReset = 1 << 5,
    [Display(Description = @"EventError", ResourceType = typeof(HToolExRes))]
    Error = 1 << 6,
    [Display(Description = @"EventBarcode", ResourceType = typeof(HToolExRes))]
    Barcode = 1 << 7,
    [Display(Description = @"EventScrewCancel", ResourceType = typeof(HToolExRes))]
    ScrewCancel = 1 << 8,
    [Display(Description = @"EventScrewReset", ResourceType = typeof(HToolExRes))]
    ScrewCountReset = 1 << 9
}