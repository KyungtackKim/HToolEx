using HToolEx.Device;
using HToolEx.Type;

namespace HToolEx;

/// <summary>
///     Communication factory design pattern class
/// </summary>
public class CommunicationFactory {
    /// <summary>
    ///     Create communication object
    /// </summary>
    /// <param name="type">type</param>
    /// <returns>communication</returns>
    public static IHComm? Create(CommTypes type) {
        return type switch {
            CommTypes.None => null,
            CommTypes.Rtu  => new HcRtu(),
            CommTypes.Tcp  => new HcTcp(),
            _              => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}