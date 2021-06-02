using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Extensions
{
    /// <summary>
    /// 日志记录相关的扩展方法。
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 使用 <see cref="LogLevel.Warning"/> 级别打印错误日志，并记录异常堆栈。
        /// </summary>
        /// <param name="logger">日志记录器实例。</param>
        /// <param name="errorCode">错误码，具体请参考 <see cref="ErrorCodes"/> 类的定义。</param>
        /// <param name="e">异常实例，可为空。</param>
        public static void LogWarningWithErrorCode(this ILogger logger, int errorCode, Exception e = null)
        {
            logger.LogWarning($"错误代码: {errorCode}\n堆栈异常: {e?.StackTrace}");
        }

        /// <summary>
        /// 使用 <see cref="LogLevel.Warning"/> 级别打印错误日志，并记录异常堆栈。
        /// </summary>
        /// <param name="logger">日志记录器的实例。</param>
        /// <param name="exception">错误码异常实例。</param>
        public static void LogWarningInfo(this ILogger logger, ErrorCodeException exception)
        {
            if (exception.ErrorCode < 50000)
            {
                throw exception;
            }

            var sb = new StringBuilder();
            sb.Append($"错误代码: {exception.ErrorCode}，信息: {ErrorCodeHelper.GetMessage(exception.ErrorCode)}");
            sb.Append($"\n附加信息:\n {JsonConvert.SerializeObject(exception.AttachObject)}");
            logger.LogWarning(sb.ToString());
        }

        /// <summary>
        /// 使用 <see cref="LogLevel.Information"/> 级别打印歌曲信息。
        /// </summary>
        /// <param name="logger">日志记录器的实例。</param>
        /// <param name="musicInfo">需要打印的歌曲信息。</param>
        public static void LogMusicInfoWithInformation(this ILogger logger, MusicInfo musicInfo)
        {
            logger.LogInformation($"歌曲名: {musicInfo.Name}, 艺术家: {musicInfo.Artist}, 歌曲路径: {musicInfo.FilePath}");
        }
    }
}