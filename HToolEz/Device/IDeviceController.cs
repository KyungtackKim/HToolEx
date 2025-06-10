using HToolEz.Defines.Enums;

namespace HToolEz.Device;

/// <summary>
///     Device controller management interface
/// </summary>
public interface IDeviceController {
    /// <summary>
    ///     Build the packet
    /// </summary>
    /// <param name="command">command</param>
    /// <param name="payload">payload</param>
    /// <returns>packet</returns>
    ReadOnlyMemory<byte> Build(DeviceCommandTypes command, object? payload = null);
}