using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HTool.Util;

/// <summary>
///     열거형(Enum) 확장 메서드 유틸리티 클래스. Description 속성에서 표시 문자열을 추출합니다.
///     Enum extension methods utility class. Extracts display strings from Description attributes.
/// </summary>
/// <remarks>
///     HTool의 모든 열거형은 [Description] 속성으로 한/영 설명을 제공합니다. UI 표시 시 이 클래스를 사용합니다.
///     All HTool enums provide Korean/English descriptions via [Description] attribute. Use this class for UI display.
/// </remarks>
public static class EnumUtil {
    /// <summary>
    ///     enum 값에서 Description 또는 이름 가져오기
    ///     Get description or name from enum value
    /// </summary>
    /// <param name="value">enum 값 / enum value</param>
    /// <returns>설명 / description</returns>
    public static string GetDesc(this Enum value) {
        // 값 문자열 가져오기
        // get value string
        var str = $"{value}";
        // 값 필드 가져오기
        // get value field
        var field = value.GetType().GetField(str);
        // 필드 확인
        // check field
        if (field == null)
            return str;
        // 속성 가져오기
        // get attributes
        if (field.GetCustomAttributes(typeof(DescriptionAttribute), false) is not DescriptionAttribute[] desc)
            return str;
        // 설명 반환
        // return description
        return desc.Length == 0 ? str : desc[0].Description;
    }

    /// <summary>
    ///     표시 이름 가져오기
    ///     Get display name
    /// </summary>
    /// <param name="value">enum 값 / enum value</param>
    /// <returns>표시 이름 / display name</returns>
    public static string GetDisplay(this Enum value) {
        // 타입 가져오기
        // get type
        var type = value.GetType();
        // 멤버 확인
        // check member
        var member = type.GetMember($"{value}");
        // 멤버 확인
        // check member
        if (member.Length == 0)
            // 반환
            // return
            return string.Empty;
        // 커스텀 속성 가져오기
        // get custom attribute
        var attr = member[0].GetCustomAttribute<DisplayAttribute>();
        // 속성 확인
        // check attribute
        if (attr == null)
            // 반환
            // return
            return string.Empty;
        // 설명 반환
        // return description
        return attr.GetDescription() ?? string.Empty;
    }

    /// <summary>
    ///     enum 설명 또는 이름에서 값 가져오기
    ///     Get value from enum description or name
    /// </summary>
    /// <param name="desc">설명 / description</param>
    /// <typeparam name="T">enum 타입 / enum type</typeparam>
    /// <returns>enum 값 / enum value</returns>
    public static (bool res, T type) GetValue<T>(string desc) where T : struct, Enum {
        // 문자열을 Enum 타입으로 직접 파싱 시도
        // attempt to directly parse the string to the Enum type
        if (Enum.TryParse<T>(desc, true, out var result))
            return (true, result);
        // 문자열이 각 Enum 멤버의 설명과 일치하는지 확인
        // fall back to checking if the string matches the description
        var names = Enum.GetNames(typeof(T));
        // 이름 확인
        // check names
        foreach (var name in names) {
            // 타입 파싱
            // parse type
            var e = (Enum)Enum.Parse(typeof(T), name);
            // 설명 확인
            // check description
            if (desc == GetDesc(e))
                // 타입 반환
                // return type
                return (true, (T)e);
        }

        // 기본 타입 반환
        // return default type
        return (false, (T)Enum.Parse(typeof(T), names[0]));
    }
}