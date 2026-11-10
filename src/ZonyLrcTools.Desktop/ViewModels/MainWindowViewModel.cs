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
    /// <summary>音乐解密导航文案。</summary>
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
        IsDarkTheme = _themeService.CurrentTheme == Avalonia.Styling.ThemeVariant.Dark;
        _themeService.ThemeChanged += (_, _) =>
            IsDarkTheme = _themeService.CurrentTheme == Avalonia.Styling.ThemeVariant.Dark;

        // Subscribe to language changes
        _localization.LanguageChanged += OnLanguageChanged;

        // 页面切换统一由导航服务管理。
        _navigation.NavigationChanged += OnNavigationChanged;

        NavigateToIndex(0);
    }

    // 避免导航通知与侧栏选中状态互相触发。
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
        // 侧栏切换不累积返回记录。
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

        // 首页快捷入口跳转时同步侧栏选中项。
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
        _themeService.ToggleTheme();
    }
}
