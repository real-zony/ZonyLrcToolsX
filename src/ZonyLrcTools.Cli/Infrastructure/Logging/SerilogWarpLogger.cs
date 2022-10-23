using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Logging;

namespace ZonyLrcTools.Cli.Infrastructure.Logging;

public class SerilogWarpLogger : IWarpLogger, ITransientDependency
{
    private readonly ILogger<SerilogWarpLogger> _logger;

    public SerilogWarpLogger(ILogger<SerilogWarpLogger> logger)
    {
        _logger = logger;
    }

    public Task DebugAsync(string message, Exception exception = null)
    {
        _logger.LogDebug(message, exception);

        return Task.CompletedTask;
    }

    public Task InfoAsync(string message, Exception exception = null)
    {
        _logger.LogInformation(message, exception);
        return Task.CompletedTask;
    }

    public Task WarnAsync(string message, Exception exception = null)
    {
        _logger.LogWarning(message, exception);
        return Task.CompletedTask;
    }

    public Task ErrorAsync(string message, Exception exception = null)
    {
        _logger.LogError(message, exception);
        return Task.CompletedTask;
    }
}