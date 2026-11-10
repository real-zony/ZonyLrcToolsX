using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Threading;

namespace ZonyLrcTools.Common.Album;

/// <inheritdoc />
public class AlbumDownloader : IAlbumDownloader, ISingletonDependency
{
    private readonly IEnumerable<IAlbumProvider> _albumProviders;

    /// <inheritdoc />
    public IEnumerable<IAlbumProvider> AvailableProviders => new Lazy<IEnumerable<IAlbumProvider>>(() =>
    {
        return _albumProviders.Where(d => d.DownloaderName == InternalAlbumProviderNames.NetEase);
    }).Value;

    private readonly IWarpLogger _logger;

    /// <summary>
    /// 创建专辑封面下载器。
    /// </summary>
    public AlbumDownloader(IEnumerable<IAlbumProvider> albumProviders,
        IWarpLogger logger)
    {
        _albumProviders = albumProviders;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DownloadAsync(List<MusicInfo> needDownloadMusicInfos,
        int parallelCount = 2,
        Func<MusicInfo, Task>? callback = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var info in needDownloadMusicInfos)
        {
            info.IsSuccessful = false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var provider = AvailableProviders.FirstOrDefault(d => d.DownloaderName == InternalAlbumProviderNames.NetEase);

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

                        var filePath = Path.ChangeExtension(info.FilePath, ".png");
                        if (File.Exists(filePath))
                        {
                            info.IsSuccessful = true;
                            return;
                        }

                        if (provider == null)
                        {
                            return;
                        }

                        var album = await provider.DownloadAsync(info.Name, info.Artist);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (album.Length == 0)
                        {
                            return;
                        }

                        var temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
                        try
                        {
                            await File.WriteAllBytesAsync(temporaryPath, album, cancellationToken);
                            cancellationToken.ThrowIfCancellationRequested();
                            // 完整写入后再发布，且不覆盖其他任务或用户刚创建的封面。
                            try
                            {
                                File.Move(temporaryPath, filePath);
                            }
                            catch (IOException) when (File.Exists(filePath))
                            {
                                // 目标已由其他任务生成，保留已有封面。
                            }
                        }
                        finally
                        {
                            File.Delete(temporaryPath);
                        }

                        info.IsSuccessful = true;
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (ErrorCodeException ex)
                    {
                        _logger.LogWarningInfo(ex);
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync("下载专辑图像失败。", ex);
                    }
                    finally
                    {
                        if (callback != null)
                        {
                            try
                            {
                                await callback(info);
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                await _logger.WarnAsync("处理专辑下载回调失败。", ex);
                            }
                        }
                    }
                }, cancellationToken), cancellationToken));

        await Task.WhenAll(warpTaskList);
        cancellationToken.ThrowIfCancellationRequested();

        await _logger.InfoAsync($"专辑数据下载完成，成功: {needDownloadMusicInfos.Count(m => m.IsSuccessful)} 失败{needDownloadMusicInfos.Count(m => m.IsSuccessful == false)}。");
    }
}
