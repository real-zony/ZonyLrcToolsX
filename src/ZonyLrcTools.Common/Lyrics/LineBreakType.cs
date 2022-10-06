namespace ZonyLrcTools.Common.Lyrics
{
    /// <summary>
    /// 换行符格式定义。
    /// </summary>
    public static class LineBreakType
    {
        /// <summary>
        /// Windows 系统。
        /// </summary>
        public const string Windows = "\r\n";

        /// <summary>
        /// macOS 系统。
        /// </summary>
        public const string MacOs = "\r";

        /// <summary>
        /// UNIX 系统(Linux)。
        /// </summary>
        public const string Unix = "\n";
    }
}