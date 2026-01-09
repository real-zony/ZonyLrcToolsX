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

        if (_navigationStack.TryPeek(out var current))
        {
            _ = current.OnDeactivatedAsync();
        }

        _navigationStack.Push(viewModel);
        _ = viewModel.OnActivatedAsync();

        NavigationChanged?.Invoke(this, viewModel);
        return viewModel;
    }

    public void GoBack()
    {
        if (!CanGoBack) return;

        var current = _navigationStack.Pop();
        _ = current.OnDeactivatedAsync();

        if (_navigationStack.TryPeek(out var previous))
        {
            _ = previous.OnActivatedAsync();
            NavigationChanged?.Invoke(this, previous);
        }
    }
}
