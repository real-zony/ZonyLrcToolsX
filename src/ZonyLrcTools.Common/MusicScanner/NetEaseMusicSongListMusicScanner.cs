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

public class NetEaseMusicSongListMusicScanner : ISingletonDependency
{
    private readonly IWarpHttpClient _warpHttpClient;
    private readonly ILogger<NetEaseMusicSongListMusicScanner> _logger;
    private const string Host = "https://music.163.com";

    private string Cookie { get; set; } = string.Empty;
    private string CsrfToken { get; set; } = string.Empty;

    public NetEaseMusicSongListMusicScanner(IWarpHttpClient warpHttpClient,
        ILogger<NetEaseMusicSongListMusicScanner> logger)
    {
        _warpHttpClient = warpHttpClient;
        _logger = logger;
    }

    public async Task<List<MusicInfo>> GetMusicInfoFromNetEaseMusicSongListAsync(string songListId, string outputDirectory, string pattern)
    {
        if (string.IsNullOrEmpty(Cookie))
        {
            var loginResponse = await LoginViqQrCodeAsync();
            Cookie = loginResponse.cookieContainer?.GetCookieHeader(new Uri(Host)) ?? string.Empty;
            CsrfToken = loginResponse.csrfToken ?? string.Empty;
        }

        var secretKey = NetEaseMusicEncryptionHelper.CreateSecretKey(16);
        var encSecKey = NetEaseMusicEncryptionHelper.RsaEncode(secretKey);
        var response = await _warpHttpClient.PostAsync<GetMusicInfoFromNetEaseMusicSongListResponse>(
            $"{Host}/weapi/v6/playlist/detail?csrf_token=e5044820d8b66e14b8c31d39f9651a98", requestOption:
            request =>
            {
                request.Headers.Add("Cookie", Cookie);
                request.Content = new FormUrlEncodedContent(HandleRequest(new
                {
                    csrf_token = CsrfToken,
                    id = songListId,
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

                var info = new MusicInfo(fakeFilePath, song.Name!, artistName);
                return info;
            }).ToList();
    }

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
        _logger.LogInformation(asciiQrCodeString);

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