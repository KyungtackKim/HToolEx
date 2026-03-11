using HTool.Core.Type.Device;

namespace Tester.Core.Type;

/// <summary>
///     ModelNames 클래스의 모델 코드 ↔ 표시 이름 양방향 변환 테스트
///     tests for bidirectional conversion between model codes and display names in ModelNames class
/// </summary>
public class ModelNamesTests {
    #region ToName

    /// <summary>
    ///     한타스 제조사의 Md 모델이 "MD"를 반환하는지 검증합니다.
    ///     verifies Hantas Md model returns "MD".
    /// </summary>
    [Fact]
    public void ToName_HantasMd_ReturnsMD() {
        // 한타스 Md 모델의 표시 이름 가져오기
        // get display name for Hantas Md model
        var result = ModelNames.ToName(Model.Md);

        // "MD"가 반환되는지 확인
        // verify "MD" is returned
        Assert.Equal("MD", result);
    }

    /// <summary>
    ///     마운츠 제조사의 Md 모델이 "EC"를 반환하는지 검증합니다.
    ///     verifies Mountz Md model returns "EC".
    /// </summary>
    [Fact]
    public void ToName_MountzMd_ReturnsEC() {
        // 마운츠 Md 모델의 표시 이름 가져오기
        // get display name for Mountz Md model
        var result = ModelNames.ToName(Model.Md, Manufacturer.Mountz);

        // "EC"가 반환되는지 확인
        // verify "EC" is returned
        Assert.Equal("EC", result);
    }

    /// <summary>
    ///     제조사 기본값(Hantas)으로 올바른 이름을 반환하는지 검증합니다.
    ///     verifies correct name is returned with default manufacturer (Hantas).
    /// </summary>
    [Fact]
    public void ToName_DefaultManufacturer_UsesHantas() {
        // 기본 제조사(Hantas)로 Bm 모델의 표시 이름 가져오기
        // get display name for Bm model with default manufacturer (Hantas)
        var result = ModelNames.ToName(Model.Bm);

        // 한타스 이름 "BM"이 반환되는지 확인
        // verify Hantas name "BM" is returned
        Assert.Equal("BM", result);
    }

    /// <summary>
    ///     한타스와 마운츠의 전체 모델 매핑이 올바른지 검증합니다.
    ///     verifies full model mapping for both Hantas and Mountz manufacturers.
    /// </summary>
    [Theory]
    [InlineData(Model.Md, Manufacturer.Hantas, "MD")]
    [InlineData(Model.Ad, Manufacturer.Hantas, "AD")]
    [InlineData(Model.Bm, Manufacturer.Hantas, "BM")]
    [InlineData(Model.Mdt, Manufacturer.Hantas, "MDT")]
    [InlineData(Model.Bmt, Manufacturer.Hantas, "BMT")]
    [InlineData(Model.Bpt, Manufacturer.Hantas, "BPT")]
    [InlineData(Model.Mdt40, Manufacturer.Hantas, "MDT40")]
    [InlineData(Model.Bmt40, Manufacturer.Hantas, "BMT40")]
    [InlineData(Model.Et, Manufacturer.Hantas, "ET")]
    [InlineData(Model.Bt, Manufacturer.Hantas, "BT")]
    [InlineData(Model.Md, Manufacturer.Mountz, "EC")]
    [InlineData(Model.Bm, Manufacturer.Mountz, "EP")]
    [InlineData(Model.Mdt, Manufacturer.Mountz, "ECT")]
    [InlineData(Model.Bmt, Manufacturer.Mountz, "EPT")]
    [InlineData(Model.Et, Manufacturer.Mountz, "ETM")]
    [InlineData(Model.Bt, Manufacturer.Mountz, "BTM")]
    public void ToName_AllModels_ReturnsExpectedName(Model model, Manufacturer manufacturer, string expected) {
        // 모델과 제조사로 표시 이름 가져오기
        // get display name by model and manufacturer
        var result = ModelNames.ToName(model, manufacturer);

        // 기대 이름과 일치하는지 확인
        // verify result matches expected name
        Assert.Equal(expected, result);
    }

    #endregion

    #region FromName

    /// <summary>
    ///     유효한 이름으로 모델 코드를 찾을 수 있는지 검증합니다.
    ///     verifies model code can be found by valid name.
    /// </summary>
    [Fact]
    public void FromName_ValidHantasName_ReturnsModel() {
        // "MDT" 이름으로 모델 코드 조회
        // look up model code by name "MDT"
        var result = ModelNames.FromName("MDT");

        // 결과가 null이 아닌지 확인
        // verify result is not null
        Assert.NotNull(result);
        // 조회된 모델이 Mdt인지 확인
        // verify looked-up model is Mdt
        Assert.Equal(Model.Mdt, result.Value);
    }

