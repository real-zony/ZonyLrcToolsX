namespace ZonyLrcTools.Common.Album;

/// <summary>
/// 下载歌曲对应的专辑封面。
/// </summary>
public interface IAlbumDownloader
{
    /// <summary>
    /// 使用给定的歌曲信息下载专辑图像，并输出到音乐文件同目录。
    /// </summary>
    /// <param name="needDownloadMusicInfos">需要下载专辑图像的歌曲信息。</param>
    /// <param name="parallelCount">下载线程/并发量。</param>
    /// <param name="callback">单首歌曲处理完成之后的回调方法。</param>
    /// <param name="cancellationToken">任务取消标记。</param>
    Task DownloadAsync(List<MusicInfo> needDownloadMusicInfos,
        int parallelCount = 2,
        Func<MusicInfo, Task>? callback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 可用的专辑封面来源。
    /// </summary>
    IEnumerable<IAlbumProvider> AvailableProviders { get; }
}
