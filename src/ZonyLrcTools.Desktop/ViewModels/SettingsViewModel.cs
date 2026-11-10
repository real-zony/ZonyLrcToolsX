using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Lyrics;
using ZonyLrcTools.Desktop.Infrastructure.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Localization;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>设置页面及自动保存状态。</summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore _settingsStore;
    private readonly IUILocalizationService? _localizationService;
    private readonly GlobalOptions _globalOptions;
    private bool _isLoading = true;

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
    private int _proxyPort = 4780;

    [ObservableProperty]
    private bool _isTranslationEnabled;

    [ObservableProperty]
    private bool _isOneLineMode;

    [ObservableProperty]
    private bool _skipExistingLyrics;

    [ObservableProperty]
    private string _selectedEncoding = "utf-8";

    [ObservableProperty]
    private bool _isOnlyOutputTranslation;

    [ObservableProperty]
    private int _selectedLineBreakIndex;

    [ObservableProperty]
    private bool _isBlockWordEnabled;

    [ObservableProperty]
    private string _blockWordFilePath = "BlockWords.json";

    [ObservableProperty]
    private bool _isRomanOutputEnabled;

    /// <summary>是否显示高级设置。</summary>
    [ObservableProperty]
    private bool _isAdvancedVisible;

    // 显示配置位置及上次保存结果。
    [ObservableProperty]
    private string _storageHint = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSaveSuccess))]
    [NotifyPropertyChangedFor(nameof(ShowSaveFailure))]
    private string _saveStatusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSaveSuccess))]
    [NotifyPropertyChangedFor(nameof(ShowSaveFailure))]
    private bool _hasSaveError;

    /// <summary>可选择的歌词编码。</summary>
    public ObservableCollection<string> AvailableEncodings { get; } = new()
    {
        "utf-8",
        "utf-8-bom",
        "gb2312",
        "gbk"
    };

    /// <summary>预设换行符，与显示列表顺序一致。</summary>
    private static readonly string[] LineBreaks =
    {
        LineBreakType.Unix,
        LineBreakType.Windows,
        LineBreakType.MacOs
    };

    /// <summary>换行符的显示名称。</summary>
    public ObservableCollection<string> LineBreakDisplayNames { get; } = new()
    {
        "LF (Unix)",
        "CRLF (Windows)",
        "CR (macOS)"
    };

    private string CurrentLineBreak => SelectedLineBreakIndex >= 0 && SelectedLineBreakIndex < LineBreaks.Length
        ? LineBreaks[SelectedLineBreakIndex]
        : _globalOptions.Provider?.Lyric?.Config?.LineBreak ?? LineBreakType.Unix;

    /// <summary>歌词提供者及搜索顺序。</summary>
    public ObservableCollection<LyricsProviderSettingViewModel> LyricsProviders { get; } = new();

    /// <summary>标签提供者及加载顺序。</summary>
    public ObservableCollection<TagInfoProviderSettingViewModel> TagProviders { get; } = new();

    // 本地化文本。
    /// <summary>设置标题的本地化文本。</summary>
    public string SettingsTitle => _localizationService?["Settings_Title"] ?? "Settings";
    /// <summary>语言设置标题的本地化文本。</summary>
    public string SettingsLanguage => _localizationService?["Settings_Language"] ?? "Language";
    /// <summary>跟随系统语言的本地化文本。</summary>
    public string SettingsFollowSystem => _localizationService?["Settings_FollowSystem"] ?? "Follow System";
    /// <summary>中文选项的本地化文本。</summary>
    public string SettingsChinese => _localizationService?["Settings_Chinese"] ?? "Chinese";
    /// <summary>英文选项的本地化文本。</summary>
    public string SettingsEnglish => _localizationService?["Settings_English"] ?? "English";
    /// <summary>代理设置标题的本地化文本。</summary>
    public string SettingsProxy => _localizationService?["Settings_Proxy"] ?? "Proxy";
    /// <summary>启用代理的本地化文本。</summary>
    public string SettingsProxyEnable => _localizationService?["Settings_ProxyEnable"] ?? "Enable Proxy";
    /// <summary>代理地址的本地化文本。</summary>
    public string SettingsProxyAddress => _localizationService?["Settings_ProxyAddress"] ?? "Proxy Address";
    /// <summary>代理端口的本地化文本。</summary>
    public string SettingsProxyPort => _localizationService?["Settings_ProxyPort"] ?? "Port";
    /// <summary>歌词提供者标题的本地化文本。</summary>
    public string SettingsLyricsProvider => _localizationService?["Settings_LyricsProvider"] ?? "Lyrics Provider";
    /// <summary>恢复默认值的本地化文本。</summary>
    public string SettingsReset => _localizationService?["Settings_Reset"] ?? "Reset";

    /// <summary>基本设置标题的本地化文本。</summary>
    public string SettingsBasicSection => _localizationService?["Settings_BasicSection"] ?? "Basics";
    /// <summary>高级设置标题的本地化文本。</summary>
    public string SettingsAdvancedSection => _localizationService?["Settings_AdvancedSection"] ?? "Advanced";
    /// <summary>高级选项开关的本地化文本。</summary>
    public string SettingsShowAdvanced => _localizationService?["Settings_ShowAdvanced"] ?? "Show advanced options";
    /// <summary>歌词设置标题的本地化文本。</summary>
    public string SettingsLyricsSection => _localizationService?["Settings_LyricsSection"] ?? "Lyrics";
    /// <summary>翻译开关的本地化文本。</summary>
    public string SettingsEnableTranslation => _localizationService?["Settings_EnableTranslation"] ?? "Enable translation lyrics";
    /// <summary>合并双语歌词开关的本地化文本。</summary>
    public string SettingsOneLineMode => _localizationService?["Settings_OneLineMode"] ?? "One-line mode (original + translation on same line)";
    /// <summary>仅输出翻译开关的本地化文本。</summary>
    public string SettingsOnlyTranslation => _localizationService?["Settings_OnlyTranslation"] ?? "Output translation only";
    /// <summary>跳过已有文件开关的本地化文本。</summary>
    public string SettingsSkipExisting => _localizationService?["Settings_SkipExisting"] ?? "Skip existing lyrics files";
    /// <summary>编码选项的本地化文本。</summary>
    public string SettingsFileEncoding => _localizationService?["Settings_FileEncoding"] ?? "File Encoding:";
    /// <summary>换行符选项的本地化文本。</summary>
    public string SettingsLineBreak => _localizationService?["Settings_LineBreak"] ?? "Line break:";
    /// <summary>罗马音开关的本地化文本。</summary>
    public string SettingsRomanOutput => _localizationService?["Settings_RomanOutput"] ?? "Enable romanized output (NetEase)";
    /// <summary>屏蔽词设置标题的本地化文本。</summary>
    public string SettingsBlockWordSection => _localizationService?["Settings_BlockWordSection"] ?? "Tag block word dictionary";
    /// <summary>屏蔽词开关的本地化文本。</summary>
    public string SettingsBlockWordEnable => _localizationService?["Settings_BlockWordEnable"] ?? "Enable block word dictionary";
    /// <summary>屏蔽词路径的本地化文本。</summary>
    public string SettingsBlockWordPath => _localizationService?["Settings_BlockWordPath"] ?? "Dictionary file (relative to the working directory)";
    /// <summary>标签提供者标题的本地化文本。</summary>
    public string SettingsTagProvider => _localizationService?["Settings_TagProvider"] ?? "Tag Provider";
    /// <summary>优先级标题的本地化文本。</summary>
    public string SettingsPriority => _localizationService?["Settings_Priority"] ?? "Priority: ";
    /// <summary>上移按钮的本地化文本。</summary>
    public string SettingsMoveUp => _localizationService?["Settings_MoveUp"] ?? "Move up";
    /// <summary>下移按钮的本地化文本。</summary>
    public string SettingsMoveDown => _localizationService?["Settings_MoveDown"] ?? "Move down";
    /// <summary>提供者顺序说明的本地化文本。</summary>
    public string SettingsProviderOrderHint => _localizationService?["Settings_ProviderOrderHint"]
        ?? "The list order is the search order. Use the arrows to change which source is tried first.";
    /// <summary>设置保存位置的本地化文本。</summary>
    public string SettingsStorage => _localizationService?["Settings_Storage"] ?? "Settings file";
    /// <summary>保存成功的本地化文本。</summary>
    public string SettingsSaved => _localizationService?["Settings_Saved"] ?? "Saved";
    /// <summary>保存失败的本地化文本。</summary>
    public string SettingsSaveFailed => _localizationService?["Settings_SaveFailed"] ?? "Could not save settings";
    /// <summary>代理重启提示的本地化文本。</summary>
    public string SettingsProxyRestartHint => _localizationService?["Settings_ProxyRestartHint"]
        ?? "Proxy changes take effect the next time the app starts.";

    /// <summary>是否显示保存成功提示。</summary>
    public bool ShowSaveSuccess => !HasSaveError && !string.IsNullOrEmpty(SaveStatusText);

    /// <summary>是否显示保存失败提示。</summary>
    public bool ShowSaveFailure => HasSaveError && !string.IsNullOrEmpty(SaveStatusText);

    /// <summary>创建设置页面。</summary>
    public SettingsViewModel(ISettingsStore settingsStore, IUILocalizationService? localizationService = null)
    {
        _settingsStore = settingsStore;
        _localizationService = localizationService;
        _globalOptions = settingsStore.Options;

        // 跟随应用语言切换更新界面。
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        LoadSettings();
        LoadLanguageSettings();
        StorageHint = $"{SettingsStorage}: {_settingsStore.ConfigFilePath}";
        _isLoading = false;
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
        OnPropertyChanged(nameof(SettingsProxyPort));
        OnPropertyChanged(nameof(SettingsLyricsProvider));
        OnPropertyChanged(nameof(SettingsReset));
        OnPropertyChanged(nameof(SettingsBasicSection));
        OnPropertyChanged(nameof(SettingsAdvancedSection));
        OnPropertyChanged(nameof(SettingsShowAdvanced));
        OnPropertyChanged(nameof(SettingsLyricsSection));
        OnPropertyChanged(nameof(SettingsEnableTranslation));
        OnPropertyChanged(nameof(SettingsOneLineMode));
        OnPropertyChanged(nameof(SettingsOnlyTranslation));
        OnPropertyChanged(nameof(SettingsSkipExisting));
        OnPropertyChanged(nameof(SettingsFileEncoding));
        OnPropertyChanged(nameof(SettingsLineBreak));
        OnPropertyChanged(nameof(SettingsRomanOutput));
        OnPropertyChanged(nameof(SettingsBlockWordSection));
        OnPropertyChanged(nameof(SettingsBlockWordEnable));
        OnPropertyChanged(nameof(SettingsBlockWordPath));
        OnPropertyChanged(nameof(SettingsTagProvider));
        OnPropertyChanged(nameof(SettingsPriority));
        OnPropertyChanged(nameof(SettingsMoveUp));
        OnPropertyChanged(nameof(SettingsMoveDown));
        OnPropertyChanged(nameof(SettingsProviderOrderHint));
        OnPropertyChanged(nameof(SettingsStorage));
        OnPropertyChanged(nameof(SettingsSaved));
        OnPropertyChanged(nameof(SettingsSaveFailed));
        OnPropertyChanged(nameof(SettingsProxyRestartHint));
        StorageHint = $"{SettingsStorage}: {_settingsStore.ConfigFilePath}";
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
            // 使用操作系统语言。
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
    partial void OnIsOnlyOutputTranslationChanged(bool value) => SaveSettingsIfNotLoading();
    partial void OnSelectedLineBreakIndexChanged(int value) => SaveSettingsIfNotLoading();
    partial void OnIsBlockWordEnabledChanged(bool value) => SaveSettingsIfNotLoading();
    partial void OnBlockWordFilePathChanged(string value) => SaveSettingsIfNotLoading();
    partial void OnIsRomanOutputEnabledChanged(bool value) => SaveSettingsIfNotLoading();

    private void ApplyLanguage(string cultureName)
    {
        _localizationService?.SetLanguage(cultureName);
    }

    private void SaveSettingsIfNotLoading()
    {
        if (_isLoading)
        {
            return;
        }

        WriteSettingsToOptions();
        _ = SaveSettingsAsync();
    }

    private void LoadSettings()
    {
        // 加载代理设置。
        IsProxyEnabled = _globalOptions.NetworkOptions?.IsEnable ?? false;
        ProxyIp = _globalOptions.NetworkOptions?.Ip ?? "127.0.0.1";
        ProxyPort = _globalOptions.NetworkOptions?.Port ?? 4780;

        // 加载歌词设置，保留未列入预设的有效编码。
        var lyricsConfig = _globalOptions.Provider?.Lyric?.Config;
        if (lyricsConfig != null)
        {
            IsTranslationEnabled = lyricsConfig.IsEnableTranslation;
            IsOneLineMode = lyricsConfig.IsOneLine;
            SkipExistingLyrics = lyricsConfig.IsSkipExistLyricFiles;
            SelectedEncoding = lyricsConfig.FileEncoding ?? "utf-8";
            if (!AvailableEncodings.Contains(SelectedEncoding))
            {
                AvailableEncodings.Add(SelectedEncoding);
            }
            IsOnlyOutputTranslation = lyricsConfig.IsOnlyOutputTranslation;
            SelectedLineBreakIndex = ResolveLineBreakIndex(lyricsConfig.LineBreak);
        }

        // 加载屏蔽词设置。
        var blockWord = _globalOptions.Provider?.Tag?.BlockWord;
        if (blockWord != null)
        {
            IsBlockWordEnabled = blockWord.IsEnable;
            BlockWordFilePath = string.IsNullOrWhiteSpace(blockWord.FilePath)
                ? "BlockWords.json"
                : blockWord.FilePath;
        }

        // 罗马音开关保存在网易云提供者的扩展设置中。
        var netEase = _globalOptions.Provider?.Lyric?.Plugin?
            .FirstOrDefault(p => string.Equals(p.Name, "NetEase", StringComparison.OrdinalIgnoreCase));
        if (netEase?.Additional != null &&
            netEase.Additional.TryGetValue("isEnableRomanOutput", out var romanValue))
        {
            IsRomanOutputEnabled = string.Equals(romanValue, "true", StringComparison.OrdinalIgnoreCase);
        }

        // 启用的提供者按优先级排列，禁用项放到末尾。
        var tagProviders = _globalOptions.Provider?.Tag?.Plugin;
        if (tagProviders != null)
        {
            foreach (var provider in tagProviders.Where(p => p.Priority != -1).OrderBy(p => p.Priority))
            {
                AddTagProviderRow(provider.Name, true);
            }

            // 禁用项不参与搜索。
            foreach (var provider in tagProviders.Where(p => p.Priority == -1))
            {
                AddTagProviderRow(provider.Name, false);
            }

            NormalizeProviderOrder(TagProviders);
        }

        // 歌词提供者与标签提供者采用相同排序规则。
        var providers = _globalOptions.Provider?.Lyric?.Plugin;
        if (providers != null)
        {
            foreach (var provider in providers.Where(p => p.Priority != -1).OrderBy(p => p.Priority))
            {
                AddLyricsProviderRow(provider.Name, true);
            }

            foreach (var provider in providers.Where(p => p.Priority == -1))
            {
                AddLyricsProviderRow(provider.Name, false);
            }

            NormalizeProviderOrder(LyricsProviders);
        }
    }

    private void AddTagProviderRow(string? name, bool isEnabled)
    {
        var row = new TagInfoProviderSettingViewModel
        {
            Name = name ?? "Unknown",
            IsEnabled = isEnabled
        };

        // 创建行后绑定移动命令。
        row.MoveUpCommand = new RelayCommand(() => MoveProviderBy(TagProviders, row, -1));
        row.MoveDownCommand = new RelayCommand(() => MoveProviderBy(TagProviders, row, 1));

        // 将行内编辑接入自动保存。
        row.PropertyChanged += OnProviderRowChanged;
        TagProviders.Add(row);
    }

    private void AddLyricsProviderRow(string? name, bool isEnabled)
    {
        var row = new LyricsProviderSettingViewModel
        {
            Name = name ?? "Unknown",
            IsEnabled = isEnabled
        };

        row.MoveUpCommand = new RelayCommand(() => MoveProviderBy(LyricsProviders, row, -1));
        row.MoveDownCommand = new RelayCommand(() => MoveProviderBy(LyricsProviders, row, 1));

        row.PropertyChanged += OnProviderRowChanged;
        LyricsProviders.Add(row);
    }

    /// <summary>提供者启用状态改变后重新排序并保存。</summary>
    private void OnProviderRowChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isLoading || _isNormalizing)
        {
            return;
        }

        if (e.PropertyName is nameof(LyricsProviderSettingViewModel.IsEnabled)
            or nameof(TagInfoProviderSettingViewModel.IsEnabled))
        {
            _isNormalizing = true;
            try
            {
                if (sender is LyricsProviderSettingViewModel) NormalizeProviderOrder(LyricsProviders);
                else if (sender is TagInfoProviderSettingViewModel) NormalizeProviderOrder(TagProviders);
            }
            finally
            {
                _isNormalizing = false;
            }

            SaveSettingsIfNotLoading();
            return;
        }

        if (e.PropertyName is nameof(LyricsProviderSettingViewModel.Priority)
            or nameof(TagInfoProviderSettingViewModel.Priority))
        {
            SaveSettingsIfNotLoading();
        }
    }

    private bool _isNormalizing;

    /// <summary>启用项在前，优先级按可见顺序从一开始。</summary>
    private void NormalizeProviderOrder<T>(ObservableCollection<T> rows) where T : class, IProviderRowViewModel
    {
        var ordered = rows.Where(r => r.IsEnabled).Concat(rows.Where(r => !r.IsEnabled)).ToList();

        for (var target = 0; target < ordered.Count; target++)
        {
            var current = rows.IndexOf(ordered[target]);
            if (current != target)
            {
                rows.Move(current, target);
            }
        }

        var enabledCount = ordered.Count(r => r.IsEnabled);
        for (var i = 0; i < ordered.Count; i++)
        {
            var row = ordered[i];

            // 禁用项统一使用 -1。
            row.Priority = i < enabledCount ? i + 1 : -1;
            row.PriorityLabel = i < enabledCount ? (i + 1).ToString(CultureInfo.InvariantCulture) : "—";
            row.CanMoveUp = i > 0 && i < enabledCount;
            row.CanMoveDown = i + 1 < enabledCount;
        }
    }

    /// <summary>在启用项范围内移动提供者。</summary>
    private void MoveProviderBy<T>(ObservableCollection<T> rows, T row, int offset)
        where T : class, IProviderRowViewModel
    {
        var index = rows.IndexOf(row);
        var target = index + offset;
        if (index < 0 || target < 0 || target >= rows.Count || !row.IsEnabled || !rows[target].IsEnabled)
        {
            return;
        }

        _isNormalizing = true;
        try
        {
            rows.Move(index, target);
            NormalizeProviderOrder(rows);
        }
        finally
        {
            _isNormalizing = false;
        }

        SaveSettingsIfNotLoading();
    }

    private static int ResolveLineBreakIndex(string? value)
    {
        for (var i = 0; i < LineBreaks.Length; i++)
        {
            if (string.Equals(LineBreaks[i], value, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private void WriteSettingsToOptions()
    {
        if (_globalOptions.NetworkOptions != null)
        {
            _globalOptions.NetworkOptions.IsEnable = IsProxyEnabled;
            _globalOptions.NetworkOptions.Ip = ProxyIp;
            _globalOptions.NetworkOptions.Port = ProxyPort;
        }

        var lyricsConfig = _globalOptions.Provider?.Lyric?.Config;
        if (lyricsConfig != null)
        {
            lyricsConfig.IsEnableTranslation = IsTranslationEnabled;
            lyricsConfig.IsOneLine = IsOneLineMode;
            lyricsConfig.IsSkipExistLyricFiles = SkipExistingLyrics;
            lyricsConfig.FileEncoding = SelectedEncoding;
            lyricsConfig.IsOnlyOutputTranslation = IsOnlyOutputTranslation;
            lyricsConfig.LineBreak = CurrentLineBreak;
        }

        var blockWord = _globalOptions.Provider?.Tag?.BlockWord;
        if (blockWord != null)
        {
            blockWord.IsEnable = IsBlockWordEnabled;
            blockWord.FilePath = BlockWordFilePath;
        }

        var netEase = _globalOptions.Provider?.Lyric?.Plugin?
            .FirstOrDefault(p => string.Equals(p.Name, "NetEase", StringComparison.OrdinalIgnoreCase));
        if (netEase != null)
        {
            netEase.Additional ??= new Dictionary<string, string>();
            netEase.Additional["isEnableRomanOutput"] = IsRomanOutputEnabled ? "true" : "false";
        }

        // 同步启用状态及搜索顺序。
        ApplyProviderSelection(_globalOptions.Provider?.Lyric?.Plugin, LyricsProviders);
        ApplyProviderSelection(_globalOptions.Provider?.Tag?.Plugin, TagProviders);
    }

    /// <summary>将歌词提供者的界面顺序写入共享配置。</summary>
    private static void ApplyProviderSelection(
        IEnumerable<LyricsProviderOptions>? options,
        IEnumerable<LyricsProviderSettingViewModel> viewModels)
    {
        if (options == null)
        {
            return;
        }

        var optionList = options.ToList();
        var order = 1;

        foreach (var row in viewModels)
        {
            var option = optionList.FirstOrDefault(o => string.Equals(o.Name, row.Name, StringComparison.OrdinalIgnoreCase));
            if (option == null)
            {
                continue;
            }

            option.Priority = row.IsEnabled ? order++ : -1;
        }
    }

    private static void ApplyProviderSelection(
        IEnumerable<TagInfoProviderOptions>? options,
        IEnumerable<TagInfoProviderSettingViewModel> viewModels)
    {
        if (options == null)
        {
            return;
        }

        var optionList = options.ToList();
        var order = 1;

        foreach (var row in viewModels)
        {
            var option = optionList.FirstOrDefault(o => string.Equals(o.Name, row.Name, StringComparison.OrdinalIgnoreCase));
            if (option == null)
            {
                continue;
            }

            option.Priority = row.IsEnabled ? order++ : -1;
        }
    }

    private async Task SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        // 合并连续输入，但配置在内存中立即生效。
        var pending = Interlocked.Increment(ref _saveRequestId);
        await Task.Delay(SaveDebounceMilliseconds, cancellationToken).ConfigureAwait(true);

        if (pending != Volatile.Read(ref _saveRequestId))
        {
            return;
        }

        var result = await _settingsStore.SaveAsync(cancellationToken).ConfigureAwait(true);
        if (pending != Volatile.Read(ref _saveRequestId))
        {
            return;
        }

        HasSaveError = !result.IsSuccess;
        SaveStatusText = result.IsSuccess
            ? $"{SettingsSaved}: {result.FilePath}"
            : $"{SettingsSaveFailed}: {result.ErrorMessage}";
        StorageHint = $"{SettingsStorage}: {_settingsStore.ConfigFilePath}";
    }

    private int _saveRequestId;
    private const int SaveDebounceMilliseconds = 400;

    [RelayCommand]
    private async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _saveRequestId);
        _isLoading = true;
        IsProxyEnabled = false;
        ProxyIp = "127.0.0.1";
        ProxyPort = 4780;
        IsTranslationEnabled = true;
        IsOneLineMode = true;
        SkipExistingLyrics = false;
        IsOnlyOutputTranslation = false;
        SelectedEncoding = "utf-8";
        SelectedLineBreakIndex = 0;
        IsBlockWordEnabled = false;
        BlockWordFilePath = "BlockWords.json";
        IsRomanOutputEnabled = false;

        // 恢复随程序附带的默认顺序，额外提供者保留在末尾。
        RestoreProviderOrder(LyricsProviders, ["NetEase", "QQ", "KuGou", "KuWo"], row => row.Name);
        RestoreProviderOrder(TagProviders, ["Taglib", "FileName"], row => row.Name);
        foreach (var row in LyricsProviders)
        {
            row.IsEnabled = true;
        }

        foreach (var row in TagProviders)
        {
            row.IsEnabled = true;
        }

        NormalizeProviderOrder(LyricsProviders);
        NormalizeProviderOrder(TagProviders);

        _isLoading = false;

        // 所有默认值恢复后只保存一次。
        WriteSettingsToOptions();
        var result = await _settingsStore.SaveAsync(cancellationToken).ConfigureAwait(true);
        HasSaveError = !result.IsSuccess;
        SaveStatusText = result.IsSuccess
            ? $"{SettingsSaved}: {result.FilePath}"
            : $"{SettingsSaveFailed}: {result.ErrorMessage}";
    }

    private static void RestoreProviderOrder<T>(ObservableCollection<T> rows, string[] names, Func<T, string> getName)
        where T : class
    {
        var target = 0;
        foreach (var name in names)
        {
            var row = rows.FirstOrDefault(row => string.Equals(getName(row), name, StringComparison.OrdinalIgnoreCase));
            if (row is not null)
            {
                rows.Move(rows.IndexOf(row), target++);
            }
        }
    }
}

