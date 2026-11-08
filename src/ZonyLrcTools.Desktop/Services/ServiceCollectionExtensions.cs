using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Desktop.Infrastructure.Configuration;
using ZonyLrcTools.Desktop.Infrastructure.Localization;
using ZonyLrcTools.Desktop.ViewModels;

namespace ZonyLrcTools.Desktop.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all services for the desktop application.
    /// </summary>
    public static IServiceCollection AddDesktopServices(this IServiceCollection services)
    {
        // Register Common project's auto dependency injection
        services.BeginAutoDependencyInject<AvaloniaWarpLogger>();
        services.BeginAutoDependencyInject<IWarpHttpClient>();

        // Configure Common services
        // ConfigureConfiguration() from Common always reads config.yaml from the executable
        // folder, which breaks portable installs and read-only install locations. The desktop
        // app resolves a writable location first and binds from there instead.
        services.ConfigureDesktopConfiguration();
        services.ConfigureLocalization();
        services.ConfigureToolService();

        // Configure Desktop localization
        // Note: Don't set ResourcesPath here because the embedded resource name is
        // ZonyLrcTools.Desktop.UIStrings (not ZonyLrcTools.Desktop.Resources.UIStrings)
        services.AddLocalization();
        services.AddSingleton<IUILocalizationService, UILocalizationService>();

        // Register GUI-specific IWarpLogger implementation
        services.AddSingleton<AvaloniaWarpLogger>();
        services.AddSingleton<IWarpLogger>(sp => sp.GetRequiredService<AvaloniaWarpLogger>());

        // Register navigation and dialog services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();

        // Stores the NetEase login cookie between launches so the QR dialog is a first-run event.
        services.AddSingleton<NetEaseSessionStore>();

        // Remembers a manually chosen KuGou database path (machine state, not a user preference).
        services.AddSingleton<KugouDatabaseStore>();

        // Register ViewModels (Singleton to preserve state across navigation)
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<LyricsDownloadViewModel>();
        services.AddSingleton<AlbumDownloadViewModel>();
        services.AddSingleton<DecryptViewModel>();
        services.AddSingleton<SettingsViewModel>();

        return services;
    }

    /// <summary>
    /// Initialize language settings based on system culture or user preference.
    /// </summary>
    public static void InitializeLanguage(string? preferredCulture = null)
    {
        CultureInfo culture;

        if (!string.IsNullOrEmpty(preferredCulture))
        {
            // Use user's preferred culture
            culture = new CultureInfo(preferredCulture);
        }
        else
        {
            // Use system culture, fallback to zh-CN if not supported
            var systemCulture = CultureInfo.CurrentUICulture.Name;
            culture = systemCulture.StartsWith("zh") ? new CultureInfo("zh-CN") :
                      systemCulture.StartsWith("en") ? new CultureInfo("en-US") :
                      new CultureInfo("zh-CN");
        }

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
