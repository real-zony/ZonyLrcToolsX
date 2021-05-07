using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.Extensions;

namespace ZonyLrcTools.Cli.Commands
{
    [Command("lyric-tool")]
    [Subcommand(typeof(ScanCommand), typeof(DownloadCommand))]
    public class ToolCommand : ToolCommandBase
    {
        public override List<string> CreateArgs()
        {
            return new();
        }

        public static async Task<int> Main(string[] args)
        {
            ConfigureLogger();
            ConfigureErrorMessage();

            try
            {
                return await BuildHostedService(args);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        #region > 程序初始化配置 <

        private static void ConfigureErrorMessage() => ErrorCodeHelper.LoadErrorMessage();

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.Console())
                .WriteTo.Logger(lc =>
                {
                    lc.Filter.ByIncludingOnly(lc => lc.Level == LogEventLevel.Warning)
                        .WriteTo.Async(c => c.File("Logs/warnings.txt"));
                })
                .WriteTo.Logger(lc =>
                {
                    lc.Filter.ByIncludingOnly(lc => lc.Level == LogEventLevel.Error)
                        .WriteTo.Async(c => c.File("Logs/errors.txt"));
                })
                .CreateLogger();
        }

        private static Task<int> BuildHostedService(string[] args)
        {
            return new HostBuilder()
                .ConfigureLogging(builder => builder.AddSerilog())
                .ConfigureHostConfiguration(builder =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(PhysicalConsole.Singleton);
                    services.BeginAutoDependencyInject<ToolCommand>();
                    services.ConfigureConfiguration();
                    services.ConfigureToolService();
                })
                .RunCommandLineApplicationAsync<ToolCommand>(args);
        }

        private static int HandleException(Exception ex)
        {
            switch (ex)
            {
                case ErrorCodeException exception:
                    Log.Logger.Error($"出现了未处理的异常，错误代码: {exception.ErrorCode}，错误信息: {ErrorCodeHelper.GetMessage(exception.ErrorCode)}\n调用栈:\n{exception.StackTrace}");
                    Environment.Exit(exception.ErrorCode);
                    return exception.ErrorCode;
                case Exception unknownException:
                    Log.Logger.Error($"出现了未处理的异常: {unknownException.Message}\n调用栈:\n{unknownException.StackTrace}");
                    Environment.Exit(-1);
                    return 1;
                default:
                    return 1;
            }
        }

        #endregion
    }
}