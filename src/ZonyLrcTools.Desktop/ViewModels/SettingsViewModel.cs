using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Lyrics;
using ZonyLrcTools.Desktop.Infrastructure.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Localization;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore _settingsStore;
    private readonly IUILocalizationService? _localizationService;
    private readonly GlobalOptions _globalOptions;
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

    /// <summary>
    /// Advanced options are collapsed by default: the common path is language + proxy + the four
    /// lyric flags, everything else is a power-user setting.
    /// </summary>
    [ObservableProperty]
    private bool _isAdvancedVisible;

    // Status line for the settings page: shows where settings are stored and whether the
    // last save succeeded.
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

    /// <summary>
    /// Encoding choices offered in the settings page. These map to the values accepted by
    /// <see cref="GlobalLyricsConfigOptions.FileEncoding"/>.
    /// </summary>
    public ObservableCollection<string> AvailableEncodings { get; } = new()
    {
        "utf-8",
        "utf-8-bom",
        "gb2312",
        "gbk"
    };

    /// <summary>
    /// Line-break choices, parallel to <see cref="LineBreakDisplayNames"/>. The stored value is the
    /// actual escape sequence so the YAML file stays compatible with the CLI.
    /// </summary>
    private static readonly string[] LineBreaks =
    {
        LineBreakType.Unix,
        LineBreakType.Windows,
        LineBreakType.MacOs
    };

    /// <summary>
    /// Display names for the line-break selector, parallel to the internal sequence table.
    /// </summary>
    public ObservableCollection<string> LineBreakDisplayNames { get; } = new()
    {
        "LF (Unix)",
        "CRLF (Windows)",
        "CR (macOS)"
    };

    /// <summary>
    /// Value the current settings file uses, for the selected line-break entry.
    /// </summary>
    private string CurrentLineBreak => LineBreaks[Math.Clamp(SelectedLineBreakIndex, 0, LineBreaks.Length - 1)];

    public ObservableCollection<LyricsProviderSettingViewModel> LyricsProviders { get; } = new();

    public ObservableCollection<TagInfoProviderSettingViewModel> TagProviders { get; } = new();

    // Localized strings
    public string SettingsTitle => _localizationService?["Settings_Title"] ?? "Settings";
    public string SettingsLanguage => _localizationService?["Settings_Language"] ?? "Language";
    public string SettingsFollowSystem => _localizationService?["Settings_FollowSystem"] ?? "Follow System";
    public string SettingsChinese => _localizationService?["Settings_Chinese"] ?? "Chinese";
    public string SettingsEnglish => _localizationService?["Settings_English"] ?? "English";
    public string SettingsProxy => _localizationService?["Settings_Proxy"] ?? "Proxy";
    public string SettingsProxyEnable => _localizationService?["Settings_ProxyEnable"] ?? "Enable Proxy";
    public string SettingsProxyAddress => _localizationService?["Settings_ProxyAddress"] ?? "Proxy Address";
    public string SettingsProxyPort => _localizationService?["Settings_ProxyPort"] ?? "Port";
    public string SettingsLyricsProvider => _localizationService?["Settings_LyricsProvider"] ?? "Lyrics Provider";
    public string SettingsReset => _localizationService?["Settings_Reset"] ?? "Reset";

    public string SettingsBasicSection => _localizationService?["Settings_BasicSection"] ?? "Basics";
    public string SettingsAdvancedSection => _localizationService?["Settings_AdvancedSection"] ?? "Advanced";
    public string SettingsShowAdvanced => _localizationService?["Settings_ShowAdvanced"] ?? "Show advanced options";
    public string SettingsLyricsSection => _localizationService?["Settings_LyricsSection"] ?? "Lyrics";
    public string SettingsEnableTranslation => _localizationService?["Settings_EnableTranslation"] ?? "Enable translation lyrics";
    public string SettingsOneLineMode => _localizationService?["Settings_OneLineMode"] ?? "One-line mode (original + translation on same line)";
    public string SettingsOnlyTranslation => _localizationService?["Settings_OnlyTranslation"] ?? "Output translation only";
    public string SettingsSkipExisting => _localizationService?["Settings_SkipExisting"] ?? "Skip existing lyrics files";
    public string SettingsFileEncoding => _localizationService?["Settings_FileEncoding"] ?? "File Encoding:";
    public string SettingsLineBreak => _localizationService?["Settings_LineBreak"] ?? "Line break:";
    public string SettingsRomanOutput => _localizationService?["Settings_RomanOutput"] ?? "Enable romanized output (NetEase)";
    public string SettingsBlockWordSection => _localizationService?["Settings_BlockWordSection"] ?? "Tag block word dictionary";
    public string SettingsBlockWordEnable => _localizationService?["Settings_BlockWordEnable"] ?? "Enable block word dictionary";
    public string SettingsBlockWordPath => _localizationService?["Settings_BlockWordPath"] ?? "Dictionary file (relative to the app folder)";
    public string SettingsTagProvider => _localizationService?["Settings_TagProvider"] ?? "Tag Provider";
    public string SettingsPriority => _localizationService?["Settings_Priority"] ?? "Priority: ";
    public string SettingsMoveUp => _localizationService?["Settings_MoveUp"] ?? "Move up";
    public string SettingsMoveDown => _localizationService?["Settings_MoveDown"] ?? "Move down";
    public string SettingsProviderOrderHint => _localizationService?["Settings_ProviderOrderHint"]
        ?? "The list order is the search order. Use the arrows to change which source is tried first.";
    public string SettingsStorage => _localizationService?["Settings_Storage"] ?? "Settings file";
    public string SettingsSaved => _localizationService?["Settings_Saved"] ?? "Saved";
    public string SettingsSaveFailed => _localizationService?["Settings_SaveFailed"] ?? "Could not save settings";
    public string SettingsProxyRestartHint => _localizationService?["Settings_ProxyRestartHint"]
        ?? "Proxy changes take effect the next time the app starts.";

    /// <summary>
    /// Save feedback: only one of these is ever true, and both stay false before the first save.
    /// </summary>
    public bool ShowSaveSuccess => !HasSaveError && !string.IsNullOrEmpty(SaveStatusText);

    public bool ShowSaveFailure => HasSaveError && !string.IsNullOrEmpty(SaveStatusText);

    public SettingsViewModel(ISettingsStore settingsStore, IUILocalizationService? localizationService = null)
    {
        _settingsStore = settingsStore;
        _localizationService = localizationService;
        _globalOptions = settingsStore.Options;

        // Subscribe to language changes
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged += OnLanguageChanged;
        }

        LoadSettings();
        LoadLanguageSettings();
        StorageHint = $"{SettingsStorage}: {_settingsStore.ConfigFilePath}";
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
            IsOnlyOutputTranslation = lyricsConfig.IsOnlyOutputTranslation;
            SelectedLineBreakIndex = ResolveLineBreakIndex(lyricsConfig.LineBreak);
        }

        // Load block word settings
        var blockWord = _globalOptions.Provider?.Tag?.BlockWord;
        if (blockWord != null)
        {
            IsBlockWordEnabled = blockWord.IsEnable;
            BlockWordFilePath = string.IsNullOrWhiteSpace(blockWord.FilePath)
                ? "BlockWords.json"
                : blockWord.FilePath;
        }

        // Load romanized output flag from the NetEase provider's extended options
        var netEase = _globalOptions.Provider?.Lyric?.Plugin?
            .FirstOrDefault(p => string.Equals(p.Name, "NetEase", StringComparison.OrdinalIgnoreCase));
        if (netEase?.Additional != null &&
            netEase.Additional.TryGetValue("isEnableRomanOutput", out var romanValue))
        {
            IsRomanOutputEnabled = string.Equals(romanValue, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Tag providers are listed in ascending priority, which is also the order the loader tries
        // them in. The list itself is the order now: the number is derived from the position, so a
        // row that is moved up shows a smaller number without any extra bookkeeping.
        var tagProviders = _globalOptions.Provider?.Tag?.Plugin;
        if (tagProviders != null)
        {
            foreach (var provider in tagProviders.Where(p => p.Priority != -1).OrderBy(p => p.Priority))
            {
                AddTagProviderRow(provider.Name, true);
            }

            // Disabled sources (priority -1) have no slot of their own; they sit at the bottom of
            // the list, greyed out, so re-enabling one just drops it back into the queue.
            foreach (var provider in tagProviders.Where(p => p.Priority == -1))
            {
                AddTagProviderRow(provider.Name, false);
            }

            NormalizeProviderOrder(TagProviders);
        }

        // Lyrics providers follow the same rule as the tag providers above.
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

        // The arrows move the row inside its own list; the callback has to be installed after the
        // row exists, otherwise the lambda would capture an unassigned local.
        row.MoveUpCommand = new RelayCommand(() => MoveProviderBy(TagProviders, row, -1));
        row.MoveDownCommand = new RelayCommand(() => MoveProviderBy(TagProviders, row, 1));

        // Rows are separate objects, so their own change notifications have to be wired back into
        // the save pipeline or the checkbox moves and the file never changes.
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

    /// <summary>
    /// Any edit inside a provider row funnels into the same debounced save that the top-level
    /// settings use. Toggling a checkbox also re-sorts the list, because enabled sources always
    /// sit above disabled ones and the visible numbering has to follow.
    /// </summary>
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

    /// <summary>
    /// The list order <em>is</em> the search order. This enforces the two invariants that make that
    /// readable: enabled rows above disabled ones, and a visible 1..n numbering that matches the
    /// position. Priorities are derived here, never typed by hand.
    /// </summary>
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

            // 禁用行在内存里就写 -1，和配置文件里的语义保持一致；否则一旦某条路径漏掉
            // ApplyProviderSelection，就会把一个不存在的「第 5 名」写回 config.yaml。
            row.Priority = i < enabledCount ? i + 1 : -1;
            row.PriorityLabel = i < enabledCount ? (i + 1).ToString(CultureInfo.InvariantCulture) : "—";
            row.CanMoveUp = i > 0 && i < enabledCount;
            row.CanMoveDown = i + 1 < enabledCount;
        }
    }

    /// <summary>
    /// Moves a row one slot up or down. Disabled rows are pinned to the bottom, so a move that
    /// would cross the enabled/disabled boundary is ignored rather than silently doing nothing
    /// visible.
    /// </summary>
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

        return 0;
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

        // Provider toggles are not decoration: unchecking writes priority -1, which is how the
        // downloader skips a source. Without this the checkboxes move and the file never changes.
        ApplyProviderSelection(_globalOptions.Provider?.Lyric?.Plugin, LyricsProviders);
        ApplyProviderSelection(_globalOptions.Provider?.Tag?.Plugin, TagProviders);
    }

    /// <summary>
    /// Push the row state onto the options: the checkbox decides whether a source runs at all
    /// (disabled sources store -1, which is how the downloader skips them) and the list position
    /// decides the search order. The number in the UI is derived from that position, so there is
    /// nothing to reconcile here — no duplicate slots, no silent renumbering.
    /// </summary>
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

    private async Task SaveSettingsAsync()
    {
        // Debounce rapid changes (dragging a port spinner, typing an IP) into one write.
        var pending = Interlocked.Increment(ref _saveRequestId);
        await Task.Delay(SaveDebounceMilliseconds).ConfigureAwait(true);

        if (pending != Volatile.Read(ref _saveRequestId))
        {
            return;
        }

        var result = await _settingsStore.SaveAsync().ConfigureAwait(true);

        HasSaveError = !result.IsSuccess;
        SaveStatusText = result.IsSuccess
            ? $"{SettingsSaved}: {result.FilePath}"
            : $"{SettingsSaveFailed}: {result.ErrorMessage}";
        StorageHint = $"{SettingsStorage}: {_settingsStore.ConfigFilePath}";
    }

    private int _saveRequestId;
    private const int SaveDebounceMilliseconds = 400;

    [RelayCommand]
    private async Task ResetToDefaults()
    {
        _isLoading = true; // Prevent multiple saves during reset
        IsProxyEnabled = false;
        ProxyIp = "127.0.0.1";
        ProxyPort = 7890;
        IsTranslationEnabled = true;
        IsOneLineMode = true;
        SkipExistingLyrics = false;
        IsOnlyOutputTranslation = false;
        SelectedEncoding = "utf-8";
        SelectedLineBreakIndex = 0;
        IsBlockWordEnabled = false;
        BlockWordFilePath = "BlockWords.json";
        IsRomanOutputEnabled = false;

        // Provider rows are settings too; leaving a disabled source behind after "reset" would
        // look like the button half worked. Everything comes back enabled, in the order the rows
        // are listed, which the numbering then reflects as 1..n again.
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

        // Save once after all resets
        WriteSettingsToOptions();
        var result = await _settingsStore.SaveAsync().ConfigureAwait(true);
        HasSaveError = !result.IsSuccess;
        SaveStatusText = result.IsSuccess
            ? $"{SettingsSaved}: {result.FilePath}"
            : $"{SettingsSaveFailed}: {result.ErrorMessage}";
    }
}

