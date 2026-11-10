using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ZonyLrcTools.Desktop.ViewModels;
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

        return folders.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<string?> ShowFilePickerAsync(string title, string[]? filters = null)
    {
        if (MainWindow == null) return null;

        var files = await MainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = BuildFileTypes(filters)
            });

        return files.FirstOrDefault()?.TryGetLocalPath();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ShowFilesPickerAsync(string title, string[]? filters = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (MainWindow == null) return Array.Empty<string>();

        var fileTypes = BuildFileTypes(filters);

        var files = await MainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true,
                FileTypeFilter = fileTypes
            });
        cancellationToken.ThrowIfCancellationRequested();

        return files
            .Select(f => f.TryGetLocalPath())
            .OfType<string>()
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
    }

    /// <summary>
    /// 将文件扩展名转换为文件选择器筛选条件。
    /// </summary>
    private static List<FilePickerFileType>? BuildFileTypes(string[]? filters)
    {
        if (filters == null || filters.Length == 0)
        {
            return null;
        }

        var patterns = filters
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.StartsWith('*') ? f : $"*.{f.TrimStart('.')}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (patterns.Length == 0)
        {
            return null;
        }

        var name = string.Join(" / ", patterns.Select(p => p.TrimStart('*', '.')));

        return new List<FilePickerFileType>
        {
            new(name) { Patterns = patterns },
            new("All files") { Patterns = new[] { "*" } }
        };
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

    /// <inheritdoc />
    public async Task ShowQrCodeLoginDialogAsync(QrCodeLoginViewModel viewModel, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (MainWindow == null)
        {
            viewModel.NotifyClosed();
            return;
        }

        var dialog = new QrCodeLoginDialog(viewModel);
        using var registration = cancellationToken.Register(
            () => Dispatcher.UIThread.Post(viewModel.RequestClose));
        await dialog.ShowDialog(MainWindow);
    }
}
