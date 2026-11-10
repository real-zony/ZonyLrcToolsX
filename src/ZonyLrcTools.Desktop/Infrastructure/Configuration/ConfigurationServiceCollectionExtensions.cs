using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>桌面配置服务注册。</summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>从可写配置目录加载配置并注册设置存储。</summary>
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