    /// <summary>
    ///     대소문자 무시 조회가 동작하는지 검증합니다.
    ///     verifies case-insensitive lookup works.
    /// </summary>
    [Fact]
    public void FromName_CaseInsensitive_ReturnsModel() {
        // 소문자 "md"로 모델 코드 조회
        // look up model code by lowercase "md"
        var result = ModelNames.FromName("md");

        // 결과가 null이 아닌지 확인
        // verify result is not null
        Assert.NotNull(result);
        // 조회된 모델이 Md인지 확인
        // verify looked-up model is Md
        Assert.Equal(Model.Md, result.Value);
    }

    /// <summary>
    ///     존재하지 않는 이름으로 조회 시 null을 반환하는지 검증합니다.
    ///     verifies unknown name returns null.
    /// </summary>
    [Fact]
    public void FromName_UnknownName_ReturnsNull() {
        // 존재하지 않는 이름으로 모델 코드 조회
        // look up model code by non-existent name
        var result = ModelNames.FromName("NONEXISTENT");

        // null이 반환되는지 확인
        // verify null is returned
        Assert.Null(result);
    }

    /// <summary>
    ///     한타스와 마운츠가 공유하는 이름("AD", "BPT")에 대해 한타스가 우선하는지 검증합니다.
    ///     verifies Hantas takes priority for names shared between Hantas and Mountz ("AD", "BPT").
    /// </summary>
    [Fact]
    public void FromName_SharedName_HantasOverridesMountz() {
        // 한타스와 마운츠 모두 "BPT" 이름을 사용 (Hantas가 우선)
        // both Hantas and Mountz use "BPT" name (Hantas takes priority)
        var result = ModelNames.FromName("BPT");

        // 결과가 null이 아닌지 확인
        // verify result is not null
        Assert.NotNull(result);
        // 모델이 Bpt인지 확인
        // verify model is Bpt
        Assert.Equal(Model.Bpt, result.Value);
    }

    #endregion

    #region Parse

    /// <summary>
    ///     유효한 한타스 이름으로 모델과 제조사 튜플을 반환하는지 검증합니다.
    ///     verifies valid Hantas name returns model and manufacturer tuple.
    /// </summary>
    [Fact]
    public void Parse_HantasName_ReturnsModelAndManufacturer() {
        // "BMT" 이름으로 모델 및 제조사 조회
        // look up model and manufacturer by name "BMT"
        var result = ModelNames.Parse("BMT");

        // 결과가 null이 아닌지 확인
        // verify result is not null
        Assert.NotNull(result);
        // 모델이 Bmt인지 확인
        // verify model is Bmt
        Assert.Equal(Model.Bmt, result.Value.Model);
        // 제조사가 Hantas인지 확인
        // verify manufacturer is Hantas
        Assert.Equal(Manufacturer.Hantas, result.Value.Manufacturer);
    }

    /// <summary>
    ///     마운츠 고유 이름("EC")으로 모델과 제조사를 올바르게 반환하는지 검증합니다.
    ///     verifies Mountz-unique name ("EC") returns correct model and manufacturer.
    /// </summary>
    [Fact]
    public void Parse_MountzUniqueName_ReturnsMountzManufacturer() {
        // 마운츠 고유 이름 "EC"로 모델 및 제조사 조회
        // look up model and manufacturer by Mountz-unique name "EC"
        var result = ModelNames.Parse("EC");

        // 결과가 null이 아닌지 확인
        // verify result is not null
        Assert.NotNull(result);
        // 모델이 Md인지 확인 (EC = Mountz의 Md 모델)
        // verify model is Md (EC = Mountz's Md model)
        Assert.Equal(Model.Md, result.Value.Model);
        // 제조사가 Mountz인지 확인
        // verify manufacturer is Mountz
        Assert.Equal(Manufacturer.Mountz, result.Value.Manufacturer);
    }

    /// <summary>
    ///     존재하지 않는 이름으로 Parse 시 null을 반환하는지 검증합니다.
    ///     verifies Parse returns null for unknown name.
    /// </summary>
    [Fact]
    public void Parse_UnknownName_ReturnsNull() {
        // 존재하지 않는 이름으로 조회
        // look up by non-existent name
        var result = ModelNames.Parse("INVALID");

        // null이 반환되는지 확인
        // verify null is returned
        Assert.Null(result);
    }

    #endregion
}