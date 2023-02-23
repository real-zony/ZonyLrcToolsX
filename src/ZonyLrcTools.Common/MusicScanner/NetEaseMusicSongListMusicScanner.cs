using System.Net.Http.Headers;
using Newtonsoft.Json;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Encryption;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.MusicScanner.JsonModel;

namespace ZonyLrcTools.Common.MusicScanner;

public class NetEaseMusicSongListMusicScanner : ITransientDependency
{
    private readonly IWarpHttpClient _warpHttpClient;

    public NetEaseMusicSongListMusicScanner(IWarpHttpClient warpHttpClient)
    {
        _warpHttpClient = warpHttpClient;
    }

    public async Task<List<MusicInfo>> GetMusicInfoFromNetEaseMusicSongListAsync(string songListId, ManualDownloadOptions options)
    {
        var secretKey = NetEaseMusicEncryptionHelper.CreateSecretKey(16);
        var encSecKey = NetEaseMusicEncryptionHelper.RsaEncode(secretKey);

        var response = await _warpHttpClient.PostAsync<GetMusicInfoFromNetEaseMusicSongListResponse>("https://music.163.com/weapi/v6/playlist/detail?csrf_token=", requestOption:
            request =>
            {
                request.Content = new FormUrlEncodedContent(HandleRequest(new
                {
                    csrf_token = "",
                    id = songListId,
                    n = 100000,
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
                var fakeFilePath = Path.Combine(options.OutputDirectory, options.OutputFileNamePattern.Replace("{Name}", song.Name).Replace("{Artist}", artistName));

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
}