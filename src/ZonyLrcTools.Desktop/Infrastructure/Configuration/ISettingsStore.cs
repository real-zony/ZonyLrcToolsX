using ZonyLrcTools.Common.Configuration;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>
/// Result of a settings persist operation.
/// </summary>
/// <param name="IsSuccess">Whether the settings were written to disk.</param>
/// <param name="FilePath">The file that was written, when the write succeeded.</param>
/// <param name="ErrorMessage">Human readable reason when the write failed.</param>
public readonly record struct SettingsSaveResult(bool IsSuccess, string? FilePath, string? ErrorMessage)
{
    public static SettingsSaveResult Success(string filePath) => new(true, filePath, null);

    public static SettingsSaveResult Failure(string errorMessage) => new(false, null, errorMessage);
}

/// <summary>
/// Single owner of the settings file: reads the bound <see cref="GlobalOptions"/> and writes it back
/// to <see cref="AppPaths.ConfigFilePath"/>.
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// The live options instance shared by the whole application. Mutating it takes effect
    /// immediately for components that read it per operation.
    /// </summary>
    GlobalOptions Options { get; }

    /// <summary>
    /// Absolute path of the settings file currently in use.
    /// </summary>
    string ConfigFilePath { get; }

    /// <summary>
    /// Serialize <see cref="Options"/> to YAML and replace the settings file atomically.
    /// </summary>
    Task<SettingsSaveResult> SaveAsync(CancellationToken cancellationToken = default);
}
