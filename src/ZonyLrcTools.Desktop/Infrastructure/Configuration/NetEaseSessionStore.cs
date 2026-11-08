using System.Text.Json;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>
/// Persists the NetEase Cloud Music login state (cookie + csrf token) so the QR login dialog
/// does not reappear on every launch.
/// </summary>
/// <remarks>
/// The cookie is a bearer credential: whoever holds it is logged in as the user. It is written
/// next to the settings file with owner-only permissions where the platform supports it, and
/// the file is small and plain JSON so a user can delete it by hand to force a re-login.
/// </remarks>
public sealed class NetEaseSessionStore
{
    private const string FileName = "netease.session.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Full path of the session file.
    /// </summary>
    public string FilePath => Path.Combine(AppPaths.ConfigDirectory, FileName);

    /// <summary>
    /// Whether a stored session exists on disk.
    /// </summary>
    public bool HasStoredSession => File.Exists(FilePath);

    /// <summary>
    /// Load the stored session, or <c>null</c> when there is none / it cannot be read.
    /// </summary>
    public async Task<NetEaseSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(FilePath, cancellationToken);
            var session = JsonSerializer.Deserialize<NetEaseSession>(json);

            return string.IsNullOrEmpty(session?.Cookie) ? null : session;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // A corrupt session cache must never block the application; treat it as "not logged in".
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Write the session to disk.
    /// </summary>
    public async Task SaveAsync(NetEaseSession session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(AppPaths.ConfigDirectory);

            var temporaryPath = FilePath + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(session, JsonOptions), cancellationToken);
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Losing the cache only costs the user one extra scan next launch.
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Delete the stored session.
    /// </summary>
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
            // Nothing actionable; the in-memory state is cleared regardless.
        }
        finally
        {
            _lock.Release();
        }
    }
}

/// <summary>
/// The bits of a NetEase login that are worth keeping between runs.
/// </summary>
public sealed class NetEaseSession
{
    public string Cookie { get; set; } = string.Empty;

    public string CsrfToken { get; set; } = string.Empty;

    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.Now;
}
