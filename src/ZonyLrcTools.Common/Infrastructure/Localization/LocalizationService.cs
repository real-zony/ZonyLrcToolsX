using System.Globalization;
using Microsoft.Extensions.Localization;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Common.Infrastructure.Localization;

/// <summary>
/// Implementation of <see cref="ILocalizationService"/> using .NET localization.
/// </summary>
public class LocalizationService : ILocalizationService, ISingletonDependency
{
    private readonly IStringLocalizer<Common.Messages> _messagesLocalizer;
    private readonly IStringLocalizer<Common.ErrorMessages> _errorLocalizer;

    private static readonly CultureInfo[] _supportedCultures =
    {
        new("zh-CN"),
        new("en-US")
    };

    public LocalizationService(
        IStringLocalizer<Common.Messages> messagesLocalizer,
        IStringLocalizer<Common.ErrorMessages> errorLocalizer)
    {
        _messagesLocalizer = messagesLocalizer;
        _errorLocalizer = errorLocalizer;
    }

    public string this[string key]
    {
        get
        {
            var value = _messagesLocalizer[key];
            return value.ResourceNotFound ? $"[{key}]" : value.Value;
        }
    }

    public string this[string key, params object[] args]
    {
        get
        {
            var value = _messagesLocalizer[key];
            if (value.ResourceNotFound) return $"[{key}]";
            return string.Format(value.Value, args);
        }
    }

    public string GetErrorMessage(int errorCode)
    {
        var key = $"Error_{errorCode}";
        var value = _errorLocalizer[key];
        return value.ResourceNotFound ? $"Unknown error: {errorCode}" : value.Value;
    }

    public string GetWarningMessage(int warningCode)
    {
        var key = $"Warning_{warningCode}";
        var value = _errorLocalizer[key];
        return value.ResourceNotFound ? $"Unknown warning: {warningCode}" : value.Value;
    }

    public string CurrentCulture => CultureInfo.CurrentUICulture.Name;

    public IReadOnlyList<CultureInfo> SupportedCultures => _supportedCultures;

    public void SetCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
