namespace ZonyLrcTools.Desktop.Services;

public interface IDialogService
{
    Task<string?> ShowFolderPickerAsync(string title);
    Task<string?> ShowFilePickerAsync(string title, string[]? filters = null);
    Task<bool> ShowConfirmDialogAsync(string title, string message);
    Task ShowMessageAsync(string title, string message);
}
