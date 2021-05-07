using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using ZonyLrcTools.Cli.Config;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Tests.Infrastructure.Network
{
    public class WarpClientTests : TestBase
    {
        [Fact]
        public async Task PostAsync_Test()
        {
            var client = ServiceProvider.GetRequiredService<IWarpHttpClient>();

            var response = await client.PostAsync(@"https://www.baidu.com");
            response.ShouldNotBeNull();
            response.ShouldContain("百度");
        }

        [Fact]
        public async Task GetAsync_Test()
        {
            var client = ServiceProvider.GetRequiredService<IWarpHttpClient>();

            var response = await client.GetAsync(@"https://www.baidu.com");
            response.ShouldNotBeNull();
            response.ShouldContain("百度");
        }

        [Fact]
        public async Task GetAsyncWithProxy_Test()
        {
            var option = ServiceProvider.GetRequiredService<IOptions<ToolOptions>>();
            option.Value.NetworkOptions.ProxyIp = "127.0.0.1";
            option.Value.NetworkOptions.ProxyPort = 4780;

            var client = ServiceProvider.GetRequiredService<IWarpHttpClient>();

            var response = await client.GetAsync(@"https://www.baidu.com");
            
            response.ShouldNotBeNull();
            response.ShouldContain("百度");
        }
    }
}