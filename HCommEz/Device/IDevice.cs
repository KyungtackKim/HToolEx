namespace HCommEz.Device {
    /// <summary>
    ///     EZtorQ interface
    /// </summary>
    public interface IDevice {
        /// <summary>
        ///     Received calibration data event
        /// </summary>
        ReceivedCal OnReceivedCal { get; set; }

        /// <summary>
        ///     Received torque data event
        /// </summary>
        ReceivedTorque OnReceivedTorque { get; set; }

        /// <summary>
        ///     Connection mode
        /// </summary>
        ConnectMode ConnectionMode { get; }

        /// <summary>
        ///     Connecting state
        /// </summary>
        ConnectionState State { get; set; }

        /// <summary>
        ///     Connection state
        /// </summary>
        bool IsConnected { get; set; }

        /// <summary>
        ///     EZTorQ information
        /// </summary>
        Information Info { get; }

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="target">target</param>
        /// <param name="option">option</param>
        /// <returns>result</returns>
        bool Connect(string target, int option = 115200);

        /// <summary>
        ///     Disconnect
        /// </summary>
        /// <returns>result</returns>
        bool Disconnect();

        /// <summary>
        ///     Request calibration data
        /// </summary>
        /// <returns>result</returns>
        bool RequestCal();

        /// <summary>
        ///     Request calibration terminate
        /// </summary>
        /// <returns>result</returns>
        bool RequestCalTerminate();

        /// <summary>
        ///     Request setting
        /// </summary>
        /// <returns>result</returns>
        bool RequestSet();

        /// <summary>
        ///     Save calibration data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>result</returns>
        bool SaveCalPoint(Information.CalInfo data);

        /// <summary>
        ///     Set calibration position
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="pos">position</param>
        /// <returns>result</returns>
        bool SetCalPoint(int type, WorkPosition pos);
    }

    /// <summary>
    ///     Received calibration data delegate
    /// </summary>
    public delegate void ReceivedCal(WorkCommand cmd, byte[] data);

    /// <summary>
    ///     Received torque data delegate
    /// </summary>
    public delegate void ReceivedTorque(double torque, string unit);
}