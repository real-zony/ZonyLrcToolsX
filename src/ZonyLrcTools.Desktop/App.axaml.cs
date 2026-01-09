using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Desktop.Services;
using ZonyLrcTools.Desktop.ViewModels;
using ZonyLrcTools.Desktop.Views;

namespace ZonyLrcTools.Desktop;

public partial class App : Application
{
    public static ServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        // Configure DI container BEFORE loading XAML
        // This is required because LocalizeExtension needs App.Services during XAML loading
        var services = new ServiceCollection();
        services.AddDesktopServices();
        Services = services.BuildServiceProvider();

        // Initialize language settings before loading XAML
        ServiceCollectionExtensions.InitializeLanguage();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Avoid duplicate validations from both Avalonia and CommunityToolkit
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services!.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
