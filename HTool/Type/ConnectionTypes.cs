using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     장치 연결 상태 열거형. Close (닫힘) → Connecting (연결 중) → Connected (연결됨) 순서로 전환됩니다.
///     Device connection state enum. Transitions: Close (closed) → Connecting (connecting) → Connected (connected).
/// </summary>
public enum ConnectionTypes {
    [Description("닫힘 / Closed")]
    Close,
    [Description("연결 중 / Connecting")]
    Connecting,
    [Description("연결됨 / Connected")]
    Connected
}