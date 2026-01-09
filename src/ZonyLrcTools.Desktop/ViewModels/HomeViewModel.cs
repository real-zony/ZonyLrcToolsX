using CommunityToolkit.Mvvm.ComponentModel;

namespace ZonyLrcTools.Desktop.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _welcomeMessage = "Welcome to ZonyLrcTools X";

    [ObservableProperty]
    private string _description = "A cross-platform tool for downloading lyrics and album covers for your music collection.";

    [ObservableProperty]
    private string _version = "4.0.3";
}
