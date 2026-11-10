using System.Text.Json;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>保存本机手动选择的酷狗数据库路径。</summary>
public sealed class KugouDatabaseStore
{
    private const string FileName = "kugou.database.json";

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>本机状态文件的完整路径。</summary>
    public string FilePath => Path.Combine(AppPaths.ConfigDirectory, FileName);

    /// <summary>读取数据库路径；文件缺失或损坏时返回空。</summary>
    public async Task<string?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(FilePath, cancellationToken);
            var stored = JsonSerializer.Deserialize(json, DesktopStateJsonContext.Default.KugouDatabaseSetting);

            return string.IsNullOrWhiteSpace(stored?.DatabasePath) ? null : stored.DatabasePath;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // 缓存损坏时恢复自动查找。
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>保存数据库路径。</summary>
    public async Task SaveAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var setting = new KugouDatabaseSetting { DatabasePath = databasePath };
            await DesktopStateFile.WriteAsync(FilePath, setting,
                DesktopStateJsonContext.Default.KugouDatabaseSetting, ownerOnly: false, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 缓存写入失败不影响本次使用。
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>忘记手动路径，恢复自动查找。</summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 调用方仍可使用本次自动查找结果。
        }
        finally
        {
            _lock.Release();
        }
    }
}

/// <summary>酷狗数据库路径的持久化格式。</summary>
public sealed class KugouDatabaseSetting
{
    /// <summary>数据库的完整路径。</summary>
    public string DatabasePath { get; set; } = string.Empty;
}
