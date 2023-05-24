using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Tests.Infrastructure.Lyrics
{
    public class NetEaseLyricsProviderTests : TestBase
    {
        private readonly ILyricsProvider _lyricsProvider;

        public NetEaseLyricsProviderTests()
        {
            _lyricsProvider = GetService<IEnumerable<ILyricsProvider>>()
                .FirstOrDefault(t => t.DownloaderName == InternalLyricsProviderNames.NetEase);
        }

        [Fact]
        [Trait("LyricsProvider ", "NetEase")]
        public async Task DownloadAsync_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Hollow", "Janet Leon");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }

        [Fact]
        public async Task DownloadAsync_Issue_75_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Daybreak", "samfree,初音ミク");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
            lyric.ToString().Contains("惑う心繋ぎ止める").ShouldBeTrue();
        }

        [Fact]
        public async Task DownloadAsync_Issue_82_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("シンデレラ (Giga First Night Remix)", "DECO 27 ギガP");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }

        [Fact]
        public async Task DownloadAsync_Issue84_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("太空彈", "01");
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
            lyric.ToString().ShouldContain("你看起来失了呼吸");
        }

        [Fact]
        public async Task DownloaderAsync_Issue88_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("茫茫草原", "姚璎格");

            lyric.ShouldNotBeNull();
        }

        [Fact]
        public async Task UnknownIssue_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("主題歌Arrietty's Song", "Cécile Corbel");

            lyric.ShouldNotBeNull();
        }

        [Fact]
        public async Task DownloaderAsync_Issue101_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("君への嘘", "VALSHE");
            lyric.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task DownloadAsync_Issue114_Test()
        {
            var options = ServiceProvider.GetRequiredService<IOptions<GlobalOptions>>();
            options.Value.Provider.Lyric.Config.IsOnlyOutputTranslation = true;

            var lyric = await _lyricsProvider.DownloadAsync("Bones", "Image Dragons");
            lyric.ToString().ShouldNotContain("Gimme, gimme, gimme some time to think");
        }

        [Fact]
        public async Task DownloadAsync_Issue123_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("橄榄树", "苏曼");
        }

        [Fact]
        public async Task DownloadAsync_Issue133_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Everything", "Yinyues");
            lyric.IsPruneMusic.ShouldBeTrue();
        }

        [Fact]
        public async Task DownloadAsync_NullException_Test()
        {
            var result = await Should.ThrowAsync<ErrorCodeException>(_lyricsProvider.DownloadAsync("創世記", "りりィ").AsTask);
            result.ErrorCode.ShouldBe(ErrorCodes.NoMatchingSong);
        }

        [Fact]
        public async Task DownloadAsync_Source_Null_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Concerto for Piano and Orchestra No. 12 in A major, K414 - 1. Allegro",
                "Wolfgang Amadeus Mozart");

            lyric.IsPruneMusic.ShouldBeTrue();
        }

        [Fact]
        public async Task DownloadAsync_Issue_20230429_01_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Don't Go Away", "By2 Twins+《Twins》");

            lyric.ShouldContain(x => x.LyricText.Contains("吴庆隆"));
        }

        [Fact]
        public async Task DownloadAsync_Issue_20230429_02_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Hanging On (2009)", "Britt Nicole");
            lyric.ShouldContain(x => x.LyricText.Contains("You see my anxious heart"));
        }

        [Fact]
        public async Task DownloadAsync_Issue_20230429_03_Test()
        {
            var lyric = await _lyricsProvider.DownloadAsync("Your Heart Is A Muscle (2012)", "Carly Rae Jepsen");
            lyric.IsPruneMusic.ShouldBeFalse();
        }
    }
}