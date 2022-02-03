using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;
using ZonyLrcTools.Cli.Infrastructure.Lyric.NetEase.JsonModel;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.Album.NetEase
{
    public class NetEaseAlbumDownloader : IAlbumDownloader, ITransientDependency
    {
        public string DownloaderName => InternalAlbumDownloaderNames.NetEase;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly Action<HttpRequestMessage> _defaultOption;

        private const string SearchMusicApi = @"https://music.163.com/api/search/get/web";
        private const string GetMusicInfoApi = @"https://music.163.com/api/song/detail";
        private const string DefaultReferer = @"https://music.163.com";

        public NetEaseAlbumDownloader(IWarpHttpClient warpHttpClient)
        {
            _warpHttpClient = warpHttpClient;
            _defaultOption = message =>
            {
                message.Headers.Referrer = new Uri(DefaultReferer);

                if (message.Content != null)
                {
                    message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                }
            };
        }

        public async ValueTask<byte[]> DownloadAsync(string songName, string artist)
        {
            var requestParameter = new SongSearchRequest(songName, artist);
            var searchResult = await _warpHttpClient.PostAsync<SongSearchResponse>(
                SearchMusicApi,
                requestParameter,
                true,
                _defaultOption);

            if (searchResult is not { StatusCode: 200 } || searchResult.Items?.SongCount <= 0)
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong);
            }

            var songDetailJsonStr = await _warpHttpClient.GetAsync(
                GetMusicInfoApi,
                new GetSongDetailsRequest(searchResult.GetFirstMatchSongId(songName)),
                _defaultOption);

            var url = JObject.Parse(songDetailJsonStr).SelectToken("$.songs[0].album.picUrl")?.Value<string>();
            if (string.IsNullOrEmpty(url))
            {
                throw new ErrorCodeException(ErrorCodes.TheReturnValueIsIllegal);
            }

            return await new HttpClient().GetByteArrayAsync(new Uri(url));
        }
    }
}