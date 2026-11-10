using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>
/// 首页与功能入口。
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IUILocalizationService _localization;

    /// <summary>
    /// 创建首页。
    /// </summary>
    public HomeViewModel(INavigationService navigation, IUILocalizationService localization)
    {
        _navigation = navigation;
        _localization = localization;
        _localization.LanguageChanged += OnLanguageChanged;
        _version = ResolveVersion();
    }

    [ObservableProperty]
    private string _version;

    /// <summary>欢迎语。</summary>
    public string WelcomeMessage => _localization["Home_Welcome"];
    /// <summary>应用说明。</summary>
    public string Description => _localization["Home_Description"];
    /// <summary>快捷操作标题。</summary>
    public string QuickActionsTitle => _localization["Home_QuickActions"];
    /// <summary>功能特性标题。</summary>
    public string FeaturesTitle => _localization["Home_Features"];
    /// <summary>支持格式说明。</summary>
    public string FeatureFormats => _localization["Home_FeatureFormats"];
    /// <summary>并行下载说明。</summary>
    public string FeatureParallel => _localization["Home_FeatureParallel"];
    /// <summary>编码支持说明。</summary>
    public string FeatureEncodings => _localization["Home_FeatureEncodings"];
    /// <summary>跨平台支持说明。</summary>
    public string FeatureCrossPlatform => _localization["Home_FeatureCrossPlatform"];
    /// <summary>版本标签。</summary>
    public string VersionLabel => _localization["Home_Version"];

    // 功能入口文案。
    /// <summary>歌词下载入口标题。</summary>
    public string LyricsCardTitle => _localization["Home_DownloadLyrics"];
    /// <summary>歌词下载入口说明。</summary>
    public string LyricsCardDescription => _localization["Home_DownloadLyricsHint"];
    /// <summary>封面下载入口标题。</summary>
    public string AlbumCardTitle => _localization["Home_DownloadAlbumCover"];
    /// <summary>封面下载入口说明。</summary>
    public string AlbumCardDescription => _localization["Home_DownloadAlbumCoverHint"];
    /// <summary>音乐解密入口标题。</summary>
    public string DecryptCardTitle => _localization["Home_Decrypt"];
    /// <summary>音乐解密入口说明。</summary>
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
            // 隐藏 SourceLink 添加的提交标识。
            var plusIndex = informational.IndexOf('+');
            return plusIndex > 0 ? informational[..plusIndex] : informational;
        }

        return assembly.GetName().Version?.ToString(3) ?? "4.0.3";
    }
}
