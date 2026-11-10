using ZonyLrcTools.Common.Configuration;

namespace ZonyLrcTools.Desktop.Infrastructure.Configuration;

/// <summary>设置保存结果。</summary>
/// <param name="IsSuccess">是否保存成功。</param>
/// <param name="FilePath">保存成功的文件路径。</param>
/// <param name="ErrorMessage">保存失败的原因。</param>
public readonly record struct SettingsSaveResult(bool IsSuccess, string? FilePath, string? ErrorMessage)
{
    /// <summary>创建保存成功的结果。</summary>
    public static SettingsSaveResult Success(string filePath) => new(true, filePath, null);

    /// <summary>创建保存失败的结果。</summary>
    public static SettingsSaveResult Failure(string errorMessage) => new(false, null, errorMessage);
}

/// <summary>统一管理共享配置及其持久化。</summary>
public interface ISettingsStore
{
    /// <summary>应用共享配置，修改后对按操作读取配置的组件立即生效。</summary>
    GlobalOptions Options { get; }

    /// <summary>当前配置文件的完整路径。</summary>
    string ConfigFilePath { get; }

    /// <summary>保存界面管理的配置值，不覆盖无法安全修改的文件。</summary>
    Task<SettingsSaveResult> SaveAsync(CancellationToken cancellationToken = default);
}
