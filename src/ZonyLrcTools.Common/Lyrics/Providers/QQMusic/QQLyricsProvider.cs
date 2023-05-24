using System.Web;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.Lyrics.Providers.QQMusic.JsonModel;

namespace ZonyLrcTools.Common.Lyrics.Providers.QQMusic
{
    public class QQLyricsProvider : LyricsProvider
    {
        public override string DownloaderName => InternalLyricsProviderNames.QQ;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricsItemCollectionFactory _lyricsItemCollectionFactory;

        // private const string QQSearchMusicUrl = @"https://c.y.qq.com/soso/fcgi-bin/client_search_cp";
        private const string QQSearchMusicUrl = @"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg";
        private const string QQGetLyricUrl = @"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg";

        private const string QQMusicRequestReferer = @"https://y.qq.com/";

        public QQLyricsProvider(IWarpHttpClient warpHttpClient,
            ILyricsItemCollectionFactory lyricsItemCollectionFactory)
        {
            _warpHttpClient = warpHttpClient;
            _lyricsItemCollectionFactory = lyricsItemCollectionFactory;
        }

        protected override async ValueTask<object> DownloadDataAsync(LyricsProviderArgs args)
        {
            var searchResult = await _warpHttpClient.GetAsync<SongSearchResponse>(
                QQSearchMusicUrl,
                new SongSearchRequest(args.SongName, args.Artist));

            ValidateSongSearchResponse(searchResult, args);

            return await _warpHttpClient.GetAsync(QQGetLyricUrl,
                new GetLyricRequest(searchResult.Data?.Song?.SongItems?.FirstOrDefault()?.SongId),
                op => op.Headers.Referrer = new Uri(QQMusicRequestReferer));
        }

        protected override async ValueTask<LyricsItemCollection> GenerateLyricAsync(object lyricsObject, LyricsProviderArgs args)
        {
            await ValueTask.CompletedTask;

            var lyricJsonString = (lyricsObject as string)!;
            lyricJsonString = lyricJsonString.Replace(@"MusicJsonCallback(", string.Empty).TrimEnd(')');

            if (lyricJsonString.Contains("\"code\":-1901"))
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }

            if (lyricJsonString.Contains("此歌曲为没有填词的纯音乐，请您欣赏"))
            {
                return _lyricsItemCollectionFactory.Build(null);
            }

            var lyricJsonObj = JObject.Parse(lyricJsonString);
            var sourceLyric = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(lyricJsonObj.SelectToken("$.lyric")!.Value<string>()));
            var translateLyric = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(lyricJsonObj.SelectToken("$.trans")!.Value<string>()));

            return _lyricsItemCollectionFactory.Build(sourceLyric, translateLyric);
        }

        protected virtual void ValidateSongSearchResponse(SongSearchResponse response, LyricsProviderArgs args)
        {
            if (response is not { StatusCode: 0 } || response.Data?.Song?.SongItems == null)
            {
                throw new ErrorCodeException(ErrorCodes.TheReturnValueIsIllegal, attachObj: args);
            }

            if (response.Data.Song.SongItems.Count <= 0)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }
        }
    }
}