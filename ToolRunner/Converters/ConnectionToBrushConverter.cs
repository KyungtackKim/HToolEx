using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HTool.Type;

namespace ToolRunner.Converters;

/// <summary>
///     converts a <see cref="Connection" /> state into the status-bar indicator colour.
///     연결 상태 → 표시등 색상 변환
/// </summary>
public sealed class ConnectionToBrushConverter : IValueConverter {
    // connected — green
    private static readonly SolidColorBrush Connected = new(Colors.LimeGreen);

    // connecting — orange
    private static readonly SolidColorBrush Connecting = new(Colors.Orange);

    // closed or unknown — gray
    private static readonly SolidColorBrush Disconnected = new(Colors.Gray);

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // map the state onto a brush, falling back to gray for anything unexpected
        return value is Connection state
            ? state switch {
                // connected — green
                Connection.Connected => Connected,
                // attempt in flight — orange
                Connection.Connecting => Connecting,
                // closing or closed — gray
                _ => Disconnected
            }
            : Disconnected;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // one-way binding only — a brush carries no connection state back
        throw new NotSupportedException();
    }
}
