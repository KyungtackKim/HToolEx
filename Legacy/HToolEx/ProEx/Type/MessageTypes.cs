using System.ComponentModel;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Type;

[PublicAPI]
public enum MessageTypes {
    [Description("Validation")]
    Validation,
    [Description("Delay time (sec)")]
    DelayTime,
    [Description("Next step")]
    NextStep,
    [Description("Minimum display time (sec)")]
    MinDisplayTime
}