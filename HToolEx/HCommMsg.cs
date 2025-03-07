using HToolEx.Type;

namespace HToolEx;

/// <summary>
///     Hantas communication message class
/// </summary>
public class HCommMsg {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="codeTypes">function code</param>
    /// <param name="addr">address</param>
    /// <param name="packet">packet data</param>
    /// <param name="retry">retry count</param>
    public HCommMsg(CodeTypes codeTypes, int addr, IReadOnlyCollection<byte> packet, int retry = 1) {
        // set information
        CodeTypes = codeTypes;
        Address = addr;
        RetryCount = retry;
        CheckSum = GetCheckSum(packet);
        Packet = new List<byte>(packet);
    }

    /// <summary>
    ///     Function code
    /// </summary>
    public CodeTypes CodeTypes { get; private set; }

    /// <summary>
    ///     Request address
    /// </summary>
    public int Address { get; }

    /// <summary>
    ///     Is activation state
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Send time
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     Send retry count
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    ///     Check sum
    /// </summary>
    public int CheckSum { get; }

    /// <summary>
    ///     Send packet data
    /// </summary>
    public List<byte> Packet { get; set; }

    private static int GetCheckSum(IEnumerable<byte> data) {
        return data.Sum(x => x);
    }
}

/// <summary>
///     HComm message object comparer class
/// </summary>
public class HCommMsgComparer : IEqualityComparer<HCommMsg> {
    /// <summary>
    ///     Check equals
    /// </summary>
    /// <param name="x">source</param>
    /// <param name="y">destination</param>
    /// <returns>result</returns>
    public bool Equals(HCommMsg? x, HCommMsg? y) {
        // check message
        if (x == null || y == null)
            return false;

        // compare
        return x.Address == y.Address && x.CheckSum == y.CheckSum;
    }

    /// <summary>
    ///     Get hash code
    /// </summary>
    /// <param name="obj">object</param>
    /// <returns>result</returns>
    public int GetHashCode(HCommMsg obj) {
        return obj.CheckSum;
    }
}