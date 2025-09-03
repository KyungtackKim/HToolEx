using HTool.Type;
using JetBrains.Annotations;

namespace HTool.Device;

/// <summary>
///     Hantas tool interface
/// </summary>
[PublicAPI]
public interface ITool {
    /// <summary>
    ///     Connection changed delegate
    /// </summary>
    delegate void PerformConnect(bool state);

    /// <summary>
    ///     Received raw data delegate
    /// </summary>
    delegate void PerformRawData(byte[] packet);

    /// <summary>
    ///     Received data delegate
    /// </summary>
    delegate void PerformReceiveData(CodeTypes codeTypes, byte[] packet);

    /// <summary>
    ///     Header size
    /// </summary>
    int HeaderSize { get; }

    /// <summary>
    ///     Function code position
    /// </summary>
    int FunctionPos { get; }

    /// <summary>
    ///     Tool device identifier number
    /// </summary>
    byte DeviceId { get; set; }

    /// <summary>
    ///     Tool revision number        ( Gen.1, Gen.2 ... )
    /// </summary>
    GenerationTypes Revision { get; set; }

    /// <summary>
    ///     Connection changed event
    /// </summary>
    event PerformConnect ChangedConnect;

    /// <summary>
    ///     Received data event
    /// </summary>
    event PerformReceiveData ReceivedData;

    /// <summary>
    ///     Received raw data event
    /// </summary>
    event PerformRawData ReceivedRaw;

    /// <summary>
    ///     Transmitted raw data event
    /// </summary>
    event PerformRawData TransmitRaw;

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
    ///     Get holding register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <returns>packet</returns>
    byte[] GetReadHoldingRegPacket(ushort addr, ushort count);

    /// <summary>
    ///     Get input register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="count">count</param>
    /// <returns>packet</returns>
    byte[] GetReadInputRegPacket(ushort addr, ushort count);

    /// <summary>
    ///     Set single register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="value">value</param>
    /// <returns>packet</returns>
    byte[] SetSingleRegPacket(ushort addr, ushort value);

    /// <summary>
    ///     Set multiple register packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="values">values</param>
    /// <returns>packet</returns>
    byte[] SetMultiRegPacket(ushort addr, ushort[] values);

    /// <summary>
    ///     Set multiple register ascii packet
    /// </summary>
    /// <param name="addr">address</param>
    /// <param name="str">string</param>
    /// <param name="length">length</param>
    /// <returns>result</returns>
    byte[] SetMultiRegStrPacket(ushort addr, string str, int length);

    /// <summary>
    ///     Get information register packet
    /// </summary>
    /// <returns>result</returns>
    byte[] GetInfoRegPacket();
}