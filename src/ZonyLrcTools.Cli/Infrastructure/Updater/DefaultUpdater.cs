using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Network;
using ZonyLrcTools.Cli.Infrastructure.Updater.JsonModel;

namespace ZonyLrcTools.Cli.Infrastructure.Updater;

public class DefaultUpdater : ISingletonDependency
{
    public const string UpdateUrl = "https://api.zony.me/lrc-tools/update";

    private readonly IWarpHttpClient _warpHttpClient;
    private readonly ILogger<DefaultUpdater> _logger;

    public DefaultUpdater(IWarpHttpClient warpHttpClient,
        ILogger<DefaultUpdater> logger)
    {
        _warpHttpClient = warpHttpClient;
        _logger = logger;
    }

    public async Task CheckUpdateAsync()
    {
        var response = await _warpHttpClient.GetAsync<NewVersionResponse>(UpdateUrl);
        if (response == null)
        {
            return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (response.NewVersion <= currentVersion)
        {
            return;
        }

        var importantItem = response.Items.FirstOrDefault(x => x.ItemType == NewVersionItemType.Important);
        if (importantItem != null)
        {
            _logger.LogWarning($"发现了新版本，请点击下面的链接进行更新：{importantItem.Url}");
            _logger.LogWarning($"最新版本号:{response.NewVersion}，当前版本号: ${currentVersion}");
            _logger.LogWarning($"更新内容:{response.NewVersionDescription}");
            
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", importantItem.Url);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", importantItem.Url);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", importantItem.Url);
            }
        }
    }
}