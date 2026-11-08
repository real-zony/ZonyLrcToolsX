using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>
/// The landing page. Its only job is to point people at the three things this app actually does,
/// so every quick-action button navigates for real instead of sitting there looking clickable.
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IUILocalizationService _localization;

    public HomeViewModel(INavigationService navigation, IUILocalizationService localization)
    {
        _navigation = navigation;
        _localization = localization;
        _localization.LanguageChanged += OnLanguageChanged;
        _version = ResolveVersion();
    }

    [ObservableProperty]
    private string _version;

    public string WelcomeMessage => _localization["Home_Welcome"];
    public string Description => _localization["Home_Description"];
    public string QuickActionsTitle => _localization["Home_QuickActions"];
    public string FeaturesTitle => _localization["Home_Features"];
    public string FeatureFormats => _localization["Home_FeatureFormats"];
    public string FeatureParallel => _localization["Home_FeatureParallel"];
    public string FeatureEncodings => _localization["Home_FeatureEncodings"];
    public string FeatureCrossPlatform => _localization["Home_FeatureCrossPlatform"];
    public string VersionLabel => _localization["Home_Version"];

    // Card copy: title + one-line explanation, both localized.
    public string LyricsCardTitle => _localization["Home_DownloadLyrics"];
    public string LyricsCardDescription => _localization["Home_DownloadLyricsHint"];
    public string AlbumCardTitle => _localization["Home_DownloadAlbumCover"];
    public string AlbumCardDescription => _localization["Home_DownloadAlbumCoverHint"];
    public string DecryptCardTitle => _localization["Home_Decrypt"];
    public string DecryptCardDescription => _localization["Home_DecryptHint"];

    [RelayCommand]
    private void OpenLyrics() => _navigation.NavigateRoot<LyricsDownloadViewModel>();

    [RelayCommand]
    private void OpenAlbum() => _navigation.NavigateRoot<AlbumDownloadViewModel>();

    [RelayCommand]
    private void OpenDecrypt() => _navigation.NavigateRoot<DecryptViewModel>();

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(WelcomeMessage));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(QuickActionsTitle));
        OnPropertyChanged(nameof(FeaturesTitle));
        OnPropertyChanged(nameof(FeatureFormats));
        OnPropertyChanged(nameof(FeatureParallel));
        OnPropertyChanged(nameof(FeatureEncodings));
        OnPropertyChanged(nameof(FeatureCrossPlatform));
        OnPropertyChanged(nameof(VersionLabel));
        OnPropertyChanged(nameof(LyricsCardTitle));
        OnPropertyChanged(nameof(LyricsCardDescription));
        OnPropertyChanged(nameof(AlbumCardTitle));
        OnPropertyChanged(nameof(AlbumCardDescription));
        OnPropertyChanged(nameof(DecryptCardTitle));
        OnPropertyChanged(nameof(DecryptCardDescription));
    }

    private static string ResolveVersion()
    {
        var assembly = typeof(HomeViewModel).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            // Strip the "+<commit sha>" suffix that SourceLink appends.
            var plusIndex = informational.IndexOf('+');
            return plusIndex > 0 ? informational[..plusIndex] : informational;
        }

        return assembly.GetName().Version?.ToString(3) ?? "4.0.3";
    }
}
