using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Lyric;

namespace ZonyLrcTools.Tests.Infrastructure.Lyric
{
    public class KuGouLyricDownloaderTests : TestBase
    {
        private readonly ILyricDownloader _lyricDownloader;

        public KuGouLyricDownloaderTests()
        {
            _lyricDownloader = GetService<IEnumerable<ILyricDownloader>>()
                .FirstOrDefault(t => t.DownloaderName == InternalLyricDownloaderNames.KuGou);
        }

        [Fact]
        public async Task DownloadAsync_Test()
        {
            var lyric = await _lyricDownloader.DownloadAsync("东方红", null);
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }
    }
}