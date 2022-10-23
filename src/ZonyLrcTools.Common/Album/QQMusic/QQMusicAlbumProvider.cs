using System.Net.Http.Headers;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Network;
using ZonyLrcTools.Common.Lyrics.Providers.QQMusic.JsonModel;

namespace ZonyLrcTools.Common.Album.QQMusic
{
    public class QQMusicAlbumProvider : IAlbumProvider, ITransientDependency
    {
        public string DownloaderName => InternalAlbumProviderNames.QQ;

        private readonly IWarpHttpClient _warpHttpClient;

        private readonly Action<HttpRequestMessage> _defaultOption = message =>
        {
            message.Headers.Referrer = new Uri(DefaultReferer);

            if (message.Content != null)
            {
                message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            }
        };

        private const string SearchApi = "https://c.y.qq.com/soso/fcgi-bin/client_search_cp";
        private const string DefaultReferer = "https://y.qq.com";

        public QQMusicAlbumProvider(IWarpHttpClient warpHttpClient)
        {
            _warpHttpClient = warpHttpClient;
        }

        public async ValueTask<byte[]> DownloadAsync(string songName, string artist)
        {
            var requestParameter = new SongSearchRequest(songName, artist);
            var searchResult = await _warpHttpClient.GetAsync<SongSearchResponse>(
                SearchApi,
                requestParameter, _defaultOption);

            return new byte[] { 0x1, 0x2 };
        }
    }
}