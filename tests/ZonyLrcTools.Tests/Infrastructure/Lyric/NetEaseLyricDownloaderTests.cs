using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Lyric;

namespace ZonyLrcTools.Tests.Infrastructure.Lyric
{
    public class NetEaseLyricDownloaderTests : TestBase
    {
        private readonly ILyricDownloader _lyricDownloader;

        public NetEaseLyricDownloaderTests()
        {
            _lyricDownloader = GetService<IEnumerable<ILyricDownloader>>()
                .FirstOrDefault(t => t.DownloaderName == InternalLyricDownloaderNames.NetEase);
        }

        [Fact]
        public async Task DownloadAsync_Test()
        {
            var lyric = await _lyricDownloader.DownloadAsync("Hollow", "Janet Leon");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }

        [Fact]
        public async Task DownloadAsync_Issue_75_Test()
        {
            var lyric = await _lyricDownloader.DownloadAsync("Daybreak", "samfree,初音ミク");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
            lyric.ToString().Contains("惑う心繋ぎ止める").ShouldBeTrue();
        }

        [Fact]
        public async Task DownloadAsync_Issue_82_Test()
        {
            var lyric = await _lyricDownloader.DownloadAsync("シンデレラ (Giga First Night Remix)", "DECO 27 ギガP");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(true);
        }

        [Fact]
        public async Task DownloadAsync_Issue84_Test()
        {
            var lyric = await _lyricDownloader.DownloadAsync("太空彈", "01");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }
    }
}