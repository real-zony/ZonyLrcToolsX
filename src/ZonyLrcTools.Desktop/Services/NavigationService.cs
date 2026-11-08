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
            // Already showing this page; do not restart it.
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
    /// Page lifecycle hooks are fire-and-forget from the caller's perspective; an exception thrown
    /// inside one must not tear down the navigation call that triggered it.
    /// </summary>
    private static async Task SafeActivateAsync(ViewModelBase viewModel)
    {
        try
        {
            await viewModel.OnActivatedAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // A failing OnActivated must not crash navigation.
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
            // A failing OnDeactivated must not crash navigation.
        }
    }
}
