using System.Runtime.CompilerServices;
using System.Text;
using HTool.Core.Type.Process;

namespace HTool.Core.Util;

/// <summary>
///     잡다한 변환/포맷 유틸리티. 토크 단위 변환, 16진수 포맷, ASCII 디코딩 등을 제공합니다.
///     miscellaneous conversion/formatting utilities. provides torque unit conversion, hex formatting, ASCII decoding, etc.
/// </summary>
/// <remarks>
///     CRC 계산은 <see cref="Crc16" />, 엔디안 변환은 <see cref="ByteOrder" />, 레지스터 패킹은 <see cref="Packing" />에 있습니다.
///     CRC calculation lives in <see cref="Crc16" />, byte order conversion in <see cref="ByteOrder" />, register packing in
///     <see cref="Packing" />.
/// </remarks>
public static class Utils {
    /// <summary>
    ///     토크 단위를 변환합니다.
    ///     convert torque unit.
    /// </summary>
    /// <param name="value">토크 값 / torque value</param>
    /// <param name="src">원본 단위 / source unit</param>
    /// <param name="dst">대상 단위 / destination unit</param>
    /// <returns>변환된 토크 / converted torque</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertTorque(float value, Unit src, Unit dst) {
        // N.cm per N.m 변환 계수
        // N.cm per N.m conversion factor
        const float nCm = 100.0f;
        // kgf.m per N.m 변환 계수
        // kgf.m per N.m conversion factor
        const float kgfM = 0.101971621f;
        // kgf.cm per N.m 변환 계수
        // kgf.cm per N.m conversion factor
        const float kgfCm = 10.1971621f;
        // lbf.in per N.m 변환 계수
        // lbf.in per N.m conversion factor
        const float lbfIn = 8.85074579f;
        // lbf.ft per N.m 변환 계수
        // lbf.ft per N.m conversion factor
        const float lbfFt = 0.737562149f;
        // ozf.in per N.m 변환 계수
        // ozf.in per N.m conversion factor
        const float ozfIn = 141.611932f;
        // 원본 단위를 N.m으로 변환
        // convert source unit to N.m
        var valueInNm = src switch {
            Unit.KgfCm => value / kgfCm,
            Unit.KgfM  => value / kgfM,
            Unit.Nm    => value,
            Unit.NCm   => value / nCm,
            Unit.LbfIn => value / lbfIn,
            Unit.OzfIn => value / ozfIn,
            Unit.LbfFt => value / lbfFt,
            _          => value
        };
        // N.m을 대상 단위로 변환하여 반환
        // convert N.m to destination unit and return
        return dst switch {
            Unit.KgfCm => valueInNm * kgfCm,
            Unit.KgfM  => valueInNm * kgfM,
            Unit.Nm    => valueInNm,
            Unit.NCm   => valueInNm * nCm,
            Unit.LbfIn => valueInNm * lbfIn,
            Unit.OzfIn => valueInNm * ozfIn,
            Unit.LbfFt => valueInNm * lbfFt,
            _          => valueInNm
        };
    }

    /// <summary>
    ///     바이트 스팬을 16진수 문자열로 변환합니다.
    ///     convert byte span to hex string.
    /// </summary>
    /// <param name="values">바이트 스팬 / byte span</param>
    /// <param name="separator">구분자 (기본값: 공백) / separator (default: space)</param>
    /// <param name="lineBreakAt">줄바꿈 위치 (0 = 줄바꿈 없음) / line break position (0 = no break)</param>
    /// <returns>16진수 문자열 / hex string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatHex(ReadOnlySpan<byte> values, string separator = " ", int lineBreakAt = 16) {
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
    ///     정수형 단위 코드를 문자열로 변환합니다.
    ///     convert integer unit code to string representation.
    /// </summary>
    /// <param name="unitCode">정수형 단위 코드 (0=kgf.cm, 1=kgf.m, ...) / unit code as integer (0=kgf.cm, 1=kgf.m, ...)</param>
    /// <returns>단위 문자열 / unit string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatUnit(int unitCode) {
        // 단위 코드를 표시 문자열로 매핑하여 반환
        // map unit code to display string and return
        return unitCode switch {
            0 => "kgf.cm",
            1 => "kgf.m",
            2 => "N.m",
            3 => "N.cm",
            4 => "ozf.in",
            5 => "lbf.ft",
            6 => "ozf.ft",
            _ => "kgf.cm"
        };
    }

    /// <summary>
    ///     문자열을 토크 단위로 변환합니다.
    ///     parse string to torque unit.
    /// </summary>
    /// <param name="text">단위 문자열 / unit string</param>
    /// <returns>단위 (미지정 시 kgf.cm) / unit (kgf.cm when unspecified)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit ParseUnit(string text) {
        // null 또는 빈 문자열 확인
        // check for null or empty string
        if (string.IsNullOrEmpty(text))
            // 단위 미지정 시 기본값 kgf.cm 반환
            // return kgf.cm as default when no unit specified
            return Unit.KgfCm;
        // 문자열을 매칭되는 Unit enum으로 변환하여 반환
        // convert string to matching Unit enum and return
        return text.ToLowerInvariant() switch {
            "kgf.cm" => Unit.KgfCm,
            "kgf.m"  => Unit.KgfM,
            "n.m"    => Unit.Nm,
            "n.cm"   => Unit.NCm,
            "lbf.in" => Unit.LbfIn,
            "ozf.in" => Unit.OzfIn,
            "lbf.ft" => Unit.LbfFt,
            _        => Unit.KgfCm
        };
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
