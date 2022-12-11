namespace ZonyLrcTools.Common.Infrastructure.Logging;

/// <summary>
/// 日志记录器，包装了 CLI 和网页日志的两种输出方式。
/// </summary>
public interface IWarpLogger
{
    Task DebugAsync(string message, Exception? exception = null);
    Task InfoAsync(string message, Exception? exception = null);
    Task WarnAsync(string message, Exception? exception = null);
    Task ErrorAsync(string message, Exception? exception = null);
}