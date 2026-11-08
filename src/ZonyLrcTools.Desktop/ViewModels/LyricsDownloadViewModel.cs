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
/// Which source the lyrics page reads its song list from.
/// </summary>
public enum LyricsScannerKind
{
    /// <summary>Scan a local folder for audio files.</summary>
    Local = 0,

    /// <summary>Read a CSV file containing "song,artist" rows.</summary>
    Csv = 1,

    /// <summary>Pull a NetEase Cloud Music playlist by id.</summary>
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
    private CancellationTokenSource? _scanCts;
    private readonly Dictionary<string, MusicFileViewModel> _musicFileMap = new();
    private List<MusicInfo> _scannedMusicInfos = new();

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
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    private bool _isDownloading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDownloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectCsvFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectOutputFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartScanCommand))]
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
    private string? _currentProcessingFile;

    // Localized strings
    public string LyricsTitle => _localization?["Lyrics_Title"] ?? "Lyrics Download";
    public string LyricsDescription => _localization?["Lyrics_Description"] ?? "Batch download lyrics for your music files";
    public string LyricsFolderLabel => _localization?["Lyrics_FolderLabel"] ?? "Music Folder";
    public string LyricsSelectFolder => _localization?["Lyrics_SelectFolder"] ?? "Select a folder containing music files...";
    public string LyricsBrowse => _localization?["Lyrics_Browse"] ?? "Browse...";
    public string LyricsParallel => _localization?["Lyrics_Parallel"] ?? "Parallel:";
    public string LyricsStartDownload => _localization?["Lyrics_StartDownload"] ?? "Start Download";
    public string LyricsStopDownload => _localization?["Lyrics_StopDownload"] ?? "Stop Download";
    public string LyricsScanning => _localization?["Lyrics_Status_Scanning"] ?? "Scanning files...";
    public string LyricsScannerLabel => _localization?["Lyrics_Scanner"] ?? "Source";
    public string LyricsScan => _localization?["Lyrics_Scan"] ?? "Scan";
    public string LyricsCsvLabel => _localization?["Lyrics_CsvLabel"] ?? "CSV File";
    public string LyricsCsvSelect => _localization?["Lyrics_CsvSelect"] ?? "Select a CSV file...";
    public string LyricsCsvBrowse => _localization?["Lyrics_CsvBrowse"] ?? "Choose...";
    public string LyricsSongListLabel => _localization?["Lyrics_SongListLabel"] ?? "Playlist ID";
    public string LyricsSongListWatermark => _localization?["Lyrics_SongListWatermark"] ?? "e.g. 7224428149 (separate multiple ids with ;)";
    public string LyricsOutputLabel => _localization?["Lyrics_OutputLabel"] ?? "Save To";
    public string LyricsOutputSelect => _localization?["Lyrics_OutputSelect"] ?? "Where the .lrc files should be written";
    public string LyricsOutputBrowse => _localization?["Lyrics_OutputBrowse"] ?? "Choose...";
    public string NetEaseSignOut => _localization?["NetEase_SignOut"] ?? "Sign out";
    public string NetEaseSignOutHint => _localization?["NetEase_SignOutHint"] ??
                                        "Forget the saved session and scan the QR code again next time.";
    public string NetEaseSignIn => _localization?["NetEase_SignIn"] ?? "Sign in…";
    public string NetEaseSignInHint => _localization?["NetEase_SignInHint"] ??
                                       "Scan a QR code now instead of waiting until the next scan.";

    /// <summary>
    /// Human readable login state. The sign in / sign out buttons alone were not enough: a user
    /// who had never signed in could not tell whether a session was stored, and a user who had
    /// signed in had no way to notice the cookie was restored from disk.
    /// </summary>
    public string NetEaseAccountStatus => IsNetEaseLoggedIn
        ? _localization?["NetEase_LoggedIn"] ?? "Signed in. Scanning a playlist reuses this session."
        : _localization?["NetEase_NotLoggedIn"] ?? "Not signed in. Scanning a playlist will show a QR code.";

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
    /// Scanner display names, indexed the same way as <see cref="SelectedScannerIndex"/>.
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

    public bool CanStartDownload => !IsDownloading && !IsScanning && MusicFiles.Count > 0;

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

        // The scanner is a singleton, so a session restored by an earlier page visit is already
        // in memory; reflect it right away instead of waiting for the first activation.
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

    // ---------------------------------------------------------------- selection commands

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectFolderAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync(
            _localization?["Lyrics_SelectFolder"] ?? "Select Music Folder");
        if (string.IsNullOrEmpty(folder)) return;

        SelectedFolderPath = folder;
        await ScanFilesAsync();
    }

    private bool CanSelectFolder => !IsScanning && !IsDownloading;

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectCsvFileAsync()
    {
        var file = await _dialogService.ShowFilePickerAsync(
            _localization?["Lyrics_CsvSelect"] ?? "Select a CSV file", [".csv"]);
        if (string.IsNullOrEmpty(file)) return;

        SelectedCsvFilePath = file;
    }

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectOutputFolderAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync(
            _localization?["Lyrics_OutputSelect"] ?? "Select Output Folder");
        if (string.IsNullOrEmpty(folder)) return;

        OutputDirectory = folder;
    }

    // ---------------------------------------------------------------- scanning

    /// <summary>
    /// Whether the current source has enough input to start reading a song list.
    /// </summary>
    public bool CanScan => !IsScanning && !IsDownloading && SelectedScannerIndex switch
    {
        (int)LyricsScannerKind.Local => !string.IsNullOrEmpty(SelectedFolderPath),
        (int)LyricsScannerKind.Csv => !string.IsNullOrEmpty(SelectedCsvFilePath),
        (int)LyricsScannerKind.NetEase => !string.IsNullOrWhiteSpace(SongListIds),
        _ => false
    };

    /// <summary>
    /// Re-read the song list using the currently selected source.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanScan))]
    private async Task StartScanAsync()
    {
        switch ((LyricsScannerKind)SelectedScannerIndex)
        {
            case LyricsScannerKind.Csv:
                await ScanCsvFileAsync();
                break;
            case LyricsScannerKind.NetEase:
                await ScanNetEasePlaylistAsync();
                break;
            default:
                await ScanFilesAsync();
                break;
        }
    }

    private async Task ScanFilesAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        ResetCollections();
        IsScanning = true;

        try
        {
            var files = (await _fileScanner.ScanMusicFilesAsync(SelectedFolderPath, _options.SupportFileExtensions))
                .ToList();

            if (files.Count == 0)
            {
                await ShowNoMusicFilesAsync();
                return;
            }

            ScanTotalCount = files.Count;

            using var warpTask = new WarpTask(ParallelCount);
            var scanTasks = files.Select(file =>
                warpTask.RunAsync(async () =>
                {
                    var musicInfo = await _tagLoader.LoadTagAsync(file);

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
                }));

            await Task.WhenAll(scanTasks);

            TotalCount = MusicFiles.Count;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task ScanCsvFileAsync()
    {
        if (string.IsNullOrEmpty(SelectedCsvFilePath)) return;

        ResetCollections();
        IsScanning = true;

        try
        {
            var musicInfos = await _csvFileMusicScanner.GetMusicInfoFromCsvFileAsync(
                SelectedCsvFilePath,
                RequireOutputDirectory(),
                DefaultFileNamePattern);

            AppendScannedMusicInfos(musicInfos);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task ScanNetEasePlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(SongListIds)) return;

        ResetCollections();
        IsScanning = true;

        using var scanCts = new CancellationTokenSource();
        _scanCts = scanCts;

        try
        {
            await RestoreNetEaseSessionAsync();

            if (!_neteaseMusicScanner.IsLoggedIn && !await LoginToNetEaseAsync())
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
                cancellationToken: scanCts.Token);

            await PersistNetEaseSessionAsync();

            AppendScannedMusicInfos(musicInfos);
        }
        catch (OperationCanceledException)
        {
            // User closed the login dialog.
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
        finally
        {
            _scanCts = null;
            IsScanning = false;

            // Whatever happened above - scan ok, user cancelled the QR dialog, request failed -
            // the account row must reflect the scanner's real state. Without this the sign-in
            // button kept saying "Sign out" after a successful login from the scan flow.
            IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;
        }
    }

    /// <summary>
    /// Load a previously saved NetEase session so the QR dialog only appears on first use.
    /// </summary>
    private async Task RestoreNetEaseSessionAsync()
    {
        if (_neteaseMusicScanner.IsLoggedIn) return;

        var session = await _sessionStore.LoadAsync();
        if (session == null) return;

        _neteaseMusicScanner.RestoreLoginState(session.Cookie, session.CsrfToken);
    }

    private async Task PersistNetEaseSessionAsync()
    {
        if (!_neteaseMusicScanner.IsLoggedIn) return;

        await _sessionStore.SaveAsync(new NetEaseSession
        {
            Cookie = _neteaseMusicScanner.Cookie,
            CsrfToken = _neteaseMusicScanner.CsrfToken,
            SavedAt = DateTimeOffset.Now
        });
    }

    /// <summary>
    /// Show the QR dialog and wait for the scan-and-confirm flow to finish.
    /// </summary>
    private async Task<bool> LoginToNetEaseAsync()
    {
        var loginViewModel = new QrCodeLoginViewModel(_localization);
        using var loginCts = new CancellationTokenSource();

        loginViewModel.Cancelled += (_, _) => loginCts.Cancel();

        var loginTask = Task.Run(async () =>
        {
            try
            {
                var isSuccess = await _neteaseMusicScanner.LoginAsync(
                    payload => Dispatcher.UIThread.Post(() => loginViewModel.SetPayload(payload)),
                    state => Dispatcher.UIThread.Post(() => loginViewModel.SetState(state)),
                    loginCts.Token);

                if (isSuccess)
                {
                    Dispatcher.UIThread.Post(() => loginViewModel.Complete());
                }

                return isSuccess;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() => loginViewModel.SetError(ex.Message));
                return false;
            }
        });

        // The dialog closes itself once the flow calls Complete(), so this await returns as soon
        // as the user either confirms on the phone or gives up.
        await _dialogService.ShowQrCodeLoginDialogAsync(loginViewModel);

        var isSuccess = await loginTask;
        if (isSuccess)
        {
            await PersistNetEaseSessionAsync();
        }

        return isSuccess;
    }

    // ---------------------------------------------------------------- shared helpers

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
    /// Fill the grid from a scanner that returns its list in one shot (CSV, NetEase).
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

    private Task ShowNoMusicFilesAsync()
    {
        return _dialogService.ShowMessageAsync(
            _localization?["Common_Info"] ?? "Info",
            _localization?["Error_NoMusicFiles"] ?? "No music files found in the selected folder.");
    }

    private Task ShowErrorAsync(string message)
    {
        return _dialogService.ShowMessageAsync(_localization?["Common_Error"] ?? "Error", message);
    }

    // ---------------------------------------------------------------- download

    [RelayCommand(CanExecute = nameof(CanStartDownload))]
    private async Task StartDownloadAsync()
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

        _downloadCts = new CancellationTokenSource();

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
        catch (OperationCanceledException)
        {
            // User cancelled
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
    /// Clears the saved NetEase session so the next playlist scan asks for a fresh QR login.
    /// <remarks>
    /// Without this the only way to recover from an expired Cookie was to delete
    /// <c>netease.session.json</c> by hand.
    /// </remarks>
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsNetEaseLoggedIn))]
    private async Task SignOutNetEaseAsync()
    {
        _neteaseMusicScanner.ClearLoginState();
        await _sessionStore.ClearAsync();
        IsNetEaseLoggedIn = false;
        await _dialogService.ShowMessageAsync(
            _localization?["Common_Info"] ?? "Info",
            _localization?["NetEase_SignedOut"] ?? "Signed out. The next playlist scan will ask for a QR login.");
    }

    private bool CanSignInNetEase => !IsNetEaseLoggedIn && !IsScanning && !IsDownloading;

    /// <summary>
    /// Let the user sign in from the page itself instead of discovering the QR dialog only after
    /// pressing "Scan" - the button used to be hidden behind one specific scanner choice.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSignInNetEase))]
    private async Task SignInNetEaseAsync()
    {
        if (await LoginToNetEaseAsync())
        {
            IsNetEaseLoggedIn = true;
        }
    }

    /// <summary>
    /// Refresh the visible login state. Called on construction and every time the page is shown,
    /// because the session may have been written by an earlier run of the app.
    /// </summary>
    private async Task RefreshNetEaseLoginStateAsync()
    {
        await RestoreNetEaseSessionAsync();
        IsNetEaseLoggedIn = _neteaseMusicScanner.IsLoggedIn;
        SignOutNetEaseCommand.NotifyCanExecuteChanged();
        SignInNetEaseCommand.NotifyCanExecuteChanged();
    }

    public override async Task OnActivatedAsync()
    {
        await base.OnActivatedAsync();
        await RefreshNetEaseLoginStateAsync();
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
