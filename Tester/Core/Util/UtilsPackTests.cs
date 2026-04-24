using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     Utils 클래스의 Pack 메서드 (값 → ushort[]) 테스트
///     tests for Pack methods (value to ushort[]) in Utils class
/// </summary>
public class UtilsPackTests {
    #region PackInt32

    /// <summary>
    ///     HighLow 워드 순서로 int32를 ushort 배열로 패킹하는지 검증합니다.
    ///     verifies int32 packing to ushort array with HighLow word order.
    /// </summary>
    [Fact]
    public void PackInt32_HighLow_ReturnsHighWordFirst() {
        // 0x12345678 테스트 값 설정
        // set test value 0x12345678
        const int value = 0x12345678;

        // HighLow 워드 순서로 패킹 실행
        // execute packing with HighLow word order
        var result = Packing.PackInt32(value);

        // 결과 배열 길이가 2인지 확인
        // verify result array length is 2
        Assert.Equal(2, result.Length);
        // 첫 번째 요소가 상위 워드 (0x1234)인지 확인
        // verify first element is high word (0x1234)
        Assert.Equal((ushort)0x1234, result[0]);
        // 두 번째 요소가 하위 워드 (0x5678)인지 확인
        // verify second element is low word (0x5678)
        Assert.Equal((ushort)0x5678, result[1]);
    }

    /// <summary>
    ///     LowHigh 워드 순서로 int32를 ushort 배열로 패킹하는지 검증합니다.
    ///     verifies int32 packing to ushort array with LowHigh word order.
    /// </summary>
    [Fact]
    public void PackInt32_LowHigh_ReturnsLowWordFirst() {
        // 0x12345678 테스트 값 설정
        // set test value 0x12345678
        const int value = 0x12345678;

        // LowHigh 워드 순서로 패킹 실행
        // execute packing with LowHigh word order
        var result = Packing.PackInt32(value, WordOrder.LowHigh);

        // 결과 배열 길이가 2인지 확인
        // verify result array length is 2
        Assert.Equal(2, result.Length);
        // 첫 번째 요소가 하위 워드 (0x5678)인지 확인
        // verify first element is low word (0x5678)
        Assert.Equal((ushort)0x5678, result[0]);
        // 두 번째 요소가 상위 워드 (0x1234)인지 확인
        // verify second element is high word (0x1234)
        Assert.Equal((ushort)0x1234, result[1]);
    }

    #endregion

    #region PackFloat

    /// <summary>
    ///     HighLow 워드 순서로 float를 ushort 배열로 패킹하는지 검증합니다.
    ///     verifies float packing to ushort array with HighLow word order.
    /// </summary>
    [Fact]
    public void PackFloat_HighLow_ReturnsCorrectWords() {
        // 1.0f의 IEEE 754 비트 = 0x3F800000
        // IEEE 754 bits for 1.0f = 0x3F800000
        const float value = 1.0f;

        // HighLow 워드 순서로 패킹 실행
        // execute packing with HighLow word order
        var result = Packing.PackFloat(value);

        // 결과 배열 길이가 2인지 확인
        // verify result array length is 2
        Assert.Equal(2, result.Length);
        // 첫 번째 요소가 상위 워드 (0x3F80)인지 확인
        // verify first element is high word (0x3F80)
        Assert.Equal((ushort)0x3F80, result[0]);
        // 두 번째 요소가 하위 워드 (0x0000)인지 확인
        // verify second element is low word (0x0000)
        Assert.Equal((ushort)0x0000, result[1]);
    }

    /// <summary>
    ///     LowHigh 워드 순서로 float를 ushort 배열로 패킹하는지 검증합니다.
    ///     verifies float packing to ushort array with LowHigh word order.
    /// </summary>
    [Fact]
    public void PackFloat_LowHigh_ReturnsSwappedWords() {
        // 1.0f의 IEEE 754 비트 = 0x3F800000
        // IEEE 754 bits for 1.0f = 0x3F800000
        const float value = 1.0f;

        // LowHigh 워드 순서로 패킹 실행
        // execute packing with LowHigh word order
        var result = Packing.PackFloat(value, WordOrder.LowHigh);

        // 첫 번째 요소가 하위 워드 (0x0000)인지 확인
        // verify first element is low word (0x0000)
        Assert.Equal((ushort)0x0000, result[0]);
        // 두 번째 요소가 상위 워드 (0x3F80)인지 확인
        // verify second element is high word (0x3F80)
        Assert.Equal((ushort)0x3F80, result[1]);
    }

    #endregion

    #region PackString

    /// <summary>
    ///     짝수 길이 문자열이 올바르게 패킹되는지 검증합니다.
    ///     verifies even-length string is packed correctly.
    /// </summary>
    [Fact]
    public void PackString_EvenLength_ReturnsCorrectWords() {
        // 4문자 짝수 길이 문자열 설정
        // set 4-character even-length string
        const string text = "AB12";

        // 문자열 패킹 실행
        // execute string packing
        var result = Packing.PackString(text);

        // 결과 배열 길이가 2인지 확인 (4문자 / 2 = 2워드)
        // verify result array length is 2 (4 chars / 2 = 2 words)
        Assert.Equal(2, result.Length);
        // 첫 번째 워드가 'A','B' 조합인지 확인
        // verify first word is 'A','B' combination
        Assert.Equal((ushort)(('A' << 8) | 'B'), result[0]);
        // 두 번째 워드가 '1','2' 조합인지 확인
        // verify second word is '1','2' combination
        Assert.Equal((ushort)(('1' << 8) | '2'), result[1]);
    }

