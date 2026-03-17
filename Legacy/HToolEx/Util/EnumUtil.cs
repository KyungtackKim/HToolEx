using System.ComponentModel;

namespace HToolEx.Util;

/// <summary>
///     Enum utilities class
/// </summary>
public static class EnumUtil {
	/// <summary>
	///     Localizer delegate for enum display names.
	///     Must be set by the host application at startup (e.g. Lang.Get).
	///     Key format: "Enum.{TypeName}.{Value}" (e.g. "Enum.GraphTypes.Torque")
	/// </summary>
	public static Func<string, string>? Localizer { get; set; }

	/// <summary>
	///     Get localized display name for an enum value.
	///     Uses Localizer delegate if set, otherwise returns the enum value name.
	/// </summary>
	/// <param name="value">enum value</param>
	/// <returns>localized display name</returns>
	public static string GetDisplay(this Enum value) {
		// build localization key
		var key = $"Enum.{value.GetType().Name}.{value}";
		// return localized string if localizer is available
		return Localizer?.Invoke(key) ?? $"{value}";
	}

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
		if (field is null)
			return str;
		// get attributes
		if (field.GetCustomAttributes(typeof(DescriptionAttribute), false) is not DescriptionAttribute[] desc)
			return str;
		// get description
		return desc.Length is 0 ? str : desc[0].Description;
	}

	/// <summary>
	///     Get value from enum description or name
	/// </summary>
	/// <param name="desc">description</param>
	/// <typeparam name="T">enum type</typeparam>
	/// <returns>enum value</returns>
	public static (bool res, T type) GetValue<T>(string desc) where T : struct, Enum {
		// attempt to directly parse the string to the enum type
		if (Enum.TryParse<T>(desc, true, out var result))
			return (true, result);
		// fall back to checking if the string matches the description of each enum member
		var names = Enum.GetNames(typeof(T));
		// check names
		foreach (var name in names) {
			// parse type
			var e = (Enum)Enum.Parse(typeof(T), name);
			// check description
			if (desc == e.GetDesc())
				// return type
				return (true, (T)e);
		}

		// return default type
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
