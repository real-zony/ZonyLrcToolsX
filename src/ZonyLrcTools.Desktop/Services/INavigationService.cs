using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    event EventHandler<ViewModelBase>? NavigationChanged;

    /// <summary>
    /// 打开页面并保留返回记录。
    /// </summary>
    TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// 切换顶层页面并清空返回记录。
    /// </summary>
    TViewModel NavigateRoot<TViewModel>() where TViewModel : ViewModelBase;

    void GoBack();
    bool CanGoBack { get; }
}
