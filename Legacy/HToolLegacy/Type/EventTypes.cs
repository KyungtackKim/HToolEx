using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     체결 작업 이벤트 종류 열거형. FormatEvent에서 발생한 이벤트 유형(OK, NG, 오류 등)을 나타냅니다.
///     Fastening operation event type enum. Represents event type (OK, NG, error, etc.) from FormatEvent.
/// </summary>
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
///     이벤트 상태 플래그 타입
///     Event status flag types
/// </summary>
[Flags]
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