using System.ComponentModel.DataAnnotations;
using HToolEx.Localization;

namespace HToolEx.Type;

/// <summary>
///     Tool lock types
/// </summary>
public enum LockTypes {
    [Display(Description = @"LockTypeUnLock", ResourceType = typeof(HToolExRes))]
    UnLock,
    [Display(Description = @"LockTypeLock", ResourceType = typeof(HToolExRes))]
    Lock,
    [Display(Description = @"LockTypeLoosenLock", ResourceType = typeof(HToolExRes))]
    LoosenOnly,
    [Display(Description = @"LockTypeFastenLock", ResourceType = typeof(HToolExRes))]
    FastenOnly
}