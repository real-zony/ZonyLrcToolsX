using CommunityToolkit.Mvvm.ComponentModel;

namespace ZonyLrcTools.Desktop.ViewModels;

/// <summary>
/// Base class for all ViewModels.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyMessage;

    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Called when the ViewModel is activated (navigated to).
    /// </summary>
    public virtual Task OnActivatedAsync() => Task.CompletedTask;

    /// <summary>
    /// Called when the ViewModel is deactivated (navigated away).
    /// </summary>
    public virtual Task OnDeactivatedAsync() => Task.CompletedTask;
}
