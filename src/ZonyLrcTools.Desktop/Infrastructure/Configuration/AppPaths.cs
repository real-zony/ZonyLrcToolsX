namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>
/// Resolves the directories the desktop application reads and writes at runtime.
/// </summary>
/// <remarks>
/// The application prefers a portable layout: settings live next to the executable so the
/// whole folder can be copied to a USB stick and moved between machines. If that directory
/// is read-only (installed under Program Files, or launched from a read-only mount), it falls
/// back to %AppData%\ZonyLrcTools instead of silently losing every setting the user changes.
/// </remarks>
public static class AppPaths
{
    private const string ApplicationFolderName = "ZonyLrcTools";
    private const string ConfigFileName = "config.yaml";

    private static readonly Lazy<string> LazyConfigDirectory = new(ResolveConfigDirectory);

    /// <summary>
    /// Directory that holds <c>config.yaml</c>.
    /// </summary>
    public static string ConfigDirectory => LazyConfigDirectory.Value;

    /// <summary>
    /// Full path of the writable <c>config.yaml</c>.
    /// </summary>
    public static string ConfigFilePath => Path.Combine(ConfigDirectory, ConfigFileName);

    /// <summary>
    /// Copy the template shipped next to the executable into the config directory when the
    /// user does not have a settings file yet. Safe to call on every startup.
    /// </summary>
    public static void EnsureConfigFile()
    {
        if (File.Exists(ConfigFilePath))
        {
            return;
        }

        var template = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (!File.Exists(template))
        {
            return;
        }

        if (string.Equals(
                Path.GetFullPath(template),
                Path.GetFullPath(ConfigFilePath),
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            File.Copy(template, ConfigFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A missing config file is not fatal: the options binder falls back to defaults.
        }
    }

    private static string ResolveConfigDirectory()
    {
        var baseDirectory = AppContext.BaseDirectory;
        if (IsWritable(baseDirectory))
        {
            return baseDirectory;
        }

        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var fallback = Path.Combine(roaming, ApplicationFolderName);

        try
        {
            Directory.CreateDirectory(fallback);
            if (IsWritable(fallback))
            {
                return fallback;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Fall through to the portable path; reading still works even when writing does not.
        }

        return baseDirectory;
    }

    private static bool IsWritable(string directory)
    {
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            return false;
        }

        var probe = Path.Combine(directory, $".write-probe-{Guid.NewGuid():N}");

        try
        {
            using var stream = new FileStream(
                probe,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1,
                FileOptions.DeleteOnClose);

            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
