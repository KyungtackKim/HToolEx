using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HTool.Type;

namespace ToolSample.Converters;

/// <summary>
///     Connection 상태를 Brush로 변환한다. StatusBar 표시등 색상용.
///     Converts Connection state to Brush. For StatusBar indicator color.
/// </summary>
public sealed class ConnectionToBrushConverter : IValueConverter {
    // 연결됨 색상 (녹색)
    // connected color (green)
    private static readonly SolidColorBrush Connected = new(Colors.LimeGreen);

    // 연결 중 색상 (주황)
    // connecting color (orange)
    private static readonly SolidColorBrush Connecting = new(Colors.Orange);

    // 해제됨 색상 (회색)
    // disconnected color (gray)
    private static readonly SolidColorBrush Disconnected = new(Colors.Gray);

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // Connection 상태에 따라 색상 반환
        // return color based on Connection state
        return value is Connection state
            ? state switch {
                // 연결됨 — 녹색
                // connected — green
                Connection.Connected => Connected,
                // 연결 중 — 주황
                // connecting — orange
                Connection.Connecting => Connecting,
                // 기본값 — 회색
                // default — gray
                _ => Disconnected
            }
            : Disconnected;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        throw new NotSupportedException();
    }
}