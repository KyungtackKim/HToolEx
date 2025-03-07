using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Preset types
/// </summary>
[Flags]
public enum PresetTypes : ulong {
    [Description("None")] None = 0,
    [Description("Preset 1")] P1 = 1L << 0,
    [Description("Preset 2")] P2 = 1L << 1,
    [Description("Preset 3")] P3 = 1L << 2,
    [Description("Preset 4")] P4 = 1L << 3,
    [Description("Preset 5")] P5 = 1L << 4,
    [Description("Preset 6")] P6 = 1L << 5,
    [Description("Preset 7")] P7 = 1L << 6,
    [Description("Preset 8")] P8 = 1L << 7,
    [Description("Preset 9")] P9 = 1L << 8,
    [Description("Preset 10")] P10 = 1L << 9,
    [Description("Preset 11")] P11 = 1L << 10,
    [Description("Preset 12")] P12 = 1L << 11,
    [Description("Preset 13")] P13 = 1L << 12,
    [Description("Preset 14")] P14 = 1L << 13,
    [Description("Preset 15")] P15 = 1L << 14,
    [Description("Preset 16")] P16 = 1L << 15,
    [Description("Preset 17")] P17 = 1L << 16,
    [Description("Preset 18")] P18 = 1L << 17,
    [Description("Preset 19")] P19 = 1L << 18,
    [Description("Preset 20")] P20 = 1L << 19,
    [Description("Preset 21")] P21 = 1L << 20,
    [Description("Preset 22")] P22 = 1L << 21,
    [Description("Preset 23")] P23 = 1L << 22,
    [Description("Preset 24")] P24 = 1L << 23,
    [Description("Preset 25")] P25 = 1L << 24,
    [Description("Preset 26")] P26 = 1L << 25,
    [Description("Preset 27")] P27 = 1L << 26,
    [Description("Preset 28")] P28 = 1L << 27,
    [Description("Preset 29")] P29 = 1L << 28,
    [Description("Preset 30")] P30 = 1L << 29,
    [Description("Preset 31")] P31 = 1L << 30
}

/// <summary>
///     Preset MS types
/// </summary>
[Flags]
public enum PresetExtendTypes : ulong {
    [Description("None")] None = 0,
    [Description("Preset 1")] P1 = 1L << 0,
    [Description("Preset 2")] P2 = 1L << 1,
    [Description("Preset 3")] P3 = 1L << 2,
    [Description("Preset 4")] P4 = 1L << 3,
    [Description("Preset 5")] P5 = 1L << 4,
    [Description("Preset 6")] P6 = 1L << 5,
    [Description("Preset 7")] P7 = 1L << 6,
    [Description("Preset 8")] P8 = 1L << 7,
    [Description("Preset 9")] P9 = 1L << 8,
    [Description("Preset 10")] P10 = 1L << 9,
    [Description("Preset 11")] P11 = 1L << 10,
    [Description("Preset 12")] P12 = 1L << 11,
    [Description("Preset 13")] P13 = 1L << 12,
    [Description("Preset 14")] P14 = 1L << 13,
    [Description("Preset 15")] P15 = 1L << 14,
    [Description("Preset 16")] P16 = 1L << 15,
    [Description("Preset 17")] P17 = 1L << 16,
    [Description("Preset 18")] P18 = 1L << 17,
    [Description("Preset 19")] P19 = 1L << 18,
    [Description("Preset 20")] P20 = 1L << 19,
    [Description("Preset 21")] P21 = 1L << 20,
    [Description("Preset 22")] P22 = 1L << 21,
    [Description("Preset 23")] P23 = 1L << 22,
    [Description("Preset 24")] P24 = 1L << 23,
    [Description("Preset 25")] P25 = 1L << 24,
    [Description("Preset 26")] P26 = 1L << 25,
    [Description("Preset 27")] P27 = 1L << 26,
    [Description("Preset 28")] P28 = 1L << 27,
    [Description("Preset 29")] P29 = 1L << 28,
    [Description("Preset 30")] P30 = 1L << 29,
    [Description("Preset 31")] P31 = 1L << 30,
    [Description("MA")] Ma = 1L << 31
}