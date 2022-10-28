using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;

namespace ZonyLrcTools.Common.Lyrics
{
    /// <summary>
    /// 歌词下载器的基类，定义了歌词下载器的常规逻辑。
    /// </summary>
    public abstract class LyricsProvider : ILyricsProvider, ITransientDependency
    {
        public abstract string DownloaderName { get; }

        /// <summary>
        /// 歌词数据下载的核心逻辑。
        /// </summary>
        /// <param name="songName">歌曲名称。</param>
        /// <param name="artist">歌曲作者/艺术家。</param>
        /// <param name="duration">歌曲的时长。</param>
        /// <returns>下载完成的歌曲数据。</returns>
        public virtual async ValueTask<LyricsItemCollection> DownloadAsync(string songName, string artist, long? duration = null)
        {
            var args = new LyricsProviderArgs(songName, artist, duration ?? 0);
            await ValidateAsync(args);
            var downloadDataObject = await DownloadDataAsync(args);
            return await GenerateLyricAsync(downloadDataObject, args);
        }

        /// <summary>
        /// 通用的验证逻辑，验证基本参数是否正确。
        /// </summary>
        /// <param name="args">歌词下载时需要的参数信息。</param>
        protected virtual ValueTask ValidateAsync(LyricsProviderArgs args)
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
        protected abstract ValueTask<object> DownloadDataAsync(LyricsProviderArgs args);

        /// <summary>
        /// 根据指定的歌词对象，生成歌词数据，常用于处理不同格式的歌词数据。
        /// </summary>
        /// <param name="lyricsObject">当 <see cref="DownloadDataAsync"/> 完成后，传递的歌词数据对象。</param>
        /// <param name="args">生成歌词时，提供的歌曲信息参数。</param>
        protected abstract ValueTask<LyricsItemCollection> GenerateLyricAsync(object lyricsObject, LyricsProviderArgs args);
    }
}