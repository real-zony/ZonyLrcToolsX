namespace ZonyLrcTools.Common.Infrastructure.Extensions
{
    /// <summary>
    /// 字符串处理相关的工具方法。
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 截断指定字符串末尾的匹配字串。
        /// </summary>
        /// <param name="string">待截断的字符串。</param>
        /// <param name="trimEndStr">需要在末尾截断的字符串。</param>
        /// <returns>截断成功的字符串实例。</returns>
        public static string TrimEnd(this string @string, string trimEndStr)
        {
            if (@string.EndsWith(trimEndStr, StringComparison.Ordinal))
            {
                return @string.Substring(0, @string.Length - trimEndStr.Length);
            }

            return @string;
        }
    }
}