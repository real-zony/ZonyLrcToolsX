using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZonyLrcTools.Common.Infrastructure.Threading;
using ZonyLrcTools.Common.MusicDecryption;
using ZonyLrcTools.Desktop.Infrastructure.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.Services;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class DecryptViewModel : ViewModelBase
{
    private const string KggExtension = ".kgg";

    private readonly IMusicDecryptor _musicDecryptor;
    private readonly IDialogService _dialogService;
    private readonly IUILocalizationService? _localization;
    private readonly IKugouKeyProvider? _kugouKeyProvider;
    private readonly KugouDatabaseStore? _kugouDatabaseStore;

    private CancellationTokenSource? _decryptCts;
    private bool _isInitialized;

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
    [NotifyCanExecuteChangedFor(nameof(StartDecryptCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearFilesCommand))]
    [NotifyPropertyChangedFor(nameof(CanStartDecrypt))]
    [NotifyPropertyChangedFor(nameof(CanClear))]
    private bool _isDecrypting;

    /// <summary>
    /// True when the KuGou key database could not be read. kgg files cannot be decrypted without
    /// it, so the page says so up front instead of letting every row fail with a wall of text.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDatabaseWarning))]
    private bool _isKugouDatabaseAvailable;

    [ObservableProperty]
    private string _kugouDatabaseStatus = string.Empty;

    // Localized strings
    public string DecryptTitle => _localization?["Decrypt_Title"] ?? "Music Decryption";
    public string DecryptDescription => _localization?["Decrypt_Description"] ?? "Convert encrypted music files (ncm / qmc / kgm / kgg / kwm) into playable formats";
    public string DecryptSelectFiles => _localization?["Decrypt_SelectFiles"] ?? "Select encrypted music files...";
    public string DecryptBrowse => _localization?["Decrypt_Browse"] ?? "Add Files...";
    public string DecryptParallel => _localization?["Decrypt_Parallel"] ?? "Parallel:";
    public string DecryptStart => _localization?["Decrypt_Start"] ?? "Start Decrypt";
    public string DecryptStop => _localization?["Decrypt_Stop"] ?? "Stop";
    public string DecryptEmptyState => _localization?["Decrypt_EmptyState"] ?? "No files yet, add files to start decrypting";
    public string DecryptClear => _localization?["Decrypt_Clear"] ?? "Clear";
    public string DecryptFilesLabel => _localization?["Decrypt_FilesLabel"] ?? "Files";
    public string DecryptSpecifyDatabase => _localization?["Decrypt_SpecifyDatabase"] ?? "Locate KGMusicV3.db...";
    public string DecryptStatusMissingDb => _localization?["Decrypt_StatusMissingDb"] ?? "KuGou database missing";
    public string DecryptViewDetail => _localization?["Decrypt_ViewDetail"] ?? "Details";
    public string DecryptFailureTitle => _localization?["Decrypt_FailureTitle"] ?? "Decryption failure";

    /// <summary>
    /// Explanation shown in the warning strip. Kept as one sentence because the real condition is
    /// simple: kgg needs the KuGou database, and that database only holds keys for songs KuGou
    /// itself downloaded on this machine.
    /// </summary>
    public string DecryptKugouRequirement => _localization?["Decrypt_KugouMissing"]
        ?? "kgg decryption needs the KuGou database KGMusicV3.db (the key is only written when KuGou downloaded that song)";

    public bool ShowDatabaseWarning => !IsKugouDatabaseAvailable;

    /// <summary>
    /// Read-only summary shown in the picker text box: "3 files selected" rather than a path,
    /// because this page takes a list, not a single location.
    /// </summary>
    public string SelectionSummary => Files.Count == 0
        ? string.Empty
        : $"{Files.Count} {CommonFiles}";

    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    public string ColumnFileName => _localization?["Column_FileName"] ?? "File Name";
    public string ColumnOutputPath => _localization?["Column_OutputPath"] ?? "Output";
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    // 全部行都被 blocked 时按钮不该亮着，否则点下去只会空转一次。
    public bool CanStartDecrypt => !IsDecrypting && Files.Any(f => !f.IsBlocked);
    public bool HasNoFiles => Files.Count == 0;

    public ObservableCollection<DecryptFileViewModel> Files { get; } = new();

    public DecryptViewModel(
        IMusicDecryptor musicDecryptor,
        IDialogService dialogService,
        IUILocalizationService? localization = null,
        IKugouKeyProvider? kugouKeyProvider = null,
        KugouDatabaseStore? kugouDatabaseStore = null)
    {
        _musicDecryptor = musicDecryptor;
        _dialogService = dialogService;
        _localization = localization;
        _kugouKeyProvider = kugouKeyProvider;
        _kugouDatabaseStore = kugouDatabaseStore;

        if (_localization != null)
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }

        Files.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasNoFiles));
            OnPropertyChanged(nameof(CanStartDecrypt));
            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(CanClear));
            StartDecryptCommand.NotifyCanExecuteChanged();
            ClearFilesCommand.NotifyCanExecuteChanged();
        };
    }

    /// <summary>
    /// Restores the manually chosen database path (if any) and probes it once so the page can show
    /// the warning strip and mark kgg rows before the user presses start.
    /// </summary>
    public override async Task OnActivatedAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
            return;
        }

        RefreshDatabaseStatus();
    }

    public async Task InitializeAsync()
    {
        _isInitialized = true;
        if (_kugouDatabaseStore != null && _kugouKeyProvider != null)
        {
            var stored = await _kugouDatabaseStore.LoadAsync().ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(stored))
            {
                _kugouKeyProvider.SetDatabasePath(stored);
            }
        }

        RefreshDatabaseStatus();
    }

    private void RefreshDatabaseStatus()
    {
        if (_kugouKeyProvider == null)
        {
            // No provider at all: keep the page usable, kgg will report the failure itself.
            IsKugouDatabaseAvailable = true;
            KugouDatabaseStatus = string.Empty;
            return;
        }

        var info = _kugouKeyProvider.Probe();
        IsKugouDatabaseAvailable = info.IsAvailable;
        KugouDatabaseStatus = info.Message;

        // Rows added while the database was missing are stale the moment it becomes readable.
        foreach (var file in Files)
        {
            if (!IsKggFile(file.FilePath)) continue;

            if (info.IsAvailable && file.IsBlocked)
            {
                file.IsBlocked = false;
                file.StatusMessage = _localization?["Status_Pending"] ?? "Pending";
            }
            else if (!info.IsAvailable && !file.IsProcessed)
            {
                file.IsBlocked = true;
                file.StatusMessage = DecryptStatusMissingDb;
            }
        }

        OnPropertyChanged(nameof(CanStartDecrypt));
        StartDecryptCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 判断某个文件是否依赖酷狗数据库才能解密。
    /// <remarks>
    /// 优先按魔数判断（改名成 .mp3 的 kgg 也能被识别），没有 provider 时退回扩展名。
    /// 这样「能不能解密」的判断和 Common 里的解密器是同一套口径。
    /// </remarks>
    /// </summary>
    private bool IsKggFile(string path)
    {
        if (_kugouKeyProvider != null)
        {
            return _kugouKeyProvider.IsKggFile(path);
        }

        return string.Equals(Path.GetExtension(path), KggExtension, StringComparison.OrdinalIgnoreCase);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(DecryptTitle));
        OnPropertyChanged(nameof(DecryptDescription));
        OnPropertyChanged(nameof(DecryptSelectFiles));
        OnPropertyChanged(nameof(DecryptBrowse));
        OnPropertyChanged(nameof(DecryptParallel));
        OnPropertyChanged(nameof(DecryptStart));
        OnPropertyChanged(nameof(DecryptStop));
        OnPropertyChanged(nameof(DecryptEmptyState));
        OnPropertyChanged(nameof(DecryptClear));
        OnPropertyChanged(nameof(DecryptFilesLabel));
        OnPropertyChanged(nameof(DecryptSpecifyDatabase));
        OnPropertyChanged(nameof(DecryptStatusMissingDb));
        OnPropertyChanged(nameof(DecryptViewDetail));
        OnPropertyChanged(nameof(DecryptFailureTitle));
        OnPropertyChanged(nameof(DecryptKugouRequirement));
        OnPropertyChanged(nameof(SelectionSummary));
        OnPropertyChanged(nameof(CommonTotal));
        OnPropertyChanged(nameof(CommonFiles));
        OnPropertyChanged(nameof(CommonSuccess));
        OnPropertyChanged(nameof(CommonFailed));
        OnPropertyChanged(nameof(ColumnFileName));
        OnPropertyChanged(nameof(ColumnOutputPath));
        OnPropertyChanged(nameof(ColumnStatus));
    }

    [RelayCommand(CanExecute = nameof(CanSelectFiles))]
    private async Task SelectFilesAsync()
    {
        var picked = await _dialogService.ShowFilesPickerAsync(
            DecryptSelectFiles,
            new[] { ".ncm", ".qmc0", ".qmc2", ".qmc3", ".qmcflac", ".qmcogg", ".mflac", ".mgg", ".kgm", ".kgma", ".kgg", ".kwm" });

        if (picked.Count == 0) return;

        // Probe once for the whole batch instead of per file: the database is either readable or
        // it is not, and re-reading it for every dropped file would be pointless I/O.
        RefreshDatabaseStatus();

        foreach (var path in picked)
        {
            // The picker can hand back the same file twice; keys must stay unique.
            if (Files.Any(f => string.Equals(f.FilePath, path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var isBlocked = IsKggFile(path) && !IsKugouDatabaseAvailable;

            var row = new DecryptFileViewModel
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                StatusMessage = isBlocked
                    ? DecryptStatusMissingDb
                    : _localization?["Status_Pending"] ?? "Pending",
                IsBlocked = isBlocked,
                ViewDetailLabel = DecryptViewDetail
            };

            // 详情命令挂在行自己身上，比在模板里通过 $parent 硬转成父 VM 再取命令更稳，
            // 也不会因为 DataTemplate 的上下文变化而悄悄失效。
            row.ShowDetailCommand = new AsyncRelayCommand(() => ShowDetailAsync(row));
            Files.Add(row);
        }

        TotalCount = Files.Count;
        OnPropertyChanged(nameof(CanStartDecrypt));
    }

    private bool CanSelectFiles => !IsDecrypting;

    [RelayCommand(CanExecute = nameof(CanSelectFiles))]
    private async Task SpecifyDatabaseAsync()
    {
        var picked = await _dialogService.ShowFilePickerAsync(
            DecryptSpecifyDatabase,
            new[] { ".db" });

        if (string.IsNullOrWhiteSpace(picked))
        {
            return;
        }

        _kugouKeyProvider?.SetDatabasePath(picked);
        RefreshDatabaseStatus();

        if (_kugouDatabaseStore != null)
        {
            await _kugouDatabaseStore.SaveAsync(picked).ConfigureAwait(true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartDecrypt))]
    private async Task StartDecryptAsync()
    {
        if (Files.Count == 0) return;
        // A kgg row without a readable database cannot decrypt, so it is left out of the run
        // entirely instead of failing one file at a time with the same message.
        var runnable = Files.Where(f => !f.IsBlocked).ToList();

        IsDecrypting = true;
        CompletedCount = 0;
        FailedCount = 0;
        ProgressPercentage = 0;
        TotalCount = runnable.Count;

        foreach (var file in Files)
        {
            file.IsProcessed = false;
            file.IsSuccessful = false;
            file.OutputPath = null;

            if (!file.IsBlocked)
            {
                file.StatusMessage = _localization?["Status_Pending"] ?? "Pending";
            }
        }

        if (runnable.Count == 0)
        {
            IsDecrypting = false;
            return;
        }

        _decryptCts = new CancellationTokenSource();
        var token = _decryptCts.Token;

        try
        {
            var parallel = ParallelCount <= 0 ? 1 : ParallelCount;

            using var warpTask = new WarpTask(parallel);

            var tasks = runnable.Select(file => warpTask.RunAsync(async () =>
            {
                token.ThrowIfCancellationRequested();

                DecryptionResult result;
                try
                {
                    result = await _musicDecryptor.ConvertMusicAsync(file.FilePath, token);
                }
                catch (Exception ex)
                {
                    result = DecryptionResult.Failed(ex.Message);
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    file.IsProcessed = true;
                    file.IsSuccessful = result.IsSuccess;

                    if (result.IsSuccess)
                    {
                        file.OutputPath = result.OutputFilePath;
                        file.ErrorDetail = null;
                        file.StatusMessage = _localization?["Status_Success"] ?? "Success";
                        CompletedCount++;
                    }
                    else
                    {
                        // The status column shows a short label; the full reason (a missing
                        // database dumps several hundred characters) lives in the row detail.
                        file.ErrorDetail = result.ErrorMessage;
                        file.StatusMessage = _localization?["Status_Failed"] ?? "Failed";
                        FailedCount++;
                    }

                    var done = CompletedCount + FailedCount;
                    ProgressPercentage = done * 100.0 / TotalCount;
                });
            }));

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // User cancelled; rows that never ran stay "Pending".
        }
        finally
        {
            IsDecrypting = false;
            _decryptCts?.Dispose();
            _decryptCts = null;
        }
    }

    /// <summary>
    /// Shows the untruncated reason a row failed. Without this the only place the explanatory text
    /// exists is a tooltip, which is unreadable for the long "database not found" message.
    /// </summary>
    private async Task ShowDetailAsync(DecryptFileViewModel? file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.ErrorDetail))
        {
            return;
        }

        await _dialogService.ShowMessageAsync(DecryptFailureTitle, file.ErrorDetail);
    }

    [RelayCommand]
    private void CancelDecrypt()
    {
        _decryptCts?.Cancel();
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void ClearFiles()
    {
        Files.Clear();
        TotalCount = 0;
        CompletedCount = 0;
        FailedCount = 0;
        ProgressPercentage = 0;
        OnPropertyChanged(nameof(CanStartDecrypt));
    }

    private bool CanClear => !IsDecrypting && Files.Count > 0;
}

public partial class DecryptFileViewModel : ObservableObject
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string? _outputPath;

    [ObservableProperty]
    private bool _isProcessed;

    [ObservableProperty]
    private bool _isSuccessful;

    /// <summary>
    /// Short label for the status column. Long explanations belong in <see cref="ErrorDetail"/>,
    /// which the detail dialog shows in full.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorDetail))]
    private string? _statusMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorDetail))]
    private string? _errorDetail;

    /// <summary>
    /// Set when the row cannot run at all (kgg without a readable KuGou database). Blocked rows are
    /// tinted so the reason is visible before pressing start.
    /// </summary>
    [ObservableProperty]
    private bool _isBlocked;

    /// <summary>
    /// 详情按钮上的文案。挂在行上，模板就不必再通过 $parent 反向取父 ViewModel。
    /// </summary>
    [ObservableProperty]
    private string? _viewDetailLabel;

    /// <summary>
    /// 由父 ViewModel 在创建行时注入，用来打开失败详情对话框。
    /// </summary>
    public IAsyncRelayCommand? ShowDetailCommand { get; set; }

    public bool HasErrorDetail => !string.IsNullOrWhiteSpace(ErrorDetail);
}
