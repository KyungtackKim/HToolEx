using System.Collections.Frozen;

namespace HTool.Core.Type.Device;

/// <summary>
///     모델 코드와 표시 이름 간 양방향 변환을 제공합니다.
///     provides bidirectional conversion between model codes and display names.
/// </summary>
public static class ModelNames {
    // 한타스 모델 → 표시 이름 매핑 / Hantas model-to-display-name mapping
    private static readonly FrozenDictionary<Model, string> HantasNames =
        new Dictionary<Model, string> {
            [Model.Md]    = "MD",
            [Model.Ad]    = "AD",
            [Model.Bm]    = "BM",
            [Model.Mdt]   = "MDT",
            [Model.Bmt]   = "BMT",
            [Model.Bpt]   = "BPT",
            [Model.Mdt40] = "MDT40",
            [Model.Bmt40] = "BMT40",
            [Model.Et]    = "ET",
            [Model.Bt]    = "BT"
        }.ToFrozenDictionary();

    // 마운츠 모델 → 표시 이름 매핑 / Mountz model-to-display-name mapping
    private static readonly FrozenDictionary<Model, string> MountzNames =
        new Dictionary<Model, string> {
            [Model.Md]    = "EC",
            [Model.Ad]    = "AD",
            [Model.Bm]    = "EP",
            [Model.Mdt]   = "ECT",
            [Model.Bmt]   = "EPT",
            [Model.Bpt]   = "BPT",
            [Model.Mdt40] = "ECT40",
            [Model.Bmt40] = "EPT40",
            [Model.Et]    = "ETM",
            [Model.Bt]    = "BTM"
        }.ToFrozenDictionary();

    // name-to-(model, manufacturer) reverse mapping (case-insensitive) / 이름 → (모델, 제조사) 역방향 매핑 (대소문자 무시)
    private static readonly FrozenDictionary<string, (Model Model, Manufacturer Manufacturer)> ReverseLookup =
        BuildReverseLookup();

    /// <summary>
    ///     모델 코드를 제조사별 표시 이름으로 변환합니다.
    ///     converts a model code to its manufacturer-specific display name.
    /// </summary>
    /// <param name="model">장치 모델 코드 / device model code</param>
    /// <param name="manufacturer">제조사 (기본값: Hantas) / manufacturer (default: Hantas)</param>
    /// <returns>표시 이름 문자열 / display name string</returns>
    public static string ToName(Model model, Manufacturer manufacturer = Manufacturer.Hantas) {
        // select the name dictionary for the given manufacturer / 제조사에 맞는 이름 사전 선택
        var names = manufacturer is Manufacturer.Hantas ? HantasNames : MountzNames;
        // look up the display name, fall back to enum member name / 표시 이름 조회, 없으면 열거형 멤버 이름 반환
        return names.GetValueOrDefault(model, model.ToString());
    }

    /// <summary>
    ///     표시 이름으로 모델 코드를 검색합니다. 제조사를 구분하지 않고 첫 번째 일치 항목을 반환합니다.
    ///     looks up a model code by display name. Returns the first match regardless of manufacturer.
    /// </summary>
    /// <param name="name">표시 이름 (대소문자 무시) / display name (case-insensitive)</param>
    /// <returns>일치하는 모델 코드, 또는 null / matching model code, or null</returns>
    public static Model? FromName(string name) {
        // attempt case-insensitive reverse lookup / 대소문자 무시 역방향 조회 시도
        return ReverseLookup.TryGetValue(name, out var entry) ? entry.Model : null;
    }

    /// <summary>
    ///     표시 이름으로 모델 코드와 제조사를 함께 검색합니다.
    ///     looks up both model code and manufacturer by display name.
    /// </summary>
    /// <param name="name">표시 이름 (대소문자 무시) / display name (case-insensitive)</param>
    /// <returns>모델과 제조사 튜플, 또는 null / model and manufacturer tuple, or null</returns>
    public static (Model Model, Manufacturer Manufacturer)? Parse(string name) {
        // attempt case-insensitive reverse lookup with full tuple / 대소문자 무시 역방향 조회 (전체 튜플 반환)
        return ReverseLookup.TryGetValue(name, out var entry) ? entry : null;
    }

    /// <summary>
    ///     양방향 역방향 조회 사전을 구축합니다. Mountz 이름을 먼저 등록하고 Hantas 이름으로 덮어씁니다.
    ///     builds the reverse lookup dictionary. Registers Mountz names first, then overwrites with Hantas names.
    /// </summary>
    private static FrozenDictionary<string, (Model Model, Manufacturer Manufacturer)> BuildReverseLookup() {
        // pre-allocate with combined capacity of both dictionaries / 두 사전의 합산 용량으로 사전 초기화
        var dict = new Dictionary<string, (Model Model, Manufacturer Manufacturer)>(
            HantasNames.Count + MountzNames.Count,
            StringComparer.OrdinalIgnoreCase
        );

        // register Mountz names first (lower priority) / 마운츠 이름을 먼저 등록 (낮은 우선순위)
        foreach (var (model, name) in MountzNames)
            dict[name] = (model, Manufacturer.Mountz);

        // overwrite with Hantas names (higher priority) / 한타스 이름으로 덮어쓰기 (높은 우선순위)
        foreach (var (model, name) in HantasNames)
            dict[name] = (model, Manufacturer.Hantas);

        // freeze for optimal read performance / 읽기 성능 최적화를 위해 고정
        return dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }
}