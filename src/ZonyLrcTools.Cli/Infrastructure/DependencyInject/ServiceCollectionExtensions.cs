using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.DependencyInject
{
    /// <summary>
    /// Service 注入的扩展方法。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 配置工具会用到的服务。
        /// </summary>
        public static IServiceCollection ConfigureToolService(this IServiceCollection services)
        {
            if (services == null)
            {
                return null;
            }

            services.AddHttpClient(DefaultWarpHttpClient.HttpClientNameConstant)
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var option = provider.GetRequiredService<IOptions<ToolOptions>>().Value;

                    return new HttpClientHandler
                    {
                        UseProxy = option.NetworkOptions.Enable,
                        Proxy = new WebProxy($"{option.NetworkOptions.ProxyIp}:{option.NetworkOptions.ProxyPort}")
                    };
                });

            return services;
        }

        /// <summary>
        /// 配置工具关联的配置信息(<see cref="IConfiguration"/>)。
        /// </summary>
        public static IServiceCollection ConfigureConfiguration(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            services.Configure<ToolOptions>(configuration.GetSection("ToolOption"));

            return services;
        }
    }
}