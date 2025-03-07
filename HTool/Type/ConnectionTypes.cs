using System.ComponentModel;
using JetBrains.Annotations;

namespace HTool.Type;

/// <summary>
///     Connection types
/// </summary>
[PublicAPI]
public enum ConnectionTypes {
    [Description("Closed")] Close,
    [Description("Connecting")] Connecting,
    [Description("Connected")] Connected
}