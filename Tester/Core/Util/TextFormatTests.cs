using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     TextFormat 클래스의 16진수 포맷과 ASCII 디코딩 테스트.
///     tests for hex formatting and ASCII decoding in TextFormat class.
/// </summary>
public class TextFormatTests {
    #region ToHex

    /// <summary>
    ///     기본 구분자(공백)로 바이트 배열을 16진수 문자열로 변환하는지 검증합니다.
    ///     verifies byte array is formatted as hex string with default separator (space).
    /// </summary>
    [Fact]
    public void ToHex_DefaultSeparator_ReturnsSpaceSeparatedHex() {
        // 테스트 바이트 배열 준비
        // prepare test byte array
        byte[] values = [0x01, 0x0A, 0xFF];

        // 기본 구분자로 16진수 문자열 변환
        // format hex string with default separator
        var result = TextFormat.ToHex(values, lineBreakAt: 0);

        // 공백 구분 16진수 문자열인지 확인
        // verify space-separated hex string
        Assert.Equal("01 0A FF", result);
    }

    /// <summary>
    ///     사용자 지정 구분자로 16진수 문자열을 변환하는지 검증합니다.
    ///     verifies hex string is formatted with custom separator.
    /// </summary>
    [Fact]
    public void ToHex_CustomSeparator_UsesSpecifiedSeparator() {
        // 테스트 바이트 배열 준비
        // prepare test byte array
        byte[] values = [0xAB, 0xCD];

        // 하이픈 구분자로 16진수 문자열 변환
        // format hex string with hyphen separator
        var result = TextFormat.ToHex(values, "-", 0);

        // 하이픈 구분 16진수 문자열인지 확인
        // verify hyphen-separated hex string
        Assert.Equal("AB-CD", result);
    }

    /// <summary>
    ///     빈 바이트 배열에 대해 빈 문자열을 반환하는지 검증합니다.
    ///     verifies empty byte array returns empty string.
    /// </summary>
    [Fact]
    public void ToHex_EmptyInput_ReturnsEmptyString() {
        // 빈 바이트 배열 준비
        // prepare empty byte array
        byte[] values = [];

        // 빈 배열의 16진수 문자열 변환
        // format hex string of empty array
        var result = TextFormat.ToHex(values);

        // 빈 문자열이 반환되는지 확인
        // verify empty string is returned
        Assert.Equal(string.Empty, result);
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
        var result = TextFormat.DecodeAscii(span);

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
        var result = TextFormat.DecodeAscii(span);

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
        var result = TextFormat.DecodeAscii(span);

        // 빈 문자열이 반환되는지 확인
        // verify empty string is returned
        Assert.Equal(string.Empty, result);
    }

    #endregion
}
