using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     TorqueUnit 클래스의 단위 변환·포맷·파싱 테스트.
///     tests for conversion, format, and parse methods in TorqueUnit class.
/// </summary>
public class TorqueUnitTests {
    #region Convert

    /// <summary>
    ///     동일 단위 변환 시 원본 값이 그대로 반환되는지 검증합니다.
    ///     verifies same-unit conversion returns the original value unchanged.
    /// </summary>
    [Fact]
    public void Convert_SameUnit_ReturnsOriginalValue() {
        // 테스트 토크 값 설정
        // set test torque value
        const float value = 10.0f;

        // N.m → N.m 동일 단위 변환 실행
        // execute same-unit conversion N.m to N.m
        var result = TorqueUnit.Convert(value, Unit.Nm, Unit.Nm);

        // 원본 값과 동일한지 확인
        // verify result equals original value
        Assert.Equal(value, result);
    }

    /// <summary>
    ///     N.m에서 kgf.cm으로의 변환이 올바른 계수를 사용하는지 검증합니다.
    ///     verifies N.m to kgf.cm conversion uses the correct factor.
    /// </summary>
    [Fact]
    public void Convert_NmToKgfCm_ReturnsCorrectValue() {
        // 1 N.m 입력 값 설정
        // set input value of 1 N.m
        const float value = 1.0f;
        // N.m → kgf.cm 변환 계수
        // N.m to kgf.cm conversion factor
        const float expectedFactor = 10.1971621f;

        // N.m에서 kgf.cm으로 변환 실행
        // execute conversion from N.m to kgf.cm
        var result = TorqueUnit.Convert(value, Unit.Nm, Unit.KgfCm);

        // 변환 결과가 기대 계수와 일치하는지 확인 (float 정밀도 허용)
        // verify conversion result matches expected factor (float precision tolerance)
        Assert.Equal(expectedFactor, result, 4);
    }

    /// <summary>
    ///     kgf.cm에서 N.m으로의 역변환이 올바른지 검증합니다.
    ///     verifies kgf.cm to N.m reverse conversion is correct.
    /// </summary>
    [Fact]
    public void Convert_KgfCmToNm_ReturnsCorrectValue() {
        // 10.1971621 kgf.cm 입력 (= 1 N.m)
        // set input of 10.1971621 kgf.cm (= 1 N.m)
        const float value = 10.1971621f;

        // kgf.cm에서 N.m으로 변환 실행
        // execute conversion from kgf.cm to N.m
        var result = TorqueUnit.Convert(value, Unit.KgfCm, Unit.Nm);

        // 변환 결과가 1.0 N.m과 일치하는지 확인
        // verify conversion result matches 1.0 N.m
        Assert.Equal(1.0f, result, 4);
    }

    /// <summary>
    ///     N.m에서 N.cm으로의 변환이 100배 스케일링인지 검증합니다.
    ///     verifies N.m to N.cm conversion applies 100x scaling.
    /// </summary>
    [Fact]
    public void Convert_NmToNCm_Returns100xScaled() {
        // 1 N.m 입력 값 설정
        // set input value of 1 N.m
        const float value = 1.0f;

        // N.m에서 N.cm으로 변환 실행
        // execute conversion from N.m to N.cm
        var result = TorqueUnit.Convert(value, Unit.Nm, Unit.NCm);

        // 100배 스케일링 결과 확인
        // verify 100x scaling result
        Assert.Equal(100.0f, result, 4);
    }

    /// <summary>
    ///     0 값의 단위 변환이 0을 반환하는지 검증합니다.
    ///     verifies conversion of zero value returns zero.
    /// </summary>
    [Fact]
    public void Convert_ZeroValue_ReturnsZero() {
        // 0 값 설정
        // set zero value
        const float value = 0.0f;

        // 0 값의 단위 변환 실행
        // execute unit conversion of zero value
        var result = TorqueUnit.Convert(value, Unit.Nm, Unit.LbfIn);

        // 변환 결과가 0인지 확인
        // verify conversion result is zero
        Assert.Equal(0.0f, result);
    }

