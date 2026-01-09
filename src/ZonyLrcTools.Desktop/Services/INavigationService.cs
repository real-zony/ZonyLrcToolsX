using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    event EventHandler<ViewModelBase>? NavigationChanged;

    TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void GoBack();
    bool CanGoBack { get; }
}
