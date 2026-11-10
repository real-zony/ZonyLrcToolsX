using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _navigationStack = new();

    public ViewModelBase? CurrentViewModel => _navigationStack.TryPeek(out var vm) ? vm : null;
    public bool CanGoBack => _navigationStack.Count > 1;

    public event EventHandler<ViewModelBase>? NavigationChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        if (_navigationStack.TryPeek(out var current) && ReferenceEquals(current, viewModel))
        {
            // 当前页面无需重复激活。
            return viewModel;
        }

        if (_navigationStack.TryPeek(out current))
        {
            _ = SafeDeactivateAsync(current);
        }

        _navigationStack.Push(viewModel);
        _ = SafeActivateAsync(viewModel);

        NavigationChanged?.Invoke(this, viewModel);
        return viewModel;
    }

    /// <inheritdoc />
    public TViewModel NavigateRoot<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        if (_navigationStack.Count == 1 && ReferenceEquals(_navigationStack.Peek(), viewModel))
        {
            return viewModel;
        }

        while (_navigationStack.TryPop(out var previous))
        {
            if (!ReferenceEquals(previous, viewModel))
            {
                _ = SafeDeactivateAsync(previous);
            }
        }

        _navigationStack.Push(viewModel);
        _ = SafeActivateAsync(viewModel);

        NavigationChanged?.Invoke(this, viewModel);
        return viewModel;
    }

    public void GoBack()
    {
        if (!CanGoBack) return;

        var current = _navigationStack.Pop();
        _ = SafeDeactivateAsync(current);

        if (_navigationStack.TryPeek(out var previous))
        {
            _ = SafeActivateAsync(previous);
            NavigationChanged?.Invoke(this, previous);
        }
    }

    /// <summary>
    /// 页面激活失败时不打断导航。
    /// </summary>
    private static async Task SafeActivateAsync(ViewModelBase viewModel)
    {
        try
        {
            await viewModel.OnActivatedAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // 页面自行处理错误，避免中断导航。
        }
    }

    private static async Task SafeDeactivateAsync(ViewModelBase viewModel)
    {
        try
        {
            await viewModel.OnDeactivatedAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // 页面自行处理错误，避免中断导航。
        }
    }
}
