namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    public class LyricDownloaderOption
    {
        /// <summary>
        /// 歌词下载器的唯一标识。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 歌词下载时的优先级，当值为 -1 时是禁用。
        /// </summary>
        public int Priority { get; set; }
    }
}