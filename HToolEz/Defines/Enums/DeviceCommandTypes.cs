using System.ComponentModel;

namespace HToolEz.Defines.Enums;

/// <summary>
///     Device command types
/// </summary>
public enum DeviceCommandTypes {
    [Description("Request calibration data")]
    ReqCalData = 0x00,
    [Description("Request calibration set point")]
    ReqCalSetPoint = 0x01,
    [Description("Request calibration save")]
    ReqCalSave = 0x02,
    [Description("Request calibration terminate")]
    ReqCalTerminate = 0x03,
    [Description("Request setting data")]
    ReqSetData = 0x04,
    [Description("Response calibration data")]
    ResCalData = 0x80,
    [Description("Response calibration set point")]
    ResCalSetPoint = 0x81,
    [Description("Response calibration save")]
    ResCalSave = 0x82,
    [Description("Response setting data")]
    ResSetData = 0x84,
    [Description("Report current AD data")]
    RepCurrentAdc = 0xA0
}