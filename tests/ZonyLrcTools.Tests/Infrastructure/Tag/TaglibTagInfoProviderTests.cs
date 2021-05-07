using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Infrastructure.Tag;

namespace ZonyLrcTools.Tests.Infrastructure.Tag
{
    public class TaglibTagInfoProviderTests : TestBase
    {
        [Fact]
        public async Task LoadAsync_Test()
        {
            var provider = ServiceProvider.GetRequiredService<IEnumerable<ITagInfoProvider>>()
                .FirstOrDefault(p => p.Name == TaglibTagInfoProvider.ConstantName);

            provider.ShouldNotBeNull();

            var info = await provider.LoadAsync(Path.Combine(Directory.GetCurrentDirectory(), "MusicFiles", "曾经艺也 - 荀彧(纯音乐版).mp3"));
            info.ShouldNotBeNull();
            info.Name.ShouldBe("荀彧(纯音乐版)");
            info.Artist.ShouldBe("曾经艺也");
        }
    }
}