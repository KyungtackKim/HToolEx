using System.Collections.Frozen;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HTool.Core.Util;

/// <summary>
///     열거형(Enum) 확장 메서드 유틸리티 클래스. FrozenDictionary 캐시를 통한 O(1) 조회를 제공합니다.
///     enum extension methods utility class. Provides O(1) lookup via FrozenDictionary cache.
/// </summary>
/// <remarks>
///     모든 열거형은 [Description] 속성으로 설명을 제공합니다. UI 표시 시 이 클래스를 사용합니다.
///     all enums provide descriptions via [Description] attribute. Use this class for UI display.
/// </remarks>
public static class EnumUtil {
	/// <summary>
	///     enum 값에서 Description 또는 이름 가져오기 (O(1) 캐시 조회)
	///     gets description or name from enum value (O(1) cached lookup)
	/// </summary>
	/// <param name="value">enum 값 / enum value</param>
	/// <returns>설명 / description</returns>
	public static string GetDesc(this Enum value) {
        // get the enum's runtime type / 캐시에서 설명 조회
        var type = value.GetType();
        // get or create the description cache for this type / 캐시 가져오기
        var cache = DescCache.GetOrCreate(type);
        // get the string key from enum value / 문자열 키 가져오기
        var key = value.ToString();
        // lookup description from cache, fallback to key / 캐시에서 조회
        return cache.GetValueOrDefault(key, key);
    }

	/// <summary>
	///     enum 설명 또는 이름에서 값 가져오기 (O(1) 역방향 캐시 조회)
	///     gets value from enum description or name (O(1) reverse cache lookup)
	/// </summary>
	/// <param name="desc">설명 / description</param>
	/// <typeparam name="T">enum 타입 / enum type</typeparam>
	/// <returns>찾으면 (true, 값), 실패 시 (false, 첫 번째 값) / (true, value) if found, (false, first value) on failure</returns>
	public static (bool res, T type) GetValue<T>(string desc) where T : struct, Enum {
        // attempt to directly parse the string to the Enum type / 문자열을 Enum 타입으로 직접 파싱 시도
        if (Enum.TryParse<T>(desc, true, out var result))
            // return found result / 찾은 결과 반환
            return (true, result);
        // get the reverse cache instance / 역방향 캐시에서 조회
        var reverse = ReverseCache<T>.Instance;
        // lookup by description text / 설명으로 조회
        if (reverse.TryGetValue(desc, out var found))
            // return found result / 찾은 결과 반환
            return (true, found);

        // return first defined value as default / 기본 타입 반환
        var values = Enum.GetValues<T>();
        // return default or first value / 기본값 또는 첫 번째 값 반환
        return (false, values.Length > 0 ? values[0] : default);
    }

