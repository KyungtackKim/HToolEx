using HToolEz.Defines.Entities;
using HToolEz.Utils;

namespace HToolEz.Device;

/// <summary>
///     Device service class
/// </summary>
public sealed class DeviceService : IAsyncDisposable {
    private readonly IDeviceConnection _comm;
    private readonly IDeviceController _con;
    private bool _isDisposed;

    /// <summary>
    ///     Constructor
    /// </summary>
    public DeviceService(IDeviceConnection comm, IDeviceController con) {
        // inject
        _comm = comm;
        _con = con;
        // reset disposed
        _isDisposed = false;
    }

    private MessageQueue<MessageRequest> MessageQueue { get; } = new();

    /// <inheritdoc />
    public ValueTask DisposeAsync() {
        // check disposed
        if (_isDisposed)
            return ValueTask.CompletedTask;
        // set disposed
        _isDisposed = true;

        // clear message
        MessageQueue.Clear();
        // dispose
        MessageQueue.Dispose();
        // success
        return ValueTask.CompletedTask;
    }
}