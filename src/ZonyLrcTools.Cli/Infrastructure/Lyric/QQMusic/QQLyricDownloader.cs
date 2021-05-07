using System.Threading.Tasks;
using ZonyLrcTools.Cli.Infrastructure.Network;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic
{
    public class QQLyricDownloader : LyricDownloader
    {
        public override string DownloaderName => InternalLyricDownloaderNames.QQ;

        private readonly IWarpHttpClient _warpHttpClient;
        private readonly ILyricItemCollectionFactory _lyricItemCollectionFactory;

        public QQLyricDownloader(IWarpHttpClient warpHttpClient,
            ILyricItemCollectionFactory lyricItemCollectionFactory)
        {
            _warpHttpClient = warpHttpClient;
            _lyricItemCollectionFactory = lyricItemCollectionFactory;
        }

        protected override ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args)
        {
            throw new System.NotImplementedException();
        }

        protected override ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}