using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     Utils 클래스의 Format/Parse 메서드 테스트
///     tests for Format/Parse methods in Utils class
/// </summary>
public class UtilsFormatTests {
    #region FormatHex

    /// <summary>
    ///     기본 구분자(공백)로 바이트 배열을 16진수 문자열로 변환하는지 검증합니다.
    ///     verifies byte array is formatted as hex string with default separator (space).
    /// </summary>
    [Fact]
    public void FormatHex_DefaultSeparator_ReturnsSpaceSeparatedHex() {
        // 테스트 바이트 배열 준비
        // prepare test byte array
        byte[] values = [0x01, 0x0A, 0xFF];

        // 기본 구분자로 16진수 문자열 변환
        // format hex string with default separator
        var result = Utils.FormatHex(values, lineBreakAt: 0);

        // 공백 구분 16진수 문자열인지 확인
        // verify space-separated hex string
        Assert.Equal("01 0A FF", result);
    }

    /// <summary>
    ///     사용자 지정 구분자로 16진수 문자열을 변환하는지 검증합니다.
    ///     verifies hex string is formatted with custom separator.
    /// </summary>
    [Fact]
    public void FormatHex_CustomSeparator_UsesSpecifiedSeparator() {
        // 테스트 바이트 배열 준비
        // prepare test byte array
        byte[] values = [0xAB, 0xCD];

        // 하이픈 구분자로 16진수 문자열 변환
        // format hex string with hyphen separator
        var result = Utils.FormatHex(values, "-", 0);

        // 하이픈 구분 16진수 문자열인지 확인
        // verify hyphen-separated hex string
        Assert.Equal("AB-CD", result);
    }

    /// <summary>
    ///     빈 바이트 배열에 대해 빈 문자열을 반환하는지 검증합니다.
    ///     verifies empty byte array returns empty string.
    /// </summary>
    [Fact]
    public void FormatHex_EmptyInput_ReturnsEmptyString() {
        // 빈 바이트 배열 준비
        // prepare empty byte array
        byte[] values = [];

        // 빈 배열의 16진수 문자열 변환
        // format hex string of empty array
        var result = Utils.FormatHex(values);

        // 빈 문자열이 반환되는지 확인
        // verify empty string is returned
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region FormatUnit

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
    public void FormatUnit_ValidCode_ReturnsExpectedString(int unitCode, string expected) {
        // 단위 코드를 문자열로 변환
        // convert unit code to string
        var result = Utils.FormatUnit(unitCode);

        // 기대 문자열과 일치하는지 확인
        // verify result matches expected string
        Assert.Equal(expected, result);
    }

    /// <summary>
    ///     범위 밖의 단위 코드에 대해 기본값 "kgf.cm"을 반환하는지 검증합니다.
    ///     verifies out-of-range unit code returns default "kgf.cm".
    /// </summary>
    [Fact]
    public void FormatUnit_InvalidCode_ReturnsDefault() {
        // 범위 밖의 단위 코드 설정
        // set out-of-range unit code
        const int invalidCode = 99;

        // 범위 밖 코드의 단위 문자열 변환
        // convert out-of-range code to unit string
        var result = Utils.FormatUnit(invalidCode);

        // 기본값 "kgf.cm"이 반환되는지 확인
        // verify default "kgf.cm" is returned
        Assert.Equal("kgf.cm", result);
    }

    #endregion

    #region ParseUnit

    /// <summary>
    ///     유효한 단위 문자열이 올바른 Unit 열거형으로 변환되는지 검증합니다.
    ///     verifies valid unit string is parsed to correct Unit enum value.
    /// </summary>
    [Theory]
    [InlineData("kgf.cm", Unit.KgfCm)]
    [InlineData("N.m", Unit.Nm)]
    [InlineData("lbf.in", Unit.LbfIn)]
    public void ParseUnit_ValidString_ReturnsCorrectEnum(string text, Unit expected) {
        // 단위 문자열을 열거형으로 변환
        // convert unit string to enum
        var result = Utils.ParseUnit(text);

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
    public void ParseUnit_NullOrEmpty_ReturnsKgfCm(string? text) {
        // null 또는 빈 문자열의 단위 변환
        // convert null or empty string to unit
        var result = Utils.ParseUnit(text!);

        // 기본값 KgfCm이 반환되는지 확인
        // verify KgfCm default is returned
        Assert.Equal(Unit.KgfCm, result);
    }

    #endregion

    #region DecodeAscii

    /// <summary>
    ///     ASCII 바이트 배열을 올바른 문자열로 디코딩하는지 검증합니다.
    ///     verifies ASCII byte array is decoded to correct string.
    /// </summary>
    [Fact]
    public void DecodeAscii_ValidAscii_ReturnsDecodedString() {
        // "HELLO" ASCII 바이트 배열 준비
        // prepare "HELLO" ASCII byte array
        var span = "HELLO"u8.ToArray();

        // ASCII 디코딩 실행
        // execute ASCII decoding
        var result = Utils.DecodeAscii(span);

        // 디코딩 결과가 "HELLO"인지 확인
        // verify decoded result is "HELLO"
        Assert.Equal("HELLO", result);
    }

    /// <summary>
    ///     후행 null 바이트가 제거되는지 검증합니다.
    ///     verifies trailing null bytes are trimmed.
    /// </summary>
    [Fact]
    public void DecodeAscii_TrailingNulls_TrimsNullBytes() {
        // 후행 null 포함 "AB" 바이트 배열 준비
        // prepare "AB" byte array with trailing nulls
        var span = "AB\0\0\0"u8.ToArray();

        // ASCII 디코딩 실행
        // execute ASCII decoding
        var result = Utils.DecodeAscii(span);

        // 후행 null이 제거된 "AB"인지 확인
        // verify "AB" with trailing nulls trimmed
        Assert.Equal("AB", result);
    }

    /// <summary>
    ///     모든 바이트가 null인 경우 빈 문자열을 반환하는지 검증합니다.
    ///     verifies all-null bytes return empty string.
    /// </summary>
    [Fact]
    public void DecodeAscii_AllNulls_ReturnsEmptyString() {
        // 전체 null 바이트 배열 준비
        // prepare all-null byte array
        var span = "\0\0\0"u8.ToArray();

        // ASCII 디코딩 실행
        // execute ASCII decoding
        var result = Utils.DecodeAscii(span);

        // 빈 문자열이 반환되는지 확인
        // verify empty string is returned
        Assert.Equal(string.Empty, result);
    }

    #endregion
}