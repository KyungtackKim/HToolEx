using HTool.Type;

namespace HTool.Data;

/// <summary>
///     Receive data format for MODBUS-RTU class
/// </summary>
public sealed class HcRtuData : IReceivedData {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">data</param>
    public HcRtuData(IReadOnlyList<byte> values) {
        // create the received data
        Create(values);
    }

    /// <summary>
    ///     Tool device id
    /// </summary>
    public byte DeviceId { get; private set; }

    /// <summary>
    ///     CRC value
    /// </summary>
    public int CyclicCheck { get; private set; }

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
    public byte[] Data { get; private set; } = null!;

    /// <summary>
    ///     Create for the data format
    /// </summary>
    /// <param name="values"></param>
    public void Create(IReadOnlyList<byte> values) {
        var pos = 0;
        // set header
        DeviceId = values[pos++];
        Code     = (CodeTypes)values[pos++];
        // check code
        if ((byte)((int)Code & (int)CodeTypes.Error) == (byte)CodeTypes.Error)
            // change code
            Code = CodeTypes.Error;
        // check function code
        switch (Code) {
            case CodeTypes.ReadHoldingReg:
            case CodeTypes.ReadInputReg:
            case CodeTypes.ReadInfoReg:
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
            case CodeTypes.Error:
                // get length
                Length = 1;
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

        // set crc values
        CyclicCheck = (values[^1] << 8) | values[^2];
    }
}