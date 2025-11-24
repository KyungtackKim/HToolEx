using System.Runtime.CompilerServices;
using HToolEz.Format;
using HToolEz.Type;
using HToolEz.Util;

namespace HToolEz.Device;

/// <summary>
///     EZtorQ-III device protocol helper (v0.10 20250715)
/// </summary>
public static class DeviceHelper {
    /// <summary>
    ///     Header size
    /// </summary>
    public const int HeaderSize = 4;

    /// <summary>
    ///     Header STX
    /// </summary>
    public static readonly byte[] HeaderStx = [0x5A, 0xA5];

    /// <summary>
    ///     Create request calibration data packet
    /// </summary>
    /// <returns>packet</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateReqCalPacket() {
        // return the packet
        return [HeaderStx[0], HeaderStx[1], 0x00, 0x01, (byte)DeviceCommandTypes.ReqCalData];
    }

    /// <summary>
    ///     Create request calibration point set data packet
    /// </summary>
    /// <param name="point">point</param>
    /// <param name="index">index</param>
    /// <returns>packet</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateReqCalSetPointPacket(CalPointModeTypes point, int index) {
        // check the point
        if (!Utils.IsKnownItem(point))
            throw new ArgumentException($"Unknown calibration point type: {point}", nameof(point));

        // check the index
        if (index is < 0x00 or > 0x0A)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be in range 0x00 to 0x0A");
#if DEBUG
        // debug log
        Console.WriteLine($"Calibration set the point: {point} and index: {index}");
#endif
        // return the packet
        return [HeaderStx[0], HeaderStx[1], 0x00, 0x03, (byte)DeviceCommandTypes.ReqCalSetPoint, (byte)point, (byte)index];
    }

    /// <summary>
    ///     Create terminate packet
    /// </summary>
    /// <returns>packet</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateTerminatePacket() {
        // return the packet
        return [HeaderStx[0], HeaderStx[1], 0x00, 0x01, (byte)DeviceCommandTypes.ReqCalTerminate];
    }

    /// <summary>
    ///     Create request setting data packet
    /// </summary>
    /// <returns>packet</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateReqSetPacket() {
        // return the packet
        return [HeaderStx[0], HeaderStx[1], 0x00, 0x01, (byte)DeviceCommandTypes.ReqSetData];
    }

    /// <summary>
    ///     Create request torque data packet
    /// </summary>
    /// <returns>packet</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateReqTorquePacket() {
        // return the packet
        return [HeaderStx[0], HeaderStx[1], 0x00, 0x01, (byte)DeviceCommandTypes.ReqTorque];
    }

    /// <summary>
    ///     Create calibration data save packet from FormatCalData
    /// </summary>
    /// <param name="calData">calibration data object</param>
    /// <returns>packet</returns>
    /// <exception cref="ArgumentNullException">Thrown when calData is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] CreateReqCalSavePacket(FormatCalData calData) {
        // check null
        if (calData == null)
            throw new ArgumentNullException(nameof(calData), "Calibration data cannot be null");

        // convert calibration data to bytes
        var calBytes = calData.ToBytes();
        // calculate total packet size (header + length + command + data)
        var packetSize = HeaderSize + 1 + calBytes.Length;
        // create packet buffer
        var packet = new byte[packetSize];

        // set header (STX)
        packet[0] = HeaderStx[0];
        packet[1] = HeaderStx[1];

        // set length (command + data size)
        var dataSize = 1 + calBytes.Length;
        packet[2] = (byte)(dataSize >> 8); // high byte
        packet[3] = (byte)dataSize;        // low byte

        // set command
        packet[4] = (byte)DeviceCommandTypes.ReqCalSave;

        // copy calibration data
        Array.Copy(calBytes, 0, packet, 5, calBytes.Length);

        return packet;
    }
}