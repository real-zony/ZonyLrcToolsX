using System.Threading.Tasks;
using ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic.JsonModel;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic
{
    public class QQLyricDownloader : LyricDownloader
    {
        public override string DownloaderName => InternalLyricDownloaderNames.QQ;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricItemCollectionFactory _lyricItemCollectionFactory;

        private const string QQSearchMusicUrl = @"https://c.y.qq.com/soso/fcgi-bin/client_search_cp";

        public QQLyricDownloader(IWarpHttpClient warpHttpClient,
            ILyricItemCollectionFactory lyricItemCollectionFactory)
        {
            _warpHttpClient = warpHttpClient;
            _lyricItemCollectionFactory = lyricItemCollectionFactory;
        }

        protected override async ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args)
        {
            var searchResult = await _warpHttpClient.PostAsync<SongSearchResponse>(
                QQSearchMusicUrl,
                new SongSearchRequest(args.SongName, args.Artist));
            throw new System.NotImplementedException();
        }

        protected override ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}