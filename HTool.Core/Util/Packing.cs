using System.Runtime.CompilerServices;
using HTool.Core.Type.Process;

namespace HTool.Core.Util;

/// <summary>
///     MODBUS 레지스터 패킹 유틸리티. 값을 ushort 배열로 변환합니다.
///     MODBUS register packing utility. converts values into ushort arrays.
/// </summary>
/// <remarks>
///     MODBUS 레지스터는 16비트 워드 단위이므로, 32비트 값이나 문자열을 레지스터 배열에 맞게 분해·결합하는 헬퍼를 제공합니다.
///     MODBUS registers are 16-bit word sized, so these helpers split or combine 32-bit values and strings to fit register
///     arrays.
/// </remarks>
public static class Packing {
    /// <summary>
    ///     int32 값을 ushort 배열로 변환합니다. MODBUS 레지스터 패킹용.
    ///     convert int32 value to ushort array for MODBUS register packing.
    /// </summary>
    /// <param name="value">int 값 / int value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>값 배열 (2워드) / value array (2 words)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackInt32(int value, WordOrder wordOrder = WordOrder.HighLow) {
        // 상위 워드 추출
        // extract high word
        var highWord = (ushort)(value >> 16);
        // 하위 워드 추출
        // extract low word
        var lowWord = (ushort)value;
        // 워드 순서에 따라 배열 반환
        // return array according to word order
        return wordOrder == WordOrder.HighLow
            ? [highWord, lowWord]
            : [lowWord, highWord];
    }

    /// <summary>
    ///     float 값을 ushort 배열로 변환합니다. MODBUS 레지스터 패킹용.
    ///     convert float value to ushort array for MODBUS register packing.
    /// </summary>
    /// <param name="value">float 값 / float value</param>
    /// <param name="wordOrder">32비트 값의 워드 순서 (기본값: HighLow/ABCD) / word order for 32-bit value (default: HighLow/ABCD)</param>
    /// <returns>값 배열 (2워드) / value array (2 words)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackFloat(float value, WordOrder wordOrder = WordOrder.HighLow) {
        // float을 int 비트로 재해석
        // reinterpret float as int bits
        var bits = BitConverter.SingleToInt32Bits(value);
        // 상위 워드 추출
        // extract high word
        var highWord = (ushort)(bits >> 16);
        // 하위 워드 추출
        // extract low word
        var lowWord = (ushort)bits;
        // 워드 순서에 따라 배열 반환
        // return array according to word order
        return wordOrder == WordOrder.HighLow
            ? [highWord, lowWord]
            : [lowWord, highWord];
    }

    /// <summary>
    ///     문자열을 워드 단위 ushort 배열로 변환합니다 (문자 2개 = 1워드).
    ///     convert string to word-sized ushort array (2 chars per word).
    /// </summary>
    /// <param name="text">텍스트 / text</param>
    /// <returns>값 배열 (빈 문자열은 빈 배열) / value array (empty array for empty string)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackString(string text) {
        // null 또는 빈 문자열 확인
        // check null or empty text
        if (string.IsNullOrEmpty(text))
            // 패킹할 내용이 없어 빈 배열 반환
            // return empty array since nothing to pack
            return [];

        // 텍스트를 스팬으로 가져오기
        // get text as span
        var span = text.AsSpan();
        // 완전한 워드 쌍의 수
        // number of full word pairs
        var length = span.Length / 2;
        // 남은 단일 문자 수(0 또는 1)
        // remaining single character count (0 or 1)
        var remain = span.Length % 2;
        // 잔여분을 포함한 결과 배열 생성
        // create result array with room for remainder
        var result = new ushort[length + remain];
        // 문자 쌍을 순회하며 패킹
        // iterate through character pairs and pack
        for (var i = 0; i < length; i++)
            // 두 문자를 하나의 ushort로 패킹
            // pack two characters into one ushort
            result[i] = (ushort)((span[i * 2] << 8) | span[i * 2 + 1]);
        // 잔여 단일 문자 존재 시 처리
        // handle remaining single character if present
        if (remain > 0)
            // 마지막 문자를 상위 바이트에 패킹
            // pack last character into high byte
            result[length] = (ushort)(span[^1] << 8);
        // 패킹된 워드 배열 반환
        // return packed word array
        return result;
    }

    /// <summary>
    ///     숫자 문자열을 두 자리씩 묶어 ushort 배열로 변환합니다.
    ///     convert digit string to ushort array by pairing consecutive digits.
    /// </summary>
    /// <param name="text">숫자 텍스트 / digit text</param>
    /// <returns>값 배열 (빈 문자열은 빈 배열) / value array (empty array for empty string)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackDigitPairs(string text) {
        // null 또는 빈 문자열 확인
        // check null or empty text
        if (string.IsNullOrEmpty(text))
            // 패킹할 내용이 없어 빈 배열 반환
            // return empty array since nothing to pack
            return [];

        // 숫자 쌍의 수(text.Length / 2)
        // number of digit pairs (text.Length / 2)
        var len = text.Length >> 1;
        // 결과 배열 생성
        // create result array
        var result = new ushort[len];
        // 텍스트를 스팬으로 가져오기
        // get text as span
        var span = text.AsSpan();
        // 숫자 쌍을 순회하며 십진수로 결합
        // iterate through digit pairs and combine as decimal
        for (int i = 0, j = 0; i < len; i++, j += 2) {
            // 첫째 자리 숫자
            // first digit of the pair
            var d1 = span[j] - '0';
            // 둘째 자리 숫자
            // second digit of the pair
            var d2 = span[j + 1] - '0';
            // 두 자리를 십진 값으로 결합
            // combine two digits into decimal value
            result[i] = (ushort)(d1 * 10 + d2);
        }

        // 숫자 쌍 배열 반환
        // return digit pair array
        return result;
    }

    /// <summary>
    ///     네트워크 주소 문자열을 ushort 배열로 변환합니다 (점 구분).
    ///     convert dot-delimited network address string to ushort array.
    /// </summary>
    /// <param name="addr">주소 (예: "192.168.0.1") / address (e.g. "192.168.0.1")</param>
    /// <returns>옥텟 배열 / octet array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] PackAddress(string addr) {
        // 파싱된 옥텟 카운터
        // parsed octet counter
        var count = 0;
        // 현재 옥텟 누적값
        // current octet accumulator
        var acc = 0;
        // 주소 문자열을 스팬으로 가져오기
        // get address string as span
        var span = addr.AsSpan();
        // 파싱된 옥텟을 위한 스택 공간 할당
        // allocate stack space for parsed octets
        Span<ushort> values = stackalloc ushort[8];
        // 문자열 끝까지 순회 (i == span.Length 포함)
        // iterate through entire string (including i == span.Length)
        for (var i = 0; i <= span.Length; i++)
            // 문자열 끝이거나 점 구분자인지 확인
            // check for end of string or dot separator
            if (i == span.Length || span[i] == '.') {
                // 누적값을 옥텟 배열에 저장
                // store accumulated value as octet
                values[count++] = (ushort)acc;
                // 다음 옥텟을 위해 누적값 초기화
                // reset accumulator for next octet
                acc = 0;
            } else {
                // 자릿수를 현재 옥텟에 누적
                // accumulate digit into current octet
                acc = acc * 10 + (span[i] - '0');
            }

        // 정확한 길이로 결과 배열 생성
        // create result array with exact count
        var result = new ushort[count];
        // 파싱된 값을 결과 배열에 복사
        // copy parsed values to result
        values[..count].CopyTo(result);
        // 파싱된 주소 배열 반환
        // return parsed address array
        return result;
    }
}