using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.Lyrics.Providers.KuWo.JsonModel;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuWo;

public class KuWoLyricsProvider : LyricsProvider
{
    public override string DownloaderName => InternalLyricsProviderNames.KuWo;

    private const string KuWoSearchMusicUrl = @"https://www.kuwo.cn/api/www/search/searchMusicBykeyWord";
    private const string KuWoSearchLyricsUrl = @"https://m.kuwo.cn/newh5/singles/songinfoandlrc";
    private const string KuWoDefaultToken = "ABCDE12345";

    private readonly IWarpHttpClient _warpHttpClient;
    private readonly ILyricsItemCollectionFactory _lyricsItemCollectionFactory;
    private readonly GlobalOptions _options;

    private static readonly ProductInfoHeaderValue UserAgent = new("Chrome", "81.0.4044.138");

    public KuWoLyricsProvider(IWarpHttpClient warpHttpClient,
        ILyricsItemCollectionFactory lyricsItemCollectionFactory,
        IOptions<GlobalOptions> options)
    {
        _warpHttpClient = warpHttpClient;
        _lyricsItemCollectionFactory = lyricsItemCollectionFactory;
        _options = options.Value;
    }

    protected override async ValueTask<object> DownloadDataAsync(LyricsProviderArgs args)
    {
        var songSearchResponse = await _warpHttpClient.GetAsync<SongSearchResponse>(KuWoSearchMusicUrl,
            new SongSearchRequest(args.SongName, args.Artist, pageSize: _options.Provider.Lyric.GetLyricProviderOption(DownloaderName).Depth),
            op =>
            {
                op.Headers.UserAgent.Add(UserAgent);
                op.Headers.Referrer = new Uri("https://kuwo.cn");
                op.Headers.Add("csrf", KuWoDefaultToken);
                op.Headers.Add("Cookie", $"kw_token={KuWoDefaultToken}");
            });

        ValidateSongSearchResponse(songSearchResponse, args);

        return await _warpHttpClient.GetAsync<GetLyricsResponse>(KuWoSearchLyricsUrl,
            new GetLyricsRequest(songSearchResponse.GetMatchedMusicId(args.SongName, args.Artist, args.Duration)),
            op =>
            {
                op.Headers.UserAgent.Add(UserAgent);
                op.Headers.Referrer = new Uri("https://m.kuwo.cn/yinyue/");
            });
    }

    protected override ValueTask<LyricsItemCollection> GenerateLyricAsync(object lyricsObject, LyricsProviderArgs args)
    {
        throw new NotImplementedException();
    }

    protected void ValidateSongSearchResponse(SongSearchResponse response, LyricsProviderArgs args)
    {
        if (response.Code != 200)
        {
            throw new ErrorCodeException(ErrorCodes.TheReturnValueIsIllegal, response.ErrorMessage, args);
        }
    }
}