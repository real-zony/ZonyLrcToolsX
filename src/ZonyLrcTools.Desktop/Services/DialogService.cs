using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ZonyLrcTools.Desktop.Views.Dialogs;

namespace ZonyLrcTools.Desktop.Services;

public class DialogService : IDialogService
{
    private Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
        .MainWindow;

    public async Task<string?> ShowFolderPickerAsync(string title)
    {
        if (MainWindow == null) return null;

        var folders = await MainWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

        return folders.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> ShowFilePickerAsync(string title, string[]? filters = null)
    {
        if (MainWindow == null) return null;

        var fileTypes = filters?.Select(f => new FilePickerFileType(f)
        {
            Patterns = new[] { f }
        }).ToList();

        var files = await MainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            });

        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<bool> ShowConfirmDialogAsync(string title, string message)
    {
        if (MainWindow == null) return false;

        var dialog = new ConfirmDialog(title, message);
        var result = await dialog.ShowDialog<bool>(MainWindow);
        return result;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        if (MainWindow == null) return;

        var dialog = new MessageDialog(title, message);
        await dialog.ShowDialog(MainWindow);
    }
}
