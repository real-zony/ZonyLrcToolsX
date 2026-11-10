using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.Lyrics;
using ZonyLrcTools.Common.MusicScanner;
using ZonyLrcTools.Common.TagInfo;
using ZonyLrcTools.Desktop.Infrastructure.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>
/// 歌词列表的数据来源。
/// </summary>
public enum LyricsScannerKind
{
    /// <summary>扫描本地音乐文件夹。</summary>
    Local = 0,

    /// <summary>读取歌曲与歌手信息的 CSV 文件。</summary>
    Csv = 1,

    /// <summary>读取网易云歌单。</summary>
    NetEase = 2
}

public partial class LyricsDownloadViewModel : ViewModelBase
{
    private const string DefaultOutputDirectoryName = "DownloadedLrc";
    private const string DefaultFileNamePattern = "{Artist} - {Name}.lrc";

    private readonly ILyricsDownloader _lyricsDownloader;
    private readonly IDialogService _dialogService;
    private readonly IFileScanner _fileScanner;
    private readonly ITagLoader _tagLoader;
    private readonly CsvFileMusicScanner _csvFileMusicScanner;
    private readonly NetEaseMusicSongListMusicScanner _neteaseMusicScanner;
    private readonly NetEaseSessionStore _sessionStore;
    private readonly GlobalOptions _options;
    private readonly IUILocalizationService? _localization;

    private CancellationTokenSource? _downloadCts;
    private readonly Dictionary<string, MusicFileViewModel> _musicFileMap = new();
    private readonly List<MusicInfo> _scannedMusicInfos = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    private string? _selectedFolderPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    private string? _selectedCsvFilePath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    private string? _songListIds;

