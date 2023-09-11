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

    private string Cookie { get; set; } = string.Empty;
    private string CsrfToken { get; set; } = string.Empty;

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
    /// 从网易云歌单获取需要下载的歌曲列表，调用这个 API 需要用户登录，否则获取的歌单数据不全。
    /// </summary>
    /// <param name="songListIds">网易云音乐歌单的 ID。</param>
    /// <param name="outputDirectory">歌词文件的输出路径。</param>
    /// <param name="pattern">输出的歌词文件格式，默认是 "{Artist} - {Title}.lrc" 的形式。</param>
    /// <returns>返回获取到的歌曲列表。</returns>
    public async Task<List<MusicInfo>> GetMusicInfoFromNetEaseMusicSongListAsync(string songListIds, string outputDirectory, string pattern)
    {
        if (string.IsNullOrEmpty(Cookie))
        {
            var loginResponse = await LoginViqQrCodeAsync();
            Cookie = loginResponse.cookieContainer?.GetCookieHeader(new Uri(Host)) ?? string.Empty;
            CsrfToken = loginResponse.csrfToken ?? string.Empty;
        }

        async Task<List<MusicInfo>> GetMusicInfoBySongIdAsync(string songId)
        {
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
                throw new ErrorCodeException(ErrorCodes.NotSupportedFileEncoding);
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
        foreach (var songListId in songListIds.Split(';'))
        {
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
    private async Task<(string? csrfToken, CookieContainer? cookieContainer)> LoginViqQrCodeAsync()
    {
        // Get unikey.
        var qrCodeKeyJson = await (await PostAsync($"{Host}/weapi/login/qrcode/unikey", new
        {
            type = 1
        })).Content.ReadAsStringAsync();
        var uniKey = JObject.Parse(qrCodeKeyJson).SelectToken("$.unikey")!.Value<string>();
        if (string.IsNullOrEmpty(uniKey)) return (null, null);

        // Generate QR code.
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode($"{Host}/login?codekey={uniKey}",
            QRCodeGenerator.ECCLevel.L);
        var qrCode = new AsciiQRCode(qrCodeData);
        var asciiQrCodeString = qrCode.GetGraphic(1, drawQuietZones: false);

        _logger.LogInformation("请使用网易云 APP 扫码登录:");
        _logger.LogInformation("\n{AsciiQrCodeString}", asciiQrCodeString);

        // Wait for login success.
        var isLogin = false;
        while (!isLogin)
        {
            var (isSuccess, cookieContainer) = await CheckIsLoginAsync(uniKey);
            isLogin = isSuccess;

            if (!isLogin)
            {
                await Task.Delay(2000);
            }
            else
            {
                return (cookieContainer?.GetCookies(new Uri(Host))["__csrf"]?.Value, cookieContainer);
            }
        }

        return (null, null);
    }

    /// <summary>
    /// 使用 <paramref name="uniKey"/> 检测是否登录成功。
    /// </summary>
    /// <param name="uniKey">由网易云 API 生成的唯一 Key，用于登录。</param>
    /// <returns>
    /// 当登录成功的时候，元组 <c>isSuccess</c> 会为 true，<c>cookieContainer</c> 会包含登录成功后的 Cookie。<br/>
    /// 如果登录失败，<c>isSuccess</c> 会为 false，<c>cookieContainer</c> 会为 null。 
    /// </returns>
    private async Task<(bool isSuccess, CookieContainer? cookieContainer)> CheckIsLoginAsync(string uniKey)
    {
        var responseMessage = await PostAsync($"{Host}/weapi/login/qrcode/client/login", new
        {
            key = uniKey,
            type = 1
        });

        var responseString = await responseMessage.Content.ReadAsStringAsync();
        var responseCode = JObject.Parse(responseString)["code"]?.Value<int>();

        if (responseCode != 803)
        {
            return (false, null);
        }

        if (!responseMessage.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return (false, null);
        }

        var cookieContainer = new CookieContainer();
        foreach (var cookie in cookies)
        {
            cookieContainer.SetCookies(new Uri(Host), cookie);
        }

        return (true, cookieContainer);
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