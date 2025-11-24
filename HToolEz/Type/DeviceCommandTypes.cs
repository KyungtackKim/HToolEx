using System.ComponentModel;

namespace HToolEz.Type;

/// <summary>
///     Device command types
/// </summary>
public enum DeviceCommandTypes : byte {
    /// <summary>
    ///     Request calibration data
    /// </summary>
    [Description("Request the calibration data")]
    ReqCalData = 0x00,

    /// <summary>
    ///     Request calibration set point
    /// </summary>
    [Description("Request the calibration set point")]
    ReqCalSetPoint = 0x01,

    /// <summary>
    ///     Request calibration save
    /// </summary>
    [Description("Request the calibration save")]
    ReqCalSave = 0x02,

    /// <summary>
    ///     Request calibration terminate
    /// </summary>
    [Description("Request the calibration terminate")]
    ReqCalTerminate = 0x03,

    /// <summary>
    ///     Request setting data
    /// </summary>
    [Description("Request the setting data")]
    ReqSetData = 0x04,

    /// <summary>
    ///     Request current torque
    /// </summary>
    [Description("Request the current torque without the unit")]
    ReqTorque = 0x05,

    /// <summary>
    ///     Response calibration data
    /// </summary>
    [Description("Response the calibration data")]
    ResCalData = 0x80,

    /// <summary>
    ///     Response calibration set point
    /// </summary>
    [Description("Response the calibration set point")]
    ResCalSetPoint = 0x81,

    /// <summary>
    ///     Response calibration save
    /// </summary>
    [Description("Response the calibration save")]
    ResCalSave = 0x82,

    /// <summary>
    ///     Response setting data
    /// </summary>
    [Description("Response the setting data")]
    ResSetData = 0x84,

    /// <summary>
    ///     Response current torque
    /// </summary>
    [Description("Response the current torque without the unit")]
    ResTorque = 0x85,

    /// <summary>
    ///     Report ADC data
    /// </summary>
    [Description("Report the ADC data")]
    RepAdc = 0xA0
}