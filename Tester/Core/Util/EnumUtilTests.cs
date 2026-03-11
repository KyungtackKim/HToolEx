using HTool.Core.Type.Device;
using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     EnumUtil 클래스의 확장 메서드 및 캐시 조회 테스트
///     tests for extension methods and cached lookup in EnumUtil class
/// </summary>
public class EnumUtilTests {
    #region GetDesc

    /// <summary>
    ///     [Description] 속성이 있는 열거형 값에서 설명 문자열을 반환하는지 검증합니다.
    ///     verifies description string is returned for enum value with [Description] attribute.
    /// </summary>
    [Fact]
    public void GetDesc_WithDescriptionAttribute_ReturnsDescription() {
        // [Description("N.m")] 속성이 있는 Unit.Nm 사용
        // use Unit.Nm which has [Description("N.m")] attribute
        const Unit unit = Unit.Nm;

        // 설명 문자열 가져오기
        // get description string
        var result = unit.GetDesc();

        // Description 속성 값 "N.m"이 반환되는지 확인
        // verify Description attribute value "N.m" is returned
        Assert.Equal("N.m", result);
    }

    /// <summary>
    ///     Direction 열거형의 Description 속성이 올바르게 반환되는지 검증합니다.
    ///     verifies Description attribute is correctly returned for Direction enum.
    /// </summary>
    [Fact]
    public void GetDesc_DirectionFastening_ReturnsFasteningDescription() {
        // [Description("Fastening")] 속성이 있는 Direction.Fastening 사용
        // use Direction.Fastening which has [Description("Fastening")] attribute
        const Direction direction = Direction.Fastening;

        // 설명 문자열 가져오기
        // get description string
        var result = direction.GetDesc();

        // Description 속성 값 "Fastening"이 반환되는지 확인
        // verify Description attribute value "Fastening" is returned
        Assert.Equal("Fastening", result);
    }

    /// <summary>
    ///     모든 Unit 열거형 값에 대해 GetDesc가 비어 있지 않은 문자열을 반환하는지 검증합니다.
    ///     verifies GetDesc returns non-empty string for all Unit enum values.
    /// </summary>
    [Fact]
    public void GetDesc_AllUnitValues_ReturnsNonEmptyString() {
        // 모든 Unit 열거형 값을 순회
        // iterate through all Unit enum values
        foreach (var unit in Enum.GetValues<Unit>()) {
            // 각 값의 설명 문자열 가져오기
            // get description string for each value
            var result = unit.GetDesc();

            // 결과가 비어 있지 않은지 확인
            // verify result is not empty
            Assert.False(string.IsNullOrEmpty(result));
        }
    }

    #endregion

    #region GetValue

    /// <summary>
    ///     Description 문자열로 열거형 값을 역방향 조회할 수 있는지 검증합니다.
    ///     verifies enum value can be reverse-looked-up by Description string.
    /// </summary>
    [Fact]
    public void GetValue_ByDescription_ReturnsTrue() {
        // Description "N.m"으로 Unit 열거형 조회
        // look up Unit enum by Description "N.m"
        var (res, type) = EnumUtil.GetValue<Unit>("N.m");

        // 조회 성공 여부 확인
        // verify lookup succeeded
        Assert.True(res);
        // 조회된 값이 Unit.Nm인지 확인
        // verify looked-up value is Unit.Nm
        Assert.Equal(Unit.Nm, type);
    }

    /// <summary>
    ///     열거형 멤버 이름으로 조회할 수 있는지 검증합니다.
    ///     verifies enum value can be looked up by member name.
    /// </summary>
    [Fact]
    public void GetValue_ByEnumName_ReturnsTrue() {
        // 열거형 멤버 이름 "Nm"으로 Unit 열거형 조회
        // look up Unit enum by member name "Nm"
        var (res, type) = EnumUtil.GetValue<Unit>("Nm");

        // 조회 성공 여부 확인
        // verify lookup succeeded
        Assert.True(res);
        // 조회된 값이 Unit.Nm인지 확인
        // verify looked-up value is Unit.Nm
        Assert.Equal(Unit.Nm, type);
    }

