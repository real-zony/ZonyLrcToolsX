using System.Collections.ObjectModel;
using Avalonia.Threading;
using ZonyLrcTools.Common.Infrastructure.Logging;

namespace ZonyLrcTools.Desktop.Services;

/// <summary>
/// Avalonia implementation of IWarpLogger that supports UI binding.
/// </summary>
public class AvaloniaWarpLogger : IWarpLogger
{
    private const int MaxLogEntries = 1000;

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public event EventHandler<LogEntry>? LogAdded;

    public Task DebugAsync(string message, Exception? exception = null)
    {
        return AddLogAsync(LogLevel.Debug, message, exception);
    }

    public Task InfoAsync(string message, Exception? exception = null)
    {
        return AddLogAsync(LogLevel.Info, message, exception);
    }

    public Task WarnAsync(string message, Exception? exception = null)
    {
        return AddLogAsync(LogLevel.Warning, message, exception);
    }

    public Task ErrorAsync(string message, Exception? exception = null)
    {
        return AddLogAsync(LogLevel.Error, message, exception);
    }

    private async Task AddLogAsync(LogLevel level, string message, Exception? exception)
    {
        var entry = new LogEntry(level, message, exception);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogEntries.Add(entry);

            while (LogEntries.Count > MaxLogEntries)
            {
                LogEntries.RemoveAt(0);
            }

            LogAdded?.Invoke(this, entry);
        });
    }
}

public record LogEntry(LogLevel Level, string Message, Exception? Exception)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
