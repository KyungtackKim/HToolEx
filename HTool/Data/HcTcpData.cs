using HTool.Type;
using JetBrains.Annotations;

namespace HTool.Data;

/// <summary>
///     Receive data format for MODBUS-TCP class
/// </summary>
[PublicAPI]
public class HcTcpData : IReceivedData {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">data</param>
    public HcTcpData(IReadOnlyList<byte> values) {
        // create the received data
        Create(values);
    }

    /// <summary>
    ///     MODBUS-TCP transaction id
    /// </summary>
    public int TransactionId { get; private set; }

    /// <summary>
    ///     MODBUS-TCP protocol id
    /// </summary>
    public int ProtocolId { get; private set; }

    /// <summary>
    ///     MODBUS-TCP uni id
    /// </summary>
    public int UnitId { get; private set; }

    /// <summary>
    ///     MODBUS-TCP frame length
    /// </summary>
    public int FrameLength { get; private set; }

    /// <summary>
    ///     MODBUS function code
    /// </summary>
    public CodeTypes Code { get; private set; }

    /// <summary>
    ///     Data length
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    ///     Data
    /// </summary>
    public byte[] Data { get; private set; } = default!;

    /// <summary>
    ///     Create for the data format
    /// </summary>
    /// <param name="values"></param>
    public void Create(IReadOnlyList<byte> values) {
        var pos = 0;
        // set header
        TransactionId = (values[pos++] << 8) | values[pos++];
        ProtocolId = (values[pos++] << 8) | values[pos++];
        FrameLength = (values[pos++] << 8) | values[pos++];
        UnitId = values[pos++];
        Code = (CodeTypes)values[pos++];

        // check function code
        switch (Code) {
            case CodeTypes.ReadHoldingReg:
            case CodeTypes.ReadInputReg:
            case CodeTypes.ReadInfoReg:
            case CodeTypes.Error:
                // get length
                Length = values[pos++];
                // create values
                Data = new byte[Length];

                break;
            case CodeTypes.WriteSingleReg:
            case CodeTypes.WriteMultiReg:
                // get length
                Length = 4;
                // create values
                Data = new byte[Length];

                break;
            case CodeTypes.Graph:
            case CodeTypes.GraphRes:
            case CodeTypes.HighResGraph:
                // get length
                Length = (values[pos++] << 8) | values[pos++];
                // create values
                Data = new byte[Length];

                break;
            default:
                // get length
                Length = 0;
                // create values
                Data = [];

                break;
        }

        // check length
        for (var i = 0; i < Length; i++)
            // set byte values
            Data[i] = values[pos + i];
    }
}