using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     공구 잠금 상태 열거형
///     tool lock state enumeration
/// </summary>
/// <remarks>
///     공구 잠금(락/언락) 제어 명령 정의이며, 본 라이브러리 내부에서 현재 참조되지 않습니다.
///     definition of tool lock/unlock control commands; currently unreferenced within this library.
/// </remarks>
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