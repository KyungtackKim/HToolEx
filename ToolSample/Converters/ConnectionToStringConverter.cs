using System.Globalization;
using System.Windows.Data;
using HTool.Type;

namespace ToolSample.Converters;

/// <summary>
///     Connection 상태를 문자열로 변환한다. StatusBar 텍스트 표시용.
///     Converts Connection state to string. For StatusBar text display.
/// </summary>
public sealed class ConnectionToStringConverter : IValueConverter {
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        // Connection 상태에 따라 문자열 반환
        // return string based on Connection state
        return value is Connection state
            ? state switch {
                // 연결됨
                // connected
                Connection.Connected => "Connected",
                // 연결 중
                // connecting
                Connection.Connecting => "Connecting...",
                // 연결 해제 중
                // closing
                Connection.Close => "Closing...",
                // 기본값 — 연결 해제됨
                // default — disconnected
                _ => "Disconnected"
            }
            : "Disconnected";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture) {
        throw new NotSupportedException();
    }
}