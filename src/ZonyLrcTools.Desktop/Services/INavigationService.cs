using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    event EventHandler<ViewModelBase>? NavigationChanged;

    /// <summary>
    /// Push a page onto the navigation stack. Use for drill-down (detail pages, wizards).
    /// </summary>
    TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Replace the whole stack with a single page. This is what the sidebar uses: switching
    /// sections is not a back-navigable action, so the stack must not grow on every click.
    /// </summary>
    TViewModel NavigateRoot<TViewModel>() where TViewModel : ViewModelBase;

    void GoBack();
    bool CanGoBack { get; }
}
