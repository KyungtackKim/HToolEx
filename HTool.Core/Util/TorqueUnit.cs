using System.Runtime.CompilerServices;
using HTool.Core.Type.Process;

namespace HTool.Core.Util;

/// <summary>
///     토크 단위 변환·포맷·파싱 유틸리티.
///     torque unit conversion, formatting, and parsing utility.
/// </summary>
/// <remarks>
///     HANTAS 제품군에서 사용하는 토크 단위(kgf.cm, N.m, lbf.in 등) 간 변환과 UI 표시·입력 파싱을 제공합니다.
///     provides conversion between torque units used by HANTAS products (kgf.cm, N.m, lbf.in, etc.) and UI
///     display/parsing.
/// </remarks>
public static class TorqueUnit {
    /// <summary>
    ///     토크 단위를 변환합니다.
    ///     convert torque unit.
    /// </summary>
    /// <param name="value">토크 값 / torque value</param>
    /// <param name="src">원본 단위 / source unit</param>
    /// <param name="dst">대상 단위 / destination unit</param>
    /// <returns>변환된 토크 / converted torque</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Convert(float value, Unit src, Unit dst) {
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
    ///     정수형 단위 코드를 표시 문자열로 변환합니다.
    ///     convert integer unit code to display string.
    /// </summary>
    /// <param name="unitCode">정수형 단위 코드 (0=kgf.cm, 1=kgf.m, ...) / unit code as integer (0=kgf.cm, 1=kgf.m, ...)</param>
    /// <returns>단위 문자열 / unit string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(int unitCode) {
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
    ///     문자열을 토크 단위 enum으로 파싱합니다.
    ///     parse string to torque unit enum.
    /// </summary>
    /// <param name="text">단위 문자열 / unit string</param>
    /// <returns>단위 (미지정 시 kgf.cm) / unit (kgf.cm when unspecified)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Parse(string text) {
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
}