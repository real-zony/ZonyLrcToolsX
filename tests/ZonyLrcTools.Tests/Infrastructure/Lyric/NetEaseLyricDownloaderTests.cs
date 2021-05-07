using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Lyric;

namespace ZonyLrcTools.Tests.Infrastructure.Lyric
{
    public class NetEaseLyricDownloaderTests : TestBase
    {
        [Fact]
        public async Task DownloadAsync_Test()
        {
            var downloaderList = ServiceProvider.GetRequiredService<IEnumerable<ILyricDownloader>>();
            var netEaseDownloader = downloaderList.FirstOrDefault(t => t.DownloaderName == InternalLyricDownloaderNames.NetEase);

            netEaseDownloader.ShouldNotBeNull();
            var lyric = await netEaseDownloader.DownloadAsync("Hollow", "Janet Leon");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }
    }
}