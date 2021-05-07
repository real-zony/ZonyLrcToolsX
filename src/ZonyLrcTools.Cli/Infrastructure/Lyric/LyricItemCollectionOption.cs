namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    public class LyricItemCollectionOption
    {
        /// <summary>
        /// 双语歌词是否合并为一行。
        /// </summary>
        public bool IsOneLine { get; set; } = false;

        /// <summary>
        /// 换行符格式，取值来自 <see cref="LineBreakType"/> 常量类。
        /// </summary>
        public string LineBreak { get; set; } = LineBreakType.Windows;

        public static readonly LyricItemCollectionOption NullInstance = new();
    }
}