using HToolEz.Type;

namespace HToolEz.Device;

/// <summary>
///     Device service interface
/// </summary>
public interface IDeviceService : IDisposable {
    /// <summary>
    ///     Perform for the data delegate
    /// </summary>
    delegate void PerformData(DeviceCommandTypes cmd, ReadOnlyMemory<byte> data);

    /// <summary>
    ///     Perform for the torque delegate
    /// </summary>
    delegate void PerformTorque(float torque, UnitTypes unit);

    /// <summary>
    ///     Connected state
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    ///     Device operation mode
    /// </summary>
    DeviceModeTypes Mode { get; set; }

    /// <summary>
    ///     Received torque data
    /// </summary>
    event PerformTorque? ReceivedTorque;

    /// <summary>
    ///     Received data event
    /// </summary>
    event PerformData? ReceivedData;

    /// <summary>
    ///     Try to connect
    /// </summary>
    /// <param name="target">target port</param>
    /// <returns>result</returns>
    bool Connect(string target);

    /// <summary>
    ///     Disconnect
    /// </summary>
    void Disconnect();

    /// <summary>
    ///     Write to the device
    /// </summary>
    /// <param name="packet">packet</param>
    /// <returns>result</returns>
    bool Write(ReadOnlyMemory<byte> packet);
}