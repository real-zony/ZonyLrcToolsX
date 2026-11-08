using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public interface IDialogService
{
    Task<string?> ShowFolderPickerAsync(string title);

    Task<string?> ShowFilePickerAsync(string title, string[]? filters = null);

    Task<IReadOnlyList<string>> ShowFilesPickerAsync(string title, string[]? filters = null);

    Task<bool> ShowConfirmDialogAsync(string title, string message);

    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Open the NetEase QR-code login dialog as a modal child of the main window.
    /// The returned task completes when the user or the login flow closes it; callers that
    /// only want the dialog on screen can ignore it.
    /// </summary>
    Task ShowQrCodeLoginDialogAsync(QrCodeLoginViewModel viewModel);
}
