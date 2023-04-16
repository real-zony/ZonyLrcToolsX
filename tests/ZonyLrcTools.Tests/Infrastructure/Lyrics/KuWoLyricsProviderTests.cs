using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Tests.Infrastructure.Lyrics;

public class KuWoLyricsProviderTests : TestBase
{
    private readonly ILyricsProvider _kuwoLyricsProvider;

    public KuWoLyricsProviderTests()
    {
        _kuwoLyricsProvider = GetService<IEnumerable<ILyricsProvider>>()
            .FirstOrDefault(t => t.DownloaderName == InternalLyricsProviderNames.KuWo);
    }

    [Fact]
    [Trait("LyricsProvider ", "KuGou")]
    public async Task DownloadAsync_Test()
    {
        var lyric = await _kuwoLyricsProvider.DownloadAsync("告白气球", "周杰伦");
        lyric.ShouldNotBeNull();
        lyric.IsPruneMusic.ShouldBeFalse();
    }

    [Fact]
    public async Task DownloadAsync_Source_Null_Test()
    {
        var lyric = await _kuwoLyricsProvider.DownloadAsync("Concerto for Piano and Orchestra No. 12 in A major, K414 - 1. Allegro",
            "Wolfgang Amadeus Mozart");

        lyric.IsPruneMusic.ShouldBeTrue();
    }
}