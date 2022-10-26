using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Tests.Infrastructure.Lyrics
{
    public class KuGouLyricProviderTests : TestBase
    {
        private readonly ILyricsProvider _lyricsProvider;

        public KuGouLyricProviderTests()
        {
            _lyricsProvider = GetService<IEnumerable<ILyricsProvider>>()
                .FirstOrDefault(t => t.DownloaderName == InternalLyricsProviderNames.KuGou);
        }

        [Fact]
        public async Task DownloadAsync_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("东方红", null);
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }
    }
}