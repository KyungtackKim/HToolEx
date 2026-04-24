using System.Runtime.CompilerServices;
using System.Text;

namespace HTool.Core.Util;

/// <summary>
///     바이트와 문자열 간 포맷/디코딩 유틸리티.
///     formatting and decoding utility between bytes and strings.
/// </summary>
/// <remarks>
///     16진수 덤프, ASCII 디코딩 등 프로토콜 로깅이나 UI 표시에 필요한 문자열 변환을 제공합니다.
///     provides string conversions needed for protocol logging or UI display, such as hex dump and ASCII decoding.
/// </remarks>
public static class TextFormat {
    /// <summary>
    ///     바이트 스팬을 16진수 문자열로 변환합니다.
    ///     convert byte span to hex string.
    /// </summary>
    /// <param name="values">바이트 스팬 / byte span</param>
    /// <param name="separator">구분자 (기본값: 공백) / separator (default: space)</param>
    /// <param name="lineBreakAt">줄바꿈 위치 (0 = 줄바꿈 없음) / line break position (0 = no break)</param>
    /// <returns>16진수 문자열 / hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHex(ReadOnlySpan<byte> values, string separator = " ", int lineBreakAt = 16) {
        // 빈 입력 확인
        // check for empty input
        if (values.IsEmpty)
            // 포맷할 바이트가 없어 빈 문자열 반환
            // return empty string since no bytes to format
            return string.Empty;

        // 용량을 추정한 StringBuilder 생성
        // create StringBuilder with estimated capacity
        var sb = new StringBuilder(values.Length * 3);
        // 각 바이트를 순회
        // iterate through each byte
        for (var i = 0; i < values.Length; i++) {
            // 두 자리 16진수 표현 추가
            // append two-digit hex representation
            sb.Append(values[i].ToString("X2"));
            // 구분자 추가 여부 확인
            // check whether to append separator
            if (i < values.Length - 1)
                // 바이트 사이에 구분자 추가
                // append separator between bytes
                sb.Append(separator);
            // 줄바꿈 위치인지 확인
            // check if line break is needed
            if (lineBreakAt > 0 && (i + 1) % lineBreakAt == 0)
                // 줄바꿈 추가
                // append line break
                sb.AppendLine();
        }

        // 포맷된 16진수 문자열 반환
        // return formatted hex string
        return sb.ToString();
    }

    /// <summary>
    ///     바이트 스팬을 ASCII 문자열로 변환합니다. 후행 null 문자는 제거됩니다.
    ///     decode byte span to ASCII string, trimming trailing null characters.
    /// </summary>
    /// <param name="span">바이트 스팬 / byte span</param>
    /// <returns>문자열 (전체 null 시 빈 문자열) / string (empty when all bytes are null)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DecodeAscii(ReadOnlySpan<byte> span) {
        // 초기 길이 가져오기
        // get initial length
        var end = span.Length;
        // 후행 null 바이트 제거
        // trim trailing null bytes
        while (end > 0 && span[end - 1] is 0)
            // 끝 위치 감소
            // decrement end position
            end--;
        // 디코딩된 문자열 반환, 전체가 null이면 빈 문자열
        // return decoded string, or empty if all bytes were null
        return end is not 0 ? Encoding.ASCII.GetString(span[..end]) : string.Empty;
    }
}
