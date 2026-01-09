using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Localization;

namespace ZonyLrcTools.Common.Infrastructure.Exceptions;

/// <summary>
/// 错误码相关的帮助类，支持国际化。
/// </summary>
public class ErrorCodeHelper : IErrorCodeHelper, ISingletonDependency
{
    private readonly ILocalizationService _localizationService;
    private readonly Dictionary<int, string> _fallbackMessages;

    public ErrorCodeHelper(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _fallbackMessages = new Dictionary<int, string>();
        LoadFallbackMessages();
    }

    public string GetMessage(int errorCode)
    {
        var localizedMessage = _localizationService.GetErrorMessage(errorCode);

        // 如果本地化消息不是默认的 "Unknown error" 格式，则返回本地化消息
        if (!localizedMessage.StartsWith("Unknown error:"))
        {
            return localizedMessage;
        }

        // 回退到 JSON 文件的消息
        return _fallbackMessages.TryGetValue(errorCode, out var message)
            ? message
            : $"未知错误: {errorCode}";
    }

    public string GetWarningMessage(int warningCode)
    {
        var localizedMessage = _localizationService.GetWarningMessage(warningCode);

        // 如果本地化消息不是默认的 "Unknown warning" 格式，则返回本地化消息
        if (!localizedMessage.StartsWith("Unknown warning:"))
        {
            return localizedMessage;
        }

        // 回退到 JSON 文件的消息
        return _fallbackMessages.TryGetValue(warningCode, out var message)
            ? message
            : $"未知警告: {warningCode}";
    }

    /// <summary>
    /// 从 error_msg.json 加载回退消息（用于兼容旧系统）。
    /// </summary>
    private void LoadFallbackMessages()
    {
        try
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "error_msg.json");
            if (!File.Exists(jsonPath))
            {
                return;
            }

            using var jsonReader = new JsonTextReader(File.OpenText(jsonPath));
            var jsonObj = JObject.Load(jsonReader);

            var errors = jsonObj.SelectTokens("$.Error.*");
            var warnings = jsonObj.SelectTokens("$.Warning.*");
            errors.Union(warnings).Select(m => m.Parent).OfType<JProperty>().ToList()
                .ForEach(m => _fallbackMessages[int.Parse(m.Name)] = m.Value.Value<string>() ?? string.Empty);
        }
        catch
        {
            // 忽略加载失败，使用本地化消息
        }
    }
}

/// <summary>
/// 静态错误码帮助类（用于不支持 DI 的场景）。
/// </summary>
public static class ErrorCodeHelperStatic
{
    private static readonly Dictionary<int, string> ErrorMessages = new();
    private static bool _isLoaded;

    /// <summary>
    /// 从 error_msg.json 文件加载错误信息。
    /// </summary>
    public static void LoadErrorMessage()
    {
        if (_isLoaded)
        {
            return;
        }

        try
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "error_msg.json");
            if (!File.Exists(jsonPath))
            {
                _isLoaded = true;
                return;
            }

            using var jsonReader = new JsonTextReader(File.OpenText(jsonPath));
            var jsonObj = JObject.Load(jsonReader);

            var errors = jsonObj.SelectTokens("$.Error.*");
            var warnings = jsonObj.SelectTokens("$.Warning.*");
            errors.Union(warnings).Select(m => m.Parent).OfType<JProperty>().ToList()
                .ForEach(m => ErrorMessages[int.Parse(m.Name)] = m.Value.Value<string>() ?? string.Empty);

            _isLoaded = true;
        }
        catch
        {
            _isLoaded = true;
        }
    }

    public static string GetMessage(int errorCode)
    {
        return ErrorMessages.TryGetValue(errorCode, out var message)
            ? message
            : $"未知错误: {errorCode}";
    }
}
