using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     공구 잠금 상태 열거형
///     tool lock state enumeration
/// </summary>
public enum ToolLock {
    /// <summary>
    ///     잠금 해제
    ///     unlock
    /// </summary>
    [Description("Unlock")]
    UnLock,

    /// <summary>
    ///     잠금
    ///     lock
    /// </summary>
    [Description("Lock")]
    Lock,

    /// <summary>
    ///     풀림만 허용
    ///     loosen only
    /// </summary>
    [Description("Loosen lock")]
    LoosenOnly,

    /// <summary>
    ///     체결만 허용
    ///     fasten only
    /// </summary>
    [Description("Fasten lock")]
    FastenOnly
}