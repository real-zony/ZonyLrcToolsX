using Microsoft.Extensions.Localization;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

namespace ZonyLrcTools.Cli.Infrastructure.Localization;

/// <summary>
/// Provides localization services for CLI commands and options.
/// </summary>
public interface ICliLocalizationService
{
    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    string this[string key] { get; }
}

/// <summary>
/// Implementation of CLI localization service.
/// </summary>
public class CliLocalizationService : ICliLocalizationService, ISingletonDependency
{
    private readonly IStringLocalizer<Cli.CommandStrings> _localizer;

    public CliLocalizationService(IStringLocalizer<Cli.CommandStrings> localizer)
    {
        _localizer = localizer;
    }

    public string this[string key]
    {
        get
        {
            var value = _localizer[key];
            return value.ResourceNotFound ? $"[{key}]" : value.Value;
        }
    }
}
