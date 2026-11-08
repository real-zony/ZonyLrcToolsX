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
        await _logger.InfoAsync("开始下载歌词文件数据...");

        if (parallelCount <= 0)
        {
            parallelCount = 1;
        }

        using var warpTask = new WarpTask(parallelCount);
        var downloadTasks = needDownloadMusicInfos.Select(info =>
            warpTask.RunAsync(() =>
                Task.Run(async () =>
                {
                    // 依次尝试所有可用的歌词来源，直到某一家真正返回了歌词。
                    // 某个来源返回空结果是「这家没搜到」，不是「这首歌没有歌词」，必须继续问下一家。
                    var isFinished = false;
                    var hasProviderError = false;

                    foreach (var lyricsProvider in AvailableProviders)
                    {
                        var result = await DownloadAndWriteLyricsAsync(lyricsProvider, info);

                        if (result == LyricsDownloadResult.Success)
                        {
                            _logger.LogSuccessful(info);
                            isFinished = true;
                            break;
                        }

                        if (result == LyricsDownloadResult.Failed)
                        {
                            hasProviderError = true;
                        }
                    }

                    if (!isFinished)
                    {
                        // 所有来源都没有返回歌词。若期间有来源报错，就判为失败；
                        // 否则说明各家都查无此曲，按纯音乐/未收录处理（与旧版的统计口径保持一致）。
                        info.IsSuccessful = !hasProviderError;
                        info.IsPruneMusic = !hasProviderError;
                    }

                    // 回调是调用方（GUI）提供的 UI 代码，它抛异常不应该拖垮整批下载。
                    if (callback != null)
                    {
                        try
                        {
                            await callback(info);
                        }
                        catch (Exception ex)
                        {
                            await _logger.WarnAsync($"处理歌曲下载回调时发生错误：{ex.Message}，歌曲名: {info.Name}。");
                        }
                    }
                }, cancellationToken), cancellationToken));

        await Task.WhenAll(downloadTasks);

        var successfulCount = needDownloadMusicInfos.Count(m => m is { IsSuccessful: true, IsPruneMusic: false });
        var skippedCount = needDownloadMusicInfos.Count(m => m is { IsSuccessful: true, IsPruneMusic: true });
        var failedCount = needDownloadMusicInfos.Count(m => m.IsSuccessful == false);
        await _logger.InfoAsync($"歌词数据下载完成，成功: {successfulCount} 跳过(纯音乐): {skippedCount} 失败{failedCount}。");
        await LogFailedSongFilesInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"歌词下载失败列表_{DateTime.Now:yyyyMMddHHmmss}.txt"), needDownloadMusicInfos);
    }

    private async Task<LyricsDownloadResult> DownloadAndWriteLyricsAsync(ILyricsProvider provider, MusicInfo info)
    {
        try
        {
            var lyrics = await provider.DownloadAsync(info.Name, info.Artist);

            // 该来源没有搜到歌词，交给调用方去问下一家，不能在这里判定整首歌完成。
            if (lyrics.IsPruneMusic)
            {
                return LyricsDownloadResult.Empty;
            }

            // 先把歌词编码成字节，再动磁盘。编码配置有误时（例如 fileEncoding 写成了不存在的名字）
            // 这一步就会抛错，此时文件还没被创建，不会留下 0 字节的空壳。
            var content = Utf8ToSelectedEncoding(lyrics);

            var newLyricsFilePath = Path.Combine(Path.GetDirectoryName(info.FilePath)!,
                $"{Path.GetFileNameWithoutExtension(info.FilePath)}.lrc");

            if (File.Exists(newLyricsFilePath))
            {
                File.Delete(newLyricsFilePath);
            }

            // Write lyrics to file.
            await using (var fileStream = new FileStream(newLyricsFilePath, FileMode.CreateNew, FileAccess.Write))
            {
                await fileStream.WriteAsync(content);
                await fileStream.FlushAsync();
            }

            info.IsSuccessful = true;
            info.IsPruneMusic = false;
            return LyricsDownloadResult.Success;
        }
        catch (ErrorCodeException ex)
        {
            // 失败标记必须放在记录日志之前：LoggerHelper 即便将来再改动行为，
            // 也不能让「失败」的歌曲以默认的 IsSuccessful = true 冒出去。
            info.IsSuccessful = false;
            info.IsPruneMusic = false;
            _logger.LogWarningInfo(ex);
            return LyricsDownloadResult.Failed;
        }
        catch (Exception ex)
        {
            info.IsSuccessful = false;
            info.IsPruneMusic = false;
            await _logger.ErrorAsync($"下载歌词文件时发生错误：{ex.Message}，歌曲名: {info.Name}，歌手: {info.Artist}。");
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

    private async Task LogFailedSongFilesInfo(string outFilePath, IEnumerable<MusicInfo> musicInfos)
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

        await File.WriteAllTextAsync(outFilePath, failedSongFiles.ToString());
    }
}