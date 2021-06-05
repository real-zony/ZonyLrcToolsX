using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Lyric;

namespace ZonyLrcTools.Tests.Infrastructure.Lyric
{
    public class QQLyricDownloaderTests : TestBase
    {
        private readonly ILyricDownloader _lyricDownloader;

        public QQLyricDownloaderTests()
        {
            _lyricDownloader = GetService<IEnumerable<ILyricDownloader>>()
                .FirstOrDefault(t => t.DownloaderName == InternalLyricDownloaderNames.QQ);
        }
        
        [Fact]
        public async Task DownloadAsync_Test()
        {
            var lyric = await _lyricDownloader.DownloadAsync("Hollow", "Janet Leon");
            lyric.ShouldNotBeNull();
            lyric.IsPruneMusic.ShouldBe(false);
        }
    }
}