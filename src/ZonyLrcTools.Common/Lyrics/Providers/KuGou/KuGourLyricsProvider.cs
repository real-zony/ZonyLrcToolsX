using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.Lyrics.Providers.KuGou.JsonModel;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuGou
{
    public class KuGourLyricsProvider : LyricsProvider
    {
        public override string DownloaderName => InternalLyricsProviderNames.KuGou;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricsItemCollectionFactory _lyricsItemCollectionFactory;
        private readonly GlobalOptions _options;

        private const string KuGouSearchMusicUrl = @"https://songsearch.kugou.com/song_search_v2";
        private const string KuGouGetLyricAccessKeyUrl = @"http://lyrics.kugou.com/search";
        private const string KuGouGetLyricUrl = @"http://lyrics.kugou.com/download";

        public KuGourLyricsProvider(IWarpHttpClient warpHttpClient,
            ILyricsItemCollectionFactory lyricsItemCollectionFactory,
            IOptions<GlobalOptions> options)
        {
            _warpHttpClient = warpHttpClient;
            _lyricsItemCollectionFactory = lyricsItemCollectionFactory;
            _options = options.Value;
        }

        protected override async ValueTask<byte[]> DownloadDataAsync(LyricsProviderArgs args)
        {
            var searchResult = await _warpHttpClient.GetAsync<SongSearchResponse>(KuGouSearchMusicUrl,
                new SongSearchRequest(args.SongName, args.Artist, _options.Provider.Lyric.GetLyricProviderOption(DownloaderName).Depth));

            ValidateSongSearchResponse(searchResult, args);

            // 获得特殊的 AccessToken 与 Id，真正请求歌词数据。
            var accessKeyResponse = await _warpHttpClient.GetAsync<GetLyricAccessKeyResponse>(KuGouGetLyricAccessKeyUrl,
                new GetLyricAccessKeyRequest(searchResult.Data.List[0].FileHash));

            var accessKeyObject = accessKeyResponse.AccessKeyDataObjects[0];
            var lyricResponse = await _warpHttpClient.GetAsync(KuGouGetLyricUrl,
                new GetLyricRequest(accessKeyObject.Id, accessKeyObject.AccessKey));

            return Encoding.UTF8.GetBytes(lyricResponse);
        }

        protected override async ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data, LyricsProviderArgs args)
        {
            await ValueTask.CompletedTask;
            var lyricJsonObj = JObject.Parse(Encoding.UTF8.GetString(data));
            if (lyricJsonObj.SelectToken("$.status").Value<int>() != 200)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }

            var lyricText = Encoding.UTF8.GetString(Convert.FromBase64String(lyricJsonObj.SelectToken("$.content").Value<string>()));
            return _lyricsItemCollectionFactory.Build(lyricText);
        }

        protected virtual void ValidateSongSearchResponse(SongSearchResponse response, LyricsProviderArgs args)
        {
            if (response.ErrorCode != 0 && response.Status != 1 || response.Data.List == null)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }
        }
    }
}