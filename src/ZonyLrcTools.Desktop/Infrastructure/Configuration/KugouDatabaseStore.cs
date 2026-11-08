using System.Text.Json;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>
/// Remembers a manually chosen Kugou database (<c>KGMusicV3.db</c>) path.
/// </summary>
/// <remarks>
/// This is machine state, not a user preference — it points at a file that happens to exist on
/// this computer. Keeping it out of <c>config.yaml</c> means the shipped configuration template
/// stays clean and CLI users never see a stray GUI-only key.
/// </remarks>
public sealed class KugouDatabaseStore
{
    private const string FileName = "kugou.database.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Full path of the settings file.
    /// </summary>
    public string FilePath => Path.Combine(AppPaths.ConfigDirectory, FileName);

    /// <summary>
    /// Load the stored database path, or <c>null</c> when there is none / it cannot be read.
    /// </summary>
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
            var stored = JsonSerializer.Deserialize<KugouDatabaseSetting>(json);

            return string.IsNullOrWhiteSpace(stored?.DatabasePath) ? null : stored.DatabasePath;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // A corrupt cache must never block the application; fall back to auto detection.
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Persist the chosen database path.
    /// </summary>
    public async Task SaveAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(AppPaths.ConfigDirectory);

            var setting = new KugouDatabaseSetting { DatabasePath = databasePath };
            var temporaryPath = FilePath + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(setting, JsonOptions), cancellationToken);
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Losing this only costs the user one extra pick next launch.
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Forget the stored path so auto detection takes over again.
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
            // Nothing actionable.
        }
        finally
        {
            _lock.Release();
        }
    }
}

/// <summary>
/// The persisted shape of <see cref="KugouDatabaseStore"/>.
/// </summary>
public sealed class KugouDatabaseSetting
{
    public string DatabasePath { get; set; } = string.Empty;
}
