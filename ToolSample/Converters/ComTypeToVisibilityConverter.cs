using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HTool.Type;

namespace ToolSample.Converters;

/// <summary>
///     ComType을 Visibility로 변환한다. ComType 매칭 시 Visible, 아닐 시 Collapsed.
///     Converts ComType to Visibility. Visible when ComType matches, Collapsed otherwise.
/// </summary>
public sealed class ComTypeToVisibilityConverter : IValueConverter {
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // 파라미터와 ComType 비교
        // compare parameter with ComType
        if (value is ComType comType && parameter is string paramStr && Enum.TryParse<ComType>(paramStr, out var target))
            // 일치 시 Visible, 불일치 시 Collapsed 반환
            // return Visible on match, Collapsed on mismatch
            return comType == target ? Visibility.Visible : Visibility.Collapsed;

        // 기본값 — Collapsed 반환
        // default — return Collapsed
        return Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        throw new NotSupportedException();
    }
}