using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Lyric;

namespace ZonyLrcTools.Tests.Infrastructure.Lyric
{
    public class KuGouLyricDownloaderTests : TestBase
    {
        [Fact]
        public async Task DownloadAsync_Test()
        {
            var downloaderList = ServiceProvider.GetRequiredService<IEnumerable<ILyricDownloader>>();
            var kuGouDownloader = downloaderList.FirstOrDefault(t => t.DownloaderName == InternalLyricDownloaderNames.KuGou);

            kuGouDownloader.ShouldNotBeNull();
            var lyric = await kuGouDownloader.DownloadAsync("东方红", null);
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }
    }
}