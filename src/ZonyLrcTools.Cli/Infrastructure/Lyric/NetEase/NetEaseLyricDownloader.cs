using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.Lyric.NetEase.JsonModel;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.NetEase
{
    public class NetEaseLyricDownloader : LyricDownloader
    {
        public override string DownloaderName => InternalLyricDownloaderNames.NetEase;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricItemCollectionFactory _lyricItemCollectionFactory;

        private const string NetEaseSearchMusicUrl = @"https://music.163.com/api/search/get/web";
        private const string NetEaseGetLyricUrl = @"https://music.163.com/api/song/lyric";

        private const string NetEaseRequestReferer = @"https://music.163.com";
        private const string NetEaseRequestContentType = @"application/x-www-form-urlencoded";

        public NetEaseLyricDownloader(IWarpHttpClient warpHttpClient,
            ILyricItemCollectionFactory lyricItemCollectionFactory)
        {
            _warpHttpClient = warpHttpClient;
            _lyricItemCollectionFactory = lyricItemCollectionFactory;
        }

        protected override async ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args)
        {
            var searchResult = await _warpHttpClient.PostAsync<SongSearchResponse>(
                NetEaseSearchMusicUrl,
                new SongSearchRequest(args.SongName, args.Artist),
                true,
                msg =>
                {
                    msg.Headers.Referrer = new Uri(NetEaseRequestReferer);
                    if (msg.Content != null)
                    {
                        msg.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(NetEaseRequestContentType);
                    }
                });

            ValidateSongSearchResponse(searchResult, args);

            var lyricResponse = await _warpHttpClient.GetAsync(
                NetEaseGetLyricUrl,
                new GetLyricRequest(searchResult.GetFirstMatchSongId(args.SongName)),
                msg => msg.Headers.Referrer = new Uri(NetEaseRequestReferer));

            return Encoding.UTF8.GetBytes(lyricResponse);
        }

        protected override async ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data, LyricDownloaderArgs args)
        {
            await ValueTask.CompletedTask;

            var json = JsonConvert.DeserializeObject<GetLyricResponse>(Encoding.UTF8.GetString(data));
            if (json?.OriginalLyric == null)
            {
                return new LyricItemCollection(LyricItemCollectionOption.NullInstance);
            }

            return _lyricItemCollectionFactory.Build(
                json.OriginalLyric.Text,
                json.TranslationLyric.Text);
        }

        protected virtual void ValidateSongSearchResponse(SongSearchResponse response, LyricDownloaderArgs args)
        {
            if (response?.StatusCode != SongSearchResponseStatusCode.Success)
            {
                throw new ErrorCodeException(ErrorCodes.TheReturnValueIsIllegal, attachObj: args);
            }

            if (response.Items?.SongCount <= 0)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong, attachObj: args);
            }
        }
    }
}