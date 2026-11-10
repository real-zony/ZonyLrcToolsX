using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public interface IDialogService
{
    Task<string?> ShowFolderPickerAsync(string title);

    Task<string?> ShowFilePickerAsync(string title, string[]? filters = null);

    /// <summary>
    /// 选择多个本地文件。
    /// </summary>
    Task<IReadOnlyList<string>> ShowFilesPickerAsync(string title, string[]? filters = null,
        CancellationToken cancellationToken = default);

    Task<bool> ShowConfirmDialogAsync(string title, string message);

    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// 显示网易云扫码登录对话框，关闭窗口后返回。
    /// </summary>
    Task ShowQrCodeLoginDialogAsync(QrCodeLoginViewModel viewModel, CancellationToken cancellationToken = default);
}