/// <summary>
/// Shared shape of a source row so the reordering logic can work on both lists without
/// duplicating it. The visible order is the search order; <see cref="Priority"/> and
/// <see cref="PriorityLabel"/> are written by the view model, never typed by the user.
/// </summary>
public interface IProviderRowViewModel
{
    bool IsEnabled { get; set; }

    int Priority { get; set; }

    string PriorityLabel { get; set; }

    bool CanMoveUp { get; set; }

    bool CanMoveDown { get; set; }

    /// <summary>
    /// Dimming for disabled rows: they are out of the running and pinned to the bottom, so they
    /// should read as inactive rather than as just another entry.
    /// </summary>
    double RowOpacity { get; }

    IRelayCommand MoveUpCommand { get; }

    IRelayCommand MoveDownCommand { get; }
}

public partial class LyricsProviderSettingViewModel : ObservableObject, IProviderRowViewModel
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RowOpacity))]
    private bool _isEnabled;

    /// <summary>
    /// Search order derived from the row position, smaller runs first. Disabled rows keep the
    /// number they would occupy but are written as -1, which is how the downloader skips them.
    /// </summary>
    [ObservableProperty]
    private int _priority;

    /// <summary>
    /// What the list shows in the number column: the position, or a dash once the row is disabled
    /// and pushed to the bottom.
    /// </summary>
    [ObservableProperty]
    private string _priorityLabel = string.Empty;

    [ObservableProperty]
    private bool _canMoveUp;

    [ObservableProperty]
    private bool _canMoveDown;

    public double RowOpacity => IsEnabled ? 1.0 : 0.45;

    public IRelayCommand MoveUpCommand { get; set; } = new RelayCommand(() => { });

    public IRelayCommand MoveDownCommand { get; set; } = new RelayCommand(() => { });
}

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

    public double RowOpacity => IsEnabled ? 1.0 : 0.45;

    public IRelayCommand MoveUpCommand { get; set; } = new RelayCommand(() => { });

    public IRelayCommand MoveDownCommand { get; set; } = new RelayCommand(() => { });
}
