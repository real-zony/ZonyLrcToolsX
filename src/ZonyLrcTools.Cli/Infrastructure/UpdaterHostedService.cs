using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ZonyLrcTools.Common.Updater;

namespace ZonyLrcTools.Cli.Infrastructure;

public class UpdaterHostedService : IHostedService
{
    private readonly IUpdater _updater;

    public UpdaterHostedService(IUpdater updater)
    {
        _updater = updater;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _updater.CheckUpdateAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}