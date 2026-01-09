using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace ZonyLrcTools.Desktop.Infrastructure.Localization;

/// <summary>
/// Avalonia markup extension for localization.
/// Usage: Text="{loc:Localize Key=Nav_Home}"
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    public LocalizeExtension()
    {
    }

    public LocalizeExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// The resource key to look up.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Default value if the key is not found.
    /// </summary>
    public string? Default { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
        {
            return Default ?? "[NO_KEY]";
        }

        var localizer = App.Services?.GetService<IStringLocalizer<Desktop.UIStrings>>();
        if (localizer == null)
        {
            return Default ?? $"[{Key}]";
        }

        var localizedValue = localizer[Key];
        if (localizedValue.ResourceNotFound)
        {
            return Default ?? $"[{Key}]";
        }

        return localizedValue.Value;
    }
}

/// <summary>
/// UI localization service for dynamic language switching.
/// </summary>
public interface IUILocalizationService
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
    /// Gets the current culture name.
    /// </summary>
    string CurrentCulture { get; }

    /// <summary>
    /// Changes the current UI language.
    /// </summary>
    void SetLanguage(string cultureName);

    /// <summary>
    /// Gets whether the language follows system settings.
    /// </summary>
    bool FollowSystem { get; set; }

    /// <summary>
    /// Event raised when the language changes.
    /// </summary>
    event EventHandler? LanguageChanged;
}

/// <summary>
/// Implementation of UI localization service.
/// </summary>
public class UILocalizationService : IUILocalizationService
{
    private readonly IStringLocalizer<Desktop.UIStrings> _localizer;
    private bool _followSystem = true;

    public UILocalizationService(IStringLocalizer<Desktop.UIStrings> localizer)
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

    public string this[string key, params object[] args]
    {
        get
        {
            var value = _localizer[key];
            if (value.ResourceNotFound) return $"[{key}]";
            return string.Format(value.Value, args);
        }
    }

    public string CurrentCulture => System.Globalization.CultureInfo.CurrentUICulture.Name;

    public bool FollowSystem
    {
        get => _followSystem;
        set => _followSystem = value;
    }

    public event EventHandler? LanguageChanged;

    public void SetLanguage(string cultureName)
    {
        var culture = new System.Globalization.CultureInfo(cultureName);
        System.Globalization.CultureInfo.CurrentCulture = culture;
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
