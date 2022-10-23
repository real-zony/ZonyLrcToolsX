namespace ZonyLrcTools.Common.Album
{
    /// <summary>
    /// 专辑封面下载器，用于匹配并下载歌曲的专辑封面。
    /// </summary>
    public interface IAlbumProvider
    {
        /// <summary>
        /// 下载器的名称。
        /// </summary>
        string DownloaderName { get; }

        /// <summary>
        /// 下载专辑封面。
        /// </summary>
        /// <param name="songName">歌曲的名称。</param>
        /// <param name="artist">歌曲的作者。</param>
        /// <returns>专辑封面的图像数据。</returns>
        ValueTask<byte[]> DownloadAsync(string songName, string artist);
    }
}