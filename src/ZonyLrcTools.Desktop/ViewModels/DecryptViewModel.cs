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

/// <summary>
/// 音乐文件解密页面。
/// </summary>
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
    [NotifyCanExecuteChangedFor(nameof(StartDecryptCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpecifyDatabaseCommand))]
    [NotifyPropertyChangedFor(nameof(CanStartDecrypt))]
    private bool _isInitializing;

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
    [NotifyCanExecuteChangedFor(nameof(SpecifyDatabaseCommand))]
    [NotifyPropertyChangedFor(nameof(CanStartDecrypt))]
    [NotifyPropertyChangedFor(nameof(CanClear))]
    private bool _isDecrypting;

    /// <summary>
    /// 是否可以读取酷狗解密密钥数据库。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDatabaseWarning))]
    private bool _isKugouDatabaseAvailable;

    [ObservableProperty]
    private string _kugouDatabaseStatus = string.Empty;

    // 界面文案。
    /// <summary>页面标题。</summary>
    public string DecryptTitle => _localization?["Decrypt_Title"] ?? "Music Decryption";
    /// <summary>页面说明。</summary>
    public string DecryptDescription => _localization?["Decrypt_Description"] ?? "Convert encrypted music files (ncm / qmc / kgm / kgg / kwm) into playable formats";
    /// <summary>文件选择提示。</summary>
    public string DecryptSelectFiles => _localization?["Decrypt_SelectFiles"] ?? "Select encrypted music files...";
    /// <summary>添加文件按钮文案。</summary>
    public string DecryptBrowse => _localization?["Decrypt_Browse"] ?? "Add Files...";
    /// <summary>并行任务数标签。</summary>
    public string DecryptParallel => _localization?["Decrypt_Parallel"] ?? "Parallel:";
    /// <summary>开始解密按钮文案。</summary>
    public string DecryptStart => _localization?["Decrypt_Start"] ?? "Start Decrypt";
    /// <summary>停止按钮文案。</summary>
    public string DecryptStop => _localization?["Decrypt_Stop"] ?? "Stop";
    /// <summary>文件列表为空时的提示。</summary>
    public string DecryptEmptyState => _localization?["Decrypt_EmptyState"] ?? "No files yet, add files to start decrypting";
    /// <summary>清空按钮文案。</summary>
    public string DecryptClear => _localization?["Decrypt_Clear"] ?? "Clear";
    /// <summary>文件列表标签。</summary>
    public string DecryptFilesLabel => _localization?["Decrypt_FilesLabel"] ?? "Files";
    /// <summary>选择数据库按钮文案。</summary>
    public string DecryptSpecifyDatabase => _localization?["Decrypt_SpecifyDatabase"] ?? "Locate KGMusicV3.db...";
    /// <summary>数据库缺失状态文案。</summary>
    public string DecryptStatusMissingDb => _localization?["Decrypt_StatusMissingDb"] ?? "KuGou database missing";
    /// <summary>查看详情按钮文案。</summary>
    public string DecryptViewDetail => _localization?["Decrypt_ViewDetail"] ?? "Details";
    /// <summary>失败详情窗口标题。</summary>
    public string DecryptFailureTitle => _localization?["Decrypt_FailureTitle"] ?? "Decryption failure";

    /// <summary>
    /// 酷狗数据库缺失时的提示。
    /// </summary>
    public string DecryptKugouRequirement => _localization?["Decrypt_KugouMissing"]
        ?? "kgg decryption needs the KuGou database KGMusicV3.db (the key is only written when KuGou downloaded that song)";

    /// <summary>是否显示数据库缺失提示。</summary>
    public bool ShowDatabaseWarning => !IsKugouDatabaseAvailable;

    /// <summary>
    /// 已选文件数量。
    /// </summary>
    public string SelectionSummary => Files.Count == 0
        ? string.Empty
        : $"{Files.Count} {CommonFiles}";

    /// <summary>总数标签。</summary>
    public string CommonTotal => _localization?["Common_Total"] ?? "Total:";
    /// <summary>文件数量单位。</summary>
    public string CommonFiles => _localization?["Common_Files"] ?? "files";
    /// <summary>成功数量标签。</summary>
    public string CommonSuccess => _localization?["Common_Success"] ?? "Success:";
    /// <summary>失败数量标签。</summary>
    public string CommonFailed => _localization?["Common_Failed"] ?? "Failed:";
    /// <summary>文件名列标题。</summary>
    public string ColumnFileName => _localization?["Column_FileName"] ?? "File Name";
    /// <summary>输出路径列标题。</summary>
    public string ColumnOutputPath => _localization?["Column_OutputPath"] ?? "Output";
    /// <summary>状态列标题。</summary>
    public string ColumnStatus => _localization?["Column_Status"] ?? "Status";

    /// <summary>是否存在可以开始解密的文件。</summary>
    public bool CanStartDecrypt => !IsDecrypting && !IsInitializing && Files.Any(f => !f.IsBlocked);
    /// <summary>文件列表是否为空。</summary>
    public bool HasNoFiles => Files.Count == 0;

    /// <summary>待解密文件列表。</summary>
    public ObservableCollection<DecryptFileViewModel> Files { get; } = new();

    /// <summary>
    /// 创建音乐解密页面。
    /// </summary>
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
    /// 恢复已保存的数据库路径并刷新文件状态。
    /// </summary>
    public override async Task OnActivatedAsync()
    {
        if (IsDecrypting || IsInitializing) return;

        if (!_isInitialized)
        {
            await InitializeAsync();
            return;
        }

        RefreshDatabaseStatus();
    }

    /// <summary>
    /// 初始化酷狗数据库路径。
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitializing) return;

        IsInitializing = true;
        try
        {
            if (_kugouDatabaseStore != null && _kugouKeyProvider != null)
            {
                var stored = await _kugouDatabaseStore.LoadAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(stored))
                {
                    _kugouKeyProvider.SetDatabasePath(stored);
                }
            }

            RefreshDatabaseStatus();
            _isInitialized = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消初始化，下次进入页面时重试。
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync(_localization?["Common_Error"] ?? "Error", ex.Message);
        }
        finally
        {
            IsInitializing = false;
        }
    }

    private void RefreshDatabaseStatus()
    {
        if (_kugouKeyProvider == null)
        {
            // 未注册密钥提供者时，由解密器报告具体错误。
            IsKugouDatabaseAvailable = true;
            KugouDatabaseStatus = string.Empty;
            return;
        }

        var info = _kugouKeyProvider.Probe();
        IsKugouDatabaseAvailable = info.IsAvailable;
        KugouDatabaseStatus = info.Message;

        // 数据库可用后解除未处理文件的阻止状态。
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

        foreach (var file in Files)
        {
            file.ViewDetailLabel = DecryptViewDetail;
            file.StatusMessage = file.IsBlocked
                ? DecryptStatusMissingDb
                : file.IsProcessed
                    ? file.IsSuccessful
                        ? _localization?["Status_Success"] ?? "Success"
                        : _localization?["Status_Failed"] ?? "Failed"
                    : _localization?["Status_Pending"] ?? "Pending";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSelectFiles))]
    private async Task SelectFilesAsync(CancellationToken cancellationToken = default)
    {
        var picked = await _dialogService.ShowFilesPickerAsync(
            DecryptSelectFiles,
            [".ncm", ".qmc0", ".qmc2", ".qmc3", ".qmcflac", ".qmcogg", ".mflac", ".mgg", ".kgm", ".kgma", ".kgg", ".kwm"],
            cancellationToken);
        if (picked.Count == 0) return;

        // 同一批文件只检查一次数据库。
        RefreshDatabaseStatus();

        foreach (var path in picked)
        {
            // 避免重复添加同一文件。
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

            row.ShowDetailCommand = new AsyncRelayCommand(token => ShowDetailAsync(row, token));
            Files.Add(row);
        }

        TotalCount = Files.Count;
        OnPropertyChanged(nameof(CanStartDecrypt));
    }

    private bool CanSelectFiles => !IsDecrypting && !IsInitializing;

    [RelayCommand(CanExecute = nameof(CanSelectFiles))]
    private async Task SpecifyDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var picked = await _dialogService.ShowFilePickerAsync(
            DecryptSpecifyDatabase,
            new[] { ".db" });
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(picked))
        {
            return;
        }

        _kugouKeyProvider?.SetDatabasePath(picked);
        RefreshDatabaseStatus();

        if (_kugouDatabaseStore != null)
        {
            await _kugouDatabaseStore.SaveAsync(picked, cancellationToken);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartDecrypt))]
    private async Task StartDecryptAsync(CancellationToken cancellationToken = default)
    {
        if (Files.Count == 0) return;
        // 缺少密钥数据库的文件不加入本次任务。
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
            file.ErrorDetail = null;

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

        _decryptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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
                    result = await Task.Run(() => _musicDecryptor.ConvertMusicAsync(file.FilePath, token), token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
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
                        // 状态列只显示摘要，完整错误在详情对话框展示。
                        file.ErrorDetail = result.ErrorMessage;
                        file.StatusMessage = _localization?["Status_Failed"] ?? "Failed";
                        FailedCount++;
                    }

                    var done = CompletedCount + FailedCount;
                    ProgressPercentage = done * 100.0 / TotalCount;
                });
            }, token));

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // 用户取消时，未完成的文件保持待处理状态。
        }
        finally
        {
            IsDecrypting = false;
            _decryptCts?.Dispose();
            _decryptCts = null;
        }
    }

    /// <summary>
    /// 显示文件解密失败的完整原因。
    /// </summary>
    private async Task ShowDetailAsync(DecryptFileViewModel? file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
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

    /// <summary>
    /// 是否可以清空文件列表。
    /// </summary>
    private bool CanClear => !IsDecrypting && Files.Count > 0;
}

