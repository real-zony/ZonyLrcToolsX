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
        Func<MusicInfo, Task>? callback = null,
        CancellationToken cancellationToken = default)
    {
        await _logger.InfoAsync("开始下载专辑图像数据...");

        var provider = AvailableProviders.FirstOrDefault(d => d.DownloaderName == InternalAlbumProviderNames.NetEase);
        if (provider == null)
        {
            // 没有任何可用的专辑图像源，全部标记为失败并回调，避免调用方一直等待。
            foreach (var info in needDownloadMusicInfos)
            {
                info.IsSuccessful = false;

                if (callback == null)
                {
                    continue;
                }

                try
                {
                    await callback(info);
                }
                catch (Exception ex)
                {
                    await _logger.WarnAsync($"处理专辑下载回调时发生错误：{ex.Message}，歌曲名: {info.Name}。");
                }
            }

            return;
        }

        if (parallelCount <= 0)
        {
            parallelCount = 1;
        }

        using var warpTask = new WarpTask(parallelCount);
        var warpTaskList = needDownloadMusicInfos.Select(info =>
            warpTask.RunAsync(() =>
                Task.Run(async () =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var album = await provider.DownloadAsync(info.Name, info.Artist);
                        var filePath = Path.Combine(Path.GetDirectoryName(info.FilePath)!,
                            $"{Path.GetFileNameWithoutExtension(info.FilePath)}.png");

                        if (album.Length <= 0)
                        {
                            info.IsSuccessful = false;
                            return;
                        }

                        // 已存在同名图像视为成功，不重复覆盖用户已有的封面。
                        if (File.Exists(filePath))
                        {
                            info.IsSuccessful = true;
                            return;
                        }

                        await new FileStream(filePath, FileMode.Create).WriteBytesToFileAsync(album);
                        info.IsSuccessful = true;
                        _logger.LogSuccessful(info);
                    }
                    catch (ErrorCodeException ex)
                    {
                        info.IsSuccessful = false;
                        _logger.LogWarningInfo(ex);
                    }
                    catch (OperationCanceledException)
                    {
                        info.IsSuccessful = false;
                    }
                    catch (Exception ex)
                    {
                        info.IsSuccessful = false;
                        await _logger.ErrorAsync($"下载专辑图像时发生错误：{ex.Message}，歌曲名: {info.Name}，歌手: {info.Artist}。");
                    }
                    finally
                    {
                        // 回调是调用方（GUI）提供的 UI 代码，它抛异常不应该拖垮整批下载。
                        if (callback != null)
                        {
                            try
                            {
                                await callback(info);
                            }
                            catch (Exception ex)
                            {
                                await _logger.WarnAsync($"处理专辑下载回调时发生错误：{ex.Message}，歌曲名: {info.Name}。");
                            }
                        }
                    }
                }, cancellationToken), cancellationToken));

        await Task.WhenAll(warpTaskList);

        await _logger.InfoAsync($"专辑数据下载完成，成功: {needDownloadMusicInfos.Count(m => m.IsSuccessful)} 失败{needDownloadMusicInfos.Count(m => m.IsSuccessful == false)}。");
    }
}
