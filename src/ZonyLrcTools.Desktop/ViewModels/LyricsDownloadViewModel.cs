using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Common.Lyrics;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class LyricsDownloadViewModel : ViewModelBase
{
    private readonly ILyricsDownloader _lyricsDownloader;
    private readonly IDialogService _dialogService;
    private readonly IUILocalizationService? _localization;

    private CancellationTokenSource? _downloadCts;

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
    private string? _currentProcessingFile;

    // Localized strings
    public string LyricsTitle => _localization?["Lyrics_Title"] ?? "Lyrics Download";
    public string LyricsDescription => _localization?["Lyrics_Description"] ?? "Batch download lyrics for your music files";
    public string LyricsSelectFolder => _localization?["Lyrics_SelectFolder"] ?? "Select music folder...";
    public string LyricsBrowse => _localization?["Lyrics_Browse"] ?? "Browse...";
    public string LyricsParallel => _localization?["Lyrics_Parallel"] ?? "Parallel:";
    public string LyricsStartDownload => _localization?["Lyrics_StartDownload"] ?? "Start Download";
    public string LyricsStopDownload => _localization?["Lyrics_StopDownload"] ?? "Stop Download";
    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    public string ColumnSongName => _localization?["Column_SongName"] ?? "Song Name";
    public string ColumnArtist => _localization?["Column_Artist"] ?? "Artist";
    public string ColumnFilePath => _localization?["Column_FilePath"] ?? "File Path";
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    public bool CanStartDownload => !IsDownloading && !string.IsNullOrEmpty(SelectedFolderPath);

    public ObservableCollection<MusicFileViewModel> MusicFiles { get; } = new();

    public LyricsDownloadViewModel(
        ILyricsDownloader lyricsDownloader,
        IDialogService dialogService,
        IUILocalizationService? localization = null)
    {
        _lyricsDownloader = lyricsDownloader;
        _dialogService = dialogService;
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
        OnPropertyChanged(nameof(CommonTotal));
        OnPropertyChanged(nameof(CommonFiles));
        OnPropertyChanged(nameof(CommonSuccess));
        OnPropertyChanged(nameof(CommonFailed));
        OnPropertyChanged(nameof(ColumnSongName));
        OnPropertyChanged(nameof(ColumnArtist));
        OnPropertyChanged(nameof(ColumnFilePath));
        OnPropertyChanged(nameof(ColumnStatus));
    }

    [RelayCommand]
    private async Task SelectFolderAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Music Folder");
        if (string.IsNullOrEmpty(folder)) return;

        SelectedFolderPath = folder;
        OnPropertyChanged(nameof(CanStartDownload));
    }

    [RelayCommand(CanExecute = nameof(CanStartDownload))]
    private async Task StartDownloadAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        IsDownloading = true;
        CompletedCount = 0;
        FailedCount = 0;
        ProgressPercentage = 0;

        _downloadCts = new CancellationTokenSource();

        try
        {
            // TODO: Implement actual download logic using _lyricsDownloader
            await Task.Delay(1000, _downloadCts.Token); // Placeholder
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
