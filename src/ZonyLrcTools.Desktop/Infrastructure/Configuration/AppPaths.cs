namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>解析桌面应用的配置目录。</summary>
/// <remarks>优先使用便携目录；不可写时回退到当前用户的应用数据目录。</remarks>
public static class AppPaths
{
    private const string ApplicationFolderName = "ZonyLrcTools";
    private const string ConfigFileName = "config.yaml";

    private static readonly Lazy<string> LazyConfigDirectory = new(ResolveConfigDirectory);

    /// <summary>配置文件所在目录。</summary>
    public static string ConfigDirectory => LazyConfigDirectory.Value;

    /// <summary>配置文件的完整路径。</summary>
    public static string ConfigFilePath => Path.Combine(ConfigDirectory, ConfigFileName);

    /// <summary>首次启动时将随程序附带的配置复制到用户目录。</summary>
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
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            File.Copy(template, ConfigFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 模板无法复制时由调用方处理缺失配置。
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
            // 没有可写目录时仍允许读取便携目录的配置。
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
