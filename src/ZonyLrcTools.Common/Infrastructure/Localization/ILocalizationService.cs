using System.Globalization;

namespace ZonyLrcTools.Common.Infrastructure.Localization;

/// <summary>
/// Provides localization services for the application.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    string this[string key] { get; }

    /// <summary>
    /// Gets a localized string by key with format arguments.
    /// </summary>
    string this[string key, params object[] args] { get; }

    /// <summary>
    /// Gets an error message by error code.
    /// </summary>
    string GetErrorMessage(int errorCode);

    /// <summary>
    /// Gets a warning message by warning code.
    /// </summary>
    string GetWarningMessage(int warningCode);

    /// <summary>
    /// Sets the current culture.
    /// </summary>
    void SetCulture(string cultureName);

    /// <summary>
    /// Gets the current culture name.
    /// </summary>
    string CurrentCulture { get; }

    /// <summary>
    /// Gets the list of supported cultures.
    /// </summary>
    IReadOnlyList<CultureInfo> SupportedCultures { get; }
}
