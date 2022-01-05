using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Tag;

namespace ZonyLrcTools.Tests.Infrastructure.Tag
{
    public class TagLoaderTests : TestBase
    {
        [Fact]
        public async Task LoadTagAsync_Test()
        {
            var tagLoader = ServiceProvider.GetRequiredService<ITagLoader>();

            tagLoader.ShouldNotBeNull();
            var info = await tagLoader.LoadTagAsync(@"D:\はるまきごはん 煮ル果実 くらげP 蜂屋ななし じん かいりきベア  - ダンスロボットダンス (アレンジメドレー (キメラver) はるまきごはん×煮ル果実×和田たけあき×栗山夕璃（蜂屋.flac");
            info.ShouldNotBeNull();
            info.Name.ShouldBe("荀彧(纯音乐版)");
            info.Artist.ShouldBe("曾经艺也");
        }
    }
}