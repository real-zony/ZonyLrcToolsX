using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.TagInfo;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class AlbumDownloadViewModel : ViewModelBase
{
    private readonly IAlbumDownloader _albumDownloader;
    private readonly IDialogService _dialogService;
    private readonly IFileScanner _fileScanner;
    private readonly ITagLoader _tagLoader;
    private readonly GlobalOptions _options;
    private readonly IUILocalizationService? _localization;

    private CancellationTokenSource? _downloadCts;
    private readonly Dictionary<string, AlbumFileViewModel> _musicFileMap = new();
    private readonly List<MusicInfo> _scannedMusicInfos = new();

    [ObservableProperty]
    private string? _selectedFolderPath;

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
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    private bool _isDownloading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartDownloadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFolderCommand))]
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    private bool _isScanning;

    [ObservableProperty]
    private int _scanProgressCount;

    [ObservableProperty]
    private int _scanTotalCount;

    [ObservableProperty]
    private double _scanProgressPercentage;

    // Localized strings
    public string AlbumTitle => _localization?["Album_Title"] ?? "Album Cover Download";
    public string AlbumDescription => _localization?["Album_Description"] ?? "Download album artwork for your music collection";
    public string AlbumFolderLabel => _localization?["Album_FolderLabel"] ?? "Music Folder";
    public string AlbumSelectFolder => _localization?["Album_SelectFolder"] ?? "Select a folder containing music files...";
    public string AlbumBrowse => _localization?["Album_Browse"] ?? "Browse...";
    public string AlbumParallel => _localization?["Album_Parallel"] ?? "Parallel:";
    public string AlbumStartDownload => _localization?["Album_StartDownload"] ?? "Start Download";
    public string AlbumStopDownload => _localization?["Album_StopDownload"] ?? "Stop Download";
    /// <summary>扫描进度提示。</summary>
    public string AlbumScanning => _localization?["Lyrics_Status_Scanning"] ?? "Scanning files...";
    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    public string ColumnSongName => _localization?["Column_SongName"] ?? "Song Name";
    public string ColumnArtist => _localization?["Column_Artist"] ?? "Artist";
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    public bool CanStartDownload => !IsDownloading && !IsScanning && MusicFiles.Count > 0;

    public bool HasNoFiles => MusicFiles.Count == 0;

    public string EmptyStateText => _localization?["Album_EmptyState"] ?? "No tasks yet, please select a folder to start scanning";

    public ObservableCollection<AlbumFileViewModel> MusicFiles { get; } = new();

    public AlbumDownloadViewModel(
        IAlbumDownloader albumDownloader,
        IDialogService dialogService,
        IFileScanner fileScanner,
        ITagLoader tagLoader,
        IOptions<GlobalOptions> options,
        IUILocalizationService? localization = null)
    {
        _albumDownloader = albumDownloader;
        _dialogService = dialogService;
        _fileScanner = fileScanner;
        _tagLoader = tagLoader;
        _options = options.Value;
        _localization = localization;

        if (_localization != null)
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }

        MusicFiles.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasNoFiles));
            OnPropertyChanged(nameof(CanStartDownload));
            StartDownloadCommand.NotifyCanExecuteChanged();
        };
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(AlbumTitle));
        OnPropertyChanged(nameof(AlbumDescription));
        OnPropertyChanged(nameof(AlbumFolderLabel));
        OnPropertyChanged(nameof(AlbumSelectFolder));
        OnPropertyChanged(nameof(AlbumBrowse));
        OnPropertyChanged(nameof(AlbumParallel));
        OnPropertyChanged(nameof(AlbumStartDownload));
        OnPropertyChanged(nameof(AlbumStopDownload));
        OnPropertyChanged(nameof(AlbumScanning));
        OnPropertyChanged(nameof(CommonTotal));
        OnPropertyChanged(nameof(CommonFiles));
        OnPropertyChanged(nameof(CommonSuccess));
        OnPropertyChanged(nameof(CommonFailed));
        OnPropertyChanged(nameof(ColumnSongName));
        OnPropertyChanged(nameof(ColumnArtist));
        OnPropertyChanged(nameof(ColumnStatus));
        OnPropertyChanged(nameof(EmptyStateText));
    }

    [RelayCommand(CanExecute = nameof(CanSelectFolder))]
    private async Task SelectFolderAsync(CancellationToken cancellationToken = default)
    {
        var folder = await _dialogService.ShowFolderPickerAsync(
            _localization?["Album_SelectFolder"] ?? "Select Music Folder");
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(folder)) return;

        SelectedFolderPath = folder;
        await ScanFilesAsync(cancellationToken);
    }

    private bool CanSelectFolder => !IsScanning && !IsDownloading;

    /// <summary>
    /// 扫描文件并读取音乐标签。
    /// </summary>
    private async Task ScanFilesAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        IsScanning = true;
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

        try
        {
            var files = (await Task.Run(
                () => _fileScanner.ScanMusicFilesAsync(SelectedFolderPath, _options.SupportFileExtensions),
                cancellationToken)).ToList();

            if (files.Count == 0)
            {
                await _dialogService.ShowMessageAsync(
                    _localization?["Common_Info"] ?? "Info",
                    _localization?["Error_NoMusicFiles"] ?? "No music files found in the selected folder.");
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
                            var vm = new AlbumFileViewModel
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
            await _dialogService.ShowMessageAsync(
                _localization?["Common_Error"] ?? "Error",
                ex.Message);
        }
        finally
        {
            IsScanning = false;
            OnPropertyChanged(nameof(CanStartDownload));
        }
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
            await _albumDownloader.DownloadAsync(
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
                            vm.StatusMessage = musicInfo.IsSuccessful
                                ? _localization?["Status_Success"] ?? "Success"
                                : _localization?["Status_Failed"] ?? "Failed";
                        }

                        if (musicInfo.IsSuccessful)
                            CompletedCount++;
                        else
                            FailedCount++;

                        var done = CompletedCount + FailedCount;
                        ProgressPercentage = done * 100.0 / TotalCount;
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
            await _dialogService.ShowMessageAsync(
                _localization?["Common_Error"] ?? "Error",
                ex.Message);
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
}

/// <summary>
/// 单个音乐文件的封面下载状态。
/// </summary>
public partial class AlbumFileViewModel : ObservableObject
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
