namespace ZonyLrcTools.Common.Lyrics;

public interface ILyricsDownloader
{
    Task DownloadAsync(List<MusicInfo> needDownloadMusicInfos,
        int parallelCount = 2,
        CancellationToken cancellationToken = default);

    IEnumerable<ILyricsProvider> AvailableProviders { get; }
}