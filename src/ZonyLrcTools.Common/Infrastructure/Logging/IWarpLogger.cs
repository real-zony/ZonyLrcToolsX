namespace ZonyLrcTools.Common.Infrastructure.Logging;

public interface IWarpLogger
{
    Task DebugAsync(string message, Exception? exception = null);
    Task InfoAsync(string message, Exception? exception = null);
    Task WarnAsync(string message, Exception? exception = null);
    Task ErrorAsync(string message, Exception? exception = null);
}