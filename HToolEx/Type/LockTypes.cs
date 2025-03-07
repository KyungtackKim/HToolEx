using System.ComponentModel;

namespace HToolEx.Type;

/// <summary>
///     Tool lock types
/// </summary>
public enum LockTypes {
    [Description("UnLock")] UnLock,
    [Description("Lock")] Lock,
    [Description("Loosen lock")] LoosenOnly,
    [Description("Fasten lock")] FastenOnly
}