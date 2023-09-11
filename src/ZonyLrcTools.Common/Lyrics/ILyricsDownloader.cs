namespace ZonyLrcTools.Common.Lyrics;

/// <summary>
/// 歌词下载核心逻辑的接口定义。
/// </summary>
public interface ILyricsDownloader
{
    /// <summary>
    /// 使用给定的歌词信息下载歌词，并输出文件到指定的路径。
    /// </summary>
    /// <param name="needDownloadMusicInfos">需要下载的歌词信息。</param>
    /// <param name="parallelCount">下载线程/并发量。</param>
    /// <param name="cancellationToken">任务取消标记。</param>
    Task DownloadAsync(List<MusicInfo> needDownloadMusicInfos,
        int parallelCount = 2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取目前可用的歌词下载器。
    /// </summary>
    IEnumerable<ILyricsProvider> AvailableProviders { get; }
}