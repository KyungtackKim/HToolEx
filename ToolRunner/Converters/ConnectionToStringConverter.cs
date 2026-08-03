using System.Globalization;
using System.Windows.Data;
using HTool.Type;

namespace ToolRunner.Converters;

/// <summary>
///     converts a <see cref="Connection" /> state into the status-bar caption.
///     연결 상태 → 상태바 문자열 변환
/// </summary>
public sealed class ConnectionToStringConverter : IValueConverter {
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // map the state onto a caption, falling back to a neutral text for anything unexpected
        return value is Connection state
            ? state switch {
                // link established
                Connection.Connected => "Connected",
                // attempt in flight
                Connection.Connecting => "Connecting",
                // shutting the socket down
                Connection.Close => "Closing",
                // fully closed
                _ => "Disconnected"
            }
            : "Disconnected";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // one-way binding only — a caption carries no connection state back
        throw new NotSupportedException();
    }
}
