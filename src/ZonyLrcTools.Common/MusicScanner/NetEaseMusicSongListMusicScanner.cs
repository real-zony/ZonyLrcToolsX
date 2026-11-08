using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Encryption;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.MusicScanner.JsonModel;

namespace ZonyLrcTools.Common.MusicScanner;

/// <summary>
/// 网易云歌单音乐扫描器，用于从网易云歌单获取需要下载的歌词列表。
/// </summary>
public class NetEaseMusicSongListMusicScanner : ISingletonDependency
{
    private readonly IWarpHttpClient _warpHttpClient;
    private readonly ILogger<NetEaseMusicSongListMusicScanner> _logger;
    private const string Host = "https://music.163.com";

    /// <summary>
    /// 登录后的 Cookie。GUI 可以在登录成功之后把它持久化下来，下次直接赋值恢复登录态。
    /// </summary>
    public string Cookie { get; set; } = string.Empty;

    /// <summary>
    /// 登录后的 CSRF Token，通常与 <see cref="Cookie"/> 一起持久化。
    /// </summary>
    public string CsrfToken { get; set; } = string.Empty;

    /// <summary>
    /// 当前是否已经持有登录态。
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(Cookie);

    /// <summary>
    /// 构建一个新的 <see cref="NetEaseMusicSongListMusicScanner"/> 对象。
    /// </summary>
    public NetEaseMusicSongListMusicScanner(IWarpHttpClient warpHttpClient,
        ILogger<NetEaseMusicSongListMusicScanner> logger)
    {
        _warpHttpClient = warpHttpClient;
        _logger = logger;
    }

    /// <summary>
    /// 清除当前登录态，下次调用歌单接口时会重新要求扫码。
    /// </summary>
    public void ClearLoginState()
    {
        Cookie = string.Empty;
        CsrfToken = string.Empty;
    }

    /// <summary>
    /// 恢复一个已有的登录态，供 GUI 从本地缓存中读回 Cookie 之后调用。
    /// </summary>
    /// <param name="cookie">之前登录成功时保存的 Cookie。</param>
    /// <param name="csrfToken">与 <paramref name="cookie"/> 一起保存的 CSRF Token。</param>
    public void RestoreLoginState(string cookie, string csrfToken)
    {
        Cookie = cookie ?? string.Empty;
        CsrfToken = csrfToken ?? string.Empty;
    }

    /// <summary>
    /// 主动触发一次扫码登录（如果已经登录则直接返回 true）。
    /// </summary>
    /// <param name="onQrCodeReady">二维码就绪回调。</param>
    /// <param name="onStateChanged">登录状态变化回调，GUI 用它显示「已扫码，请在手机上确认」之类的中间态。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>登录成功时为 true。</returns>
    public async Task<bool> LoginAsync(
        Action<NetEaseQrCodePayload>? onQrCodeReady = null,
        Action<NetEaseLoginState>? onStateChanged = null,
        CancellationToken cancellationToken = default)
    {
        if (IsLoggedIn)
        {
            onStateChanged?.Invoke(NetEaseLoginState.Succeeded);
            return true;
        }

        var loginResponse = await LoginViqQrCodeAsync(onQrCodeReady, onStateChanged, cancellationToken);
        Cookie = loginResponse.cookieContainer?.GetCookieHeader(new Uri(Host)) ?? string.Empty;
        CsrfToken = loginResponse.csrfToken ?? string.Empty;

        return IsLoggedIn;
    }

