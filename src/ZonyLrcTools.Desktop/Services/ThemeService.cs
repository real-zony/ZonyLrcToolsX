using Avalonia;
using Avalonia.Styling;

namespace ZonyLrcTools.Desktop.Services;

public class ThemeService : IThemeService
{
    public ThemeVariant CurrentTheme => Application.Current?.ActualThemeVariant ?? ThemeVariant.Default;

    public event EventHandler<ThemeVariant>? ThemeChanged;

    public void SetTheme(ThemeVariant theme)
    {
        if (Application.Current == null) return;

        Application.Current.RequestedThemeVariant = theme;
        ThemeChanged?.Invoke(this, theme);
    }

    public void ToggleTheme()
    {
        var newTheme = CurrentTheme == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;

        SetTheme(newTheme);
    }
}
