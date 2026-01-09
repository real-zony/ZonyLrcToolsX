namespace ZonyLrcTools.Common.Infrastructure.Exceptions;

/// <summary>
/// 错误码帮助类接口。
/// </summary>
public interface IErrorCodeHelper
{
    /// <summary>
    /// 获取错误消息。
    /// </summary>
    /// <param name="errorCode">错误代码。</param>
    /// <returns>对应的错误消息。</returns>
    string GetMessage(int errorCode);

    /// <summary>
    /// 获取警告消息。
    /// </summary>
    /// <param name="warningCode">警告代码。</param>
    /// <returns>对应的警告消息。</returns>
    string GetWarningMessage(int warningCode);
}
