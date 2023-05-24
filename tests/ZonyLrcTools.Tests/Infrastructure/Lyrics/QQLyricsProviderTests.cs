using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Tests.Infrastructure.Lyrics
{
    public class QQLyricsProviderTests : TestBase
    {
        private readonly ILyricsProvider _lyricsProvider;

        public QQLyricsProviderTests()
        {
            _lyricsProvider = GetService<IEnumerable<ILyricsProvider>>()
                .FirstOrDefault(t => t.DownloaderName == InternalLyricsProviderNames.QQ);
        }

        [Fact]
        [Trait("LyricsProvider", "QQ")]
        public async Task DownloadAsync_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("东风破", "周杰伦");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }

        // About the new feature mentioned in issue #87.
        // Github Issue: https://github.com/real-zony/ZonyLrcToolsX/issues/87
        [Fact]
        public async Task DownloadAsync_Issue85_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Looking at Me", "Sabrina Carpenter");

            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBeFalse();
            lyric.ToString().ShouldContain("你好像快要不能呼吸");
        }

        [Fact]
        public async Task DownloadAsync_Issue133_Test()
        {
            await Should.ThrowAsync<ErrorCodeException>(async () => await _lyricsProvider.DownloadAsync("天ノ弱", "漆柚"));
        }
    }
}