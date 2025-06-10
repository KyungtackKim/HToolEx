using HToolEz.Defines.Entities;
using HToolEz.Defines.Enums;

namespace HToolEz.Device;

/// <summary>
///     Device(EZtorQ-III) control management class
/// </summary>
public sealed class DeviceV1Controller : IDeviceController {
    private static readonly byte[] Header = [0x5A, 0xA5];

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Build(DeviceCommandTypes command, object? payload = null) {
        return command switch {
            DeviceCommandTypes.ReqCalData or DeviceCommandTypes.ReqCalTerminate or DeviceCommandTypes.ReqSetData => BuildSimple(command),
            DeviceCommandTypes.ReqCalSetPoint when payload is (CalPointModeTypes mode, CalPointTypes point) => BuildCalSet(mode, point),
            DeviceCommandTypes.ReqCalSave when payload is CalibrationData cal => BuildCalSave(cal),
            _ => throw new NotSupportedException($"{command} 또는 {payload} 은 지원되지 않습니다.")
        };
    }

    private static ReadOnlyMemory<byte> BuildSimple(DeviceCommandTypes command) {
        // create buffer
        Span<byte> buffer = stackalloc byte[5];
        // set the data
        buffer[0] = Header[0];
        buffer[1] = Header[1];
        buffer[2] = 0x01;
        buffer[3] = 0x00;
        buffer[4] = (byte)command;
        // return the packet
        return buffer.ToArray();
    }

    private static ReadOnlyMemory<byte> BuildCalSet(CalPointModeTypes mode, CalPointTypes point) {
        // create buffer
        Span<byte> buffer = stackalloc byte[7];
        // set the data
        buffer[0] = Header[0];
        buffer[1] = Header[1];
        buffer[2] = 0x03;
        buffer[3] = 0x00;
        buffer[4] = (byte)DeviceCommandTypes.ReqCalSetPoint;
        // write to data
        buffer[5] = (byte)mode;
        buffer[6] = (byte)point;
        // return the packet
        return buffer.ToArray();
    }

    private static ReadOnlyMemory<byte> BuildCalSave(CalibrationData data) {
        // create buffer
        Span<byte> buffer = stackalloc byte[CalibrationData.Size.Max()];
        // get the length
        var length = CalibrationData.Size[(int)data.Body] - 4;
        // set the data
        buffer[0] = Header[0];
        buffer[1] = Header[1];
        buffer[2] = (byte)((length >> 0) & 0xFF);
        buffer[3] = (byte)((length >> 8) & 0xFF);
        buffer[4] = (byte)DeviceCommandTypes.ReqCalSave;
        // write to data
        data.WriteTo(buffer);
        // return the packet
        return buffer[..CalibrationData.Size[(int)data.Body]].ToArray();
    }
}