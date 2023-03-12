using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.MusicScanner;

namespace ZonyLrcTools.Tests.MusicScanner;

public class CsvFileMusicScannerTests : TestBase
{
    [Fact]
    public async Task GetMusicInfoFromCsvFileAsync_Test()
    {
        var musicScanner = GetService<CsvFileMusicScanner>();
        var musicInfo = await musicScanner.GetMusicInfoFromCsvFileAsync(Path.Combine("TestData", "test.csv"), "DownloadedLrc", "{Artist} - {Name}.lrc");

        musicInfo.ShouldNotBeNull();
        musicInfo.Count.ShouldBeGreaterThan(0);
        musicInfo.Count.ShouldBe(5);
    }
}