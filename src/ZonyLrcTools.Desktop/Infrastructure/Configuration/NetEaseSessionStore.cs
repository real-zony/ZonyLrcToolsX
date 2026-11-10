using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>持久化网易云登录会话。</summary>
/// <remarks>会话属于登录凭据；Unix 文件仅限当前用户读写，Windows 继承目录权限。</remarks>
public sealed class NetEaseSessionStore
{
    private const string FileName = "netease.session.json";

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>会话文件的完整路径。</summary>
    public string FilePath => Path.Combine(AppPaths.ConfigDirectory, FileName);

    /// <summary>是否存在已保存的会话。</summary>
    public bool HasStoredSession => File.Exists(FilePath);

    /// <summary>读取会话；文件缺失或损坏时返回空。</summary>
    public async Task<NetEaseSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(FilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            var json = await File.ReadAllTextAsync(FilePath, cancellationToken);
            var session = JsonSerializer.Deserialize(json, DesktopStateJsonContext.Default.NetEaseSession);

            return string.IsNullOrEmpty(session?.Cookie) ? null : session;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // 损坏的缓存按未登录处理。
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>保存会话。</summary>
    public async Task SaveAsync(NetEaseSession session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DesktopStateFile.WriteAsync(FilePath, session,
                DesktopStateJsonContext.Default.NetEaseSession, ownerOnly: true, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 缓存写入失败不影响本次会话。
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>删除已保存的会话。</summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(FilePath);
        }
        finally
        {
            _lock.Release();
        }
    }
}

/// <summary>网易云登录凭据。</summary>
public sealed class NetEaseSession
{
    /// <summary>登录 Cookie。</summary>
    public string Cookie { get; set; } = string.Empty;

    /// <summary>请求校验令牌。</summary>
    public string CsrfToken { get; set; } = string.Empty;

    /// <summary>会话保存时间，使用 UTC。</summary>
    public DateTimeOffset SavedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

[JsonSerializable(typeof(NetEaseSession))]
[JsonSerializable(typeof(KugouDatabaseSetting))]
internal partial class DesktopStateJsonContext : JsonSerializerContext
{
}

internal static class DesktopStateFile
{
    internal static async Task WriteAsync<T>(string path, T value, JsonTypeInfo<T> typeInfo,
        bool ownerOnly, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + $".{Guid.NewGuid():N}.tmp";
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous
        };
        if (ownerOnly && !OperatingSystem.IsWindows())
        {
            // 在创建时限制权限，避免先写入凭据再修改权限的泄露窗口。
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        try
        {
            await using (var stream = new FileStream(temporaryPath, options))
            {
                await JsonSerializer.SerializeAsync(stream, value, typeInfo, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }
}
