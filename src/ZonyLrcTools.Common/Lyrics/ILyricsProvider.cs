namespace ZonyLrcTools.Common.Lyrics
{
    /// <summary>
    /// 歌词数据下载器，用于匹配并下载歌曲的歌词。
    /// </summary>
    public interface ILyricsProvider
    {
        /// <summary>
        /// 下载歌词数据。
        /// </summary>
        /// <param name="songName">歌曲的名称。</param>
        /// <param name="artist">歌曲的作者。</param>
        /// <param name="duration">歌曲的时长。</param>
        /// <returns>歌曲的歌词数据对象。</returns>
        ValueTask<LyricsItemCollection> DownloadAsync(string songName, string artist, long? duration = null);

        /// <summary>
        /// 下载器的名称。
        /// </summary>
        string DownloaderName { get; }
    }
}