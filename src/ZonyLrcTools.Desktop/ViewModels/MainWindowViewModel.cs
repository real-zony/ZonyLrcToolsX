using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly IUILocalizationService _localization;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private int _selectedNavigationIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeButtonText))]
    private bool _isDarkTheme;

    [ObservableProperty]
    private string _title = "ZonyLrcTools X";

    // Navigation localized strings
    public string NavHome => _localization["Nav_Home"];
    public string NavLyricsDownload => _localization["Nav_LyricsDownload"];
    public string NavAlbumDownload => _localization["Nav_AlbumDownload"];
    public string NavSettings => _localization["Nav_Settings"];
    public string HomeDescription => _localization["Home_Description"];

    public string ThemeButtonText => IsDarkTheme
        ? _localization["Settings_ThemeLight"]
        : _localization["Settings_ThemeDark"];

    public MainWindowViewModel(
        IThemeService themeService,
        IUILocalizationService localization,
        HomeViewModel homeViewModel)
    {
        _themeService = themeService;
        _localization = localization;

        // Subscribe to language changes
        _localization.LanguageChanged += OnLanguageChanged;

        // Set initial page
        CurrentPage = homeViewModel;
        SelectedNavigationIndex = 0;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Notify all localized properties have changed
        OnPropertyChanged(nameof(NavHome));
        OnPropertyChanged(nameof(NavLyricsDownload));
        OnPropertyChanged(nameof(NavAlbumDownload));
        OnPropertyChanged(nameof(NavSettings));
        OnPropertyChanged(nameof(HomeDescription));
        OnPropertyChanged(nameof(ThemeButtonText));
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _themeService.SetTheme(IsDarkTheme
            ? Avalonia.Styling.ThemeVariant.Dark
            : Avalonia.Styling.ThemeVariant.Light);
    }
}
