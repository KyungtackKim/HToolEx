using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ToolSample.Converters;

/// <summary>
///     bool을 Visibility로 변환한다. true → Visible, false → Collapsed.
///     Converts bool to Visibility. true → Visible, false → Collapsed.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter {
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        return value is Visibility.Visible;
    }
}