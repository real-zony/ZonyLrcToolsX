using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.Lyrics.Providers.NetEase.JsonModel;

namespace ZonyLrcTools.Common.Album.NetEase
{
    public class NetEaseAlbumProvider : IAlbumProvider, ITransientDependency
    {
        public string DownloaderName => InternalAlbumProviderNames.NetEase;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly Action<HttpRequestMessage> _defaultOption;

        private const string SearchMusicApi = @"https://music.163.com/api/search/get/web";
        private const string GetMusicInfoApi = @"https://music.163.com/api/song/detail";
        private const string DefaultReferer = @"https://music.163.com";

        public NetEaseAlbumProvider(IWarpHttpClient warpHttpClient)
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

            if (searchResult is not { StatusCode: 200 } || searchResult.Items is not { SongCount: > 0 })
            {
                throw new ErrorCodeException(ErrorCodes.NoMatchingSong);
            }

            var songDetailJsonStr = await _warpHttpClient.GetAsync(
                GetMusicInfoApi,
                new GetSongDetailsRequest(searchResult.GetFirstMatchSongId(songName, null)),
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