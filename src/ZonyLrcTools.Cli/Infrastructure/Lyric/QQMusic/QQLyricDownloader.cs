using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic.JsonModel;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic
{
    public class QQLyricDownloader : LyricDownloader
    {
        public override string DownloaderName => InternalLyricDownloaderNames.QQ;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricItemCollectionFactory _lyricItemCollectionFactory;

        // private const string QQSearchMusicUrl = @"https://c.y.qq.com/soso/fcgi-bin/client_search_cp";
        private const string QQSearchMusicUrl = @"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg";
        private const string QQGetLyricUrl = @"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg";

        private const string QQMusicRequestReferer = @"https://y.qq.com/";

        public QQLyricDownloader(IWarpHttpClient warpHttpClient,
            ILyricItemCollectionFactory lyricItemCollectionFactory)
        {
            _warpHttpClient = warpHttpClient;
            _lyricItemCollectionFactory = lyricItemCollectionFactory;
        }

        protected override async ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args)
        {
            var searchResult = await _warpHttpClient.GetAsync<SongSearchResponse>(
                QQSearchMusicUrl,
                new SongSearchRequest(args.SongName, args.Artist));

            ValidateSongSearchResponse(searchResult, args);

            var lyricJsonString = await _warpHttpClient.GetAsync(QQGetLyricUrl,
                new GetLyricRequest(searchResult.Data.Song.SongItems.FirstOrDefault()?.SongId),
                op => op.Headers.Referrer = new Uri(QQMusicRequestReferer));

            return Encoding.UTF8.GetBytes(lyricJsonString);
        }

        protected override async ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data, LyricDownloaderArgs args)
        {
            await ValueTask.CompletedTask;

            var lyricJsonString = Encoding.UTF8.GetString(data);
            lyricJsonString = lyricJsonString.Replace(@"MusicJsonCallback(", string.Empty).TrimEnd(')');

            if (lyricJsonString.Contains("\"code\":-1901"))
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }

            if (lyricJsonString.Contains("此歌曲为没有填词的纯音乐，请您欣赏"))
            {
                return _lyricItemCollectionFactory.Build(null);
            }

            var lyricJsonObj = JObject.Parse(lyricJsonString);
            var sourceLyric = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(lyricJsonObj.SelectToken("$.lyric").Value<string>()));
            var translateLyric = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(lyricJsonObj.SelectToken("$.trans").Value<string>()));

            return _lyricItemCollectionFactory.Build(sourceLyric, translateLyric);
        }

        protected virtual void ValidateSongSearchResponse(SongSearchResponse response, LyricDownloaderArgs args)
        {
            if (response is not { StatusCode: 0 } || response.Data.Song.SongItems == null)
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