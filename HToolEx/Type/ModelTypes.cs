using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Hantas tool model types
/// </summary>
public enum ModelTypes {
    [Description("md")] Md = 1,
    [Description("mdt")] Mdt = 15,
    [Description("bm")] Bm = 10,
    [Description("bmt")] Bmt = 19,
    [Description("bpt")] Bpt = 20,
    [Description("mdt40")] Mdt40 = 27,
    [Description("bmt40")] Bmt40 = 29,
    [Description("ept")] Ept = 99
}

/// <summary>
///     Mountz tool model types
/// </summary>
public enum ModelTypesMountz {
    [Description("ec")] Ec = 1,
    [Description("ect")] Ect = 15,
    [Description("ep")] Ep = 10,
    [Description("ept")] Ept = 19,
    [Description("bpt")] Bpt = 20,
    [Description("ect40")] Ect40 = 27,
    [Description("ept40")] Ept40 = 29
}