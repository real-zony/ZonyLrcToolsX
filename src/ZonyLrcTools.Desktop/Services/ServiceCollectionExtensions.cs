using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Network;
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
        services.ConfigureConfiguration();
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

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<LyricsDownloadViewModel>();
        services.AddTransient<AlbumDownloadViewModel>();
        services.AddTransient<SettingsViewModel>();

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
