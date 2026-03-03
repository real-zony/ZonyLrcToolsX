using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class AlbumDownloadViewModel : ViewModelBase
{
    private readonly IAlbumDownloader _albumDownloader;
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
    [NotifyPropertyChangedFor(nameof(CanStartDownload))]
    private bool _isDownloading;

    // Localized strings
    public string AlbumTitle => _localization?["Album_Title"] ?? "Album Cover Download";
    public string AlbumDescription => _localization?["Album_Description"] ?? "Download album artwork for your music collection";
    public string AlbumFolderLabel => _localization?["Album_FolderLabel"] ?? "Music Folder";
    public string AlbumSelectFolder => _localization?["Album_SelectFolder"] ?? "Select a folder containing music files...";
    public string AlbumBrowse => _localization?["Album_Browse"] ?? "Browse...";
    public string AlbumParallel => _localization?["Album_Parallel"] ?? "Parallel:";
    public string AlbumStartDownload => _localization?["Album_StartDownload"] ?? "Start Download";
    public string AlbumStopDownload => _localization?["Album_StopDownload"] ?? "Stop Download";
    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    public string ColumnSongName => _localization?["Column_SongName"] ?? "Song Name";
    public string ColumnArtist => _localization?["Column_Artist"] ?? "Artist";
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    public bool CanStartDownload => !IsDownloading && !string.IsNullOrEmpty(SelectedFolderPath);

    public bool HasNoFiles => MusicFiles.Count == 0;

    public string EmptyStateText => _localization?["Album_EmptyState"] ?? "No tasks yet, please select a folder to start scanning";

    public ObservableCollection<MusicFileViewModel> MusicFiles { get; } = new();

    public AlbumDownloadViewModel(
        IAlbumDownloader albumDownloader,
        IDialogService dialogService,
        IUILocalizationService? localization = null)
    {
        _albumDownloader = albumDownloader;
        _dialogService = dialogService;
        _localization = localization;

        if (_localization != null)
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }

        MusicFiles.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasNoFiles));
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
        OnPropertyChanged(nameof(CommonTotal));
        OnPropertyChanged(nameof(CommonFiles));
        OnPropertyChanged(nameof(CommonSuccess));
        OnPropertyChanged(nameof(CommonFailed));
        OnPropertyChanged(nameof(ColumnSongName));
        OnPropertyChanged(nameof(ColumnArtist));
        OnPropertyChanged(nameof(ColumnStatus));
        OnPropertyChanged(nameof(EmptyStateText));
    }

    [RelayCommand]
    private async Task SelectFolderAsync()
    {
        var folder = await _dialogService.ShowFolderPickerAsync("Select Music Folder");
        if (string.IsNullOrEmpty(folder)) return;

        SelectedFolderPath = folder;
        OnPropertyChanged(nameof(CanStartDownload));
    }

    [RelayCommand]
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
            // TODO: Implement actual download logic using _albumDownloader
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
