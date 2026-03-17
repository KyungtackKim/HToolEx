using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Model command types
/// </summary>
public enum ModelCmdTypes {
    [Display(Description = @"ModelCmdNone", ResourceType = typeof(HToolExRes))]
    None,
    [Display(Description = @"ModelCmdFastening", ResourceType = typeof(HToolExRes))]
    Fastening,
    [Display(Description = @"ModelCmdDelay", ResourceType = typeof(HToolExRes))]
    Delay,
    [Display(Description = @"ModelCmdInput", ResourceType = typeof(HToolExRes))]
    Input,
    [Display(Description = @"ModelCmdOutput", ResourceType = typeof(HToolExRes))]
    Output,
    [Display(Description = @"ModelCmdBarcode", ResourceType = typeof(HToolExRes))]
    Barcode
}