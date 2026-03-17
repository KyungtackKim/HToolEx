using HToolEx.Type;
using JetBrains.Annotations;

namespace HToolEx.Data;

/// <summary>
///     Hantas communication received tcp data class
/// </summary>
public class HCommTcpData : IHCommData {
    /// <summary>
    ///     Constructor
    /// </summary>
    public HCommTcpData() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values"></param>
    public HCommTcpData(IReadOnlyList<byte> values) {
        // create message
        Create(values);
    }

    /// <summary>
    ///     Transaction id
    /// </summary>
    [PublicAPI]
    public int TransactionId { get; private set; }

    /// <summary>
    ///     Protocol id
    /// </summary>
    [PublicAPI]
    public int ProtocolId { get; private set; }

    /// <summary>
    ///     Frame length
    /// </summary>
    [PublicAPI]
    public int FrameLength { get; private set; }

    /// <summary>
    ///     Unit id
    /// </summary>
    [PublicAPI]
    public byte UnitId { get; private set; }

    /// <summary>
    ///     Function code
    /// </summary>
    [PublicAPI]
    public CodeTypes CodeTypes { get; private set; }

    /// <summary>
    ///     Data length
    /// </summary>
    [PublicAPI]
    public int Length { get; private set; }

    /// <summary>
    ///     Byte values
    /// </summary>
    [PublicAPI]
    public byte[] Values { get; private set; } = default!;

    /// <summary>
    ///     Create message
    /// </summary>
    /// <param name="values">values</param>
    [PublicAPI]
    public void Create(IReadOnlyList<byte> values) {
        var pos = 0;
        // set header
        TransactionId = (values[pos++] << 8) | values[pos++];
        ProtocolId    = (values[pos++] << 8) | values[pos++];
        FrameLength   = (values[pos++] << 8) | values[pos++];
        UnitId        = values[pos++];
        CodeTypes     = (CodeTypes)values[pos++];

        // check function code
        switch (CodeTypes) {
            case CodeTypes.ReadHoldingReg:
            case CodeTypes.ReadInputReg:
            case CodeTypes.Error:
                // get length
                Length = values[pos++];
                // create values
                Values = new byte[Length];

                break;
            case CodeTypes.WriteSingleReg:
            case CodeTypes.WriteMultiReg:
                // get length
                Length = 4;
                // create values
                Values = new byte[Length];

                break;
            case CodeTypes.Graph:
            case CodeTypes.GraphRes:
                // get length
                Length = (values[pos++] << 8) | values[pos++];
                // create values
                Values = new byte[Length];

                break;
            default:
                // get length
                Length = 0;
                // create values
                Values = [];

                break;
        }

        // check length
        for (var i = 0; i < Length; i++)
            // set byte values
            Values[i] = values[pos + i];
    }
}