	/// <summary>
	///     정수 값이 enum 타입에 정의되어 있는지 확인합니다 (O(1) 캐시 조회).
	///     checks if integer value is defined in enum type (O(1) cached lookup).
	/// </summary>
	/// <typeparam name="T">enum 타입 / enum type</typeparam>
	/// <param name="value">확인할 정수 값 / integer value to check</param>
	/// <returns>정의되어 있으면 true / true if defined</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefined<T>(long value) where T : struct, Enum {
        // 캐시된 FrozenSet으로 확인
        // check using cached FrozenSet
        return DefinedCache<T>.Values.Contains(value);
    }

	/// <summary>
	///     enum 값이 해당 타입에 정의되어 있는지 확인합니다 (O(1) 캐시 조회).
	///     checks if enum value is defined in its type (O(1) cached lookup).
	/// </summary>
	/// <typeparam name="T">enum 타입 / enum type</typeparam>
	/// <param name="value">확인할 enum 값 / enum value to check</param>
	/// <returns>정의되어 있으면 true / true if defined</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefined<T>(T value) where T : struct, Enum {
        // long으로 변환하여 캐시된 세트에서 확인
        // convert to long and check against cached set
        return DefinedCache<T>.Values.Contains(Convert.ToInt64(value));
    }

	/// <summary>
	///     Description 속성 캐시 (타입별 FrozenDictionary)
	///     description attribute cache (FrozenDictionary per type)
	/// </summary>
	private static class DescCache {
        // per-type cache storage / 타입별 캐시 저장소
        private static readonly Dictionary<System.Type, FrozenDictionary<string, string>> Store = new();

        // lock object for thread-safe access / 락 객체
        private static readonly object Lock = new();

        /// <summary>
        ///     타입에 대한 캐시를 가져오거나 생성합니다.
        ///     gets or creates cache for a type.
        /// </summary>
        /// <param name="enumType">enum 타입 / enum type</param>
        /// <returns>이름 → 설명 매핑 / name to description mapping</returns>
        public static FrozenDictionary<string, string> GetOrCreate(System.Type enumType) {
            lock (Lock) {
                // check if cache already exists for this type / 캐시 존재 확인
                if (Store.TryGetValue(enumType, out var cached))
                    // return existing cache / 기존 캐시 반환
                    return cached;

                // create new dictionary for descriptions / 새 캐시 생성
                var dict = new Dictionary<string, string>();
                // iterate over all public static fields / 필드 순회
                foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                    // get field name / 이름 가져오기
                    var name = field.Name;
                    // get Description attribute if present / Description 속성 가져오기
                    var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    // set description from attribute or fallback to name / 설명 설정
                    dict[name] = attr.Length > 0 ? ((DescriptionAttribute)attr[0]).Description : name;
                }

                // convert to FrozenDictionary and store in cache / FrozenDictionary로 변환 후 저장
                var frozen = dict.ToFrozenDictionary();
                // store in cache / 캐시에 저장
                Store[enumType] = frozen;
                // return frozen dictionary / 변환된 딕셔너리 반환
                return frozen;
            }
        }
    }

	/// <summary>
	///     역방향 캐시: 설명 → enum 값 (제네릭 타입별 FrozenDictionary)
	///     reverse cache: description to enum value (FrozenDictionary per generic type)
	/// </summary>
	/// <typeparam name="T">enum 타입 / enum type</typeparam>
	private static class ReverseCache<T> where T : struct, Enum {
		/// <summary>
		///     설명 → enum 값 역방향 매핑
		///     description to enum value reverse mapping
		/// </summary>
		public static readonly FrozenDictionary<string, T> Instance = BuildReverse();

		/// <summary>
		///     역방향 캐시를 생성합니다.
		///     builds the reverse cache.
		/// </summary>
		private static FrozenDictionary<string, T> BuildReverse() {
            // create dictionary for reverse mapping / 딕셔너리 생성
            var dict = new Dictionary<string, T>();
            // iterate over all enum values / enum 값 순회
            foreach (var val in Enum.GetValues<T>()) {
                // get description for this value / Description 가져오기
                var desc = val.GetDesc();
                // add mapping, first value wins on duplicate / 매핑 추가 (중복 시 첫 번째 우선)
                dict.TryAdd(desc, val);
            }

            // convert to FrozenDictionary for O(1) lookup / FrozenDictionary로 변환
            return dict.ToFrozenDictionary();
        }
    }

	/// <summary>
	///     정의된 값 캐시 (제네릭 타입별 FrozenSet)
	///     defined values cache (FrozenSet per generic type)
	/// </summary>
	/// <typeparam name="T">enum 타입 / enum type</typeparam>
	private static class DefinedCache<T> where T : struct, Enum {
		/// <summary>
		///     미리 계산된 유효 enum 값 세트 (long)
		///     pre-computed set of valid enum values (as long)
		/// </summary>
		public static readonly FrozenSet<long> Values =
            // 모든 enum 값을 long으로 변환하여 FrozenSet 생성
            // convert all enum values to long and create FrozenSet
            Enum.GetValues<T>().Select(v => Convert.ToInt64(v)).ToFrozenSet();
    }
}