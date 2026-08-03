using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ToolRunner.Converters;

/// <summary>
///     converts a boolean into <see cref="Visibility" />, collapsing (not hiding) on false so the
///     hidden element gives its layout space back.
///     bool → Visibility 변환 (false는 Collapsed로 공간까지 회수)
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter {
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // true shows the element, anything else collapses it
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // one-way binding only — visibility is driven by the view model flag
        throw new NotSupportedException();
    }
}