/// <summary>可排序的提供者设置行。</summary>
public interface IProviderRowViewModel
{
    /// <summary>是否启用。</summary>
    bool IsEnabled { get; set; }

    /// <summary>搜索优先级，禁用时为 -1。</summary>
    int Priority { get; set; }

    /// <summary>界面显示的顺序。</summary>
    string PriorityLabel { get; set; }

    /// <summary>是否可以向上移动。</summary>
    bool CanMoveUp { get; set; }

    /// <summary>是否可以向下移动。</summary>
    bool CanMoveDown { get; set; }

    /// <summary>禁用时降低行透明度。</summary>
    double RowOpacity { get; }

    /// <summary>向上移动命令。</summary>
    IRelayCommand MoveUpCommand { get; }

    /// <summary>向下移动命令。</summary>
    IRelayCommand MoveDownCommand { get; }
}

/// <summary>歌词提供者设置行。</summary>
public partial class LyricsProviderSettingViewModel : ObservableObject, IProviderRowViewModel
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RowOpacity))]
    private bool _isEnabled;

    /// <summary>搜索优先级，禁用时为 -1。</summary>
    [ObservableProperty]
    private int _priority;

    /// <summary>显示的顺序，禁用时显示横线。</summary>
    [ObservableProperty]
    private string _priorityLabel = string.Empty;

    [ObservableProperty]
    private bool _canMoveUp;

    [ObservableProperty]
    private bool _canMoveDown;

    /// <inheritdoc />
    public double RowOpacity => IsEnabled ? 1.0 : 0.45;

    /// <inheritdoc />
    public IRelayCommand MoveUpCommand { get; set; } = new RelayCommand(() => { });

    /// <inheritdoc />
    public IRelayCommand MoveDownCommand { get; set; } = new RelayCommand(() => { });
}

/// <summary>标签提供者设置行。</summary>
public partial class TagInfoProviderSettingViewModel : ObservableObject, IProviderRowViewModel
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RowOpacity))]
    private bool _isEnabled;

    /// <inheritdoc cref="LyricsProviderSettingViewModel.Priority" />
    [ObservableProperty]
    private int _priority;

    /// <inheritdoc cref="LyricsProviderSettingViewModel.PriorityLabel" />
    [ObservableProperty]
    private string _priorityLabel = string.Empty;

    [ObservableProperty]
    private bool _canMoveUp;

    [ObservableProperty]
    private bool _canMoveDown;

    /// <inheritdoc />
    public double RowOpacity => IsEnabled ? 1.0 : 0.45;

    /// <inheritdoc />
    public IRelayCommand MoveUpCommand { get; set; } = new RelayCommand(() => { });

    /// <inheritdoc />
    public IRelayCommand MoveDownCommand { get; set; } = new RelayCommand(() => { });
}
