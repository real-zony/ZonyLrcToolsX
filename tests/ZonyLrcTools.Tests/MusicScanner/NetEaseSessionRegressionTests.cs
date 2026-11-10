#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.MusicScanner;

namespace ZonyLrcTools.Tests.MusicScanner;

/// <summary>
/// 网易云登录状态、并发与 HTTP 错误的回归测试。
/// </summary>
public sealed class NetEaseSessionRegressionTests
{
    /// <summary>
    /// 登录成功回调只能在 Cookie 保存后发出。
    /// </summary>
    [Fact]
    public async Task Login_SuccessCallbackSeesCommittedSessionAsync()
    {
        using var handler = new TestHandler((request, _) => Task.FromResult(
            IsKeyRequest(request) ? KeyResponse() : SuccessResponse()));
        var scanner = CreateScanner(handler);
        var observedLogin = false;

        var success = await scanner.LoginAsync(_ => { }, state =>
        {
            if (state == NetEaseLoginState.Succeeded)
            {
                observedLogin = scanner.IsLoggedIn && scanner.CsrfToken == "csrf-value";
            }
        });

        Assert.True(success);
        Assert.True(observedLogin);
        Assert.Contains("MUSIC_U=session-value", scanner.Cookie);
    }

    /// <summary>
    /// 并发登录复用已经建立的会话，不生成互相干扰的二维码。
    /// </summary>
    [Fact]
    public async Task Login_ConcurrentRequestsUseOneQrSessionAsync()
    {
        var releaseKey = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var keyRequests = 0;
        using var handler = new TestHandler(async (request, cancellationToken) =>
        {
            if (IsKeyRequest(request))
            {
                Interlocked.Increment(ref keyRequests);
                await releaseKey.Task.WaitAsync(cancellationToken);
                return KeyResponse();
            }

            return SuccessResponse();
        });
        var scanner = CreateScanner(handler);

        var first = scanner.LoginAsync(_ => { });
        var second = scanner.LoginAsync(_ => { });
        releaseKey.SetResult();
        var results = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.All(results, result => Assert.True(result));
        Assert.Equal(1, keyRequests);
        Assert.True(scanner.IsLoggedIn);
    }

    /// <summary>
    /// 清除登录后，迟到的扫码结果不能恢复已退出的会话。
    /// </summary>
    [Fact]
    public async Task Login_ClearStateRejectsPendingResultAsync()
    {
        var releaseKey = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new TestHandler(async (request, cancellationToken) =>
        {
            if (IsKeyRequest(request))
            {
                await releaseKey.Task.WaitAsync(cancellationToken);
                return KeyResponse();
            }

            return SuccessResponse();
        });
        var scanner = CreateScanner(handler);
        var login = scanner.LoginAsync(_ => { });

        scanner.ClearLoginState();
        releaseKey.SetResult();
        var success = await login.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(success);
        Assert.False(scanner.IsLoggedIn);
        Assert.Empty(scanner.CsrfToken);
    }

    /// <summary>
    /// 恢复的会话不能被旧扫码请求覆盖。
    /// </summary>
    [Fact]
    public async Task Login_RestoreStateRejectsPendingResultAsync()
    {
        var releaseKey = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new TestHandler(async (request, cancellationToken) =>
        {
            if (IsKeyRequest(request))
            {
                await releaseKey.Task.WaitAsync(cancellationToken);
                return KeyResponse();
            }

            return SuccessResponse();
        });
        var scanner = CreateScanner(handler);
        var login = scanner.LoginAsync(_ => { });

        scanner.RestoreLoginState("MUSIC_U=restored", "restored-csrf");
        releaseKey.SetResult();
        await login.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal("MUSIC_U=restored", scanner.Cookie);
        Assert.Equal("restored-csrf", scanner.CsrfToken);
    }

    /// <summary>
    /// HTTP 失败不能被响应中的成功业务码掩盖。
    /// </summary>
    [Fact]
    public async Task Login_HttpFailureIsNotSuccessfulAsync()
    {
        using var handler = new TestHandler((request, _) =>
        {
            var response = IsKeyRequest(request) ? KeyResponse() : SuccessResponse();
            response.StatusCode = HttpStatusCode.ServiceUnavailable;
            return Task.FromResult(response);
        });
        var scanner = CreateScanner(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => scanner.LoginAsync(_ => { }));

        Assert.False(scanner.IsLoggedIn);
    }

    /// <summary>
    /// 非扫码状态业务码应失败，而不是永久等待扫码。
    /// </summary>
    [Theory]
    [InlineData("{\"code\":500}")]
    [InlineData("{\"code\":803}")]
    public async Task Login_InvalidPollResponseFailsAsync(string body)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var handler = new TestHandler((request, _) => Task.FromResult(
            IsKeyRequest(request) ? KeyResponse() : JsonResponse(body)));
        var scanner = CreateScanner(handler);

        await Assert.ThrowsAsync<ErrorCodeException>(() =>
            scanner.LoginAsync(_ => { }, cancellationToken: cancellation.Token));

        Assert.False(scanner.IsLoggedIn);
    }

    /// <summary>
    /// 二维码过期时结束登录并通知调用方。
    /// </summary>
    [Fact]
    public async Task Login_ExpiredQrReturnsFalseAsync()
    {
        using var handler = new TestHandler((request, _) => Task.FromResult(
            IsKeyRequest(request) ? KeyResponse() : JsonResponse("{\"code\":800}")));
        var scanner = CreateScanner(handler);
        var lastState = NetEaseLoginState.Waiting;

        var success = await scanner.LoginAsync(_ => { }, state => lastState = state);

        Assert.False(success);
        Assert.Equal(NetEaseLoginState.Expired, lastState);
    }

    /// <summary>
    /// 关闭扫码窗口可以取消正在等待的 HTTP 请求。
    /// </summary>
    [Fact]
    public async Task Login_CancellationReachesHttpRequestAsync()
    {
        using var cancellation = new CancellationTokenSource();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new TestHandler(async (_, cancellationToken) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return KeyResponse();
        });
        var scanner = CreateScanner(handler);
        var login = scanner.LoginAsync(_ => { }, cancellationToken: cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => login.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(scanner.IsLoggedIn);
    }

    private static NetEaseMusicSongListMusicScanner CreateScanner(HttpMessageHandler handler)
        => new(new DefaultWarpHttpClient(new TestClientFactory(handler)),
            NullLogger<NetEaseMusicSongListMusicScanner>.Instance);

    private static bool IsKeyRequest(HttpRequestMessage request)
        => request.RequestUri!.AbsolutePath.EndsWith("/unikey", StringComparison.Ordinal);

    private static HttpResponseMessage KeyResponse() => JsonResponse("{\"code\":200,\"unikey\":\"qr-key\"}");

    private static HttpResponseMessage SuccessResponse()
    {
        var response = JsonResponse("{\"code\":803}");
        response.Headers.TryAddWithoutValidation("Set-Cookie", new[]
        {
            "MUSIC_U=session-value; Path=/; Domain=music.163.com; HttpOnly",
            "__csrf=csrf-value; Path=/; Domain=music.163.com"
        });
        return response;
    }

    private static HttpResponseMessage JsonResponse(string body)
        => new(HttpStatusCode.OK) { Content = new StringContent(body) };

    private sealed class TestClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class TestHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
