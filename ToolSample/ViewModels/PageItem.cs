namespace ToolSample.ViewModels;

/// <summary>
///     사이드바 네비게이션 항목.
///     Sidebar navigation item.
/// </summary>
/// <param name="Name">표시 이름 / display name</param>
/// <param name="ViewModelType">대응 ViewModel 타입 / corresponding ViewModel type</param>
public record PageItem(string Name, Type ViewModelType);