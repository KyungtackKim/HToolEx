using HTool.Type;
using JetBrains.Annotations;

namespace HTool.Device;

/// <summary>
///     Hantas tool class
/// </summary>
[PublicAPI]
public class Tool {
    /// <summary>
    ///     Create the communication tool
    /// </summary>
    /// <param name="type">communication type</param>
    /// <returns>result</returns>
    public static ITool Create(ComTypes type) {
        return type switch {
            ComTypes.Rtu => new HcRtu(),
            ComTypes.Tcp => new HcTcp(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}