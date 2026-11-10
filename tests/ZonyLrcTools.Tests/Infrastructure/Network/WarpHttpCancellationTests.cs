using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ZonyLrcTools.Common.Infrastructure.Network;

namespace ZonyLrcTools.Tests.Infrastructure.Network;

/// <summary>
/// 验证等待服务器响应时可以取消请求。
/// </summary>
public class WarpHttpCancellationTests
{
    /// <summary>
    /// 服务端未响应时，取消操作应及时结束请求。
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancelsPendingRequestAsync(bool isPost)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var url = $"http://127.0.0.1:{endpoint.Port}/";
        using var cancellation = new CancellationTokenSource();
        var client = new DefaultWarpHttpClient(new LocalClientFactory());
        var request = SendRequestAsync(client, url, isPost, cancellation.Token);

        using var connection = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    private static async Task SendRequestAsync(IWarpHttpClient client, string url, bool isPost,
        CancellationToken cancellationToken = default)
    {
        if (isPost)
        {
            using var response = await client.PostReturnHttpResponseAsync(url, cancellationToken: cancellationToken);
        }
        else
        {
            await client.GetAsync(url, cancellationToken: cancellationToken);
        }
    }

    private sealed class LocalClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new SocketsHttpHandler { UseProxy = false });
        }
    }
}
