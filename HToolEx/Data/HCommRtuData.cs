using HToolEx.Type;
using JetBrains.Annotations;

namespace HToolEx.Data;

/// <summary>
///     Hantas communication received rtu data class
/// </summary>
public class HCommRtuData : IHCommData {
    /// <summary>
    ///     Constructor
    /// </summary>
    public HCommRtuData() { }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">total frame values</param>
    public HCommRtuData(IReadOnlyList<byte> values) {
        // create message
        Create(values);
    }

    /// <summary>
    ///     Device ID
    /// </summary>
    [PublicAPI]
    public byte DeviceId { get; private set; }

    /// <summary>
    ///     CRC values
    /// </summary>
    [PublicAPI]
    public int CyclicCheck { get; private set; }

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
        DeviceId = values[pos++];
        CodeTypes = (CodeTypes)values[pos++];

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
                Values = Array.Empty<byte>();

                break;
        }

        // check length
        for (var i = 0; i < Length; i++)
            // set byte values
            Values[i] = values[pos + i];

        // set crc values
        CyclicCheck = (values[^1] << 8) | values[^2];
    }
}