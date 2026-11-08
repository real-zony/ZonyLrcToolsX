using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Bind <see cref="GlobalOptions"/> from the writable settings file resolved by
    /// <see cref="AppPaths"/>, instead of the read-only folder next to the executable that
    /// <see cref="ServiceCollectionExtensions.ConfigureConfiguration"/> uses.
    /// </summary>
    public static IServiceCollection ConfigureDesktopConfiguration(this IServiceCollection services)
    {
        AppPaths.EnsureConfigFile();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppPaths.ConfigDirectory)
            .AddYamlFile(Path.GetFileName(AppPaths.ConfigFilePath), optional: true, reloadOnChange: false)
            .Build();

        services.Configure<GlobalOptions>(configuration);
        services.AddSingleton<ISettingsStore, SettingsStore>();

        return services;
    }
}