    /// <summary>
    ///     대소문자를 무시한 멤버 이름 조회가 동작하는지 검증합니다.
    ///     verifies case-insensitive member name lookup works.
    /// </summary>
    [Fact]
    public void GetValue_CaseInsensitiveName_ReturnsTrue() {
        // 소문자 멤버 이름 "nm"으로 Unit 열거형 조회
        // look up Unit enum by lowercase member name "nm"
        var (res, type) = EnumUtil.GetValue<Unit>("nm");

        // 조회 성공 여부 확인
        // verify lookup succeeded
        Assert.True(res);
        // 조회된 값이 Unit.Nm인지 확인
        // verify looked-up value is Unit.Nm
        Assert.Equal(Unit.Nm, type);
    }

    /// <summary>
    ///     존재하지 않는 문자열로 조회 시 false를 반환하는지 검증합니다.
    ///     verifies unknown string returns false.
    /// </summary>
    [Fact]
    public void GetValue_UnknownString_ReturnsFalse() {
        // 존재하지 않는 문자열로 Unit 열거형 조회
        // look up Unit enum by non-existent string
        var (res, _) = EnumUtil.GetValue<Unit>("UNKNOWN_UNIT");

        // 조회 실패 여부 확인
        // verify lookup failed
        Assert.False(res);
    }

    #endregion

    #region IsDefined

    /// <summary>
    ///     정의된 long 값에 대해 IsDefined가 true를 반환하는지 검증합니다.
    ///     verifies IsDefined returns true for a defined long value.
    /// </summary>
    [Fact]
    public void IsDefined_DefinedLongValue_ReturnsTrue() {
        // Model.Md의 정수 값 (1)으로 확인
        // check with integer value (1) of Model.Md
        var result = EnumUtil.IsDefined<Model>(1L);

        // 정의된 값에 대해 true 반환 확인
        // verify true is returned for defined value
        Assert.True(result);
    }

    /// <summary>
    ///     정의되지 않은 long 값에 대해 IsDefined가 false를 반환하는지 검증합니다.
    ///     verifies IsDefined returns false for an undefined long value.
    /// </summary>
    [Fact]
    public void IsDefined_UndefinedLongValue_ReturnsFalse() {
        // Model에 정의되지 않은 값 (999)으로 확인
        // check with undefined value (999) in Model
        var result = EnumUtil.IsDefined<Model>(999L);

        // 정의되지 않은 값에 대해 false 반환 확인
        // verify false is returned for undefined value
        Assert.False(result);
    }

    /// <summary>
    ///     정의된 열거형 값에 대해 IsDefined(T)가 true를 반환하는지 검증합니다.
    ///     verifies IsDefined(T) returns true for a defined enum value.
    /// </summary>
    [Fact]
    public void IsDefined_DefinedEnumValue_ReturnsTrue() {
        // Model.Bm 열거형 값으로 확인
        // check with Model.Bm enum value
        var result = EnumUtil.IsDefined(Model.Bm);

        // 정의된 값에 대해 true 반환 확인
        // verify true is returned for defined value
        Assert.True(result);
    }

    /// <summary>
    ///     정의되지 않은 열거형 값에 대해 IsDefined(T)가 false를 반환하는지 검증합니다.
    ///     verifies IsDefined(T) returns false for an undefined enum value.
    /// </summary>
    [Fact]
    public void IsDefined_UndefinedEnumValue_ReturnsFalse() {
        // Model에 정의되지 않은 캐스팅 값으로 확인
        // check with undefined casted value in Model
        var result = EnumUtil.IsDefined((Model)999);

        // 정의되지 않은 값에 대해 false 반환 확인
        // verify false is returned for undefined value
        Assert.False(result);
    }

    #endregion
}