    [ObservableProperty]
    private string? _outputDirectory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocalScanner))]
    [NotifyPropertyChangedFor(nameof(IsCsvScanner))]
    [NotifyPropertyChangedFor(nameof(IsNetEaseScanner))]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    private int _selectedScannerIndex;

    [ObservableProperty]
    private int _parallelCount = 2;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _failedCount;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDownloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectCsvFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectOutputFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignInNetEaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignOutNetEaseCommand))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    [NotifyPropertyChangedFor(nameof(CanEditInputs))]
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    private bool _isDownloading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDownloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectCsvFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectOutputFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignInNetEaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignOutNetEaseCommand))]
    [NotifyPropertyChangedFor(nameof(CanEditInputs))]
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    private bool _isScanning;

    [ObservableProperty]
    private int _scanProgressCount;

    [ObservableProperty]
    private int _scanTotalCount;

    [ObservableProperty]
    private double _scanProgressPercentage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NetEaseAccountStatus))]
    [NotifyCanExecuteChangedFor(nameof(SignOutNetEaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignInNetEaseCommand))]
    private bool _isNetEaseLoggedIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDownloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectCsvFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectOutputFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignInNetEaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(SignOutNetEaseCommand))]
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    [NotifyPropertyChangedFor(nameof(CanScan))]
    [NotifyPropertyChangedFor(nameof(CanEditInputs))]
    private bool _isAccountBusy;

    [ObservableProperty]
    private string? _currentProcessingFile;

    // 界面文案。
    public string LyricsTitle => _localization?["Lyrics_Title"] ?? "Lyrics Download";
    public string LyricsDescription => _localization?["Lyrics_Description"] ?? "Batch download lyrics for your music files";
    public string LyricsFolderLabel => _localization?["Lyrics_FolderLabel"] ?? "Music Folder";
    public string LyricsSelectFolder => _localization?["Lyrics_SelectFolder"] ?? "Select a folder containing music files...";
    public string LyricsBrowse => _localization?["Lyrics_Browse"] ?? "Browse...";
    public string LyricsParallel => _localization?["Lyrics_Parallel"] ?? "Parallel:";
    public string LyricsStartDownload => _localization?["Lyrics_StartDownload"] ?? "Start Download";
    public string LyricsStopDownload => _localization?["Lyrics_StopDownload"] ?? "Stop Download";
    public string LyricsScanning => _localization?["Lyrics_Status_Scanning"] ?? "Scanning files...";
    /// <summary>数据来源标签。</summary>
    public string LyricsScannerLabel => _localization?["Lyrics_Scanner"] ?? "Source";
    /// <summary>扫描按钮文案。</summary>
    public string LyricsScan => _localization?["Lyrics_Scan"] ?? "Scan";
    /// <summary>CSV 文件标签。</summary>
    public string LyricsCsvLabel => _localization?["Lyrics_CsvLabel"] ?? "CSV File";
    /// <summary>CSV 文件选择提示。</summary>
    public string LyricsCsvSelect => _localization?["Lyrics_CsvSelect"] ?? "Select a CSV file...";
    /// <summary>CSV 文件选择按钮文案。</summary>
    public string LyricsCsvBrowse => _localization?["Lyrics_CsvBrowse"] ?? "Choose...";
    /// <summary>网易云歌单编号标签。</summary>
    public string LyricsSongListLabel => _localization?["Lyrics_SongListLabel"] ?? "Playlist ID";
    /// <summary>网易云歌单编号输入提示。</summary>
    public string LyricsSongListWatermark => _localization?["Lyrics_SongListWatermark"] ?? "e.g. 7224428149 (separate multiple ids with ;)";
    /// <summary>输出目录标签。</summary>
    public string LyricsOutputLabel => _localization?["Lyrics_OutputLabel"] ?? "Save To";
    /// <summary>输出目录选择提示。</summary>
    public string LyricsOutputSelect => _localization?["Lyrics_OutputSelect"] ?? "Where the .lrc files should be written";
    /// <summary>输出目录选择按钮文案。</summary>
    public string LyricsOutputBrowse => _localization?["Lyrics_OutputBrowse"] ?? "Choose...";
    /// <summary>退出登录按钮文案。</summary>
    public string NetEaseSignOut => _localization?["NetEase_SignOut"] ?? "Sign out";
    /// <summary>退出登录提示。</summary>
    public string NetEaseSignOutHint => _localization?["NetEase_SignOutHint"] ??
                                        "Forget the saved session and scan the QR code again next time.";
    /// <summary>登录按钮文案。</summary>
    public string NetEaseSignIn => _localization?["NetEase_SignIn"] ?? "Sign in…";
    /// <summary>登录按钮提示。</summary>
    public string NetEaseSignInHint => _localization?["NetEase_SignInHint"] ??
                                       "Scan a QR code now instead of waiting until the next scan.";

    /// <summary>
    /// 当前网易云登录状态。
    /// </summary>
    public string NetEaseAccountStatus => IsNetEaseLoggedIn
        ? _localization?["NetEase_LoggedIn"] ?? "Signed in. Scanning a playlist reuses this session."
        : _localization?["NetEase_NotLoggedIn"] ?? "Not signed in. Scanning a playlist will show a QR code.";

    /// <summary>账号状态标签。</summary>
    public string NetEaseAccountLabel => _localization?["NetEase_AccountLabel"] ?? "Account";
    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    public string ColumnSongName => _localization?["Column_SongName"] ?? "Song Name";
    public string ColumnArtist => _localization?["Column_Artist"] ?? "Artist";
    public string ColumnFilePath => _localization?["Column_FilePath"] ?? "File Path";
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    /// <summary>
    /// 按 <see cref="SelectedScannerIndex"/> 排列的数据来源名称。
    /// </summary>
    public string[] ScannerNames =>
    [
        _localization?["Scanner_Local"] ?? "Local Folder",
        _localization?["Scanner_Csv"] ?? "CSV File",
        _localization?["Scanner_NetEase"] ?? "NetEase Playlist"
    ];

    public bool IsLocalScanner => SelectedScannerIndex == (int)LyricsScannerKind.Local;
    public bool IsCsvScanner => SelectedScannerIndex == (int)LyricsScannerKind.Csv;
    public bool IsNetEaseScanner => SelectedScannerIndex == (int)LyricsScannerKind.NetEase;

    public bool CanStartDownload => CanEditInputs && MusicFiles.Count > 0;

    public bool HasNoFiles => MusicFiles.Count == 0;

    public string EmptyStateText => _localization?["Lyrics_EmptyState"] ?? "No tasks yet, please select a folder to start scanning";

    public ObservableCollection<MusicFileViewModel> MusicFiles { get; } = new();

    public LyricsDownloadViewModel(
        ILyricsDownloader lyricsDownloader,
        IDialogService dialogService,
        IFileScanner fileScanner,
        ITagLoader tagLoader,
        CsvFileMusicScanner csvFileMusicScanner,
        NetEaseMusicSongListMusicScanner neteaseMusicScanner,
        NetEaseSessionStore sessionStore,
        IOptions<GlobalOptions> options,
        IUILocalizationService? localization = null)
    {
        _lyricsDownloader = lyricsDownloader;
        _dialogService = dialogService;
        _fileScanner = fileScanner;
        _tagLoader = tagLoader;
        _csvFileMusicScanner = csvFileMusicScanner;
        _neteaseMusicScanner = neteaseMusicScanner;
        _sessionStore = sessionStore;
        _options = options.Value;
        _localization = localization;

        OutputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            DefaultOutputDirectoryName);

        if (_localization != null)
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }

        // 扫描器为单例，页面直接复用已恢复的登录状态。
        IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;

        MusicFiles.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasNoFiles));
            OnPropertyChanged(nameof(CanStartDownload));
            StartDownloadCommand.NotifyCanExecuteChanged();
        };
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(LyricsTitle));
        OnPropertyChanged(nameof(LyricsDescription));
        OnPropertyChanged(nameof(LyricsFolderLabel));
        OnPropertyChanged(nameof(LyricsSelectFolder));
        OnPropertyChanged(nameof(LyricsBrowse));
        OnPropertyChanged(nameof(LyricsParallel));
        OnPropertyChanged(nameof(LyricsStartDownload));
        OnPropertyChanged(nameof(LyricsStopDownload));
        OnPropertyChanged(nameof(LyricsScanning));
        OnPropertyChanged(nameof(LyricsScannerLabel));
        OnPropertyChanged(nameof(LyricsScan));
        OnPropertyChanged(nameof(LyricsCsvLabel));
        OnPropertyChanged(nameof(LyricsCsvSelect));
        OnPropertyChanged(nameof(LyricsCsvBrowse));
        OnPropertyChanged(nameof(LyricsSongListLabel));
        OnPropertyChanged(nameof(LyricsSongListWatermark));
        OnPropertyChanged(nameof(LyricsOutputLabel));
        OnPropertyChanged(nameof(LyricsOutputSelect));
        OnPropertyChanged(nameof(LyricsOutputBrowse));
        OnPropertyChanged(nameof(NetEaseSignOut));
        OnPropertyChanged(nameof(NetEaseSignOutHint));
        OnPropertyChanged(nameof(NetEaseSignIn));
        OnPropertyChanged(nameof(NetEaseSignInHint));
        OnPropertyChanged(nameof(NetEaseAccountStatus));
        OnPropertyChanged(nameof(NetEaseAccountLabel));
        OnPropertyChanged(nameof(CommonTotal));
        OnPropertyChanged(nameof(CommonFiles));
        OnPropertyChanged(nameof(CommonSuccess));
        OnPropertyChanged(nameof(CommonFailed));
        OnPropertyChanged(nameof(ColumnSongName));
        OnPropertyChanged(nameof(ColumnArtist));
        OnPropertyChanged(nameof(ColumnFilePath));
        OnPropertyChanged(nameof(ColumnStatus));
        OnPropertyChanged(nameof(EmptyStateText));
        OnPropertyChanged(nameof(ScannerNames));
    }

    partial void OnSelectedScannerIndexChanged(int value) => InvalidateScan();
    partial void OnSelectedCsvFilePathChanged(string? value) => InvalidateScan();
    partial void OnSongListIdsChanged(string? value) => InvalidateScan();
    partial void OnOutputDirectoryChanged(string? value) => InvalidateScan();

    private void InvalidateScan()
    {
        if (CanEditInputs)
        {
            ResetCollections();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectFolderAsync(CancellationToken cancellationToken = default)
    {
        var folder = await _dialogService.ShowFolderPickerAsync(
            _localization?["Lyrics_SelectFolder"] ?? "Select Music Folder");
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(folder)) return;

        SelectedFolderPath = folder;
        await ScanFilesAsync(cancellationToken);
    }

    /// <summary>
    /// 是否可以修改任务来源与输出目录。
    /// </summary>
    public bool CanEditInputs => !IsScanning && !IsDownloading && !IsAccountBusy;

    private bool CanSelectFolder => CanEditInputs;

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectCsvFileAsync(CancellationToken cancellationToken = default)
    {
        var file = await _dialogService.ShowFilePickerAsync(
            _localization?["Lyrics_CsvSelect"] ?? "Select a CSV file", [".csv"]);
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(file)) return;

        SelectedCsvFilePath = file;
    }

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectOutputFolderAsync(CancellationToken cancellationToken = default)
    {
        var folder = await _dialogService.ShowFolderPickerAsync(
            _localization?["Lyrics_OutputSelect"] ?? "Select Output Folder");
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(folder)) return;

        OutputDirectory = folder;
    }


    /// <summary>
    /// 当前来源是否具备扫描所需的输入。
    /// </summary>
    public bool CanScan => CanEditInputs && (SelectedScannerIndex switch
    {
        (int)LyricsScannerKind.Local => !string.IsNullOrEmpty(SelectedFolderPath),
        (int)LyricsScannerKind.Csv => !string.IsNullOrEmpty(SelectedCsvFilePath),
        (int)LyricsScannerKind.NetEase => !string.IsNullOrWhiteSpace(SongListIds),
        _ => false
    });

    /// <summary>
    /// 使用当前来源重新读取歌曲列表。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanScan))]
    private async Task StartScanAsync(CancellationToken cancellationToken = default)
    {
        switch ((LyricsScannerKind)SelectedScannerIndex)
        {
            case LyricsScannerKind.Csv:
                await ScanCsvFileAsync(cancellationToken);
                break;
            case LyricsScannerKind.NetEase:
                await ScanNetEasePlaylistAsync(cancellationToken);
                break;
            default:
                await ScanFilesAsync(cancellationToken);
                break;
        }
    }

    private async Task ScanFilesAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        ResetCollections();
        IsScanning = true;

        try
        {
            var files = (await Task.Run(
                () => _fileScanner.ScanMusicFilesAsync(SelectedFolderPath, _options.SupportFileExtensions),
                cancellationToken)).ToList();

            if (files.Count == 0)
            {
                await ShowNoMusicFilesAsync(cancellationToken);
                return;
            }

            ScanTotalCount = files.Count;

            using var warpTask = new WarpTask(ParallelCount);
            var scanTasks = files.Select(file =>
                warpTask.RunAsync(async () =>
                {
                    var musicInfo = await Task.Run(() => _tagLoader.LoadTagAsync(file).AsTask(), cancellationToken);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (musicInfo != null &&
                            (!string.IsNullOrEmpty(musicInfo.Name) || !string.IsNullOrEmpty(musicInfo.Artist)))
                        {
                            AddRow(musicInfo);
                        }

                        ScanProgressCount++;
                        ScanProgressPercentage = ScanProgressCount * 100.0 / ScanTotalCount;
                    });
                }, cancellationToken));

            await Task.WhenAll(scanTasks);

            TotalCount = MusicFiles.Count;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消扫描。
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task ScanCsvFileAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(SelectedCsvFilePath)) return;

        ResetCollections();
        IsScanning = true;

        try
        {
            var outputDirectory = RequireOutputDirectory();
            var musicInfos = await Task.Run(() => _csvFileMusicScanner.GetMusicInfoFromCsvFileAsync(
                SelectedCsvFilePath, outputDirectory, DefaultFileNamePattern, cancellationToken), cancellationToken);

            AppendScannedMusicInfos(musicInfos);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消扫描。
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task ScanNetEasePlaylistAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(SongListIds)) return;

        ResetCollections();
        IsScanning = true;

        try
        {
            await RestoreNetEaseSessionAsync(cancellationToken);

            if (!_neteaseMusicScanner.IsLoggedIn && !await LoginToNetEaseAsync(cancellationToken))
            {
                return;
            }

            IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;

            // 扫码等待与歌单请求都要能被打断，否则 Cookie 恰好失效时这里会一直挂着。
            var musicInfos = await _neteaseMusicScanner.GetMusicInfoFromNetEaseMusicSongListAsync(
                SongListIds,
                RequireOutputDirectory(),
                DefaultFileNamePattern,
                onQrCodeReady: null,
                onStateChanged: null,
                cancellationToken: cancellationToken);

            await PersistNetEaseSessionAsync(cancellationToken);

            AppendScannedMusicInfos(musicInfos);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消扫描。
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
        }
        finally
        {
            IsScanning = false;

            // 无论登录成功还是取消，都以扫描器中的状态为准。
            IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;
        }
    }

    /// <summary>
    /// 恢复已保存的网易云登录状态。
    /// </summary>
    private async Task RestoreNetEaseSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_neteaseMusicScanner.IsLoggedIn) return;

        var session = await _sessionStore.LoadAsync(cancellationToken);
        if (session == null) return;

        _neteaseMusicScanner.RestoreLoginState(session.Cookie, session.CsrfToken);
    }

    private async Task PersistNetEaseSessionAsync(CancellationToken cancellationToken = default)
    {
        if (!_neteaseMusicScanner.IsLoggedIn) return;

        await _sessionStore.SaveAsync(new NetEaseSession
        {
            Cookie = _neteaseMusicScanner.Cookie,
            CsrfToken = _neteaseMusicScanner.CsrfToken,
            SavedAtUtc = DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    /// <summary>
    /// 显示二维码并等待登录或窗口关闭。
    /// </summary>
    private async Task<bool> LoginToNetEaseAsync(CancellationToken cancellationToken = default)
    {
        var loginViewModel = new QrCodeLoginViewModel(_localization);
        using var loginCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        void OnCancelled(object? sender, EventArgs args) => loginCts.Cancel();
        loginViewModel.Cancelled += OnCancelled;

        var loginTask = Task.Run(async () =>
        {
            try
            {
                var expired = false;
                var isSuccess = await _neteaseMusicScanner.LoginAsync(
                    payload => Dispatcher.UIThread.Post(() => loginViewModel.SetPayload(payload)),
                    state =>
                    {
                        expired = state == NetEaseLoginState.Expired;
                        Dispatcher.UIThread.Post(() => loginViewModel.SetState(state));
                    },
                    loginCts.Token);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (isSuccess)
                    {
                        loginViewModel.Complete();
                    }
                    else if (!expired)
                    {
                        loginViewModel.SetError(_localization?["NetEase_LoginFailed"] ??
                            "Login did not complete. Close this dialog and try again.");
                    }
                });

                return isSuccess;
            }
            catch (OperationCanceledException) when (loginCts.IsCancellationRequested)
            {
                return false;
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() => loginViewModel.SetError(ex.Message));
                return false;
            }
        });

        try
        {
            await _dialogService.ShowQrCodeLoginDialogAsync(loginViewModel, cancellationToken);
        }
        finally
        {
            loginViewModel.NotifyClosed();
            loginCts.Cancel();
            await loginTask;
            loginViewModel.Cancelled -= OnCancelled;
        }

        var isSuccess = await loginTask;
        if (isSuccess)
        {
            await PersistNetEaseSessionAsync(cancellationToken);
        }

        return isSuccess;
    }


    private void ResetCollections()
    {
        MusicFiles.Clear();
        _musicFileMap.Clear();
        _scannedMusicInfos.Clear();
        ScanProgressCount = 0;
        ScanTotalCount = 0;
        ScanProgressPercentage = 0;
        TotalCount = 0;
        CompletedCount = 0;
        FailedCount = 0;
        ProgressPercentage = 0;
    }

    private void AddRow(MusicInfo musicInfo)
    {
        if (_musicFileMap.ContainsKey(musicInfo.FilePath)) return;

        var vm = new MusicFileViewModel
        {
            FilePath = musicInfo.FilePath,
            Name = musicInfo.Name,
            Artist = musicInfo.Artist,
            StatusMessage = _localization?["Status_Pending"] ?? "Pending"
        };

        MusicFiles.Add(vm);
        _musicFileMap[musicInfo.FilePath] = vm;
        _scannedMusicInfos.Add(musicInfo);
    }

    /// <summary>
    /// 将 CSV 或网易云扫描结果加入列表。
    /// </summary>
    private void AppendScannedMusicInfos(List<MusicInfo> musicInfos)
    {
        foreach (var musicInfo in musicInfos.Where(info => !string.IsNullOrEmpty(info.Name)))
        {
            AddRow(musicInfo);
        }

        ScanTotalCount = MusicFiles.Count;
        ScanProgressCount = ScanTotalCount;
        ScanProgressPercentage = 100;
        TotalCount = MusicFiles.Count;
    }

    private string RequireOutputDirectory()
    {
        if (!string.IsNullOrWhiteSpace(OutputDirectory))
        {
            return OutputDirectory;
        }

        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            DefaultOutputDirectoryName);
        OutputDirectory = fallback;

        return fallback;
    }

    private Task ShowNoMusicFilesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _dialogService.ShowMessageAsync(
            _localization?["Common_Info"] ?? "Info",
            _localization?["Error_NoMusicFiles"] ?? "No music files found in the selected folder.");
    }

    private Task ShowErrorAsync(string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _dialogService.ShowMessageAsync(_localization?["Common_Error"] ?? "Error", message);
    }


    [RelayCommand(CanExecute = nameof(CanStartDownload))]
    private async Task StartDownloadAsync(CancellationToken cancellationToken = default)
    {
        if (_scannedMusicInfos.Count == 0) return;

        IsDownloading = true;
        CompletedCount = 0;
        FailedCount = 0;
        ProgressPercentage = 0;
        TotalCount = _scannedMusicInfos.Count;

        foreach (var vm in MusicFiles)
        {
            vm.IsProcessed = false;
            vm.IsSuccessful = false;
            vm.StatusMessage = _localization?["Status_Pending"] ?? "Pending";
        }

        _downloadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await _lyricsDownloader.DownloadAsync(
                _scannedMusicInfos,
                ParallelCount,
                async musicInfo =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (_musicFileMap.TryGetValue(musicInfo.FilePath, out var vm))
                        {
                            vm.IsProcessed = true;
                            vm.IsSuccessful = musicInfo.IsSuccessful;

                            if (musicInfo.IsPruneMusic)
                            {
                                vm.StatusMessage = _localization?["Status_Skipped"] ?? "Skipped";
                            }
                            else if (musicInfo.IsSuccessful)
                            {
                                vm.StatusMessage = _localization?["Status_Success"] ?? "Success";
                            }
                            else
                            {
                                vm.StatusMessage = _localization?["Status_Failed"] ?? "Failed";
                            }
                        }

                        if (musicInfo.IsSuccessful)
                            CompletedCount++;
                        else
                            FailedCount++;

                        ProgressPercentage = (CompletedCount + FailedCount) * 100.0 / TotalCount;
                    });
                },
                _downloadCts.Token);
        }
        catch (OperationCanceledException) when (_downloadCts.IsCancellationRequested)
        {
            // 用户取消下载。
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
        }
        finally
        {
            IsDownloading = false;
            _downloadCts?.Dispose();
            _downloadCts = null;
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        _downloadCts?.Cancel();
    }

    /// <summary>
    /// 清除已保存的网易云登录状态。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSignOutNetEase))]
    private async Task SignOutNetEaseAsync(CancellationToken cancellationToken = default)
    {
        IsAccountBusy = true;
        try
        {
            await _sessionStore.ClearAsync(cancellationToken);
            _neteaseMusicScanner.ClearLoginState();
            await _dialogService.ShowMessageAsync(
                _localization?["Common_Info"] ?? "Info",
                _localization?["NetEase_SignedOut"] ?? "Signed out. The next playlist scan will ask for a QR login.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消操作。
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
        }
        finally
        {
            IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;
            IsAccountBusy = false;
        }
    }

    private bool CanSignInNetEase => !IsNetEaseLoggedIn && CanEditInputs;
    private bool CanSignOutNetEase => IsNetEaseLoggedIn && CanEditInputs;

    /// <summary>
    /// 主动发起网易云扫码登录。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSignInNetEase))]
    private async Task SignInNetEaseAsync(CancellationToken cancellationToken = default)
    {
        IsAccountBusy = true;
        try
        {
            await LoginToNetEaseAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消登录。
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
        }
        finally
        {
            IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;
            IsAccountBusy = false;
        }
    }

    /// <summary>
    /// 页面打开时恢复登录状态。
    /// </summary>
    public override async Task OnActivatedAsync()
    {
        if (!CanEditInputs) return;

        IsAccountBusy = true;
        try
        {
            await RestoreNetEaseSessionAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;
            IsAccountBusy = false;
        }
    }
}

public partial class MusicFileViewModel : ObservableObject
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _artist = string.Empty;

    [ObservableProperty]
    private bool _isProcessed;

    [ObservableProperty]
    private bool _isSuccessful;

    [ObservableProperty]
    private string? _statusMessage;
}
