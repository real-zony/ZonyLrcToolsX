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

    private const string KuWoSearchMusicUrl = @"https://search.kuwo.cn/r.s";
    private const string KuWoSearchLyricsUrl = @"https://m.kuwo.cn/newh5/singles/songinfoandlrc";
    private const string KuWoDefaultToken = "ABCDE12345";

    private readonly IWarpHttpClient _warpHttpClient;
    private readonly GlobalOptions _options;

    private static readonly ProductInfoHeaderValue UserAgent = new("Chrome", "81.0.4044.138");

    public KuWoLyricsProvider(IWarpHttpClient warpHttpClient,
        IOptions<GlobalOptions> options)
    {
        _warpHttpClient = warpHttpClient;
        _options = options.Value;
    }

    protected override async ValueTask<object> DownloadDataAsync(LyricsProviderArgs args)
    {
        var songSearchResponse = await _warpHttpClient.GetAsync<SongSearchResponse>(KuWoSearchMusicUrl,
            new SongSearchRequest(args.SongName, args.Artist,
                pageSize: _options.Provider.Lyric.GetLyricProviderOption(DownloaderName).Depth),
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

    protected override async ValueTask<LyricsItemCollection> GenerateLyricAsync(object lyricsObject,
        LyricsProviderArgs args)
    {
        await ValueTask.CompletedTask;

        var lyricsResponse = (GetLyricsResponse)lyricsObject;
        if (lyricsResponse.Data?.Lyrics == null)
        {
            return new LyricsItemCollection(null);
        }

        var items = lyricsResponse.Data.Lyrics.Select(l =>
        {
            var position = double.Parse(l.Position);
            var positionSpan = TimeSpan.FromSeconds(position);
            return new LyricsItem(positionSpan.Minutes,
                double.Parse($"{positionSpan.Seconds}.{positionSpan.Milliseconds}"), l.Text);
        });

        var lyricsItemCollection = new LyricsItemCollection(_options.Provider.Lyric.Config);
        lyricsItemCollection.AddRange(items);
        return lyricsItemCollection;
    }

    protected virtual void ValidateSongSearchResponse(SongSearchResponse response, LyricsProviderArgs args)
    {
        if (response.TotalCount == 0)
        {
            throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
        }
    }
}