    /// <summary>
    ///     홀수 길이 문자열이 마지막 문자를 상위 바이트에 패킹하고 하위를 0으로 패딩하는지 검증합니다.
    ///     verifies odd-length string packs last char in high byte with zero-padded low byte.
    /// </summary>
    [Fact]
    public void PackString_OddLength_ZeroPadsLastWord() {
        // 3문자 홀수 길이 문자열 설정
        // set 3-character odd-length string
        const string text = "ABC";

        // 문자열 패킹 실행
        // execute string packing
        var result = Packing.PackString(text);

        // 결과 배열 길이가 2인지 확인 (1쌍 + 1나머지)
        // verify result array length is 2 (1 pair + 1 remainder)
        Assert.Equal(2, result.Length);
        // 첫 번째 워드가 'A','B' 조합인지 확인
        // verify first word is 'A','B' combination
        Assert.Equal((ushort)(('A' << 8) | 'B'), result[0]);
        // 두 번째 워드가 'C' + zero-pad인지 확인
        // verify second word is 'C' + zero-pad
        Assert.Equal((ushort)('C' << 8), result[1]);
    }

    /// <summary>
    ///     null 또는 빈 문자열에 대해 빈 배열을 반환하는지 검증합니다.
    ///     verifies null or empty string returns empty array.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PackString_NullOrEmpty_ReturnsEmptyArray(string? text) {
        // null 또는 빈 문자열의 패킹 실행
        // execute packing of null or empty string
        var result = Packing.PackString(text!);

        // 빈 배열이 반환되는지 확인
        // verify empty array is returned
        Assert.Empty(result);
    }

    #endregion

    #region PackDigitPairs

    /// <summary>
    ///     숫자 문자열을 두 자리씩 묶어 패킹하는지 검증합니다.
    ///     verifies digit string is packed by pairing consecutive digits.
    /// </summary>
    [Fact]
    public void PackDigitPairs_ValidDigits_ReturnsPairedValues() {
        // 4자리 숫자 문자열 설정 ("1234" → [12, 34])
        // set 4-digit string ("1234" -> [12, 34])
        const string text = "1234";

        // 숫자쌍 패킹 실행
        // execute digit pair packing
        var result = Packing.PackDigitPairs(text);

        // 결과 배열 길이가 2인지 확인
        // verify result array length is 2
        Assert.Equal(2, result.Length);
        // 첫 번째 요소가 12인지 확인
        // verify first element is 12
        Assert.Equal((ushort)12, result[0]);
        // 두 번째 요소가 34인지 확인
        // verify second element is 34
        Assert.Equal((ushort)34, result[1]);
    }

    /// <summary>
    ///     빈 문자열에 대해 빈 배열을 반환하는지 검증합니다.
    ///     verifies empty string returns empty array.
    /// </summary>
    [Fact]
    public void PackDigitPairs_EmptyString_ReturnsEmptyArray() {
        // 빈 문자열의 패킹 실행
        // execute packing of empty string
        var result = Packing.PackDigitPairs("");

        // 빈 배열이 반환되는지 확인
        // verify empty array is returned
        Assert.Empty(result);
    }

    #endregion

    #region PackAddress

    /// <summary>
    ///     점 구분 IP 주소를 ushort 배열로 패킹하는지 검증합니다.
    ///     verifies dot-delimited IP address is packed to ushort array.
    /// </summary>
    [Fact]
    public void PackAddress_ValidIp_ReturnsFourOctets() {
        // 점 구분 IP 주소 설정
        // set dot-delimited IP address
        const string addr = "192.168.1.1";

        // IP 주소 패킹 실행
        // execute IP address packing
        var result = Packing.PackAddress(addr);

        // 결과 배열 길이가 4인지 확인
        // verify result array length is 4
        Assert.Equal(4, result.Length);
        // 첫 번째 옥텟이 192인지 확인
        // verify first octet is 192
        Assert.Equal((ushort)192, result[0]);
        // 두 번째 옥텟이 168인지 확인
        // verify second octet is 168
        Assert.Equal((ushort)168, result[1]);
        // 세 번째 옥텟이 1인지 확인
        // verify third octet is 1
        Assert.Equal((ushort)1, result[2]);
        // 네 번째 옥텟이 1인지 확인
        // verify fourth octet is 1
        Assert.Equal((ushort)1, result[3]);
    }

    /// <summary>
    ///     서브넷 마스크 주소를 올바르게 패킹하는지 검증합니다.
    ///     verifies subnet mask address is packed correctly.
    /// </summary>
    [Fact]
    public void PackAddress_SubnetMask_ReturnsCorrectOctets() {
        // 서브넷 마스크 주소 설정
        // set subnet mask address
        const string addr = "255.255.255.0";

        // 서브넷 마스크 패킹 실행
        // execute subnet mask packing
        var result = Packing.PackAddress(addr);

        // 첫 번째 옥텟이 255인지 확인
        // verify first octet is 255
        Assert.Equal((ushort)255, result[0]);
        // 두 번째 옥텟이 255인지 확인
        // verify second octet is 255
        Assert.Equal((ushort)255, result[1]);
        // 세 번째 옥텟이 255인지 확인
        // verify third octet is 255
        Assert.Equal((ushort)255, result[2]);
        // 네 번째 옥텟이 0인지 확인
        // verify fourth octet is 0
        Assert.Equal((ushort)0, result[3]);
    }

    #endregion
}