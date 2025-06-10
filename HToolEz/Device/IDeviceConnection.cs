using HToolEz.Defines.Enums;

namespace HToolEz.Device;

/// <summary>
///     Device connection management interface
/// </summary>
public interface IDeviceConnection : IAsyncDisposable {
    /// <summary>
    ///     Connection state
    /// </summary>
    DeviceConnectionTypes ConnectionState { get; }

    /// <summary>
    ///     ConnectAsync to the device
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="option">option</param>
    /// <param name="token">cancellation token</param>
    /// <returns>result</returns>
    Task<bool> ConnectAsync(string target, object? option = null, CancellationToken token = default);

    /// <summary>
    ///     DisconnectAsync from the device
    /// </summary>
    /// <returns>result</returns>
    Task DisconnectAsync();

    /// <summary>
    ///     SendAsync to the device
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="token">cancellation token</param>
    /// <returns>result</returns>
    ValueTask<bool> SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default);

    /// <summary>
    ///     ReceiveAsync from the device
    /// </summary>
    /// <param name="token">cancellation token</param>
    /// <returns>result</returns>
    IAsyncEnumerable<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token = default);
}