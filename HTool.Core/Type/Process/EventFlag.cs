using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     이벤트 상태 플래그 열거형
///     event status flag enumeration
/// </summary>
[Flags]
public enum EventFlag {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None = 0,

    /// <summary>
    ///     기타
    ///     miscellaneous
    /// </summary>
    [Description("Etc.")]
    Etc = 1 << 0,

    /// <summary>
    ///     체결 OK
    ///     fastening OK
    /// </summary>
    [Description("Fastening OK")]
    FastenOk = 1 << 1,

    /// <summary>
    ///     체결 NG
    ///     fastening NG
    /// </summary>
    [Description("Fastening NG")]
    FastenNg = 1 << 2,

    /// <summary>
    ///     체결 / 풀림 전환
    ///     fasten-loosen toggle
    /// </summary>
    [Description("F / L")]
    Fl = 1 << 3,

    /// <summary>
    ///     프리셋 변경
    ///     preset change
    /// </summary>
    [Description("Preset change")]
    PresetChange = 1 << 4,

    /// <summary>
    ///     알람 리셋
    ///     alarm reset
    /// </summary>
    [Description("Alarm reset")]
    AlarmReset = 1 << 5,

    /// <summary>
    ///     에러
    ///     error
    /// </summary>
    [Description("Error")]
    Error = 1 << 6,

    /// <summary>
    ///     바코드
    ///     barcode
    /// </summary>
    [Description("Barcode")]
    Barcode = 1 << 7,

    /// <summary>
    ///     나사 취소
    ///     screw cancel
    /// </summary>
    [Description("Screw cancel")]
    ScrewCancel = 1 << 8,

    /// <summary>
    ///     나사 카운트 리셋
    ///     screw count reset
    /// </summary>
    [Description("Screw count reset")]
    ScrewCountReset = 1 << 9
}