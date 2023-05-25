using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.Updater.JsonModel;

namespace ZonyLrcTools.Common.Updater;

public class DefaultUpdater : IUpdater, ISingletonDependency
{
    public const string UpdateUrl = "https://api.myzony.com/lrc-tools/update";

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
        if (!IsCheckUpdate())
        {
            return;
        }

        var response = await _warpHttpClient.GetAsync<NewVersionResponse?>(UpdateUrl);
        if (response == null)
        {
            return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (response.NewVersion <= currentVersion)
        {
            return;
        }

        var importantItem = response.Items?.FirstOrDefault(x => x.ItemType == NewVersionItemType.Important);
        if (importantItem?.Url != null)
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

    private bool IsCheckUpdate()
    {
        var lockFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.lock");

        if (!File.Exists(lockFile))
        {
            File.WriteAllText(lockFile, DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            return true;
        }

        if (long.TryParse(File.ReadAllText(lockFile), out var time))
        {
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (now - time <= 86400 /* 1 Day */)
            {
                return false;
            }
        }

        File.WriteAllText(lockFile, DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
        return true;
    }
}