/// <summary>
/// 单个文件的解密状态。
/// </summary>
public partial class DecryptFileViewModel : ObservableObject
{
    /// <summary>输入文件路径。</summary>
    [ObservableProperty]
    private string _filePath = string.Empty;

    /// <summary>输入文件名。</summary>
    [ObservableProperty]
    private string _fileName = string.Empty;

    /// <summary>解密后的输出路径。</summary>
    [ObservableProperty]
    private string? _outputPath;

    /// <summary>是否已完成处理。</summary>
    [ObservableProperty]
    private bool _isProcessed;

    /// <summary>是否解密成功。</summary>
    [ObservableProperty]
    private bool _isSuccessful;

    /// <summary>
    /// 状态列摘要，完整原因保存在 <see cref="ErrorDetail"/>。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorDetail))]
    private string? _statusMessage;

    /// <summary>解密失败的完整原因。</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorDetail))]
    private string? _errorDetail;

    /// <summary>
    /// 是否因缺少酷狗数据库而无法开始解密。
    /// </summary>
    [ObservableProperty]
    private bool _isBlocked;

    /// <summary>
    /// 详情按钮文案。
    /// </summary>
    [ObservableProperty]
    private string? _viewDetailLabel;

    /// <summary>
    /// 由父 ViewModel 在创建行时注入，用来打开失败详情对话框。
    /// </summary>
    public IAsyncRelayCommand? ShowDetailCommand { get; set; }

    /// <summary>
    /// 是否存在可查看的错误详情。
    /// </summary>
    public bool HasErrorDetail => !string.IsNullOrWhiteSpace(ErrorDetail);
}
