using System.Threading.Tasks;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    /// <summary>
    /// 歌词下载器的基类，定义了歌词下载器的常规逻辑。
    /// </summary>
    public abstract class LyricDownloader : ILyricDownloader, ITransientDependency
    {
        public abstract string DownloaderName { get; }

        public virtual async ValueTask<LyricItemCollection> DownloadAsync(string songName, string artist)
        {
            var args = new LyricDownloaderArgs(songName, artist);
            await ValidateAsync(args);
            var downloadDataBytes = await DownloadDataAsync(args);
            return await GenerateLyricAsync(downloadDataBytes);
        }

        protected virtual ValueTask ValidateAsync(LyricDownloaderArgs args)
        {
            if (string.IsNullOrEmpty(args.SongName))
            {
                throw new ErrorCodeException(ErrorCodes.SongNameIsNull, attachObj: args);
            }

            if (string.IsNullOrEmpty(args.SongName) && string.IsNullOrEmpty(args.Artist))
            {
                throw new ErrorCodeException(ErrorCodes.SongNameAndArtistIsNull, attachObj: args);
            }

            return ValueTask.CompletedTask;
        }

        protected abstract ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args);

        protected abstract ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data);
    }
}