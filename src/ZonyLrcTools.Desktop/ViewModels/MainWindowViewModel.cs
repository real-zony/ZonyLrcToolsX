using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly IUILocalizationService _localization;
    private readonly INavigationService _navigation;

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
    public string NavDecrypt => _localization["Nav_Decrypt"];
    public string NavSettings => _localization["Nav_Settings"];
    public string HomeDescription => _localization["Home_Description"];

    public string ThemeButtonText => IsDarkTheme
        ? _localization["Settings_ThemeLight"]
        : _localization["Settings_ThemeDark"];

    public MainWindowViewModel(
        IThemeService themeService,
        IUILocalizationService localization,
        INavigationService navigation)
    {
        _themeService = themeService;
        _localization = localization;
        _navigation = navigation;

        // Subscribe to language changes
        _localization.LanguageChanged += OnLanguageChanged;

        // The navigation service is the single owner of "which page is showing".
        _navigation.NavigationChanged += OnNavigationChanged;

        NavigateToIndex(0);
    }

    // Guards against the re-entrant loop "index changed -> navigate -> navigation changed -> index changed".
    private bool _isSyncingNavigation;

    partial void OnSelectedNavigationIndexChanged(int value)
    {
        if (_isSyncingNavigation)
        {
            return;
        }

        NavigateToIndex(value);
    }

    private void NavigateToIndex(int index)
    {
        // NavigateRoot keeps the stack depth at one: sidebar switching is not a back-navigable
        // action, so clicking around must not grow history.
        CurrentPage = index switch
        {
            1 => _navigation.NavigateRoot<LyricsDownloadViewModel>(),
            2 => _navigation.NavigateRoot<AlbumDownloadViewModel>(),
            3 => _navigation.NavigateRoot<DecryptViewModel>(),
            4 => _navigation.NavigateRoot<SettingsViewModel>(),
            _ => _navigation.NavigateRoot<HomeViewModel>()
        };
    }

    private void OnNavigationChanged(object? sender, ViewModelBase viewModel)
    {
        CurrentPage = viewModel;

        // The home page quick actions navigate directly, so the sidebar has to follow them.
        var index = IndexOf(viewModel);
        if (index < 0 || index == SelectedNavigationIndex)
        {
            return;
        }

        _isSyncingNavigation = true;
        try
        {
            SelectedNavigationIndex = index;
        }
        finally
        {
            _isSyncingNavigation = false;
        }
    }

    private static int IndexOf(ViewModelBase viewModel) => viewModel switch
    {
        LyricsDownloadViewModel => 1,
        AlbumDownloadViewModel => 2,
        DecryptViewModel => 3,
        SettingsViewModel => 4,
        HomeViewModel => 0,
        _ => -1
    };

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Notify all localized properties have changed
        OnPropertyChanged(nameof(NavHome));
        OnPropertyChanged(nameof(NavLyricsDownload));
        OnPropertyChanged(nameof(NavAlbumDownload));
        OnPropertyChanged(nameof(NavDecrypt));
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