    /// <summary>
    /// 从网易云歌单获取需要下载的歌曲列表，调用这个 API 需要用户登录，否则获取的歌单数据不全。
    /// </summary>
    /// <param name="songListIds">网易云音乐歌单的 ID，多个歌单以分号分隔。</param>
    /// <param name="outputDirectory">歌词文件的输出路径。</param>
    /// <param name="pattern">输出的歌词文件格式，默认是 "{Artist} - {Title}.lrc" 的形式。</param>
    /// <param name="onQrCodeReady">
    /// 二维码就绪时的回调。CLI 不传，走原有的终端 ASCII 输出；GUI 传入后会收到可渲染的二维码数据。
    /// </param>
    /// <param name="onStateChanged">登录状态变化回调。</param>
    /// <param name="cancellationToken">取消令牌，用于中断扫码等待与歌单请求。</param>
    /// <returns>返回获取到的歌曲列表。</returns>
    public async Task<List<MusicInfo>> GetMusicInfoFromNetEaseMusicSongListAsync(
        string songListIds,
        string outputDirectory,
        string pattern,
        Action<NetEaseQrCodePayload>? onQrCodeReady = null,
        Action<NetEaseLoginState>? onStateChanged = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Cookie))
        {
            var loginResponse = await LoginViqQrCodeAsync(onQrCodeReady, onStateChanged, cancellationToken);
            Cookie = loginResponse.cookieContainer?.GetCookieHeader(new Uri(Host)) ?? string.Empty;
            CsrfToken = loginResponse.csrfToken ?? string.Empty;

            if (string.IsNullOrEmpty(Cookie))
            {
                // 登录没成（用户取消、二维码过期），直接失败，不要再拿空 Cookie 去请求歌单。
                throw new ErrorCodeException(ErrorCodes.UnableGetSongListFromNetEaseCloudMusic);
            }
        }

        async Task<List<MusicInfo>> GetMusicInfoBySongIdAsync(string songId)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var secretKey = NetEaseMusicEncryptionHelper.CreateSecretKey(16);
            var encSecKey = NetEaseMusicEncryptionHelper.RsaEncode(secretKey);
            var response = await _warpHttpClient.PostAsync<GetMusicInfoFromNetEaseMusicSongListResponse>(
                $"{Host}/weapi/v6/playlist/detail?csrf_token={CsrfToken}", requestOption:
                request =>
                {
                    request.Headers.Add("Cookie", Cookie);
                    request.Content = new FormUrlEncodedContent(HandleRequest(new
                    {
                        csrf_token = CsrfToken,
                        id = songId,
                        n = 1000,
                        offset = 0,
                        total = true,
                        limit = 1000,
                    }, secretKey, encSecKey));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                });

            if (response.Code != 200 || response.PlayList?.SongList == null)
            {
                throw new ErrorCodeException(ErrorCodes.UnableGetSongListFromNetEaseCloudMusic);
            }

            return response.PlayList.SongList
                .Where(song => !string.IsNullOrEmpty(song.Name))
                .Select(song =>
                {
                    var artistName = song.Artists?.FirstOrDefault()?.Name ?? string.Empty;
                    var fakeFilePath = Path.Combine(outputDirectory, pattern.Replace("{Name}", song.Name).Replace("{Artist}", artistName));

                    return new MusicInfo(fakeFilePath, song.Name!, artistName);
                }).ToList();
        }

        var musicInfoList = new List<MusicInfo>();
        foreach (var songListId in songListIds.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("正在获取歌单 {SongListId} 的歌曲列表。", songListId);
            var musicInfos = await GetMusicInfoBySongIdAsync(songListId);
            musicInfoList.AddRange(musicInfos);
        }

        return musicInfoList;
    }

    /// <summary>
    /// 用于加密请求参数，具体加密算法请参考网易云音乐的 JS 代码。
    /// </summary>
    /// <param name="srcParams"></param>
    /// <param name="secretKey"></param>
    /// <param name="encSecKey"></param>
    /// <returns></returns>
    private Dictionary<string, string> HandleRequest(object srcParams, string secretKey, string encSecKey)
    {
        return new Dictionary<string, string>
        {
            {
                "params", NetEaseMusicEncryptionHelper.AesEncode(
                    NetEaseMusicEncryptionHelper.AesEncode(
                        JsonConvert.SerializeObject(srcParams), NetEaseMusicEncryptionHelper.Nonce), secretKey)
            },
            { "encSecKey", encSecKey }
        };
    }

    /// <summary>
    /// 通过二维码登录网易云音乐，登录成功后返回 Cookie 和 CSRF Token。
    /// </summary>
    /// <param name="onQrCodeReady">
    /// 二维码就绪回调。为空时保持原有行为 —— 把 ASCII 二维码打到日志里，供 CLI 在终端展示。
    /// </param>
    /// <param name="onStateChanged">登录状态变化回调。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    private async Task<(string? csrfToken, CookieContainer? cookieContainer)> LoginViqQrCodeAsync(
        Action<NetEaseQrCodePayload>? onQrCodeReady,
        Action<NetEaseLoginState>? onStateChanged,
        CancellationToken cancellationToken)
    {
        // Get unikey.
        var qrCodeKeyJson = await (await PostAsync($"{Host}/weapi/login/qrcode/unikey", new
        {
            type = 1
        })).Content.ReadAsStringAsync(cancellationToken);
        var uniKey = JObject.Parse(qrCodeKeyJson).SelectToken("$.unikey")!.Value<string>();
        if (string.IsNullOrEmpty(uniKey)) return (null, null);

        // Generate QR code.
        var qrGenerator = new QRCodeGenerator();
        var qrContent = $"{Host}/login?codekey={uniKey}";
        var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.L);
        var asciiQrCodeString = new AsciiQRCode(qrCodeData).GetGraphic(1, drawQuietZones: false);

        if (onQrCodeReady != null)
        {
            onQrCodeReady(new NetEaseQrCodePayload(qrContent, uniKey, asciiQrCodeString));
        }
        else
        {
            _logger.LogInformation("请使用网易云 APP 扫码登录:");
            _logger.LogInformation("\n{AsciiQrCodeString}", asciiQrCodeString);
        }

        // Wait for login success.
        var lastState = NetEaseLoginState.Waiting;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (state, cookieContainer) = await CheckIsLoginAsync(uniKey, cancellationToken);

            if (state != lastState)
            {
                lastState = state;
                onStateChanged?.Invoke(state);
            }

            if (state == NetEaseLoginState.Succeeded)
            {
                return (cookieContainer?.GetCookies(new Uri(Host))["__csrf"]?.Value, cookieContainer);
            }

            if (state == NetEaseLoginState.Expired)
            {
                // 二维码已失效，继续轮询没有意义。
                return (null, null);
            }

            await Task.Delay(2000, cancellationToken);
        }
    }

    private async Task<(NetEaseLoginState state, CookieContainer? cookieContainer)> CheckIsLoginAsync(
        string uniKey,
        CancellationToken cancellationToken)
    {
        var responseMessage = await PostAsync($"{Host}/weapi/login/qrcode/client/login", new
        {
            key = uniKey,
            type = 1
        });

        var responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        var responseCode = JObject.Parse(responseString)["code"]?.Value<int>();

        switch (responseCode)
        {
            // 800 二维码过期、801 等待扫码、802 已扫码待确认、803 登录成功。
            case 800:
                return (NetEaseLoginState.Expired, null);
            case 802:
                return (NetEaseLoginState.ScanConfirmed, null);
            case 803:
                break;
            default:
                return (NetEaseLoginState.Waiting, null);
        }

        if (!responseMessage.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return (NetEaseLoginState.Waiting, null);
        }

        var cookieContainer = new CookieContainer();
        foreach (var cookie in cookies)
        {
            cookieContainer.SetCookies(new Uri(Host), cookie);
        }

        return (NetEaseLoginState.Succeeded, cookieContainer);
    }

    /// <summary>
    /// 封装了网易云音乐的加密请求方式。
    /// </summary>
    /// <param name="url">需要请求的网易云音乐 API 地址。</param>
    /// <param name="params">API 请求参数。</param>
    /// <returns>
    /// 正常情况下会返回一个 <see cref="HttpResponseMessage"/> 对象。
    /// </returns>
    private async Task<HttpResponseMessage> PostAsync(string url, object @params)
    {
        var secretKey = NetEaseMusicEncryptionHelper.CreateSecretKey(16);
        var encSecKey = NetEaseMusicEncryptionHelper.RsaEncode(secretKey);

        return await _warpHttpClient.PostReturnHttpResponseAsync(url, requestOption:
            request =>
            {
                request.Content = new FormUrlEncodedContent(HandleRequest(@params, secretKey, encSecKey));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            });
    }
}
