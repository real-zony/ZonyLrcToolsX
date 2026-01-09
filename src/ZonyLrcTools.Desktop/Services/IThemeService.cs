using Avalonia.Styling;

namespace ZonyLrcTools.Desktop.Services;

public interface IThemeService
{
    ThemeVariant CurrentTheme { get; }
    event EventHandler<ThemeVariant>? ThemeChanged;

    void SetTheme(ThemeVariant theme);
    void ToggleTheme();
}
