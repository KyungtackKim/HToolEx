using HToolEx.Type;

namespace HToolEx.ProEx.Util;

/// <summary>
///     Create packet for ParaMon-Pro X
/// </summary>
public static class ProPacket {
    /// <summary>
    ///     Get holding register packet from Pro X
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public static byte[] GetReadHoldingRegPacketFromPro(ushort addr, ushort count, byte id) {
        // create packet
        var packet = new List<byte> {
            /*TID   */0x00, 0x00,
            /*PID   */0x00, 0x00,
            /*LENGTH*/0x00, 0x06,
            /*UID   */id,
            /*FC    */(byte)CodeTypes.ReadHoldingReg,
            /*ADDR  */(byte)((addr >> 8) & 0xFF), (byte)(addr & 0xFF),
            /*COUNT */(byte)((count >> 8) & 0xFF), (byte)(count & 0xFF)
        };
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Get input register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public static byte[] GetReadInputRegPacketFromPro(ushort addr, ushort count, byte id = 1) {
        // create packet
        var packet = new List<byte> {
            /*TID   */0x00, 0x00,
            /*PID   */0x00, 0x00,
            /*LENGTH*/0x00, 0x06,
            /*UID   */id,
            /*FC    */(byte)CodeTypes.ReadInputReg,
            /*ADDR  */(byte)((addr >> 8) & 0xFF), (byte)(addr & 0xFF),
            /*COUNT */(byte)((count >> 8) & 0xFF), (byte)(count & 0xFF)
        };
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set single register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public static byte[] SetSingleRegPacketFromPro(ushort addr, ushort value, byte id = 1) {
        // create packet
        var packet = new List<byte> {
            /*TID   */0x00, 0x00,
            /*PID   */0x00, 0x00,
            /*LENGTH*/0x00, 0x06,
            /*UID   */id,
            /*FC    */(byte)CodeTypes.WriteSingleReg,
            /*ADDR  */(byte)((addr >> 8) & 0xFF), (byte)(addr & 0xFF),
            /*COUNT */(byte)((value >> 8) & 0xFF), (byte)(value & 0xFF)
        };
        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set multiple register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public static byte[] SetMultiRegPacketFromPro(ushort addr, ushort[] values, byte id = 1) {
        // get count
        var count = values.Length;
        // create packet
        var packet = new List<byte> {
            /*TID   */0x00, 0x00,
            /*PID   */0x00, 0x00,
            /*LENGTH*/0x00, 0x00,
            /*UID   */id,
            /*FC    */(byte)CodeTypes.WriteMultiReg,
            /*ADDR  */(byte)((addr >> 8) & 0xFF), (byte)(addr & 0xFF),
            /*COUNT */(byte)((count >> 8) & 0xFF), (byte)(count & 0xFF),
            /*LENGTH*/(byte)(count * 2)
        };
        // check values
        foreach (var value in values) {
            packet.Add((byte)((value >> 8) & 0xFF));
            packet.Add((byte)(value & 0xFF));
        }

        // get total length
        var len = packet.Count - 6;
        // change length
        packet[4] = (byte)((len >> 8) & 0xFF);
        packet[5] = (byte)(len & 0xFF);

        // packet
        return packet.ToArray();
    }

    /// <summary>
    ///     Set multiple register ascii packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string</param>
    /// <param name="length">length</param>
    /// <param name="id">id</param>
    /// <returns>result</returns>
    public static byte[] SetMultiRegStrPacketFromPro(ushort addr, string str, int length, byte id = 1) {
        // check length
        if (length < str.Length)
            // set length
            length = str.Length;
        // get count
        var count = length / 2;
        // create packet
        var packet = new List<byte> {
            /*TID   */0x00, 0x00,
            /*PID   */0x00, 0x00,
            /*LENGTH*/0x00, 0x06,
            /*UID   */id,
            /*FC    */(byte)CodeTypes.WriteMultiReg,
            /*ADDR  */(byte)((addr >> 8) & 0xFF), (byte)(addr & 0xFF),
            /*COUNT */(byte)((count >> 8) & 0xFF), (byte)(count & 0xFF),
            /*LENGTH*/(byte)length
        };
        // add string
        packet.AddRange(str.Select(c => (byte)c));
        // check string length
        if (str.Length < length)
            // add dummy data
            packet.AddRange(new byte[length - str.Length]);

        // packet
        return packet.ToArray();
    }
}