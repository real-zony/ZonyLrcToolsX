using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Cli.Infrastructure.Lyric.KuGou.JsonModel;
using ZonyLrcTools.Common.Configuration;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.KuGou
{
    public class KuGourLyricDownloader : LyricDownloader
    {
        public override string DownloaderName => InternalLyricDownloaderNames.KuGou;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricItemCollectionFactory _lyricItemCollectionFactory;
        private readonly GlobalOptions _options;

        private const string KuGouSearchMusicUrl = @"https://songsearch.kugou.com/song_search_v2";
        private const string KuGouGetLyricAccessKeyUrl = @"http://lyrics.kugou.com/search";
        private const string KuGouGetLyricUrl = @"http://lyrics.kugou.com/download";

        public KuGourLyricDownloader(IWarpHttpClient warpHttpClient,
            ILyricItemCollectionFactory lyricItemCollectionFactory,
            IOptions<GlobalOptions> options)
        {
            _warpHttpClient = warpHttpClient;
            _lyricItemCollectionFactory = lyricItemCollectionFactory;
            _options = options.Value;
        }

        protected override async ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args)
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

        protected override async ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data, LyricDownloaderArgs args)
        {
            await ValueTask.CompletedTask;
            var lyricJsonObj = JObject.Parse(Encoding.UTF8.GetString(data));
            if (lyricJsonObj.SelectToken("$.status").Value<int>() != 200)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }

            var lyricText = Encoding.UTF8.GetString(Convert.FromBase64String(lyricJsonObj.SelectToken("$.content").Value<string>()));
            return _lyricItemCollectionFactory.Build(lyricText);
        }

        protected virtual void ValidateSongSearchResponse(SongSearchResponse response, LyricDownloaderArgs args)
        {
            if (response.ErrorCode != 0 && response.Status != 1 || response.Data.List == null)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }
        }
    }
}