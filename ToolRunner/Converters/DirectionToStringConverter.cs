using System.Globalization;
using System.Windows.Data;
using ToolRunner.Models;

namespace ToolRunner.Converters;

/// <summary>
///     converts a <see cref="RunDirection" /> into its display caption, so the selector shows
///     "CW (fastening)" instead of the raw enum name "Cw".
///     회전 방향 → 표시 문자열 변환
/// </summary>
public sealed class DirectionToStringConverter : IValueConverter {
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // map the direction onto its caption, falling back to a placeholder for anything unexpected
        return value is RunDirection direction
            ? direction switch {
                // clockwise turns the fastener in
                RunDirection.Cw => "CW (fastening)",
                // counter-clockwise backs the fastener out
                RunDirection.Ccw => "CCW (loosening)",
                // unknown value — show a placeholder
                _ => "---"
            }
            : "---";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // one-way binding only — the selected item is bound as the enum itself
        throw new NotSupportedException();
    }
}
