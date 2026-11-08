using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Desktop.Infrastructure.Localization;

namespace ZonyLrcTools.Desktop.Views.Dialogs;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    public MessageDialog(string title, string message) : this()
    {
        TitleText.Text = title;
        MessageText.Text = message;
        Title = title;

        var localization = App.Services?.GetService<IUILocalizationService>();
        OkButton.Content = localization?["Common_OK"] ?? "OK";
        CopyButton.Content = localization?["Common_Copy"] ?? "Copy";
    }

    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null)
        {
            return;
        }

        await clipboard.SetTextAsync(MessageText.Text ?? string.Empty);
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
