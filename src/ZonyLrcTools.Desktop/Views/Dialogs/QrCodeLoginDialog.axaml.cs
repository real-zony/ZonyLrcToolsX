using Avalonia.Controls;
using Avalonia.Interactivity;
using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Views.Dialogs;

public partial class QrCodeLoginDialog : Window
{
    private QrCodeLoginViewModel? _viewModel;

    public QrCodeLoginDialog()
    {
        InitializeComponent();
    }

    public QrCodeLoginDialog(QrCodeLoginViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        Title = viewModel.LoginTitle;

        viewModel.CloseRequested += OnCloseRequested;
        Closed += OnClosed;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.NotifyClosed();
        _viewModel = null;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
