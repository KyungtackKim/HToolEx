﻿using System.ComponentModel;
using HTool.Type;
using HTool.Util;
using JetBrains.Annotations;

namespace HTool.Format;

/// <summary>
///     Hantas tool message format class
/// </summary>
[PublicAPI]
public class FormatMessage {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="code">code</param>
    /// <param name="addr">address</param>
    /// <param name="packet">packet</param>
    /// <param name="retry">retry</param>
    public FormatMessage(CodeTypes code, int addr, IReadOnlyCollection<byte> packet, int retry = 1) {
        // set message values
        Code = code;
        Address = addr;
        Retry = retry;
        CheckSum = Utils.CalculateCheckSum(packet);
        Packet = [..packet];
    }

    /// <summary>
    ///     Default address
    /// </summary>
    public static int EmptyAddr { get; }

    /// <summary>
    ///     Function code
    /// </summary>
    public CodeTypes Code { get; private set; }

    /// <summary>
    ///     Address
    /// </summary>
    public int Address { get; private set; }

    /// <summary>
    ///     Retry count
    /// </summary>
    public int Retry { get; private set; }

    /// <summary>
    ///     Activation state
    /// </summary>
    public bool Activated { get; private set; }

    /// <summary>
    ///     Activation time
    /// </summary>
    public DateTime ActiveTime { get; private set; }

    /// <summary>
    ///     Message check sum
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; private set; }

    /// <summary>
    ///     Packet data
    /// </summary>
    [Browsable(false)]
    public List<byte> Packet { get; private set; }

    /// <summary>
    ///     Activate message
    /// </summary>
    public void Activate() {
        // set activated state
        Activated = true;
        // set activated time
        ActiveTime = DateTime.Now;
        // sub retry count
        Retry--;
    }

    /// <summary>
    ///     De-active message
    /// </summary>
    /// <returns>retry count</returns>
    public int DeActive() {
        // reset activated state
        Activated = false;
        // return retry count
        return Retry;
    }
}

/// <summary>
///     hantas tool message comparer class
/// </summary>
[PublicAPI]
public class MessageComparer : IEqualityComparer<FormatMessage> {
    /// <summary>
    ///     Check equals
    /// </summary>
    /// <param name="x">source</param>
    /// <param name="y">destination</param>
    /// <returns>result</returns>
    public bool Equals(FormatMessage? x, FormatMessage? y) {
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
    public int GetHashCode(FormatMessage obj) {
        return obj.CheckSum;
    }
}