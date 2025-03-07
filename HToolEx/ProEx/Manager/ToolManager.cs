using HToolEx.ProEx.Format;
using HToolEx.Util;

namespace HToolEx.ProEx.Manager;

/// <summary>
///     Tool manager in ParaMon-Pro X
/// </summary>
public static class ToolManager {
    private static ConcurrentQueueWithCheck<FormatToolInfo> MemberTools { get; } = new();
    private static ConcurrentQueueWithCheck<FormatToolInfo> ScanTools { get; } = new();

    /// <summary>
    ///     Selected tool id
    /// </summary>
    public static int SelectedTool { get; set; } = -1;

    /// <summary>
    ///     Get total member tools
    /// </summary>
    /// <returns>tools</returns>
    public static List<FormatToolInfo>? GetMemberTools() {
        return !MemberTools.GetItems(out var items) ? null : items;
    }

    /// <summary>
    ///     Get total scan tools
    /// </summary>
    /// <returns>tools</returns>
    public static List<FormatToolInfo>? GetScanTools() {
        return !ScanTools.GetItems(out var items) ? null : items;
    }

    /// <summary>
    ///     Clear total member tools
    /// </summary>
    public static void ClearMemberTools() {
        // clear
        MemberTools.Clear();
    }

    /// <summary>
    ///     Clear total scan tools
    /// </summary>
    public static void ClearScanTools() {
        // clear
        ScanTools.Clear();
    }

    /// <summary>
    ///     Add member tool
    /// </summary>
    /// <param name="tool">tool</param>
    public static void AddMemberTools(FormatToolInfo tool) {
        // add member tool
        MemberTools.Enqueue(tool);
    }

    /// <summary>
    ///     Add scan tool
    /// </summary>
    /// <param name="tool">tool</param>
    public static void AddScanTools(FormatToolInfo tool) {
        // add member tool
        ScanTools.Enqueue(tool);
    }
}