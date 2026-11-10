using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Options;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Infrastructure.Threading;

namespace ZonyLrcTools.Common.Lyrics;

/// <inheritdoc cref="ZonyLrcTools.Common.Lyrics.ILyricsDownloader" />
public class LyricsDownloader : ILyricsDownloader, ISingletonDependency
{
    private readonly IEnumerable<ILyricsProvider> _lyricsProviders;
    private readonly IWarpLogger _logger;
    private readonly GlobalOptions _options;

    public IEnumerable<ILyricsProvider> AvailableProviders => new Lazy<IEnumerable<ILyricsProvider>>(() =>
    {
        return _options.Provider.Lyric.Plugin
            .Where(op => op.Priority != -1)
            .OrderBy(op => op.Priority)
            .Join(_lyricsProviders,
                op => op.Name,
                loader => loader.DownloaderName,
                (_, loader) => loader);
    }).Value;

    public LyricsDownloader(IEnumerable<ILyricsProvider> lyricsProviders,
        IOptions<GlobalOptions> options,
        IWarpLogger logger)
    {
        _lyricsProviders = lyricsProviders;
        _logger = logger;
        _options = options.Value;
    }

    public async Task DownloadAsync(List<MusicInfo> needDownloadMusicInfos,
        int parallelCount = 1,
        Func<MusicInfo, Task>? callback = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var info in needDownloadMusicInfos)
        {
            info.IsSuccessful = false;
            info.IsPruneMusic = false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var providers = AvailableProviders.ToArray();
        if (parallelCount <= 0)
        {
            parallelCount = 1;
        }

        using var warpTask = new WarpTask(parallelCount);
        var downloadTasks = needDownloadMusicInfos.Select(info =>
            warpTask.RunAsync(() =>
                Task.Run(async () =>
                {
                    try
                    {
                        var isFinished = false;
                        var hasProviderError = providers.Length == 0;

                        foreach (var lyricsProvider in providers)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var result = await DownloadAndWriteLyricsAsync(lyricsProvider, info, cancellationToken);
                            if (result == LyricsDownloadResult.Success)
                            {
                                isFinished = true;
                                break;
                            }

                            hasProviderError |= result == LyricsDownloadResult.Failed;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                        if (!isFinished)
                        {
                            // 只有所有来源均返回空结果时，才按纯音乐处理。
                            info.IsSuccessful = !hasProviderError;
                            info.IsPruneMusic = !hasProviderError;
                        }
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
                                await _logger.WarnAsync("处理歌曲下载回调失败。", ex);
                            }
                        }
                    }
                }, cancellationToken), cancellationToken));

        await Task.WhenAll(downloadTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var successfulCount = needDownloadMusicInfos.Count(m => m is { IsSuccessful: true, IsPruneMusic: false });
        var skippedCount = needDownloadMusicInfos.Count(m => m is { IsSuccessful: true, IsPruneMusic: true });
        var failedCount = needDownloadMusicInfos.Count(m => m.IsSuccessful == false);
        await _logger.InfoAsync($"歌词数据下载完成，成功: {successfulCount} 跳过(纯音乐): {skippedCount} 失败{failedCount}。");
        await LogFailedSongFilesInfoAsync(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"歌词下载失败列表_{DateTime.UtcNow:yyyyMMddHHmmss}.txt"),
            needDownloadMusicInfos, cancellationToken);
    }

    private async Task<LyricsDownloadResult> DownloadAndWriteLyricsAsync(ILyricsProvider provider, MusicInfo info,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var lyrics = await provider.DownloadAsync(info.Name, info.Artist);
            cancellationToken.ThrowIfCancellationRequested();

            // 空结果只表示当前来源未找到歌词，继续尝试其他来源。
            if (lyrics.IsPruneMusic)
            {
                return LyricsDownloadResult.Empty;
            }

            // 编码成功后再创建文件，避免配置错误破坏原歌词。
            var content = Utf8ToSelectedEncoding(lyrics);

            var newLyricsFilePath = Path.ChangeExtension(info.FilePath, ".lrc");
            var temporaryPath = $"{newLyricsFilePath}.{Guid.NewGuid():N}.tmp";
            try
            {
                await File.WriteAllBytesAsync(temporaryPath, content, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                File.Move(temporaryPath, newLyricsFilePath, overwrite: true);
            }
            finally
            {
                File.Delete(temporaryPath);
            }

            info.IsSuccessful = true;
            info.IsPruneMusic = false;
            return LyricsDownloadResult.Success;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ErrorCodeException ex)
        {
            info.IsSuccessful = false;
            info.IsPruneMusic = false;
            _logger.LogWarningInfo(ex);
            return LyricsDownloadResult.Failed;
        }
        catch (Exception ex)
        {
            info.IsSuccessful = false;
            info.IsPruneMusic = false;
            await _logger.WarnAsync("当前歌词来源下载失败，将尝试其他来源。", ex);
            return LyricsDownloadResult.Failed;
        }
    }

    /// <summary>
    /// 单个歌词来源的处理结果。
    /// </summary>
    private enum LyricsDownloadResult
    {
        /// <summary>歌词已写入文件。</summary>
        Success,

        /// <summary>该来源没有返回歌词（既可能是没有收录，也可能是纯音乐）。</summary>
        Empty,

        /// <summary>该来源处理过程中出错。</summary>
        Failed
    }

    // Convert UTF-8 to selected encoding.
    private byte[] Utf8ToSelectedEncoding(LyricsItemCollection lyrics)
    {
        var supportEncodings = Encoding.GetEncodings();

        // If the encoding is UTF-8 BOM, add the BOM to the front of the lyrics.
        if (_options.Provider.Lyric.Config.FileEncoding == "utf-8-bom")
        {
            return Encoding.UTF8.GetPreamble().Concat(lyrics.GetUtf8Bytes()).ToArray();
        }

        if (supportEncodings.All(x => x.Name != _options.Provider.Lyric.Config.FileEncoding))
        {
            throw new ErrorCodeException(ErrorCodes.NotSupportedFileEncoding);
        }

        return Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(_options.Provider.Lyric.Config.FileEncoding), lyrics.GetUtf8Bytes());
    }

    private async Task LogFailedSongFilesInfoAsync(string outFilePath, IEnumerable<MusicInfo> musicInfos,
        CancellationToken cancellationToken = default)
    {
        var failedSongList = musicInfos.Where(m => m.IsSuccessful == false).ToImmutableList();
        if (!failedSongList.Any())
        {
            return;
        }

        var failedSongFiles = new StringBuilder();
        failedSongFiles.AppendLine("歌曲名,歌手,路径");

        foreach (var failedSongFile in failedSongList)
        {
            failedSongFiles.AppendLine($"{failedSongFile.Name},{failedSongFile.Artist},{failedSongFile.FilePath}");
        }

        await File.WriteAllTextAsync(outFilePath, failedSongFiles.ToString(), cancellationToken);
    }
}