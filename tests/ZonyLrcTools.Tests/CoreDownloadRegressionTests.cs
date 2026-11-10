#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Album;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Extensions;
using ZonyLrcTools.Common.Infrastructure.Logging;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Tests;

/// <summary>
/// 下载结果、取消与文件保护的回归测试。
/// </summary>
public sealed class CoreDownloadRegressionTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly RecordingLogger _logger = new();

    /// <summary>
    /// 创建独立的输出目录。
    /// </summary>
    public CoreDownloadRegressionTests()
    {
        Directory.CreateDirectory(_directory);
    }

    /// <summary>
    /// 首个来源无歌词时继续查找后续来源。
    /// </summary>
    [Fact]
    public async Task Lyrics_EmptyProviderFallsBackAsync()
    {
        var info = CreateMusicInfo();
        var downloader = CreateLyricsDownloader(
            new TestLyricsProvider("empty", () => new LyricsItemCollection(new GlobalLyricsConfigOptions())),
            new TestLyricsProvider("available", CreateLyrics));

        await downloader.DownloadAsync([info]);

        Assert.True(info.IsSuccessful);
        Assert.False(info.IsPruneMusic);
        Assert.Contains("可用歌词", await File.ReadAllTextAsync(Path.ChangeExtension(info.FilePath, ".lrc")));
    }

    /// <summary>
    /// 来源失败后仍可使用后续来源。
    /// </summary>
    [Fact]
    public async Task Lyrics_FailedProviderFallsBackAsync()
    {
        var info = CreateMusicInfo();
        var failure = new InvalidOperationException("来源不可用");
        var downloader = CreateLyricsDownloader(
            new TestLyricsProvider("failed", () => throw failure),
            new TestLyricsProvider("available", CreateLyrics));

        await downloader.DownloadAsync([info]);

        Assert.True(info.IsSuccessful);
        Assert.False(info.IsPruneMusic);
        Assert.Contains("可用歌词", await File.ReadAllTextAsync(Path.ChangeExtension(info.FilePath, ".lrc")));
        Assert.Contains(failure, _logger.Exceptions);
    }

    /// <summary>
    /// 取消发生在来源返回之前时保留原歌词。
    /// </summary>
    [Fact]
    public async Task Lyrics_CancellationPreservesExistingFileAsync()
    {
        using var cancellation = new CancellationTokenSource();
        var info = CreateMusicInfo();
        var lyricsPath = Path.ChangeExtension(info.FilePath, ".lrc");
        await File.WriteAllTextAsync(lyricsPath, "原歌词");
        var downloader = CreateLyricsDownloader(new TestLyricsProvider("cancel", () =>
        {
            cancellation.Cancel();
            return CreateLyrics();
        }));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            downloader.DownloadAsync([info], cancellationToken: cancellation.Token));

        Assert.False(info.IsSuccessful);
        Assert.Equal("原歌词", await File.ReadAllTextAsync(lyricsPath));
        Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
        Assert.Empty(_logger.Exceptions);
    }

    /// <summary>
    /// 没有启用来源时不能将歌曲标记为纯音乐。
    /// </summary>
    [Fact]
    public async Task Lyrics_NoProvidersReportsFailureAsync()
    {
        using var cancellation = new CancellationTokenSource();
        var info = CreateMusicInfo();
        var downloader = CreateLyricsDownloader();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => downloader.DownloadAsync([info], callback: _ =>
        {
            cancellation.Cancel();
            return Task.CompletedTask;
        }, cancellationToken: cancellation.Token));

        Assert.False(info.IsSuccessful);
        Assert.False(info.IsPruneMusic);
        Assert.False(File.Exists(Path.ChangeExtension(info.FilePath, ".lrc")));
    }

    /// <summary>
    /// 已有封面不依赖网络结果，也不会被覆盖。
    /// </summary>
    [Fact]
    public async Task Album_ExistingFileIsSuccessfulWithoutProviderRequestAsync()
    {
        var info = CreateMusicInfo();
        var albumPath = Path.ChangeExtension(info.FilePath, ".png");
        await File.WriteAllBytesAsync(albumPath, [1, 2, 3]);
        var downloader = new AlbumDownloader(
            [new TestAlbumProvider(() => throw new InvalidOperationException("不应请求来源"))], _logger);

        await downloader.DownloadAsync([info]);

        Assert.True(info.IsSuccessful);
        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(albumPath));
        Assert.Empty(_logger.Exceptions);
    }

    /// <summary>
    /// 来源返回时已取消则不创建封面。
    /// </summary>
    [Fact]
    public async Task Album_CancellationDoesNotPublishFileAsync()
    {
        using var cancellation = new CancellationTokenSource();
        var info = CreateMusicInfo();
        var downloader = new AlbumDownloader([new TestAlbumProvider(() =>
        {
            cancellation.Cancel();
            return [1, 2, 3];
        })], _logger);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            downloader.DownloadAsync([info], cancellationToken: cancellation.Token));

        Assert.False(info.IsSuccessful);
        Assert.Empty(Directory.GetFiles(_directory));
        Assert.Empty(_logger.Exceptions);
    }

    /// <summary>
    /// 无来源时仍应遵守预先取消的令牌。
    /// </summary>
    [Fact]
    public async Task Album_NoProvidersHonorsCancellationAsync()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var downloader = new AlbumDownloader([], _logger);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            downloader.DownloadAsync([CreateMusicInfo()], cancellationToken: cancellation.Token));
    }

    /// <summary>
    /// 日志辅助方法保留异常对象且不重新抛出业务错误。
    /// </summary>
    [Fact]
    public void Logger_PreservesErrorCodeException()
    {
        var exception = new ErrorCodeException(ErrorCodes.NotSupportedFileEncoding);

        _logger.LogWarningInfo(exception);

        Assert.Same(exception, Assert.Single(_logger.Exceptions));
    }

    private MusicInfo CreateMusicInfo() => new(Path.Combine(_directory, "歌曲.mp3"), "歌曲", "歌手");

    private static LyricsItemCollection CreateLyrics() => new(new GlobalLyricsConfigOptions())
    {
        new(0, 1, "可用歌词")
    };

    private LyricsDownloader CreateLyricsDownloader(params TestLyricsProvider[] providers)
    {
        var options = new GlobalOptions
        {
            Provider = new ProviderOptions
            {
                Lyric = new LyricsOptions
                {
                    Config = new GlobalLyricsConfigOptions(),
                    Plugin = providers.Select((provider, index) => new LyricsProviderOptions
                    {
                        Name = provider.DownloaderName,
                        Priority = index
                    }).ToArray()
                }
            }
        };
        return new LyricsDownloader(providers, Options.Create(options), _logger);
    }

    /// <summary>
    /// 删除测试生成的输出。
    /// </summary>
    public void Dispose() => Directory.Delete(_directory, true);

    private sealed class TestLyricsProvider(string name, Func<LyricsItemCollection> download) : ILyricsProvider
    {
        public string DownloaderName => name;

        public ValueTask<LyricsItemCollection> DownloadAsync(string songName, string artist, long? duration = null)
            => ValueTask.FromResult(download());
    }

    private sealed class TestAlbumProvider(Func<byte[]> download) : IAlbumProvider
    {
        public string DownloaderName => InternalAlbumProviderNames.NetEase;

        public ValueTask<byte[]> DownloadAsync(string songName, string artist) => ValueTask.FromResult(download());
    }

    private sealed class RecordingLogger : IWarpLogger
    {
        public List<Exception> Exceptions { get; } = [];

        public Task DebugAsync(string message, Exception? exception = null) => RecordAsync(exception);
        public Task InfoAsync(string message, Exception? exception = null) => RecordAsync(exception);
        public Task WarnAsync(string message, Exception? exception = null) => RecordAsync(exception);
        public Task ErrorAsync(string message, Exception? exception = null) => RecordAsync(exception);

        private Task RecordAsync(Exception? exception)
        {
            if (exception != null)
            {
                Exceptions.Add(exception);
            }

            return Task.CompletedTask;
        }
    }
}
