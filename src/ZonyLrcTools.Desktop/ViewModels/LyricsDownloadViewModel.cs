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
using ZonyLrcTools.Common.TagInfo;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class LyricsDownloadViewModel : ViewModelBase
{
    private readonly ILyricsDownloader _lyricsDownloader;
    private readonly IDialogService _dialogService;
    private readonly IFileScanner _fileScanner;
    private readonly ITagLoader _tagLoader;
    private readonly GlobalOptions _options;
    private readonly IUILocalizationService? _localization;

    private CancellationTokenSource? _downloadCts;
    private Dictionary<string, MusicFileViewModel> _musicFileMap = new();
    private List<MusicInfo> _scannedMusicInfos = new();

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

    [ObservableProperty]
    private string? _currentProcessingFile;

    // Localized strings
    public string LyricsTitle => _localization?["Lyrics_Title"] ?? "Lyrics Download";
    public string LyricsDescription => _localization?["Lyrics_Description"] ?? "Batch download lyrics for your music files";
    public string LyricsSelectFolder => _localization?["Lyrics_SelectFolder"] ?? "Select music folder...";
    public string LyricsBrowse => _localization?["Lyrics_Browse"] ?? "Browse...";
    public string LyricsParallel => _localization?["Lyrics_Parallel"] ?? "Parallel:";
    public string LyricsStartDownload => _localization?["Lyrics_StartDownload"] ?? "Start Download";
    public string LyricsStopDownload => _localization?["Lyrics_StopDownload"] ?? "Stop Download";
    public string LyricsScanning => _localization?["Lyrics_Status_Scanning"] ?? "Scanning files...";
    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    public string ColumnSongName => _localization?["Column_SongName"] ?? "Song Name";
    public string ColumnArtist => _localization?["Column_Artist"] ?? "Artist";
    public string ColumnFilePath => _localization?["Column_FilePath"] ?? "File Path";
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    public bool CanStartDownload => !IsDownloading && !IsScanning && MusicFiles.Count > 0;

    public ObservableCollection<MusicFileViewModel> MusicFiles { get; } = new();

    public LyricsDownloadViewModel(
        ILyricsDownloader lyricsDownloader,
        IDialogService dialogService,
        IFileScanner fileScanner,
        ITagLoader tagLoader,
        IOptions<GlobalOptions> options,
        IUILocalizationService? localization = null)
    {
        _lyricsDownloader = lyricsDownloader;
        _dialogService = dialogService;
        _fileScanner = fileScanner;
        _tagLoader = tagLoader;
        _options = options.Value;
        _localization = localization;

        if (_localization != null)
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(LyricsTitle));
        OnPropertyChanged(nameof(LyricsDescription));
        OnPropertyChanged(nameof(LyricsSelectFolder));
        OnPropertyChanged(nameof(LyricsBrowse));
        OnPropertyChanged(nameof(LyricsParallel));
        OnPropertyChanged(nameof(LyricsStartDownload));
        OnPropertyChanged(nameof(LyricsStopDownload));
        OnPropertyChanged(nameof(LyricsScanning));
        OnPropertyChanged(nameof(CommonTotal));
        OnPropertyChanged(nameof(CommonFiles));
        OnPropertyChanged(nameof(CommonSuccess));
        OnPropertyChanged(nameof(CommonFailed));
        OnPropertyChanged(nameof(ColumnSongName));
        OnPropertyChanged(nameof(ColumnArtist));
        OnPropertyChanged(nameof(ColumnFilePath));
        OnPropertyChanged(nameof(ColumnStatus));
    }

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

    private async Task ScanFilesAsync()
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
            var files = (await _fileScanner.ScanMusicFilesAsync(SelectedFolderPath, _options.SupportFileExtensions))
                .ToList();

            if (files.Count == 0)
            {
                await _dialogService.ShowMessageAsync(
                    _localization?["Common_Info"] ?? "Info",
                    _localization?["Error_NoMusicFiles"] ?? "No music files found in the selected folder.");
                IsScanning = false;
                OnPropertyChanged(nameof(CanStartDownload));
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

                        ScanProgressCount++;
                        ScanProgressPercentage = ScanProgressCount * 100.0 / ScanTotalCount;
                    });
                }));

            await Task.WhenAll(scanTasks);

            TotalCount = MusicFiles.Count;
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
    private async Task StartDownloadAsync()
    {
        if (_scannedMusicInfos.Count == 0) return;

        IsDownloading = true;
        CompletedCount = 0;
        FailedCount = 0;
        ProgressPercentage = 0;
        TotalCount = _scannedMusicInfos.Count;

        // Reset all row statuses
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
