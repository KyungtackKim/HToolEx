using JetBrains.Annotations;

namespace HTool.Util;

/// <summary>
///     Constants class
/// </summary>
[PublicAPI]
public static class Constants {
    /// <summary>
    ///     Baud rate for Serial communication
    /// </summary>
    public static readonly int[] BaudRates = [9600, 19200, 38400, 57600, 115200, 230400];

    /// <summary>
    ///     Barcode length
    /// </summary>
    public static int BarcodeLength = 64;

    /// <summary>
    ///     Process period
    /// </summary>
    public static int ProcessPeriod => 50;

    /// <summary>
    ///     Keep alive request period
    /// </summary>
    public static int KeepAlivePeriod => 3000;

    /// <summary>
    ///     Process lock time out
    /// </summary>
    public static int ProcessLockTime => 200;

    /// <summary>
    ///     Process timeout
    /// </summary>
    public static int ProcessTimeout => 500;

    /// <summary>
    ///     Connect timeout
    /// </summary>
    public static int ConnectTimeout => 5000;

    /// <summary>
    ///     Message timeout
    /// </summary>
    public static int MessageTimeout => 1000;

    /// <summary>
    ///     Keep alive timeout
    /// </summary>
    public static int KeepAliveTimeout => 10;
}