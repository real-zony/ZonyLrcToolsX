using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Common.TagInfo;

namespace ZonyLrcTools.Tests.Infrastructure.Tag
{
    public class TagLoaderTests : TestBase
    {
        [Fact]
        public async Task LoadTagAsync_Test()
        {
            var tagLoader = ServiceProvider.GetRequiredService<ITagLoader>();

            tagLoader.ShouldNotBeNull();
            var info = await tagLoader.LoadTagAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MusicFiles", "曾经艺也 - 荀彧(纯音乐版).mp3"));
            info.ShouldNotBeNull();
            info.Name.ShouldBe("荀彧(纯音乐版)");
            info.Artist.ShouldBe("曾经艺也");
        }
    }
}