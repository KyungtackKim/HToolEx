using System.ComponentModel;

namespace HTool.Type;

/// <summary>
///     Connection types
/// </summary>
public enum ConnectionTypes {
    [Description("Closed")]
    Close,
    [Description("Connecting")]
    Connecting,
    [Description("Connected")]
    Connected
}