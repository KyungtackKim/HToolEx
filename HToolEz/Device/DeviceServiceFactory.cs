namespace HToolEz.Device;

/// <summary>
///     Device service factory design pattern class
/// </summary>
public static class DeviceServiceFactory {
    /// <summary>
    ///     Device connection type enumeration
    /// </summary>
    public enum ConnectionTypes {
        /// <summary>
        ///     Serial port connection
        /// </summary>
        Serial

        // Future expansion examples:
        // Tcp,  // TCP/IP connection
        // Mock  // Mock for testing
    }

    /// <summary>
    ///     Create device service object
    /// </summary>
    /// <param name="type">connection type</param>
    /// <returns>device service instance (as interface for polymorphism)</returns>
    /// <exception cref="NotSupportedException">unsupported connection type</exception>
    public static IDeviceService Create(ConnectionTypes type) {
        return type switch {
            ConnectionTypes.Serial => new DeviceService(),
            // Future expansion examples:
            // ConnectionTypes.Tcp  => new DeviceServiceTcp(),
            // ConnectionTypes.Mock => new DeviceServiceMock(),
            _ => throw new NotSupportedException($"Connection type '{type}' is not supported")
        };
    }
}