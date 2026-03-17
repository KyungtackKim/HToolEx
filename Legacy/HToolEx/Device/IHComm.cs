using HToolEx.Type;

namespace HToolEx.Device;

/// <summary>
///     Hantas communication interface
/// </summary>
public interface IHComm {
    /// <summary>
    ///     Connection message delegate
    /// </summary>
    delegate void PerformConnectMsg(bool state);

    /// <summary>
    ///     Received data delegate
    /// </summary>
    delegate void PerformReceiveData(byte[] packet);

    /// <summary>
    ///     Received message delegate
    /// </summary>
    delegate void PerformReceiveMsg(CodeTypes codeTypes, byte[] packet);

    /// <summary>
    ///     Device ID
    /// </summary>
    byte DeviceId { get; set; }

    /// <summary>
    ///     Connection state
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    ///     Connect device
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="option">option</param>
    /// <param name="id">device id</param>
    /// <returns>result</returns>
    bool Connect(string target, int option, byte id = 0x01);

    /// <summary>
    ///     Close device
    /// </summary>
    void Close();

    /// <summary>
    ///     Write packet
    /// </summary>
    /// <param name="packet">packet</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    bool Write(byte[] packet, int length);

    /// <summary>
    ///     Received message event
    /// </summary>
    event PerformReceiveMsg ReceivedMsg;

    /// <summary>
    ///     Received data event
    /// </summary>
    event PerformReceiveData ReceivedData;

    /// <summary>
    ///     Connection message event
    /// </summary>
    event PerformConnectMsg ConnectedMsg;

    /// <summary>
    ///     Get holding register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    byte[] GetReadHoldingRegPacket(ushort addr, ushort count, int id = 1);

    /// <summary>
    ///     Get input register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    byte[] GetReadInputRegPacket(ushort addr, ushort count, int id = 1);

    /// <summary>
    ///     Set single register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public byte[] SetSingleRegPacket(ushort addr, ushort value, int id = 1);

    /// <summary>
    ///     Set multiple register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <param name="id">id</param>
    /// <returns>packet</returns>
    public byte[] SetMultiRegPacket(ushort addr, ushort[] values, int id = 1);

    /// <summary>
    ///     Set multiple register ascii packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string</param>
    /// <param name="length">length</param>
    /// <param name="id">id</param>
    /// <returns>result</returns>
    public byte[] SetMultiRegStrPacket(ushort addr, string str, int length, int id = 1);
}