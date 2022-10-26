using Shouldly;
using Xunit;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Lyrics;

namespace ZonyLrcTools.Tests.Infrastructure.Lyrics
{
    public class LyricCollectionTests : TestBase
    {
        [Fact]
        public void LyricCollectionLineBreak_Test()
        {
            var lyricObject = new LyricsItemCollection(new GlobalLyricsConfigOptions
            {
                IsOneLine = false,
                LineBreak = LineBreakType.MacOs
            })
            {
                new(0, 20, "你好世界!"),
                new(0, 22, "Hello World!")
            };

            lyricObject.ToString().ShouldContain(LineBreakType.MacOs);
        }
    }
}