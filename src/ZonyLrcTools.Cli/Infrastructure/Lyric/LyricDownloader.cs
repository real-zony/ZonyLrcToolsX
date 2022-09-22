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

        /// <summary>
        /// 歌词数据下载的核心逻辑。
        /// </summary>
        /// <param name="songName">歌曲名称。</param>
        /// <param name="artist">歌曲作者/艺术家。</param>
        /// <param name="duration">歌曲的时长。</param>
        /// <returns>下载完成的歌曲数据。</returns>
        public virtual async ValueTask<LyricItemCollection> DownloadAsync(string songName, string artist, long? duration = null)
        {
            var args = new LyricDownloaderArgs(songName, artist, duration ?? 0);
            await ValidateAsync(args);
            var downloadDataBytes = await DownloadDataAsync(args);
            return await GenerateLyricAsync(downloadDataBytes, args);
        }

        /// <summary>
        /// 通用的验证逻辑，验证基本参数是否正确。
        /// </summary>
        /// <param name="args">歌词下载时需要的参数信息。</param>
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

        /// <summary>
        /// 根据指定的歌曲参数，下载歌词数据。
        /// </summary>
        protected abstract ValueTask<byte[]> DownloadDataAsync(LyricDownloaderArgs args);

        /// <summary>
        /// 根据指定的歌词二进制数据，生成歌词数据。
        /// </summary>
        /// <param name="data">歌词的原始二进制数据。</param>
        protected abstract ValueTask<LyricItemCollection> GenerateLyricAsync(byte[] data, LyricDownloaderArgs args);
    }
}