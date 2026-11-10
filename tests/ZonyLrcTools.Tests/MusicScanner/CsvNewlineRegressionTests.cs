using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZonyLrcTools.Common.MusicScanner;

namespace ZonyLrcTools.Tests.MusicScanner;

/// <summary>
/// CSV 换行符与取消行为的回归测试。
/// </summary>
public sealed class CsvNewlineRegressionTests
{
    /// <summary>
    /// 各平台导出的 CSV 均可读取并去除字段两端空白。
    /// </summary>
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public async Task Scan_AcceptsPlatformNewlinesAsync(string newline)
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "songs.csv");
        try
        {
            await File.WriteAllTextAsync(path,
                $"Name,Artist{newline} Song One , Artist One {newline}{newline}Song Two,Artist Two{newline}",
                new UTF8Encoding(true));
            var scanner = new CsvFileMusicScanner();

            var songs = await scanner.GetMusicInfoFromCsvFileAsync(path, directory, "{Artist} - {Name}.lrc");

            Assert.Equal(2, songs.Count);
            Assert.Equal("Song One", songs[0].Name);
            Assert.Equal("Artist One", songs[0].Artist);
            Assert.Equal(Path.Combine(directory, "Artist Two - Song Two.lrc"), songs[1].FilePath);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// 已取消的扫描不再访问文件。
    /// </summary>
    [Fact]
    public async Task Scan_PreCanceledDoesNotReadFileAsync()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var scanner = new CsvFileMusicScanner();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => scanner.GetMusicInfoFromCsvFileAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.csv"),
            Path.GetTempPath(), "{Name}.lrc", cancellation.Token));
    }
}
