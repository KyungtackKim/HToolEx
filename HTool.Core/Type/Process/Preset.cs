using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     프리셋 플래그 열거형. P1~P31 및 Ma(다축) 플래그를 포함합니다.
///     preset flag enumeration. Includes P1 through P31 and the Ma (multi-axis) flag.
/// </summary>
[Flags]
public enum Preset : ulong {
    /// <summary>
    ///     없음
    ///     none
    /// </summary>
    [Description("None")]
    None = 0,

    /// <summary>
    ///     프리셋 1
    ///     preset 1
    /// </summary>
    [Description("Preset 1")]
    P1 = 1L << 0,

    /// <summary>
    ///     프리셋 2
    ///     preset 2
    /// </summary>
    [Description("Preset 2")]
    P2 = 1L << 1,

    /// <summary>
    ///     프리셋 3
    ///     preset 3
    /// </summary>
    [Description("Preset 3")]
    P3 = 1L << 2,

    /// <summary>
    ///     프리셋 4
    ///     preset 4
    /// </summary>
    [Description("Preset 4")]
    P4 = 1L << 3,

    /// <summary>
    ///     프리셋 5
    ///     preset 5
    /// </summary>
    [Description("Preset 5")]
    P5 = 1L << 4,

    /// <summary>
    ///     프리셋 6
    ///     preset 6
    /// </summary>
    [Description("Preset 6")]
    P6 = 1L << 5,

    /// <summary>
    ///     프리셋 7
    ///     preset 7
    /// </summary>
    [Description("Preset 7")]
    P7 = 1L << 6,

    /// <summary>
    ///     프리셋 8
    ///     preset 8
    /// </summary>
    [Description("Preset 8")]
    P8 = 1L << 7,

    /// <summary>
    ///     프리셋 9
    ///     preset 9
    /// </summary>
    [Description("Preset 9")]
    P9 = 1L << 8,

    /// <summary>
    ///     프리셋 10
    ///     preset 10
    /// </summary>
    [Description("Preset 10")]
    P10 = 1L << 9,

    /// <summary>
    ///     프리셋 11
    ///     preset 11
    /// </summary>
    [Description("Preset 11")]
    P11 = 1L << 10,

    /// <summary>
    ///     프리셋 12
    ///     preset 12
    /// </summary>
    [Description("Preset 12")]
    P12 = 1L << 11,

    /// <summary>
    ///     프리셋 13
    ///     preset 13
    /// </summary>
    [Description("Preset 13")]
    P13 = 1L << 12,

    /// <summary>
    ///     프리셋 14
    ///     preset 14
    /// </summary>
    [Description("Preset 14")]
    P14 = 1L << 13,

    /// <summary>
    ///     프리셋 15
    ///     preset 15
    /// </summary>
    [Description("Preset 15")]
    P15 = 1L << 14,

    /// <summary>
    ///     프리셋 16
    ///     preset 16
    /// </summary>
    [Description("Preset 16")]
    P16 = 1L << 15,

    /// <summary>
    ///     프리셋 17
    ///     preset 17
    /// </summary>
    [Description("Preset 17")]
    P17 = 1L << 16,

    /// <summary>
    ///     프리셋 18
    ///     preset 18
    /// </summary>
    [Description("Preset 18")]
    P18 = 1L << 17,

    /// <summary>
    ///     프리셋 19
    ///     preset 19
    /// </summary>
    [Description("Preset 19")]
    P19 = 1L << 18,

    /// <summary>
    ///     프리셋 20
    ///     preset 20
    /// </summary>
    [Description("Preset 20")]
    P20 = 1L << 19,

    /// <summary>
    ///     프리셋 21
    ///     preset 21
    /// </summary>
    [Description("Preset 21")]
    P21 = 1L << 20,

    /// <summary>
    ///     프리셋 22
    ///     preset 22
    /// </summary>
    [Description("Preset 22")]
    P22 = 1L << 21,

    /// <summary>
    ///     프리셋 23
    ///     preset 23
    /// </summary>
    [Description("Preset 23")]
    P23 = 1L << 22,

    /// <summary>
    ///     프리셋 24
    ///     preset 24
    /// </summary>
    [Description("Preset 24")]
    P24 = 1L << 23,

    /// <summary>
    ///     프리셋 25
    ///     preset 25
    /// </summary>
    [Description("Preset 25")]
    P25 = 1L << 24,

    /// <summary>
    ///     프리셋 26
    ///     preset 26
    /// </summary>
    [Description("Preset 26")]
    P26 = 1L << 25,

    /// <summary>
    ///     프리셋 27
    ///     preset 27
    /// </summary>
    [Description("Preset 27")]
    P27 = 1L << 26,

    /// <summary>
    ///     프리셋 28
    ///     preset 28
    /// </summary>
    [Description("Preset 28")]
    P28 = 1L << 27,

    /// <summary>
    ///     프리셋 29
    ///     preset 29
    /// </summary>
    [Description("Preset 29")]
    P29 = 1L << 28,

    /// <summary>
    ///     프리셋 30
    ///     preset 30
    /// </summary>
    [Description("Preset 30")]
    P30 = 1L << 29,

    /// <summary>
    ///     프리셋 31
    ///     preset 31
    /// </summary>
    [Description("Preset 31")]
    P31 = 1L << 30,

    /// <summary>
    ///     다축(MA)
    ///     multi-axis (MA)
    /// </summary>
    [Description("MA")]
    Ma = 1L << 31,

    /// <summary>
    ///     전체 프리셋 (P1~P31)
    ///     all presets (P1 through P31)
    /// </summary>
    [Description("All presets")]
    AllPresets = P1  | P2  | P3  | P4  | P5  | P6  | P7  | P8  |
                 P9  | P10 | P11 | P12 | P13 | P14 | P15 | P16 |
                 P17 | P18 | P19 | P20 | P21 | P22 | P23 | P24 |
                 P25 | P26 | P27 | P28 | P29 | P30 | P31,

    /// <summary>
    ///     전체 (P1~P31 + Ma)
    ///     all (P1 through P31 plus Ma)
    /// </summary>
    [Description("All")]
    All = AllPresets | Ma
}