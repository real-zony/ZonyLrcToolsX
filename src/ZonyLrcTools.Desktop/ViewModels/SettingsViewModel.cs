using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Localization;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly GlobalOptions _globalOptions;
    private readonly IUILocalizationService? _localizationService;
    private bool _isLoading = true; // Prevent saving during initial load

    [ObservableProperty]
    private bool _followSystemLanguage = true;

    [ObservableProperty]
    private bool _isChineseSelected = true;

    [ObservableProperty]
    private bool _isEnglishSelected;

    [ObservableProperty]
    private bool _isProxyEnabled;

    [ObservableProperty]
    private string _proxyIp = "127.0.0.1";

    [ObservableProperty]
    private int _proxyPort = 7890;

    [ObservableProperty]
    private bool _isTranslationEnabled;

    [ObservableProperty]
    private bool _isOneLineMode;

    [ObservableProperty]
    private bool _skipExistingLyrics;

    [ObservableProperty]
    private string _selectedEncoding = "utf-8";

    // Localized strings
    public string SettingsTitle => _localizationService?["Settings_Title"] ?? "Settings";
    public string SettingsLanguage => _localizationService?["Settings_Language"] ?? "Language";
    public string SettingsFollowSystem => _localizationService?["Settings_FollowSystem"] ?? "Follow System";
    public string SettingsChinese => _localizationService?["Settings_Chinese"] ?? "Chinese";
    public string SettingsEnglish => _localizationService?["Settings_English"] ?? "English";
    public string SettingsProxy => _localizationService?["Settings_Proxy"] ?? "Proxy";
    public string SettingsProxyEnable => _localizationService?["Settings_ProxyEnable"] ?? "Enable Proxy";
    public string SettingsProxyAddress => _localizationService?["Settings_ProxyAddress"] ?? "Proxy Address";
    public string SettingsLyricsProvider => _localizationService?["Settings_LyricsProvider"] ?? "Lyrics Provider";
    public string SettingsReset => _localizationService?["Settings_Reset"] ?? "Reset";

    public ObservableCollection<string> AvailableEncodings { get; } = new()
    {
        "utf-8",
        "utf-8-bom",
        "gb2312",
        "gbk"
    };

    public ObservableCollection<LyricsProviderSettingViewModel> LyricsProviders { get; } = new();

    public SettingsViewModel(IOptions<GlobalOptions> options, IUILocalizationService? localizationService = null)
    {
        _globalOptions = options.Value;
        _localizationService = localizationService;

        // Subscribe to language changes
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        LoadSettings();
        LoadLanguageSettings();
        _isLoading = false; // Allow saving after initial load
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(SettingsTitle));
        OnPropertyChanged(nameof(SettingsLanguage));
        OnPropertyChanged(nameof(SettingsFollowSystem));
        OnPropertyChanged(nameof(SettingsChinese));
        OnPropertyChanged(nameof(SettingsEnglish));
        OnPropertyChanged(nameof(SettingsProxy));
        OnPropertyChanged(nameof(SettingsProxyEnable));
        OnPropertyChanged(nameof(SettingsProxyAddress));
        OnPropertyChanged(nameof(SettingsLyricsProvider));
        OnPropertyChanged(nameof(SettingsReset));
    }

    private void LoadLanguageSettings()
    {
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        IsChineseSelected = currentCulture.StartsWith("zh");
        IsEnglishSelected = currentCulture.StartsWith("en");
    }

    partial void OnFollowSystemLanguageChanged(bool value)
    {
        if (value)
        {
            // Auto-detect system language
            var systemCulture = CultureInfo.InstalledUICulture.Name;
            ApplyLanguage(systemCulture.StartsWith("zh") ? "zh-CN" : "en-US");
        }
        SaveSettingsIfNotLoading();
    }

    partial void OnIsChineseSelectedChanged(bool value)
    {
        if (value && !FollowSystemLanguage)
        {
            ApplyLanguage("zh-CN");
        }
        SaveSettingsIfNotLoading();
    }

    partial void OnIsEnglishSelectedChanged(bool value)
    {
        if (value && !FollowSystemLanguage)
        {
            ApplyLanguage("en-US");
        }
        SaveSettingsIfNotLoading();
    }

    partial void OnIsProxyEnabledChanged(bool value) => SaveSettingsIfNotLoading();
    partial void OnProxyIpChanged(string value) => SaveSettingsIfNotLoading();
    partial void OnProxyPortChanged(int value) => SaveSettingsIfNotLoading();
    partial void OnIsTranslationEnabledChanged(bool value) => SaveSettingsIfNotLoading();
    partial void OnIsOneLineModeChanged(bool value) => SaveSettingsIfNotLoading();
    partial void OnSkipExistingLyricsChanged(bool value) => SaveSettingsIfNotLoading();
    partial void OnSelectedEncodingChanged(string value) => SaveSettingsIfNotLoading();

    private void ApplyLanguage(string cultureName)
    {
        _localizationService?.SetLanguage(cultureName);
    }

    private void SaveSettingsIfNotLoading()
    {
        if (!_isLoading)
        {
            SaveSettingsToFile();
        }
    }

    private void LoadSettings()
    {
        // Load network settings
        IsProxyEnabled = _globalOptions.NetworkOptions?.IsEnable ?? false;
        ProxyIp = _globalOptions.NetworkOptions?.Ip ?? "127.0.0.1";
        ProxyPort = _globalOptions.NetworkOptions?.Port ?? 7890;

        // Load lyrics settings
        var lyricsConfig = _globalOptions.Provider?.Lyric?.Config;
        if (lyricsConfig != null)
        {
            IsTranslationEnabled = lyricsConfig.IsEnableTranslation;
            IsOneLineMode = lyricsConfig.IsOneLine;
            SkipExistingLyrics = lyricsConfig.IsSkipExistLyricFiles;
            SelectedEncoding = lyricsConfig.FileEncoding ?? "utf-8";
        }

        // Load lyrics providers
        var providers = _globalOptions.Provider?.Lyric?.Plugin;
        if (providers != null)
        {
            foreach (var provider in providers.OrderBy(p => p.Priority))
            {
                LyricsProviders.Add(new LyricsProviderSettingViewModel
                {
                    Name = provider.Name ?? "Unknown",
                    Priority = provider.Priority,
                    IsEnabled = provider.Priority != -1
                });
            }
        }
    }

    private void SaveSettingsToFile()
    {
        try
        {
            // Update GlobalOptions with current values
            if (_globalOptions.NetworkOptions != null)
            {
                _globalOptions.NetworkOptions.IsEnable = IsProxyEnabled;
                _globalOptions.NetworkOptions.Ip = ProxyIp;
                _globalOptions.NetworkOptions.Port = ProxyPort;
            }

            if (_globalOptions.Provider?.Lyric?.Config != null)
            {
                _globalOptions.Provider.Lyric.Config.IsEnableTranslation = IsTranslationEnabled;
                _globalOptions.Provider.Lyric.Config.IsOneLine = IsOneLineMode;
                _globalOptions.Provider.Lyric.Config.IsSkipExistLyricFiles = SkipExistingLyrics;
                _globalOptions.Provider.Lyric.Config.FileEncoding = SelectedEncoding;
            }

            // Serialize and save to file
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(_globalOptions);
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yaml");
            File.WriteAllText(configPath, yaml);
        }
        catch (Exception ex)
        {
            // Log error but don't crash - settings save is best effort
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        _isLoading = true; // Prevent multiple saves during reset
        IsProxyEnabled = false;
        ProxyIp = "127.0.0.1";
        ProxyPort = 7890;
        IsTranslationEnabled = false;
        IsOneLineMode = false;
        SkipExistingLyrics = true;
        SelectedEncoding = "utf-8";
        _isLoading = false;
        SaveSettingsToFile(); // Save once after all resets
    }
}

public partial class LyricsProviderSettingViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _priority;

    [ObservableProperty]
    private bool _isEnabled;
}
