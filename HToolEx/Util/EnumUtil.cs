using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HToolEx.Util;

/// <summary>
///     Enum utilities class
/// </summary>
public static class EnumUtil {
    /// <summary>
    ///     Get description or name from enum value
    /// </summary>
    /// <param name="value">value</param>
    /// <returns>description</returns>
    public static string GetDesc(this Enum value) {
        // get value string
        var str = $"{value}";
        // get value field
        var field = value.GetType().GetField(str);
        // check field
        if (field == null)
            return str;
        // get attributes
        if (field.GetCustomAttributes(typeof(DescriptionAttribute), false) is not DescriptionAttribute[] desc)
            return str;
        // get description
        return desc.Length == 0 ? str : desc[0].Description;
    }

    /// <summary>
    ///     Get display name
    /// </summary>
    /// <param name="value">enum value</param>
    /// <returns>display</returns>
    public static string GetDisplay(this Enum value) {
        // get type
        var type = value.GetType();
        // check member
        var member = type.GetMember($"{value}");
        // check member
        if (member.Length == 0)
            // return
            return string.Empty;
        // get custom attribute
        var attr = member[0].GetCustomAttribute<DisplayAttribute>();
        // check attribute
        if (attr == null)
            // return
            return string.Empty;
        // get description
        return attr.GetDescription() ?? string.Empty;
    }

    /// <summary>
    ///     Get value from enum description or name
    /// </summary>
    /// <param name="desc">description</param>
    /// <typeparam name="T">enum type</typeparam>
    /// <returns>enum value</returns>
    public static (bool res, T type) GetValue<T>(string desc) where T : struct, Enum {
        // Attempt to directly parse the string to the Enum type.
        if (Enum.TryParse<T>(desc, true, out var result))
            return (true, result);
        // Fall back to checking if the string matches the description of each Enum member.
        var names = Enum.GetNames(typeof(T));
        // Check names
        foreach (var name in names) {
            // Parse type
            var e = (Enum)Enum.Parse(typeof(T), name);
            // Check description
            if (desc == GetDesc(e))
                // Return type
                return (true, (T)e);
        }

        // Return default type
        return (false, (T)Enum.Parse(typeof(T), names[0]));
    }

    /// <summary>
    ///     Get int value
    /// </summary>
    /// <param name="value">enum</param>
    /// <returns>value</returns>
    public static int GetIntValue(this Enum value) {
        // check defined enum
        if (!Enum.IsDefined(value.GetType(), value))
            // not defined
            throw new ArgumentException($"{value} is not a valid enum value!");
        // get int value
        return (int)(object)value;
    }
}