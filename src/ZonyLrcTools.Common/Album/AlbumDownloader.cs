using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.IO;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Threading;

namespace ZonyLrcTools.Common.Album;

public class AlbumDownloader : IAlbumDownloader, ISingletonDependency
{
    private readonly IEnumerable<IAlbumProvider> _albumProviders;

    public IEnumerable<IAlbumProvider> AvailableProviders => new Lazy<IEnumerable<IAlbumProvider>>(() =>
    {
        return _albumProviders.Where(d => d.DownloaderName == InternalAlbumProviderNames.NetEase);
    }).Value;

    private readonly IWarpLogger _logger;

    public AlbumDownloader(IEnumerable<IAlbumProvider> albumProviders,
        IWarpLogger logger)
    {
        _albumProviders = albumProviders;
        _logger = logger;
    }

    public async Task DownloadAsync(List<MusicInfo> needDownloadMusicInfos,
        int parallelCount = 2,
        CancellationToken cancellationToken = default)
    {
        await _logger.InfoAsync("开始下载专辑图像数据...");

        var provider = AvailableProviders.FirstOrDefault(d => d.DownloaderName == InternalAlbumProviderNames.NetEase);
        if (provider == null)
        {
            return;
        }

        var warpTask = new WarpTask(parallelCount);
        var warpTaskList = needDownloadMusicInfos.Select(info =>
            warpTask.RunAsync(() =>
                Task.Run(async () =>
                {
                    _logger.LogSuccessful(info);

                    try
                    {
                        var album = await provider.DownloadAsync(info.Name, info.Artist);
                        var filePath = Path.Combine(Path.GetDirectoryName(info.FilePath)!, $"{Path.GetFileNameWithoutExtension(info.FilePath)}.png");
                        if (File.Exists(filePath) || album.Length <= 0)
                        {
                            return;
                        }

                        await new FileStream(filePath, FileMode.Create).WriteBytesToFileAsync(album);
                    }
                    catch (ErrorCodeException ex)
                    {
                        _logger.LogWarningInfo(ex);
                    }
                }, cancellationToken), cancellationToken));

        await Task.WhenAll(warpTaskList);

        await _logger.InfoAsync($"专辑数据下载完成，成功: {needDownloadMusicInfos.Count(m => m.IsSuccessful)} 失败{needDownloadMusicInfos.Count(m => m.IsSuccessful == false)}。");
    }
}