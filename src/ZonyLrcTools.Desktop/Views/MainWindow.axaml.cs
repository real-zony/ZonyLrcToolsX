using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;
        if (sender is not ListBox listBox) return;

        var services = App.Services;
        if (services == null) return;

        switch (listBox.SelectedIndex)
        {
            case 0:
                viewModel.CurrentPage = services.GetRequiredService<HomeViewModel>();
                break;
            case 1:
                viewModel.CurrentPage = services.GetRequiredService<LyricsDownloadViewModel>();
                break;
            case 2:
                viewModel.CurrentPage = services.GetRequiredService<AlbumDownloadViewModel>();
                break;
            case 3:
                viewModel.CurrentPage = services.GetRequiredService<SettingsViewModel>();
                break;
        }
    }
}