    /// <summary>
    ///     lbf.in에서 ozf.in으로의 변환이 16배 관계를 따르는지 검증합니다.
    ///     verifies lbf.in to ozf.in conversion follows the 16x relationship.
    /// </summary>
    [Fact]
    public void Convert_LbfInToOzfIn_ReturnsCorrectValue() {
        // 1 lbf.in 입력 값 설정
        // set input value of 1 lbf.in
        const float value = 1.0f;

        // lbf.in에서 ozf.in으로 변환 실행
        // execute conversion from lbf.in to ozf.in
        var result = TorqueUnit.Convert(value, Unit.LbfIn, Unit.OzfIn);

        // 1 lbf.in = 16 ozf.in 관계 확인 (정밀도 허용)
        // verify 1 lbf.in = 16 ozf.in relationship (precision tolerance)
        Assert.Equal(16.0f, result, 1);
    }

    #endregion

    #region Format

    /// <summary>
    ///     각 단위 코드가 올바른 문자열을 반환하는지 검증합니다.
    ///     verifies each unit code returns the correct string.
    /// </summary>
    [Theory]
    [InlineData(0, "kgf.cm")]
    [InlineData(1, "kgf.m")]
    [InlineData(2, "N.m")]
    [InlineData(3, "N.cm")]
    [InlineData(4, "ozf.in")]
    [InlineData(5, "lbf.ft")]
    [InlineData(6, "ozf.ft")]
    public void Format_ValidCode_ReturnsExpectedString(int unitCode, string expected) {
        // 단위 코드를 문자열로 변환
        // convert unit code to string
        var result = TorqueUnit.Format(unitCode);

        // 기대 문자열과 일치하는지 확인
        // verify result matches expected string
        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     범위 밖의 단위 코드에 대해 기본값 "kgf.cm"을 반환하는지 검증합니다.
    ///     verifies out-of-range unit code returns default "kgf.cm".
    /// </summary>
    [Fact]
    public void Format_InvalidCode_ReturnsDefault() {
        // 범위 밖의 단위 코드 설정
        // set out-of-range unit code
        const int invalidCode = 99;

        // 범위 밖 코드의 단위 문자열 변환
        // convert out-of-range code to unit string
        var result = TorqueUnit.Format(invalidCode);

        // 기본값 "kgf.cm"이 반환되는지 확인
        // verify default "kgf.cm" is returned
        Assert.Equal("kgf.cm", result);
    }

    #endregion

    #region Parse

    /// <summary>
    ///     유효한 단위 문자열이 올바른 Unit 열거형으로 변환되는지 검증합니다.
    ///     verifies valid unit string is parsed to correct Unit enum value.
    /// </summary>
    [Theory]
    [InlineData("kgf.cm", Unit.KgfCm)]
    [InlineData("N.m", Unit.Nm)]
    [InlineData("lbf.in", Unit.LbfIn)]
    public void Parse_ValidString_ReturnsCorrectEnum(string text, Unit expected) {
        // 단위 문자열을 열거형으로 변환
        // convert unit string to enum
        var result = TorqueUnit.Parse(text);

        // 기대 열거형 값과 일치하는지 확인
        // verify result matches expected enum value
        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     null 또는 빈 문자열에 대해 KgfCm 기본값을 반환하는지 검증합니다.
    ///     verifies null or empty string returns KgfCm default.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Parse_NullOrEmpty_ReturnsKgfCm(string? text) {
        // null 또는 빈 문자열의 단위 변환
        // convert null or empty string to unit
        var result = TorqueUnit.Parse(text!);

        // 기본값 KgfCm이 반환되는지 확인
        // verify KgfCm default is returned
        Assert.Equal(Unit.KgfCm, result);
    }

    #endregion
}